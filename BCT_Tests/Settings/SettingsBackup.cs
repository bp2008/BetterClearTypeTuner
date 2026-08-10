using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace BCT_Tests.Settings
{
	/// <summary>
	/// A snapshot of everything the harness is going to change, complete enough to put the machine
	/// back exactly as it was — including the case where a key or a value did not exist at all
	/// before the run, which then has to be deleted rather than reset to some assumed default.
	///
	/// The snapshot is also written to disk before anything is touched, so that a run that is
	/// killed part way through can still be undone with "BCT_Tests.exe --restore &lt;file&gt;".
	/// </summary>
	internal class SettingsBackup
	{
		private class ValueEntry
		{
			public string Name;
			public RegistryValueKind Kind;
			public string Data;
		}

		private class KeyEntry
		{
			public Hive Hive;
			public string Path;
			public bool Existed;
			public List<ValueEntry> Values = new List<ValueEntry>();
		}

		private readonly List<KeyEntry> keys = new List<KeyEntry>();

		public bool AntialiasingEnabled;
		public SmoothingType SmoothingType;
		public SmoothingOrientation Orientation;
		public uint Contrast;

		/// <summary>Value names found that this harness cannot faithfully restore. Empty in practice.</summary>
		public readonly List<string> UnsupportedValues = new List<string>();

		public static SettingsBackup Capture()
		{
			SettingsBackup backup = new SettingsBackup();

			backup.AntialiasingEnabled = SystemFontSmoothing.GetAntialiasingEnabled();
			backup.SmoothingType = SystemFontSmoothing.GetSmoothingType();
			backup.Orientation = SystemFontSmoothing.GetOrientation();
			backup.Contrast = SystemFontSmoothing.GetContrast();

			foreach (Hive hive in new Hive[] { Hive.HKCU, Hive.HKLM })
			{
				using (RegistryKey root = AvalonRegistry.OpenBase(hive))
				{
					foreach (string display in AvalonRegistry.DisplayNames)
						backup.keys.Add(CaptureKey(backup, root, hive, AvalonRegistry.KeyPath(display)));
					// Captured last, so that if Avalon.Graphics itself is not present on this
					// machine the whole tree the harness created gets removed again on restore.
					backup.keys.Add(CaptureKey(backup, root, hive, AvalonRegistry.BasePath));
				}
			}
			return backup;
		}

		private static KeyEntry CaptureKey(SettingsBackup backup, RegistryKey root, Hive hive, string path)
		{
			KeyEntry entry = new KeyEntry { Hive = hive, Path = path };
			using (RegistryKey key = root.OpenSubKey(path, false))
			{
				entry.Existed = key != null;
				if (key == null)
					return entry;
				foreach (string valueName in key.GetValueNames())
				{
					RegistryValueKind kind = key.GetValueKind(valueName);
					object value = key.GetValue(valueName);
					if (kind != RegistryValueKind.DWord && kind != RegistryValueKind.String)
					{
						backup.UnsupportedValues.Add(hive + "\\" + path + "\\" + valueName + " (" + kind + ")");
						continue;
					}
					entry.Values.Add(new ValueEntry
					{
						Name = valueName,
						Kind = kind,
						Data = Convert.ToString(value, CultureInfo.InvariantCulture)
					});
				}
			}
			return entry;
		}

		/// <summary>
		/// Puts every captured key, value and system setting back the way it was.  Values that
		/// appeared during the run are deleted, and keys that did not exist beforehand are removed
		/// entirely.  Returns the problems encountered, if any; an empty list means a clean restore.
		/// </summary>
		public List<string> Restore()
		{
			List<string> problems = new List<string>();

			foreach (KeyEntry entry in keys)
			{
				try
				{
					using (RegistryKey root = AvalonRegistry.OpenBase(entry.Hive))
					{
						// A key the run never managed to change needs no write, and attempting one
						// would fail on a hive this process cannot write to and report a "problem"
						// that is not one.
						if (Matches(root, entry))
							continue;

						if (!entry.Existed)
						{
							root.DeleteSubKeyTree(entry.Path, false);
							continue;
						}

						using (RegistryKey key = root.CreateSubKey(entry.Path))
						{
							if (key == null)
							{
								problems.Add("could not open " + entry.Hive + "\\" + entry.Path);
								continue;
							}

							HashSet<string> wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
							foreach (ValueEntry value in entry.Values)
								wanted.Add(value.Name);
							foreach (string present in key.GetValueNames())
							{
								if (!wanted.Contains(present))
									key.DeleteValue(present, false);
							}

							foreach (ValueEntry value in entry.Values)
							{
								if (value.Kind == RegistryValueKind.DWord)
									key.SetValue(value.Name, int.Parse(value.Data, CultureInfo.InvariantCulture), RegistryValueKind.DWord);
								else
									key.SetValue(value.Name, value.Data, RegistryValueKind.String);
							}
						}
					}
				}
				catch (Exception ex)
				{
					problems.Add(entry.Hive + "\\" + entry.Path + ": " + ex.Message);
				}
			}

			try
			{
				// Order matters a little: put the mode back before re-enabling smoothing, so the
				// desktop never briefly repaints in a state the user did not have.
				SystemFontSmoothing.SetSmoothingType(SmoothingType);
				SystemFontSmoothing.SetOrientation(Orientation);
				SystemFontSmoothing.SetContrast(Contrast);
				SystemFontSmoothing.SetAntialiasingEnabled(AntialiasingEnabled);
			}
			catch (Exception ex)
			{
				problems.Add("system font smoothing settings: " + ex.Message);
			}

			return problems;
		}

		/// <summary>
		/// True when the key is already exactly as it was captured, values and all.
		/// </summary>
		private static bool Matches(RegistryKey root, KeyEntry entry)
		{
			using (RegistryKey key = root.OpenSubKey(entry.Path, false))
			{
				if (key == null)
					return !entry.Existed;
				if (!entry.Existed)
					return false;

				string[] present = key.GetValueNames();
				if (present.Length != entry.Values.Count)
					return false;
				foreach (ValueEntry value in entry.Values)
				{
					object actual = key.GetValue(value.Name);
					if (actual == null || Convert.ToString(actual, CultureInfo.InvariantCulture) != value.Data)
						return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Verifies that the live state now matches this snapshot.  Returns the differences found.
		/// </summary>
		public List<string> Verify()
		{
			List<string> differences = new List<string>();

			if (SystemFontSmoothing.GetAntialiasingEnabled() != AntialiasingEnabled)
				differences.Add("SPI FontSmoothing is " + SystemFontSmoothing.GetAntialiasingEnabled() + ", expected " + AntialiasingEnabled);
			if (SystemFontSmoothing.GetSmoothingType() != SmoothingType)
				differences.Add("SPI FontSmoothingType is " + SystemFontSmoothing.GetSmoothingType() + ", expected " + SmoothingType);
			if (SystemFontSmoothing.GetOrientation() != Orientation)
				differences.Add("SPI FontSmoothingOrientation is " + SystemFontSmoothing.GetOrientation() + ", expected " + Orientation);
			if (SystemFontSmoothing.GetContrast() != Contrast)
				differences.Add("SPI FontSmoothingContrast is " + SystemFontSmoothing.GetContrast() + ", expected " + Contrast);

			foreach (KeyEntry entry in keys)
			{
				using (RegistryKey root = AvalonRegistry.OpenBase(entry.Hive))
				using (RegistryKey key = root.OpenSubKey(entry.Path, false))
				{
					if (key == null)
					{
						if (entry.Existed)
							differences.Add(entry.Hive + "\\" + entry.Path + " is missing but existed before the run");
						continue;
					}
					if (!entry.Existed)
					{
						differences.Add(entry.Hive + "\\" + entry.Path + " exists but did not before the run");
						continue;
					}

					HashSet<string> expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (ValueEntry value in entry.Values)
					{
						expected.Add(value.Name);
						object actual = key.GetValue(value.Name);
						if (actual == null)
							differences.Add(entry.Hive + "\\" + entry.Path + "\\" + value.Name + " is missing");
						else if (Convert.ToString(actual, CultureInfo.InvariantCulture) != value.Data)
							differences.Add(entry.Hive + "\\" + entry.Path + "\\" + value.Name + " is " + actual + ", expected " + value.Data);
					}
					foreach (string present in key.GetValueNames())
					{
						if (!expected.Contains(present))
							differences.Add(entry.Hive + "\\" + entry.Path + "\\" + present + " is present but was not before the run");
					}
				}
			}
			return differences;
		}

		#region Persistence

		private const char Sep = '\t';

		public void Save(string path)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("# BCT_Tests settings backup, captured " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			sb.AppendLine("# Restore with: BCT_Tests.exe --restore \"" + path + "\"");
			sb.Append("SPI").Append(Sep).Append("FontSmoothing").Append(Sep).Append(AntialiasingEnabled ? "1" : "0").AppendLine();
			sb.Append("SPI").Append(Sep).Append("FontSmoothingType").Append(Sep).Append((uint)SmoothingType).AppendLine();
			sb.Append("SPI").Append(Sep).Append("FontSmoothingOrientation").Append(Sep).Append((uint)Orientation).AppendLine();
			sb.Append("SPI").Append(Sep).Append("FontSmoothingContrast").Append(Sep).Append(Contrast).AppendLine();
			foreach (KeyEntry entry in keys)
			{
				sb.Append("KEY").Append(Sep).Append(entry.Hive).Append(Sep).Append(entry.Path).Append(Sep)
					.Append(entry.Existed ? "EXISTS" : "ABSENT").AppendLine();
				foreach (ValueEntry value in entry.Values)
				{
					sb.Append("VAL").Append(Sep).Append(entry.Hive).Append(Sep).Append(entry.Path).Append(Sep)
						.Append(value.Name).Append(Sep).Append(value.Kind).Append(Sep).Append(value.Data).AppendLine();
				}
			}
			File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
		}

		public static SettingsBackup Load(string path)
		{
			SettingsBackup backup = new SettingsBackup();
			Dictionary<string, KeyEntry> byPath = new Dictionary<string, KeyEntry>(StringComparer.OrdinalIgnoreCase);

			foreach (string rawLine in File.ReadAllLines(path))
			{
				string line = rawLine.TrimEnd();
				if (line.Length == 0 || line.StartsWith("#"))
					continue;
				string[] parts = line.Split(Sep);
				switch (parts[0])
				{
					case "SPI":
						switch (parts[1])
						{
							case "FontSmoothing": backup.AntialiasingEnabled = parts[2] == "1"; break;
							case "FontSmoothingType": backup.SmoothingType = (SmoothingType)uint.Parse(parts[2], CultureInfo.InvariantCulture); break;
							case "FontSmoothingOrientation": backup.Orientation = (SmoothingOrientation)uint.Parse(parts[2], CultureInfo.InvariantCulture); break;
							case "FontSmoothingContrast": backup.Contrast = uint.Parse(parts[2], CultureInfo.InvariantCulture); break;
						}
						break;
					case "KEY":
						{
							KeyEntry entry = new KeyEntry
							{
								Hive = (Hive)Enum.Parse(typeof(Hive), parts[1]),
								Path = parts[2],
								Existed = parts[3] == "EXISTS"
							};
							backup.keys.Add(entry);
							byPath[parts[1] + "|" + parts[2]] = entry;
						}
						break;
					case "VAL":
						{
							KeyEntry entry;
							if (!byPath.TryGetValue(parts[1] + "|" + parts[2], out entry))
								break;
							entry.Values.Add(new ValueEntry
							{
								Name = parts[3],
								Kind = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), parts[4]),
								// A string value could itself contain a tab, so take everything left.
								Data = string.Join(Sep.ToString(), parts, 5, parts.Length - 5)
							});
						}
						break;
				}
			}
			return backup;
		}

		#endregion

		public string Describe()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("System font smoothing: " + (AntialiasingEnabled ? "on" : "off")
				+ ", type " + SmoothingType + ", orientation " + Orientation + ", contrast " + Contrast);
			foreach (KeyEntry entry in keys)
			{
				sb.Append("  ").Append(entry.Hive).Append('\\').Append(entry.Path).Append(": ");
				if (!entry.Existed)
					sb.AppendLine("(key does not exist)");
				else if (entry.Values.Count == 0)
					sb.AppendLine("(no values)");
				else
				{
					List<string> pairs = new List<string>();
					foreach (ValueEntry value in entry.Values)
						pairs.Add(value.Name + "=" + value.Data);
					sb.AppendLine(string.Join(", ", pairs));
				}
			}
			return sb.ToString();
		}
	}
}
