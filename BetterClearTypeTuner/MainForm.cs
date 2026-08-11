using BetterClearTypeTuner.Native;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterClearTypeTuner
{
	public partial class MainForm : Form
	{
		bool dirty = false;
		bool initialized = false;
		bool setDefaults = false;
		Color TextColor = SystemColors.WindowText;
		Color BackgroundColor = SystemColors.Control;
		List<Control> fontableControls = new List<Control>();
		SortedList<string, float> baselineFontSizes = new SortedList<string, float>();
		/// <summary>
		/// Renders the DirectWrite preview. Null only if the interop could not be initialized.
		/// </summary>
		DirectWriteSampleRenderer dwRenderer;
		/// <summary>
		/// Caption for the DirectWrite preview, restored after an error has been displayed there.
		/// </summary>
		string dwZoomHeaderText;
		/// <summary>
		/// What the instance that restarted itself with administrator rights was in the middle of
		/// doing, or null on an ordinary launch.
		/// </summary>
		readonly StartupState startupState;

		/// <summary>
		/// Parameterless constructor for the Windows Forms designer, which cannot instantiate a
		/// form that only offers a constructor with arguments.
		/// </summary>
		public MainForm() : this(null)
		{
		}

		public MainForm(StartupState startupState)
		{
			this.startupState = startupState;

			InitializeComponent();

			dwZoomHeaderText = lblDwZoomHeader.Text;
			dwRenderer = new DirectWriteSampleRenderer();

			InitializeDpiScale();
			InitializeHelpText();
			GatherFontableControls(this, this.Font.FontFamily.Name);

			this.Text += " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

			bool startInDarkMode = (startupState != null && startupState.DarkMode.HasValue)
				? startupState.DarkMode.Value
				: PrefersDarkMode();
			if (startInDarkMode)
				cbDarkmode.Checked = true;
			else
				SetDarkMode(this, false);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			UpdateStatus();
			cbFontAntialiasing.Focus();
			FixFontSizing();
			DpiScalingInitHack();
			// The placement is restored last because the hack above moves the window about, and the
			// bounds handed over by the previous instance are the ones that must win.
			if (!RestoreWindowPlacement())
				ShrinkToFitScreen();
			else if (startupState.Maximized)
				this.WindowState = FormWindowState.Maximized;
			initialized = true;

			if (startupState != null && startupState.HasSettings)
				ApplyStartupSettings();
		}

		/// <summary>
		/// Puts this window exactly where the instance that asked for elevation had it, so that the
		/// restart looks like the same window rather than a new one.  Returns false if there is no
		/// placement to restore, or if the monitor it names is no longer part of the desktop.
		/// </summary>
		private bool RestoreWindowPlacement()
		{
			if (startupState == null || !startupState.HasWindowBounds)
				return false;
			if (!SystemInformation.VirtualScreen.IntersectsWith(startupState.WindowBounds))
				return false;

			this.StartPosition = FormStartPosition.Manual;
			this.Bounds = startupState.WindowBounds;
			return true;
		}

		/// <summary>
		/// Replays the change that the previous, non-elevated instance was making when it restarted
		/// itself.  Now that the process has the rights to finish the job, the change is applied
		/// exactly as if the user had just made it in this window.
		/// </summary>
		private void ApplyStartupSettings()
		{
			DisableEvents();
			cbFontAntialiasing.Checked = startupState.AntialiasingEnabled;
			rbGrayscale.Checked = startupState.PixelStructure == 0;
			rbRGB.Checked = startupState.PixelStructure == 1;
			rbBGR.Checked = startupState.PixelStructure == 2;
			ShowValue(nudGdiContrast, startupState.GdiContrast, FontSmoothing.ContrastMin, FontSmoothing.ContrastMax);
			ShowValue(nudDwContrast, startupState.GammaLevel, GammaLevelMin, GammaLevelMax);
			ShowValue(nudClearTypeLevel, startupState.ClearTypeLevel, ClearTypeLevelMin, ClearTypeLevelMax);
			ShowValue(nudEnhancedContrast, startupState.EnhancedContrastLevel, EnhancedContrastLevelMin, EnhancedContrastLevelMax);
			EnableEvents();

			setDefaults = startupState.RestoreDefaults;
			ControlsChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// The default window size fits a 1280x720 screen at 100% scaling, but not once DPI
		/// scaling is applied, so shrink it to whatever the screen actually has room for.
		/// The content panel scrolls to compensate.
		/// </summary>
		private void ShrinkToFitScreen()
		{
			Rectangle workingArea = Screen.FromControl(this).WorkingArea;
			int width = Math.Min(this.Width, workingArea.Width);
			int height = Math.Min(this.Height, workingArea.Height);
			if (width == this.Width && height == this.Height)
				return;

			this.Size = new Size(width, height);
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(
				workingArea.X + ((workingArea.Width - width) / 2),
				workingArea.Y + ((workingArea.Height - height) / 2));
		}

		private void DpiScalingInitHack()
		{
			// If the form is not currently on the primary monitor, move it to the primary monitor, then move it back to this monitor.
			Screen[] allScreens = Screen.AllScreens;
			Screen primary = allScreens.FirstOrDefault(s => s.Primary);
			Screen parentScreen = null;
			Point startPos = this.Location;
			foreach (Screen screen in allScreens)
			{
				Point formCenter = new Point(startPos.X + (this.Size.Width / 2), startPos.Y + (this.Size.Height / 2));
				if (screen.Bounds.Contains(formCenter))
				{
					parentScreen = screen;
					break;
				}
			}
			if (primary != null && parentScreen != primary)
			{
				this.StartPosition = FormStartPosition.Manual;
				this.Location = new Point(primary.Bounds.X + ((primary.Bounds.Width - this.Size.Width) / 2),
					primary.Bounds.Y + ((primary.Bounds.Height - this.Size.Height) / 2));
				if (parentScreen != null)
				{
					this.Location = startPos;
				}
			}
		}

		private void GatherFontableControls(Control control, string fontName)
		{
			baselineFontSizes[control.Name] = control.Font.Size;
			if (control.Font.FontFamily.Name == fontName)
				fontableControls.Add(control);
			foreach (Control child in control.Controls)
				GatherFontableControls(child, fontName);
		}
		private void SetDarkMode(Control control, bool dark)
		{
			if (control == this)
			{
				// Main Form only
				if (dark)
				{
					TextColor = Color.White;
					BackgroundColor = ColorTranslator.FromHtml("#121212");
				}
				else
				{
					TextColor = SystemColors.WindowText;
					BackgroundColor = SystemColors.Control;
				}
				control.BackColor = BackgroundColor;
				control.ForeColor = TextColor;
			}
			else if (control.Name == "panelSmall" || control is PictureBox)
			{
				if (dark)
				{
					control.BackColor = Color.Black;
				}
				else
				{
					control.BackColor = Color.White;
				}
			}
			else if (control.Name.StartsWith("panelRule"))
			{
				// The thin horizontal rules that separate the setting groups.
				if (dark)
					control.BackColor = ColorTranslator.FromHtml("#3D3D3D");
				else
					control.BackColor = SystemColors.ControlDark;
			}
			else if (control.Name.StartsWith("lblSample"))
			{
				control.BackColor = Color.Transparent;
				if (dark)
				{
					control.ForeColor = Color.White;
				}
				else
				{
					control.ForeColor = Color.Black;
				}
			}
			else if (control is LinkLabel linkLabel)
			{
				// LinkLabel is a Label, but its link colors are separate from ForeColor and the
				// default dark blue is unreadable against the dark background.
				linkLabel.BackColor = Color.Transparent;
				linkLabel.ForeColor = dark ? ColorTranslator.FromHtml("#DEDEDE") : Color.Black;
				linkLabel.LinkColor = dark ? ColorTranslator.FromHtml("#69B4FF") : Color.FromArgb(0, 0, 192);
				linkLabel.ActiveLinkColor = linkLabel.LinkColor;
				linkLabel.VisitedLinkColor = dark ? ColorTranslator.FromHtml("#BB9CFF") : Color.FromArgb(128, 0, 128);
			}
			else if (control is Label || control is RadioButton || control is CheckBox)
			{
				control.BackColor = Color.Transparent;
				if (dark)
				{
					control.ForeColor = ColorTranslator.FromHtml("#DEDEDE");
				}
				else
				{
					control.ForeColor = Color.Black;
				}
			}
			else if (control is Button)
			{
				if (dark)
				{
					control.BackColor = Color.Black;
					control.ForeColor = ColorTranslator.FromHtml("#DEDEDE");
				}
				else
				{
					control.BackColor = SystemColors.Control;
					control.ForeColor = Color.Black;
				}
			}
			else if (control is NumericUpDown)
			{
				if (dark)
				{
					control.BackColor = Color.Black;
					control.ForeColor = ColorTranslator.FromHtml("#DEDEDE");
				}
				else
				{
					control.BackColor = Color.White;
					control.ForeColor = Color.Black;
				}
			}
			foreach (Control child in control.Controls)
			{
				SetDarkMode(child, dark);
			}
			if (control == this)
			{
				// The loop above repainted every label in the theme's text color, which undoes
				// the graying out of the settings that do not apply in the current mode.
				ApplyEnabledStates();
				CopyZoomedSnapshot();
			}
		}

		private void ControlsChanged(object sender, EventArgs e)
		{
			bool restoringDefaults = setDefaults;
			setDefaults = false;
			if (initialized && !restartingElevated)
			{
				registryAccessDenied = false;
				if (restoringDefaults || AvalonValuesDiffer())
					dirty = true;

				SetAvalonKeys(restoringDefaults);
				if (rbGrayscale.Checked)
				{
					SetFontSmoothingTypeIfNotAlready(FontSmoothingType.Standard);
				}
				else if (rbRGB.Checked)
				{
					SetFontSmoothingTypeIfNotAlready(FontSmoothingType.ClearType);
					SetFontSmoothingIfNotAlready(FontSmoothingOrientation.RGB);
				}
				else if (rbBGR.Checked)
				{
					SetFontSmoothingTypeIfNotAlready(FontSmoothingType.ClearType);
					SetFontSmoothingIfNotAlready(FontSmoothingOrientation.BGR);
				}
				if (FontSmoothing.GetContrast() != DesiredGdiContrast)
				{
					FontSmoothing.SetContrast(DesiredGdiContrast);
					dirty = true;
				}
				if (FontSmoothing.GetAntialiasingEnabled() != cbFontAntialiasing.Checked)
				{
					FontSmoothing.SetAntialiasingEnabled(cbFontAntialiasing.Checked);
					dirty = true;
				}
				if (registryAccessDenied && RestartAsAdministrator(restoringDefaults))
				{
					// The elevated instance is about to redo all of this properly, so there is
					// nothing left for this one to display.  Closing from inside a control's event
					// handler would dispose the form out from under the code that raised the event,
					// so let the current event finish first.
					restartingElevated = true;
					BeginInvoke((Action)Close);
					return;
				}
				if (dirty)
					UpdateStatus();
			}
		}

		#region Values to write
		/// <summary>
		/// GDI contrast (SPI_SETFONTSMOOTHINGCONTRAST).  Only GDI ClearType uses it.
		/// </summary>
		private uint DesiredGdiContrast
		{
			get { return Clamp((uint)nudGdiContrast.Value, FontSmoothing.ContrastMin, FontSmoothing.ContrastMax); }
		}
		/// <summary>
		/// Which of the three antialiasing mode buttons is selected: 0 = grayscale, 1 = RGB,
		/// 2 = BGR, or -1 when none of them is, which happens when Windows reports a subpixel
		/// orientation this application does not offer.
		/// </summary>
		private int SelectedPixelStructure
		{
			get
			{
				if (rbGrayscale.Checked)
					return 0;
				if (rbRGB.Checked)
					return 1;
				if (rbBGR.Checked)
					return 2;
				return -1;
			}
		}
		/// <summary>
		/// Subpixel structure: 0 = flat (grayscale), 1 = RGB, 2 = BGR.  The registry has no way to
		/// say "none of the above", so a selection of none is written as grayscale.
		/// </summary>
		private int DesiredPixelStructure
		{
			get
			{
				int selected = SelectedPixelStructure;
				return selected < 0 ? 0 : selected;
			}
		}
		/// <summary>
		/// ClearType Level.  Grayscale mode stores 0, because that is the value which stops
		/// DirectWrite from blending across subpixels for clients that read this key directly.
		/// </summary>
		private int DesiredClearTypeLevel
		{
			get
			{
				if (DesiredPixelStructure == 0)
					return 0;
				return (int)Clamp((uint)nudClearTypeLevel.Value, ClearTypeLevelMin, ClearTypeLevelMax);
			}
		}
		/// <summary>
		/// DirectWrite contrast (GammaLevel).
		/// </summary>
		private int DesiredGammaLevel
		{
			get { return (int)Clamp((uint)nudDwContrast.Value, GammaLevelMin, GammaLevelMax); }
		}
		/// <summary>
		/// DirectWrite enhanced contrast (EnhancedContrastLevel).
		/// </summary>
		private int DesiredEnhancedContrastLevel
		{
			get { return (int)Clamp((uint)nudEnhancedContrast.Value, EnhancedContrastLevelMin, EnhancedContrastLevelMax); }
		}
		/// <summary>
		/// Returns true if any Avalon.Graphics value that this application manages is not
		/// already what the controls ask for.
		/// </summary>
		private bool AvalonValuesDiffer()
		{
			return GetAvalonValue("PixelStructure", -1) != DesiredPixelStructure
				|| GetAvalonValue("ClearTypeLevel", (int)ClearTypeLevelDefault) != DesiredClearTypeLevel
				|| GetAvalonValue("GammaLevel", (int)GammaLevelDefault) != DesiredGammaLevel
				|| GetAvalonValue("EnhancedContrastLevel", (int)EnhancedContrastLevelDefault) != DesiredEnhancedContrastLevel;
		}
		#endregion

		private void SetAvalonKeys(bool setDefaults)
		{
			if (setDefaults)
			{
				DeleteRegistrySubkeys(Registry.LocalMachine, AvalonKeyPath);
				DeleteRegistrySubkeys(Registry.CurrentUser, AvalonKeyPath);
				return;
			}

			foreach (string displayName in GetDisplayNames())
			{
				string keyPath = AvalonKeyPath + "\\" + displayName;

				// Local Machine.  GammaLevel and PixelStructure are the only two values
				// DirectWrite reads from this hive; anything else written here is ignored.
				SetRegistryDWORDValue(Registry.LocalMachine, keyPath, "GammaLevel", DesiredGammaLevel);
				SetRegistryDWORDValue(Registry.LocalMachine, keyPath, "PixelStructure", DesiredPixelStructure);

				// Current User.  These take precedence over the Local Machine values above, and
				// this is the only hive in which ClearTypeLevel and EnhancedContrastLevel work.
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "ClearTypeLevel", DesiredClearTypeLevel);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "EnhancedContrastLevel", DesiredEnhancedContrastLevel);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "GammaLevel", DesiredGammaLevel);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "PixelStructure", DesiredPixelStructure);
				// Measurably inert, but written anyway so that a value left behind by another
				// tuner is put back to its documented default rather than left in place.
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "GrayscaleEnhancedContrastLevel", 100);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "TextContrastLevel", 1);
			}
		}

		private void SetFontSmoothingIfNotAlready(FontSmoothingOrientation orientation)
		{
			if (FontSmoothing.GetFontSmoothingOrientation() != orientation)
			{
				FontSmoothing.SetFontSmoothingOrientation(orientation);
				dirty = true;
			}
		}

		private void SetFontSmoothingTypeIfNotAlready(FontSmoothingType type)
		{
			if (FontSmoothing.GetFontSmoothingType() != type)
			{
				FontSmoothing.SetFontSmoothingType(type);
				dirty = true;
			}
		}
		private void BtnRestoreDefaults_Click(object sender, EventArgs e)
		{
			DisableEvents();
			setDefaults = true;
			cbFontAntialiasing.Checked = true;
			rbRGB.Checked = true;
			nudGdiContrast.Value = FontSmoothing.ContrastDefault;
			nudDwContrast.Value = GammaLevelDefault;
			nudClearTypeLevel.Value = ClearTypeLevelDefault;
			nudEnhancedContrast.Value = EnhancedContrastLevelDefault;
			EnableEvents();
			ControlsChanged(sender, e);
		}

		private void btnApply_Click(object sender, EventArgs e)
		{
			// Nothing may have changed, in which case ControlsChanged writes nothing and leaves
			// the previews alone, so refresh them here to show that the click was noticed.
			ControlsChanged(sender, e);
			UpdateStatus();
		}

		private void btnChangeFont_Click(object sender, EventArgs e)
		{
			if (fontDialog1.ShowDialog() == DialogResult.OK)
			{
				foreach (Control c in fontableControls)
				{
					c.Font = new Font(fontDialog1.Font.Name, c.Font.Size, c.Font.Style, c.Font.Unit);
				}
				CopyZoomedSnapshot();
			}
		}

		private void cbDarkmode_CheckedChanged(object sender, EventArgs e)
		{
			SetDarkMode(this, cbDarkmode.Checked);
		}

		private void MainForm_DpiChanged(object sender, DpiChangedEventArgs e)
		{
			FixFontSizing();
			SetTimeout.OnGui(CopyZoomedSnapshot, 100, this, ex => MessageBox.Show(ex.ToString()));
			SetTimeout.OnGui(CopyZoomedSnapshot, 500, this, ex => MessageBox.Show(ex.ToString()));
			SetTimeout.OnGui(CopyZoomedSnapshot, 1000, this, ex => MessageBox.Show(ex.ToString()));
		}

		#region Registry
		/// <summary>
		/// Where DirectWrite's per-display tuning values live, under both HKLM and HKCU.
		/// </summary>
		public const string AvalonKeyPath = "Software\\Microsoft\\Avalon.Graphics";
		/// <summary>
		/// DirectWrite/WPF ClearType amount (0 = grayscale … 100 = full). Same key as cttune.
		/// Only meaningful in the ClearType modes, and only under HKCU.
		/// </summary>
		public const uint ClearTypeLevelMin = 0;
		public const uint ClearTypeLevelMax = 100;
		public const uint ClearTypeLevelDefault = 100;
		/// <summary>
		/// DirectWrite contrast (GammaLevel).  Higher numbers give lighter text.  Unlike the GDI
		/// contrast this also applies to grayscale antialiasing.  Microsoft documents the default
		/// as 1900.
		/// </summary>
		public const uint GammaLevelMin = 1000;
		public const uint GammaLevelMax = 2200;
		public const uint GammaLevelDefault = 1900;
		/// <summary>
		/// DirectWrite's second contrast control (EnhancedContrastLevel).  Higher numbers give
		/// darker text.  Applies to grayscale as well as ClearType, and only under HKCU.
		/// DirectWrite ignores values above 400 entirely.
		/// </summary>
		public const uint EnhancedContrastLevelMin = 0;
		public const uint EnhancedContrastLevelMax = 400;
		public const uint EnhancedContrastLevelDefault = 50;

		/// <summary>
		/// Set while a write pass is refused for lack of permission.  Cleared at the start of each
		/// pass, and answered at the end of it by <see cref="RestartAsAdministrator"/>.
		/// </summary>
		bool registryAccessDenied = false;
		/// <summary>
		/// True once the user has dismissed a UAC prompt, so that every further tweak in this
		/// session does not raise another one.
		/// </summary>
		bool elevationRefused = false;
		/// <summary>
		/// True once an elevated instance has been started and this one is on its way out.  One
		/// user action can raise several change events - picking an antialiasing mode unchecks the
		/// previous radio button, and both raise CheckedChanged - and they all arrive before the
		/// posted Close runs, so without this the later ones would each start another instance.
		/// </summary>
		bool restartingElevated = false;
		/// <summary>
		/// True once a registry failure that administrator rights cannot fix has been reported.
		/// </summary>
		bool registryFailReported = false;

		private void SetRegistryDWORDValue(RegistryKey baseKey, string keyPath, string name, int value)
		{
			// Reading first costs nothing and matters a great deal: opening a key for writing is
			// refused under HKEY_LOCAL_MACHINE without administrator rights whether or not the
			// value would actually change, and that refusal is what raises the UAC prompt.
			if (RegistryDWORDValueEquals(baseKey, keyPath, name, value))
				return;
			try
			{
				RegistryKey key = baseKey.CreateSubKey(keyPath);
				key.SetValue(name, value, RegistryValueKind.DWord);
			}
			catch (SecurityException)
			{
				registryAccessDenied = true;
			}
			catch (UnauthorizedAccessException)
			{
				registryAccessDenied = true;
			}
		}
		private bool RegistryDWORDValueEquals(RegistryKey baseKey, string keyPath, string name, int value)
		{
			try
			{
				using (RegistryKey key = baseKey.OpenSubKey(keyPath, false))
					return key != null && key.GetValue(name) is int existing && existing == value;
			}
			catch
			{
				return false;
			}
		}
		private int GetRegistryDWORDValue(RegistryKey baseKey, string keyPath, string name, int defaultValue)
		{
			try
			{
				RegistryKey key = baseKey.OpenSubKey(keyPath, false);
				if (key == null)
					return defaultValue;
				object value = key.GetValue(name);
				if (value == null)
					return defaultValue;
				if (value is int i)
					return i;
				if (int.TryParse(value.ToString(), out int parsed))
					return parsed;
			}
			catch
			{
			}
			return defaultValue;
		}
		/// <summary>
		/// Reads one of the Avalon.Graphics values from HKCU.  Every display is written with the
		/// same values, so the first display speaks for all of them.
		/// </summary>
		private int GetAvalonValue(string name, int defaultValue)
		{
			string[] displayNames = GetDisplayNames();
			if (displayNames.Length == 0)
				return defaultValue;
			return GetRegistryDWORDValue(Registry.CurrentUser, AvalonKeyPath + "\\" + displayNames[0], name, defaultValue);
		}
		private void DeleteRegistrySubkeys(RegistryKey baseKey, string keyPath)
		{
			try
			{
				RegistryKey folder = baseKey.OpenSubKey(keyPath, true);
				if (folder != null)
					foreach (string subkeyName in folder.GetSubKeyNames())
						folder.DeleteSubKeyTree(subkeyName);
				//RegistryKey key = baseKey.DeleteSubKeyTree();
				//key.SetValue(name, value, RegistryValueKind.DWord);
			}
			catch (SecurityException)
			{
				registryAccessDenied = true;
			}
			catch (UnauthorizedAccessException)
			{
				registryAccessDenied = true;
			}
		}
		#endregion
		#region Elevation
		/// <summary>
		/// Called at the end of a write pass that Windows refused.  Restarts this application with
		/// administrator rights, handing the new instance the change being made and this window's
		/// placement so that it can carry on where this one left off.  Returns true if the elevated
		/// instance started, in which case this one has nothing left to do.
		/// </summary>
		private bool RestartAsAdministrator(bool restoringDefaults)
		{
			if (Elevation.IsElevated || (startupState != null && startupState.ElevationAttempted))
			{
				// Administrator rights are not what is missing, so restarting again would only earn
				// the same refusal.  Something else owns the key, such as a group policy.
				ReportRegistryFailure("Unable to set all registry values.  While your change may have "
					+ "worked, some values could not be written even with administrator permission.");
				return false;
			}
			if (elevationRefused)
				return false;

			StartupState state = new StartupState();
			state.RestoreDefaults = restoringDefaults;
			state.AntialiasingEnabled = cbFontAntialiasing.Checked;
			state.PixelStructure = SelectedPixelStructure;
			state.GdiContrast = (int)nudGdiContrast.Value;
			state.GammaLevel = (int)nudDwContrast.Value;
			state.ClearTypeLevel = (int)nudClearTypeLevel.Value;
			state.EnhancedContrastLevel = (int)nudEnhancedContrast.Value;
			state.DarkMode = cbDarkmode.Checked;
			state.Maximized = this.WindowState == FormWindowState.Maximized;
			// RestoreBounds is the size the window would go back to, which is the one worth carrying
			// over while it is maximized or minimized.
			state.WindowBounds = this.WindowState == FormWindowState.Normal ? this.Bounds : this.RestoreBounds;

			string error;
			ElevationResult result = Elevation.RestartElevated(state.ToArguments(), out error);
			if (result == ElevationResult.Started)
				return true;

			elevationRefused = true;
			MessageBox.Show(this,
				(result == ElevationResult.Refused
					? "Administrator permission is needed to change this setting for every application on this computer, and it was not granted."
					: "This application was unable to restart itself as an administrator: " + error)
				+ "\r\n\r\nYour change has been applied everywhere it could be.  To apply it everywhere, "
				+ "run Better ClearType Tuner as an administrator.",
				"Administrator permission required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}

		private void ReportRegistryFailure(string message)
		{
			if (registryFailReported)
				return;
			registryFailReported = true;
			MessageBox.Show(this, message, "Better ClearType Tuner", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		#endregion
		#region Helpers
		private string[] GetDisplayNames()
		{
			return Screen.AllScreens
				.Select(s =>
				{
					int idxStart = s.DeviceName.LastIndexOf('\\');
					if (idxStart < 0)
						idxStart = 0;
					else
						idxStart++;
					return s.DeviceName.Substring(idxStart);
				})
				.ToArray();
		}
		private void UpdateStatus()
		{
			dirty = false;
			if (InvokeRequired)
				Invoke((Action)UpdateStatus);
			else
			{
				// Read font settings
				bool aaEnabled = FontSmoothing.GetAntialiasingEnabled();
				FontSmoothingOrientation orientation = FontSmoothing.GetFontSmoothingOrientation();
				FontSmoothingType smoothingType = FontSmoothing.GetFontSmoothingType();
				uint gdiContrast = FontSmoothing.GetContrast();
				int gammaLevel = GetAvalonValue("GammaLevel", (int)GammaLevelDefault);
				int clearTypeLevel = GetAvalonValue("ClearTypeLevel", (int)ClearTypeLevelDefault);
				int enhancedContrastLevel = GetAvalonValue("EnhancedContrastLevel", (int)EnhancedContrastLevelDefault);

				// Update UI controls
				DisableEvents();

				cbFontAntialiasing.Checked = aaEnabled;

				bool clearTypeSelected = false;
				if (smoothingType == FontSmoothingType.Standard)
					rbGrayscale.Checked = true;
				else if (orientation == FontSmoothingOrientation.RGB)
					clearTypeSelected = rbRGB.Checked = true;
				else if (orientation == FontSmoothingOrientation.BGR)
					clearTypeSelected = rbBGR.Checked = true;
				else
					rbGrayscale.Checked = rbRGB.Checked = rbBGR.Checked = false;

				ShowValue(nudGdiContrast, (int)gdiContrast, FontSmoothing.ContrastMin, FontSmoothing.ContrastMax);
				ShowValue(nudDwContrast, gammaLevel, GammaLevelMin, GammaLevelMax);
				ShowValue(nudEnhancedContrast, enhancedContrastLevel, EnhancedContrastLevelMin, EnhancedContrastLevelMax);
				// Grayscale mode stores a ClearType Level of 0, which is not the user's choice of
				// level but a consequence of the mode, so only read it back when it is in use.
				if (aaEnabled && clearTypeSelected)
					ShowValue(nudClearTypeLevel, clearTypeLevel, ClearTypeLevelMin, ClearTypeLevelMax);

				ApplyEnabledStates();

				EnableEvents();

				string quick = "The Wizard's lily box. ";
				// Update status text
				if (!aaEnabled)
					status.Text = quick + "Font Antialiasing is disabled.";
				else if (smoothingType == FontSmoothingType.ClearType)
					status.Text = quick + orientation
						+ "  ·  GDI contrast " + gdiContrast
						+ "  ·  DirectWrite contrast " + gammaLevel
						+ ", ClearType Level " + clearTypeLevel
						+ ", enhanced contrast " + enhancedContrastLevel;
				else
					status.Text = quick + "Grayscale"
						+ "  ·  DirectWrite contrast " + gammaLevel
						+ ", enhanced contrast " + enhancedContrastLevel;

				// Snapshot the sample text and render it zoomed-in
				CopyZoomedSnapshot();
			}
		}

		private uint Clamp(uint val, uint minimum, uint maximum)
		{
			if (val > maximum)
				val = maximum;
			if (val < minimum)
				val = minimum;
			return val;
		}

		/// <summary>
		/// Puts a value that was read back from the system into its input box, in red if it is
		/// outside the range that Windows documents for that setting.  The box itself may not be
		/// able to show such a value, but the color still says that something else set it.
		/// </summary>
		private void ShowValue(NumericUpDown nud, int value, uint documentedMin, uint documentedMax)
		{
			decimal shown = value;
			if (shown < nud.Minimum)
				shown = nud.Minimum;
			else if (shown > nud.Maximum)
				shown = nud.Maximum;
			nud.Value = shown;
			nud.ForeColor = (value < documentedMin || value > documentedMax) ? Color.Red : InputTextColor;
		}

		/// <summary>
		/// Grays out every setting that the current antialiasing mode makes irrelevant, so that
		/// the window only offers changes which will actually alter the rendered text.
		/// </summary>
		private void ApplyEnabledStates()
		{
			// With antialiasing off, text is drawn with hard pixel edges and nothing below the
			// checkbox reaches either renderer.
			bool aaEnabled = cbFontAntialiasing.Checked;
			bool clearType = aaEnabled && (rbRGB.Checked || rbBGR.Checked);

			rbGrayscale.Enabled = rbRGB.Enabled = rbBGR.Enabled = aaEnabled;

			// GDI applies its contrast only while drawing ClearType; grayscale GDI text ignores it.
			SetRowEnabled(clearType, nudGdiContrast, lblGdiContrast, lblGdiContrastRange, lblGdiHeader);
			// Both DirectWrite contrast controls also act on grayscale antialiasing.
			SetRowEnabled(aaEnabled, nudDwContrast, lblDwContrast, lblDwContrastRange, lblDwHeader);
			SetRowEnabled(aaEnabled, nudEnhancedContrast, lblEnhancedContrast, lblEnhancedContrastRange);
			// ClearType Level is the subpixel blend amount, so it means nothing outside RGB/BGR.
			SetRowEnabled(clearType, nudClearTypeLevel, lblClearTypeLevel, lblClearTypeLevelRange);
		}

		private void SetRowEnabled(bool enabled, NumericUpDown nud, params Label[] labels)
		{
			nud.Enabled = enabled;
			foreach (Label label in labels)
				label.ForeColor = enabled ? LabelTextColor : DisabledTextColor;
		}

		/// <summary>Color that <see cref="SetDarkMode"/> gives ordinary label text.</summary>
		private Color LabelTextColor
		{
			get { return cbDarkmode.Checked ? ColorTranslator.FromHtml("#DEDEDE") : Color.Black; }
		}
		/// <summary>Color for the labels of a setting that has no effect in the current mode.</summary>
		private Color DisabledTextColor
		{
			get { return cbDarkmode.Checked ? ColorTranslator.FromHtml("#6B6B6B") : SystemColors.GrayText; }
		}
		/// <summary>Color that <see cref="SetDarkMode"/> gives text inside an input box.</summary>
		private Color InputTextColor
		{
			get { return cbDarkmode.Checked ? ColorTranslator.FromHtml("#DEDEDE") : Color.Black; }
		}

		private void CopyZoomedSnapshot()
		{
			this.Invalidate(true);
			//foreach (Control control in this.Controls)
			//	control.Invalidate();
			//.Invalidate();
			using (Bitmap src = new Bitmap(panelSmall.Width, panelSmall.Height))
			{
				panelSmall.DrawToBitmap(src, new Rectangle(0, 0, panelSmall.Width, panelSmall.Height));
				Image old = pbZoomed.Image;
				pbZoomed.Image = ScaleFast(src, 4);
				old?.Dispose();
			}
			RenderDirectWritePreview();
		}

		/// <summary>
		/// Draws the same sample text through DirectWrite. Unlike the GDI preview above it,
		/// this reflects the ClearType Level, and it reads the settings straight from the
		/// controls rather than from the registry, so it updates without restarting anything.
		/// </summary>
		private void RenderDirectWritePreview()
		{
			if (dwRenderer == null)
				return;
			if (!dwRenderer.Available)
			{
				ShowDirectWriteError(dwRenderer.LastError);
				return;
			}

			Font[] fonts = new Font[] { lblSample1.Font, lblSample2.Font, lblSample3.Font };
			string[] texts = new string[] { lblSample1.Text, lblSample2.Text, lblSample3.Text };

			DirectWriteSampleRenderer.Settings settings = new DirectWriteSampleRenderer.Settings
			{
				AntialiasingEnabled = cbFontAntialiasing.Checked,
				SmoothingType = rbGrayscale.Checked ? FontSmoothingType.Standard : FontSmoothingType.ClearType,
				Orientation = rbBGR.Checked ? FontSmoothingOrientation.BGR : FontSmoothingOrientation.RGB,
				GammaLevel = (uint)nudDwContrast.Value,
				ClearTypeLevel = rbGrayscale.Checked ? 0 : (int)nudClearTypeLevel.Value,
				EnhancedContrastLevel = (int)nudEnhancedContrast.Value
			};

			Bitmap rendered = dwRenderer.Render(pbDwSmall.Width, pbDwSmall.Height, fonts, texts,
				this.DeviceDpi, TextColorForSamples(), BackColorForSamples(), settings);
			if (rendered == null)
			{
				ShowDirectWriteError(dwRenderer.LastError);
				return;
			}

			Image oldZoomed = pbDwZoomed.Image;
			pbDwZoomed.Image = ScaleFast(rendered, 4);
			oldZoomed?.Dispose();

			Image oldSmall = pbDwSmall.Image;
			pbDwSmall.Image = rendered;
			oldSmall?.Dispose();

			lblDwZoomHeader.Text = dwZoomHeaderText;
		}

		private void ShowDirectWriteError(string message)
		{
			Image oldZoomed = pbDwZoomed.Image;
			pbDwZoomed.Image = null;
			oldZoomed?.Dispose();

			Image oldSmall = pbDwSmall.Image;
			pbDwSmall.Image = null;
			oldSmall?.Dispose();

			lblDwZoomHeader.Text = "DirectWrite preview unavailable: " + (message ?? "unknown error");
		}

		private Color TextColorForSamples()
		{
			return cbDarkmode.Checked ? Color.White : Color.Black;
		}

		private Color BackColorForSamples()
		{
			return cbDarkmode.Checked ? Color.Black : Color.White;
		}
		private Bitmap ScaleFast(Bitmap src, double scale)
		{
			int targetW = (int)(src.Width * scale);
			int targetH = (int)(src.Height * scale);
			int[] targetPixels = resizePixels(GetRawRGB(src), src.Width, src.Height, targetW, targetH);

			Bitmap target = new Bitmap(targetW, targetH, PixelFormat.Format32bppRgb);
			BitmapData targetData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
			Marshal.Copy(targetPixels, 0, targetData.Scan0, targetPixels.Length);
			target.UnlockBits(targetData);
			return target;
		}
		private int[] GetRawRGB(Bitmap bmp)
		{
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
			int[] raw = new int[data.Width * data.Height];
			Marshal.Copy(data.Scan0, raw, 0, raw.Length);
			bmp.UnlockBits(data);
			return raw;
		}

		public int[] resizePixels(int[] pixels, int w1, int h1, int w2, int h2)
		{
			int[] temp = new int[w2 * h2];
			double x_ratio = w1 / (double)w2;
			double y_ratio = h1 / (double)h2;
			double px, py;
			for (int i = 0; i < h2; i++)
			{
				for (int j = 0; j < w2; j++)
				{
					px = Math.Floor(j * x_ratio);
					py = Math.Floor(i * y_ratio);
					temp[(i * w2) + j] = pixels[(int)((py * w1) + px)];
				}
			}
			return temp;
		}
		public void DisableEvents()
		{
			cbFontAntialiasing.CheckedChanged -= ControlsChanged;
			rbGrayscale.CheckedChanged -= ControlsChanged;
			rbRGB.CheckedChanged -= ControlsChanged;
			rbBGR.CheckedChanged -= ControlsChanged;
			nudGdiContrast.ValueChanged -= ControlsChanged;
			nudDwContrast.ValueChanged -= ControlsChanged;
			nudClearTypeLevel.ValueChanged -= ControlsChanged;
			nudEnhancedContrast.ValueChanged -= ControlsChanged;
		}
		public void EnableEvents()
		{
			cbFontAntialiasing.CheckedChanged += ControlsChanged;
			rbGrayscale.CheckedChanged += ControlsChanged;
			rbRGB.CheckedChanged += ControlsChanged;
			rbBGR.CheckedChanged += ControlsChanged;
			nudGdiContrast.ValueChanged += ControlsChanged;
			nudDwContrast.ValueChanged += ControlsChanged;
			nudClearTypeLevel.ValueChanged += ControlsChanged;
			nudEnhancedContrast.ValueChanged += ControlsChanged;
		}
		private static bool PrefersDarkMode()
		{
			try
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
				if (key == null)
					return false;
				object value = key.GetValue("AppsUseLightTheme");
				if (value == null)
					return false;
				if (int.TryParse(value.ToString(), out int v) && v == 0)
					return true;
			}
			catch
			{
			}
			return false;
		}
		/// <summary>
		/// Gets the Dpi Scale as reported by the operating system.  For font scaling purposes, this must be divided by the DPI scale at the time of application launch.
		/// </summary>
		private double OSReportedDpiScale
		{
			get
			{
				return this.LogicalToDeviceUnits(10000) / 10000.0;
			}
		}
		/// <summary>
		/// Gets the current DPI scale as a floating-point number, where 1.0 is 100% scale.
		/// </summary>
		private double CurrentFontScale
		{
			get
			{
				return OSReportedDpiScale / InitialDpiScale;
			}
		}
		private double InitialDpiScale = 1;
		private void InitializeDpiScale()
		{
			InitialDpiScale = OSReportedDpiScale;
			if (InitialDpiScale <= 0)
				InitialDpiScale = 1;
		}
		private void FixFontSizing()
		{
			double fontScale = CurrentFontScale;
			foreach (Control c in fontableControls)
			{
				c.Font = new Font(c.Font.Name, (float)(baselineFontSizes[c.Name] * fontScale), c.Font.Style, c.Font.Unit);
			}
		}
		#endregion

		#region Per-setting help
		/// <summary>
		/// Note appended to every DirectWrite setting, which the preview applies live but other
		/// applications only read once.
		/// </summary>
		private const string DirectWriteRestartNote =
			"The DirectWrite preview updates immediately.  Applications such as Firefox, Edge and "
			+ "WPF read this value when they start, so they have to be restarted to pick it up.";

		private const string HelpGdiContrast =
			"GDI contrast - SystemParametersInfo, SPI_SETFONTSMOOTHINGCONTRAST\r\n"
			+ "Range 1000 to 2200, default 1200.  Higher numbers give lighter text.\r\n"
			+ "\r\n"
			+ "GDI applies this only while it is drawing ClearType, so it has no effect in "
			+ "grayscale mode.  DirectWrite applications ignore it entirely; they have their own "
			+ "contrast setting below.";

		private const string HelpDwContrast =
			"DirectWrite contrast - HKCU and HKLM\\" + AvalonKeyPath + "\\<display>\\GammaLevel\r\n"
			+ "Range 1000 to 2200, default 1900.  Higher numbers give lighter text.\r\n"
			+ "\r\n"
			+ "This is DirectWrite's gamma.  Unlike the GDI contrast it applies to grayscale "
			+ "antialiasing as well as to ClearType.  GDI ignores it.\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		private const string HelpClearTypeLevel =
			"ClearType Level - HKCU\\" + AvalonKeyPath + "\\<display>\\ClearTypeLevel\r\n"
			+ "Range 0 to 100, default 100.\r\n"
			+ "\r\n"
			+ "How much of the antialiasing is done with the display's individual color subpixels "
			+ "rather than with whole gray pixels.  Lower it to reduce color fringing at the cost "
			+ "of sharpness.  It therefore only means anything in the RGB and BGR modes, and GDI "
			+ "ignores it in all of them.  This is the same setting as the color-intensity step of "
			+ "the Windows ClearType tuner.\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		private const string HelpEnhancedContrast =
			"Enhanced Contrast - HKCU\\" + AvalonKeyPath + "\\<display>\\EnhancedContrastLevel\r\n"
			+ "Range 0 to 400, default 50.  Higher numbers give darker text.\r\n"
			+ "\r\n"
			+ "A second contrast control, applied on top of the DirectWrite contrast above and "
			+ "pulling in the opposite direction.  It applies to grayscale antialiasing as well as "
			+ "to ClearType.  GDI ignores it, and DirectWrite discards values above 400.\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		/// <summary>
		/// Hangs the help text off the inputs as tooltips.  The [?] links show the same text in a
		/// dialog, for anyone who does not think to hover.
		/// </summary>
		private void InitializeHelpText()
		{
			SetHelpText(HelpGdiContrast, nudGdiContrast, lblGdiContrast, lblGdiContrastRange, linkGdiContrast);
			SetHelpText(HelpDwContrast, nudDwContrast, lblDwContrast, lblDwContrastRange, linkDwContrast);
			SetHelpText(HelpClearTypeLevel, nudClearTypeLevel, lblClearTypeLevel, lblClearTypeLevelRange, linkClearTypeLevel);
			SetHelpText(HelpEnhancedContrast, nudEnhancedContrast, lblEnhancedContrast, lblEnhancedContrastRange, linkEnhancedContrast);
		}

		private void SetHelpText(string text, params Control[] controls)
		{
			foreach (Control control in controls)
				toolTip1.SetToolTip(control, text);
		}

		private void ShowHelp(LinkLabel link, string title, string text)
		{
			link.LinkVisited = true;
			MessageBox.Show(this, text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void linkGdiContrast_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelp(linkGdiContrast, "About GDI Contrast", HelpGdiContrast);
		}

		private void linkDwContrast_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelp(linkDwContrast, "About DirectWrite Contrast", HelpDwContrast);
		}

		private void linkClearTypeLevel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelp(linkClearTypeLevel, "About ClearType Level", HelpClearTypeLevel);
		}

		private void linkEnhancedContrast_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelp(linkEnhancedContrast, "About Enhanced Contrast", HelpEnhancedContrast);
		}
		#endregion
	}
}
