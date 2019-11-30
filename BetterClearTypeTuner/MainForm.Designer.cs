namespace BetterClearTypeTuner
{
	partial class MainForm
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.panelSmall = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.pbZoomed = new System.Windows.Forms.PictureBox();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.status = new System.Windows.Forms.ToolStripStatusLabel();
			this.cbFontAntialiasing = new System.Windows.Forms.CheckBox();
			this.rbGrayscale = new System.Windows.Forms.RadioButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.rbBGR = new System.Windows.Forms.RadioButton();
			this.rbRGB = new System.Windows.Forms.RadioButton();
			this.nudContrast = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.btnRestoreDefaults = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.btnSet = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.lblNotAdmin = new System.Windows.Forms.Label();
			this.panelSmall.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbZoomed)).BeginInit();
			this.statusStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudContrast)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(1, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(155, 31);
			this.label1.TabIndex = 0;
			this.label1.Text = "The quick brown fox jumps over the lazy dog.";
			// 
			// panelSmall
			// 
			this.panelSmall.BackColor = System.Drawing.Color.White;
			this.panelSmall.Controls.Add(this.label3);
			this.panelSmall.Controls.Add(this.label2);
			this.panelSmall.Controls.Add(this.label1);
			this.panelSmall.ForeColor = System.Drawing.Color.Black;
			this.panelSmall.Location = new System.Drawing.Point(12, 390);
			this.panelSmall.Name = "panelSmall";
			this.panelSmall.Size = new System.Drawing.Size(160, 120);
			this.panelSmall.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(1, 73);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(156, 45);
			this.label3.TabIndex = 2;
			this.label3.Text = "The quick brown fox jumps over the lazy";
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(1, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(158, 39);
			this.label2.TabIndex = 1;
			this.label2.Text = "The quick brown fox jumps over the lazy dog.";
			// 
			// pbZoomed
			// 
			this.pbZoomed.Location = new System.Drawing.Point(178, 30);
			this.pbZoomed.Name = "pbZoomed";
			this.pbZoomed.Size = new System.Drawing.Size(640, 480);
			this.pbZoomed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pbZoomed.TabIndex = 2;
			this.pbZoomed.TabStop = false;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status});
			this.statusStrip1.Location = new System.Drawing.Point(0, 517);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(822, 22);
			this.statusStrip1.SizingGrip = false;
			this.statusStrip1.TabIndex = 3;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// status
			// 
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size(16, 17);
			this.status.Text = "…";
			// 
			// cbFontAntialiasing
			// 
			this.cbFontAntialiasing.AutoSize = true;
			this.cbFontAntialiasing.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbFontAntialiasing.Location = new System.Drawing.Point(12, 12);
			this.cbFontAntialiasing.Name = "cbFontAntialiasing";
			this.cbFontAntialiasing.Size = new System.Drawing.Size(152, 17);
			this.cbFontAntialiasing.TabIndex = 1;
			this.cbFontAntialiasing.Text = "Enable Font Antialiasing";
			this.cbFontAntialiasing.UseVisualStyleBackColor = true;
			this.cbFontAntialiasing.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// rbGrayscale
			// 
			this.rbGrayscale.AutoSize = true;
			this.rbGrayscale.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbGrayscale.Location = new System.Drawing.Point(19, 3);
			this.rbGrayscale.Name = "rbGrayscale";
			this.rbGrayscale.Size = new System.Drawing.Size(73, 17);
			this.rbGrayscale.TabIndex = 2;
			this.rbGrayscale.TabStop = true;
			this.rbGrayscale.Text = "Grayscale";
			this.rbGrayscale.UseVisualStyleBackColor = true;
			this.rbGrayscale.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.rbBGR);
			this.panel1.Controls.Add(this.rbRGB);
			this.panel1.Controls.Add(this.rbGrayscale);
			this.panel1.Location = new System.Drawing.Point(12, 30);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(160, 70);
			this.panel1.TabIndex = 2;
			// 
			// rbBGR
			// 
			this.rbBGR.AutoSize = true;
			this.rbBGR.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbBGR.Location = new System.Drawing.Point(19, 49);
			this.rbBGR.Name = "rbBGR";
			this.rbBGR.Size = new System.Drawing.Size(46, 17);
			this.rbBGR.TabIndex = 4;
			this.rbBGR.TabStop = true;
			this.rbBGR.Text = "BGR";
			this.rbBGR.UseVisualStyleBackColor = true;
			this.rbBGR.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// rbRGB
			// 
			this.rbRGB.AutoSize = true;
			this.rbRGB.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbRGB.Location = new System.Drawing.Point(19, 26);
			this.rbRGB.Name = "rbRGB";
			this.rbRGB.Size = new System.Drawing.Size(46, 17);
			this.rbRGB.TabIndex = 3;
			this.rbRGB.TabStop = true;
			this.rbRGB.Text = "RGB";
			this.rbRGB.UseVisualStyleBackColor = true;
			this.rbRGB.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// nudContrast
			// 
			this.nudContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudContrast.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.nudContrast.Location = new System.Drawing.Point(69, 105);
			this.nudContrast.Maximum = new decimal(new int[] {
            2200,
            0,
            0,
            0});
			this.nudContrast.Name = "nudContrast";
			this.nudContrast.Size = new System.Drawing.Size(54, 22);
			this.nudContrast.TabIndex = 5;
			this.nudContrast.Value = new decimal(new int[] {
            1200,
            0,
            0,
            0});
			this.nudContrast.ValueChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(9, 108);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(54, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "Contrast:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(64, 131);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(65, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "[1000-2200]";
			// 
			// btnRestoreDefaults
			// 
			this.btnRestoreDefaults.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnRestoreDefaults.Location = new System.Drawing.Point(9, 157);
			this.btnRestoreDefaults.Name = "btnRestoreDefaults";
			this.btnRestoreDefaults.Size = new System.Drawing.Size(159, 23);
			this.btnRestoreDefaults.TabIndex = 7;
			this.btnRestoreDefaults.Text = "Restore Defaults";
			this.toolTip1.SetToolTip(this.btnRestoreDefaults, "Deletes legacy registry keys and \r\nrestores common default settings:\r\n* RGB subpi" +
        "xel antialiasing\r\n* contrast 0 (which is out of range, \r\n    but it is nonethele" +
        "ss the default)");
			this.btnRestoreDefaults.UseVisualStyleBackColor = true;
			this.btnRestoreDefaults.Click += new System.EventHandler(this.BtnRestoreDefaults_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(13, 374);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(117, 13);
			this.label6.TabIndex = 11;
			this.label6.Text = "Normal-Scale Sample:";
			// 
			// label7
			// 
			this.label7.AutoEllipsis = true;
			this.label7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(12, 220);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(160, 50);
			this.label7.TabIndex = 12;
			this.label7.Text = "You may need to reboot your computer for changes to take effect everywhere.\r\n";
			// 
			// btnSet
			// 
			this.btnSet.Location = new System.Drawing.Point(129, 105);
			this.btnSet.Name = "btnSet";
			this.btnSet.Size = new System.Drawing.Size(43, 23);
			this.btnSet.TabIndex = 6;
			this.btnSet.Text = "> set";
			this.toolTip1.SetToolTip(this.btnSet, "Contrast is set when you unfocus the contrast control, or when you click this but" +
        "ton.");
			this.btnSet.UseVisualStyleBackColor = true;
			this.btnSet.Click += new System.EventHandler(this.ControlsChanged);
			// 
			// label8
			// 
			this.label8.AutoEllipsis = true;
			this.label8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(12, 277);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(160, 94);
			this.label8.TabIndex = 14;
			this.label8.Text = "The font-smoothing settings chosen above affect all connected displays, because W" +
    "indows 10 1903 currently does not offer a functional way to set these per-monito" +
    "r.";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label9.Location = new System.Drawing.Point(426, 12);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(122, 13);
			this.label9.TabIndex = 15;
			this.label9.Text = "400% Zoomed Sample:";
			// 
			// lblNotAdmin
			// 
			this.lblNotAdmin.BackColor = System.Drawing.Color.White;
			this.lblNotAdmin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblNotAdmin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblNotAdmin.ForeColor = System.Drawing.Color.Red;
			this.lblNotAdmin.Location = new System.Drawing.Point(3, 188);
			this.lblNotAdmin.Name = "lblNotAdmin";
			this.lblNotAdmin.Size = new System.Drawing.Size(172, 27);
			this.lblNotAdmin.TabIndex = 16;
			this.lblNotAdmin.Text = "Please run as administrator";
			this.lblNotAdmin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(822, 539);
			this.Controls.Add(this.lblNotAdmin);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.btnSet);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.btnRestoreDefaults);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.nudContrast);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.cbFontAntialiasing);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.pbZoomed);
			this.Controls.Add(this.panelSmall);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Better ClearType Tuner";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.panelSmall.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbZoomed)).EndInit();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudContrast)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panelSmall;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox pbZoomed;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel status;
		private System.Windows.Forms.CheckBox cbFontAntialiasing;
		private System.Windows.Forms.RadioButton rbGrayscale;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton rbBGR;
		private System.Windows.Forms.RadioButton rbRGB;
		private System.Windows.Forms.NumericUpDown nudContrast;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnRestoreDefaults;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnSet;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label lblNotAdmin;
	}
}

