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
			this.cbFontAntialiasing = new System.Windows.Forms.CheckBox();
			this.panelAaMode = new System.Windows.Forms.Panel();
			this.rbBGR = new System.Windows.Forms.RadioButton();
			this.rbRGB = new System.Windows.Forms.RadioButton();
			this.rbGrayscale = new System.Windows.Forms.RadioButton();
			this.lblGdiHeader = new System.Windows.Forms.Label();
			this.panelRuleGdi = new System.Windows.Forms.Panel();
			this.lblGdiContrast = new System.Windows.Forms.Label();
			this.nudGdiContrast = new System.Windows.Forms.NumericUpDown();
			this.linkGdiContrast = new System.Windows.Forms.LinkLabel();
			this.lblGdiContrastRange = new System.Windows.Forms.Label();
			this.lblDwHeader = new System.Windows.Forms.Label();
			this.panelRuleDw = new System.Windows.Forms.Panel();
			this.cbDwOverride = new System.Windows.Forms.CheckBox();
			this.linkDwOverride = new System.Windows.Forms.LinkLabel();
			this.lblDwContrast = new System.Windows.Forms.Label();
			this.nudDwContrast = new System.Windows.Forms.NumericUpDown();
			this.linkDwContrast = new System.Windows.Forms.LinkLabel();
			this.lblDwContrastRange = new System.Windows.Forms.Label();
			this.lblClearTypeLevel = new System.Windows.Forms.Label();
			this.nudClearTypeLevel = new System.Windows.Forms.NumericUpDown();
			this.linkClearTypeLevel = new System.Windows.Forms.LinkLabel();
			this.lblClearTypeLevelRange = new System.Windows.Forms.Label();
			this.lblEnhancedContrast = new System.Windows.Forms.Label();
			this.nudEnhancedContrast = new System.Windows.Forms.NumericUpDown();
			this.linkEnhancedContrast = new System.Windows.Forms.LinkLabel();
			this.lblEnhancedContrastRange = new System.Windows.Forms.Label();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnRestoreDefaults = new System.Windows.Forms.Button();
			this.lblNotes = new System.Windows.Forms.Label();
			this.lblGdiSmallHeader = new System.Windows.Forms.Label();
			this.panelSmall = new System.Windows.Forms.Panel();
			this.lblSample3 = new System.Windows.Forms.Label();
			this.lblSample2 = new System.Windows.Forms.Label();
			this.lblSample1 = new System.Windows.Forms.Label();
			this.lblDwSmallHeader = new System.Windows.Forms.Label();
			this.pbDwSmall = new System.Windows.Forms.PictureBox();
			this.lblGdiZoomHeader = new System.Windows.Forms.Label();
			this.pbZoomed = new System.Windows.Forms.PictureBox();
			this.lblDwZoomHeader = new System.Windows.Forms.Label();
			this.pbDwZoomed = new System.Windows.Forms.PictureBox();
			this.cbDarkmode = new System.Windows.Forms.CheckBox();
			this.btnChangeFont = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.fontDialog1 = new System.Windows.Forms.FontDialog();
			this.status = new System.Windows.Forms.Label();
			this.panelBottomBorder = new System.Windows.Forms.Panel();
			this.panelContent.SuspendLayout();
			this.panelAaMode.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudGdiContrast)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudDwContrast)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudClearTypeLevel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudEnhancedContrast)).BeginInit();
			this.panelSmall.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbDwSmall)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbZoomed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbDwZoomed)).BeginInit();
			this.SuspendLayout();
			// 
			// panelContent
			// 
			this.panelContent.AutoScroll = true;
			this.panelContent.Controls.Add(this.cbFontAntialiasing);
			this.panelContent.Controls.Add(this.panelAaMode);
			this.panelContent.Controls.Add(this.lblGdiHeader);
			this.panelContent.Controls.Add(this.panelRuleGdi);
			this.panelContent.Controls.Add(this.lblGdiContrast);
			this.panelContent.Controls.Add(this.nudGdiContrast);
			this.panelContent.Controls.Add(this.linkGdiContrast);
			this.panelContent.Controls.Add(this.lblGdiContrastRange);
			this.panelContent.Controls.Add(this.lblDwHeader);
			this.panelContent.Controls.Add(this.panelRuleDw);
			this.panelContent.Controls.Add(this.cbDwOverride);
			this.panelContent.Controls.Add(this.linkDwOverride);
			this.panelContent.Controls.Add(this.lblDwContrast);
			this.panelContent.Controls.Add(this.nudDwContrast);
			this.panelContent.Controls.Add(this.linkDwContrast);
			this.panelContent.Controls.Add(this.lblDwContrastRange);
			this.panelContent.Controls.Add(this.lblClearTypeLevel);
			this.panelContent.Controls.Add(this.nudClearTypeLevel);
			this.panelContent.Controls.Add(this.linkClearTypeLevel);
			this.panelContent.Controls.Add(this.lblClearTypeLevelRange);
			this.panelContent.Controls.Add(this.lblEnhancedContrast);
			this.panelContent.Controls.Add(this.nudEnhancedContrast);
			this.panelContent.Controls.Add(this.linkEnhancedContrast);
			this.panelContent.Controls.Add(this.lblEnhancedContrastRange);
			this.panelContent.Controls.Add(this.btnApply);
			this.panelContent.Controls.Add(this.btnRestoreDefaults);
			this.panelContent.Controls.Add(this.lblNotes);
			this.panelContent.Controls.Add(this.lblGdiSmallHeader);
			this.panelContent.Controls.Add(this.panelSmall);
			this.panelContent.Controls.Add(this.lblDwSmallHeader);
			this.panelContent.Controls.Add(this.pbDwSmall);
			this.panelContent.Controls.Add(this.lblGdiZoomHeader);
			this.panelContent.Controls.Add(this.pbZoomed);
			this.panelContent.Controls.Add(this.lblDwZoomHeader);
			this.panelContent.Controls.Add(this.pbDwZoomed);
			this.panelContent.Controls.Add(this.cbDarkmode);
			this.panelContent.Controls.Add(this.btnChangeFont);
			this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContent.Location = new System.Drawing.Point(0, 0);
			this.panelContent.Margin = new System.Windows.Forms.Padding(4);
			this.panelContent.Name = "panelContent";
			this.panelContent.Size = new System.Drawing.Size(1485, 680);
			this.panelContent.TabIndex = 0;
			// 
			// cbFontAntialiasing
			// 
			this.cbFontAntialiasing.BackColor = System.Drawing.Color.Transparent;
			this.cbFontAntialiasing.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbFontAntialiasing.Location = new System.Drawing.Point(15, 6);
			this.cbFontAntialiasing.Margin = new System.Windows.Forms.Padding(4);
			this.cbFontAntialiasing.Name = "cbFontAntialiasing";
			this.cbFontAntialiasing.Size = new System.Drawing.Size(250, 25);
			this.cbFontAntialiasing.TabIndex = 1;
			this.cbFontAntialiasing.Text = "Enable Font Antialiasing";
			this.toolTip1.SetToolTip(this.cbFontAntialiasing, "SystemParametersInfo: SPI_SETFONTSMOOTHING\r\nWith antialiasing off, every other se" +
        "tting below is ignored,\r\nbecause text is drawn with hard pixel edges only.");
			this.cbFontAntialiasing.UseVisualStyleBackColor = false;
			this.cbFontAntialiasing.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// panelAaMode
			// 
			this.panelAaMode.Controls.Add(this.rbBGR);
			this.panelAaMode.Controls.Add(this.rbRGB);
			this.panelAaMode.Controls.Add(this.rbGrayscale);
			this.panelAaMode.Location = new System.Drawing.Point(15, 32);
			this.panelAaMode.Margin = new System.Windows.Forms.Padding(4);
			this.panelAaMode.Name = "panelAaMode";
			this.panelAaMode.Size = new System.Drawing.Size(340, 88);
			this.panelAaMode.TabIndex = 2;
			// 
			// rbBGR
			// 
			this.rbBGR.BackColor = System.Drawing.Color.Transparent;
			this.rbBGR.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbBGR.Location = new System.Drawing.Point(24, 60);
			this.rbBGR.Margin = new System.Windows.Forms.Padding(4);
			this.rbBGR.Name = "rbBGR";
			this.rbBGR.Size = new System.Drawing.Size(200, 25);
			this.rbBGR.TabIndex = 2;
			this.rbBGR.TabStop = true;
			this.rbBGR.Text = "BGR";
			this.toolTip1.SetToolTip(this.rbBGR, "ClearType subpixel antialiasing for displays whose subpixels\r\nare ordered blue, g" +
        "reen, red from left to right.");
			this.rbBGR.UseVisualStyleBackColor = false;
			this.rbBGR.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// rbRGB
			// 
			this.rbRGB.BackColor = System.Drawing.Color.Transparent;
			this.rbRGB.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbRGB.Location = new System.Drawing.Point(24, 31);
			this.rbRGB.Margin = new System.Windows.Forms.Padding(4);
			this.rbRGB.Name = "rbRGB";
			this.rbRGB.Size = new System.Drawing.Size(200, 25);
			this.rbRGB.TabIndex = 1;
			this.rbRGB.TabStop = true;
			this.rbRGB.Text = "RGB";
			this.toolTip1.SetToolTip(this.rbRGB, "ClearType subpixel antialiasing for displays whose subpixels\r\nare ordered red, gr" +
        "een, blue from left to right.  This is by far\r\nthe most common layout.");
			this.rbRGB.UseVisualStyleBackColor = false;
			this.rbRGB.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// rbGrayscale
			// 
			this.rbGrayscale.BackColor = System.Drawing.Color.Transparent;
			this.rbGrayscale.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbGrayscale.Location = new System.Drawing.Point(24, 2);
			this.rbGrayscale.Margin = new System.Windows.Forms.Padding(4);
			this.rbGrayscale.Name = "rbGrayscale";
			this.rbGrayscale.Size = new System.Drawing.Size(200, 25);
			this.rbGrayscale.TabIndex = 0;
			this.rbGrayscale.TabStop = true;
			this.rbGrayscale.Text = "Grayscale";
			this.toolTip1.SetToolTip(this.rbGrayscale, "SystemParametersInfo: SPI_SETFONTSMOOTHINGTYPE = Standard\r\nSmooths with shades of" +
        " gray only, so there is no color fringing.");
			this.rbGrayscale.UseVisualStyleBackColor = false;
			this.rbGrayscale.CheckedChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// lblGdiHeader
			// 
			this.lblGdiHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblGdiHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGdiHeader.Location = new System.Drawing.Point(15, 126);
			this.lblGdiHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGdiHeader.Name = "lblGdiHeader";
			this.lblGdiHeader.Size = new System.Drawing.Size(340, 20);
			this.lblGdiHeader.TabIndex = 3;
			this.lblGdiHeader.Text = "GDI  (this app, File Explorer, Chrome)";
			this.lblGdiHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelRuleGdi
			// 
			this.panelRuleGdi.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelRuleGdi.Location = new System.Drawing.Point(15, 146);
			this.panelRuleGdi.Margin = new System.Windows.Forms.Padding(4);
			this.panelRuleGdi.Name = "panelRuleGdi";
			this.panelRuleGdi.Size = new System.Drawing.Size(340, 1);
			this.panelRuleGdi.TabIndex = 4;
			// 
			// lblGdiContrast
			// 
			this.lblGdiContrast.BackColor = System.Drawing.Color.Transparent;
			this.lblGdiContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGdiContrast.Location = new System.Drawing.Point(15, 152);
			this.lblGdiContrast.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGdiContrast.Name = "lblGdiContrast";
			this.lblGdiContrast.Size = new System.Drawing.Size(138, 30);
			this.lblGdiContrast.TabIndex = 5;
			this.lblGdiContrast.Text = "Contrast:";
			this.lblGdiContrast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// nudGdiContrast
			// 
			this.nudGdiContrast.BackColor = System.Drawing.Color.White;
			this.nudGdiContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudGdiContrast.ForeColor = System.Drawing.Color.Black;
			this.nudGdiContrast.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.nudGdiContrast.Location = new System.Drawing.Point(157, 154);
			this.nudGdiContrast.Margin = new System.Windows.Forms.Padding(4);
			this.nudGdiContrast.Maximum = new decimal(new int[] {
            2200,
            0,
            0,
            0});
			this.nudGdiContrast.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.nudGdiContrast.Name = "nudGdiContrast";
			this.nudGdiContrast.Size = new System.Drawing.Size(68, 26);
			this.nudGdiContrast.TabIndex = 6;
			this.nudGdiContrast.Value = new decimal(new int[] {
            1200,
            0,
            0,
            0});
			this.nudGdiContrast.ValueChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// linkGdiContrast
			// 
			this.linkGdiContrast.BackColor = System.Drawing.Color.Transparent;
			this.linkGdiContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkGdiContrast.Location = new System.Drawing.Point(329, 157);
			this.linkGdiContrast.Name = "linkGdiContrast";
			this.linkGdiContrast.Size = new System.Drawing.Size(24, 20);
			this.linkGdiContrast.TabIndex = 7;
			this.linkGdiContrast.TabStop = true;
			this.linkGdiContrast.Text = "[?]";
			this.linkGdiContrast.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkGdiContrast_LinkClicked);
			// 
			// lblGdiContrastRange
			// 
			this.lblGdiContrastRange.BackColor = System.Drawing.Color.Transparent;
			this.lblGdiContrastRange.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGdiContrastRange.Location = new System.Drawing.Point(229, 152);
			this.lblGdiContrastRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGdiContrastRange.Name = "lblGdiContrastRange";
			this.lblGdiContrastRange.Size = new System.Drawing.Size(96, 30);
			this.lblGdiContrastRange.TabIndex = 8;
			this.lblGdiContrastRange.Text = "[1000-2200]";
			this.lblGdiContrastRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblDwHeader
			// 
			this.lblDwHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblDwHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDwHeader.Location = new System.Drawing.Point(15, 190);
			this.lblDwHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDwHeader.Name = "lblDwHeader";
			this.lblDwHeader.Size = new System.Drawing.Size(340, 20);
			this.lblDwHeader.TabIndex = 9;
			this.lblDwHeader.Text = "DirectWrite  (Firefox, Edge, WPF)";
			this.lblDwHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelRuleDw
			// 
			this.panelRuleDw.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelRuleDw.Location = new System.Drawing.Point(15, 210);
			this.panelRuleDw.Margin = new System.Windows.Forms.Padding(4);
			this.panelRuleDw.Name = "panelRuleDw";
			this.panelRuleDw.Size = new System.Drawing.Size(340, 1);
			this.panelRuleDw.TabIndex = 10;
			//
			// cbDwOverride
			//
			this.cbDwOverride.BackColor = System.Drawing.Color.Transparent;
			this.cbDwOverride.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbDwOverride.Location = new System.Drawing.Point(15, 216);
			this.cbDwOverride.Margin = new System.Windows.Forms.Padding(4);
			this.cbDwOverride.Name = "cbDwOverride";
			this.cbDwOverride.Size = new System.Drawing.Size(305, 24);
			this.cbDwOverride.TabIndex = 11;
			this.cbDwOverride.Text = "Override DirectWrite defaults";
			this.cbDwOverride.UseVisualStyleBackColor = false;
			this.cbDwOverride.CheckedChanged += new System.EventHandler(this.cbDwOverride_CheckedChanged);
			//
			// linkDwOverride
			//
			this.linkDwOverride.BackColor = System.Drawing.Color.Transparent;
			this.linkDwOverride.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkDwOverride.Location = new System.Drawing.Point(329, 218);
			this.linkDwOverride.Name = "linkDwOverride";
			this.linkDwOverride.Size = new System.Drawing.Size(24, 20);
			this.linkDwOverride.TabIndex = 12;
			this.linkDwOverride.TabStop = true;
			this.linkDwOverride.Text = "[?]";
			this.linkDwOverride.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDwOverride_LinkClicked);
			//
			// lblDwContrast
			//
			this.lblDwContrast.BackColor = System.Drawing.Color.Transparent;
			this.lblDwContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDwContrast.Location = new System.Drawing.Point(15, 246);
			this.lblDwContrast.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDwContrast.Name = "lblDwContrast";
			this.lblDwContrast.Size = new System.Drawing.Size(138, 30);
			this.lblDwContrast.TabIndex = 13;
			this.lblDwContrast.Text = "Contrast:";
			this.lblDwContrast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// nudDwContrast
			// 
			this.nudDwContrast.BackColor = System.Drawing.Color.White;
			this.nudDwContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudDwContrast.ForeColor = System.Drawing.Color.Black;
			this.nudDwContrast.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.nudDwContrast.Location = new System.Drawing.Point(157, 248);
			this.nudDwContrast.Margin = new System.Windows.Forms.Padding(4);
			this.nudDwContrast.Maximum = new decimal(new int[] {
            2200,
            0,
            0,
            0});
			this.nudDwContrast.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.nudDwContrast.Name = "nudDwContrast";
			this.nudDwContrast.Size = new System.Drawing.Size(68, 26);
			this.nudDwContrast.TabIndex = 14;
			this.nudDwContrast.Value = new decimal(new int[] {
            1800,
            0,
            0,
            0});
			this.nudDwContrast.ValueChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// linkDwContrast
			// 
			this.linkDwContrast.BackColor = System.Drawing.Color.Transparent;
			this.linkDwContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkDwContrast.Location = new System.Drawing.Point(329, 251);
			this.linkDwContrast.Name = "linkDwContrast";
			this.linkDwContrast.Size = new System.Drawing.Size(24, 20);
			this.linkDwContrast.TabIndex = 15;
			this.linkDwContrast.TabStop = true;
			this.linkDwContrast.Text = "[?]";
			this.linkDwContrast.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDwContrast_LinkClicked);
			// 
			// lblDwContrastRange
			// 
			this.lblDwContrastRange.BackColor = System.Drawing.Color.Transparent;
			this.lblDwContrastRange.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDwContrastRange.Location = new System.Drawing.Point(229, 246);
			this.lblDwContrastRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDwContrastRange.Name = "lblDwContrastRange";
			this.lblDwContrastRange.Size = new System.Drawing.Size(96, 30);
			this.lblDwContrastRange.TabIndex = 16;
			this.lblDwContrastRange.Text = "[1000-2200]";
			this.lblDwContrastRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblClearTypeLevel
			// 
			this.lblClearTypeLevel.BackColor = System.Drawing.Color.Transparent;
			this.lblClearTypeLevel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblClearTypeLevel.Location = new System.Drawing.Point(15, 278);
			this.lblClearTypeLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblClearTypeLevel.Name = "lblClearTypeLevel";
			this.lblClearTypeLevel.Size = new System.Drawing.Size(138, 30);
			this.lblClearTypeLevel.TabIndex = 17;
			this.lblClearTypeLevel.Text = "ClearType Level:";
			this.lblClearTypeLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			this.nudClearTypeLevel.Location = new System.Drawing.Point(157, 280);
			this.nudClearTypeLevel.Margin = new System.Windows.Forms.Padding(4);
			this.nudClearTypeLevel.Name = "nudClearTypeLevel";
			this.nudClearTypeLevel.Size = new System.Drawing.Size(68, 26);
			this.nudClearTypeLevel.TabIndex = 18;
			this.nudClearTypeLevel.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.nudClearTypeLevel.ValueChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// linkClearTypeLevel
			// 
			this.linkClearTypeLevel.BackColor = System.Drawing.Color.Transparent;
			this.linkClearTypeLevel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkClearTypeLevel.Location = new System.Drawing.Point(329, 283);
			this.linkClearTypeLevel.Name = "linkClearTypeLevel";
			this.linkClearTypeLevel.Size = new System.Drawing.Size(24, 20);
			this.linkClearTypeLevel.TabIndex = 19;
			this.linkClearTypeLevel.TabStop = true;
			this.linkClearTypeLevel.Text = "[?]";
			this.linkClearTypeLevel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkClearTypeLevel_LinkClicked);
			// 
			// lblClearTypeLevelRange
			// 
			this.lblClearTypeLevelRange.BackColor = System.Drawing.Color.Transparent;
			this.lblClearTypeLevelRange.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblClearTypeLevelRange.Location = new System.Drawing.Point(229, 278);
			this.lblClearTypeLevelRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblClearTypeLevelRange.Name = "lblClearTypeLevelRange";
			this.lblClearTypeLevelRange.Size = new System.Drawing.Size(96, 30);
			this.lblClearTypeLevelRange.TabIndex = 20;
			this.lblClearTypeLevelRange.Text = "[0-100]";
			this.lblClearTypeLevelRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblEnhancedContrast
			// 
			this.lblEnhancedContrast.BackColor = System.Drawing.Color.Transparent;
			this.lblEnhancedContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblEnhancedContrast.Location = new System.Drawing.Point(15, 310);
			this.lblEnhancedContrast.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblEnhancedContrast.Name = "lblEnhancedContrast";
			this.lblEnhancedContrast.Size = new System.Drawing.Size(138, 30);
			this.lblEnhancedContrast.TabIndex = 21;
			this.lblEnhancedContrast.Text = "Enhanced Contrast:";
			this.lblEnhancedContrast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// nudEnhancedContrast
			// 
			this.nudEnhancedContrast.BackColor = System.Drawing.Color.White;
			this.nudEnhancedContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudEnhancedContrast.ForeColor = System.Drawing.Color.Black;
			this.nudEnhancedContrast.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudEnhancedContrast.Location = new System.Drawing.Point(157, 312);
			this.nudEnhancedContrast.Margin = new System.Windows.Forms.Padding(4);
			this.nudEnhancedContrast.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
			this.nudEnhancedContrast.Name = "nudEnhancedContrast";
			this.nudEnhancedContrast.Size = new System.Drawing.Size(68, 26);
			this.nudEnhancedContrast.TabIndex = 22;
			this.nudEnhancedContrast.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.nudEnhancedContrast.ValueChanged += new System.EventHandler(this.ControlsChanged);
			// 
			// linkEnhancedContrast
			// 
			this.linkEnhancedContrast.BackColor = System.Drawing.Color.Transparent;
			this.linkEnhancedContrast.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkEnhancedContrast.Location = new System.Drawing.Point(329, 315);
			this.linkEnhancedContrast.Name = "linkEnhancedContrast";
			this.linkEnhancedContrast.Size = new System.Drawing.Size(24, 20);
			this.linkEnhancedContrast.TabIndex = 23;
			this.linkEnhancedContrast.TabStop = true;
			this.linkEnhancedContrast.Text = "[?]";
			this.linkEnhancedContrast.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkEnhancedContrast_LinkClicked);
			// 
			// lblEnhancedContrastRange
			// 
			this.lblEnhancedContrastRange.BackColor = System.Drawing.Color.Transparent;
			this.lblEnhancedContrastRange.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblEnhancedContrastRange.Location = new System.Drawing.Point(229, 310);
			this.lblEnhancedContrastRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblEnhancedContrastRange.Name = "lblEnhancedContrastRange";
			this.lblEnhancedContrastRange.Size = new System.Drawing.Size(96, 30);
			this.lblEnhancedContrastRange.TabIndex = 24;
			this.lblEnhancedContrastRange.Text = "[0-400]";
			this.lblEnhancedContrastRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnApply
			// 
			this.btnApply.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnApply.Location = new System.Drawing.Point(15, 348);
			this.btnApply.Margin = new System.Windows.Forms.Padding(4);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(110, 29);
			this.btnApply.TabIndex = 25;
			this.btnApply.Text = "Apply";
			this.toolTip1.SetToolTip(this.btnApply, "Values are applied when you leave a box after typing in it,\r\nor when you click th" +
        "is button.");
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// btnRestoreDefaults
			// 
			this.btnRestoreDefaults.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnRestoreDefaults.Location = new System.Drawing.Point(133, 348);
			this.btnRestoreDefaults.Margin = new System.Windows.Forms.Padding(4);
			this.btnRestoreDefaults.Name = "btnRestoreDefaults";
			this.btnRestoreDefaults.Size = new System.Drawing.Size(170, 29);
			this.btnRestoreDefaults.TabIndex = 26;
			this.btnRestoreDefaults.Text = "Restore Defaults";
			this.toolTip1.SetToolTip(this.btnRestoreDefaults, resources.GetString("btnRestoreDefaults.ToolTip"));
			this.btnRestoreDefaults.UseVisualStyleBackColor = true;
			this.btnRestoreDefaults.Click += new System.EventHandler(this.BtnRestoreDefaults_Click);
			// 
			// lblNotes
			// 
			this.lblNotes.AutoEllipsis = true;
			this.lblNotes.BackColor = System.Drawing.Color.Transparent;
			this.lblNotes.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblNotes.Location = new System.Drawing.Point(15, 386);
			this.lblNotes.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNotes.Name = "lblNotes";
			this.lblNotes.Size = new System.Drawing.Size(340, 90);
			this.lblNotes.TabIndex = 28;
			this.lblNotes.Text = "Applications read these settings at startup, so restart them (or reboot) to see t" +
    "he change everywhere.  All connected displays are affected; Windows has no worki" +
    "ng per-monitor equivalent.";
			// 
			// lblGdiSmallHeader
			// 
			this.lblGdiSmallHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblGdiSmallHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGdiSmallHeader.Location = new System.Drawing.Point(15, 478);
			this.lblGdiSmallHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGdiSmallHeader.Name = "lblGdiSmallHeader";
			this.lblGdiSmallHeader.Size = new System.Drawing.Size(275, 18);
			this.lblGdiSmallHeader.TabIndex = 29;
			this.lblGdiSmallHeader.Text = "Normal Scale - GDI:";
			// 
			// panelSmall
			// 
			this.panelSmall.BackColor = System.Drawing.Color.White;
			this.panelSmall.Controls.Add(this.lblSample3);
			this.panelSmall.Controls.Add(this.lblSample2);
			this.panelSmall.Controls.Add(this.lblSample1);
			this.panelSmall.ForeColor = System.Drawing.Color.Black;
			this.panelSmall.Location = new System.Drawing.Point(15, 498);
			this.panelSmall.Margin = new System.Windows.Forms.Padding(4);
			this.panelSmall.Name = "panelSmall";
			this.panelSmall.Size = new System.Drawing.Size(275, 75);
			this.panelSmall.TabIndex = 30;
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
			// lblDwSmallHeader
			// 
			this.lblDwSmallHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblDwSmallHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDwSmallHeader.Location = new System.Drawing.Point(15, 577);
			this.lblDwSmallHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDwSmallHeader.Name = "lblDwSmallHeader";
			this.lblDwSmallHeader.Size = new System.Drawing.Size(275, 18);
			this.lblDwSmallHeader.TabIndex = 31;
			this.lblDwSmallHeader.Text = "Normal Scale - DirectWrite:";
			// 
			// pbDwSmall
			// 
			this.pbDwSmall.BackColor = System.Drawing.Color.White;
			this.pbDwSmall.Location = new System.Drawing.Point(15, 597);
			this.pbDwSmall.Margin = new System.Windows.Forms.Padding(4);
			this.pbDwSmall.Name = "pbDwSmall";
			this.pbDwSmall.Size = new System.Drawing.Size(275, 75);
			this.pbDwSmall.TabIndex = 32;
			this.pbDwSmall.TabStop = false;
			// 
			// lblGdiZoomHeader
			// 
			this.lblGdiZoomHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblGdiZoomHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGdiZoomHeader.Location = new System.Drawing.Point(370, 6);
			this.lblGdiZoomHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGdiZoomHeader.Name = "lblGdiZoomHeader";
			this.lblGdiZoomHeader.Size = new System.Drawing.Size(760, 20);
			this.lblGdiZoomHeader.TabIndex = 33;
			this.lblGdiZoomHeader.Text = "400% Zoomed - GDI  (this app, File Explorer, Chrome).  Reflects the settings that" +
    " are already applied.";
			this.lblGdiZoomHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pbZoomed
			// 
			this.pbZoomed.BackColor = System.Drawing.Color.White;
			this.pbZoomed.Location = new System.Drawing.Point(370, 29);
			this.pbZoomed.Margin = new System.Windows.Forms.Padding(4);
			this.pbZoomed.Name = "pbZoomed";
			this.pbZoomed.Size = new System.Drawing.Size(1100, 300);
			this.pbZoomed.TabIndex = 34;
			this.pbZoomed.TabStop = false;
			// 
			// lblDwZoomHeader
			// 
			this.lblDwZoomHeader.BackColor = System.Drawing.Color.Transparent;
			this.lblDwZoomHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDwZoomHeader.Location = new System.Drawing.Point(370, 342);
			this.lblDwZoomHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblDwZoomHeader.Name = "lblDwZoomHeader";
			this.lblDwZoomHeader.Size = new System.Drawing.Size(1100, 20);
			this.lblDwZoomHeader.TabIndex = 35;
			this.lblDwZoomHeader.Text = "400% Zoomed - DirectWrite  (Firefox, Edge, WPF).  Live preview: every DirectWrite" +
    " setting is applied here without a restart.";
			this.lblDwZoomHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pbDwZoomed
			// 
			this.pbDwZoomed.BackColor = System.Drawing.Color.White;
			this.pbDwZoomed.Location = new System.Drawing.Point(370, 366);
			this.pbDwZoomed.Margin = new System.Windows.Forms.Padding(4);
			this.pbDwZoomed.Name = "pbDwZoomed";
			this.pbDwZoomed.Size = new System.Drawing.Size(1100, 300);
			this.pbDwZoomed.TabIndex = 36;
			this.pbDwZoomed.TabStop = false;
			// 
			// cbDarkmode
			// 
			this.cbDarkmode.BackColor = System.Drawing.Color.Transparent;
			this.cbDarkmode.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbDarkmode.Location = new System.Drawing.Point(1187, 3);
			this.cbDarkmode.Margin = new System.Windows.Forms.Padding(4);
			this.cbDarkmode.Name = "cbDarkmode";
			this.cbDarkmode.Size = new System.Drawing.Size(125, 25);
			this.cbDarkmode.TabIndex = 37;
			this.cbDarkmode.Text = "Dark Mode";
			this.cbDarkmode.UseVisualStyleBackColor = false;
			this.cbDarkmode.CheckedChanged += new System.EventHandler(this.cbDarkmode_CheckedChanged);
			// 
			// btnChangeFont
			// 
			this.btnChangeFont.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnChangeFont.Location = new System.Drawing.Point(1321, 0);
			this.btnChangeFont.Margin = new System.Windows.Forms.Padding(4);
			this.btnChangeFont.Name = "btnChangeFont";
			this.btnChangeFont.Size = new System.Drawing.Size(149, 29);
			this.btnChangeFont.TabIndex = 38;
			this.btnChangeFont.Text = "Change Font";
			this.btnChangeFont.UseVisualStyleBackColor = true;
			this.btnChangeFont.Click += new System.EventHandler(this.btnChangeFont_Click);
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
			this.status.Location = new System.Drawing.Point(0, 681);
			this.status.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.status.Name = "status";
			this.status.Padding = new System.Windows.Forms.Padding(8, 4, 8, 0);
			this.status.Size = new System.Drawing.Size(1485, 25);
			this.status.TabIndex = 18;
			this.status.Text = "...";
			// 
			// panelBottomBorder
			// 
			this.panelBottomBorder.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelBottomBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelBottomBorder.Location = new System.Drawing.Point(0, 680);
			this.panelBottomBorder.Margin = new System.Windows.Forms.Padding(4);
			this.panelBottomBorder.Name = "panelBottomBorder";
			this.panelBottomBorder.Size = new System.Drawing.Size(1485, 1);
			this.panelBottomBorder.TabIndex = 17;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(1485, 706);
			this.Controls.Add(this.panelContent);
			this.Controls.Add(this.panelBottomBorder);
			this.Controls.Add(this.status);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(520, 313);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Better ClearType Tuner";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.DpiChanged += new System.Windows.Forms.DpiChangedEventHandler(this.MainForm_DpiChanged);
			this.panelContent.ResumeLayout(false);
			this.panelAaMode.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.nudGdiContrast)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudDwContrast)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudClearTypeLevel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudEnhancedContrast)).EndInit();
			this.panelSmall.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbDwSmall)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbZoomed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbDwZoomed)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelContent;
		private System.Windows.Forms.CheckBox cbFontAntialiasing;
		private System.Windows.Forms.Panel panelAaMode;
		private System.Windows.Forms.RadioButton rbGrayscale;
		private System.Windows.Forms.RadioButton rbRGB;
		private System.Windows.Forms.RadioButton rbBGR;
		private System.Windows.Forms.Label lblGdiHeader;
		private System.Windows.Forms.Panel panelRuleGdi;
		private System.Windows.Forms.Label lblGdiContrast;
		private System.Windows.Forms.NumericUpDown nudGdiContrast;
		private System.Windows.Forms.LinkLabel linkGdiContrast;
		private System.Windows.Forms.Label lblGdiContrastRange;
		private System.Windows.Forms.Label lblDwHeader;
		private System.Windows.Forms.Panel panelRuleDw;
		private System.Windows.Forms.CheckBox cbDwOverride;
		private System.Windows.Forms.LinkLabel linkDwOverride;
		private System.Windows.Forms.Label lblDwContrast;
		private System.Windows.Forms.NumericUpDown nudDwContrast;
		private System.Windows.Forms.LinkLabel linkDwContrast;
		private System.Windows.Forms.Label lblDwContrastRange;
		private System.Windows.Forms.Label lblClearTypeLevel;
		private System.Windows.Forms.NumericUpDown nudClearTypeLevel;
		private System.Windows.Forms.LinkLabel linkClearTypeLevel;
		private System.Windows.Forms.Label lblClearTypeLevelRange;
		private System.Windows.Forms.Label lblEnhancedContrast;
		private System.Windows.Forms.NumericUpDown nudEnhancedContrast;
		private System.Windows.Forms.LinkLabel linkEnhancedContrast;
		private System.Windows.Forms.Label lblEnhancedContrastRange;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnRestoreDefaults;
		private System.Windows.Forms.Label lblNotes;
		private System.Windows.Forms.Label lblGdiSmallHeader;
		private System.Windows.Forms.Panel panelSmall;
		private System.Windows.Forms.Label lblSample1;
		private System.Windows.Forms.Label lblSample2;
		private System.Windows.Forms.Label lblSample3;
		private System.Windows.Forms.Label lblDwSmallHeader;
		private System.Windows.Forms.PictureBox pbDwSmall;
		private System.Windows.Forms.Label lblGdiZoomHeader;
		private System.Windows.Forms.PictureBox pbZoomed;
		private System.Windows.Forms.Label lblDwZoomHeader;
		private System.Windows.Forms.PictureBox pbDwZoomed;
		private System.Windows.Forms.CheckBox cbDarkmode;
		private System.Windows.Forms.Button btnChangeFont;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.FontDialog fontDialog1;
		private System.Windows.Forms.Label status;
		private System.Windows.Forms.Panel panelBottomBorder;
	}
}
