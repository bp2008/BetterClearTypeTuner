using System;
using System.Runtime.InteropServices;

namespace BetterClearTypeTuner.Native
{
	/// <summary>
	/// What DirectWrite resolves its rendering parameters to right now, asked of DirectWrite rather
	/// than assumed.
	///
	/// This matters because the Avalon.Graphics registry values are absent on a clean Windows
	/// installation, and in that state DirectWrite falls back to values of its own which are not the
	/// ones Microsoft documents for the registry.  Measured on Windows 11 26200 with the keys
	/// removed, the fallbacks are gamma 1.8, enhanced contrast 0.5, ClearType level 1.0 - so a
	/// window that displays the documented GammaLevel default of 1900 while nothing is set is
	/// showing a number that is not in effect.
	///
	/// Asking DirectWrite avoids hard-coding any of that, and it stays correct if a future Windows
	/// changes the fallbacks.
	/// </summary>
	public struct DirectWriteDefaults
	{
		/// <summary>DirectWrite contrast in the UI's 1000-2200 units (gamma * 1000).</summary>
		public int GammaLevel;
		/// <summary>Enhanced contrast in the UI's 0-400 units (enhanced contrast * 100).</summary>
		public int EnhancedContrastLevel;
		/// <summary>
		/// Grayscale enhanced contrast in the UI's 0-400 units.  Left at
		/// <see cref="DefaultGrayscaleEnhancedContrastLevel"/> when this DirectWrite is too old to
		/// have the setting; <see cref="GrayscaleMeasured"/> says which happened.
		/// </summary>
		public int GrayscaleEnhancedContrastLevel;
		/// <summary>True if <see cref="GrayscaleEnhancedContrastLevel"/> came from DirectWrite.</summary>
		public bool GrayscaleMeasured;
		/// <summary>ClearType level in the UI's 0-100 units (ClearType level * 100).</summary>
		public int ClearTypeLevel;
		/// <summary>Subpixel geometry, whose values match the PixelStructure registry value.</summary>
		public DWRITE_PIXEL_GEOMETRY PixelGeometry;

		/// <summary>
		/// What Microsoft documents for GrayscaleEnhancedContrastLevel, used only when DirectWrite
		/// cannot be asked.
		/// </summary>
		public const int DefaultGrayscaleEnhancedContrastLevel = 100;

		/// <summary>
		/// Asks DirectWrite for the parameters it would use for the primary monitor.  Returns false
		/// if DirectWrite could not be reached at all, in which case the caller should fall back to
		/// the documented defaults.
		/// </summary>
		public static bool TryMeasure(out DirectWriteDefaults defaults)
		{
			defaults = new DirectWriteDefaults();
			defaults.GrayscaleEnhancedContrastLevel = DefaultGrayscaleEnhancedContrastLevel;

			object factoryObj = null;
			IDWriteRenderingParams renderingParams = null;
			try
			{
				// ISOLATED rather than SHARED, and created afresh on every call: the shared factory
				// is process-wide and caches what it has already resolved, which would go stale the
				// moment this application writes or deletes the registry values.  An isolated
				// factory re-reads them, so a measurement taken immediately after a change reflects
				// the change.
				int hr = DWrite.DWriteCreateFactory(DWRITE_FACTORY_TYPE.ISOLATED, DWrite.IID_IDWriteFactory, out factoryObj);
				if (hr != 0 || factoryObj == null)
					return false;

				IDWriteFactory factory = (IDWriteFactory)factoryObj;
				hr = factory.CreateRenderingParams(out renderingParams);
				if (hr != 0 || renderingParams == null)
					return false;

				defaults.GammaLevel = Round(renderingParams.GetGamma(), 1000);
				defaults.EnhancedContrastLevel = Round(renderingParams.GetEnhancedContrast(), 100);
				defaults.ClearTypeLevel = Round(renderingParams.GetClearTypeLevel(), 100);
				defaults.PixelGeometry = renderingParams.GetPixelGeometry();

				// The grayscale contrast lives on the DirectWrite 1.1 interface only.  Its absence
				// is not a failed measurement, just an older Windows, so the rest of the answer
				// still stands.  The cast is a QueryInterface on the same object, which is why the
				// result is not released separately.
				try
				{
					IDWriteRenderingParams1 renderingParams1 = renderingParams as IDWriteRenderingParams1;
					if (renderingParams1 != null)
					{
						defaults.GrayscaleEnhancedContrastLevel = Round(renderingParams1.GetGrayscaleEnhancedContrast(), 100);
						defaults.GrayscaleMeasured = true;
					}
				}
				catch
				{
				}
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				Release(renderingParams);
				Release(factoryObj);
			}
		}

		/// <summary>
		/// Converts one of DirectWrite's floating point parameters into the integer units the
		/// registry and this application's inputs use.
		/// </summary>
		private static int Round(float value, int scale)
		{
			return (int)Math.Round(value * scale, MidpointRounding.AwayFromZero);
		}

		private static void Release(object o)
		{
			if (o != null && Marshal.IsComObject(o))
				Marshal.ReleaseComObject(o);
		}
	}
}
