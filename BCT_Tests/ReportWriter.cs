using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BCT_Tests.Settings;

namespace BCT_Tests
{
	/// <summary>
	/// Turns the measurements into the compatibility matrix, once as GitHub-flavored markdown and
	/// once as a standalone HTML page that also shows the rendered samples.
	/// </summary>
	internal static class ReportWriter
	{
		public const string MarkdownFileName = "CompatibilityMatrix.md";
		public const string HtmlFileName = "CompatibilityMatrix.html";

		private const string Affected = "✅";
		private const string Unaffected = "—";
		private const string NotTested = "n/t";

		#region Shared text

		private static string Title { get { return "Windows text rendering: which settings affect which renderer"; } }

		private static string EnvironmentLine()
		{
			return OsDescription() + " · "
				+ (Environment.Is64BitProcess ? "64-bit" : "32-bit") + " test process · displays: "
				+ string.Join(", ", AvalonRegistry.DisplayNames) + " · CLR " + Environment.Version;
		}

		/// <summary>
		/// The real Windows version.  Environment.OSVersion reports 6.2 for an application without
		/// a compatibility manifest, which would put the wrong OS on the report.
		/// </summary>
		private static string OsDescription()
		{
			try
			{
				using (Microsoft.Win32.RegistryKey root = Microsoft.Win32.RegistryKey.OpenBaseKey(
					Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64))
				using (Microsoft.Win32.RegistryKey key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false))
				{
					string product = key.GetValue("ProductName") as string;
					string displayVersion = key.GetValue("DisplayVersion") as string;
					string build = Convert.ToString(key.GetValue("CurrentBuild"), CultureInfo.InvariantCulture);
					object updateBuildRevision = key.GetValue("UBR");

					int buildNumber;
					// ProductName was never updated for Windows 11 and still reads "Windows 10".
					if (product != null && product.Contains("Windows 10")
						&& int.TryParse(build, out buildNumber) && buildNumber >= 22000)
						product = product.Replace("Windows 10", "Windows 11");

					return product + (string.IsNullOrEmpty(displayVersion) ? "" : " " + displayVersion)
						+ " (build " + build + (updateBuildRevision == null ? "" : "." + updateBuildRevision) + ")";
				}
			}
			catch (Exception)
			{
				return "Windows " + Environment.OSVersion.Version;
			}
		}

		private static string[] MethodNotes(RunResults results)
		{
			return new string[]
			{
				"Each row is measured by clearing **every** Avalon.Graphics value under both hives on every display, "
					+ "setting **one** value, rendering, and comparing the result pixel for pixel against the same render "
					+ "taken with no Avalon.Graphics values present at all.",
				"Every render happens in a freshly launched process. DirectWrite resolves its default rendering "
					+ "parameters when a factory is created, and real applications pick these settings up at startup, so a "
					+ "new process per configuration is both the safe way to avoid a stale cache and a faithful model of "
					+ "what an application sees.",
				"The GDI sample is drawn with `CreateFontIndirect` + `TextOut` on a 32-bit DIB, with `DEFAULT_QUALITY` so "
					+ "that GDI consults the system font-smoothing settings rather than being told what to do.",
				"**The two DirectWrite columns are not the same experiment.** "
					+ "`IDWriteFactory::CreateMonitorRenderingParams` picks up the Avalon.Graphics registry values and "
					+ "`SPI_SETFONTSMOOTHINGORIENTATION` — but **not** `SPI_SETFONTSMOOTHING` (the antialiasing on/off "
					+ "switch) and **not** `SPI_SETFONTSMOOTHINGTYPE` (grayscale versus ClearType). It reports "
					+ "`clearTypeLevel = 1` and `renderingMode = DEFAULT` even with antialiasing switched off system-wide. "
					+ "**DirectWrite (raw defaults)** hands exactly those parameters to "
					+ "`IDWriteBitmapRenderTarget::DrawGlyphRun` and changes nothing, so it antialiases in every mode.",
				"**DirectWrite (as applications use it)** does what a real client has to do: it keeps DirectWrite's tuning "
					+ "parameters — gamma, enhanced contrast, ClearType level, pixel geometry — but reads the system "
					+ "font-smoothing state itself and picks the rendering mode from it, because DirectWrite will not. "
					+ "Antialiasing off becomes `DWRITE_RENDERING_MODE_ALIASED`; grayscale becomes antialiased with the "
					+ "ClearType level forced to 0. This is the same policy this repository's own "
					+ "`DirectWriteSampleRenderer` applies, and it is the column that corresponds to what you see in "
					+ "Firefox, WPF and the tuner's own preview.",
				"That split is the point of the table. A ✅ in the raw-defaults column means DirectWrite itself consumed "
					+ "the value; a ✅ only in the application column means the application had to act on it. It also "
					+ "explains why an Avalon.Graphics value can look dead in the application column while working in the "
					+ "raw column: once the application has committed to aliased or grayscale output, the ClearType tuning "
					+ "it would have fed no longer reaches the glyphs.",
				"The whole sweep is run once inside each of the " + results.States.Count + " system font-smoothing "
					+ "configurations that form the columns, because a setting can matter in one mode and be "
					+ "irrelevant in another.",
				"Both DirectWrite columns use the `IDWriteBitmapRenderTarget` path. A client drawing through Direct2D or "
					+ "`IDWriteGlyphRunAnalysis` sets its antialias mode through a different API and can differ in detail, "
					+ "though it faces the same split: the mode is the application's decision, the tuning is DirectWrite's.",
				"A repeat of each baseline render is compared against the original to confirm the renders are "
					+ "reproducible; " + (results.RendersAreDeterministic
						? "every repeat came back byte-identical, so any difference reported below is real."
						: "**at least one repeat differed, so these results are not trustworthy.**"),
				results.RenderCount + " renders were taken in " + results.Duration.TotalMinutes.ToString("0.0") + " minutes."
			};
		}

		/// <summary>The three renderers each column group reports on.</summary>
		private enum Renderer
		{
			Gdi,
			DirectWriteDefaults,
			DirectWriteAppConfigured
		}

		private static readonly Renderer[] Renderers = new Renderer[]
		{
			Renderer.Gdi, Renderer.DirectWriteAppConfigured, Renderer.DirectWriteDefaults
		};

		private static string RendererLabel(Renderer renderer)
		{
			switch (renderer)
			{
				case Renderer.Gdi: return "GDI";
				case Renderer.DirectWriteAppConfigured: return "DirectWrite (as applications use it)";
				default: return "DirectWrite (raw defaults)";
			}
		}

		/// <summary>
		/// The base state's label for the matrix, where one column per state means the full label
		/// makes the table much wider than the readable width.  A newline is a line break.
		/// </summary>
		private static string MatrixLabel(BaseState state)
		{
			return string.IsNullOrEmpty(state.ShortLabel) ? state.Label : state.ShortLabel;
		}

		private static bool Affects(SettingResult result, Renderer renderer)
		{
			switch (renderer)
			{
				case Renderer.Gdi: return result.GdiAffected;
				case Renderer.DirectWriteAppConfigured: return result.DwAppAffected;
				default: return result.DwAffected;
			}
		}

		private static Rendering.ImageDifference DiffFor(ValueResult value, Renderer renderer)
		{
			switch (renderer)
			{
				case Renderer.Gdi: return value.GdiDiff;
				case Renderer.DirectWriteAppConfigured: return value.DwAppDiff;
				default: return value.DwDiff;
			}
		}

		private static Rendering.ImageDifference DiffFor(BaseStateComparison comparison, Renderer renderer)
		{
			switch (renderer)
			{
				case Renderer.Gdi: return comparison.GdiDiff;
				case Renderer.DirectWriteAppConfigured: return comparison.DwAppDiff;
				default: return comparison.DwDiff;
			}
		}

		private static string ImageFor(RenderRecord record, Renderer renderer)
		{
			switch (renderer)
			{
				case Renderer.Gdi: return record.GdiImage;
				case Renderer.DirectWriteAppConfigured: return record.DwAppImage;
				default: return record.DwImage;
			}
		}

		private static string Verdict(SettingResult result, Renderer renderer)
		{
			if (result == null)
				return NotTested;
			if (!result.Tested)
				return NotTested;
			if (!Affects(result, renderer))
				return Unaffected;
			double worst = 0;
			foreach (ValueResult value in result.Values)
			{
				ImageDifferenceHolder holder = new ImageDifferenceHolder(DiffFor(value, renderer));
				if (holder.Percent > worst)
					worst = holder.Percent;
			}
			return Affected + " " + worst.ToString("0.#") + "%";
		}

		/// <summary>Small shim so a null difference reads as zero instead of throwing.</summary>
		private struct ImageDifferenceHolder
		{
			public readonly double Percent;
			public ImageDifferenceHolder(Rendering.ImageDifference diff)
			{
				Percent = diff == null ? 0 : diff.PercentDifferent;
			}
		}

		private static string DiffCell(Rendering.ImageDifference diff)
		{
			if (diff == null || diff.Error != null)
				return NotTested;
			return diff.AnyDifference
				? Affected + " " + diff.PercentDifferent.ToString("0.#") + "%"
				: Unaffected + " identical";
		}

		private static string LocationLabel(SettingUnderTest setting)
		{
			switch (setting.Category)
			{
				case SettingCategory.AvalonCurrentUser: return @"HKCU\...\Avalon.Graphics\<display>";
				case SettingCategory.AvalonLocalMachine: return @"HKLM\...\Avalon.Graphics\<display>";
				default: return "SystemParametersInfo";
			}
		}

		/// <summary>
		/// The location as a markdown code span.  It has to be code: "&lt;display&gt;" would
		/// otherwise be swallowed as an HTML tag when GitHub renders the file.
		/// </summary>
		private static string MarkdownLocation(SettingUnderTest setting)
		{
			return "`" + LocationLabel(setting) + "`";
		}

		/// <summary>
		/// Which DirectWrite rendering parameters a setting was observed to move, aggregated over
		/// every base state.  This is read straight back out of IDWriteRenderingParams, so it says
		/// what a value feeds rather than what it looks like it feeds.
		/// </summary>
		private static List<string> DwParametersTouched(RunResults results, SettingUnderTest setting)
		{
			List<string> names = new List<string>();
			foreach (SettingResult result in results.Results.Where(r => r.Setting.Id == setting.Id))
				foreach (string name in result.DwParametersTouched)
					if (!names.Contains(name))
						names.Add(name);
			return names;
		}

		private static bool AnyEffect(RunResults results, SettingUnderTest setting)
		{
			return results.Results.Any(r => r.Setting.Id == setting.Id && r.Tested
				&& (r.GdiAffected || r.DwAffected || r.DwAppAffected));
		}

		#endregion

		#region Markdown

		public static void WriteMarkdown(RunResults results, string path)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("# " + Title);
			sb.AppendLine();
			sb.AppendLine("Measured on " + results.Started.ToString("yyyy-MM-dd HH:mm") + " by the `BCT_Tests` harness in this repository.");
			sb.AppendLine();
			sb.AppendLine(EnvironmentLine());
			sb.AppendLine();

			if (results.Warnings.Count > 0)
			{
				sb.AppendLine("> [!WARNING]");
				foreach (string warning in results.Warnings.Distinct())
					sb.AppendLine("> " + warning);
				sb.AppendLine();
			}

			sb.AppendLine("## How this was measured");
			sb.AppendLine();
			foreach (string note in MethodNotes(results))
				sb.AppendLine("* " + note);
			sb.AppendLine();

			sb.AppendLine("## The matrix");
			sb.AppendLine();
			sb.AppendLine("Legend: " + Affected + " the setting changes the rendered pixels (with the largest share of pixels "
				+ "it changed) · " + Unaffected + " byte-identical output · " + NotTested + " not tested.");
			sb.AppendLine();

			sb.Append("| Where | Setting |");
			foreach (Renderer renderer in Renderers)
				foreach (BaseState state in results.States)
					sb.Append(" " + RendererLabel(renderer) + "<br>" + MatrixLabel(state).Replace("\n", "<br>") + " |");
			sb.AppendLine();
			sb.Append("| --- | --- |");
			for (int i = 0; i < results.States.Count * Renderers.Length; i++)
				sb.Append(" :---: |");
			sb.AppendLine();

			foreach (SettingUnderTest setting in results.Settings)
			{
				sb.Append("| " + MarkdownLocation(setting) + " | `" + setting.Name + "` |");
				foreach (Renderer renderer in Renderers)
					foreach (BaseState state in results.States)
						sb.Append(" " + Verdict(results.Find(state, setting), renderer) + " |");
				sb.AppendLine();
			}
			sb.AppendLine();

			if (results.BaseStateComparisons.Count > 0)
			{
				sb.AppendLine("## Does the system font-smoothing mode reach each renderer?");
				sb.AppendLine();
				sb.AppendLine("The sweep above only ever compares renders taken *within* one column. This table compares the "
					+ "columns against each other, using the baseline render of each base state with no Avalon.Graphics "
					+ "values present at all.");
				sb.AppendLine();
				sb.Append("| System font smoothing |");
				foreach (Renderer renderer in Renderers)
					sb.Append(" " + RendererLabel(renderer) + " |");
				sb.AppendLine();
				sb.Append("| --- |");
				for (int i = 0; i < Renderers.Length; i++)
					sb.Append(" --- |");
				sb.AppendLine();
				foreach (BaseStateComparison comparison in results.BaseStateComparisons)
				{
					sb.Append("| " + comparison.State.Label + " |");
					foreach (Renderer renderer in Renderers)
						sb.Append(" " + DiffCell(DiffFor(comparison, renderer)) + " |");
					sb.AppendLine();
				}
				sb.AppendLine();
			}

			sb.AppendLine("## What each setting actually feeds");
			sb.AppendLine();
			sb.AppendLine("The DirectWrite column below is read back from `IDWriteRenderingParams` after the value was "
				+ "written, so it names the parameter the value lands in rather than inferring it from the picture.");
			sb.AppendLine();
			sb.AppendLine("| Where | Setting | Documented as | DirectWrite parameters it moved | Any effect on rendering |");
			sb.AppendLine("| --- | --- | --- | --- | --- |");
			foreach (SettingUnderTest setting in results.Settings)
			{
				List<string> touched = DwParametersTouched(results, setting);
				bool tested = results.Results.Any(r => r.Setting.Id == setting.Id && r.Tested);
				sb.AppendLine("| " + MarkdownLocation(setting) + " | `" + setting.Name + "` | " + setting.Documentation
					+ " | " + (touched.Count == 0 ? "none" : "`" + string.Join("`, `", touched) + "`")
					+ " | " + (!tested ? NotTested : AnyEffect(results, setting) ? "yes" : "no") + " |");
			}
			sb.AppendLine();

			sb.AppendLine("## Baseline renders");
			sb.AppendLine();
			sb.AppendLine("With no Avalon.Graphics values present at all, so only the system font-smoothing settings apply.");
			sb.AppendLine();
			foreach (BaseState state in results.States)
			{
				RenderRecord baseline;
				if (!results.Baselines.TryGetValue(state.Id, out baseline))
					continue;
				sb.AppendLine("### " + state.Label);
				sb.AppendLine();
				sb.AppendLine("DirectWrite reported " + DwParamsSummary(baseline) + ".");
				sb.AppendLine();
				foreach (Renderer renderer in Renderers)
				{
					sb.AppendLine(RendererLabel(renderer) + ":");
					sb.AppendLine();
					sb.AppendLine("![" + RendererLabel(renderer) + ", " + state.Label + "](" + ImageFor(baseline, renderer) + ")");
					sb.AppendLine();
				}
			}

			sb.AppendLine("## Per-setting detail");
			sb.AppendLine();
			foreach (BaseState state in results.States)
			{
				sb.AppendLine("### Base state: " + state.Label);
				sb.AppendLine();
				foreach (SettingUnderTest setting in results.Settings)
				{
					SettingResult result = results.Find(state, setting);
					if (result == null)
						continue;
					sb.AppendLine("#### `" + setting.Name + "` — " + MarkdownLocation(setting));
					sb.AppendLine();
					if (!result.Tested)
					{
						sb.AppendLine("_" + result.NotTestedReason + "._");
						sb.AppendLine();
						continue;
					}
					sb.AppendLine("Compared against: " + setting.BaselineLabel + ".");
					sb.AppendLine();
					sb.AppendLine("| Value | GDI vs baseline | DirectWrite (as applications use it) vs baseline "
						+ "| DirectWrite (raw defaults) vs baseline | DirectWrite parameters | Images |");
					sb.AppendLine("| --- | --- | --- | --- | --- | --- |");
					foreach (ValueResult value in result.Values)
					{
						sb.AppendLine("| " + value.Value.Label
							+ " | " + (value.GdiDiff == null ? "n/a" : value.GdiDiff.ToString())
							+ " | " + (value.DwAppDiff == null ? "n/a" : value.DwAppDiff.ToString())
							+ " | " + (value.DwDiff == null ? "n/a" : value.DwDiff.ToString())
							+ " | " + (value.DwParamChanges.Count == 0 ? "unchanged" : string.Join("; ", value.DwParamChanges))
							+ " | [GDI](" + value.Render.GdiImage + ") · [DW app](" + value.Render.DwAppImage
							+ ") · [DW raw](" + value.Render.DwImage + ") |");
					}
					sb.AppendLine();
				}
			}

			sb.AppendLine("## Restoring the machine");
			sb.AppendLine();
			sb.AppendLine("The harness snapshots every value it is about to touch, including whether the key or value "
				+ "existed at all, and puts it all back when the run finishes.");
			sb.AppendLine();
			sb.AppendLine("```");
			sb.AppendLine(results.BackupDescription.TrimEnd());
			sb.AppendLine("```");
			sb.AppendLine();
			if (results.RestoreProblems.Count == 0 && results.RestoreDifferences.Count == 0)
				sb.AppendLine("After the run, every captured value was verified to be back at its original state.");
			else
			{
				sb.AppendLine("**Restore did not come back clean:**");
				sb.AppendLine();
				foreach (string problem in results.RestoreProblems)
					sb.AppendLine("* " + problem);
				foreach (string difference in results.RestoreDifferences)
					sb.AppendLine("* " + difference);
			}
			sb.AppendLine();

			File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
		}

		private static string DwParamsSummary(RenderRecord record)
		{
			if (record == null || record.Get("dw.monitor.gamma") == null)
				return "no parameters";
			return "gamma " + record.Get("dw.monitor.gamma")
				+ ", enhanced contrast " + record.Get("dw.monitor.enhancedContrast")
				+ ", grayscale enhanced contrast " + record.Get("dw.monitor.grayscaleEnhancedContrast")
				+ ", ClearType level " + record.Get("dw.monitor.clearTypeLevel")
				+ ", pixel geometry " + record.Get("dw.monitor.pixelGeometry")
				+ ", rendering mode " + record.Get("dw.monitor.renderingMode");
		}

		#endregion

		#region HTML

		public static void WriteHtml(RunResults results, string path)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<!DOCTYPE html>");
			sb.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\">");
			sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
			sb.AppendLine("<title>" + Escape(Title) + "</title>");
			sb.AppendLine("<style>" + Css() + "</style>");
			sb.AppendLine("</head><body>");

			// Every part of the page is its own <section> so that a sticky heading stops sticking when
			// its own part scrolls away, instead of hanging over the part that follows it.
			sb.AppendLine("<section>");
			sb.AppendLine("<h1>" + Escape(Title) + "</h1>");
			sb.AppendLine("<p class=\"meta\">Measured on " + Escape(results.Started.ToString("yyyy-MM-dd HH:mm"))
				+ " by the <code>BCT_Tests</code> harness.<br>" + Escape(EnvironmentLine()) + "</p>");

			if (results.Warnings.Count > 0)
			{
				sb.AppendLine("<div class=\"warn\"><strong>Warnings</strong><ul>");
				foreach (string warning in results.Warnings.Distinct())
					sb.AppendLine("<li>" + Escape(warning) + "</li>");
				sb.AppendLine("</ul></div>");
			}

			sb.AppendLine(Section("How this was measured") + "<ul>");
			foreach (string note in MethodNotes(results))
				sb.AppendLine("<li>" + Inline(note) + "</li>");
			sb.AppendLine("</ul>");

			sb.AppendLine(Section("The matrix"));
			sb.AppendLine("<p class=\"legend\"><span class=\"yes\">" + Affected + "</span> changes the rendered pixels "
				+ "(with the largest share of pixels it changed) &middot; <span class=\"no\">" + Unaffected
				+ "</span> byte-identical output &middot; <span class=\"nt\">" + NotTested + "</span> not tested.</p>");

			sb.AppendLine("<div class=\"tablewrap\"><table class=\"matrix\"><thead><tr><th rowspan=\"2\">Where</th><th rowspan=\"2\">Setting</th>");
			foreach (Renderer renderer in Renderers)
				sb.Append("<th colspan=\"" + results.States.Count + "\">" + Escape(RendererLabel(renderer)) + "</th>");
			sb.AppendLine("</tr><tr>");
			foreach (Renderer renderer in Renderers)
				foreach (BaseState state in results.States)
					sb.Append("<th>" + Escape(MatrixLabel(state)).Replace("\n", "<br>") + "</th>");
			sb.AppendLine("</tr></thead><tbody>");
			foreach (SettingUnderTest setting in results.Settings)
			{
				sb.Append("<tr><td class=\"where\">" + Escape(LocationLabel(setting)) + "</td><td><code>" + Escape(setting.Name) + "</code></td>");
				foreach (Renderer renderer in Renderers)
					foreach (BaseState state in results.States)
						sb.Append(VerdictCell(results.Find(state, setting), renderer));
				sb.AppendLine("</tr>");
			}
			sb.AppendLine("</tbody></table></div>");

			if (results.BaseStateComparisons.Count > 0)
			{
				sb.AppendLine(Section("Does the system font-smoothing mode reach each renderer?"));
				sb.AppendLine("<p>The sweep above only ever compares renders taken <em>within</em> one column. This table "
					+ "compares the columns against each other, using the baseline render of each base state with no "
					+ "Avalon.Graphics values present at all.</p>");
				sb.Append("<div class=\"tablewrap\"><table><thead><tr><th>System font smoothing</th>");
				foreach (Renderer renderer in Renderers)
					sb.Append("<th>" + Escape(RendererLabel(renderer)) + "</th>");
				sb.AppendLine("</tr></thead><tbody>");
				foreach (BaseStateComparison comparison in results.BaseStateComparisons)
				{
					sb.Append("<tr><td>" + Escape(comparison.State.Label) + "</td>");
					foreach (Renderer renderer in Renderers)
						sb.Append(DiffTableCell(DiffFor(comparison, renderer)));
					sb.AppendLine("</tr>");
				}
				sb.AppendLine("</tbody></table></div>");
			}

			sb.AppendLine(Section("What each setting actually feeds"));
			sb.AppendLine("<p>The DirectWrite column is read back from <code>IDWriteRenderingParams</code> after the value "
				+ "was written, so it names the parameter the value lands in rather than inferring it from the picture.</p>");
			sb.AppendLine("<div class=\"tablewrap\"><table><thead><tr><th>Where</th><th>Setting</th><th>Documented as</th>"
				+ "<th>DirectWrite parameters it moved</th><th>Any effect</th></tr></thead><tbody>");
			foreach (SettingUnderTest setting in results.Settings)
			{
				List<string> touched = DwParametersTouched(results, setting);
				bool tested = results.Results.Any(r => r.Setting.Id == setting.Id && r.Tested);
				sb.AppendLine("<tr><td class=\"where\">" + Escape(LocationLabel(setting)) + "</td><td><code>"
					+ Escape(setting.Name) + "</code></td><td class=\"doc\">" + Escape(setting.Documentation) + "</td><td>"
					+ (touched.Count == 0 ? "<span class=\"no\">none</span>" : "<code>" + Escape(string.Join(", ", touched)) + "</code>")
					+ "</td><td>" + (!tested ? "<span class=\"nt\">" + NotTested + "</span>"
						: AnyEffect(results, setting) ? "<span class=\"yes\">yes</span>" : "<span class=\"no\">no</span>")
					+ "</td></tr>");
			}
			sb.AppendLine("</tbody></table></div>");

			sb.AppendLine(Section("Baseline renders"));
			sb.AppendLine("<p>With no Avalon.Graphics values present at all, so only the system font-smoothing settings apply.</p>");
			sb.AppendLine("<p class=\"zoomtoggle\"><label><input type=\"checkbox\" id=\"zoom\"> Magnify samples 3&times; (nearest neighbour)</label></p>");
			sb.AppendLine("<div class=\"samples\">");
			foreach (BaseState state in results.States)
			{
				RenderRecord baseline;
				if (!results.Baselines.TryGetValue(state.Id, out baseline))
					continue;
				sb.AppendLine("<div class=\"sample\"><h3>" + Escape(state.Label) + "</h3>");
				sb.AppendLine("<p class=\"params\">" + Escape(DwParamsSummary(baseline)) + "</p>");
				foreach (Renderer renderer in Renderers)
					sb.AppendLine("<figure><figcaption>" + Escape(RendererLabel(renderer)) + "</figcaption><img src=\""
						+ Escape(ImageFor(baseline, renderer)) + "\" alt=\"sample\"></figure>");
				sb.AppendLine("</div>");
			}
			sb.AppendLine("</div>");

			sb.AppendLine(Section("Per-setting detail"));
			foreach (BaseState state in results.States)
			{
				sb.AppendLine("<section class=\"basestate\"><h3>Base state: " + Escape(state.Label) + "</h3>");
				foreach (SettingUnderTest setting in results.Settings)
				{
					SettingResult result = results.Find(state, setting);
					if (result == null)
						continue;
					bool affected = result.Tested && (result.GdiAffected || result.DwAffected || result.DwAppAffected);
					sb.AppendLine("<details" + (affected ? " open" : "") + "><summary><code>" + Escape(setting.Name)
						+ "</code> <span class=\"where\">" + Escape(LocationLabel(setting)) + "</span> "
						+ (result.Tested
							? "<span class=\"" + (result.GdiAffected ? "yes" : "no") + "\">GDI " + (result.GdiAffected ? "affected" : "unchanged") + "</span> "
								+ "<span class=\"" + (result.DwAppAffected ? "yes" : "no") + "\">DW app " + (result.DwAppAffected ? "affected" : "unchanged") + "</span> "
								+ "<span class=\"" + (result.DwAffected ? "yes" : "no") + "\">DW raw " + (result.DwAffected ? "affected" : "unchanged") + "</span>"
							: "<span class=\"nt\">" + NotTested + "</span>")
						+ "</summary>");
					if (!result.Tested)
					{
						sb.AppendLine("<p>" + Escape(result.NotTestedReason) + "</p></details>");
						continue;
					}
					sb.AppendLine("<p>Compared against: " + Escape(setting.BaselineLabel) + ".</p>");
					sb.AppendLine("<div class=\"tablewrap\"><table><thead><tr><th>Value</th><th>GDI vs baseline</th>"
						+ "<th>DirectWrite (as applications use it) vs baseline</th><th>DirectWrite (raw defaults) vs baseline</th>"
						+ "<th>DirectWrite parameters</th></tr></thead><tbody>");
					foreach (ValueResult value in result.Values)
					{
						sb.AppendLine("<tr><td>" + Escape(value.Value.Label) + "</td><td>"
							+ Escape(value.GdiDiff == null ? "n/a" : value.GdiDiff.ToString()) + "</td><td>"
							+ Escape(value.DwAppDiff == null ? "n/a" : value.DwAppDiff.ToString()) + "</td><td>"
							+ Escape(value.DwDiff == null ? "n/a" : value.DwDiff.ToString()) + "</td><td>"
							+ (value.DwParamChanges.Count == 0 ? "unchanged" : Escape(string.Join("; ", value.DwParamChanges)))
							+ "</td></tr>");
					}
					sb.AppendLine("</tbody></table></div>");
					sb.AppendLine("<div class=\"samples\">");
					foreach (ValueResult value in result.Values)
					{
						sb.AppendLine("<div class=\"sample\"><h4>" + Escape(setting.Name) + " = " + Escape(value.Value.Label) + "</h4>");
						foreach (Renderer renderer in Renderers)
							sb.AppendLine("<figure><figcaption>" + Escape(RendererLabel(renderer)) + "</figcaption><img loading=\"lazy\" src=\""
								+ Escape(ImageFor(value.Render, renderer)) + "\" alt=\"sample\"></figure>");
						sb.AppendLine("</div>");
					}
					sb.AppendLine("</div></details>");
				}
				sb.AppendLine("</section>");
			}

			sb.AppendLine(Section("Restoring the machine"));
			sb.AppendLine("<p>The harness snapshots every value it is about to touch, including whether the key or value "
				+ "existed at all, and puts it all back when the run finishes.</p>");
			sb.AppendLine("<pre>" + Escape(results.BackupDescription.TrimEnd()) + "</pre>");
			if (results.RestoreProblems.Count == 0 && results.RestoreDifferences.Count == 0)
				sb.AppendLine("<p class=\"ok\">After the run, every captured value was verified to be back at its original state.</p>");
			else
			{
				sb.AppendLine("<div class=\"warn\"><strong>Restore did not come back clean</strong><ul>");
				foreach (string problem in results.RestoreProblems)
					sb.AppendLine("<li>" + Escape(problem) + "</li>");
				foreach (string difference in results.RestoreDifferences)
					sb.AppendLine("<li>" + Escape(difference) + "</li>");
				sb.AppendLine("</ul></div>");
			}
			sb.AppendLine("</section>");

			sb.AppendLine("<script>");
			sb.AppendLine("document.getElementById('zoom').addEventListener('change', function () {");
			sb.AppendLine("  document.body.classList.toggle('zoomed', this.checked);");
			sb.AppendLine("});");
			sb.AppendLine("</script>");
			sb.AppendLine("</body></html>");

			File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
		}

		/// <summary>
		/// Ends the part of the page that was open and starts the next one with its heading.  Callers
		/// emit the opening &lt;section&gt; of the first part and the closing tag of the last one.
		/// </summary>
		private static string Section(string heading)
		{
			return "</section>\r\n<section><h2>" + Escape(heading) + "</h2>";
		}

		private static string DiffTableCell(Rendering.ImageDifference diff)
		{
			string text = DiffCell(diff);
			string cls = text == NotTested ? "nt" : text.StartsWith(Unaffected) ? "no" : "yes";
			return "<td class=\"" + cls + "\">" + Escape(text) + "</td>";
		}

		private static string VerdictCell(SettingResult result, Renderer renderer)
		{
			string text = Verdict(result, renderer);
			string cls = text == NotTested ? "nt" : text == Unaffected ? "no" : "yes";
			return "<td class=\"" + cls + "\">" + Escape(text) + "</td>";
		}

		private static string Css()
		{
			return @"
/* The heading heights are declared rather than measured, because each sticky level parks
   directly below the one above it and has to know how tall that one is. */
:root { color-scheme: light dark; --page-bg: #fff; --h2-height: 44px; --h3-height: 34px; }
body { font-family: 'Segoe UI', system-ui, sans-serif; line-height: 1.5; margin: 0 auto; padding: 24px; max-width: 1200px;
       background: var(--page-bg); color: #16181d; }
h1 { font-size: 1.7rem; margin-bottom: 4px; }
h2 { position: sticky; top: 0; z-index: 30; background: var(--page-bg);
     font-size: 1.3rem; line-height: 1.4; margin: 2.2rem 0 0.6rem; padding: 8px 0 5px;
     border-bottom: 1px solid #d8dce3; }
h3 { position: sticky; top: var(--h2-height); z-index: 20; background: var(--page-bg);
     font-size: 1.05rem; line-height: 1.4; margin: 1.6rem 0 0.5rem; padding: 5px 0; }
code { font-family: Consolas, 'Cascadia Mono', monospace; font-size: 0.92em; background: #f2f4f7; padding: 1px 4px; border-radius: 3px; }
p.meta { color: #5b6270; margin-top: 0; }
/* Wide tables scroll inside their own box; the page itself must never scroll sideways. */
.tablewrap { overflow-x: auto; margin: 12px 0; }
.tablewrap table { margin: 0; }
table { border-collapse: collapse; margin: 12px 0; width: 100%; }
td code { white-space: normal; overflow-wrap: anywhere; }
th, td { border: 1px solid #d8dce3; padding: 5px 9px; text-align: left; vertical-align: top; font-size: 0.94rem; }
thead th { background: #f2f4f7; text-align: center; }
table.matrix td:nth-child(n+3) { text-align: center; white-space: nowrap; }
/* The matrix is the widest table on the page and the browser squeezes its first two columns
   hardest to make room, so it gets tighter cells than the rest and a setting name is allowed to
   wrap between words but never inside one: a column reading ClearT / ypeLev / el saves nothing
   worth having.  The column headings carry their own line breaks and never need more. */
table.matrix thead th { white-space: nowrap; }
table.matrix td code { overflow-wrap: normal; }
table.matrix th, table.matrix td { padding: 5px 6px; }
td.where, span.where { color: #5b6270; font-size: 0.86rem; font-family: Consolas, monospace; }
td.doc { color: #43485a; font-size: 0.86rem; }
.yes { color: #0a7d33; font-weight: 600; }
.no { color: #8a9099; }
.nt { color: #9a6a00; }
td.yes { background: #e9f7ec; }
td.no  { background: #fafbfc; }
td.nt  { background: #fdf6e5; }
.warn { border-left: 4px solid #d08700; background: #fdf6e5; padding: 8px 14px; margin: 14px 0; border-radius: 3px; }
.ok { color: #0a7d33; }
.legend, .params { color: #5b6270; font-size: 0.9rem; }
details { border: 1px solid #e3e6eb; border-radius: 4px; padding: 6px 12px; margin: 8px 0; }
/* The negative margins pull the sticky summary out over the box's own padding, so nothing
   scrolls through the gap between it and the border. */
summary { cursor: pointer; position: sticky; top: calc(var(--h2-height) + var(--h3-height)); z-index: 10;
          background: var(--page-bg); margin: -6px -12px 6px; padding: 6px 12px; border-radius: 3px 3px 0 0; }
summary code { font-weight: 600; }
.samples { display: flex; flex-wrap: wrap; gap: 18px; }
.sample { border: 1px solid #e3e6eb; border-radius: 4px; padding: 8px 12px; background: #fbfcfd; }
/* A sample's own caption is part of the card, not a section heading, so it never sticks. */
.sample h3, .sample h4 { position: static; margin: 0 0 6px; padding: 0; font-size: 0.95rem; }
figure { margin: 6px 0; }
figcaption { font-size: 0.8rem; color: #5b6270; }
/* The samples are pixel data: never let the browser smooth them, and magnify by whole pixels. */
.samples img { display: block; background: #fff; border: 1px solid #e3e6eb; image-rendering: pixelated; }
body.zoomed .samples img { width: 1140px; max-width: 100%; }
pre { background: #f2f4f7; padding: 10px 12px; border-radius: 4px; overflow-x: auto; font-size: 0.86rem; }
@media (prefers-color-scheme: dark) {
  :root { --page-bg: #14161a; }
  body { color: #e6e8ec; }
  h2 { border-bottom-color: #2c3038; }
  code, pre, thead th { background: #1e2128; }
  th, td, details, .sample, .samples img { border-color: #2c3038; }
  .sample { background: #181b21; }
  td.yes { background: #14251a; }
  td.no  { background: #17191e; }
  td.nt  { background: #262013; }
  .yes { color: #5fd67f; } .no { color: #878d98; } .nt { color: #e0b23c; }
  .warn { background: #262013; border-left-color: #d0a000; }
  p.meta, td.where, span.where, td.doc, .legend, .params, figcaption { color: #98a0ad; }
}
";
		}

		private static string Escape(string text)
		{
			if (text == null)
				return "";
			return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
		}

		/// <summary>
		/// Renders the small amount of markdown the method notes use (`code` and **bold**) as HTML.
		/// </summary>
		private static string Inline(string text)
		{
			StringBuilder sb = new StringBuilder();
			bool inCode = false;
			bool inBold = false;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '`')
				{
					sb.Append(inCode ? "</code>" : "<code>");
					inCode = !inCode;
				}
				else if (text[i] == '*' && i + 1 < text.Length && text[i + 1] == '*')
				{
					sb.Append(inBold ? "</strong>" : "<strong>");
					inBold = !inBold;
					i++;
				}
				else
				{
					sb.Append(Escape(text[i].ToString()));
				}
			}
			return sb.ToString();
		}

		#endregion
	}
}
