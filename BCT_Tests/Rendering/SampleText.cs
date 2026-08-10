using System;
using System.Drawing;

namespace BCT_Tests.Rendering
{
	/// <summary>
	/// One line of the sample image.  Both renderers draw exactly these lines at exactly these
	/// positions, so that a pixel-for-pixel comparison of two runs shows only the effect of the
	/// setting that changed between them.
	/// </summary>
	internal class SampleLine
	{
		public string FaceName;
		public float PointSize;
		/// <summary>Y coordinate of the top of the line box, in pixels.</summary>
		public int Top;
		public string Text;

		public SampleLine(string faceName, float pointSize, int top, string text)
		{
			FaceName = faceName;
			PointSize = pointSize;
			Top = top;
			Text = text;
		}

		/// <summary>Em size in pixels at <see cref="SampleText.Dpi"/>.</summary>
		public float PixelSize { get { return PointSize * SampleText.Dpi / 72f; } }
	}

	internal static class SampleText
	{
		/// <summary>
		/// Everything is rendered at a fixed 96 DPI into an offscreen bitmap, so the results do
		/// not depend on which monitor the harness happens to be running on or on its DPI scaling.
		/// </summary>
		public const float Dpi = 96f;

		public const int Width = 380;
		public const int Height = 145;

		public const int LeftMargin = 5;

		public static readonly Color Foreground = Color.Black;
		public static readonly Color Background = Color.White;

		public static readonly SampleLine[] Lines = new SampleLine[]
		{
			new SampleLine("Segoe UI", 9f, 6, "The Wizard's lily box. 0123456789"),
			new SampleLine("Segoe UI", 12f, 30, "The Wizard's lily box. iIlL1"),
			new SampleLine("Segoe UI", 16f, 60, "Sphinx of black quartz"),
			new SampleLine("Segoe UI", 24f, 96, "Hamburgefonstiv")
		};
	}
}
