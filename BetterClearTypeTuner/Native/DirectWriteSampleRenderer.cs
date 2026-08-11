using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BetterClearTypeTuner.Native
{
	/// <summary>
	/// Renders sample text with DirectWrite so the effect of settings that GDI ignores
	/// (most notably ClearType Level) can be previewed.
	///
	/// The rendering parameters are passed to DirectWrite per-draw via
	/// IDWriteFactory::CreateCustomRenderingParams, so changes appear immediately without
	/// writing the registry and without restarting this application. Apps such as Firefox,
	/// Edge and WPF read the same values out of the registry at startup and feed them into
	/// the same API, which is why this preview matches what they will draw after a restart.
	/// </summary>
	public class DirectWriteSampleRenderer : IDisposable
	{
		#region GDI interop

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int left, top, right, bottom;
		}

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateSolidBrush(uint color);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		[DllImport("user32.dll")]
		private static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hbr);

		[DllImport("gdi32.dll")]
		private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h,
			IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

		private const uint SRCCOPY = 0x00CC0020;

		#endregion

		/// <summary>
		/// The font-smoothing state to preview. These are the live control values, not
		/// whatever happens to be stored in the registry.
		/// </summary>
		public struct Settings
		{
			public bool AntialiasingEnabled;
			public FontSmoothingType SmoothingType;
			public FontSmoothingOrientation Orientation;
			/// <summary>
			/// DirectWrite contrast in the same 1000-2200 units the UI and the GammaLevel
			/// registry value use.  This is not the GDI contrast, which DirectWrite never reads.
			/// </summary>
			public uint GammaLevel;
			/// <summary>ClearType level in the same 0-100 units the UI and registry use.</summary>
			public int ClearTypeLevel;
			/// <summary>Enhanced contrast in the same 0-400 units the UI and registry use.</summary>
			public int EnhancedContrastLevel;
		}

		private IDWriteFactory factory;
		private IDWriteGdiInterop gdiInterop;
		private bool disposed;

		/// <summary>
		/// Set when construction or a render failed. The UI shows this instead of a preview.
		/// </summary>
		public string LastError { get; private set; }

		public DirectWriteSampleRenderer()
		{
			try
			{
				object factoryObj;
				int hr = DWrite.DWriteCreateFactory(DWRITE_FACTORY_TYPE.SHARED, DWrite.IID_IDWriteFactory, out factoryObj);
				if (hr != 0 || factoryObj == null)
				{
					LastError = "DWriteCreateFactory failed (0x" + hr.ToString("X8") + ")";
					return;
				}
				factory = (IDWriteFactory)factoryObj;

				hr = factory.GetGdiInterop(out gdiInterop);
				if (hr != 0 || gdiInterop == null)
				{
					LastError = "IDWriteFactory::GetGdiInterop failed (0x" + hr.ToString("X8") + ")";
					gdiInterop = null;
				}
			}
			catch (Exception ex)
			{
				LastError = ex.Message;
			}
		}

		public bool Available { get { return factory != null && gdiInterop != null; } }

		/// <summary>
		/// Renders each string in <paramref name="texts"/> on its own line using the matching
		/// font in <paramref name="fonts"/>, and returns the result as a bitmap of the
		/// requested size. Returns null on failure, with the reason in LastError.
		/// </summary>
		public Bitmap Render(int width, int height, Font[] fonts, string[] texts, float dpiY,
			Color foreColor, Color backColor, Settings settings)
		{
			if (!Available)
				return null;
			if (fonts == null || texts == null || fonts.Length != texts.Length || fonts.Length == 0)
				return null;
			if (width <= 0 || height <= 0)
				return null;

			IDWriteFont dwFont = null;
			IDWriteFontFace fontFace = null;
			IDWriteRenderingParams renderingParams = null;
			IDWriteBitmapRenderTarget target = null;
			IntPtr fontFacePtr = IntPtr.Zero;
			IntPtr brush = IntPtr.Zero;

			try
			{
				// The face name, weight and italic flag come from the font the user picked;
				// the size is supplied per-line as fontEmSize instead.
				LOGFONTW logFont = new LOGFONTW();
				object boxed = logFont;
				fonts[0].ToLogFont(boxed);
				logFont = (LOGFONTW)boxed;

				int hr = gdiInterop.CreateFontFromLOGFONT(ref logFont, out dwFont);
				if (hr != 0 || dwFont == null)
				{
					LastError = "This font is not available to DirectWrite (0x" + hr.ToString("X8") + ")";
					return null;
				}

				hr = dwFont.CreateFontFace(out fontFace);
				if (hr != 0 || fontFace == null)
				{
					LastError = "CreateFontFace failed (0x" + hr.ToString("X8") + ")";
					return null;
				}

				DWRITE_FONT_METRICS fontMetrics;
				fontFace.GetMetrics(out fontMetrics);
				if (fontMetrics.designUnitsPerEm == 0)
				{
					LastError = "Font reported no design units per em.";
					return null;
				}

				renderingParams = CreateRenderingParams(settings);
				if (renderingParams == null)
					return null;

				hr = gdiInterop.CreateBitmapRenderTarget(IntPtr.Zero, (uint)width, (uint)height, out target);
				if (hr != 0 || target == null)
				{
					LastError = "CreateBitmapRenderTarget failed (0x" + hr.ToString("X8") + ")";
					return null;
				}
				// Work in raw pixels: em sizes below are already converted from points.
				target.SetPixelsPerDip(1f);

				IntPtr hdc = target.GetMemoryDC();
				if (hdc == IntPtr.Zero)
				{
					LastError = "GetMemoryDC returned null.";
					return null;
				}

				// The render target's DIB starts out zeroed (black), and DrawGlyphRun blends
				// onto whatever is already there, so the background must be painted first.
				brush = CreateSolidBrush(ToColorRef(backColor));
				RECT full = new RECT { left = 0, top = 0, right = width, bottom = height };
				FillRect(hdc, ref full, brush);

				fontFacePtr = Marshal.GetComInterfaceForObject(fontFace, typeof(IDWriteFontFace));
				uint textColor = ToColorRef(foreColor);

				// Aliased rendering ignores the ClearType parameters entirely, and its glyph
				// advances are whole pixels, so measure it the way it is drawn.
				DWRITE_MEASURING_MODE measuringMode = settings.AntialiasingEnabled
					? DWRITE_MEASURING_MODE.NATURAL
					: DWRITE_MEASURING_MODE.GDI_CLASSIC;

				float penY = 0f;
				for (int i = 0; i < texts.Length; i++)
				{
					float emPx = fonts[i].SizeInPoints * dpiY / 72f;
					float scale = emPx / fontMetrics.designUnitsPerEm;
					float ascentPx = fontMetrics.ascent * scale;
					float lineHeightPx = (fontMetrics.ascent + fontMetrics.descent + fontMetrics.lineGap) * scale;

					DrawLine(target, fontFacePtr, fontFace, texts[i], emPx, scale,
						1f, penY + ascentPx, measuringMode, renderingParams, textColor);

					penY += lineHeightPx;
				}

				// Copy the DIB out into a managed bitmap for display and magnification.
				Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppRgb);
				using (Graphics g = Graphics.FromImage(result))
				{
					IntPtr destHdc = g.GetHdc();
					try
					{
						BitBlt(destHdc, 0, 0, width, height, hdc, 0, 0, SRCCOPY);
					}
					finally
					{
						g.ReleaseHdc(destHdc);
					}
				}
				LastError = null;
				return result;
			}
			catch (Exception ex)
			{
				LastError = ex.Message;
				return null;
			}
			finally
			{
				if (fontFacePtr != IntPtr.Zero)
					Marshal.Release(fontFacePtr);
				if (brush != IntPtr.Zero)
					DeleteObject(brush);
				ReleaseCom(target);
				ReleaseCom(renderingParams);
				ReleaseCom(fontFace);
				ReleaseCom(dwFont);
			}
		}

		private void DrawLine(IDWriteBitmapRenderTarget target, IntPtr fontFacePtr, IDWriteFontFace fontFace,
			string text, float emPx, float designToPixel, float baselineX, float baselineY,
			DWRITE_MEASURING_MODE measuringMode, IDWriteRenderingParams renderingParams, uint textColor)
		{
			if (string.IsNullOrEmpty(text))
				return;

			// The sample strings are plain left-to-right text, so a direct codepoint-to-glyph
			// mapping is sufficient; no shaping, kerning or bidi reordering is applied.
			uint[] codePoints = new uint[text.Length];
			for (int i = 0; i < text.Length; i++)
				codePoints[i] = text[i];

			ushort[] glyphIndices = new ushort[text.Length];
			if (fontFace.GetGlyphIndices(codePoints, (uint)codePoints.Length, glyphIndices) != 0)
				return;

			DWRITE_GLYPH_METRICS[] glyphMetrics = new DWRITE_GLYPH_METRICS[text.Length];
			if (fontFace.GetDesignGlyphMetrics(glyphIndices, (uint)glyphIndices.Length, glyphMetrics, 0) != 0)
				return;

			float[] advances = new float[text.Length];
			for (int i = 0; i < text.Length; i++)
				advances[i] = glyphMetrics[i].advanceWidth * designToPixel;

			GCHandle indicesHandle = GCHandle.Alloc(glyphIndices, GCHandleType.Pinned);
			GCHandle advancesHandle = GCHandle.Alloc(advances, GCHandleType.Pinned);
			try
			{
				DWRITE_GLYPH_RUN run = new DWRITE_GLYPH_RUN
				{
					fontFace = fontFacePtr,
					fontEmSize = emPx,
					glyphCount = (uint)glyphIndices.Length,
					glyphIndices = indicesHandle.AddrOfPinnedObject(),
					glyphAdvances = advancesHandle.AddrOfPinnedObject(),
					glyphOffsets = IntPtr.Zero,
					isSideways = 0,
					bidiLevel = 0
				};
				target.DrawGlyphRun(baselineX, baselineY, measuringMode, ref run, renderingParams, textColor, IntPtr.Zero);
			}
			finally
			{
				indicesHandle.Free();
				advancesHandle.Free();
			}
		}

		private IDWriteRenderingParams CreateRenderingParams(Settings settings)
		{
			// Map the UI's units onto DirectWrite's. The pixel geometry enum values are
			// deliberately identical to the legacy PixelStructure registry values.
			float gamma = settings.GammaLevel / 1000f;
			if (gamma < 1f)
				gamma = 1f;
			else if (gamma > 2.2f)
				gamma = 2.2f;

			// DirectWrite reads EnhancedContrastLevel in hundredths and ignores anything above 4.
			float enhancedContrast = settings.EnhancedContrastLevel / 100f;
			if (enhancedContrast < 0f)
				enhancedContrast = 0f;
			else if (enhancedContrast > 4f)
				enhancedContrast = 4f;

			float clearTypeLevel = settings.ClearTypeLevel / 100f;
			if (clearTypeLevel < 0f)
				clearTypeLevel = 0f;
			else if (clearTypeLevel > 1f)
				clearTypeLevel = 1f;

			DWRITE_PIXEL_GEOMETRY geometry;
			if (settings.SmoothingType != FontSmoothingType.ClearType)
			{
				// Grayscale antialiasing: no subpixel structure and no color fringing.
				geometry = DWRITE_PIXEL_GEOMETRY.FLAT;
				clearTypeLevel = 0f;
			}
			else if (settings.Orientation == FontSmoothingOrientation.BGR)
				geometry = DWRITE_PIXEL_GEOMETRY.BGR;
			else
				geometry = DWRITE_PIXEL_GEOMETRY.RGB;

			DWRITE_RENDERING_MODE mode = settings.AntialiasingEnabled
				? DWRITE_RENDERING_MODE.NATURAL_SYMMETRIC
				: DWRITE_RENDERING_MODE.ALIASED;

			IDWriteRenderingParams result;
			int hr = factory.CreateCustomRenderingParams(gamma, enhancedContrast, clearTypeLevel, geometry, mode, out result);
			if (hr != 0 || result == null)
			{
				LastError = "CreateCustomRenderingParams failed (0x" + hr.ToString("X8") + ")";
				return null;
			}
			return result;
		}

		private static uint ToColorRef(Color c)
		{
			return (uint)(c.R | (c.G << 8) | (c.B << 16));
		}

		private static void ReleaseCom(object o)
		{
			if (o != null && Marshal.IsComObject(o))
				Marshal.ReleaseComObject(o);
		}

		public void Dispose()
		{
			if (disposed)
				return;
			disposed = true;
			ReleaseCom(gdiInterop);
			ReleaseCom(factory);
			gdiInterop = null;
			factory = null;
		}
	}
}
