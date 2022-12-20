using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterClearTypeTuner.Native
{
	public static class UXTheme
	{
		[DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#132")]
		public static extern bool ShouldAppsUseDarkMode();
		[DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#135")]
		public static extern bool AllowDarkModeForApp();
		[DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
		public static extern bool ShouldSystemUseDarkMode();
	}
}
