namespace BCT_Tests
{
	partial class MainTestingForm
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
			this.panelTop = new System.Windows.Forms.Panel();
			this.lblIntro = new System.Windows.Forms.Label();
			this.btnRun = new System.Windows.Forms.Button();
			this.btnRunElevated = new System.Windows.Forms.Button();
			this.btnOpenResults = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.panelTop.SuspendLayout();
			this.SuspendLayout();
			//
			// panelTop
			//
			this.panelTop.Controls.Add(this.btnOpenResults);
			this.panelTop.Controls.Add(this.btnRunElevated);
			this.panelTop.Controls.Add(this.btnRun);
			this.panelTop.Controls.Add(this.lblIntro);
			this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTop.Location = new System.Drawing.Point(0, 0);
			this.panelTop.Name = "panelTop";
			this.panelTop.Size = new System.Drawing.Size(884, 116);
			this.panelTop.TabIndex = 0;
			//
			// lblIntro
			//
			this.lblIntro.Location = new System.Drawing.Point(12, 9);
			this.lblIntro.Name = "lblIntro";
			this.lblIntro.Size = new System.Drawing.Size(860, 60);
			this.lblIntro.TabIndex = 0;
			this.lblIntro.Text = "Measures which Avalon.Graphics registry values and system font-smoothing settings " +
				"affect GDI and DirectWrite text rendering.\r\n\r\nThe sweep temporarily changes your " +
				"font smoothing settings and registry values, then puts them all back.";
			//
			// btnRun
			//
			this.btnRun.Location = new System.Drawing.Point(15, 75);
			this.btnRun.Name = "btnRun";
			this.btnRun.Size = new System.Drawing.Size(180, 30);
			this.btnRun.TabIndex = 1;
			this.btnRun.Text = "Run sweep";
			this.btnRun.UseVisualStyleBackColor = true;
			this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
			//
			// btnRunElevated
			//
			this.btnRunElevated.Location = new System.Drawing.Point(201, 75);
			this.btnRunElevated.Name = "btnRunElevated";
			this.btnRunElevated.Size = new System.Drawing.Size(230, 30);
			this.btnRunElevated.TabIndex = 2;
			this.btnRunElevated.Text = "Run sweep as administrator";
			this.btnRunElevated.UseVisualStyleBackColor = true;
			this.btnRunElevated.Click += new System.EventHandler(this.btnRunElevated_Click);
			//
			// btnOpenResults
			//
			this.btnOpenResults.Location = new System.Drawing.Point(437, 75);
			this.btnOpenResults.Name = "btnOpenResults";
			this.btnOpenResults.Size = new System.Drawing.Size(180, 30);
			this.btnOpenResults.TabIndex = 3;
			this.btnOpenResults.Text = "Open results folder";
			this.btnOpenResults.UseVisualStyleBackColor = true;
			this.btnOpenResults.Click += new System.EventHandler(this.btnOpenResults_Click);
			//
			// txtLog
			//
			this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
			this.txtLog.Location = new System.Drawing.Point(0, 116);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(884, 445);
			this.txtLog.TabIndex = 4;
			this.txtLog.WordWrap = false;
			//
			// MainTestingForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(884, 561);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.panelTop);
			this.Name = "MainTestingForm";
			this.Text = "Better ClearType Tuner - Testing App";
			this.panelTop.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Panel panelTop;
		private System.Windows.Forms.Label lblIntro;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.Button btnRunElevated;
		private System.Windows.Forms.Button btnOpenResults;
		private System.Windows.Forms.TextBox txtLog;
	}
}
