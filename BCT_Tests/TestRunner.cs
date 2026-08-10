using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BCT_Tests.Rendering;
using BCT_Tests.Settings;

namespace BCT_Tests
{
	internal class RenderRecord
	{
		public string Name;
		public string GdiImage;
		public string DwImage;
		public string DwAppImage;
		public string RawFile;
		public Dictionary<string, string> Info = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public string Get(string key)
		{
			string value;
			return Info.TryGetValue(key, out value) ? value : null;
		}
	}

	internal class ValueResult
	{
		public SettingValue Value;
		public RenderRecord Render;
		public ImageDifference GdiDiff;
		public ImageDifference DwDiff;
		public ImageDifference DwAppDiff;
		/// <summary>Which DirectWrite rendering parameters moved compared to the baseline render.</summary>
		public List<string> DwParamChanges = new List<string>();
	}

	internal class SettingResult
	{
		public BaseState State;
		public SettingUnderTest Setting;
		public List<ValueResult> Values = new List<ValueResult>();
		public string NotTestedReason;
		/// <summary>Set only when the setting has a prerequisite and so got its own baseline render.</summary>
		public RenderRecord Baseline;

		public bool Tested { get { return NotTestedReason == null; } }

		public bool GdiAffected
		{
			get { return Values.Any(v => v.GdiDiff != null && v.GdiDiff.AnyDifference); }
		}

		public bool DwAffected
		{
			get { return Values.Any(v => v.DwDiff != null && v.DwDiff.AnyDifference); }
		}

		public bool DwAppAffected
		{
			get { return Values.Any(v => v.DwAppDiff != null && v.DwAppDiff.AnyDifference); }
		}

		/// <summary>
		/// The DirectWrite parameters this setting was seen to move, across all of its values.
		/// This is the strongest available evidence of what a registry value actually feeds.
		/// </summary>
		public List<string> DwParametersTouched
		{
			get
			{
				List<string> names = new List<string>();
				foreach (ValueResult value in Values)
					foreach (string change in value.DwParamChanges)
					{
						string name = change.Split(' ')[0];
						if (!names.Contains(name))
							names.Add(name);
					}
				return names;
			}
		}
	}

	/// <summary>
	/// One base state's baseline renders compared against the first base state's, which is what
	/// answers "does the system font-smoothing mode reach this renderer at all?".
	/// </summary>
	internal class BaseStateComparison
	{
		public BaseState Reference;
		public BaseState State;
		public ImageDifference GdiDiff;
		public ImageDifference DwDiff;
		public ImageDifference DwAppDiff;
	}

	internal class RunResults
	{
		public DateTime Started = DateTime.Now;
		public TimeSpan Duration;
		public string OutputDirectory;
		public List<BaseState> States = new List<BaseState>();
		public List<SettingUnderTest> Settings = new List<SettingUnderTest>();
		public Dictionary<string, RenderRecord> Baselines = new Dictionary<string, RenderRecord>();
		public List<SettingResult> Results = new List<SettingResult>();
		public List<BaseStateComparison> BaseStateComparisons = new List<BaseStateComparison>();
		public List<string> Warnings = new List<string>();
		public string BackupDescription;
		public List<string> RestoreProblems = new List<string>();
		public List<string> RestoreDifferences = new List<string>();
		public bool RendersAreDeterministic;
		public bool LocalMachineWritable;
		public int RenderCount;

		public SettingResult Find(BaseState state, SettingUnderTest setting)
		{
			return Results.FirstOrDefault(r => r.State.Id == state.Id && r.Setting.Id == setting.Id);
		}
	}

	/// <summary>
	/// Drives the whole sweep: back up, walk every base state and every setting one value at a
	/// time, restore, then hand the measurements to the report writer.
	/// </summary>
	internal class TestRunner
	{
		private readonly string outputDir;
		private readonly Action<string> log;
		private readonly string exePath;
		private readonly bool allowWithoutLocalMachineAccess;
		private readonly bool quick;

		public TestRunner(string outputDir, Action<string> log, bool allowWithoutLocalMachineAccess, bool quick)
		{
			this.outputDir = outputDir;
			this.log = log;
			this.allowWithoutLocalMachineAccess = allowWithoutLocalMachineAccess;
			this.quick = quick;
			exePath = Process.GetCurrentProcess().MainModule.FileName;
		}

		public RunResults Run()
		{
			RunResults results = new RunResults { OutputDirectory = outputDir };
			Stopwatch stopwatch = Stopwatch.StartNew();

			Directory.CreateDirectory(outputDir);
			// Renders from a previous run would otherwise be compared against this run's baseline
			// if a configuration failed to produce a new image.
			DeleteIfPresent(Path.Combine(outputDir, ChildRenderProcess.ImagesFolder));
			DeleteIfPresent(Path.Combine(outputDir, ChildRenderProcess.RawFolder));

			// The snapshot has to be taken before anything else touches the registry, including
			// the write probe below, which would otherwise create a key the snapshot thinks was
			// always there.
			log("Capturing current settings...");
			SettingsBackup backup = SettingsBackup.Capture();
			string backupPath = Path.Combine(outputDir, "settings-backup.txt");
			backup.Save(backupPath);
			results.BackupDescription = backup.Describe();
			log(results.BackupDescription);
			log("Backup written to " + backupPath);
			foreach (string unsupported in backup.UnsupportedValues)
				results.Warnings.Add("Registry value of a type this harness cannot restore, left untouched: " + unsupported);

			string hklmError;
			results.LocalMachineWritable = AvalonRegistry.CanWriteLocalMachine(out hklmError);
			if (!results.LocalMachineWritable)
			{
				string message = "HKEY_LOCAL_MACHINE is not writable (" + hklmError + "), so this run cannot test "
					+ "the HKLM values and cannot clear the HKLM values that are already set. Re-run elevated for a complete matrix.";
				if (!allowWithoutLocalMachineAccess)
				{
					log("ERROR: " + message);
					log("Pass --allow-partial to run anyway with the HKLM rows left untested.");
					results.Warnings.Add(message);
					results.Duration = stopwatch.Elapsed;
					return results;
				}
				results.Warnings.Add(message);
				log("WARNING: " + message);
			}

			results.States = TestPlan.BaseStates();
			results.Settings = TestPlan.Settings();
			if (quick)
			{
				// Enough of the plan to exercise every code path end to end without sitting through
				// the whole sweep; used to check the harness itself, not to produce a matrix.
				results.States = results.States.Where(s => s.Id == "cleartype-rgb").ToList();
				results.Settings = results.Settings
					.Where(s => s.Id == "hkcu-cleartypelevel" || s.Id == "hklm-pixelstructure" || s.Id == "spi-fontsmoothingcontrast")
					.ToList();
				log("QUICK MODE: only " + results.States.Count + " base state and " + results.Settings.Count + " settings.");
			}
			results.RendersAreDeterministic = true;

			try
			{
				foreach (BaseState state in results.States)
				{
					log("");
					log("=== Base state: " + state.Label + " ===");
					state.Apply();
					ClearAvalonValues(results);

					RenderRecord baseline = Render(state.Id + "__baseline", results);
					results.Baselines[state.Id] = baseline;
					log("  baseline: " + Describe(baseline));

					// A repeat of the same configuration proves the renders are reproducible; if
					// they were not, "the image changed" would mean nothing.
					RenderRecord repeat = Render(state.Id + "__baseline-repeat", results);
					ImageDifference gdiRepeat = Compare(baseline.GdiImage, repeat.GdiImage);
					ImageDifference dwRepeat = Compare(baseline.DwImage, repeat.DwImage);
					ImageDifference dwAppRepeat = Compare(baseline.DwAppImage, repeat.DwAppImage);
					if (gdiRepeat.AnyDifference || dwRepeat.AnyDifference || dwAppRepeat.AnyDifference)
					{
						results.RendersAreDeterministic = false;
						results.Warnings.Add("Renders are not reproducible in base state " + state.Label
							+ " (GDI: " + gdiRepeat + ", DirectWrite: " + dwRepeat + "). Treat the results with suspicion.");
						log("  WARNING: repeat render differs (GDI " + gdiRepeat + ", DW " + dwRepeat + ")");
					}

					foreach (SettingUnderTest setting in results.Settings)
					{
						SettingResult result = new SettingResult { State = state, Setting = setting };
						results.Results.Add(result);

						if (setting.NeedsLocalMachineWrite && !results.LocalMachineWritable)
						{
							result.NotTestedReason = "not run without write access to HKEY_LOCAL_MACHINE";
							continue;
						}

						// A setting with a prerequisite is compared against the prerequisite alone,
						// so what the comparison shows is still only the value under test.
						RenderRecord comparisonBaseline = baseline;
						if (setting.ApplyPrerequisite != null)
						{
							ClearAvalonValues(results);
							setting.ApplyPrerequisite();
							comparisonBaseline = Render(state.Id + "__" + setting.Id + "__baseline", results);
							result.Baseline = comparisonBaseline;
						}

						foreach (SettingValue value in setting.Values)
						{
							ClearAvalonValues(results);
							if (setting.IsSystemParameter)
								state.Apply();
							if (setting.ApplyPrerequisite != null)
								setting.ApplyPrerequisite();
							value.Apply();

							RenderRecord render = Render(state.Id + "__" + setting.Id + "__" + value.Id, results);
							ValueResult valueResult = new ValueResult
							{
								Value = value,
								Render = render,
								GdiDiff = Compare(comparisonBaseline.GdiImage, render.GdiImage),
								DwDiff = Compare(comparisonBaseline.DwImage, render.DwImage),
								DwAppDiff = Compare(comparisonBaseline.DwAppImage, render.DwAppImage)
							};
							valueResult.DwParamChanges = DiffDwParams(comparisonBaseline, render);
							result.Values.Add(valueResult);
						}

						// A system parameter was just moved away from the base state's value, so
						// put the base state back before the next setting is measured.
						if (setting.IsSystemParameter)
							state.Apply();

						log("  " + setting.Name + " (" + CategoryLabel(setting.Category) + "): GDI "
							+ (result.GdiAffected ? "AFFECTED" : "no change") + ", DirectWrite "
							+ (result.DwAffected ? "AFFECTED" : "no change")
							+ (result.DwParametersTouched.Count > 0 ? "  [" + string.Join(", ", result.DwParametersTouched) + "]" : ""));
					}
				}
			}
			finally
			{
				CompareBaseStates(results);

				log("");
				log("Restoring original settings...");
				results.RestoreProblems = backup.Restore();
				results.RestoreDifferences = backup.Verify();
				foreach (string problem in results.RestoreProblems)
					log("  restore problem: " + problem);
				foreach (string difference in results.RestoreDifferences)
					log("  still different after restore: " + difference);
				if (results.RestoreProblems.Count == 0 && results.RestoreDifferences.Count == 0)
					log("  all settings verified back at their original values.");
				log("  now: " + SystemFontSmoothing.Describe());
			}

			results.Duration = stopwatch.Elapsed;
			SaveMeta(results);
			return results;
		}

		#region Rebuilding the reports without re-measuring

		public const string MetaFileName = "run-meta.txt";

		/// <summary>
		/// Everything about the run that cannot be recomputed from the images and raw files left
		/// behind.  Everything else is deliberately not stored, so that a rebuilt report is derived
		/// from the same measurements the original one was.
		/// </summary>
		private void SaveMeta(RunResults results)
		{
			List<string> lines = new List<string>();
			lines.Add("started=" + results.Started.ToString("o"));
			lines.Add("durationSeconds=" + results.Duration.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
			lines.Add("renderCount=" + results.RenderCount);
			lines.Add("localMachineWritable=" + results.LocalMachineWritable);
			lines.Add("backupDescription=" + results.BackupDescription.Replace("\r", "").Replace("\n", "\\n"));
			foreach (string warning in results.Warnings)
				lines.Add("warning=" + warning.Replace("\r", " ").Replace("\n", " "));
			foreach (string problem in results.RestoreProblems)
				lines.Add("restoreProblem=" + problem.Replace("\r", " ").Replace("\n", " "));
			foreach (string difference in results.RestoreDifferences)
				lines.Add("restoreDifference=" + difference.Replace("\r", " ").Replace("\n", " "));
			File.WriteAllLines(Path.Combine(outputDir, MetaFileName), lines);
		}

		/// <summary>
		/// Rebuilds the measurements from the images and raw files a previous run left behind, so
		/// the reports can be regenerated without touching the machine's settings again.  The test
		/// plan is the same code the run used, so the configuration names line up.
		/// </summary>
		public static RunResults RebuildFromDisk(string outputDir, Action<string> log)
		{
			RunResults results = new RunResults { OutputDirectory = outputDir, RendersAreDeterministic = true };
			TestRunner runner = new TestRunner(outputDir, log, true, false);

			string metaPath = Path.Combine(outputDir, MetaFileName);
			if (!File.Exists(metaPath))
			{
				log("No " + MetaFileName + " in " + outputDir + "; run the sweep first.");
				return results;
			}
			foreach (string line in File.ReadAllLines(metaPath))
			{
				int idx = line.IndexOf('=');
				if (idx <= 0)
					continue;
				string key = line.Substring(0, idx);
				string value = line.Substring(idx + 1);
				switch (key)
				{
					case "started": results.Started = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); break;
					case "durationSeconds": results.Duration = TimeSpan.FromSeconds(double.Parse(value, CultureInfo.InvariantCulture)); break;
					case "renderCount": results.RenderCount = int.Parse(value, CultureInfo.InvariantCulture); break;
					case "localMachineWritable": results.LocalMachineWritable = bool.Parse(value); break;
					case "backupDescription": results.BackupDescription = value.Replace("\\n", Environment.NewLine); break;
					case "warning": results.Warnings.Add(value); break;
					case "restoreProblem": results.RestoreProblems.Add(value); break;
					case "restoreDifference": results.RestoreDifferences.Add(value); break;
				}
			}

			results.States = TestPlan.BaseStates();
			results.Settings = TestPlan.Settings();

			foreach (BaseState state in results.States)
			{
				RenderRecord baseline = runner.LoadRender(state.Id + "__baseline");
				if (baseline == null)
					continue;
				results.Baselines[state.Id] = baseline;

				RenderRecord repeat = runner.LoadRender(state.Id + "__baseline-repeat");
				if (repeat != null && (runner.Compare(baseline.GdiImage, repeat.GdiImage).AnyDifference
					|| runner.Compare(baseline.DwImage, repeat.DwImage).AnyDifference
					|| runner.Compare(baseline.DwAppImage, repeat.DwAppImage).AnyDifference))
					results.RendersAreDeterministic = false;

				foreach (SettingUnderTest setting in results.Settings)
				{
					SettingResult result = new SettingResult { State = state, Setting = setting };
					results.Results.Add(result);

					RenderRecord comparisonBaseline = baseline;
					if (setting.ApplyPrerequisite != null)
					{
						RenderRecord settingBaseline = runner.LoadRender(state.Id + "__" + setting.Id + "__baseline");
						if (settingBaseline != null)
						{
							comparisonBaseline = settingBaseline;
							result.Baseline = settingBaseline;
						}
					}

					foreach (SettingValue value in setting.Values)
					{
						RenderRecord render = runner.LoadRender(state.Id + "__" + setting.Id + "__" + value.Id);
						if (render == null)
							continue;
						result.Values.Add(new ValueResult
						{
							Value = value,
							Render = render,
							GdiDiff = runner.Compare(comparisonBaseline.GdiImage, render.GdiImage),
							DwDiff = runner.Compare(comparisonBaseline.DwImage, render.DwImage),
							DwAppDiff = runner.Compare(comparisonBaseline.DwAppImage, render.DwAppImage),
							DwParamChanges = DiffDwParams(comparisonBaseline, render)
						});
					}

					if (result.Values.Count == 0)
						result.NotTestedReason = setting.NeedsLocalMachineWrite && !results.LocalMachineWritable
							? "not run without write access to HKEY_LOCAL_MACHINE"
							: "no renders found for this setting";
				}
			}

			runner.CompareBaseStates(results);
			return results;
		}

		#endregion

		/// <summary>
		/// Compares each base state's baseline against the first one's.  The registry sweep only
		/// ever compares renders taken within one base state, so without this the report would
		/// never say whether the system font-smoothing mode reaches a given renderer at all.
		/// </summary>
		private void CompareBaseStates(RunResults results)
		{
			RenderRecord reference;
			if (results.States.Count < 2 || !results.Baselines.TryGetValue(results.States[0].Id, out reference))
				return;

			for (int i = 1; i < results.States.Count; i++)
			{
				RenderRecord other;
				if (!results.Baselines.TryGetValue(results.States[i].Id, out other))
					continue;
				results.BaseStateComparisons.Add(new BaseStateComparison
				{
					Reference = results.States[0],
					State = results.States[i],
					GdiDiff = Compare(reference.GdiImage, other.GdiImage),
					DwDiff = Compare(reference.DwImage, other.DwImage),
					DwAppDiff = Compare(reference.DwAppImage, other.DwAppImage)
				});
			}

			log("");
			log("System font-smoothing mode, compared with \"" + results.States[0].Label + "\":");
			foreach (BaseStateComparison comparison in results.BaseStateComparisons)
			{
				log("  " + comparison.State.Label.PadRight(26)
					+ " GDI " + (comparison.GdiDiff.AnyDifference ? "AFFECTED" : "no change")
					+ ", DirectWrite defaults " + (comparison.DwDiff.AnyDifference ? "AFFECTED" : "no change")
					+ ", DirectWrite app-configured " + (comparison.DwAppDiff.AnyDifference ? "AFFECTED" : "no change"));
			}
		}

		private static void DeleteIfPresent(string directory)
		{
			if (Directory.Exists(directory))
				Directory.Delete(directory, true);
		}

		private void ClearAvalonValues(RunResults results)
		{
			try
			{
				AvalonRegistry.ClearAll();
			}
			catch (Exception ex)
			{
				string message = "Could not clear Avalon.Graphics values: " + ex.Message;
				if (!results.Warnings.Contains(message))
					results.Warnings.Add(message);
			}
		}

		private static string CategoryLabel(SettingCategory category)
		{
			switch (category)
			{
				case SettingCategory.AvalonCurrentUser: return "HKCU";
				case SettingCategory.AvalonLocalMachine: return "HKLM";
				default: return "system";
			}
		}

		private static string Describe(RenderRecord record)
		{
			string monitorParams = record.Get("dw.monitor.gamma") == null
				? "DirectWrite parameters unavailable"
				: "DirectWrite gamma=" + record.Get("dw.monitor.gamma")
					+ " clearTypeLevel=" + record.Get("dw.monitor.clearTypeLevel")
					+ " geometry=" + record.Get("dw.monitor.pixelGeometry")
					+ " mode=" + record.Get("dw.monitor.renderingMode");
			return monitorParams;
		}

		/// <summary>
		/// Launches a fresh process to render the current configuration and reads back what it
		/// recorded.
		/// </summary>
		private RenderRecord Render(string configName, RunResults results)
		{
			// The child inherits this console, so anything it prints on the way down lands in the
			// run log rather than in a pipe nobody is draining.
			ProcessStartInfo startInfo = new ProcessStartInfo(exePath,
				"--render --out \"" + outputDir + "\" --name \"" + configName + "\"")
			{
				UseShellExecute = false,
				CreateNoWindow = true
			};
			using (Process process = Process.Start(startInfo))
			{
				if (!process.WaitForExit(60000))
				{
					try { process.Kill(); }
					catch (Exception) { }
					results.Warnings.Add("Render process for " + configName + " timed out.");
					return EmptyRecord(configName);
				}
				if (process.ExitCode != 0)
					results.Warnings.Add("Render process for " + configName + " exited with code " + process.ExitCode + ".");
			}
			results.RenderCount++;

			RenderRecord record = LoadRender(configName);
			if (record == null)
			{
				results.Warnings.Add("Render process for " + configName + " produced no data file.");
				return EmptyRecord(configName);
			}

			string error = record.Get("dw.error");
			if (error != null)
				results.Warnings.Add(configName + ": DirectWrite error: " + error);
			error = record.Get("dwapp.error");
			if (error != null)
				results.Warnings.Add(configName + ": DirectWrite (app-configured) error: " + error);
			error = record.Get("gdi.error");
			if (error != null)
				results.Warnings.Add(configName + ": GDI error: " + error);

			return record;
		}

		private static RenderRecord EmptyRecord(string configName)
		{
			return new RenderRecord
			{
				Name = configName,
				GdiImage = ChildRenderProcess.GdiImageRelativePath(configName),
				DwImage = ChildRenderProcess.DwImageRelativePath(configName),
				DwAppImage = ChildRenderProcess.DwAppImageRelativePath(configName),
				RawFile = ChildRenderProcess.RawRelativePath(configName)
			};
		}

		/// <summary>
		/// Reads back what a render process recorded.  Returns null if that configuration was never
		/// rendered.
		/// </summary>
		private RenderRecord LoadRender(string configName)
		{
			RenderRecord record = EmptyRecord(configName);
			string rawPath = Path.Combine(outputDir, record.RawFile.Replace('/', Path.DirectorySeparatorChar));
			if (!File.Exists(rawPath))
				return null;
			foreach (string line in File.ReadAllLines(rawPath))
			{
				int idx = line.IndexOf('=');
				if (idx > 0)
					record.Info[line.Substring(0, idx)] = line.Substring(idx + 1);
			}
			return record;
		}

		private ImageDifference Compare(string relativeA, string relativeB)
		{
			return ImageCompare.CompareFiles(
				Path.Combine(outputDir, relativeA.Replace('/', Path.DirectorySeparatorChar)),
				Path.Combine(outputDir, relativeB.Replace('/', Path.DirectorySeparatorChar)));
		}

		private static readonly string[] DwParamKeys = new string[]
		{
			"gamma", "enhancedContrast", "clearTypeLevel", "pixelGeometry", "renderingMode", "grayscaleEnhancedContrast"
		};

		private static List<string> DiffDwParams(RenderRecord baseline, RenderRecord render)
		{
			List<string> changes = new List<string>();
			foreach (string key in DwParamKeys)
			{
				string before = baseline.Get("dw.monitor." + key);
				string after = render.Get("dw.monitor." + key);
				if (before == null || after == null)
					continue;
				if (before != after)
					changes.Add(key + " " + before + " -> " + after);
			}
			return changes;
		}
	}
}
