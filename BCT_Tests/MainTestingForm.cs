using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BCT_Tests
{
	/// <summary>
	/// A thin front end for the sweep.  Everything it does is also available on the command line
	/// (see <see cref="Program.Main"/>), which is how it is normally run.
	/// </summary>
	public partial class MainTestingForm : Form
	{
		private readonly string outputDir = Program.DefaultOutputDirectory;
		private Thread worker;

		public MainTestingForm()
		{
			InitializeComponent();
			Log("Results folder: " + outputDir);
			if (!Program.IsElevated)
			{
				Log("");
				Log("This process is not elevated, so the HKEY_LOCAL_MACHINE values can neither be set nor");
				Log("cleared.  Use \"Run sweep as administrator\" for a complete matrix.");
			}
		}

		private void btnRun_Click(object sender, EventArgs e)
		{
			if (worker != null && worker.IsAlive)
				return;
			SetRunning(true);
			worker = new Thread(delegate ()
			{
				int exitCode;
				try
				{
					exitCode = Program.RunSweep(outputDir, !Program.IsElevated, false, Log);
				}
				catch (Exception ex)
				{
					Log("FAILED: " + ex);
					exitCode = 1;
				}
				BeginInvoke((Action)delegate ()
				{
					SetRunning(false);
					Log(exitCode == 0 ? "Done." : "Finished with problems (exit code " + exitCode + ").");
				});
			});
			worker.IsBackground = true;
			worker.Start();
		}

		private void btnRunElevated_Click(object sender, EventArgs e)
		{
			string error;
			if (Program.RelaunchElevated(outputDir, out error))
				Log("Launched an elevated run; watch its console window for progress.");
			else
				Log("Could not start an elevated run: " + error);
		}

		private void btnOpenResults_Click(object sender, EventArgs e)
		{
			Directory.CreateDirectory(outputDir);
			Process.Start("explorer.exe", "\"" + outputDir + "\"");
		}

		private void SetRunning(bool running)
		{
			btnRun.Enabled = !running;
			btnRunElevated.Enabled = !running;
			Text = running
				? "Better ClearType Tuner - Testing App (running...)"
				: "Better ClearType Tuner - Testing App";
		}

		private void Log(string line)
		{
			if (InvokeRequired)
			{
				BeginInvoke((Action<string>)Log, line);
				return;
			}
			txtLog.AppendText(line + Environment.NewLine);
		}
	}
}
