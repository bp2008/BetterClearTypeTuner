using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using BCT_Tests.Native;
using BCT_Tests.Settings;

namespace BCT_Tests.Rendering
{
	/// <summary>
	/// The rendering parameters DirectWrite hands out for the current system configuration.
	/// These are read back from the object rather than computed, so they show directly which
	/// registry values DirectWrite folded into its defaults.
	/// </summary>
	internal class DwRenderingParamValues
	{
		public float Gamma;
		public float EnhancedContrast;
		public float ClearTypeLevel;
		public DWRITE_PIXEL_GEOMETRY PixelGeometry;
		public DWRITE_RENDERING_MODE RenderingMode;
		/// <summary>Null when IDWriteRenderingParams1 is unavailable (before Windows 8.1).</summary>
		public float? GrayscaleEnhancedContrast;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("gamma=").Append(Gamma.ToString("0.####"));
			sb.Append(" enhancedContrast=").Append(EnhancedContrast.ToString("0.####"));
			sb.Append(" clearTypeLevel=").Append(ClearTypeLevel.ToString("0.####"));
			sb.Append(" pixelGeometry=").Append(PixelGeometry);
			sb.Append(" renderingMode=").Append(RenderingMode);
			sb.Append(" grayscaleEnhancedContrast=")
				.Append(GrayscaleEnhancedContrast.HasValue ? GrayscaleEnhancedContrast.Value.ToString("0.####") : "n/a");
			return sb.ToString();
		}
	}

	internal class DwRenderResult
	{
		public Bitmap Image;
		/// <summary>Defaults for the primary monitor (IDWriteFactory::CreateMonitorRenderingParams).</summary>
		public DwRenderingParamValues MonitorParams;
		/// <summary>Defaults with no monitor specified (IDWriteFactory::CreateRenderingParams).</summary>
		public DwRenderingParamValues DefaultParams;
		/// <summary>The parameters actually used to draw, which differ from MonitorParams in AppConfigured mode.</summary>
		public DwRenderingParamValues EffectiveParams;
		public string Error;
	}

	/// <summary>
	/// How the DirectWrite sample decides what to draw.
	/// </summary>
	internal enum DwRenderMode
	{
		/// <summary>
		/// Hand IDWriteFactory::CreateMonitorRenderingParams straight to DrawGlyphRun and change
		/// nothing.  This shows what DirectWrite derives on its own, which is purely a function of
		/// the Avalon.Graphics registry values: those parameters never reflect
		/// SPI_GETFONTSMOOTHING or SPI_GETFONTSMOOTHINGTYPE.
		/// </summary>
		SystemDefaults,

		/// <summary>
		/// Do what a real DirectWrite client does: take the tuning parameters from DirectWrite, but
		/// pick the rendering mode from the system font-smoothing settings, because DirectWrite will
		/// not do that for you.  This is the same policy this repository's own
		/// DirectWriteSampleRenderer applies, and it is what makes a DirectWrite application follow
		/// the user's choice of no antialiasing / grayscale / ClearType.
		/// </summary>
		AppConfigured
	}

	/// <summary>
	/// Draws the sample lines with DirectWrite through IDWriteBitmapRenderTarget.
	///
	/// Each configuration is rendered in a freshly launched process so that no cached parameters
	/// from a previous configuration can leak into the result.  See <see cref="DwRenderMode"/> for
	/// why the sample is drawn twice.
	/// </summary>
	internal static class DirectWriteRenderer
	{
		public static DwRenderResult Render(DwRenderMode mode)
		{
			return Render(SampleText.Lines, SampleText.Width, SampleText.Height,
				SampleText.Foreground, SampleText.Background, mode);
		}

		public static DwRenderResult Render(SampleLine[] lines, int width, int height, Color fore, Color back,
			DwRenderMode mode)
		{
			DwRenderResult result = new DwRenderResult();

			IDWriteFactory factory = null;
			IDWriteGdiInterop gdiInterop = null;
			IDWriteRenderingParams monitorParams = null;
			IDWriteRenderingParams defaultParams = null;
			IDWriteRenderingParams appParams = null;
			IDWriteBitmapRenderTarget target = null;
			IntPtr brush = IntPtr.Zero;

			try
			{
				object factoryObj;
				// ISOLATED rather than SHARED: an isolated factory has its own state and its own
				// copy of the system defaults, which keeps this process from picking up anything
				// another component in the process may already have cached.
				int hr = DWrite.DWriteCreateFactory(DWRITE_FACTORY_TYPE.ISOLATED, DWrite.IID_IDWriteFactory, out factoryObj);
				if (hr != 0 || factoryObj == null)
				{
					result.Error = "DWriteCreateFactory failed (0x" + hr.ToString("X8") + ")";
					return result;
				}
				factory = (IDWriteFactory)factoryObj;

				hr = factory.GetGdiInterop(out gdiInterop);
				if (hr != 0 || gdiInterop == null)
				{
					result.Error = "GetGdiInterop failed (0x" + hr.ToString("X8") + ")";
					return result;
				}

				IntPtr primaryMonitor = NativeMethods.MonitorFromPoint(
					new NativeMethods.POINT { x = 0, y = 0 }, NativeMethods.MONITOR_DEFAULTTOPRIMARY);
				hr = factory.CreateMonitorRenderingParams(primaryMonitor, out monitorParams);
				if (hr != 0 || monitorParams == null)
				{
					result.Error = "CreateMonitorRenderingParams failed (0x" + hr.ToString("X8") + ")";
					return result;
				}
				result.MonitorParams = ReadParams(monitorParams);

				if (factory.CreateRenderingParams(out defaultParams) == 0 && defaultParams != null)
					result.DefaultParams = ReadParams(defaultParams);

				IDWriteRenderingParams drawParams = monitorParams;
				DWRITE_MEASURING_MODE measuringMode = DWRITE_MEASURING_MODE.NATURAL;
				if (mode == DwRenderMode.AppConfigured)
				{
					appParams = BuildAppConfiguredParams(factory, result.MonitorParams, result);
					if (appParams == null)
						return result;
					drawParams = appParams;
					// Aliased rendering places glyphs on whole pixels, so it has to be measured the
					// way it is drawn; this mirrors DirectWriteSampleRenderer in the main project.
					if (!SystemFontSmoothing.GetAntialiasingEnabled())
						measuringMode = DWRITE_MEASURING_MODE.GDI_CLASSIC;
				}
				result.EffectiveParams = ReadParams(drawParams);

				hr = gdiInterop.CreateBitmapRenderTarget(IntPtr.Zero, (uint)width, (uint)height, out target);
				if (hr != 0 || target == null)
				{
					result.Error = "CreateBitmapRenderTarget failed (0x" + hr.ToString("X8") + ")";
					return result;
				}
				// Work in raw pixels; the em sizes below are already converted from points.
				target.SetPixelsPerDip(1f);

				IntPtr hdc = target.GetMemoryDC();
				if (hdc == IntPtr.Zero)
				{
					result.Error = "GetMemoryDC returned null.";
					return result;
				}

				// The render target's DIB starts out zeroed (black) and DrawGlyphRun blends onto
				// whatever is already there, so the background has to be painted first.
				brush = NativeMethods.CreateSolidBrush(NativeMethods.ToColorRef(back));
				NativeMethods.RECT full = new NativeMethods.RECT { left = 0, top = 0, right = width, bottom = height };
				NativeMethods.FillRect(hdc, ref full, brush);

				uint textColor = NativeMethods.ToColorRef(fore);
				foreach (SampleLine line in lines)
				{
					string lineError = DrawLine(gdiInterop, target, line, drawParams, measuringMode, textColor);
					if (lineError != null && result.Error == null)
						result.Error = lineError;
				}

				Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					IntPtr destHdc = g.GetHdc();
					try
					{
						NativeMethods.BitBlt(destHdc, 0, 0, width, height, hdc, 0, 0, NativeMethods.SRCCOPY);
					}
					finally
					{
						g.ReleaseHdc(destHdc);
					}
				}
				result.Image = bitmap;
				return result;
			}
			catch (Exception ex)
			{
				result.Error = ex.ToString();
				return result;
			}
			finally
			{
				if (brush != IntPtr.Zero)
					NativeMethods.DeleteObject(brush);
				ReleaseCom(target);
				ReleaseCom(appParams);
				ReleaseCom(defaultParams);
				ReleaseCom(monitorParams);
				ReleaseCom(gdiInterop);
				ReleaseCom(factory);
			}
		}

		/// <summary>
		/// Draws one sample line.  Returns null on success or a description of what went wrong.
		/// </summary>
		/// <summary>
		/// Builds the parameters a real DirectWrite client would draw with.
		///
		/// The tuning values — gamma, enhanced contrast, ClearType level, pixel geometry — are kept
		/// exactly as DirectWrite derived them from the registry, so the registry sweep still
		/// measures what it is supposed to measure.  Only the antialiasing decision is taken over,
		/// because that decision is not DirectWrite's to make: the system font-smoothing settings
		/// never reach IDWriteRenderingParams, so an application that wants to honour them has to
		/// read them itself and say so.
		///
		///   antialiasing off  ->  DWRITE_RENDERING_MODE_ALIASED
		///   grayscale         ->  antialiased with the ClearType level forced to 0, which is how
		///                         DirectWrite is told to produce grayscale coverage
		///   ClearType         ->  antialiased with the registry's ClearType level and geometry
		/// </summary>
		private static IDWriteRenderingParams BuildAppConfiguredParams(IDWriteFactory factory,
			DwRenderingParamValues monitor, DwRenderResult result)
		{
			bool antialiasingEnabled = SystemFontSmoothing.GetAntialiasingEnabled();
			bool clearType = SystemFontSmoothing.GetSmoothingType() == SmoothingType.ClearType;

			float clearTypeLevel = clearType ? monitor.ClearTypeLevel : 0f;
			DWRITE_RENDERING_MODE renderingMode = antialiasingEnabled
				? DWRITE_RENDERING_MODE.NATURAL_SYMMETRIC
				: DWRITE_RENDERING_MODE.ALIASED;

			IDWriteRenderingParams appParams;
			int hr = factory.CreateCustomRenderingParams(monitor.Gamma, monitor.EnhancedContrast, clearTypeLevel,
				monitor.PixelGeometry, renderingMode, out appParams);
			if (hr != 0 || appParams == null)
			{
				result.Error = "CreateCustomRenderingParams failed (0x" + hr.ToString("X8") + ")";
				return null;
			}
			return appParams;
		}

		private static string DrawLine(IDWriteGdiInterop gdiInterop, IDWriteBitmapRenderTarget target,
			SampleLine line, IDWriteRenderingParams renderingParams, DWRITE_MEASURING_MODE measuringMode, uint textColor)
		{
			IDWriteFont dwFont = null;
			IDWriteFontFace fontFace = null;
			IntPtr fontFacePtr = IntPtr.Zero;
			try
			{
				NativeMethods.LOGFONTW logFont = GdiRenderer.BuildLogFont(line);
				int hr = gdiInterop.CreateFontFromLOGFONT(ref logFont, out dwFont);
				if (hr != 0 || dwFont == null)
					return "CreateFontFromLOGFONT failed for " + line.FaceName + " (0x" + hr.ToString("X8") + ")";

				hr = dwFont.CreateFontFace(out fontFace);
				if (hr != 0 || fontFace == null)
					return "CreateFontFace failed (0x" + hr.ToString("X8") + ")";

				DWRITE_FONT_METRICS fontMetrics;
				fontFace.GetMetrics(out fontMetrics);
				if (fontMetrics.designUnitsPerEm == 0)
					return "Font reported no design units per em.";

				float emPx = line.PixelSize;
				float designToPixel = emPx / fontMetrics.designUnitsPerEm;
				float baselineY = line.Top + (fontMetrics.ascent * designToPixel);

				// Plain left-to-right sample strings, so a direct codepoint-to-glyph mapping is
				// enough; no shaping, kerning or bidi reordering is applied.
				uint[] codePoints = new uint[line.Text.Length];
				for (int i = 0; i < line.Text.Length; i++)
					codePoints[i] = line.Text[i];

				ushort[] glyphIndices = new ushort[line.Text.Length];
				hr = fontFace.GetGlyphIndices(codePoints, (uint)codePoints.Length, glyphIndices);
				if (hr != 0)
					return "GetGlyphIndices failed (0x" + hr.ToString("X8") + ")";

				DWRITE_GLYPH_METRICS[] glyphMetrics = new DWRITE_GLYPH_METRICS[line.Text.Length];
				hr = fontFace.GetDesignGlyphMetrics(glyphIndices, (uint)glyphIndices.Length, glyphMetrics, 0);
				if (hr != 0)
					return "GetDesignGlyphMetrics failed (0x" + hr.ToString("X8") + ")";

				float[] advances = new float[line.Text.Length];
				for (int i = 0; i < line.Text.Length; i++)
					advances[i] = glyphMetrics[i].advanceWidth * designToPixel;

				fontFacePtr = Marshal.GetComInterfaceForObject(fontFace, typeof(IDWriteFontFace));

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
					// The measuring mode is fixed for a given base state, so glyph positions stay
					// identical across the registry sweep and any pixel difference between two runs
					// comes from the rendering parameters.
					hr = target.DrawGlyphRun(SampleText.LeftMargin, baselineY, measuringMode,
						ref run, renderingParams, textColor, IntPtr.Zero);
					if (hr != 0)
						return "DrawGlyphRun failed (0x" + hr.ToString("X8") + ")";
				}
				finally
				{
					indicesHandle.Free();
					advancesHandle.Free();
				}
				return null;
			}
			finally
			{
				if (fontFacePtr != IntPtr.Zero)
					Marshal.Release(fontFacePtr);
				ReleaseCom(fontFace);
				ReleaseCom(dwFont);
			}
		}

		private static DwRenderingParamValues ReadParams(IDWriteRenderingParams p)
		{
			DwRenderingParamValues values = new DwRenderingParamValues
			{
				Gamma = p.GetGamma(),
				EnhancedContrast = p.GetEnhancedContrast(),
				ClearTypeLevel = p.GetClearTypeLevel(),
				PixelGeometry = p.GetPixelGeometry(),
				RenderingMode = p.GetRenderingMode()
			};

			IDWriteRenderingParams1 p1 = p as IDWriteRenderingParams1;
			if (p1 != null)
			{
				try
				{
					values.GrayscaleEnhancedContrast = p1.GetGrayscaleEnhancedContrast();
				}
				catch (Exception)
				{
					// Left null: the interface is only available on Windows 8.1 and later.
				}
			}
			return values;
		}

		private static void ReleaseCom(object o)
		{
			if (o != null && Marshal.IsComObject(o))
				Marshal.ReleaseComObject(o);
		}
	}
}
