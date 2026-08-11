using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BetterClearTypeTuner.Native
{
	/// <summary>
	/// Which of DirectWrite's two rasterization paths a preview is drawn down.  The choice is not
	/// the user's and not the system's: every application makes it for itself, which is why both
	/// are previewed side by side rather than one being selected by the antialiasing mode.
	/// </summary>
	public enum DwPipeline
	{
		/// <summary>
		/// ClearType rasterization, the default for a bitmap render target.  Reads the ClearType
		/// level, the pixel geometry and the enhanced contrast.  Used by Firefox, Edge, WPF and
		/// most DirectWrite applications that draw to a window of their own.
		/// </summary>
		ClearType,

		/// <summary>
		/// Grayscale rasterization, reached only by asking for it with SetTextAntialiasMode.  Reads
		/// the grayscale enhanced contrast, and ignores the ClearType level and pixel geometry
		/// entirely.  Used by the Settings app, WinUI and Store apps, whose text is drawn onto
		/// composition surfaces that may be transparent, transformed or animated.
		/// </summary>
		Grayscale
	}

	/// <summary>
	/// Renders sample text with DirectWrite so the effect of settings that GDI ignores
	/// (most notably ClearType Level) can be previewed.
	///
	/// The rendering parameters are passed to DirectWrite per-draw via
	/// CreateCustomRenderingParams, so changes appear immediately without writing the registry and
	/// without restarting this application. Apps such as Firefox, Edge and WPF read the same values
	/// out of the registry at startup and feed them into the same API, which is why this preview
	/// matches what they will draw after a restart.
	///
	/// Both of DirectWrite's rasterization paths can be drawn; see <see cref="DwPipeline"/>.
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
			/// <summary>
			/// Enhanced contrast in the same 0-400 units the UI and registry use.  Only the
			/// ClearType path reads this one.
			/// </summary>
			public int EnhancedContrastLevel;
			/// <summary>
			/// Grayscale enhanced contrast in the same 0-400 units the UI and registry use.  Only
			/// the grayscale path reads this one.
			/// </summary>
			public int GrayscaleEnhancedContrastLevel;
		}

		private IDWriteFactory factory;
		/// <summary>
		/// The same factory asked for its DirectWrite 1.1 interface.  Null before Windows 8 without
		/// the Platform Update, in which case the grayscale path cannot be previewed.
		/// </summary>
		private IDWriteFactory1 factory1;
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

				// The same object under a newer interface, so it is not released separately.
				// Failure here is not an error: it only means this Windows is too old to have a
				// grayscale contrast setting for anything to read.
				try
				{
					factory1 = factory as IDWriteFactory1;
				}
				catch (Exception)
				{
					factory1 = null;
				}

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
		/// True if <see cref="DwPipeline.Grayscale"/> can be drawn, which needs DirectWrite 1.1.
		/// </summary>
		public bool GrayscaleAvailable { get { return Available && factory1 != null; } }

		/// <summary>
		/// Renders each string in <paramref name="texts"/> on its own line using the matching
		/// font in <paramref name="fonts"/>, and returns the result as a bitmap of the
		/// requested size. Returns null on failure, with the reason in LastError.
		/// </summary>
		public Bitmap Render(int width, int height, Font[] fonts, string[] texts, float dpiY,
			Color foreColor, Color backColor, Settings settings, DwPipeline pipeline)
		{
			if (!Available)
				return null;
			if (pipeline == DwPipeline.Grayscale && factory1 == null)
			{
				LastError = "Grayscale rasterization needs DirectWrite 1.1, which this version of Windows does not have.";
				return null;
			}
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

				renderingParams = CreateRenderingParams(settings, pipeline);
				if (renderingParams == null)
					return null;

				hr = gdiInterop.CreateBitmapRenderTarget(IntPtr.Zero, (uint)width, (uint)height, out target);
				if (hr != 0 || target == null)
				{
					LastError = "CreateBitmapRenderTarget failed (0x" + hr.ToString("X8") + ")";
					return null;
				}
				if (pipeline == DwPipeline.Grayscale)
				{
					// A new render target rasterizes ClearType, so the grayscale path has to be
					// asked for explicitly.  This is the whole difference between the two DirectWrite
					// previews: the parameters above are only read once the mode has decided which of
					// them apply.  The same object under a newer interface, so it is not released
					// separately from target.
					IDWriteBitmapRenderTarget1 target1 = target as IDWriteBitmapRenderTarget1;
					if (target1 == null)
					{
						LastError = "This render target does not support grayscale antialiasing.";
						return null;
					}
					hr = target1.SetTextAntialiasMode(DWRITE_TEXT_ANTIALIAS_MODE.GRAYSCALE);
					if (hr != 0)
					{
						LastError = "SetTextAntialiasMode failed (0x" + hr.ToString("X8") + ")";
						return null;
					}
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

		private IDWriteRenderingParams CreateRenderingParams(Settings settings, DwPipeline pipeline)
		{
			// Map the UI's units onto DirectWrite's. The pixel geometry enum values are
			// deliberately identical to the legacy PixelStructure registry values.
			float gamma = settings.GammaLevel / 1000f;
			if (gamma < 1f)
				gamma = 1f;
			else if (gamma > 2.2f)
				gamma = 2.2f;

			// DirectWrite reads both contrast levels in hundredths and ignores anything above 4.
			float enhancedContrast = Contrast(settings.EnhancedContrastLevel);
			float grayscaleEnhancedContrast = Contrast(settings.GrayscaleEnhancedContrastLevel);

			float clearTypeLevel = settings.ClearTypeLevel / 100f;
			if (clearTypeLevel < 0f)
				clearTypeLevel = 0f;
			else if (clearTypeLevel > 1f)
				clearTypeLevel = 1f;

			DWRITE_PIXEL_GEOMETRY geometry;
			if (settings.SmoothingType != FontSmoothingType.ClearType)
				geometry = DWRITE_PIXEL_GEOMETRY.FLAT;
			else if (settings.Orientation == FontSmoothingOrientation.BGR)
				geometry = DWRITE_PIXEL_GEOMETRY.BGR;
			else
				geometry = DWRITE_PIXEL_GEOMETRY.RGB;

			DWRITE_RENDERING_MODE mode = settings.AntialiasingEnabled
				? DWRITE_RENDERING_MODE.NATURAL_SYMMETRIC
				: DWRITE_RENDERING_MODE.ALIASED;

			int hr;
			if (pipeline == DwPipeline.Grayscale)
			{
				// Grayscale rasterization computes one coverage value per pixel, so there is no
				// subpixel structure to blend across and neither the ClearType level nor the pixel
				// geometry is consulted.  They are passed as their inert values to make that plain.
				IDWriteRenderingParams1 grayscaleResult;
				hr = factory1.CreateCustomRenderingParams(gamma, enhancedContrast, grayscaleEnhancedContrast,
					0f, DWRITE_PIXEL_GEOMETRY.FLAT, mode, out grayscaleResult);
				if (hr != 0 || grayscaleResult == null)
				{
					LastError = "IDWriteFactory1::CreateCustomRenderingParams failed (0x" + hr.ToString("X8") + ")";
					return null;
				}
				// The same object under the interface DrawGlyphRun takes, not a second one.
				return (IDWriteRenderingParams)grayscaleResult;
			}

			// The five-argument overload is deliberate rather than a leftover: it is what a
			// DirectWrite application written against the original interface calls, and the
			// grayscale contrast it cannot express is one this path would not read anyway.
			IDWriteRenderingParams result;
			hr = factory.CreateCustomRenderingParams(gamma, enhancedContrast, clearTypeLevel, geometry, mode, out result);
			if (hr != 0 || result == null)
			{
				LastError = "CreateCustomRenderingParams failed (0x" + hr.ToString("X8") + ")";
				return null;
			}
			return result;
		}

		/// <summary>Converts one of the UI's 0-400 contrast values into DirectWrite's units.</summary>
		private static float Contrast(int level)
		{
			float value = level / 100f;
			if (value < 0f)
				return 0f;
			if (value > 4f)
				return 4f;
			return value;
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
			// factory1 is the same underlying object as factory, so releasing factory covers both.
			factory1 = null;
			ReleaseCom(factory);
			gdiInterop = null;
			factory = null;
		}
	}
}
