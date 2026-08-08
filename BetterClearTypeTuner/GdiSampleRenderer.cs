using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SdBitmap = System.Drawing.Bitmap;
using SdPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace BetterClearTypeTuner;

/// <summary>
/// Renders sample text with GDI+ using the system text-rendering hint so the preview
/// tracks Windows font-smoothing changes (unlike Avalonia/Skia chrome text).
/// </summary>
internal static class GdiSampleRenderer
{
	public const string DefaultSample = "The quick brown fox jumps over the lazy dog.";

	public static WriteableBitmap RenderZoomed(string fontFamily, bool darkBackground, int width = 160, int height = 120, int scale = 4)
	{
		using var src = RenderGdiBitmap(fontFamily, darkBackground, width, height);
		using var scaled = ScaleNearest(src, scale);
		return ToAvaloniaBitmap(scaled);
	}

	public static WriteableBitmap RenderNormal(string fontFamily, bool darkBackground, int width = 160, int height = 120)
	{
		using var src = RenderGdiBitmap(fontFamily, darkBackground, width, height);
		return ToAvaloniaBitmap(src);
	}

	private static SdBitmap RenderGdiBitmap(string fontFamily, bool darkBackground, int width, int height)
	{
		Color back = darkBackground ? Color.Black : Color.White;
		Color fore = darkBackground ? Color.White : Color.Black;

		var src = new SdBitmap(width, height, SdPixelFormat.Format32bppArgb);
		using var g = Graphics.FromImage(src);
		g.Clear(back);
		// SystemDefault follows SPI font-smoothing (ClearType / grayscale / off).
		g.TextRenderingHint = TextRenderingHint.SystemDefault;
		g.PageUnit = GraphicsUnit.Pixel;

		DrawLine(g, DefaultSample, fontFamily, 11f, fore, new RectangleF(2, 2, width - 4, 32));
		DrawLine(g, DefaultSample, fontFamily, 13f, fore, new RectangleF(2, 34, width - 4, 36));
		DrawLine(g, "The quick brown fox jumps over the lazy", fontFamily, 16f, fore, new RectangleF(2, 72, width - 4, 44));
		return src;
	}

	private static void DrawLine(Graphics g, string text, string fontFamily, float pixelSize, Color fore, RectangleF bounds)
	{
		using var font = new Font(fontFamily, pixelSize, FontStyle.Regular, GraphicsUnit.Pixel);
		using var brush = new SolidBrush(fore);
		g.DrawString(text, font, brush, bounds);
	}

	private static SdBitmap ScaleNearest(SdBitmap src, int scale)
	{
		int w = src.Width * scale;
		int h = src.Height * scale;
		var dst = new SdBitmap(w, h, SdPixelFormat.Format32bppArgb);
		using var g = Graphics.FromImage(dst);
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
		g.Clear(Color.Transparent);
		g.DrawImage(src, new Rectangle(0, 0, w, h));
		return dst;
	}

	private static WriteableBitmap ToAvaloniaBitmap(SdBitmap src)
	{
		var bitmapData = src.LockBits(
			new Rectangle(0, 0, src.Width, src.Height),
			ImageLockMode.ReadOnly,
			SdPixelFormat.Format32bppArgb);
		try
		{
			var wb = new WriteableBitmap(
				new Avalonia.PixelSize(src.Width, src.Height),
				new Avalonia.Vector(96, 96),
				Avalonia.Platform.PixelFormat.Bgra8888,
				AlphaFormat.Unpremul);

			using var fb = wb.Lock();
			int height = src.Height;
			int srcStride = bitmapData.Stride;
			int dstStride = fb.RowBytes;
			int rowBytes = Math.Min(srcStride, dstStride);
			var buffer = new byte[rowBytes];
			for (int y = 0; y < height; y++)
			{
				Marshal.Copy(bitmapData.Scan0 + y * srcStride, buffer, 0, rowBytes);
				Marshal.Copy(buffer, 0, fb.Address + y * dstStride, rowBytes);
			}

			return wb;
		}
		finally
		{
			src.UnlockBits(bitmapData);
		}
	}
}
