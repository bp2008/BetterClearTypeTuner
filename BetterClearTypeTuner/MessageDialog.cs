using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;

namespace BetterClearTypeTuner
{
	/// <summary>
	/// A stand-in for <see cref="MessageBox"/> that looks and behaves much like one, but can be
	/// resized and lets the user select the message text to copy it elsewhere.  That matters here
	/// because some of these dialogs carry help text worth quoting and others carry exception
	/// details worth pasting into a bug report.
	///
	/// The static Show methods mirror the <see cref="MessageBox.Show(string)"/> overloads, so a call
	/// site only has to swap the type name.
	/// </summary>
	public partial class MessageDialog : Form
	{
		/// <summary>
		/// The buttons along the bottom, in left to right order.
		/// </summary>
		private readonly List<Button> dialogButtons = new List<Button>();
		/// <summary>
		/// Every measurement in this dialog is a multiple of the message font's line height, which
		/// makes the layout follow the font and the display scaling without any DPI arithmetic.
		/// </summary>
		private int unit = 15;
		/// <summary>
		/// The icon shown to the left of the message, already sized for this dialog, or null when
		/// <see cref="MessageBoxIcon.None"/> was asked for.  Owned by this form.
		/// </summary>
		private Icon messageIcon;
		/// <summary>
		/// Set only when this form created its own font and therefore has to dispose of it.  A font
		/// borrowed from the owner window belongs to that window.
		/// </summary>
		private Font ownedFont;
		/// <summary>
		/// Where the icon is drawn, in <see cref="panelMessage"/> coordinates.
		/// </summary>
		private Rectangle iconBounds;
		/// <summary>
		/// How small the user is allowed to drag the window, as a client size.  Turned into a window
		/// size in OnLoad, once the border thickness is known.
		/// </summary>
		private Size minimumClientSize;

		#region Show
		public static DialogResult Show(string text)
		{
			return ShowCore(null, text, null, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption)
		{
			return ShowCore(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
		{
			return ShowCore(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return ShowCore(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return ShowCore(null, text, caption, buttons, icon, defaultButton);
		}

		public static DialogResult Show(IWin32Window owner, string text)
		{
			return ShowCore(owner, text, null, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption)
		{
			return ShowCore(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
		{
			return ShowCore(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return ShowCore(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return ShowCore(owner, text, caption, buttons, icon, defaultButton);
		}

		/// <summary>
		/// Shows the dialog without the alert sound.  For a message the user asked to see, such as
		/// help text behind a link, there is nothing to alert them to.
		/// </summary>
		public static DialogResult ShowQuiet(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return ShowCore(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, false);
		}

		private static DialogResult ShowCore(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, bool playSound = true)
		{
			Form ownerForm = FindOwnerForm(owner);
			using (MessageDialog dialog = new MessageDialog(ownerForm, text, caption, buttons, icon, defaultButton))
			{
				if (playSound)
					PlayAlertSound(icon);
				if (owner != null)
					return dialog.ShowDialog(owner);
				return dialog.ShowDialog();
			}
		}

		/// <summary>
		/// Finds the form the dialog should take its size, position and colors from.  When no owner
		/// was named, the active form stands in, which is what ShowDialog() would pick as the owner
		/// anyway.
		/// </summary>
		private static Form FindOwnerForm(IWin32Window owner)
		{
			if (owner == null)
				return Form.ActiveForm;
			Form ownerForm = owner as Form;
			if (ownerForm != null)
				return ownerForm;
			Control control = Control.FromHandle(owner.Handle);
			return control != null ? control.FindForm() : Form.ActiveForm;
		}

		/// <summary>
		/// Plays the sound that a MessageBox with this icon would have played.
		/// </summary>
		private static void PlayAlertSound(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Error: SystemSounds.Hand.Play(); break;
				case MessageBoxIcon.Question: SystemSounds.Question.Play(); break;
				case MessageBoxIcon.Warning: SystemSounds.Exclamation.Play(); break;
				case MessageBoxIcon.Information: SystemSounds.Asterisk.Play(); break;
				default: SystemSounds.Beep.Play(); break;
			}
		}
		#endregion

		/// <summary>
		/// Parameterless constructor for the Windows Forms designer, which cannot instantiate a form
		/// that only offers a constructor with arguments.
		/// </summary>
		public MessageDialog() : this(null, "", null, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1)
		{
		}

		private MessageDialog(Form ownerForm, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			InitializeComponent();

			// Borrowing the owner's font keeps this dialog in step with the main window, including the
			// per-monitor font scaling that window applies to itself.
			if (ownerForm != null)
			{
				this.Font = ownerForm.Font;
			}
			else
			{
				ownedFont = SystemFonts.MessageBoxFont;
				this.Font = ownedFont;
			}
			unit = Math.Max(8, this.Font.Height);

			// A MessageBox with no caption shows an empty title bar, so an empty caption stays empty.
			this.Text = caption ?? "";
			txtMessage.Text = text ?? "";
			txtMessage.SelectionStart = 0;
			txtMessage.SelectionLength = 0;

			CreateIcon(icon);
			CreateButtons(buttons, defaultButton);
			ApplyMetrics();
			ApplyTheme(ownerForm);
			SizeToContent(ownerForm);
		}

		#region Layout
		/// <summary>
		/// Sets every padding and control size from <see cref="unit"/>.  The numbers are picked to
		/// land on the familiar MessageBox spacing at 100% scaling.
		/// </summary>
		private void ApplyMetrics()
		{
			int pad = unit;
			int gap = unit * 4 / 5;
			int iconSize = messageIcon != null ? messageIcon.Width : 0;

			iconBounds = new Rectangle(pad, pad, iconSize, iconSize);
			panelMessage.Padding = new Padding(
				pad + (iconSize > 0 ? iconSize + gap : 0),
				pad,
				pad,
				pad);

			int strip = unit * 2 / 3;
			flowButtons.Padding = new Padding(strip);
			panelButtons.Height = ButtonHeight + strip * 2;
		}

		private int ButtonHeight
		{
			get { return unit * 5 / 3; }
		}

		/// <summary>
		/// The narrowest the button strip can be drawn without clipping a button.
		/// </summary>
		private int ButtonStripWidth
		{
			get
			{
				int width = flowButtons.Padding.Horizontal;
				foreach (Button button in dialogButtons)
					width += button.Width + button.Margin.Horizontal;
				return width;
			}
		}

		/// <summary>
		/// Picks a window size that shows the whole message where that is reasonable, and lets the
		/// message scroll where it is not.  The user can drag the window to any size afterward.
		/// </summary>
		private void SizeToContent(Form ownerForm)
		{
			Rectangle workingArea = (ownerForm != null ? Screen.FromControl(ownerForm) : Screen.PrimaryScreen).WorkingArea;

			int minTextWidth = unit * 12;
			int maxTextWidth = Math.Min(unit * 34, Math.Max(minTextWidth, workingArea.Width / 2));
			Size measured = TextRenderer.MeasureText(txtMessage.Text, this.Font,
				new Size(maxTextWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);

			// A word longer than the wrap width, which a stack trace tends to have, measures wider
			// than the limit that was handed in.  The rich text box breaks it, so the limit stands.
			int textWidth = Math.Max(minTextWidth, Math.Min(measured.Width + unit / 2, maxTextWidth));

			// The scroll bar only appears when the text does not fit, so leaving room for it here is
			// what keeps it from appearing at the size this method picks.
			int contentWidth = textWidth + SystemInformation.VerticalScrollBarWidth;
			// Measured again at the width the text will really have, because the first pass wrapped
			// it into a narrower column and so counted lines that are not going to be there.
			int textHeight = TextRenderer.MeasureText(txtMessage.Text, this.Font,
				new Size(contentWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height
				+ unit / 2;

			int clientWidth = panelMessage.Padding.Horizontal + contentWidth;
			int clientHeight = panelMessage.Padding.Vertical
				+ Math.Max(textHeight, iconBounds.Height)
				+ panelButtons.Height;

			minimumClientSize = new Size(
				Math.Max(ButtonStripWidth, unit * 14),
				panelButtons.Height + unit * 3);

			clientWidth = Clamp(clientWidth, minimumClientSize.Width, workingArea.Width * 3 / 4);
			clientHeight = Clamp(clientHeight, minimumClientSize.Height, workingArea.Height * 3 / 4);
			this.ClientSize = new Size(clientWidth, clientHeight);

			if (ownerForm == null)
				this.StartPosition = FormStartPosition.CenterScreen;
		}

		private static int Clamp(int value, int min, int max)
		{
			if (max < min)
				max = min;
			return Math.Max(min, Math.Min(max, value));
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// The border thickness is only known once the window exists, and MinimumSize is a window
			// size rather than a client size.
			this.MinimumSize = new Size(
				minimumClientSize.Width + (this.Width - this.ClientSize.Width),
				minimumClientSize.Height + (this.Height - this.ClientSize.Height));
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			// The message box is out of the tab order, so without this the first button would be
			// focused but not shown as the default until the user pressed a key.
			if (this.AcceptButton is Button)
				((Button)this.AcceptButton).Focus();
		}

		private void panelMessage_Paint(object sender, PaintEventArgs e)
		{
			if (messageIcon == null)
				return;
			// Windows may not have the icon at the exact size this dialog wants, in which case
			// DrawIcon scales it, so ask for the better of the two available filters.
			InterpolationMode previous = e.Graphics.InterpolationMode;
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.DrawIcon(messageIcon, iconBounds);
			e.Graphics.InterpolationMode = previous;
		}
		#endregion

		#region Icon and buttons
		private void CreateIcon(MessageBoxIcon icon)
		{
			Icon source;
			switch (icon)
			{
				case MessageBoxIcon.Error: source = SystemIcons.Error; break;
				case MessageBoxIcon.Question: source = SystemIcons.Question; break;
				case MessageBoxIcon.Warning: source = SystemIcons.Warning; break;
				case MessageBoxIcon.Information: source = SystemIcons.Information; break;
				default: return;
			}
			int size = unit * 2;
			try
			{
				// Picks whichever size the system icon actually contains that is closest to the one
				// asked for; panelMessage_Paint scales from there if they differ.
				messageIcon = new Icon(source, size, size);
			}
			catch (Exception)
			{
				messageIcon = (Icon)source.Clone();
			}
		}

		/// <summary>
		/// Builds the same button set, in the same order, that a MessageBox would have shown.
		/// </summary>
		private void CreateButtons(MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
		{
			Button cancelButton = null;
			switch (buttons)
			{
				case MessageBoxButtons.OKCancel:
					AddButton("OK", DialogResult.OK);
					cancelButton = AddButton("Cancel", DialogResult.Cancel);
					break;
				case MessageBoxButtons.AbortRetryIgnore:
					AddButton("&Abort", DialogResult.Abort);
					AddButton("&Retry", DialogResult.Retry);
					AddButton("&Ignore", DialogResult.Ignore);
					break;
				case MessageBoxButtons.YesNoCancel:
					AddButton("&Yes", DialogResult.Yes);
					AddButton("&No", DialogResult.No);
					cancelButton = AddButton("Cancel", DialogResult.Cancel);
					break;
				case MessageBoxButtons.YesNo:
					AddButton("&Yes", DialogResult.Yes);
					AddButton("&No", DialogResult.No);
					break;
				case MessageBoxButtons.RetryCancel:
					AddButton("&Retry", DialogResult.Retry);
					cancelButton = AddButton("Cancel", DialogResult.Cancel);
					break;
				default:
					// A MessageBox with only an OK button answers OK to Escape and to the close box.
					cancelButton = AddButton("OK", DialogResult.OK);
					break;
			}

			// The flow panel fills from the right, so the rightmost button is the one to add first.
			for (int i = dialogButtons.Count - 1; i >= 0; i--)
				flowButtons.Controls.Add(dialogButtons[i]);

			int defaultIndex = (int)defaultButton / 256;
			if (defaultIndex < 0 || defaultIndex >= dialogButtons.Count)
				defaultIndex = 0;
			this.AcceptButton = dialogButtons[defaultIndex];

			this.CancelButton = cancelButton;
			// Without a way to cancel, there is nothing for the close box to return, so it goes away
			// just as it does on a Yes/No MessageBox.
			this.ControlBox = cancelButton != null;
		}

		private Button AddButton(string text, DialogResult result)
		{
			Button button = new Button();
			button.Text = text;
			button.DialogResult = result;
			button.AutoSize = false;
			button.Margin = new Padding(unit / 3, 0, 0, 0);
			button.TabIndex = dialogButtons.Count;
			button.UseVisualStyleBackColor = true;
			button.Size = new Size(
				Math.Max(unit * 5, TextRenderer.MeasureText(text, this.Font).Width + unit * 2),
				ButtonHeight);
			dialogButtons.Add(button);
			return button;
		}
		#endregion

		#region Theme
		/// <summary>
		/// Takes the owner window's colors, so that the dialog is dark when the application is dark.
		/// In the light theme the owner's colors are the system colors a MessageBox would have used
		/// anyway, so this changes nothing there.
		/// </summary>
		private void ApplyTheme(Form ownerForm)
		{
			Color back = ownerForm != null ? ownerForm.BackColor : SystemColors.Control;
			Color fore = ownerForm != null ? ownerForm.ForeColor : SystemColors.ControlText;
			bool dark = IsDark(back);

			this.BackColor = back;
			this.ForeColor = fore;
			panelButtons.BackColor = back;
			flowButtons.BackColor = back;

			// A MessageBox separates the message from the buttons by putting the message on the
			// window background and the buttons on the control background.
			Color messageBack = dark ? Lighten(back, 13) : SystemColors.Window;
			panelMessage.BackColor = messageBack;
			txtMessage.BackColor = messageBack;
			txtMessage.ForeColor = dark ? ColorTranslator.FromHtml("#DEDEDE") : SystemColors.WindowText;

			// Only the dark theme touches the buttons.  A light theme button is wanted exactly as
			// Windows draws it, and that is what UseVisualStyleBackColor gets; a button still drawn
			// that way would ignore the dark BackColor below.
			if (dark)
			{
				foreach (Button button in dialogButtons)
				{
					button.UseVisualStyleBackColor = false;
					button.BackColor = Color.Black;
					button.ForeColor = ColorTranslator.FromHtml("#DEDEDE");
				}
			}
		}

		private static bool IsDark(Color color)
		{
			return (color.R * 299 + color.G * 587 + color.B * 114) / 1000 < 128;
		}

		private static Color Lighten(Color color, int amount)
		{
			return Color.FromArgb(
				Math.Min(255, color.R + amount),
				Math.Min(255, color.G + amount),
				Math.Min(255, color.B + amount));
		}
		#endregion

		#region Copying
		/// <summary>
		/// The menu item shortcuts only reach the message box while it has the focus, and it starts
		/// out without it.  This catches the same two keystrokes for the rest of the dialog, so that
		/// Ctrl+C copies the message the way it does on a MessageBox.
		/// </summary>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (!txtMessage.Focused)
			{
				if (keyData == (Keys.Control | Keys.C))
				{
					CopyMessage();
					return true;
				}
				if (keyData == (Keys.Control | Keys.A))
				{
					SelectAllMessage();
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void menuCopy_Click(object sender, EventArgs e)
		{
			CopyMessage();
		}

		private void menuSelectAll_Click(object sender, EventArgs e)
		{
			SelectAllMessage();
		}

		private void CopyMessage()
		{
			// With nothing selected this copies the whole message, which is what Ctrl+C on a
			// MessageBox does and the only sensible answer for a keystroke that arrived from a button.
			string text = txtMessage.SelectionLength > 0 ? txtMessage.SelectedText : txtMessage.Text;
			if (string.IsNullOrEmpty(text))
				return;
			// A rich text box hands back bare line feeds, which some Windows applications paste as one
			// long line.  The message arrived with CRLF line breaks, so it leaves that way too.
			text = text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");
			try
			{
				Clipboard.SetText(text);
			}
			catch (Exception)
			{
				// Another process can have the clipboard open, and failing to copy is not worth a
				// second dialog on top of this one.
			}
		}

		private void SelectAllMessage()
		{
			txtMessage.Focus();
			txtMessage.SelectAll();
		}
		#endregion

		private void DisposeOwnedResources()
		{
			if (messageIcon != null)
			{
				messageIcon.Dispose();
				messageIcon = null;
			}
			if (ownedFont != null)
			{
				ownedFont.Dispose();
				ownedFont = null;
			}
		}
	}
}
