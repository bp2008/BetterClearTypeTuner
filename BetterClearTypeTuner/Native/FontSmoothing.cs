using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterClearTypeTuner.Native
{
	public static class FontSmoothing
	{
		/// <summary>
		/// Returns true if font antialiasing is enabled.
		/// </summary>
		/// <returns></returns>
		public static bool GetAntialiasingEnabled()
		{
			bool enabled = false;
			User32.SystemParametersInfoW(SPI.SPI_GETFONTSMOOTHING, 0, ref enabled, SPIF.None);
			return enabled;
		}
		/// <summary>
		/// Enables or disables font antialiasing.
		/// </summary>
		/// <returns></returns>
		public static void SetAntialiasingEnabled(bool enabled)
		{
			User32.SystemParametersInfoW(SPI.SPI_SETFONTSMOOTHING, enabled, IntPtr.Zero, SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDCHANGE);
		}

		/// <summary>
		/// Returns the current FontSmoothingType.
		/// </summary>
		/// <returns></returns>
		public static FontSmoothingType GetFontSmoothingType()
		{
			uint type = uint.MaxValue;
			User32.SystemParametersInfoW(SPI.SPI_GETFONTSMOOTHINGTYPE, 0, ref type, SPIF.None);
			return (FontSmoothingType)type;
		}
		/// <summary>
		/// Sets the FontSmoothingType.
		/// </summary>
		/// <returns></returns>
		public static void SetFontSmoothingType(FontSmoothingType type)
		{
			User32.SystemParametersInfoW(SPI.SPI_SETFONTSMOOTHINGTYPE, 0, (uint)type, SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDCHANGE);
		}

		/// <summary>
		/// Gets the current font smoothing orientation (subpixel structure).
		/// </summary>
		/// <returns></returns>
		public static FontSmoothingOrientation GetFontSmoothingOrientation()
		{
			uint orientation = uint.MaxValue;
			User32.SystemParametersInfoW(SPI.SPI_GETFONTSMOOTHINGORIENTATION, 0, ref orientation, SPIF.None);
			return (FontSmoothingOrientation)orientation;
		}

		/// <summary>
		/// Sets the current font smoothing orientation (subpixel structure).
		/// </summary>
		/// <returns></returns>
		public static void SetFontSmoothingOrientation(FontSmoothingOrientation orientation)
		{
			User32.SystemParametersInfoW(SPI.SPI_SETFONTSMOOTHINGORIENTATION, 0, (uint)orientation, SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDCHANGE);
		}
		/// <summary>
		/// Returns the current font smoothing contrast.
		/// </summary>
		/// <returns></returns>
		public static uint GetContrast()
		{
			uint contrast = uint.MaxValue;
			User32.SystemParametersInfoW(SPI.SPI_GETFONTSMOOTHINGCONTRAST, 0, ref contrast, SPIF.None);
			return contrast;
		}
		/// <summary>
		/// Microsoft-documented valid range for ClearType contrast (SPI_SETFONTSMOOTHINGCONTRAST).
		/// Values outside this range break Java AWT/Swing (IllegalArgumentException on KEY_TEXT_LCD_CONTRAST).
		/// </summary>
		public const uint ContrastMin = 1000;
		public const uint ContrastMax = 2200;
		/// <summary>
		/// The default contrast value for ClearType (SPI_SETFONTSMOOTHINGCONTRAST) is a matter of some debate.  Windows API docs say 1400.  Experimental evidence shows that 1200 is what Windows actually ships with.  So we use 1200 as the default contrast value here.
		/// </summary>
		public const uint ContrastDefault = 1200;

		/// <summary>
		/// Sets the font smoothing contrast.
		/// Values are clamped to the documented 1000–2200 range.
		/// </summary>
		/// <returns></returns>
		public static void SetContrast(uint contrast)
		{
			if (contrast < ContrastMin)
				contrast = ContrastMin;
			else if (contrast > ContrastMax)
				contrast = ContrastMax;
			User32.SystemParametersInfoW(SPI.SPI_SETFONTSMOOTHINGCONTRAST, 0, contrast, SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDCHANGE);
		}
	}
	public enum FontSmoothingOrientation : uint
	{
		BGR = 0x0000,
		RGB = 0x0001,
		Unknown = uint.MaxValue
	}
	public enum FontSmoothingType : uint
	{
		Standard = 0x0001,
		ClearType = 0x0002,
		Unknown = uint.MaxValue
	}
}
