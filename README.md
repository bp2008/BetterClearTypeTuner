# Better ClearType Tuner
A better way to configure ClearType font smoothing on Windows.

## Features

Quickly set font-smoothing settings on modern Windows and know what you are getting, unlike using the broken ClearType tuner that is built-in to the OS.

* Enable or disable font antialiasing.
* Choose between Grayscale antialiasing or subpixel antialiasing using RGB or BGR subpixel layouts.
* Edit the contrast of font rendering (when using RGB or BGR subpixel antialiasing).
* Edit **ClearType Level** (0–100): DirectWrite/WPF “amount of ClearType” (`ClearTypeLevel`). Useful for apps that honor it (Firefox, WPF); many GDI apps and Chromium ignore it — the GDI zoom preview will not change.
* Preview results at several font sizes with a 400% zoomed GDI sample (so you see Windows font smoothing, not the UI toolkit’s renderer).
* Crisp UI on high-DPI / multi-monitor setups (Avalonia + PerMonitorV2 — fixes the blurry WinForms client area from [#14](https://github.com/bp2008/BetterClearTypeTuner/issues/14)).
* Runs on Windows on ARM via .NET 8 (native RID; no MacType-style hook driver to port).

## Usage

Download from the [Releases Section](https://github.com/bp2008/BetterClearTypeTuner/releases), extract, and run.

**Requirements:** [.NET 8 desktop runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows x64 / ARM64).

### Build from source

```powershell
dotnet build BetterClearTypeTuner.sln -c Release
dotnet run --project BetterClearTypeTuner -c Release
```

### Windows on ARM

Self-contained / framework-dependent `net8.0-windows` builds run natively on Windows on ARM. Unlike MacType, there is no architecture-specific hook driver to port.

## Caveats

As of Windows 10 1903, several pages of Windows' built-in ClearType tuner have little or no effect on **GDI** text rendering. **ClearType Level** is exposed because it still affects DirectWrite/WPF (and some browsers); other Avalon keys stay at sane defaults.

There appears to be some level of support for setting different ClearType settings on different monitors. However, this appears to be entirely non-functional in modern Windows, so this program sets all monitors the same.

The zoomed preview is drawn with **GDI+** using the system text-rendering hint so it tracks SPI ClearType changes. Avalonia draws the chrome (buttons, labels); that text may not match GDI exactly — by design.

**Sources / further reading**

* This project's investigation notes: [ClearType Investigations (wiki)](https://github.com/bp2008/BetterClearTypeTuner/wiki/ClearType-Investigations) — and [discussion in #11](https://github.com/bp2008/BetterClearTypeTuner/issues/11).
* Microsoft documents *per-display* WPF/Avalon keys such as `PixelStructure` under `SOFTWARE\Microsoft\Avalon.Graphics\<display>` ([ClearType Registry Settings — WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/cleartype-registry-settings)), while GDI font-smoothing orientation is a single `SystemParametersInfo` value ([`SPI_*FONTSMOOTHINGORIENTATION`](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa)) with no monitor handle. DirectWrite can load per-`HMONITOR` defaults only when apps call [`CreateMonitorRenderingParams`](https://learn.microsoft.com/en-us/windows/win32/api/dwrite/nf-dwrite-idwritefactory-createmonitorrenderingparams) ([multi-monitor how-to](https://learn.microsoft.com/en-us/windows/win32/directwrite/how-to-add-support-for-multiple-monitors)).
* Independent mapping of what each `cttune.exe` step writes: [Reupen — What does each step in the ClearType Text Tuner do?](https://blog.yuo.be/2025/05/20/what-does-each-step-in-the-cleartype-tuner-do/) (2025).
* Longer discussion of ClearType (setting planes, tools, and mixed RGB/BGR desks as a corollary): [ClearType](https://docs.devcentr.org/general-knowledge/explanation/cleartype.html) · [HCI Nerdz topic](https://hci-nerdz.github.io/docs/hci-nerdz/cleartype.html) · [essay](https://hci-nerdz.github.io/blog/cleartype-tuner-in-windows/).

## Shout-out to MacType

For those who want more advanced text rendering tweaks, [MacType](https://www.mactype.net/) enables much deeper customization (although it doesn't work with everything) and is particularly useful on [displays that use subpixel layouts not natively supported in Windows](https://github.com/snowie2000/mactype/issues/720).
