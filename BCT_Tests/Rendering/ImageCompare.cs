using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BCT_Tests.Rendering
{
	internal class ImageDifference
	{
		public int DifferentPixels;
		public int TotalPixels;
		public int MaxChannelDelta;
		/// <summary>Set when the two images could not be compared at all (missing or mismatched).</summary>
		public string Error;

		public bool AnyDifference { get { return Error == null && DifferentPixels > 0; } }

		public double PercentDifferent
		{
			get { return TotalPixels == 0 ? 0 : (DifferentPixels * 100.0) / TotalPixels; }
		}

		public override string ToString()
		{
			if (Error != null)
				return "error: " + Error;
			if (DifferentPixels == 0)
				return "identical";
			return DifferentPixels + " px (" + PercentDifferent.ToString("0.0") + "%), max channel delta " + MaxChannelDelta;
		}
	}

	internal static class ImageCompare
	{
		/// <summary>
		/// Compares two PNG files pixel for pixel.  The renders are deterministic for a given
		/// configuration, so "any difference at all" is a meaningful signal that the setting
		/// changed something; the magnitude is reported alongside it for context.
		/// </summary>
		public static ImageDifference CompareFiles(string pathA, string pathB)
		{
			ImageDifference diff = new ImageDifference();
			if (!System.IO.File.Exists(pathA) || !System.IO.File.Exists(pathB))
			{
				diff.Error = "one or both images are missing";
				return diff;
			}
			try
			{
				using (Bitmap a = new Bitmap(pathA))
				using (Bitmap b = new Bitmap(pathB))
					return Compare(a, b);
			}
			catch (Exception ex)
			{
				diff.Error = ex.Message;
				return diff;
			}
		}

		public static ImageDifference Compare(Bitmap a, Bitmap b)
		{
			ImageDifference diff = new ImageDifference();
			if (a.Width != b.Width || a.Height != b.Height)
			{
				diff.Error = "image sizes differ";
				return diff;
			}

			int[] pixelsA = GetPixels(a);
			int[] pixelsB = GetPixels(b);
			diff.TotalPixels = pixelsA.Length;
			for (int i = 0; i < pixelsA.Length; i++)
			{
				if (pixelsA[i] == pixelsB[i])
					continue;
				diff.DifferentPixels++;
				for (int shift = 0; shift <= 16; shift += 8)
				{
					int delta = Math.Abs(((pixelsA[i] >> shift) & 0xFF) - ((pixelsB[i] >> shift) & 0xFF));
					if (delta > diff.MaxChannelDelta)
						diff.MaxChannelDelta = delta;
				}
			}
			return diff;
		}

		private static int[] GetPixels(Bitmap bmp)
		{
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
			try
			{
				int[] raw = new int[bmp.Width * bmp.Height];
				// LockBits with an explicit pixel format gives a packed buffer whose stride may
				// still exceed the row width, so copy row by row rather than in one block.
				for (int y = 0; y < bmp.Height; y++)
					Marshal.Copy(data.Scan0 + (y * data.Stride), raw, y * bmp.Width, bmp.Width);
				// The alpha byte of Format32bppRgb is undefined; mask it off so it cannot show up
				// as a difference.
				for (int i = 0; i < raw.Length; i++)
					raw[i] &= 0x00FFFFFF;
				return raw;
			}
			finally
			{
				bmp.UnlockBits(data);
			}
		}
	}
}
