# What DirectWrite resolves to

Measured with `IDWriteFactory::CreateRenderingParams` on an isolated factory, one
registry state at a time.  These are the numbers DirectWrite hands to every
application that asks it for the default rendering parameters, so they are what
Firefox, Edge and WPF start from.

Generated 2026-08-11 07:44:43 on Microsoft Windows NT 6.2.9200.0.

| Registry state | DirectWrite resolves to |
| --- | --- |
| keys absent entirely | GammaLevel 1800, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| key present, display subkey present, no values | GammaLevel 1800, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| HKCU GammaLevel=1300 only | GammaLevel 1300, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| HKCU ClearTypeLevel=40 only | GammaLevel 1800, EnhancedContrastLevel 50, ClearTypeLevel 40, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| HKCU EnhancedContrastLevel=200 only | GammaLevel 1800, EnhancedContrastLevel 200, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| HKCU PixelStructure=2 only | GammaLevel 1800, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 2 (BGR), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| HKLM GammaLevel=1500 only (no HKCU key at all) | GammaLevel 1500, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| HKLM GammaLevel=1500 and HKCU GammaLevel=1300 | GammaLevel 1300, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| keys absent, SPI orientation BGR | GammaLevel 1800, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 2 (BGR), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| keys absent, SPI orientation RGB | GammaLevel 1800, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
| HKCU PixelStructure=1 (RGB), SPI orientation BGR | GammaLevel 1800, EnhancedContrastLevel 50, ClearTypeLevel 100, PixelStructure 1 (RGB), RenderingMode DEFAULT, GrayscaleEnhancedContrastLevel 100 |
