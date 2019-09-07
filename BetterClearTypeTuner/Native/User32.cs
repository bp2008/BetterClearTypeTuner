using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterClearTypeTuner.Native
{
	public static class User32
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(SPI uiAction, uint uiParam, ref uint pvParam, SPIF fWinIni); // Overloads can be made where pvParam can be any type

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(SPI uiAction, uint uiParam, uint pvParam, SPIF fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(SPI uiAction, uint uiParam, ref bool pvParam, SPIF fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(SPI uiAction, uint uiParam, bool pvParam, SPIF fWinIni);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(SPI uiAction, bool uiParam, IntPtr pvParam, SPIF fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(SPI uiAction, uint uiParam, IntPtr pvParam, SPIF fWinIni);

		// For setting a string parameter
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, String pvParam, SPIF fWinIni);

		// For reading a string parameter
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, StringBuilder pvParam, SPIF fWinIni);

		//[DllImport("user32.dll", SetLastError = true)]
		//[return: MarshalAs(UnmanagedType.Bool)]
		//public static extern bool SystemParametersInfoW(SPI uiAction, uint uiParam, ref ANIMATIONINFO pvParam, SPIF fWinIni);
	}
}
