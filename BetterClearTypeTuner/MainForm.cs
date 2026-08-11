using BetterClearTypeTuner.Native;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
		/// Captions for the two DirectWrite previews, restored after an error has been displayed
		/// in their place.
		/// </summary>
		string dwZoomHeaderText;
		string dwGrayZoomHeaderText;
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

			// Taken before anything below can move a control, so these are the designer's own
			// positions after Windows Forms has scaled them for the current DPI.
			CapturePreviewGridDesign();

			dwZoomHeaderText = lblDwZoomHeader.Text;
			dwGrayZoomHeaderText = lblDwGrayZoomHeader.Text;
			dwRenderer = new DirectWriteSampleRenderer();

			previewResizeTimer = new Timer();
			previewResizeTimer.Interval = 150;
			previewResizeTimer.Tick += PreviewResizeTimer_Tick;
			panelContent.SizeChanged += PanelContent_SizeChanged;

			InitializeDpiScale();
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
			LayoutPreviewGrid();
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
			cbDwOverride.Checked = startupState.DwOverride;
			ShowValue(nudGdiContrast, startupState.GdiContrast, FontSmoothing.ContrastMin, FontSmoothing.ContrastMax);
			ShowValue(nudDwContrast, startupState.GammaLevel, GammaLevelMin, GammaLevelMax);
			ShowValue(nudClearTypeLevel, startupState.ClearTypeLevel, ClearTypeLevelMin, ClearTypeLevelMax);
			ShowValue(nudEnhancedContrast, startupState.EnhancedContrastLevel, EnhancedContrastLevelMin, EnhancedContrastLevelMax);
			ShowValue(nudGrayscaleContrast, startupState.GrayscaleEnhancedContrastLevel, EnhancedContrastLevelMin, EnhancedContrastLevelMax);
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
			else if (control.Name == "lblBusy")
			{
				// Coloured rather than left to the ordinary label treatment: this one has to be
				// noticeable at a glance, and it is the only thing on screen that moves.
				if (dark)
				{
					control.BackColor = ColorTranslator.FromHtml("#4A3A00");
					control.ForeColor = ColorTranslator.FromHtml("#FFD666");
				}
				else
				{
					control.BackColor = ColorTranslator.FromHtml("#FFF4CE");
					control.ForeColor = ColorTranslator.FromHtml("#7A5D00");
				}
			}
			else if (control.Name == "panelNormalScale")
			{
				// Only the gaps between the three sample boxes show, so this has to match the
				// window rather than the samples.
				control.BackColor = BackgroundColor;
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

		#region Busy indicator
		/// <summary>
		/// How many nested operations are currently in progress.  One user action routinely starts
		/// several - clicking Apply runs a change pass and then a refresh, and picking an
		/// antialiasing mode unchecks the previous radio button as well as checking the new one -
		/// so the indicator is put away by the outermost one rather than the first to finish.
		/// </summary>
		private int busyDepth;

		/// <summary>
		/// Shows that something is happening before anything slow starts.
		///
		/// All of this work runs on the UI thread: writing the registry, broadcasting the change to
		/// every top-level window on the desktop with SPIF_SENDCHANGE, and then redrawing three
		/// samples.  The window cannot repaint while any of that is going on, so the indicator has
		/// to be painted before it begins rather than left to the next paint cycle - hence the
		/// Refresh, which is the whole point of this method.
		/// </summary>
		private void BeginBusy()
		{
			if (busyDepth++ > 0)
				return;
			if (!IsHandleCreated)
				return;
			lblBusy.Visible = true;
			lblBusy.Refresh();
			this.UseWaitCursor = true;
		}

		private void EndBusy()
		{
			if (busyDepth > 0)
				busyDepth--;
			if (busyDepth > 0)
				return;
			if (!IsHandleCreated)
				return;
			this.UseWaitCursor = false;
			lblBusy.Visible = false;
			lblBusy.Update();
		}
		#endregion

		private void ControlsChanged(object sender, EventArgs e)
		{
			BeginBusy();
			try
			{
				ControlsChangedCore(sender, e);
			}
			finally
			{
				EndBusy();
			}
		}

		private void ControlsChangedCore(object sender, EventArgs e)
		{
			bool restoringDefaults = setDefaults;
			setDefaults = false;
			if (initialized && !restartingElevated)
			{
				registryAccessDenied = false;
				if (restoringDefaults || AvalonValuesDiffer())
					dirty = true;

				// Restoring defaults means the clean-installation state, which is the keys absent.
				SetAvalonKeys(restoringDefaults || !cbDwOverride.Checked);
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
		/// ClearType Level, written exactly as it is set.
		///
		/// Earlier versions forced this to zero in grayscale mode, on the grounds that zero is what
		/// stops DirectWrite blending across subpixels.  It is also what made the setting look dead:
		/// the box was showing one number while a different one was being written, so moving it
		/// changed nothing and said nothing about why.  Grayscale is conveyed by PixelStructure
		/// instead, which is the value that actually removes the subpixel structure, and the two
		/// DirectWrite previews now show directly what any combination of the two does.
		/// </summary>
		private int DesiredClearTypeLevel
		{
			get { return (int)Clamp((uint)nudClearTypeLevel.Value, ClearTypeLevelMin, ClearTypeLevelMax); }
		}
		/// <summary>
		/// DirectWrite contrast (GammaLevel).
		/// </summary>
		private int DesiredGammaLevel
		{
			get { return (int)Clamp((uint)nudDwContrast.Value, GammaLevelMin, GammaLevelMax); }
		}
		/// <summary>
		/// DirectWrite enhanced contrast (EnhancedContrastLevel), read only by the ClearType path.
		/// </summary>
		private int DesiredEnhancedContrastLevel
		{
			get { return (int)Clamp((uint)nudEnhancedContrast.Value, EnhancedContrastLevelMin, EnhancedContrastLevelMax); }
		}
		/// <summary>
		/// DirectWrite grayscale enhanced contrast (GrayscaleEnhancedContrastLevel), read only by
		/// the grayscale path.
		/// </summary>
		private int DesiredGrayscaleEnhancedContrastLevel
		{
			get { return (int)Clamp((uint)nudGrayscaleContrast.Value, EnhancedContrastLevelMin, EnhancedContrastLevelMax); }
		}
		/// <summary>
		/// Returns true if any Avalon.Graphics value that this application manages is not
		/// already what the controls ask for.
		/// </summary>
		private bool AvalonValuesDiffer()
		{
			// Whether the keys are there at all is itself one of the settings, and the one that
			// decides whether any of the others are worth comparing.
			if (AvalonKeysExist() != cbDwOverride.Checked)
				return true;
			if (!cbDwOverride.Checked)
				return false;

			return GetAvalonValue("PixelStructure", -1) != DesiredPixelStructure
				|| GetAvalonValue("ClearTypeLevel", FallbackClearTypeLevel) != DesiredClearTypeLevel
				|| GetAvalonValue("GammaLevel", FallbackGammaLevel) != DesiredGammaLevel
				|| GetAvalonValue("EnhancedContrastLevel", FallbackEnhancedContrastLevel) != DesiredEnhancedContrastLevel
				|| GetAvalonValue("GrayscaleEnhancedContrastLevel", FallbackGrayscaleEnhancedContrastLevel)
					!= DesiredGrayscaleEnhancedContrastLevel;
		}
		#endregion

		/// <summary>
		/// Returns true if either hive holds a per-display subkey with at least one value in it,
		/// which is what "these settings are in force" amounts to.
		///
		/// A key with subkeys but no values does not count, and that is not a guess: asked what it
		/// resolves to with the display subkeys present but empty, DirectWrite gives exactly the
		/// answer it gives when the whole key is missing.  So an emptied out key - which is what a
		/// tuner that clears its values without removing its keys leaves behind - is reported here
		/// the same way DirectWrite treats it, as no override at all.
		/// </summary>
		private bool AvalonKeysExist()
		{
			return HasDisplayValues(Registry.CurrentUser) || HasDisplayValues(Registry.LocalMachine);
		}

		private bool HasDisplayValues(RegistryKey baseKey)
		{
			try
			{
				using (RegistryKey key = baseKey.OpenSubKey(AvalonKeyPath, false))
				{
					if (key == null)
						return false;
					foreach (string subkeyName in key.GetSubKeyNames())
					{
						using (RegistryKey display = key.OpenSubKey(subkeyName, false))
						{
							if (display != null && display.GetValueNames().Length > 0)
								return true;
						}
					}
				}
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// Writes the Avalon.Graphics values, or removes them entirely when
		/// <paramref name="removeKeys"/> is set - which is what both the unticked override checkbox
		/// and the Restore Defaults button ask for.
		/// </summary>
		private void SetAvalonKeys(bool removeKeys)
		{
			// Whatever this pass does, it changes what DirectWrite would resolve these values to, so
			// the measurement taken from it no longer speaks for the current state.
			InvalidateDirectWriteDefaults();

			if (removeKeys)
			{
				// The only operation in this application that can still need administrator rights.
				// Nothing writes the machine-wide key any more, but an older version of this
				// program or another tuner may have left one, and "no override" has to mean the
				// whole key is gone rather than only this user's half of it.  Tested for first so
				// that the usual case - no machine-wide key at all - does not open it for writing
				// and raise a prompt for a deletion with nothing to delete.
				if (HasDisplayValues(Registry.LocalMachine))
					DeleteRegistryKeyTree(Registry.LocalMachine, AvalonKeyPath);
				DeleteRegistryKeyTree(Registry.CurrentUser, AvalonKeyPath);
				// A refused deletion is not something the framework reports - see
				// DeleteRegistryKeyTree - so whether the keys are gone is established by looking
				// for them rather than by nothing having gone wrong.  Without this, a key that
				// this instance is not allowed to remove leaves the override checkbox ticking
				// itself straight back on, with no prompt for the rights that would fix it.
				//
				// The answer replaces anything the two calls above reported rather than adding to
				// it, so that a failure which turns out not to have left any setting behind does
				// not ask the user for administrator rights it has no use for.
				registryAccessDenied = AvalonKeysExist();
				return;
			}

			foreach (string displayName in GetDisplayNames())
			{
				string keyPath = AvalonKeyPath + "\\" + displayName;

				// Current User only, and that is on purpose.
				//
				// Earlier versions also wrote GammaLevel and PixelStructure under HKEY_LOCAL_MACHINE.
				// Opening that hive for writing is refused without administrator rights whether or
				// not the value would change, so every single settings change raised a UAC prompt.
				// It bought nothing: asked what it resolves to with the machine-wide key absent
				// entirely, DirectWrite answers with the Current User values exactly, through both
				// CreateRenderingParams and CreateMonitorRenderingParams.  The full set is written
				// here, so there is no value left for a machine-wide one to supply.
				//
				// The trade is that these settings are now this user's rather than the computer's.
				// For a tool that tunes text to one pair of eyes on one set of monitors, per-user is
				// the right scope anyway - and it is the scope that does not need a prompt.
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "ClearTypeLevel", DesiredClearTypeLevel);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "EnhancedContrastLevel", DesiredEnhancedContrastLevel);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "GammaLevel", DesiredGammaLevel);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "PixelStructure", DesiredPixelStructure);
				SetRegistryDWORDValue(Registry.CurrentUser, keyPath, "GrayscaleEnhancedContrastLevel",
					DesiredGrayscaleEnhancedContrastLevel);
				// A WPF 3.5-era value with no known reader in modern Windows.  Not exposed, but
				// written at its documented default so that a value left behind by another tuner is
				// put back rather than left in place.
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
		/// <summary>
		/// Ticking the box does not need to seed the inputs with anything: while it was unticked
		/// they were already displaying the values DirectWrite falls back to, so writing exactly
		/// what is on screen is what makes turning the override on leave the text looking the same.
		/// </summary>
		private void cbDwOverride_CheckedChanged(object sender, EventArgs e)
		{
			BeginBusy();
			try
			{
				ControlsChanged(sender, e);
				// Unticking removes the keys, after which the inputs should stop showing the values
				// that were in them and start showing what DirectWrite substitutes instead.
				// ControlsChanged only refreshes the display when it decided something changed, so
				// make sure of it.
				if (!restartingElevated)
					UpdateStatus();
			}
			finally
			{
				EndBusy();
			}
		}

		private void BtnRestoreDefaults_Click(object sender, EventArgs e)
		{
			BeginBusy();
			try
			{
				RestoreDefaultsCore(sender, e);
			}
			finally
			{
				EndBusy();
			}
		}

		private void RestoreDefaultsCore(object sender, EventArgs e)
		{
			DisableEvents();
			setDefaults = true;
			cbFontAntialiasing.Checked = true;
			rbRGB.Checked = true;
			// The clean-installation state is the Avalon.Graphics keys absent, so Restore Defaults
			// turns the override off.  The values below are only what the inputs fall back to if
			// DirectWrite cannot be asked; UpdateStatus replaces them with measured ones.
			cbDwOverride.Checked = false;
			nudGdiContrast.Value = FontSmoothing.ContrastDefault;
			nudDwContrast.Value = GammaLevelDefault;
			nudClearTypeLevel.Value = ClearTypeLevelDefault;
			nudEnhancedContrast.Value = EnhancedContrastLevelDefault;
			nudGrayscaleContrast.Value = GrayscaleEnhancedContrastLevelDefault;
			EnableEvents();
			ControlsChanged(sender, e);
			// ControlsChanged only refreshes the display when it wrote something, and a machine which
			// is already at its defaults - the registry keys absent and the system parameters
			// untouched - gives it nothing to write, which would leave this click looking ignored.
			if (!restartingElevated)
				UpdateStatus();
		}

		private void btnApply_Click(object sender, EventArgs e)
		{
			BeginBusy();
			try
			{
				// Nothing may have changed, in which case ControlsChanged writes nothing and leaves
				// the previews alone, so refresh them here to show that the click was noticed.
				ControlsChanged(sender, e);
				UpdateStatus();
			}
			finally
			{
				EndBusy();
			}
		}

		private void btnChangeFont_Click(object sender, EventArgs e)
		{
			// Outside the busy scope: the indicator is for work the user is waiting on, and while
			// the font dialog is open they are not waiting on this window at all.
			if (fontDialog1.ShowDialog() != DialogResult.OK)
				return;
			BeginBusy();
			try
			{
				foreach (Control c in fontableControls)
				{
					c.Font = new Font(fontDialog1.Font.Name, c.Font.Size, c.Font.Style, c.Font.Unit);
				}
				CopyZoomedSnapshot();
			}
			finally
			{
				EndBusy();
			}
		}

		private void cbDarkmode_CheckedChanged(object sender, EventArgs e)
		{
			BeginBusy();
			try
			{
				SetDarkMode(this, cbDarkmode.Checked);
			}
			finally
			{
				EndBusy();
			}
		}

		private void MainForm_DpiChanged(object sender, DpiChangedEventArgs e)
		{
			// Windows Forms rescales the controls, but the preview grid's minimum sizes were
			// measured at the old scale and have to be moved with them.
			if (e.DeviceDpiOld > 0)
				ScalePreviewGridDesign(e.DeviceDpiNew / (double)e.DeviceDpiOld);
			LayoutPreviewGrid();
			FixFontSizing();
			SetTimeout.OnGui(CopyZoomedSnapshot, 100, this, ex => MessageDialog.Show(ex.ToString()));
			SetTimeout.OnGui(CopyZoomedSnapshot, 500, this, ex => MessageDialog.Show(ex.ToString()));
			SetTimeout.OnGui(CopyZoomedSnapshot, 1000, this, ex => MessageDialog.Show(ex.ToString()));
		}

		#region Registry
		/// <summary>
		/// Where DirectWrite's per-display tuning values live.  Written under HKCU only; HKLM is
		/// still looked at, so that a machine-wide key left by another tuner is noticed and can be
		/// removed, but never written.  See <see cref="SetAvalonKeys"/>.
		/// </summary>
		public const string AvalonKeyPath = "Software\\Microsoft\\Avalon.Graphics";
		/// <summary>
		/// DirectWrite/WPF ClearType amount (0 = grayscale … 100 = full). Same key as cttune.
		/// Inert while PixelStructure is flat, which is what grayscale mode writes.
		/// </summary>
		public const uint ClearTypeLevelMin = 0;
		public const uint ClearTypeLevelMax = 100;
		public const uint ClearTypeLevelDefault = 100;
		/// <summary>
		/// DirectWrite contrast (GammaLevel).  Higher numbers give lighter text.  Unlike the GDI
		/// contrast this also applies to grayscale antialiasing.
		///
		/// Microsoft documents the default as 1900, but that is not the number DirectWrite uses.
		/// Asked what it resolves to with the Avalon.Graphics values absent - the state of a clean
		/// Windows installation - it answers with a gamma of 1.8, which is a GammaLevel of 1800.
		/// This constant is only a last resort for a machine where DirectWrite cannot be reached at
		/// all; everywhere else the value is measured rather than assumed.  See
		/// <see cref="DirectWriteDefaults"/>.
		/// </summary>
		public const uint GammaLevelMin = 1000;
		public const uint GammaLevelMax = 2200;
		public const uint GammaLevelDefault = 1800;
		/// <summary>
		/// DirectWrite's second contrast control (EnhancedContrastLevel).  Higher numbers give
		/// darker text.  Read only when DirectWrite is rasterizing ClearType.
		/// DirectWrite ignores values above 400 entirely.
		/// </summary>
		public const uint EnhancedContrastLevelMin = 0;
		public const uint EnhancedContrastLevelMax = 400;
		public const uint EnhancedContrastLevelDefault = 50;
		/// <summary>
		/// The same control for grayscale rasterization (GrayscaleEnhancedContrastLevel), which is
		/// a separate value read by a separate code path.  Same units and same range; the default
		/// is 100 rather than 50.
		/// </summary>
		public const uint GrayscaleEnhancedContrastLevelDefault = 100;

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
				// Closed rather than left to the finalizer: a handle still open on one of these
				// keys is a handle the deletion pass has to contend with when the override is
				// switched back off.
				using (RegistryKey key = baseKey.CreateSubKey(keyPath))
				{
					if (key == null)
					{
						registryAccessDenied = true;
						return;
					}
					key.SetValue(name, value, RegistryValueKind.DWord);
				}
			}
			catch (SecurityException)
			{
				registryAccessDenied = true;
			}
			catch (UnauthorizedAccessException)
			{
				registryAccessDenied = true;
			}
			catch (IOException)
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
				// UpdateStatus reads several of these per refresh, and every one of them left open
				// is another handle on a key this application may shortly be asked to delete.
				using (RegistryKey key = baseKey.OpenSubKey(keyPath, false))
				{
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
			}
			catch
			{
			}
			return defaultValue;
		}
		#region What DirectWrite falls back to
		/// <summary>
		/// The last measurement taken from DirectWrite, and whether it succeeded.  Measuring means
		/// creating a DirectWrite factory, which is too expensive to repeat for every keystroke in
		/// an input box, so the result is cached until something invalidates it.
		/// </summary>
		DirectWriteDefaults dwDefaults;
		bool dwDefaultsValid;
		bool dwDefaultsMeasured;

		/// <summary>
		/// Discards the cached measurement.  Called after the Avalon.Graphics values are written or
		/// deleted, because that is precisely what the measurement depends on.
		/// </summary>
		private void InvalidateDirectWriteDefaults()
		{
			dwDefaultsMeasured = false;
		}

		/// <summary>
		/// Takes the measurement if the cached one has been invalidated.  Returns false if
		/// DirectWrite could not be reached, in which case the cached values mean nothing.
		/// </summary>
		private bool EnsureDwDefaultsMeasured()
		{
			if (!dwDefaultsMeasured)
			{
				dwDefaultsValid = DirectWriteDefaults.TryMeasure(out dwDefaults);
				dwDefaultsMeasured = true;
			}
			return dwDefaultsValid;
		}

		/// <summary>
		/// What a missing Avalon.Graphics value actually resolves to, which is what the window
		/// should display rather than the value Microsoft documents as the default.  On a clean
		/// Windows installation none of these values exist and DirectWrite substitutes fallbacks of
		/// its own - a gamma of 1.8, not the documented 1900 - so displaying the documented number
		/// would claim a setting that is not in force.  The documented defaults are used only if
		/// DirectWrite could not be reached at all.
		/// </summary>
		private int FallbackGammaLevel
		{
			get { return EnsureDwDefaultsMeasured() ? dwDefaults.GammaLevel : (int)GammaLevelDefault; }
		}
		private int FallbackClearTypeLevel
		{
			get { return EnsureDwDefaultsMeasured() ? dwDefaults.ClearTypeLevel : (int)ClearTypeLevelDefault; }
		}
		private int FallbackEnhancedContrastLevel
		{
			get { return EnsureDwDefaultsMeasured() ? dwDefaults.EnhancedContrastLevel : (int)EnhancedContrastLevelDefault; }
		}
		private int FallbackGrayscaleEnhancedContrastLevel
		{
			get
			{
				return EnsureDwDefaultsMeasured()
					? dwDefaults.GrayscaleEnhancedContrastLevel
					: (int)GrayscaleEnhancedContrastLevelDefault;
			}
		}
		#endregion

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
		/// <summary>
		/// Removes a key and everything beneath it, doing nothing if it is not there.  The key itself
		/// has to go and not just its contents: a clean Windows installation has no Avalon.Graphics
		/// key at all, and at least one consumer decides whether the ClearType tuner has ever been run
		/// by testing whether the key exists, so an emptied out key does not read as untuned.
		///
		/// Failure here is not reliably an exception.  DeleteSubKeyTree opens the key for writing
		/// first, and the internal open it uses hands back a plain null when Windows refuses - the
		/// same null it gives for a key that is not there - so with throwOnMissingSubKey off the
		/// refusal is read as "already gone" and the call returns quietly having deleted nothing.
		/// That is why the caller checks afterwards whether the keys are actually gone instead of
		/// trusting a clean return.  The catches below still matter: a refusal met further down the
		/// tree, once the key has been opened, does throw.
		/// </summary>
		private void DeleteRegistryKeyTree(RegistryKey baseKey, string keyPath)
		{
			try
			{
				baseKey.DeleteSubKeyTree(keyPath, false);
			}
			catch (SecurityException)
			{
				registryAccessDenied = true;
			}
			catch (UnauthorizedAccessException)
			{
				registryAccessDenied = true;
			}
			catch (IOException)
			{
				// Every registry error that is not a permission problem arrives as this, including
				// the one for a key that another handle is holding open.  Letting it out of here
				// would take down the application from inside a control's event handler.
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
				ReportRegistryFailure("Unable to change all registry values.  While your change may have "
					+ "worked, some values could not be written or removed even with administrator "
					+ "permission.  Something other than permissions is holding them, such as a group policy.");
				return false;
			}
			if (elevationRefused)
				return false;

			StartupState state = new StartupState();
			state.RestoreDefaults = restoringDefaults;
			state.AntialiasingEnabled = cbFontAntialiasing.Checked;
			state.PixelStructure = SelectedPixelStructure;
			state.GdiContrast = (int)nudGdiContrast.Value;
			state.DwOverride = cbDwOverride.Checked;
			state.GammaLevel = (int)nudDwContrast.Value;
			state.ClearTypeLevel = (int)nudClearTypeLevel.Value;
			state.EnhancedContrastLevel = (int)nudEnhancedContrast.Value;
			state.GrayscaleEnhancedContrastLevel = (int)nudGrayscaleContrast.Value;
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
			MessageDialog.Show(this,
				(result == ElevationResult.Refused
					? "Administrator permission is needed to remove text rendering settings that apply to every user of this computer, and it was not granted."
					: "This application was unable to restart itself as an administrator: " + error)
				+ "\r\n\r\nYour own settings have been changed successfully.  What is left behind is a "
				+ "machine-wide copy under HKEY_LOCAL_MACHINE, written by an older version of this "
				+ "program or by another tuner.  It does not affect how text looks for you, because "
				+ "your own settings take precedence over it, but it will keep applying to any other "
				+ "user of this computer who has none of their own.  To clear it out, run Better "
				+ "ClearType Tuner as an administrator.",
				"Administrator permission required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}

		private void ReportRegistryFailure(string message)
		{
			if (registryFailReported)
				return;
			registryFailReported = true;
			MessageDialog.Show(this, message, "Better ClearType Tuner", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
				bool avalonKeysExist = AvalonKeysExist();
				// Where a value is absent from the registry, show what DirectWrite actually
				// substitutes for it rather than the value Microsoft documents as the default.
				int gammaLevel = GetAvalonValue("GammaLevel", FallbackGammaLevel);
				int clearTypeLevel = GetAvalonValue("ClearTypeLevel", FallbackClearTypeLevel);
				int enhancedContrastLevel = GetAvalonValue("EnhancedContrastLevel", FallbackEnhancedContrastLevel);
				int grayscaleContrastLevel = GetAvalonValue("GrayscaleEnhancedContrastLevel", FallbackGrayscaleEnhancedContrastLevel);

				// Update UI controls
				DisableEvents();

				cbFontAntialiasing.Checked = aaEnabled;
				cbDwOverride.Checked = avalonKeysExist;

				if (smoothingType == FontSmoothingType.Standard)
					rbGrayscale.Checked = true;
				else if (orientation == FontSmoothingOrientation.RGB)
					rbRGB.Checked = true;
				else if (orientation == FontSmoothingOrientation.BGR)
					rbBGR.Checked = true;
				else
					rbGrayscale.Checked = rbRGB.Checked = rbBGR.Checked = false;

				ShowValue(nudGdiContrast, (int)gdiContrast, FontSmoothing.ContrastMin, FontSmoothing.ContrastMax);
				ShowValue(nudDwContrast, gammaLevel, GammaLevelMin, GammaLevelMax);
				ShowValue(nudEnhancedContrast, enhancedContrastLevel, EnhancedContrastLevelMin, EnhancedContrastLevelMax);
				ShowValue(nudGrayscaleContrast, grayscaleContrastLevel, EnhancedContrastLevelMin, EnhancedContrastLevelMax);
				ShowValue(nudClearTypeLevel, clearTypeLevel, ClearTypeLevelMin, ClearTypeLevelMax);

				ApplyEnabledStates();

				EnableEvents();

				// Says that the DirectWrite figures which follow are the ones DirectWrite fell back
				// to rather than ones anybody chose, so that they are not read as settings in force.
				string dwSource = avalonKeysExist ? "DirectWrite" : "DirectWrite defaults:";
				// Update status text
				if (!aaEnabled)
					status.Text = "Font Antialiasing is disabled.";
				else
					status.Text = (smoothingType == FontSmoothingType.ClearType ? orientation.ToString() : "Grayscale")
						+ "  ·  GDI contrast " + gdiContrast
						+ "  ·  " + dwSource + " contrast " + gammaLevel
						+ ", ClearType Level " + clearTypeLevel
						+ ", enhanced contrast " + enhancedContrastLevel
						+ ", grayscale " + grayscaleContrastLevel;

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
		/// Grays out the settings that nothing can currently read.
		///
		/// That is a much shorter list than it used to be, and deliberately so.  Earlier versions
		/// also grayed out a setting whenever the antialiasing mode above made it look irrelevant -
		/// ClearType Level outside RGB/BGR, the GDI contrast in grayscale mode - which quietly
		/// taught the wrong lesson.  The antialiasing mode is not a mode the whole computer is in.
		/// It commands GDI, and it is a hint the DirectWrite applications that bother to read it may
		/// take; a WinUI application rasterizes grayscale whatever this window says, and a GDI
		/// application can ask for ClearType per font.  So a setting the current mode does not use
		/// is still a setting some running application does, and graying it out only made those
		/// applications look unaffected by a value that was never being written.
		///
		/// What remains is the two cases where nothing anywhere is reading the value: antialiasing
		/// switched off entirely, and the DirectWrite override switched off, where the boxes are
		/// reporting what DirectWrite falls back to rather than offering anything to change.
		/// </summary>
		private void ApplyEnabledStates()
		{
			// With antialiasing off, text is drawn with hard pixel edges and nothing below the
			// checkbox reaches either renderer.
			bool aaEnabled = cbFontAntialiasing.Checked;
			bool dwOverride = aaEnabled && cbDwOverride.Checked;

			rbGrayscale.Enabled = rbRGB.Enabled = rbBGR.Enabled = aaEnabled;
			cbDwOverride.Enabled = aaEnabled;
			// The section headings follow the antialiasing switch alone.  Turning the override off
			// does not stop DirectWrite from drawing this application's text, it only means the
			// settings under the heading are being reported rather than chosen.
			lblDwHeader.ForeColor = aaEnabled ? LabelTextColor : DisabledTextColor;
			lblDwClearTypePath.ForeColor = dwOverride ? LabelTextColor : DisabledTextColor;
			lblDwGrayscalePath.ForeColor = dwOverride ? LabelTextColor : DisabledTextColor;

			SetRowEnabled(aaEnabled, nudGdiContrast, lblGdiContrast, lblGdiContrastRange, lblGdiHeader);
			SetRowEnabled(dwOverride, nudDwContrast, lblDwContrast, lblDwContrastRange);
			SetRowEnabled(dwOverride, nudClearTypeLevel, lblClearTypeLevel, lblClearTypeLevelRange);
			SetRowEnabled(dwOverride, nudEnhancedContrast, lblEnhancedContrast, lblEnhancedContrastRange);
			SetRowEnabled(dwOverride, nudGrayscaleContrast, lblGrayscaleContrast, lblGrayscaleContrastRange);
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

		#region Preview grid layout
		/// <summary>
		/// The designer's own geometry for the four preview quadrants, captured once at the DPI the
		/// window came up at.  <see cref="LayoutPreviewGrid"/> never lays the grid out smaller than
		/// this, so widening the window grows the quadrants and narrowing it scrolls instead.
		/// </summary>
		private int designGridLeft, designGridTop, designHeaderHeight, designHeaderGap;
		private int designColumnWidth, designRowHeight, designColumnGap, designRowGap;
		private int designRightMargin, designBottomMargin, designLaneInset, designLaneGap;
		private bool previewGridCaptured;
		/// <summary>
		/// Redrawing three samples is far too much work to do for every intermediate size Windows
		/// reports while a window is being dragged, so the boxes are moved at once and their
		/// contents are redrawn when the dragging stops.
		/// </summary>
		private System.Windows.Forms.Timer previewResizeTimer;

		private void CapturePreviewGridDesign()
		{
			designGridLeft = lblNormalScaleHeader.Left;
			designGridTop = lblNormalScaleHeader.Top;
			designHeaderHeight = lblNormalScaleHeader.Height;
			designHeaderGap = panelNormalScale.Top - lblNormalScaleHeader.Top;
			designColumnWidth = pbZoomed.Width;
			designRowHeight = pbZoomed.Height;
			designColumnGap = pbZoomed.Left - (panelNormalScale.Left + panelNormalScale.Width);
			designRowGap = lblDwZoomHeader.Top - (panelNormalScale.Top + panelNormalScale.Height);
			designLaneInset = panelNormalScale.Width - panelSmall.Width;
			designLaneGap = panelSmall.Top - (lblGdiSmallHeader.Top + lblGdiSmallHeader.Height);
			// Falling back to the column gap keeps these in scaled units on the off chance that the
			// content panel has not been given its size yet.
			designRightMargin = Math.Max(designColumnGap, panelContent.Width - (pbZoomed.Left + pbZoomed.Width));
			designBottomMargin = Math.Max(designColumnGap, panelContent.Height - (pbDwZoomed.Top + pbDwZoomed.Height));
			previewGridCaptured = true;
		}

		/// <summary>
		/// Moves the captured geometry to a new DPI.  Windows Forms rescales the controls itself,
		/// but the minimums above were measured at the old scale and would otherwise let the grid
		/// lay out smaller than the designer intended.
		/// </summary>
		private void ScalePreviewGridDesign(double factor)
		{
			if (!previewGridCaptured || factor <= 0 || factor == 1)
				return;
			designGridLeft = (int)Math.Round(designGridLeft * factor);
			designGridTop = (int)Math.Round(designGridTop * factor);
			designHeaderHeight = (int)Math.Round(designHeaderHeight * factor);
			designHeaderGap = (int)Math.Round(designHeaderGap * factor);
			designColumnWidth = (int)Math.Round(designColumnWidth * factor);
			designRowHeight = (int)Math.Round(designRowHeight * factor);
			designColumnGap = (int)Math.Round(designColumnGap * factor);
			designRowGap = (int)Math.Round(designRowGap * factor);
			designRightMargin = (int)Math.Round(designRightMargin * factor);
			designBottomMargin = (int)Math.Round(designBottomMargin * factor);
			designLaneInset = (int)Math.Round(designLaneInset * factor);
			designLaneGap = (int)Math.Round(designLaneGap * factor);
		}

		private void PanelContent_SizeChanged(object sender, EventArgs e)
		{
			LayoutPreviewGrid();
			if (initialized)
			{
				previewResizeTimer.Stop();
				previewResizeTimer.Start();
			}
		}

		private void PreviewResizeTimer_Tick(object sender, EventArgs e)
		{
			previewResizeTimer.Stop();
			BeginBusy();
			try
			{
				CopyZoomedSnapshot();
			}
			finally
			{
				EndBusy();
			}
		}

		/// <summary>
		/// Lays the four preview quadrants out across whatever width and height the window has.
		/// Both columns and both rows always get the same size as each other, so the two DirectWrite
		/// samples stay directly comparable.
		///
		/// The scrollbar allowance is subtracted whether or not a scrollbar is showing, and the
		/// measurement is taken from the panel's outer size rather than its client size.  Taken the
		/// other way round, growing a quadrant could bring a scrollbar in, which would shrink the
		/// client size, which would shrink the quadrant, which would send the scrollbar away again.
		/// </summary>
		private void LayoutPreviewGrid()
		{
			if (!previewGridCaptured)
				return;

			int usableWidth = panelContent.Width - SystemInformation.VerticalScrollBarWidth
				- designGridLeft - designRightMargin;
			int columnWidth = Math.Max(designColumnWidth, (usableWidth - designColumnGap) / 2);

			int usableHeight = panelContent.Height - SystemInformation.HorizontalScrollBarHeight
				- designGridTop - designBottomMargin - (2 * designHeaderGap) - designRowGap;
			int rowHeight = Math.Max(designRowHeight, usableHeight / 2);

			int leftColumn = designGridLeft;
			int rightColumn = leftColumn + columnWidth + designColumnGap;
			int topHeader = designGridTop;
			int topBox = topHeader + designHeaderGap;
			int bottomHeader = topBox + rowHeight + designRowGap;
			int bottomBox = bottomHeader + designHeaderGap;

			lblNormalScaleHeader.SetBounds(leftColumn, topHeader, columnWidth, designHeaderHeight);
			panelNormalScale.SetBounds(leftColumn, topBox, columnWidth, rowHeight);
			lblGdiZoomHeader.SetBounds(rightColumn, topHeader, columnWidth, designHeaderHeight);
			pbZoomed.SetBounds(rightColumn, topBox, columnWidth, rowHeight);
			lblDwZoomHeader.SetBounds(leftColumn, bottomHeader, columnWidth, designHeaderHeight);
			pbDwZoomed.SetBounds(leftColumn, bottomBox, columnWidth, rowHeight);
			lblDwGrayZoomHeader.SetBounds(rightColumn, bottomHeader, columnWidth, designHeaderHeight);
			pbDwGrayZoomed.SetBounds(rightColumn, bottomBox, columnWidth, rowHeight);

			LayoutNormalScaleLanes(columnWidth, rowHeight);
		}

		/// <summary>
		/// Divides the normal-scale quadrant equally between the three rendering paths.
		///
		/// Each of those boxes is also the source its zoom box magnifies, so the division has to
		/// leave every one of them at least a quarter of the quadrant's height for the magnified
		/// copy to fill the box beside it.  A third of the height less one caption clears that
		/// comfortably at any size this window can be given.
		/// </summary>
		private void LayoutNormalScaleLanes(int columnWidth, int rowHeight)
		{
			int laneHeight = rowHeight / 3;
			int boxWidth = Math.Max(1, columnWidth - designLaneInset);
			int boxHeight = Math.Max(1, laneHeight - designHeaderHeight - designLaneGap);

			LayoutLane(lblGdiSmallHeader, panelSmall, 0, laneHeight, columnWidth, boxWidth, boxHeight);
			LayoutLane(lblDwSmallHeader, pbDwSmall, laneHeight, laneHeight, columnWidth, boxWidth, boxHeight);
			LayoutLane(lblDwGraySmallHeader, pbDwGraySmall, 2 * laneHeight, laneHeight, columnWidth, boxWidth, boxHeight);
		}

		private void LayoutLane(Label header, Control box, int top, int laneHeight,
			int columnWidth, int boxWidth, int boxHeight)
		{
			header.SetBounds(0, top, columnWidth, designHeaderHeight);
			box.SetBounds(0, top + designHeaderHeight + designLaneGap, boxWidth, boxHeight);
		}
		#endregion

		/// <summary>
		/// Redraws all three previews: the GDI sample is snapshotted out of the live controls, and
		/// the two DirectWrite samples are drawn from scratch.  Each one is then magnified into the
		/// zoom box beside it.
		/// </summary>
		private void CopyZoomedSnapshot()
		{
			this.Invalidate(true);
			using (Bitmap src = new Bitmap(panelSmall.Width, panelSmall.Height))
			{
				panelSmall.DrawToBitmap(src, new Rectangle(0, 0, panelSmall.Width, panelSmall.Height));
				SetImage(pbZoomed, Magnify(src, ZoomFactor, pbZoomed.Width, pbZoomed.Height));
			}
			RenderDirectWritePreview();
		}

		/// <summary>How much the zoom boxes magnify their sample.</summary>
		private const int ZoomFactor = 4;

		/// <summary>
		/// Draws the sample text through both of DirectWrite's rasterization paths.  Unlike the GDI
		/// preview these read the settings straight from the controls rather than from the registry,
		/// so they update without writing anything and without restarting anything.
		///
		/// The two paths are drawn side by side rather than one being picked, because the choice
		/// belongs to each application rather than to the antialiasing mode: a WinUI application
		/// draws its text down the grayscale path no matter what this window is set to.
		/// </summary>
		private void RenderDirectWritePreview()
		{
			if (dwRenderer == null)
				return;
			if (!dwRenderer.Available)
			{
				ShowDirectWriteError(pbDwSmall, pbDwZoomed, lblDwZoomHeader, dwRenderer.LastError);
				ShowDirectWriteError(pbDwGraySmall, pbDwGrayZoomed, lblDwGrayZoomHeader, dwRenderer.LastError);
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
				ClearTypeLevel = (int)nudClearTypeLevel.Value,
				EnhancedContrastLevel = (int)nudEnhancedContrast.Value,
				GrayscaleEnhancedContrastLevel = (int)nudGrayscaleContrast.Value
			};

			RenderDirectWriteLane(DwPipeline.ClearType, settings, fonts, texts,
				pbDwSmall, pbDwZoomed, lblDwZoomHeader, dwZoomHeaderText);
			RenderDirectWriteLane(DwPipeline.Grayscale, settings, fonts, texts,
				pbDwGraySmall, pbDwGrayZoomed, lblDwGrayZoomHeader, dwGrayZoomHeaderText);
		}

		private void RenderDirectWriteLane(DwPipeline pipeline, DirectWriteSampleRenderer.Settings settings,
			Font[] fonts, string[] texts, PictureBox small, PictureBox zoomed, Label header, string headerText)
		{
			if (small.Width <= 0 || small.Height <= 0)
				return;

			Bitmap rendered = dwRenderer.Render(small.Width, small.Height, fonts, texts,
				this.DeviceDpi, TextColorForSamples(), BackColorForSamples(), settings, pipeline);
			if (rendered == null)
			{
				ShowDirectWriteError(small, zoomed, header, dwRenderer.LastError);
				return;
			}

			SetImage(zoomed, Magnify(rendered, ZoomFactor, zoomed.Width, zoomed.Height));
			SetImage(small, rendered);
			header.Text = headerText;
		}

		private void ShowDirectWriteError(PictureBox small, PictureBox zoomed, Label header, string message)
		{
			SetImage(zoomed, null);
			SetImage(small, null);
			header.Text = "Preview unavailable: " + (message ?? "unknown error");
		}

		/// <summary>Swaps in a new image and disposes the one it replaces.</summary>
		private static void SetImage(PictureBox box, Image image)
		{
			Image old = box.Image;
			box.Image = image;
			old?.Dispose();
		}

		private Color TextColorForSamples()
		{
			return cbDarkmode.Checked ? Color.White : Color.Black;
		}

		private Color BackColorForSamples()
		{
			return cbDarkmode.Checked ? Color.Black : Color.White;
		}
		/// <summary>
		/// Magnifies the top-left corner of <paramref name="src"/> by a whole-number factor, with
		/// each source pixel becoming a solid block so that nothing is blended and the individual
		/// subpixels stay visible.
		///
		/// The result is exactly the size asked for rather than the full magnification, which is
		/// what lets the zoom boxes be narrower than the sample they magnify: whatever runs off the
		/// right-hand edge is simply never drawn.  Anything past the end of the source is filled
		/// with the sample background so a box taller or wider than its source looks like blank
		/// paper rather than a black band.
		/// </summary>
		private Bitmap Magnify(Bitmap src, int scale, int targetW, int targetH)
		{
			if (targetW < 1)
				targetW = 1;
			if (targetH < 1)
				targetH = 1;

			int[] source = GetRawRGB(src);
			int[] target = new int[targetW * targetH];
			int fill = BackColorForSamples().ToArgb();

			for (int y = 0; y < targetH; y++)
			{
				int sy = y / scale;
				int row = y * targetW;
				if (sy >= src.Height)
				{
					for (int x = 0; x < targetW; x++)
						target[row + x] = fill;
					continue;
				}
				int sourceRow = sy * src.Width;
				for (int x = 0; x < targetW; x++)
				{
					int sx = x / scale;
					target[row + x] = sx < src.Width ? source[sourceRow + sx] : fill;
				}
			}

			Bitmap result = new Bitmap(targetW, targetH, PixelFormat.Format32bppRgb);
			BitmapData resultData = result.LockBits(new Rectangle(0, 0, targetW, targetH), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
			Marshal.Copy(target, 0, resultData.Scan0, target.Length);
			result.UnlockBits(resultData);
			return result;
		}
		private int[] GetRawRGB(Bitmap bmp)
		{
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
			int[] raw = new int[data.Width * data.Height];
			Marshal.Copy(data.Scan0, raw, 0, raw.Length);
			bmp.UnlockBits(data);
			return raw;
		}
		public void DisableEvents()
		{
			cbDwOverride.CheckedChanged -= cbDwOverride_CheckedChanged;
			cbFontAntialiasing.CheckedChanged -= ControlsChanged;
			rbGrayscale.CheckedChanged -= ControlsChanged;
			rbRGB.CheckedChanged -= ControlsChanged;
			rbBGR.CheckedChanged -= ControlsChanged;
			nudGdiContrast.ValueChanged -= ControlsChanged;
			nudDwContrast.ValueChanged -= ControlsChanged;
			nudClearTypeLevel.ValueChanged -= ControlsChanged;
			nudEnhancedContrast.ValueChanged -= ControlsChanged;
			nudGrayscaleContrast.ValueChanged -= ControlsChanged;
		}
		public void EnableEvents()
		{
			cbDwOverride.CheckedChanged += cbDwOverride_CheckedChanged;
			cbFontAntialiasing.CheckedChanged += ControlsChanged;
			rbGrayscale.CheckedChanged += ControlsChanged;
			rbRGB.CheckedChanged += ControlsChanged;
			rbBGR.CheckedChanged += ControlsChanged;
			nudGdiContrast.ValueChanged += ControlsChanged;
			nudDwContrast.ValueChanged += ControlsChanged;
			nudClearTypeLevel.ValueChanged += ControlsChanged;
			nudEnhancedContrast.ValueChanged += ControlsChanged;
			nudGrayscaleContrast.ValueChanged += ControlsChanged;
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
			"The DirectWrite previews update immediately.  Applications read this value when they "
			+ "start, so they have to be restarted to pick it up.";

		/// <summary>
		/// The one thing worth understanding about the DirectWrite settings, appended to each of the
		/// three that only one of the two paths reads.
		/// </summary>
		private const string DirectWritePathNote =
			"DirectWrite rasterizes glyphs in one of two ways, and each application picks for itself "
			+ "rather than being told by the antialiasing mode above:\r\n"
			+ "\r\n"
			+ "  ClearType path - computes coverage separately for the red, green and blue subpixels. "
			+ "Firefox, Edge, WPF and most applications that draw into a window of their own.\r\n"
			+ "\r\n"
			+ "  Grayscale path - computes one coverage value per pixel.  The Settings app, WinUI and "
			+ "Store apps, whose text is drawn onto composition surfaces that may be transparent, "
			+ "transformed or animated, and so cannot carry per-subpixel coverage.\r\n"
			+ "\r\n"
			+ "That is why both are previewed at once, and why a setting one path ignores is still "
			+ "worth setting: something on this computer is almost certainly using the other path.";

		private const string HelpGdiContrast =
			"GDI contrast - SystemParametersInfo, SPI_SETFONTSMOOTHINGCONTRAST\r\n"
			+ "Range 1000 to 2200, default 1200.  Higher numbers give lighter text.\r\n"
			+ "\r\n"
			+ "GDI reads this only while it is drawing ClearType.  That is normally decided by the "
			+ "antialiasing mode above, but an application can ask for ClearType for a particular "
			+ "font regardless of it, so this is left adjustable in grayscale mode too.\r\n"
			+ "\r\n"
			+ "DirectWrite applications ignore this value entirely; they have their own contrast "
			+ "settings below.";

		private const string HelpDwOverride =
			"Override DirectWrite defaults - HKCU\\" + AvalonKeyPath + "\r\n"
			+ "\r\n"
			+ "A clean Windows installation has no Avalon.Graphics registry key at all, and "
			+ "DirectWrite falls back to settings of its own.  Apps like Firefox may also override "
			+ "with their own different defaults. Clear this checkbox to return to that "
			+ "state: the key is removed and the settings below become "
			+ "read-only, showing what DirectWrite falls back to rather than anything chosen here.\r\n"
			+ "\r\n"
			+ "These settings are written for your Windows account only, which is why changing them "
			+ "needs no administrator permission.  DirectWrite reads a per-user value in preference "
			+ "to a machine-wide one, and this program writes the complete set, so there is nothing "
			+ "a machine-wide copy could add.  Clearing this checkbox will ask for administrator "
			+ "permission only if it finds a machine-wide key to remove, left by an older version of "
			+ "this program or by another tuner.\r\n"
			+ "\r\n"
			+ "Tick it to write the key and take control of those three settings.  They start at the "
			+ "values that were already in effect, so turning the override on does not by itself "
			+ "change how anything looks.\r\n"
			+ "\r\n"
			+ "It is deliberately all or nothing.  Which of these values DirectWrite honours is "
			+ "inconsistent enough that a half-written key produces rendering that matches neither "
			+ "the defaults nor the settings asked for.\r\n"
			+ "\r\n"
			+ "The RGB and BGR buttons above keep working either way.  With the key absent "
			+ "DirectWrite takes the subpixel order from the Windows font smoothing setting, and "
			+ "with it present the PixelStructure value written here says the same thing.\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		private const string HelpDwContrast =
			"DirectWrite contrast - HKCU\\" + AvalonKeyPath + "\\<display>\\GammaLevel\r\n"
			+ "Range 1000 to 2200.  Higher numbers give lighter text.\r\n"
			+ "\r\n"
			+ "This is DirectWrite's gamma, and the one DirectWrite setting that both rasterization "
			+ "paths read, so it is the setting to reach for when you want every DirectWrite "
			+ "application to change together.  GDI ignores it.\r\n"
			+ "\r\n"
			+ "Microsoft documents the default as 1900, but that is not what DirectWrite uses.  "
			+ "Asked what it resolves to with this value absent, DirectWrite answers with a gamma of "
			+ "1.8 - a GammaLevel of 1800 - so that is what this box shows while the override above "
			+ "is off, and what it starts from when the override is turned on.\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		private const string HelpClearTypeLevel =
			"ClearType Level - HKCU\\" + AvalonKeyPath + "\\<display>\\ClearTypeLevel\r\n"
			+ "Range 0 to 100, default 100.  ClearType path only.\r\n"
			+ "\r\n"
			+ "How much of the antialiasing is done with the display's individual color subpixels "
			+ "rather than with whole gray pixels.  Lower it to reduce color fringing at the cost of "
			+ "sharpness.  This is the same setting as the color-intensity step of the Windows "
			+ "ClearType tuner.  GDI ignores it.\r\n"
			+ "\r\n"
			+ "While Grayscale is selected at the top of this window, this setting does nothing at "
			+ "all - measurably nothing, not merely little.  That mode writes a PixelStructure of "
			+ "flat, and with no subpixel structure to blend across there is nothing for a ClearType "
			+ "level to scale: rendering at 0 and at 100 comes out pixel for pixel identical.  It is "
			+ "left adjustable anyway, because the value is still written and still read by anything "
			+ "that pairs it with a subpixel geometry of its own.\r\n"
			+ "\r\n"
			+ "Turning it down to zero is also not the same as grayscale rendering, however much it "
			+ "may look like it in the ClearType preview.  Zero collapses the ClearType output "
			+ "instead of switching paths, so what applies is still the ClearType contrast below and "
			+ "not the grayscale one.  Set the two previews side by side and you can see they do not "
			+ "match.\r\n"
			+ "\r\n"
			+ DirectWritePathNote + "\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		private const string HelpEnhancedContrast =
			"Enhanced Contrast - HKCU\\" + AvalonKeyPath + "\\<display>\\EnhancedContrastLevel\r\n"
			+ "Range 0 to 400, default 50.  Higher numbers give darker text.  ClearType path only.\r\n"
			+ "\r\n"
			+ "A second contrast control, applied on top of the DirectWrite contrast above and "
			+ "pulling in the opposite direction.  GDI ignores it, and DirectWrite discards values "
			+ "above 400.  Some applications also ignore it while still respecting the other "
			+ "DirectWrite settings.\r\n"
			+ "\r\n"
			+ "This is the ClearType path's copy of the setting.  Grayscale rasterization has its "
			+ "own, below, and reads that one instead.\r\n"
			+ "\r\n"
			+ DirectWritePathNote + "\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		private const string HelpGrayscaleContrast =
			"Grayscale Enhanced Contrast - HKCU\\" + AvalonKeyPath + "\\<display>\\GrayscaleEnhancedContrastLevel\r\n"
			+ "Range 0 to 400, default 100.  Higher numbers give darker text.  Grayscale path only.\r\n"
			+ "\r\n"
			+ "The same control as Enhanced Contrast above, kept as a separate value because "
			+ "grayscale rasterization is a separate code path with its own contrast.  Setting one "
			+ "does nothing to the other.\r\n"
			+ "\r\n"
			+ "This is the setting that reaches the Windows Settings app and other WinUI and Store "
			+ "applications, which rasterize grayscale no matter which antialiasing mode is selected "
			+ "at the top of this window.  Watch the grayscale preview while you change it; the "
			+ "ClearType preview will not move.\r\n"
			+ "\r\n"
			+ DirectWritePathNote + "\r\n"
			+ "\r\n"
			+ DirectWriteRestartNote;

		/// <summary>
		/// Shows one of the help texts in a dialog.  These are too long to hang off the inputs as
		/// tooltips without covering most of the window, so the [?] links are the only way to them.
		/// </summary>
		private void ShowHelp(LinkLabel link, string title, string text)
		{
			link.LinkVisited = true;
			MessageDialog.ShowQuiet(this, text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void linkGdiContrast_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelp(linkGdiContrast, "About GDI Contrast", HelpGdiContrast);
		}

		private void linkDwOverride_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelp(linkDwOverride, "About Overriding DirectWrite Defaults", HelpDwOverride);
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

		private void linkGrayscaleContrast_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelp(linkGrayscaleContrast, "About Grayscale Enhanced Contrast", HelpGrayscaleContrast);
		}
		#endregion
	}
}
