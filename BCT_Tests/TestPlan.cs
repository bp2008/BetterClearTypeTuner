using System;
using System.Collections.Generic;
using BCT_Tests.Settings;

namespace BCT_Tests
{
	/// <summary>
	/// One value of one setting to measure.  Applying a value always happens on top of a cleared
	/// slate (see <see cref="AvalonRegistry.ClearAll"/>), so exactly one Avalon.Graphics value is
	/// present in the registry while its render is taken.
	/// </summary>
	internal class SettingValue
	{
		public string Id;
		public string Label;
		public Action Apply;

		public SettingValue(string id, string label, Action apply)
		{
			Id = id;
			Label = label;
			Apply = apply;
		}
	}

	internal enum SettingCategory
	{
		AvalonCurrentUser,
		AvalonLocalMachine,
		SystemParameter
	}

	internal class SettingUnderTest
	{
		public string Id;
		public SettingCategory Category;
		public string Name;
		/// <summary>Where the setting lives, for the report.</summary>
		public string Location;
		/// <summary>Documented range, default and source.</summary>
		public string Documentation;
		public List<SettingValue> Values = new List<SettingValue>();
		/// <summary>The label the report shows for the state each value is compared against.</summary>
		public string BaselineLabel;
		/// <summary>
		/// Optional extra state applied before this setting's value, for a setting that can only
		/// matter once something else has been arranged.  When set, this setting gets its own
		/// baseline render — the prerequisite alone — so the comparison still isolates the value
		/// under test.
		/// </summary>
		public Action ApplyPrerequisite;

		public bool NeedsLocalMachineWrite { get { return Category == SettingCategory.AvalonLocalMachine; } }
		public bool IsSystemParameter { get { return Category == SettingCategory.SystemParameter; } }
	}

	/// <summary>
	/// A system font-smoothing configuration to run the whole registry sweep inside.  These are the
	/// "rendering modes" of the compatibility matrix: each one is measured for GDI and for
	/// DirectWrite separately.
	/// </summary>
	internal class BaseState
	{
		public string Id;
		public string Label;
		/// <summary>
		/// The label for the compatibility matrix, where every base state gets its own column and the
		/// full label makes the table far wider than it needs to be.  A newline is a line break.
		/// </summary>
		public string ShortLabel;
		public bool Antialiasing;
		public SmoothingType Type;
		public SmoothingOrientation Orientation;
		public uint Contrast;

		public void Apply()
		{
			// Type and orientation are set before the master switch so that enabling smoothing
			// never briefly shows a mode that is not part of this configuration.
			SystemFontSmoothing.SetSmoothingType(Type);
			SystemFontSmoothing.SetOrientation(Orientation);
			SystemFontSmoothing.SetContrast(Contrast);
			SystemFontSmoothing.SetAntialiasingEnabled(Antialiasing);
		}
	}

	internal static class TestPlan
	{
		/// <summary>Contrast used by every base state, so it is never a hidden variable.</summary>
		public const uint BaseContrast = 1200;

		public static List<BaseState> BaseStates()
		{
			return new List<BaseState>
			{
				new BaseState
				{
					Id = "aa-off",
					Label = "No antialiasing",
					ShortLabel = "No AA",
					Antialiasing = false,
					Type = SmoothingType.Standard,
					Orientation = SmoothingOrientation.RGB,
					Contrast = BaseContrast
				},
				new BaseState
				{
					Id = "grayscale",
					Label = "Grayscale antialiasing",
					ShortLabel = "Grayscale",
					Antialiasing = true,
					Type = SmoothingType.Standard,
					Orientation = SmoothingOrientation.RGB,
					Contrast = BaseContrast
				},
				new BaseState
				{
					Id = "cleartype-rgb",
					Label = "ClearType (RGB)",
					ShortLabel = "ClearType\n(RGB)",
					Antialiasing = true,
					Type = SmoothingType.ClearType,
					Orientation = SmoothingOrientation.RGB,
					Contrast = BaseContrast
				},
				new BaseState
				{
					Id = "cleartype-bgr",
					Label = "ClearType (BGR)",
					ShortLabel = "ClearType\n(BGR)",
					Antialiasing = true,
					Type = SmoothingType.ClearType,
					Orientation = SmoothingOrientation.BGR,
					Contrast = BaseContrast
				}
			};
		}

		public static List<SettingUnderTest> Settings()
		{
			List<SettingUnderTest> settings = new List<SettingUnderTest>();

			// ---- Avalon.Graphics, both hives ----------------------------------------------
			// Microsoft documents each of the four WPF values in exactly one hive, but nothing
			// says the other hive is ignored, so both are measured for every value name.

			AddAvalon(settings, Hive.HKCU, AvalonRegistry.ClearTypeLevel,
				"0-100, default 100. Documented (WPF ClearType Registry Settings).",
				new int[] { 0, 50, 100 });
			AddAvalon(settings, Hive.HKCU, AvalonRegistry.GammaLevel,
				"1000-2200, default 1900. Documented for HKEY_LOCAL_MACHINE only.",
				new int[] { 1000, 1600, 2200 });
			AddAvalon(settings, Hive.HKCU, AvalonRegistry.PixelStructure,
				"0 = Flat, 1 = RGB, 2 = BGR. Documented for HKEY_LOCAL_MACHINE only.",
				new int[] { 0, 1, 2 });
			AddAvalon(settings, Hive.HKCU, AvalonRegistry.TextContrastLevel,
				"0-6, default 1. Documented (WPF ClearType Registry Settings).",
				new int[] { 0, 3, 6 });
			AddAvalon(settings, Hive.HKCU, AvalonRegistry.EnhancedContrastLevel,
				"Undocumented; written by cttune.exe. Feeds IDWriteRenderingParams::GetEnhancedContrast.",
				new int[] { 0, 50, 1000 });
			AddAvalon(settings, Hive.HKCU, AvalonRegistry.GrayscaleEnhancedContrastLevel,
				"Undocumented; written by cttune.exe. Feeds IDWriteRenderingParams1::GetGrayscaleEnhancedContrast.",
				new int[] { 0, 100, 1000 });

			AddAvalon(settings, Hive.HKLM, AvalonRegistry.ClearTypeLevel,
				"0-100, default 100. Documented for HKEY_CURRENT_USER only.",
				new int[] { 0, 50, 100 });
			AddAvalon(settings, Hive.HKLM, AvalonRegistry.GammaLevel,
				"1000-2200, default 1900. Documented (WPF ClearType Registry Settings).",
				new int[] { 1000, 1600, 2200 });
			AddAvalon(settings, Hive.HKLM, AvalonRegistry.PixelStructure,
				"0 = Flat, 1 = RGB, 2 = BGR, default 0. Documented (WPF ClearType Registry Settings).",
				new int[] { 0, 1, 2 });
			AddAvalon(settings, Hive.HKLM, AvalonRegistry.TextContrastLevel,
				"0-6, default 1. Documented for HKEY_CURRENT_USER only.",
				new int[] { 0, 3, 6 });
			AddAvalon(settings, Hive.HKLM, AvalonRegistry.EnhancedContrastLevel,
				"Undocumented; cttune.exe writes it to HKEY_CURRENT_USER.",
				new int[] { 0, 50, 1000 });
			AddAvalon(settings, Hive.HKLM, AvalonRegistry.GrayscaleEnhancedContrastLevel,
				"Undocumented; cttune.exe writes it to HKEY_CURRENT_USER.",
				new int[] { 0, 100, 1000 });

			// GrayscaleEnhancedContrastLevel is expected to apply only while DirectWrite is actually
			// rendering grayscale, which it never does on its own here, so it is also measured with
			// ClearTypeLevel pinned to 0 to put DirectWrite in grayscale mode first.
			SettingUnderTest grayscaleContrast = new SettingUnderTest
			{
				Id = "hkcu-grayscaleenhancedcontrastlevel-grayscale",
				Category = SettingCategory.AvalonCurrentUser,
				Name = "GrayscaleEnhancedContrastLevel (with ClearTypeLevel = 0)",
				Location = "HKCU\\" + AvalonRegistry.BasePath + "\\<display>\\" + AvalonRegistry.GrayscaleEnhancedContrastLevel,
				Documentation = "Undocumented; written by cttune.exe. Measured with ClearTypeLevel = 0, which is what puts "
					+ "DirectWrite into grayscale rendering.",
				BaselineLabel = "ClearTypeLevel = 0, GrayscaleEnhancedContrastLevel not present",
				ApplyPrerequisite = delegate
				{
					AvalonRegistry.SetValueOnAllDisplays(Hive.HKCU, AvalonRegistry.ClearTypeLevel, 0);
				}
			};
			foreach (int value in new int[] { 0, 100, 1000 })
			{
				int captured = value;
				grayscaleContrast.Values.Add(new SettingValue(value.ToString(), value.ToString(), delegate
				{
					AvalonRegistry.SetValueOnAllDisplays(Hive.HKCU, AvalonRegistry.GrayscaleEnhancedContrastLevel, captured);
				}));
			}
			settings.Add(grayscaleContrast);

			// ---- System settings, for comparison -------------------------------------------
			// These are not Avalon.Graphics values, but they are the settings the main
			// application does use, so measuring them the same way shows what a control that
			// definitely works looks like next to one that does not.

			SettingUnderTest contrast = new SettingUnderTest
			{
				Id = "spi-fontsmoothingcontrast",
				Category = SettingCategory.SystemParameter,
				Name = "FontSmoothingContrast",
				Location = "SystemParametersInfo SPI_SETFONTSMOOTHINGCONTRAST",
				Documentation = "1000-2200. Documented (SystemParametersInfo).",
				BaselineLabel = BaseContrast.ToString()
			};
			contrast.Values.Add(new SettingValue("1000", "1000", delegate { SystemFontSmoothing.SetContrast(1000); }));
			contrast.Values.Add(new SettingValue("2200", "2200", delegate { SystemFontSmoothing.SetContrast(2200); }));
			settings.Add(contrast);

			SettingUnderTest orientation = new SettingUnderTest
			{
				Id = "spi-fontsmoothingorientation",
				Category = SettingCategory.SystemParameter,
				Name = "FontSmoothingOrientation",
				Location = "SystemParametersInfo SPI_SETFONTSMOOTHINGORIENTATION",
				Documentation = "0 = BGR, 1 = RGB. Documented (SystemParametersInfo).",
				BaselineLabel = "the base state's own orientation"
			};
			// Both values are measured because the base states use both orientations; whichever one
			// matches the base state simply comes back identical.
			orientation.Values.Add(new SettingValue("rgb", "RGB",
				delegate { SystemFontSmoothing.SetOrientation(SmoothingOrientation.RGB); }));
			orientation.Values.Add(new SettingValue("bgr", "BGR",
				delegate { SystemFontSmoothing.SetOrientation(SmoothingOrientation.BGR); }));
			settings.Add(orientation);

			return settings;
		}

		private static void AddAvalon(List<SettingUnderTest> settings, Hive hive, string valueName,
			string documentation, int[] values)
		{
			SettingUnderTest setting = new SettingUnderTest
			{
				Id = (hive == Hive.HKCU ? "hkcu-" : "hklm-") + valueName.ToLowerInvariant(),
				Category = hive == Hive.HKCU ? SettingCategory.AvalonCurrentUser : SettingCategory.AvalonLocalMachine,
				Name = valueName,
				Location = (hive == Hive.HKCU ? "HKCU" : "HKLM") + "\\" + AvalonRegistry.BasePath + "\\<display>\\" + valueName,
				Documentation = documentation,
				BaselineLabel = "value not present"
			};
			foreach (int value in values)
			{
				int captured = value;
				Hive capturedHive = hive;
				string capturedName = valueName;
				setting.Values.Add(new SettingValue(value.ToString(), value.ToString(),
					delegate { AvalonRegistry.SetValueOnAllDisplays(capturedHive, capturedName, captured); }));
			}
			settings.Add(setting);
		}
	}
}
