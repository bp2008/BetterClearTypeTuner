using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BCT_Tests.Native;
using BCT_Tests.Settings;
using Microsoft.Win32;

namespace BCT_Tests
{
	/// <summary>
	/// Asks DirectWrite what it resolves its rendering parameters to, rather than assuming.
	///
	/// The rest of this harness measures rendered pixels, which answers "does this setting change
	/// anything".  This answers the question underneath it: what does DirectWrite think the settings
	/// are?  It matters most in the state a clean Windows installation is actually in, with no
	/// Avalon.Graphics key at all, where DirectWrite substitutes values of its own that are not the
	/// ones Microsoft documents for the registry.
	///
	/// The sweep also covers the partial states - the key present but empty, one value set and the
	/// rest missing - because those behave less predictably than either extreme, and knowing which
	/// of them DirectWrite treats as "unset" explains a good deal of otherwise baffling behaviour.
	/// </summary>
	internal static class DWriteDefaultsProbe
	{
		/// <summary>
		/// One measurement of DirectWrite's resolved parameters.
		/// </summary>
		private struct Measurement
		{
			public bool Ok;
			public string Error;
			public float Gamma;
			public float EnhancedContrast;
			public float ClearTypeLevel;
			public DWRITE_PIXEL_GEOMETRY PixelGeometry;
			public DWRITE_RENDERING_MODE RenderingMode;
			public bool HasGrayscale;
			public float GrayscaleEnhancedContrast;

			/// <summary>The measurement in the integer units the registry and Better ClearType Tuner use.</summary>
			public string AsRegistryUnits()
			{
				if (!Ok)
					return "FAILED: " + Error;
				return "GammaLevel " + Scale(Gamma, 1000)
					+ ", EnhancedContrastLevel " + Scale(EnhancedContrast, 100)
					+ ", ClearTypeLevel " + Scale(ClearTypeLevel, 100)
					+ ", PixelStructure " + (int)PixelGeometry + " (" + PixelGeometry + ")"
					+ ", RenderingMode " + RenderingMode
					+ (HasGrayscale ? ", GrayscaleEnhancedContrastLevel " + Scale(GrayscaleEnhancedContrast, 100) : "");
			}

			private static int Scale(float value, int scale)
			{
				return (int)Math.Round(value * scale, MidpointRounding.AwayFromZero);
			}
		}

		/// <summary>
		/// Reads DirectWrite's resolved parameters for the primary monitor.
		///
		/// The factory is ISOLATED and built afresh for every measurement.  A shared factory is
		/// process-wide and holds on to what it has already resolved, so a sweep that changes the
		/// registry between measurements would keep reading back the first answer.
		/// </summary>
		private static Measurement Measure()
		{
			return Measure(hMonitor: IntPtr.Zero);
		}

		private static Measurement Measure(IntPtr hMonitor)
		{
			Measurement m = new Measurement();
			object factoryObj = null;
			IDWriteRenderingParams p = null;
			try
			{
				int hr = DWrite.DWriteCreateFactory(DWRITE_FACTORY_TYPE.ISOLATED, DWrite.IID_IDWriteFactory, out factoryObj);
				if (hr != 0 || factoryObj == null)
				{
					m.Error = "DWriteCreateFactory 0x" + hr.ToString("X8");
					return m;
				}
				IDWriteFactory factory = (IDWriteFactory)factoryObj;

				hr = hMonitor == IntPtr.Zero
					? factory.CreateRenderingParams(out p)
					: factory.CreateMonitorRenderingParams(hMonitor, out p);
				if (hr != 0 || p == null)
				{
					m.Error = (hMonitor == IntPtr.Zero ? "CreateRenderingParams" : "CreateMonitorRenderingParams")
						+ " 0x" + hr.ToString("X8");
					return m;
				}

				m.Gamma = p.GetGamma();
				m.EnhancedContrast = p.GetEnhancedContrast();
				m.ClearTypeLevel = p.GetClearTypeLevel();
				m.PixelGeometry = p.GetPixelGeometry();
				m.RenderingMode = p.GetRenderingMode();

				IDWriteRenderingParams1 p1 = p as IDWriteRenderingParams1;
				if (p1 != null)
				{
					m.HasGrayscale = true;
					m.GrayscaleEnhancedContrast = p1.GetGrayscaleEnhancedContrast();
				}
				m.Ok = true;
				return m;
			}
			catch (Exception ex)
			{
				m.Error = ex.Message;
				return m;
			}
			finally
			{
				Release(p);
				Release(factoryObj);
			}
		}

		private static void Release(object o)
		{
			if (o != null && Marshal.IsComObject(o))
				Marshal.ReleaseComObject(o);
		}

		/// <summary>
		/// Reports what DirectWrite resolves to right now, changing nothing.
		/// </summary>
		public static int ReportCurrent(Action<string> log)
		{
			log("Avalon.Graphics registry state:");
			log(DescribeRegistry());
			log(SystemFontSmoothing.Describe());
			log("");
			log("DirectWrite resolves (primary monitor):");
			log("  " + Measure().AsRegistryUnits());
			foreach (KeyValuePair<string, IntPtr> monitor in Monitors())
				log("  " + monitor.Key.PadRight(10) + " " + Measure(monitor.Value).AsRegistryUnits());
			return 0;
		}

		/// <summary>
		/// Runs the whole sweep: puts the machine into each state in turn, asks DirectWrite what it
		/// resolves to, and puts everything back.  Needs administrator rights, because the states
		/// include removing the HKEY_LOCAL_MACHINE key.
		/// </summary>
		public static int RunSweep(string outputDir, Action<string> log)
		{
			if (!Program.IsElevated)
			{
				log("This sweep has to remove the HKEY_LOCAL_MACHINE key, so it needs administrator rights.");
				log("Re-run an elevated BCT_Tests.exe --dwrite-defaults --probe.");
				return 2;
			}

			Directory.CreateDirectory(outputDir);
			SettingsBackup backup = SettingsBackup.Capture();
			string backupPath = Path.Combine(outputDir, "dwrite-defaults-backup.txt");
			backup.Save(backupPath);
			log("Backup saved to " + backupPath);
			log("If this run is interrupted, undo it with:  BCT_Tests.exe --restore \"" + backupPath + "\"");
			log("");
			log("Starting state:");
			log(backup.Describe());
			log("");

			List<string> lines = new List<string>();
			try
			{
				SmoothingOrientation originalOrientation = SystemFontSmoothing.GetOrientation();

				// Each scenario is described by what it leaves in the registry.  They run from the
				// clean-installation state outwards, so the first result is the one that says what
				// DirectWrite falls back to when nothing has ever been tuned.
				Record(lines, log, "keys absent entirely", delegate
				{
					DeleteBothHives();
				});

				Record(lines, log, "key present, display subkey present, no values", delegate
				{
					DeleteBothHives();
					CreateEmptyDisplayKeys();
				});

				foreach (string valueName in new string[]
					{ AvalonRegistry.GammaLevel, AvalonRegistry.ClearTypeLevel,
					  AvalonRegistry.EnhancedContrastLevel, AvalonRegistry.PixelStructure })
				{
					string name = valueName;
					int probeValue = ProbeValueFor(name);
					Record(lines, log, "HKCU " + name + "=" + probeValue + " only", delegate
					{
						DeleteBothHives();
						AvalonRegistry.SetValueOnAllDisplays(Hive.HKCU, name, probeValue);
					});
				}

				Record(lines, log, "HKLM GammaLevel=1500 only (no HKCU key at all)", delegate
				{
					DeleteBothHives();
					AvalonRegistry.SetValueOnAllDisplays(Hive.HKLM, AvalonRegistry.GammaLevel, 1500);
				});

				Record(lines, log, "HKLM GammaLevel=1500 and HKCU GammaLevel=1300", delegate
				{
					DeleteBothHives();
					AvalonRegistry.SetValueOnAllDisplays(Hive.HKLM, AvalonRegistry.GammaLevel, 1500);
					AvalonRegistry.SetValueOnAllDisplays(Hive.HKCU, AvalonRegistry.GammaLevel, 1300);
				});

				// Whether the subpixel order survives with no PixelStructure value to state it is
				// the question that decides whether removing these keys costs a BGR user anything.
				Record(lines, log, "keys absent, SPI orientation BGR", delegate
				{
					DeleteBothHives();
					SystemFontSmoothing.SetOrientation(SmoothingOrientation.BGR);
				});

				Record(lines, log, "keys absent, SPI orientation RGB", delegate
				{
					DeleteBothHives();
					SystemFontSmoothing.SetOrientation(SmoothingOrientation.RGB);
				});

				Record(lines, log, "HKCU PixelStructure=1 (RGB), SPI orientation BGR", delegate
				{
					DeleteBothHives();
					AvalonRegistry.SetValueOnAllDisplays(Hive.HKCU, AvalonRegistry.PixelStructure, 1);
					SystemFontSmoothing.SetOrientation(SmoothingOrientation.BGR);
				});

				SystemFontSmoothing.SetOrientation(originalOrientation);
			}
			finally
			{
				log("");
				log("Restoring ...");
				List<string> problems = backup.Restore();
				List<string> differences = backup.Verify();
				foreach (string problem in problems)
					log("  restore problem: " + problem);
				foreach (string difference in differences)
					log("  still different: " + difference);
				log(problems.Count == 0 && differences.Count == 0
					? "  Restored and verified."
					: "  RESTORE INCOMPLETE - undo with: BCT_Tests.exe --restore \"" + backupPath + "\"");
			}

			string reportPath = Path.Combine(outputDir, "DWriteDefaults.md");
			File.WriteAllText(reportPath, BuildReport(lines), Encoding.UTF8);
			log("");
			log("Wrote " + reportPath);
			return 0;
		}

		/// <summary>
		/// A value distinguishable from every default, so that a measurement echoing it back proves
		/// DirectWrite read it rather than fell back to something that happened to look the same.
		/// </summary>
		private static int ProbeValueFor(string valueName)
		{
			if (valueName == AvalonRegistry.GammaLevel)
				return 1300;
			if (valueName == AvalonRegistry.ClearTypeLevel)
				return 40;
			if (valueName == AvalonRegistry.EnhancedContrastLevel)
				return 200;
			if (valueName == AvalonRegistry.PixelStructure)
				return 2;
			return 1;
		}

		private static void Record(List<string> lines, Action<string> log, string caption, Action arrange)
		{
			arrange();
			Measurement m = Measure();
			log(caption.PadRight(48) + " -> " + m.AsRegistryUnits());
			lines.Add("| " + caption + " | " + m.AsRegistryUnits().Replace("|", "\\|") + " |");
		}

		private static void DeleteBothHives()
		{
			foreach (Hive hive in new Hive[] { Hive.HKCU, Hive.HKLM })
			{
				using (RegistryKey root = AvalonRegistry.OpenBase(hive))
					root.DeleteSubKeyTree(AvalonRegistry.BasePath, false);
			}
		}

		/// <summary>
		/// Recreates the display subkeys with nothing in them, which is the state a tuner that clears
		/// its values without removing its keys leaves behind.
		/// </summary>
		private static void CreateEmptyDisplayKeys()
		{
			using (RegistryKey root = AvalonRegistry.OpenBase(Hive.HKCU))
			{
				foreach (string display in AvalonRegistry.DisplayNames)
					root.CreateSubKey(AvalonRegistry.KeyPath(display));
			}
		}

		private static string DescribeRegistry()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Hive hive in new Hive[] { Hive.HKCU, Hive.HKLM })
			{
				using (RegistryKey root = AvalonRegistry.OpenBase(hive))
				using (RegistryKey key = root.OpenSubKey(AvalonRegistry.BasePath, false))
				{
					if (key == null)
					{
						sb.AppendLine("  " + hive + "  (key does not exist)");
						continue;
					}
					string[] subs = key.GetSubKeyNames();
					if (subs.Length == 0)
					{
						sb.AppendLine("  " + hive + "  (key exists, no subkeys)");
						continue;
					}
					foreach (string sub in subs)
					{
						using (RegistryKey display = key.OpenSubKey(sub, false))
						{
							if (display == null)
								continue;
							List<string> pairs = new List<string>();
							foreach (string name in display.GetValueNames())
								pairs.Add(name + "=" + display.GetValue(name));
							sb.AppendLine("  " + hive + "\\" + sub + "  "
								+ (pairs.Count == 0 ? "(no values)" : string.Join(", ", pairs)));
						}
					}
				}
			}
			return sb.ToString().TrimEnd();
		}

		/// <summary>
		/// The Windows version, read from the registry rather than from Environment.OSVersion.
		/// Without a compatibility manifest naming Windows 10, that property reports 6.2.9200 on
		/// every modern Windows - which in a report of version-specific findings would be worse
		/// than saying nothing.
		/// </summary>
		private static string DescribeWindows()
		{
			try
			{
				using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				using (RegistryKey key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false))
				{
					if (key != null)
					{
						string product = Convert.ToString(key.GetValue("ProductName"));
						string display = Convert.ToString(key.GetValue("DisplayVersion"));
						string build = Convert.ToString(key.GetValue("CurrentBuild"));
						string ubr = Convert.ToString(key.GetValue("UBR"));
						string described = product;
						if (!string.IsNullOrEmpty(display))
							described += " " + display;
						if (!string.IsNullOrEmpty(build))
							described += " (build " + build + (string.IsNullOrEmpty(ubr) ? "" : "." + ubr) + ")";
						if (!string.IsNullOrEmpty(described))
							return described;
					}
				}
			}
			catch (Exception)
			{
			}
			return Environment.OSVersion.VersionString;
		}

		private static Dictionary<string, IntPtr> Monitors()
		{
			Dictionary<string, IntPtr> found = new Dictionary<string, IntPtr>();
			foreach (Screen screen in Screen.AllScreens)
			{
				IntPtr handle = NativeMethods.MonitorFromPoint(
					new NativeMethods.POINT
					{
						x = screen.Bounds.Left + (screen.Bounds.Width / 2),
						y = screen.Bounds.Top + (screen.Bounds.Height / 2)
					},
					NativeMethods.MONITOR_DEFAULTTONEAREST);
				int idx = screen.DeviceName.LastIndexOf('\\');
				found[idx < 0 ? screen.DeviceName : screen.DeviceName.Substring(idx + 1)] = handle;
			}
			return found;
		}

		private static string BuildReport(List<string> rows)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("# What DirectWrite resolves to");
			sb.AppendLine();
			sb.AppendLine("Measured with `IDWriteFactory::CreateRenderingParams` on an isolated factory, one");
			sb.AppendLine("registry state at a time.  These are the numbers DirectWrite hands to every");
			sb.AppendLine("application that asks it for the default rendering parameters, so they are what");
			sb.AppendLine("Firefox, Edge and WPF start from.");
			sb.AppendLine();
			sb.AppendLine("Generated " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " on " + DescribeWindows() + ".");
			sb.AppendLine();
			sb.AppendLine("| Registry state | DirectWrite resolves to |");
			sb.AppendLine("| --- | --- |");
			foreach (string row in rows)
				sb.AppendLine(row);
			return sb.ToString();
		}
	}
}
