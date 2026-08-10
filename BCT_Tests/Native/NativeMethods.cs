using System;
using System.Runtime.InteropServices;

namespace BCT_Tests.Native
{
	/// <summary>
	/// GDI and USER32 interop used by the test harness.  The harness deliberately draws with raw
	/// GDI rather than through WinForms or GDI+, so that what lands in the bitmap is exactly what
	/// the operating system's GDI text renderer produces for the current system settings.
	/// </summary>
	public static class NativeMethods
	{
		#region Constants

		public const int DEFAULT_CHARSET = 1;
		public const int OUT_DEFAULT_PRECIS = 0;
		public const int CLIP_DEFAULT_PRECIS = 0;
		/// <summary>
		/// Font quality DEFAULT_QUALITY means "follow the system font smoothing settings", which is
		/// the whole point of these tests.  CLEARTYPE_QUALITY would override them.
		/// </summary>
		public const int DEFAULT_QUALITY = 0;
		public const int DEFAULT_PITCH = 0;
		public const int FF_DONTCARE = 0;
		public const int FW_NORMAL = 400;

		public const int TRANSPARENT = 1;
		public const int OPAQUE = 2;

		public const int BI_RGB = 0;
		public const uint DIB_RGB_COLORS = 0;

		public const uint MONITOR_DEFAULTTONULL = 0;
		public const uint MONITOR_DEFAULTTOPRIMARY = 1;
		public const uint MONITOR_DEFAULTTONEAREST = 2;

		#endregion

		#region Structs

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left, top, right, bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int x, y;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct LOGFONTW
		{
			public int lfHeight;
			public int lfWidth;
			public int lfEscapement;
			public int lfOrientation;
			public int lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string lfFaceName;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFOHEADER
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}

		/// <summary>
		/// BITMAPINFO with room for the single RGBQUAD that the C declaration ends with.  Only
		/// 32bpp BI_RGB bitmaps are created here, so the color table is never actually read.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFO
		{
			public BITMAPINFOHEADER bmiHeader;
			public uint bmiColors0;
		}

		#endregion

		#region gdi32

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint usage,
			out IntPtr ppvBits, IntPtr hSection, uint offset);

		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateSolidBrush(uint color);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateFontIndirectW")]
		public static extern IntPtr CreateFontIndirectW(ref LOGFONTW logFont);

		[DllImport("gdi32.dll")]
		public static extern uint SetTextColor(IntPtr hdc, uint color);

		[DllImport("gdi32.dll")]
		public static extern uint SetBkColor(IntPtr hdc, uint color);

		[DllImport("gdi32.dll")]
		public static extern int SetBkMode(IntPtr hdc, int mode);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "TextOutW")]
		public static extern bool TextOutW(IntPtr hdc, int x, int y, string text, int count);

		[DllImport("gdi32.dll")]
		public static extern bool GdiFlush();

		[DllImport("gdi32.dll")]
		public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h,
			IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

		public const uint SRCCOPY = 0x00CC0020;

		#endregion

		#region user32

		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

		[DllImport("user32.dll")]
		public static extern int FillRect(IntPtr hdc, ref RECT lprc, IntPtr hbr);

		[DllImport("user32.dll")]
		public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, UIntPtr pvParam, uint fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfoW(uint uiAction, bool uiParam, IntPtr pvParam, uint fWinIni);

		#endregion

		/// <summary>
		/// Converts a color to the 0x00BBGGRR COLORREF layout that GDI and DirectWrite expect.
		/// </summary>
		public static uint ToColorRef(System.Drawing.Color c)
		{
			return (uint)(c.R | (c.G << 8) | (c.B << 16));
		}
	}
}
