namespace BetterClearTypeTuner
{
	partial class MessageDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();
			base.Dispose(disposing);
			// After the base class is done with them, because the form was still drawing with these.
			if (disposing)
				DisposeOwnedResources();
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.panelMessage = new System.Windows.Forms.Panel();
			this.txtMessage = new System.Windows.Forms.RichTextBox();
			this.menuMessage = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.menuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.panelButtons = new System.Windows.Forms.Panel();
			this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.panelMessage.SuspendLayout();
			this.menuMessage.SuspendLayout();
			this.panelButtons.SuspendLayout();
			this.SuspendLayout();
			//
			// panelMessage
			//
			this.panelMessage.Controls.Add(this.txtMessage);
			this.panelMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMessage.Location = new System.Drawing.Point(0, 0);
			this.panelMessage.Name = "panelMessage";
			// The real padding is worked out at run time, because the left edge has to leave room for
			// the icon.  See ApplyMetrics().
			this.panelMessage.Padding = new System.Windows.Forms.Padding(12);
			this.panelMessage.Size = new System.Drawing.Size(404, 112);
			this.panelMessage.TabIndex = 0;
			this.panelMessage.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMessage_Paint);
			//
			// txtMessage
			//
			this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtMessage.ContextMenuStrip = this.menuMessage;
			this.txtMessage.DetectUrls = false;
			this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtMessage.Location = new System.Drawing.Point(12, 12);
			this.txtMessage.Name = "txtMessage";
			this.txtMessage.ReadOnly = true;
			this.txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.txtMessage.Size = new System.Drawing.Size(380, 88);
			this.txtMessage.TabIndex = 1;
			// Keeping the message box out of the tab order leaves the initial focus on the default
			// button, the way a MessageBox has it.  The text stays selectable with the mouse.
			this.txtMessage.TabStop = false;
			this.txtMessage.Text = "";
			//
			// menuMessage
			//
			this.menuMessage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.menuCopy,
			this.menuSelectAll});
			this.menuMessage.Name = "menuMessage";
			this.menuMessage.Size = new System.Drawing.Size(134, 48);
			//
			// menuCopy
			//
			this.menuCopy.Name = "menuCopy";
			this.menuCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.menuCopy.ShowShortcutKeys = true;
			this.menuCopy.Size = new System.Drawing.Size(133, 22);
			this.menuCopy.Text = "&Copy";
			this.menuCopy.Click += new System.EventHandler(this.menuCopy_Click);
			//
			// menuSelectAll
			//
			this.menuSelectAll.Name = "menuSelectAll";
			this.menuSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
			this.menuSelectAll.ShowShortcutKeys = true;
			this.menuSelectAll.Size = new System.Drawing.Size(133, 22);
			this.menuSelectAll.Text = "Select &All";
			this.menuSelectAll.Click += new System.EventHandler(this.menuSelectAll_Click);
			//
			// panelButtons
			//
			this.panelButtons.Controls.Add(this.flowButtons);
			this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelButtons.Location = new System.Drawing.Point(0, 112);
			this.panelButtons.Name = "panelButtons";
			this.panelButtons.Size = new System.Drawing.Size(404, 45);
			this.panelButtons.TabIndex = 1;
			//
			// flowButtons
			//
			this.flowButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowButtons.Location = new System.Drawing.Point(0, 0);
			this.flowButtons.Name = "flowButtons";
			this.flowButtons.Padding = new System.Windows.Forms.Padding(10);
			this.flowButtons.Size = new System.Drawing.Size(404, 45);
			this.flowButtons.TabIndex = 0;
			this.flowButtons.WrapContents = false;
			//
			// MessageDialog
			//
			// Every size in this form is computed from the message font at run time, so the automatic
			// scaling that would otherwise fight with it is turned off.
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(404, 157);
			this.Controls.Add(this.panelMessage);
			this.Controls.Add(this.panelButtons);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MessageDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "";
			this.panelMessage.ResumeLayout(false);
			this.menuMessage.ResumeLayout(false);
			this.panelButtons.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel panelMessage;
		private System.Windows.Forms.RichTextBox txtMessage;
		private System.Windows.Forms.ContextMenuStrip menuMessage;
		private System.Windows.Forms.ToolStripMenuItem menuCopy;
		private System.Windows.Forms.ToolStripMenuItem menuSelectAll;
		private System.Windows.Forms.Panel panelButtons;
		private System.Windows.Forms.FlowLayoutPanel flowButtons;
	}
}
