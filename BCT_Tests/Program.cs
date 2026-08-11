using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using BCT_Tests.Settings;

namespace BCT_Tests
{
	internal static class Program
	{
		/// <summary>
		/// Test harness for working out which Windows text-rendering settings affect which
		/// rendering engine.
		///
		///   (no arguments)                         Opens the runner window.
		///   --run [--out DIR] [--allow-partial]    Runs the whole sweep and writes the reports.
		///   --render --out DIR --name NAME         Renders one configuration.  Used internally.
		///   --report-only [--out DIR]              Rewrites the reports from an earlier run's files.
		///   --restore FILE                         Puts a settings-backup.txt file back.
		///   --dwrite-defaults                      Reports what DirectWrite resolves to right now.
		///   --dwrite-defaults --probe [--out DIR]  Sweeps the registry states and reports each one.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			CommandLine cmd = new CommandLine(args);

			if (cmd.Has("render"))
			{
				string outputDir = cmd.Value("out");
				string name = cmd.Value("name");
				if (outputDir == null || name == null)
				{
					Console.Error.WriteLine("--render needs both --out and --name.");
					return 2;
				}
				try
				{
					return ChildRenderProcess.Run(outputDir, name);
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("Render failed: " + ex);
					return 1;
				}
			}

			if (cmd.Has("restore"))
			{
				string file = cmd.Value("restore");
				if (file == null || !File.Exists(file))
				{
					Console.Error.WriteLine("--restore needs the path of a settings-backup.txt file.");
					return 2;
				}
				SettingsBackup backup = SettingsBackup.Load(file);
				List<string> problems = backup.Restore();
				List<string> differences = backup.Verify();
				foreach (string problem in problems)
					Console.Error.WriteLine("restore problem: " + problem);
				foreach (string difference in differences)
					Console.Error.WriteLine("still different: " + difference);
				Console.WriteLine(problems.Count == 0 && differences.Count == 0
					? "Restored and verified."
					: "Restore finished with problems.");
				return problems.Count == 0 && differences.Count == 0 ? 0 : 1;
			}

			if (cmd.Has("dwrite-defaults"))
			{
				// Without --probe this only reads, so it is safe to run at any time and is the
				// quickest way to see what DirectWrite makes of the current settings.
				if (!cmd.Has("probe"))
					return DWriteDefaultsProbe.ReportCurrent(Console.WriteLine);

				int exitCode = DWriteDefaultsProbe.RunSweep(cmd.Value("out") ?? DefaultOutputDirectory, Console.WriteLine);
				if (cmd.Has("pause"))
				{
					Console.WriteLine();
					Console.WriteLine("Press Enter to close.");
					Console.ReadLine();
				}
				return exitCode;
			}

			if (cmd.Has("report-only"))
			{
				string outputDir = cmd.Value("out") ?? cmd.Value("report-only") ?? DefaultOutputDirectory;
				RunResults results = TestRunner.RebuildFromDisk(outputDir, Console.WriteLine);
				if (results.Results.Count == 0)
					return 2;
				WriteReports(results, outputDir, Console.WriteLine);
				return 0;
			}

			if (cmd.Has("run"))
			{
				int exitCode = RunSweep(cmd.Value("out") ?? DefaultOutputDirectory, cmd.Has("allow-partial"),
					cmd.Has("quick"), Console.WriteLine);
				if (cmd.Has("pause"))
				{
					Console.WriteLine();
					Console.WriteLine("Press Enter to close.");
					Console.ReadLine();
				}
				return exitCode;
			}

			Application.Run(new MainTestingForm());
			return 0;
		}

		/// <summary>
		/// Runs the sweep and writes the reports.  Returns 0 only if the machine was verifiably put
		/// back the way it was found.
		/// </summary>
		public const string LogFileName = "run-log.txt";

		public static int RunSweep(string outputDir, bool allowPartial, bool quick, Action<string> consoleLog)
		{
			// An elevated run gets its own console window that disappears with the process, so the
			// log is also kept on disk next to the reports.
			Directory.CreateDirectory(outputDir);
			StreamWriter logFile = new StreamWriter(Path.Combine(outputDir, LogFileName), false) { AutoFlush = true };
			Action<string> log = delegate (string line)
			{
				consoleLog(line);
				logFile.WriteLine(line);
			};

			try
			{
				return RunSweepCore(outputDir, allowPartial, quick, log);
			}
			finally
			{
				logFile.Dispose();
			}
		}

		private static int RunSweepCore(string outputDir, bool allowPartial, bool quick, Action<string> log)
		{
			log("Output directory: " + outputDir);
			log("Running " + (IsElevated ? "elevated" : "WITHOUT elevation (HKEY_LOCAL_MACHINE values cannot be changed)"));
			log("");

			TestRunner runner = new TestRunner(outputDir, log, allowPartial, quick);
			RunResults results = runner.Run();

			if (results.Results.Count == 0)
			{
				log("No measurements were taken.");
				return 2;
			}

			WriteReports(results, outputDir, log);

			return results.RestoreProblems.Count == 0 && results.RestoreDifferences.Count == 0 ? 0 : 1;
		}

		private static void WriteReports(RunResults results, string outputDir, Action<string> log)
		{
			string markdownPath = Path.Combine(outputDir, ReportWriter.MarkdownFileName);
			string htmlPath = Path.Combine(outputDir, ReportWriter.HtmlFileName);
			ReportWriter.WriteMarkdown(results, markdownPath);
			ReportWriter.WriteHtml(results, htmlPath);

			log("");
			log("Wrote " + markdownPath);
			log("Wrote " + htmlPath);
			log("");
			log(Summarize(results));
		}

		private static string Summarize(RunResults results)
		{
			List<string> lines = new List<string>();
			lines.Add("Summary (settings that changed the rendered pixels):");
			foreach (SettingUnderTest setting in results.Settings)
			{
				List<string> where = new List<string>();
				bool tested = false;
				foreach (BaseState state in results.States)
				{
					SettingResult result = results.Find(state, setting);
					if (result == null || !result.Tested)
						continue;
					tested = true;
					if (result.GdiAffected)
						where.Add("GDI/" + state.Label);
					if (result.DwAppAffected)
						where.Add("DWapp/" + state.Label);
					if (result.DwAffected)
						where.Add("DWraw/" + state.Label);
				}
				string label = (setting.Category == SettingCategory.AvalonCurrentUser ? "HKCU "
					: setting.Category == SettingCategory.AvalonLocalMachine ? "HKLM " : "SPI  ") + setting.Name;
				lines.Add("  " + label.PadRight(40) + " "
					+ (!tested ? "not tested" : where.Count == 0 ? "no effect anywhere" : string.Join(", ", where)));
			}
			return string.Join(Environment.NewLine, lines);
		}

		public static bool IsElevated
		{
			get
			{
				try
				{
					using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
						return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		/// <summary>
		/// The results land next to the project rather than in bin\, so a rebuild does not wipe
		/// them and they are easy to find.
		/// </summary>
		public static string DefaultOutputDirectory
		{
			get
			{
				string exeDir = Path.GetDirectoryName(Application.ExecutablePath);
				string projectDir = Path.GetFullPath(Path.Combine(exeDir, "..", ".."));
				if (File.Exists(Path.Combine(projectDir, "BCT_Tests.csproj")))
					return Path.Combine(projectDir, "Results");
				return Path.Combine(exeDir, "Results");
			}
		}

		/// <summary>
		/// Relaunches this program elevated to run the sweep, because clearing and setting the
		/// HKEY_LOCAL_MACHINE values needs administrator rights.
		/// </summary>
		public static bool RelaunchElevated(string outputDir, out string error)
		{
			error = null;
			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo(Application.ExecutablePath,
					"--run --out \"" + outputDir + "\" --pause")
				{
					UseShellExecute = true,
					Verb = "runas"
				};
				Process.Start(startInfo);
				return true;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				return false;
			}
		}

		private class CommandLine
		{
			private readonly Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			public CommandLine(string[] args)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (!args[i].StartsWith("--"))
						continue;
					string name = args[i].Substring(2);
					string value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : null;
					options[name] = value;
				}
			}

			public bool Has(string name)
			{
				return options.ContainsKey(name);
			}

			public string Value(string name)
			{
				string value;
				return options.TryGetValue(name, out value) ? value : null;
			}
		}
	}
}
