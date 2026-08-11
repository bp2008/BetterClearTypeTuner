using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace BetterClearTypeTuner
{
	/// <summary>
	/// What one instance of this application hands to the copy of itself that it restarts with
	/// administrator rights: the change it was in the middle of making, and where its window was.
	/// The elevated instance replays both, so the only thing the user sees of the restart is the
	/// UAC prompt itself.
	/// </summary>
	public class StartupState
	{
		private const string ArgElevationAttempted = "--elevation-attempted";
		private const string ArgRestoreDefaults = "--restore-defaults";
		private const string ArgAntialiasing = "--antialiasing=";
		private const string ArgPixelStructure = "--pixel-structure=";
		private const string ArgGdiContrast = "--gdi-contrast=";
		private const string ArgDwOverride = "--dw-override=";
		private const string ArgGammaLevel = "--dw-contrast=";
		private const string ArgClearTypeLevel = "--cleartype-level=";
		private const string ArgEnhancedContrast = "--enhanced-contrast=";
		private const string ArgDarkMode = "--dark-mode=";
		private const string ArgMaximized = "--maximized=";
		private const string ArgWindow = "--window=";
		/// <summary>
		/// How many of the arguments above carry a setting.  All of them or none of them are
		/// replayed, because applying a half-parsed command line would write values the previous
		/// instance never asked for.  Keep this in step with the settings themselves: if it is left
		/// too low a missing argument goes unnoticed, and if it is left too high every replay is
		/// silently discarded and the elevated instance comes up having forgotten the change.
		/// </summary>
		private const int SettingCount = 7;

		/// <summary>
		/// True if this instance was started by another one asking for elevation.  It stops a
		/// restart loop if the elevated process still cannot write the registry.
		/// </summary>
		public bool ElevationAttempted;
		/// <summary>
		/// True if the change being replayed is the Restore Defaults button, which deletes the
		/// Avalon.Graphics keys instead of writing them.
		/// </summary>
		public bool RestoreDefaults;
		public bool AntialiasingEnabled;
		/// <summary>
		/// 0 = grayscale, 1 = RGB, 2 = BGR, -1 = none of the three selected.
		/// </summary>
		public int PixelStructure = -1;
		public int GdiContrast;
		/// <summary>
		/// True if the Avalon.Graphics keys are to be written, false if they are to be removed.
		/// </summary>
		public bool DwOverride;
		public int GammaLevel;
		public int ClearTypeLevel;
		public int EnhancedContrastLevel;
		/// <summary>
		/// Dark mode checkbox state, or null to let the new instance read the Windows preference
		/// as it would on an ordinary launch.
		/// </summary>
		public bool? DarkMode;
		public bool Maximized;
		/// <summary>
		/// Window bounds in screen coordinates.  While maximized this is the restore size, so that
		/// un-maximizing the new window gives back the old one's size.
		/// </summary>
		public Rectangle WindowBounds = Rectangle.Empty;

		private bool settingsComplete;

		/// <summary>True if there is a change to replay.</summary>
		public bool HasSettings { get { return settingsComplete; } }
		/// <summary>True if there is a window placement to restore.</summary>
		public bool HasWindowBounds { get { return WindowBounds.Width > 0 && WindowBounds.Height > 0; } }

		/// <summary>
		/// Renders this state as a command line.  Every value is an integer, so nothing here can
		/// contain a space or a quote and no escaping is needed.
		/// </summary>
		public string ToArguments()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(ArgElevationAttempted);
			if (RestoreDefaults)
				sb.Append(' ').Append(ArgRestoreDefaults);
			Append(sb, ArgAntialiasing, AntialiasingEnabled ? 1 : 0);
			Append(sb, ArgPixelStructure, PixelStructure);
			Append(sb, ArgGdiContrast, GdiContrast);
			Append(sb, ArgDwOverride, DwOverride ? 1 : 0);
			Append(sb, ArgGammaLevel, GammaLevel);
			Append(sb, ArgClearTypeLevel, ClearTypeLevel);
			Append(sb, ArgEnhancedContrast, EnhancedContrastLevel);
			if (DarkMode.HasValue)
				Append(sb, ArgDarkMode, DarkMode.Value ? 1 : 0);
			if (Maximized)
				Append(sb, ArgMaximized, 1);
			if (HasWindowBounds)
				sb.Append(' ').Append(ArgWindow)
					.Append(Number(WindowBounds.X)).Append(',')
					.Append(Number(WindowBounds.Y)).Append(',')
					.Append(Number(WindowBounds.Width)).Append(',')
					.Append(Number(WindowBounds.Height));
			return sb.ToString();
		}

		/// <summary>
		/// Reads back what <see cref="ToArguments"/> wrote.  Returns null for a command line that
		/// says nothing about any of this, which is the ordinary case of a user double-clicking
		/// the executable.
		/// </summary>
		public static StartupState FromCommandLine(string[] args)
		{
			if (args == null || args.Length == 0)
				return null;

			StartupState state = new StartupState();
			int settingsSeen = 0;
			foreach (string arg in args)
			{
				int value;
				if (string.Equals(arg, ArgElevationAttempted, StringComparison.OrdinalIgnoreCase))
					state.ElevationAttempted = true;
				else if (string.Equals(arg, ArgRestoreDefaults, StringComparison.OrdinalIgnoreCase))
					state.RestoreDefaults = true;
				else if (TryValue(arg, ArgAntialiasing, out value))
				{
					state.AntialiasingEnabled = value != 0;
					settingsSeen++;
				}
				else if (TryValue(arg, ArgPixelStructure, out value))
				{
					state.PixelStructure = value;
					settingsSeen++;
				}
				else if (TryValue(arg, ArgGdiContrast, out value))
				{
					state.GdiContrast = value;
					settingsSeen++;
				}
				else if (TryValue(arg, ArgDwOverride, out value))
				{
					state.DwOverride = value != 0;
					settingsSeen++;
				}
				else if (TryValue(arg, ArgGammaLevel, out value))
				{
					state.GammaLevel = value;
					settingsSeen++;
				}
				else if (TryValue(arg, ArgClearTypeLevel, out value))
				{
					state.ClearTypeLevel = value;
					settingsSeen++;
				}
				else if (TryValue(arg, ArgEnhancedContrast, out value))
				{
					state.EnhancedContrastLevel = value;
					settingsSeen++;
				}
				else if (TryValue(arg, ArgDarkMode, out value))
					state.DarkMode = value != 0;
				else if (TryValue(arg, ArgMaximized, out value))
					state.Maximized = value != 0;
				else if (arg.StartsWith(ArgWindow, StringComparison.OrdinalIgnoreCase))
					state.WindowBounds = ParseBounds(arg.Substring(ArgWindow.Length));
			}
			state.settingsComplete = settingsSeen == SettingCount;

			if (settingsSeen == 0 && !state.HasWindowBounds && !state.ElevationAttempted)
				return null;
			return state;
		}

		private static void Append(StringBuilder sb, string prefix, int value)
		{
			sb.Append(' ').Append(prefix).Append(Number(value));
		}

		private static string Number(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		private static bool TryValue(string arg, string prefix, out int value)
		{
			value = 0;
			if (!arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				return false;
			return int.TryParse(arg.Substring(prefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
		}

		private static Rectangle ParseBounds(string value)
		{
			string[] parts = value.Split(',');
			if (parts.Length != 4)
				return Rectangle.Empty;
			int[] n = new int[4];
			for (int i = 0; i < 4; i++)
				if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out n[i]))
					return Rectangle.Empty;
			return new Rectangle(n[0], n[1], n[2], n[3]);
		}
	}
}
