using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BCT_Tests.Settings
{
	internal enum Hive
	{
		HKCU,
		HKLM
	}

	/// <summary>
	/// Access to the per-display ClearType settings under Software\Microsoft\Avalon.Graphics.
	///
	/// Microsoft documents four of these values for WPF (ClearTypeLevel and TextContrastLevel under
	/// HKEY_CURRENT_USER, GammaLevel and PixelStructure under HKEY_LOCAL_MACHINE):
	/// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/cleartype-registry-settings
	/// Windows' own cttune.exe also writes EnhancedContrastLevel and GrayscaleEnhancedContrastLevel,
	/// which are not documented there but correspond to IDWriteRenderingParams::GetEnhancedContrast
	/// and IDWriteRenderingParams1::GetGrayscaleEnhancedContrast.  The harness tests every one of
	/// them in both hives, because which hive a value is honored in is exactly the sort of thing
	/// the documentation does not settle.
	/// </summary>
	internal static class AvalonRegistry
	{
		public const string BasePath = @"Software\Microsoft\Avalon.Graphics";

		public const string ClearTypeLevel = "ClearTypeLevel";
		public const string GammaLevel = "GammaLevel";
		public const string PixelStructure = "PixelStructure";
		public const string TextContrastLevel = "TextContrastLevel";
		public const string EnhancedContrastLevel = "EnhancedContrastLevel";
		public const string GrayscaleEnhancedContrastLevel = "GrayscaleEnhancedContrastLevel";

		/// <summary>
		/// Every value name the harness writes, and therefore every value name it clears in order
		/// to get a known starting point before each measurement.
		/// </summary>
		public static readonly string[] AllValueNames = new string[]
		{
			ClearTypeLevel,
			EnhancedContrastLevel,
			GammaLevel,
			GrayscaleEnhancedContrastLevel,
			PixelStructure,
			TextContrastLevel
		};

		private static readonly Regex DisplaySubkeyPattern = new Regex(@"^DISPLAY\d+$", RegexOptions.IgnoreCase);

		private static string[] cachedDisplayNames;

		/// <summary>
		/// The display subkey names to operate on: the displays Windows currently reports, plus any
		/// DISPLAYn subkeys that already exist under either hive (a stale key for a monitor that is
		/// not attached right now would otherwise be left holding a value that skews a measurement).
		///
		/// Subkeys that are not display names — HKCU has an "IgnoreDwmFlushErrors" subkey, for
		/// instance — are deliberately left alone.
		/// </summary>
		public static string[] DisplayNames
		{
			get
			{
				if (cachedDisplayNames != null)
					return cachedDisplayNames;

				SortedSet<string> names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (Screen screen in Screen.AllScreens)
				{
					int idxStart = screen.DeviceName.LastIndexOf('\\');
					string name = idxStart < 0 ? screen.DeviceName : screen.DeviceName.Substring(idxStart + 1);
					if (name.Length > 0)
						names.Add(name);
				}
				foreach (Hive hive in new Hive[] { Hive.HKCU, Hive.HKLM })
				{
					using (RegistryKey root = OpenBase(hive))
					using (RegistryKey baseFolder = root.OpenSubKey(BasePath, false))
					{
						if (baseFolder == null)
							continue;
						foreach (string subkey in baseFolder.GetSubKeyNames())
						{
							if (DisplaySubkeyPattern.IsMatch(subkey))
								names.Add(subkey);
						}
					}
				}
				cachedDisplayNames = names.ToArray();
				return cachedDisplayNames;
			}
		}

		/// <summary>
		/// Opens the root of a hive.  The 64-bit view is requested explicitly so that the values
		/// this harness writes are the ones a 64-bit application reads, with no WOW64 redirection
		/// in the picture.
		/// </summary>
		public static RegistryKey OpenBase(Hive hive)
		{
			return RegistryKey.OpenBaseKey(
				hive == Hive.HKCU ? RegistryHive.CurrentUser : RegistryHive.LocalMachine,
				RegistryView.Registry64);
		}

		public static string KeyPath(string displayName)
		{
			return BasePath + "\\" + displayName;
		}

		/// <summary>Writes a DWORD to every display's subkey in the given hive.</summary>
		public static void SetValueOnAllDisplays(Hive hive, string valueName, int value)
		{
			using (RegistryKey root = OpenBase(hive))
			{
				foreach (string display in DisplayNames)
				{
					using (RegistryKey key = root.CreateSubKey(KeyPath(display)))
					{
						if (key != null)
							key.SetValue(valueName, value, RegistryValueKind.DWord);
					}
				}
			}
		}

		/// <summary>Removes a value from every display's subkey in the given hive, if present.</summary>
		public static void DeleteValueOnAllDisplays(Hive hive, string valueName)
		{
			using (RegistryKey root = OpenBase(hive))
			{
				foreach (string display in DisplayNames)
				{
					using (RegistryKey key = root.OpenSubKey(KeyPath(display), true))
					{
						if (key == null)
							continue;
						if (key.GetValue(valueName) != null)
							key.DeleteValue(valueName, false);
					}
				}
			}
		}

		/// <summary>
		/// Removes every value this harness knows about from every display's subkey in both hives.
		/// With none of them set, WPF and DirectWrite fall back to the system font smoothing
		/// settings, which is the documented default state and the baseline each measurement starts
		/// from.
		/// </summary>
		public static void ClearAll()
		{
			foreach (Hive hive in new Hive[] { Hive.HKCU, Hive.HKLM })
				foreach (string valueName in AllValueNames)
					DeleteValueOnAllDisplays(hive, valueName);
		}

		/// <summary>
		/// Returns true if the current process can write to HKLM's Avalon.Graphics keys, which the
		/// full sweep needs both to set HKLM values and to clear the ones that are already there.
		/// </summary>
		public static bool CanWriteLocalMachine(out string error)
		{
			error = null;
			try
			{
				using (RegistryKey root = OpenBase(Hive.HKLM))
				using (RegistryKey key = root.CreateSubKey(KeyPath(DisplayNames.Length > 0 ? DisplayNames[0] : "DISPLAY1")))
				{
					if (key == null)
					{
						error = "could not open the key";
						return false;
					}
					// A probe value is the only way to know whether the key is really writable;
					// CreateSubKey can succeed on an existing key the caller may not write to.
					const string probeName = "BCT_Tests_WriteProbe";
					key.SetValue(probeName, 1, RegistryValueKind.DWord);
					key.DeleteValue(probeName, false);
					return true;
				}
			}
			catch (Exception ex)
			{
				error = ex.Message;
				return false;
			}
		}
	}
}
