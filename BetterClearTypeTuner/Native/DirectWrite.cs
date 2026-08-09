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
		[PreserveSig] int CreateRenderingParams__();
		[PreserveSig] int CreateMonitorRenderingParams__();

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

	[ComImport, Guid("2f0da53a-2add-47cd-82ee-d9ec34688e75"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDWriteRenderingParams
	{
		[PreserveSig] float GetGamma();
		[PreserveSig] float GetEnhancedContrast();
		[PreserveSig] float GetClearTypeLevel();
		[PreserveSig] DWRITE_PIXEL_GEOMETRY GetPixelGeometry();
		[PreserveSig] DWRITE_RENDERING_MODE GetRenderingMode();
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
