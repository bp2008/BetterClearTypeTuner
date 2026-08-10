using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using BCT_Tests.Rendering;
using BCT_Tests.Settings;
using Microsoft.Win32;

namespace BCT_Tests
{
	/// <summary>
	/// The "--render" mode.  One configuration is rendered per process launch, and the process then
	/// exits.
	///
	/// This is not an optimisation problem: DirectWrite resolves its default rendering parameters
	/// when a factory is created and applications normally pick up ClearType registry changes only
	/// after a restart, so measuring a fresh process per configuration is both the safe way to
	/// avoid measuring a stale cache and a faithful model of how a real application sees these
	/// settings.
	/// </summary>
	internal static class ChildRenderProcess
	{
		public const string ImagesFolder = "images";
		public const string RawFolder = "raw";

		public static string GdiImageRelativePath(string configName)
		{
			return ImagesFolder + "/" + configName + ".gdi.png";
		}

		public static string DwImageRelativePath(string configName)
		{
			return ImagesFolder + "/" + configName + ".dw.png";
		}

		public static string DwAppImageRelativePath(string configName)
		{
			return ImagesFolder + "/" + configName + ".dwapp.png";
		}

		public static string RawRelativePath(string configName)
		{
			return RawFolder + "/" + configName + ".txt";
		}

		public static int Run(string outputDir, string configName)
		{
			Directory.CreateDirectory(Path.Combine(outputDir, ImagesFolder));
			Directory.CreateDirectory(Path.Combine(outputDir, RawFolder));

			List<string> info = new List<string>();
			info.Add("config=" + configName);
			info.Add("time=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

			// What the system settings actually look like from inside this process, so the report
			// can never be wrong about the state a render was taken in.
			info.Add("spi.antialiasing=" + SystemFontSmoothing.GetAntialiasingEnabled());
			info.Add("spi.smoothingType=" + SystemFontSmoothing.GetSmoothingType());
			info.Add("spi.orientation=" + SystemFontSmoothing.GetOrientation());
			info.Add("spi.contrast=" + SystemFontSmoothing.GetContrast());
			AppendRegistryState(info);

			string gdiPath = Path.Combine(outputDir, GdiImageRelativePath(configName).Replace('/', Path.DirectorySeparatorChar));
			try
			{
				using (Bitmap gdi = GdiRenderer.Render())
					gdi.Save(gdiPath, ImageFormat.Png);
			}
			catch (Exception ex)
			{
				info.Add("gdi.error=" + Flatten(ex.ToString()));
			}

			DwRenderResult dw = RenderDirectWrite(outputDir, configName, DwRenderMode.SystemDefaults,
				DwImageRelativePath(configName), "dw", info);
			AppendParams(info, "dw.monitor", dw.MonitorParams);
			AppendParams(info, "dw.default", dw.DefaultParams);

			DwRenderResult dwApp = RenderDirectWrite(outputDir, configName, DwRenderMode.AppConfigured,
				DwAppImageRelativePath(configName), "dwapp", info);
			AppendParams(info, "dwapp.effective", dwApp.EffectiveParams);

			File.WriteAllLines(Path.Combine(outputDir, RawRelativePath(configName).Replace('/', Path.DirectorySeparatorChar)), info);
			return 0;
		}

		private static DwRenderResult RenderDirectWrite(string outputDir, string configName, DwRenderMode mode,
			string relativePath, string infoPrefix, List<string> info)
		{
			DwRenderResult result = DirectWriteRenderer.Render(mode);
			try
			{
				if (result.Image != null)
				{
					result.Image.Save(Path.Combine(outputDir, relativePath.Replace('/', Path.DirectorySeparatorChar)),
						ImageFormat.Png);
					result.Image.Dispose();
				}
			}
			catch (Exception ex)
			{
				info.Add(infoPrefix + ".saveError=" + Flatten(ex.ToString()));
			}
			if (result.Error != null)
				info.Add(infoPrefix + ".error=" + Flatten(result.Error));
			return result;
		}

		private static void AppendParams(List<string> info, string prefix, DwRenderingParamValues p)
		{
			if (p == null)
				return;
			info.Add(prefix + ".gamma=" + p.Gamma.ToString("0.######", CultureInfo.InvariantCulture));
			info.Add(prefix + ".enhancedContrast=" + p.EnhancedContrast.ToString("0.######", CultureInfo.InvariantCulture));
			info.Add(prefix + ".clearTypeLevel=" + p.ClearTypeLevel.ToString("0.######", CultureInfo.InvariantCulture));
			info.Add(prefix + ".pixelGeometry=" + p.PixelGeometry);
			info.Add(prefix + ".renderingMode=" + p.RenderingMode);
			info.Add(prefix + ".grayscaleEnhancedContrast=" + (p.GrayscaleEnhancedContrast.HasValue
				? p.GrayscaleEnhancedContrast.Value.ToString("0.######", CultureInfo.InvariantCulture)
				: "n/a"));
		}

		/// <summary>
		/// Records the Avalon.Graphics values as this process sees them, which confirms the parent's
		/// registry writes really landed before the render was taken.
		/// </summary>
		private static void AppendRegistryState(List<string> info)
		{
			foreach (Hive hive in new Hive[] { Hive.HKCU, Hive.HKLM })
			{
				using (RegistryKey root = AvalonRegistry.OpenBase(hive))
				{
					foreach (string display in AvalonRegistry.DisplayNames)
					{
						using (RegistryKey key = root.OpenSubKey(AvalonRegistry.KeyPath(display), false))
						{
							if (key == null)
							{
								info.Add("reg." + hive + "." + display + "=(key absent)");
								continue;
							}
							List<string> pairs = new List<string>();
							foreach (string name in AvalonRegistry.AllValueNames)
							{
								object value = key.GetValue(name);
								if (value != null)
									pairs.Add(name + "=" + value);
							}
							info.Add("reg." + hive + "." + display + "=" + (pairs.Count == 0 ? "(none)" : string.Join(" ", pairs)));
						}
					}
				}
			}
		}

		private static string Flatten(string text)
		{
			return text.Replace("\r", " ").Replace("\n", " ");
		}
	}
}
