using System;
using BCT_Tests.Native;

namespace BCT_Tests.Settings
{
	internal enum SmoothingType : uint
	{
		Standard = 1,
		ClearType = 2
	}

	internal enum SmoothingOrientation : uint
	{
		BGR = 0,
		RGB = 1
	}

	/// <summary>
	/// The system-wide font smoothing settings, read and written through SystemParametersInfo the
	/// same way the main application does.  These are the settings that GDI actually consults, and
	/// the ones WPF and DirectWrite fall back to when no Avalon.Graphics values are set.
	/// </summary>
	internal static class SystemFontSmoothing
	{
		private const uint SPI_GETFONTSMOOTHING = 0x004A;
		private const uint SPI_SETFONTSMOOTHING = 0x004B;
		private const uint SPI_GETFONTSMOOTHINGTYPE = 0x200A;
		private const uint SPI_SETFONTSMOOTHINGTYPE = 0x200B;
		private const uint SPI_GETFONTSMOOTHINGCONTRAST = 0x200C;
		private const uint SPI_SETFONTSMOOTHINGCONTRAST = 0x200D;
		private const uint SPI_GETFONTSMOOTHINGORIENTATION = 0x2012;
		private const uint SPI_SETFONTSMOOTHINGORIENTATION = 0x2013;

		private const uint SPIF_UPDATEINIFILE = 0x01;
		private const uint SPIF_SENDCHANGE = 0x02;
		private const uint WriteFlags = SPIF_UPDATEINIFILE | SPIF_SENDCHANGE;

		public static bool GetAntialiasingEnabled()
		{
			bool enabled = false;
			NativeMethods.SystemParametersInfoW(SPI_GETFONTSMOOTHING, 0, ref enabled, 0);
			return enabled;
		}

		public static void SetAntialiasingEnabled(bool enabled)
		{
			NativeMethods.SystemParametersInfoW(SPI_SETFONTSMOOTHING, enabled, IntPtr.Zero, WriteFlags);
		}

		public static SmoothingType GetSmoothingType()
		{
			uint type = 0;
			NativeMethods.SystemParametersInfoW(SPI_GETFONTSMOOTHINGTYPE, 0, ref type, 0);
			return (SmoothingType)type;
		}

		public static void SetSmoothingType(SmoothingType type)
		{
			NativeMethods.SystemParametersInfoW(SPI_SETFONTSMOOTHINGTYPE, 0, new UIntPtr((uint)type), WriteFlags);
		}

		public static SmoothingOrientation GetOrientation()
		{
			uint orientation = 0;
			NativeMethods.SystemParametersInfoW(SPI_GETFONTSMOOTHINGORIENTATION, 0, ref orientation, 0);
			return (SmoothingOrientation)orientation;
		}

		public static void SetOrientation(SmoothingOrientation orientation)
		{
			NativeMethods.SystemParametersInfoW(SPI_SETFONTSMOOTHINGORIENTATION, 0, new UIntPtr((uint)orientation), WriteFlags);
		}

		public static uint GetContrast()
		{
			uint contrast = 0;
			NativeMethods.SystemParametersInfoW(SPI_GETFONTSMOOTHINGCONTRAST, 0, ref contrast, 0);
			return contrast;
		}

		public static void SetContrast(uint contrast)
		{
			NativeMethods.SystemParametersInfoW(SPI_SETFONTSMOOTHINGCONTRAST, 0, new UIntPtr(contrast), WriteFlags);
		}

		public static string Describe()
		{
			if (!GetAntialiasingEnabled())
				return "antialiasing off";
			SmoothingType type = GetSmoothingType();
			if (type == SmoothingType.ClearType)
				return "ClearType " + GetOrientation() + ", contrast " + GetContrast();
			return "grayscale, contrast " + GetContrast();
		}
	}
}
