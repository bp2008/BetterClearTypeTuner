using System;
using System.Drawing;
using System.Drawing.Imaging;
using BCT_Tests.Native;

namespace BCT_Tests.Rendering
{
	/// <summary>
	/// Draws the sample lines with raw GDI (CreateFontIndirect + TextOut) onto a 32bpp DIB.
	///
	/// The font is created with DEFAULT_QUALITY, which is what ordinary GDI applications use and
	/// what makes GDI consult the system font-smoothing settings.  Nothing here overrides those
	/// settings, so whatever appears in the bitmap is what GDI decided to do with the machine's
	/// current configuration.
	/// </summary>
	internal static class GdiRenderer
	{
		public static Bitmap Render()
		{
			return Render(SampleText.Lines, SampleText.Width, SampleText.Height,
				SampleText.Foreground, SampleText.Background);
		}

		public static Bitmap Render(SampleLine[] lines, int width, int height, Color fore, Color back)
		{
			IntPtr hdcScreen = IntPtr.Zero;
			IntPtr hdcMem = IntPtr.Zero;
			IntPtr hBmp = IntPtr.Zero;
			IntPtr oldBmp = IntPtr.Zero;
			IntPtr brush = IntPtr.Zero;
			try
			{
				hdcScreen = NativeMethods.GetDC(IntPtr.Zero);
				hdcMem = NativeMethods.CreateCompatibleDC(hdcScreen);
				if (hdcMem == IntPtr.Zero)
					throw new Exception("CreateCompatibleDC failed.");

				NativeMethods.BITMAPINFO bmi = new NativeMethods.BITMAPINFO();
				bmi.bmiHeader.biSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.BITMAPINFOHEADER));
				bmi.bmiHeader.biWidth = width;
				// Negative height makes this a top-down DIB, so the bits are laid out the same way
				// System.Drawing expects and no row flipping is needed on the way out.
				bmi.bmiHeader.biHeight = -height;
				bmi.bmiHeader.biPlanes = 1;
				bmi.bmiHeader.biBitCount = 32;
				bmi.bmiHeader.biCompression = NativeMethods.BI_RGB;

				IntPtr bits;
				hBmp = NativeMethods.CreateDIBSection(hdcMem, ref bmi, NativeMethods.DIB_RGB_COLORS, out bits, IntPtr.Zero, 0);
				if (hBmp == IntPtr.Zero || bits == IntPtr.Zero)
					throw new Exception("CreateDIBSection failed.");
				oldBmp = NativeMethods.SelectObject(hdcMem, hBmp);

				// ClearType blends against the background that is already in the DC, so paint it
				// first and then draw the text opaquely over it.
				brush = NativeMethods.CreateSolidBrush(NativeMethods.ToColorRef(back));
				NativeMethods.RECT full = new NativeMethods.RECT { left = 0, top = 0, right = width, bottom = height };
				NativeMethods.FillRect(hdcMem, ref full, brush);

				NativeMethods.SetBkMode(hdcMem, NativeMethods.OPAQUE);
				NativeMethods.SetBkColor(hdcMem, NativeMethods.ToColorRef(back));
				NativeMethods.SetTextColor(hdcMem, NativeMethods.ToColorRef(fore));

				foreach (SampleLine line in lines)
				{
					NativeMethods.LOGFONTW lf = BuildLogFont(line);
					IntPtr hFont = NativeMethods.CreateFontIndirectW(ref lf);
					if (hFont == IntPtr.Zero)
						continue;
					IntPtr oldFont = NativeMethods.SelectObject(hdcMem, hFont);
					NativeMethods.TextOutW(hdcMem, SampleText.LeftMargin, line.Top, line.Text, line.Text.Length);
					NativeMethods.SelectObject(hdcMem, oldFont);
					NativeMethods.DeleteObject(hFont);
				}

				// GDI batches drawing calls; the DIB bits are not guaranteed to be up to date until
				// the batch is flushed.
				NativeMethods.GdiFlush();

				return CopyFromDibBits(bits, width, height);
			}
			finally
			{
				if (brush != IntPtr.Zero)
					NativeMethods.DeleteObject(brush);
				if (hdcMem != IntPtr.Zero)
				{
					if (oldBmp != IntPtr.Zero)
						NativeMethods.SelectObject(hdcMem, oldBmp);
					NativeMethods.DeleteDC(hdcMem);
				}
				if (hBmp != IntPtr.Zero)
					NativeMethods.DeleteObject(hBmp);
				if (hdcScreen != IntPtr.Zero)
					NativeMethods.ReleaseDC(IntPtr.Zero, hdcScreen);
			}
		}

		/// <summary>
		/// Copies the DIB's pixels into a bitmap that owns its own memory.  Bitmap.Clone is not an
		/// option here: GDI+ can hand back a "copy" that still points at the original scan0, which
		/// then dangles as soon as the DIB is deleted.
		/// </summary>
		private static Bitmap CopyFromDibBits(IntPtr bits, int width, int height)
		{
			Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppRgb);
			BitmapData data = result.LockBits(new Rectangle(0, 0, width, height),
				ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
			try
			{
				int[] row = new int[width];
				for (int y = 0; y < height; y++)
				{
					System.Runtime.InteropServices.Marshal.Copy(bits + (y * width * 4), row, 0, width);
					System.Runtime.InteropServices.Marshal.Copy(row, 0, data.Scan0 + (y * data.Stride), width);
				}
			}
			finally
			{
				result.UnlockBits(data);
			}
			return result;
		}

		/// <summary>
		/// Builds the LOGFONT for a sample line.  Both renderers use this, so they ask for the
		/// same typeface at the same size.
		/// </summary>
		public static NativeMethods.LOGFONTW BuildLogFont(SampleLine line)
		{
			return new NativeMethods.LOGFONTW
			{
				lfHeight = -(int)Math.Round(line.PixelSize),
				lfWidth = 0,
				lfEscapement = 0,
				lfOrientation = 0,
				lfWeight = NativeMethods.FW_NORMAL,
				lfItalic = 0,
				lfUnderline = 0,
				lfStrikeOut = 0,
				lfCharSet = NativeMethods.DEFAULT_CHARSET,
				lfOutPrecision = NativeMethods.OUT_DEFAULT_PRECIS,
				lfClipPrecision = NativeMethods.CLIP_DEFAULT_PRECIS,
				lfQuality = NativeMethods.DEFAULT_QUALITY,
				lfPitchAndFamily = NativeMethods.DEFAULT_PITCH | NativeMethods.FF_DONTCARE,
				lfFaceName = line.FaceName
			};
		}
	}
}
