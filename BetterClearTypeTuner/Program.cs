using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterClearTypeTuner
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			// The only arguments this application accepts are the ones it passes to itself when it
			// restarts with administrator rights.  See StartupState.
			Application.Run(new MainForm(StartupState.FromCommandLine(args)));
		}
	}
}
