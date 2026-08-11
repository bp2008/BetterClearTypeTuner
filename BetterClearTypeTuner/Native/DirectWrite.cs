using System;
using System.Runtime.InteropServices;

namespace BetterClearTypeTuner.Native
{
	#region Enums

	public enum DWRITE_FACTORY_TYPE
	{
		SHARED = 0,
		ISOLATED = 1
	}

	/// <summary>
	/// Subpixel geometry of the display. These values intentionally match the legacy
	/// "PixelStructure" registry value written to Software\Microsoft\Avalon.Graphics.
	/// </summary>
	public enum DWRITE_PIXEL_GEOMETRY
	{
		FLAT = 0,
		RGB = 1,
		BGR = 2
	}

	public enum DWRITE_RENDERING_MODE
	{
		DEFAULT = 0,
		ALIASED = 1,
		GDI_CLASSIC = 2,
		GDI_NATURAL = 3,
		NATURAL = 4,
		NATURAL_SYMMETRIC = 5,
		OUTLINE = 6
	}

	public enum DWRITE_MEASURING_MODE
	{
		NATURAL = 0,
		GDI_CLASSIC = 1,
		GDI_NATURAL = 2
	}

	/// <summary>
	/// How coverage is computed when glyphs are rasterized, and the setting which decides which of
	/// DirectWrite's two contrast controls is consulted: ClearType rasterization uses
	/// EnhancedContrast, grayscale rasterization uses GrayscaleEnhancedContrast, and neither one has
	/// any effect in the other mode.
	///
	/// A bitmap render target starts in CLEARTYPE, so a ClearType level of zero produces text that
	/// looks gray but is still rasterized down the ClearType path.  Only GRAYSCALE reaches the
	/// grayscale path that composited XAML applications - the Settings app, WinUI and Store apps -
	/// use for all of their text.
	/// </summary>
	public enum DWRITE_TEXT_ANTIALIAS_MODE
	{
		CLEARTYPE = 0,
		GRAYSCALE = 1
	}

	#endregion

	#region Structs

	[StructLayout(LayoutKind.Sequential)]
	public struct DWRITE_FONT_METRICS
	{
		public ushort designUnitsPerEm;
		public ushort ascent;
		public ushort descent;
		public short lineGap;
		public ushort capHeight;
		public ushort xHeight;
		public short underlinePosition;
		public ushort underlineThickness;
		public short strikethroughPosition;
		public ushort strikethroughThickness;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DWRITE_GLYPH_METRICS
	{
		public int leftSideBearing;
		public uint advanceWidth;
		public int rightSideBearing;
		public int topSideBearing;
		public uint advanceHeight;
		public int bottomSideBearing;
		public int verticalOriginY;
	}

	/// <summary>
	/// Pointer fields are raw IntPtr so that the caller controls pinning/lifetime explicitly.
	/// fontFace must be a raw IDWriteFontFace* obtained from Marshal.GetComInterfaceForObject.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct DWRITE_GLYPH_RUN
	{
		public IntPtr fontFace;
		public float fontEmSize;
		public uint glyphCount;
		public IntPtr glyphIndices;
		public IntPtr glyphAdvances;
		public IntPtr glyphOffsets;
		public int isSideways;
		public uint bidiLevel;
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

	#endregion

	#region Interfaces

	// Only the methods this application calls are declared with real signatures.
	// Every preceding method must still occupy its vtable slot, so unused slots are
	// declared as no-argument placeholders. They are never invoked.

	[ComImport, Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteFactory
	{
		[PreserveSig] int GetSystemFontCollection__();
		[PreserveSig] int CreateCustomFontCollection__();
		[PreserveSig] int RegisterFontCollectionLoader__();
		[PreserveSig] int UnregisterFontCollectionLoader__();
		[PreserveSig] int CreateFontFileReference__();
		[PreserveSig] int CreateCustomFontFileReference__();
		[PreserveSig] int CreateFontFace__();

		/// <summary>
		/// The rendering parameters DirectWrite itself resolves for the primary monitor, from the
		/// Avalon.Graphics registry values and the system font smoothing settings.  With those
		/// registry values absent this reports DirectWrite's own built-in fallbacks, which is the
		/// only way to find out what they are.
		/// </summary>
		[PreserveSig]
		int CreateRenderingParams(out IDWriteRenderingParams renderingParams);

		/// <summary>
		/// As CreateRenderingParams, but for one specific monitor, which is how an application
		/// picks up the per-display Avalon.Graphics values.
		/// </summary>
		[PreserveSig]
		int CreateMonitorRenderingParams(IntPtr monitor, out IDWriteRenderingParams renderingParams);

		[PreserveSig]
		int CreateCustomRenderingParams(
			float gamma,
			float enhancedContrast,
			float clearTypeLevel,
			DWRITE_PIXEL_GEOMETRY pixelGeometry,
			DWRITE_RENDERING_MODE renderingMode,
			out IDWriteRenderingParams renderingParams);

		[PreserveSig] int RegisterFontFileLoader__();
		[PreserveSig] int UnregisterFontFileLoader__();
		[PreserveSig] int CreateTextFormat__();
		[PreserveSig] int CreateTypography__();

		[PreserveSig]
		int GetGdiInterop(out IDWriteGdiInterop gdiInterop);

		// Remaining methods (CreateTextLayout and beyond) are unused and omitted.
	}

	/// <summary>
	/// DirectWrite 1.1, which shipped with Windows 8 and the Platform Update for Windows 7.  The
	/// only thing this application wants from it is the seven-argument
	/// CreateCustomRenderingParams, because the five-argument one on IDWriteFactory has no way to
	/// express a grayscale contrast at all.
	/// </summary>
	[ComImport, Guid("30572f99-dac6-41db-a16e-0486307e606a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteFactory1
	{
		// IDWriteFactory's own twenty-one methods come first.  Every one of them has to occupy its
		// vtable slot for the two that follow to be found, whether or not it is ever called.
		[PreserveSig] int GetSystemFontCollection__();
		[PreserveSig] int CreateCustomFontCollection__();
		[PreserveSig] int RegisterFontCollectionLoader__();
		[PreserveSig] int UnregisterFontCollectionLoader__();
		[PreserveSig] int CreateFontFileReference__();
		[PreserveSig] int CreateCustomFontFileReference__();
		[PreserveSig] int CreateFontFace__();

		[PreserveSig]
		int CreateRenderingParams(out IDWriteRenderingParams renderingParams);

		[PreserveSig]
		int CreateMonitorRenderingParams(IntPtr monitor, out IDWriteRenderingParams renderingParams);

		/// <summary>The five-argument overload, which is reached through IDWriteFactory instead.</summary>
		[PreserveSig] int CreateCustomRenderingParams0__();

		[PreserveSig] int RegisterFontFileLoader__();
		[PreserveSig] int UnregisterFontFileLoader__();
		[PreserveSig] int CreateTextFormat__();
		[PreserveSig] int CreateTypography__();

		[PreserveSig]
		int GetGdiInterop(out IDWriteGdiInterop gdiInterop);

		[PreserveSig] int CreateTextLayout__();
		[PreserveSig] int CreateGdiCompatibleTextLayout__();
		[PreserveSig] int CreateEllipsisTrimmingSign__();
		[PreserveSig] int CreateTextAnalyzer__();
		[PreserveSig] int CreateNumberSubstitution__();
		[PreserveSig] int CreateGlyphRunAnalysis__();

		// IDWriteFactory1 adds these two.
		[PreserveSig] int GetEudcFontCollection__();

		/// <summary>
		/// Note the argument order: the grayscale contrast comes before the ClearType level, not
		/// after it.
		/// </summary>
		[PreserveSig]
		int CreateCustomRenderingParams(
			float gamma,
			float enhancedContrast,
			float grayscaleEnhancedContrast,
			float clearTypeLevel,
			DWRITE_PIXEL_GEOMETRY pixelGeometry,
			DWRITE_RENDERING_MODE renderingMode,
			out IDWriteRenderingParams1 renderingParams);
	}

	[ComImport, Guid("2f0da53a-2add-47cd-82ee-d9ec34688e75"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteRenderingParams
	{
		[PreserveSig] float GetGamma();
		[PreserveSig] float GetEnhancedContrast();
		[PreserveSig] float GetClearTypeLevel();
		[PreserveSig] DWRITE_PIXEL_GEOMETRY GetPixelGeometry();
		[PreserveSig] DWRITE_RENDERING_MODE GetRenderingMode();
	}

	/// <summary>
	/// Adds the one parameter that only grayscale rasterization reads.  Available from DirectWrite
	/// 1.1; on anything older the QueryInterface for it fails and the grayscale preview is not
	/// offered.
	/// </summary>
	[ComImport, Guid("94413cf4-a6fc-4248-8b50-6674348fcad3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteRenderingParams1
	{
		[PreserveSig] float GetGamma();
		[PreserveSig] float GetEnhancedContrast();
		[PreserveSig] float GetClearTypeLevel();
		[PreserveSig] DWRITE_PIXEL_GEOMETRY GetPixelGeometry();
		[PreserveSig] DWRITE_RENDERING_MODE GetRenderingMode();

		[PreserveSig] float GetGrayscaleEnhancedContrast();
	}

	[ComImport, Guid("1edd9491-9853-4299-898f-6432983b6f3a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteGdiInterop
	{
		[PreserveSig]
		int CreateFontFromLOGFONT(ref LOGFONTW logFont, out IDWriteFont font);

		[PreserveSig] int ConvertFontToLOGFONT__();
		[PreserveSig] int ConvertFontFaceToLOGFONT__();
		[PreserveSig] int CreateFontFaceFromHdc__();

		[PreserveSig]
		int CreateBitmapRenderTarget(IntPtr hdc, uint width, uint height, out IDWriteBitmapRenderTarget renderTarget);
	}

	[ComImport, Guid("acd16696-8c14-4f5d-877e-fe3fc1d32738"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteFont
	{
		[PreserveSig] int GetFontFamily__();
		[PreserveSig] int GetWeight__();
		[PreserveSig] int GetStretch__();
		[PreserveSig] int GetStyle__();
		[PreserveSig] int IsSymbolFont__();
		[PreserveSig] int GetFaceNames__();
		[PreserveSig] int GetInformationalStrings__();
		[PreserveSig] int GetSimulations__();
		[PreserveSig] int GetMetrics__();
		[PreserveSig] int HasCharacter__();

		[PreserveSig]
		int CreateFontFace(out IDWriteFontFace fontFace);
	}

	[ComImport, Guid("5f49804d-7024-4d43-bfa9-d25984f53849"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteFontFace
	{
		[PreserveSig] int GetType__();
		[PreserveSig] int GetFiles__();
		[PreserveSig] int GetIndex__();
		[PreserveSig] int GetSimulations__();
		[PreserveSig] int IsSymbolFont__();

		[PreserveSig]
		void GetMetrics(out DWRITE_FONT_METRICS fontFaceMetrics);

		[PreserveSig] int GetGlyphCount__();

		[PreserveSig]
		int GetDesignGlyphMetrics(
			[In, MarshalAs(UnmanagedType.LPArray)] ushort[] glyphIndices,
			uint glyphCount,
			[Out, MarshalAs(UnmanagedType.LPArray)] DWRITE_GLYPH_METRICS[] glyphMetrics,
			int isSideways);

		[PreserveSig]
		int GetGlyphIndices(
			[In, MarshalAs(UnmanagedType.LPArray)] uint[] codePoints,
			uint codePointCount,
			[Out, MarshalAs(UnmanagedType.LPArray)] ushort[] glyphIndices);

		// Remaining methods (TryGetFontTable and beyond) are unused and omitted.
	}

	[ComImport, Guid("5e5a32a3-8dff-4773-9ff6-0696eab77267"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteBitmapRenderTarget
	{
		[PreserveSig]
		int DrawGlyphRun(
			float baselineOriginX,
			float baselineOriginY,
			DWRITE_MEASURING_MODE measuringMode,
			ref DWRITE_GLYPH_RUN glyphRun,
			IDWriteRenderingParams renderingParams,
			uint textColor,
			IntPtr blackBoxRect);

		[PreserveSig] IntPtr GetMemoryDC();
		[PreserveSig] float GetPixelsPerDip();

		[PreserveSig]
		int SetPixelsPerDip(float pixelsPerDip);

		// Remaining methods (GetCurrentTransform and beyond) are unused and omitted.
	}

	/// <summary>
	/// The same render target with the antialiasing mode exposed.  Without this the target stays in
	/// its default ClearType mode, and no amount of setting the ClearType level to zero moves it
	/// onto the grayscale path: it only collapses the ClearType output until it happens to look
	/// gray.
	/// </summary>
	[ComImport, Guid("791e8298-3ef3-4230-9880-c9bdecc42064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteBitmapRenderTarget1
	{
		[PreserveSig]
		int DrawGlyphRun(
			float baselineOriginX,
			float baselineOriginY,
			DWRITE_MEASURING_MODE measuringMode,
			ref DWRITE_GLYPH_RUN glyphRun,
			IDWriteRenderingParams renderingParams,
			uint textColor,
			IntPtr blackBoxRect);

		[PreserveSig] IntPtr GetMemoryDC();
		[PreserveSig] float GetPixelsPerDip();

		[PreserveSig]
		int SetPixelsPerDip(float pixelsPerDip);

		[PreserveSig] int GetCurrentTransform__();
		[PreserveSig] int SetCurrentTransform__();
		[PreserveSig] int GetSize__();
		[PreserveSig] int Resize__();

		[PreserveSig] DWRITE_TEXT_ANTIALIAS_MODE GetTextAntialiasMode();

		[PreserveSig]
		int SetTextAntialiasMode(DWRITE_TEXT_ANTIALIAS_MODE antialiasMode);
	}

	#endregion

	public static class DWrite
	{
		[DllImport("dwrite.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		public static extern int DWriteCreateFactory(
			DWRITE_FACTORY_TYPE factoryType,
			[MarshalAs(UnmanagedType.LPStruct)] Guid iid,
			[MarshalAs(UnmanagedType.IUnknown)] out object factory);

		public static readonly Guid IID_IDWriteFactory = new Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48");
	}
}
