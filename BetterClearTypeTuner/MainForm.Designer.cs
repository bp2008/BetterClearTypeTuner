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
			if (disposing && dwRenderer != null)
			{
				dwRenderer.Dispose();
				dwRenderer = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.panelContent = new System.Windows.Forms.Panel();
			this.cbDarkmode = new System.Windows.Forms.CheckBox();
			this.btnChangeFont = new System.Windows.Forms.Button();
			this.lblNotAdmin = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.lblDwSmallHeader = new System.Windows.Forms.Label();
			this.lblDwZoomHeader = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.btnSet = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.btnRestoreDefaults = new System.Windows.Forms.Button();
			this.labelClearTypeLevelRange = new System.Windows.Forms.Label();
			this.labelClearTypeLevel = new System.Windows.Forms.Label();
			this.nudClearTypeLevel = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.nudContrast = new System.Windows.Forms.NumericUpDown();
			this.panel1 = new System.Windows.Forms.Panel();
			this.rbBGR = new System.Windows.Forms.RadioButton();
			this.rbRGB = new System.Windows.Forms.RadioButton();
			this.rbGrayscale = new System.Windows.Forms.RadioButton();
			this.cbFontAntialiasing = new System.Windows.Forms.CheckBox();
			this.pbZoomed = new System.Windows.Forms.PictureBox();
			this.pbDwZoomed = new System.Windows.Forms.PictureBox();
			this.panelSmall = new System.Windows.Forms.Panel();
			this.lblSample3 = new System.Windows.Forms.Label();
			this.lblSample2 = new System.Windows.Forms.Label();
			this.lblSample1 = new System.Windows.Forms.Label();
			this.pbDwSmall = new System.Windows.Forms.PictureBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.fontDialog1 = new System.Windows.Forms.FontDialog();
			this.status = new System.Windows.Forms.Label();
			this.panelBottomBorder = new System.Windows.Forms.Panel();
			this.linkAboutClearTypeLevel = new System.Windows.Forms.LinkLabel();
			this.panelContent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudClearTypeLevel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudContrast)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbZoomed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbDwZoomed)).BeginInit();
			this.panelSmall.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbDwSmall)).BeginInit();
			this.SuspendLayout();
			// 
			// panelContent
			// 
			this.panelContent.AutoScroll = true;
			this.panelContent.Controls.Add(this.linkAboutClearTypeLevel);
			this.panelContent.Controls.Add(this.cbDarkmode);
			this.panelContent.Controls.Add(this.btnChangeFont);
			this.panelContent.Controls.Add(this.lblNotAdmin);
			this.panelContent.Controls.Add(this.label9);
			this.panelContent.Controls.Add(this.lblDwSmallHeader);
			this.panelContent.Controls.Add(this.lblDwZoomHeader);
			this.panelContent.Controls.Add(this.label8);
			this.panelContent.Controls.Add(this.btnSet);
			this.panelContent.Controls.Add(this.label7);
			this.panelContent.Controls.Add(this.label6);
			this.panelContent.Controls.Add(this.btnRestoreDefaults);
			this.panelContent.Controls.Add(this.labelClearTypeLevelRange);
			this.panelContent.Controls.Add(this.labelClearTypeLevel);
			this.panelContent.Controls.Add(this.nudClearTypeLevel);
			this.panelContent.Controls.Add(this.label5);
			this.panelContent.Controls.Add(this.label4);
			this.panelContent.Controls.Add(this.nudContrast);
			this.panelContent.Controls.Add(this.panel1);
			this.panelContent.Controls.Add(this.cbFontAntialiasing);
			this.panelContent.Controls.Add(this.pbZoomed);
			this.panelContent.Controls.Add(this.pbDwZoomed);
			this.panelContent.Controls.Add(this.panelSmall);
			this.panelContent.Controls.Add(this.pbDwSmall);
			this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContent.Location = new System.Drawing.Point(0, 0);
			this.panelContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.panelContent.Name = "panelContent";
			this.panelContent.Size = new System.Drawing.Size(1420, 674);
			this.panelContent.TabIndex = 0;
			// 
			// cbDarkmode
			// 
			this.cbDarkmode.BackColor = System.Drawing.Color.Transparent;
			this.cbDarkmode.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbDarkmode.Location = new System.Drawing.Point(1122, 5);
			this.cbDarkmode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cbDarkmode.Name = "cbDarkmode";
			this.cbDarkmode.Size = new System.Drawing.Size(125, 25);
			this.cbDarkmode.TabIndex = 8;
			this.cbDarkmode.Text = "Dark Mode";
			this.cbDarkmode.UseVisualStyleBackColor = false;
			this.cbDarkmode.CheckedChanged += new System.EventHandler(this.cbDarkmode_CheckedChanged);
			// 
			// btnChangeFont
			// 
			this.btnChangeFont.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnChangeFont.Location = new System.Drawing.Point(1256, 2);
			this.btnChangeFont.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnChangeFont.Name = "btnChangeFont";
			this.btnChangeFont.Size = new System.Drawing.Size(149, 29);
			this.btnChangeFont.TabIndex = 9;
			this.btnChangeFont.Text = "Change Font";
			this.btnChangeFont.UseVisualStyleBackColor = true;
			this.btnChangeFont.Click += new System.EventHandler(this.btnChangeFont_Click);
			// 
			// lblNotAdmin
			// 
			this.lblNotAdmin.BackColor = System.Drawing.Color.White;
			this.lblNotAdmin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblNotAdmin.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblNotAdmin.ForeColor = System.Drawing.Color.Red;
			this.lblNotAdmin.Location = new System.Drawing.Point(4, 289);
			this.lblNotAdmin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNotAdmin.Name = "lblNotAdmin";
			this.lblNotAdmin.Size = new System.Drawing.Size(214, 33);
			this.lblNotAdmin.TabIndex = 16;
			this.lblNotAdmin.Text = "Please run as administrator";
			this.lblNotAdmin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label9
			// 
			this.label9.BackColor = System.Drawing.Color.Transparent;
			this.label9.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label9.Location = new System.Drawing.Point(305, 8);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(800, 20);
			this.label9.TabIndex = 15;
			this.label9.Text = "400% Zoomed — GDI  (this app, File Explorer, Chrome)";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label9.Click += new System.EventHandler(this.label9_Click);
			// 
			// lblDwSmallHeader
			// 
			this.lblDwSmallHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblDwSmallHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDwSmallHeader.Location = new System.Drawing.Point(15, 561);
			this.lblDwSmallHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDwSmallHeader.Name = "lblDwSmallHeader";
			this.lblDwSmallHeader.Size = new System.Drawing.Size(275, 18);
			this.lblDwSmallHeader.TabIndex = 21;
			this.lblDwSmallHeader.Text = "Normal Scale — DirectWrite:";
			// 
			// lblDwZoomHeader
			// 
			this.lblDwZoomHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblDwZoomHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDwZoomHeader.Location = new System.Drawing.Point(305, 344);
			this.lblDwZoomHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDwZoomHeader.Name = "lblDwZoomHeader";
			this.lblDwZoomHeader.Size = new System.Drawing.Size(1100, 20);
			this.lblDwZoomHeader.TabIndex = 22;
			this.lblDwZoomHeader.Text = "400% Zoomed — DirectWrite  (Firefox, Edge, WPF).  Honors ClearType Level, live.";
			this.lblDwZoomHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label8
			// 
			this.label8.AutoEllipsis = true;
			this.label8.BackColor = System.Drawing.Color.Transparent;
			this.label8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(15, 370);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(275, 80);
			this.label8.TabIndex = 14;
			this.label8.Text = "The font-smoothing settings chosen above affect all connected displays, because W" +
    "indows 10 1903 currently does not offer a functional way to set these per-monito" +
    "r.";
			// 
			// btnSet
			// 
			this.btnSet.Location = new System.Drawing.Point(161, 131);
			this.btnSet.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnSet.Name = "btnSet";
			this.btnSet.Size = new System.Drawing.Size(54, 29);
			this.btnSet.TabIndex = 6;
			this.btnSet.Text = "> set";
			this.toolTip1.SetToolTip(this.btnSet, "Contrast is set when you unfocus the contrast control, or when you click this but" +
        "ton.");
			this.btnSet.UseVisualStyleBackColor = true;
			this.btnSet.Click += new System.EventHandler(this.ControlsChanged);
			// 
			// label7
			// 
			this.label7.AutoEllipsis = true;
			this.label7.BackColor = System.Drawing.Color.Transparent;
			this.label7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(15, 329);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(275, 38);
			this.label7.TabIndex = 12;
			this.label7.Text = "You may need to reboot your computer for changes to take effect everywhere.\r\n";
			// 
			// label6
			// 
			this.label6.BackColor = System.Drawing.Color.Transparent;
			this.label6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(15, 458);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(275, 18);
			this.label6.TabIndex = 11;
			this.label6.Text = "Normal Scale — GDI:";
			// 
			// btnRestoreDefaults
			// 
			this.btnRestoreDefaults.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnRestoreDefaults.Location = new System.Drawing.Point(11, 250);
			this.btnRestoreDefaults.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnRestoreDefaults.Name = "btnRestoreDefaults";
			this.btnRestoreDefaults.Size = new System.Drawing.Size(199, 29);
			this.btnRestoreDefaults.TabIndex = 7;
			this.btnRestoreDefaults.Text = "Restore Defaults";
			this.toolTip1.SetToolTip(this.btnRestoreDefaults, "Deletes legacy registry keys and \r\nrestores common default settings:\r\n* RGB subpi" +
        "xel antialiasing\r\n* contrast 1400\r\n* ClearType Level 100");
			this.btnRestoreDefaults.UseVisualStyleBackColor = true;
			this.btnRestoreDefaults.Click += new System.EventHandler(this.BtnRestoreDefaults_Click);
			// 
			// labelClearTypeLevelRange
			// 
			this.labelClearTypeLevelRange.BackColor = System.Drawing.Color.Transparent;
			this.labelClearTypeLevelRange.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelClearTypeLevelRange.Location = new System.Drawing.Point(82, 220);
			this.labelClearTypeLevelRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelClearTypeLevelRange.Name = "labelClearTypeLevelRange";
			this.labelClearTypeLevelRange.Size = new System.Drawing.Size(110, 24);
			this.labelClearTypeLevelRange.TabIndex = 20;
			this.labelClearTypeLevelRange.Text = "[0-100]";
			// 
			// labelClearTypeLevel
			// 
			this.labelClearTypeLevel.BackColor = System.Drawing.Color.Transparent;
			this.labelClearTypeLevel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelClearTypeLevel.Location = new System.Drawing.Point(11, 178);
			this.labelClearTypeLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelClearTypeLevel.Name = "labelClearTypeLevel";
			this.labelClearTypeLevel.Size = new System.Drawing.Size(74, 46);
			this.labelClearTypeLevel.TabIndex = 19;
			this.labelClearTypeLevel.Text = "ClearType\r\nLevel:";
			this.labelClearTypeLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolTip1.SetToolTip(this.labelClearTypeLevel, "Registry: HKCU\\Software\\Microsoft\\Avalon.Graphics\\<display>\\ClearTypeLevel\r\nSame " +
        "setting as Windows ClearType Tuner step for color intensity.");
			// 
			// nudClearTypeLevel
			// 
			this.nudClearTypeLevel.BackColor = System.Drawing.Color.White;
			this.nudClearTypeLevel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudClearTypeLevel.ForeColor = System.Drawing.Color.Black;
			this.nudClearTypeLevel.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudClearTypeLevel.Location = new System.Drawing.Point(86, 190);
			this.nudClearTypeLevel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.nudClearTypeLevel.Name = "nudClearTypeLevel";
			this.nudClearTypeLevel.Size = new System.Drawing.Size(68, 26);
			this.nudClearTypeLevel.TabIndex = 10;
			this.toolTip1.SetToolTip(this.nudClearTypeLevel, resources.GetString("nudClearTypeLevel.ToolTip"));
			this.nudClearTypeLevel.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.nudClearTypeLevel.ValueChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// label5
			// 
			this.label5.BackColor = System.Drawing.Color.Transparent;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(82, 161);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(110, 22);
			this.label5.TabIndex = 9;
			this.label5.Text = "[1000-2200]";
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(11, 135);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(68, 22);
			this.label4.TabIndex = 8;
			this.label4.Text = "Contrast:";
			// 
			// nudContrast
			// 
			this.nudContrast.BackColor = System.Drawing.Color.White;
			this.nudContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudContrast.ForeColor = System.Drawing.Color.Black;
			this.nudContrast.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.nudContrast.Location = new System.Drawing.Point(86, 131);
			this.nudContrast.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.nudContrast.Maximum = new decimal(new int[] {
            2200,
            0,
            0,
            0});
			this.nudContrast.Name = "nudContrast";
			this.nudContrast.Size = new System.Drawing.Size(68, 26);
			this.nudContrast.TabIndex = 5;
			this.nudContrast.Value = new decimal(new int[] {
            1200,
            0,
            0,
            0});
			this.nudContrast.ValueChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.rbBGR);
			this.panel1.Controls.Add(this.rbRGB);
			this.panel1.Controls.Add(this.rbGrayscale);
			this.panel1.Location = new System.Drawing.Point(15, 38);
			this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(200, 88);
			this.panel1.TabIndex = 2;
			// 
			// rbBGR
			// 
			this.rbBGR.BackColor = System.Drawing.Color.Transparent;
			this.rbBGR.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbBGR.Location = new System.Drawing.Point(24, 60);
			this.rbBGR.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.rbBGR.Name = "rbBGR";
			this.rbBGR.Size = new System.Drawing.Size(172, 25);
			this.rbBGR.TabIndex = 4;
			this.rbBGR.TabStop = true;
			this.rbBGR.Text = "BGR";
			this.rbBGR.UseVisualStyleBackColor = false;
			this.rbBGR.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// rbRGB
			// 
			this.rbRGB.BackColor = System.Drawing.Color.Transparent;
			this.rbRGB.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbRGB.Location = new System.Drawing.Point(24, 31);
			this.rbRGB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.rbRGB.Name = "rbRGB";
			this.rbRGB.Size = new System.Drawing.Size(172, 25);
			this.rbRGB.TabIndex = 3;
			this.rbRGB.TabStop = true;
			this.rbRGB.Text = "RGB";
			this.rbRGB.UseVisualStyleBackColor = false;
			this.rbRGB.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// rbGrayscale
			// 
			this.rbGrayscale.BackColor = System.Drawing.Color.Transparent;
			this.rbGrayscale.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbGrayscale.Location = new System.Drawing.Point(24, 2);
			this.rbGrayscale.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.rbGrayscale.Name = "rbGrayscale";
			this.rbGrayscale.Size = new System.Drawing.Size(172, 25);
			this.rbGrayscale.TabIndex = 2;
			this.rbGrayscale.TabStop = true;
			this.rbGrayscale.Text = "Grayscale";
			this.rbGrayscale.UseVisualStyleBackColor = false;
			this.rbGrayscale.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// cbFontAntialiasing
			// 
			this.cbFontAntialiasing.BackColor = System.Drawing.Color.Transparent;
			this.cbFontAntialiasing.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbFontAntialiasing.Location = new System.Drawing.Point(15, 10);
			this.cbFontAntialiasing.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cbFontAntialiasing.Name = "cbFontAntialiasing";
			this.cbFontAntialiasing.Size = new System.Drawing.Size(221, 25);
			this.cbFontAntialiasing.TabIndex = 1;
			this.cbFontAntialiasing.Text = "Enable Font Antialiasing";
			this.cbFontAntialiasing.UseVisualStyleBackColor = false;
			this.cbFontAntialiasing.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// pbZoomed
			// 
			this.pbZoomed.BackColor = System.Drawing.Color.White;
			this.pbZoomed.Location = new System.Drawing.Point(305, 31);
			this.pbZoomed.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.pbZoomed.Name = "pbZoomed";
			this.pbZoomed.Size = new System.Drawing.Size(1100, 300);
			this.pbZoomed.TabIndex = 2;
			this.pbZoomed.TabStop = false;
			// 
			// pbDwZoomed
			// 
			this.pbDwZoomed.BackColor = System.Drawing.Color.White;
			this.pbDwZoomed.Location = new System.Drawing.Point(305, 368);
			this.pbDwZoomed.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.pbDwZoomed.Name = "pbDwZoomed";
			this.pbDwZoomed.Size = new System.Drawing.Size(1100, 300);
			this.pbDwZoomed.TabIndex = 4;
			this.pbDwZoomed.TabStop = false;
			// 
			// panelSmall
			// 
			this.panelSmall.BackColor = System.Drawing.Color.White;
			this.panelSmall.Controls.Add(this.lblSample3);
			this.panelSmall.Controls.Add(this.lblSample2);
			this.panelSmall.Controls.Add(this.lblSample1);
			this.panelSmall.ForeColor = System.Drawing.Color.Black;
			this.panelSmall.Location = new System.Drawing.Point(15, 479);
			this.panelSmall.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.panelSmall.Name = "panelSmall";
			this.panelSmall.Size = new System.Drawing.Size(275, 75);
			this.panelSmall.TabIndex = 1;
			// 
			// lblSample3
			// 
			this.lblSample3.BackColor = System.Drawing.Color.Transparent;
			this.lblSample3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSample3.Location = new System.Drawing.Point(1, 46);
			this.lblSample3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSample3.Name = "lblSample3";
			this.lblSample3.Size = new System.Drawing.Size(272, 29);
			this.lblSample3.TabIndex = 2;
			this.lblSample3.Text = "The quick brown fox jumps over the lazy dog.";
			// 
			// lblSample2
			// 
			this.lblSample2.BackColor = System.Drawing.Color.Transparent;
			this.lblSample2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSample2.Location = new System.Drawing.Point(1, 22);
			this.lblSample2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSample2.Name = "lblSample2";
			this.lblSample2.Size = new System.Drawing.Size(272, 24);
			this.lblSample2.TabIndex = 1;
			this.lblSample2.Text = "The quick brown fox jumps over the lazy dog.";
			// 
			// lblSample1
			// 
			this.lblSample1.BackColor = System.Drawing.Color.Transparent;
			this.lblSample1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSample1.Location = new System.Drawing.Point(1, 1);
			this.lblSample1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSample1.Name = "lblSample1";
			this.lblSample1.Size = new System.Drawing.Size(272, 20);
			this.lblSample1.TabIndex = 0;
			this.lblSample1.Text = "The quick brown fox jumps over the lazy dog.";
			// 
			// pbDwSmall
			// 
			this.pbDwSmall.BackColor = System.Drawing.Color.White;
			this.pbDwSmall.Location = new System.Drawing.Point(15, 582);
			this.pbDwSmall.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.pbDwSmall.Name = "pbDwSmall";
			this.pbDwSmall.Size = new System.Drawing.Size(275, 75);
			this.pbDwSmall.TabIndex = 3;
			this.pbDwSmall.TabStop = false;
			// 
			// fontDialog1
			// 
			this.fontDialog1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			// 
			// status
			// 
			this.status.BackColor = System.Drawing.Color.Transparent;
			this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.status.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.status.Location = new System.Drawing.Point(0, 675);
			this.status.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.status.Name = "status";
			this.status.Padding = new System.Windows.Forms.Padding(8, 4, 8, 0);
			this.status.Size = new System.Drawing.Size(1420, 25);
			this.status.TabIndex = 18;
			this.status.Text = "...";
			// 
			// panelBottomBorder
			// 
			this.panelBottomBorder.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelBottomBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelBottomBorder.Location = new System.Drawing.Point(0, 674);
			this.panelBottomBorder.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.panelBottomBorder.Name = "panelBottomBorder";
			this.panelBottomBorder.Size = new System.Drawing.Size(1420, 1);
			this.panelBottomBorder.TabIndex = 17;
			// 
			// linkAboutClearTypeLevel
			// 
			this.linkAboutClearTypeLevel.AutoSize = true;
			this.linkAboutClearTypeLevel.Location = new System.Drawing.Point(161, 192);
			this.linkAboutClearTypeLevel.Name = "linkAboutClearTypeLevel";
			this.linkAboutClearTypeLevel.Size = new System.Drawing.Size(23, 19);
			this.linkAboutClearTypeLevel.TabIndex = 23;
			this.linkAboutClearTypeLevel.TabStop = true;
			this.linkAboutClearTypeLevel.Text = "[?]";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(1420, 700);
			this.Controls.Add(this.panelContent);
			this.Controls.Add(this.panelBottomBorder);
			this.Controls.Add(this.status);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MinimumSize = new System.Drawing.Size(520, 313);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Better ClearType Tuner";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.DpiChanged += new System.Windows.Forms.DpiChangedEventHandler(this.MainForm_DpiChanged);
			this.panelContent.ResumeLayout(false);
			this.panelContent.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudClearTypeLevel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudContrast)).EndInit();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbZoomed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbDwZoomed)).EndInit();
			this.panelSmall.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbDwSmall)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelContent;
		private System.Windows.Forms.Label lblSample1;
		private System.Windows.Forms.Panel panelSmall;
		private System.Windows.Forms.Label lblSample3;
		private System.Windows.Forms.Label lblSample2;
		private System.Windows.Forms.PictureBox pbZoomed;
		private System.Windows.Forms.PictureBox pbDwSmall;
		private System.Windows.Forms.PictureBox pbDwZoomed;
		private System.Windows.Forms.CheckBox cbFontAntialiasing;
		private System.Windows.Forms.RadioButton rbGrayscale;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton rbBGR;
		private System.Windows.Forms.RadioButton rbRGB;
		private System.Windows.Forms.NumericUpDown nudContrast;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown nudClearTypeLevel;
		private System.Windows.Forms.Label labelClearTypeLevel;
		private System.Windows.Forms.Label labelClearTypeLevelRange;
		private System.Windows.Forms.Button btnRestoreDefaults;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnSet;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label lblDwSmallHeader;
		private System.Windows.Forms.Label lblDwZoomHeader;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label lblNotAdmin;
		private System.Windows.Forms.Button btnChangeFont;
		private System.Windows.Forms.FontDialog fontDialog1;
		private System.Windows.Forms.CheckBox cbDarkmode;
		private System.Windows.Forms.Label status;
		private System.Windows.Forms.Panel panelBottomBorder;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.LinkLabel linkAboutClearTypeLevel;
	}
}
