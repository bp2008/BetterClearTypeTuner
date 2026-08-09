# Better ClearType Tuner
A better way to configure ClearType font smoothing on Windows 10.

## Features

Quickly set font-smoothing settings in Windows 10 and know what you are getting, unlike using the broken ClearType tuner that is built-in to the OS.  This program includes all font-smoothing settings that I have found to work in modern Windows, and does not expose settings that are non-functional.

* Enable or disable font antialiasing.
* Choose between Grayscale antialiasing or subpixel antialiasing using RGB or BGR subpixel layouts.
* Edit the contrast of font rendering (when using RGB or BGR subpixel antialiasing).
* Edit **ClearType Level** (0–100): the DirectWrite/WPF “amount of ClearType” registry setting (`ClearTypeLevel`). 0 is grayscale for those engines; 100 is full ClearType. Useful for apps that honor it (for example Firefox and WPF); many GDI apps and Chromium-based browsers ignore it.
* Preview the results at several font sizes and see a zoomed-in view to better-understand what is going on internally!

![Main Application Screenshot](https://i.imgur.com/1dMqenI.png)

## Usage

Download from the [Releases Section](https://github.com/bp2008/BetterClearTypeTuner/releases), extract, and run.

Requires **.NET Framework 4.8.1** (included with recent Windows 10/11; install the [4.8.1 offline installer](https://dotnet.microsoft.com/download/dotnet-framework/net481) if needed).

## Caveats

As of Windows 10 1903, several pages of Windows' built-in ClearType tuner have little or no effect on **GDI** text rendering.  This program originally omitted those Avalon.Graphics-only knobs and wrote sane defaults instead.  **ClearType Level** is now exposed because it still affects DirectWrite/WPF (and some browsers); the in-app GDI zoom preview will not change when you adjust it.  Other Avalon keys (`EnhancedContrastLevel`, `TextContrastLevel`, `GrayscaleEnhancedContrastLevel`) remain at sane defaults.

There appears to be some level of support for setting different ClearType settings on different monitors.  However, this appears to be entirely non-functional in modern Windows, so this program sets all monitors the same.

**Sources / further reading**

* This project's investigation notes: [ClearType Investigations (wiki)](https://github.com/bp2008/BetterClearTypeTuner/wiki/ClearType-Investigations) — and [discussion in #11](https://github.com/bp2008/BetterClearTypeTuner/issues/11).
* Microsoft documents *per-display* WPF/Avalon keys such as `PixelStructure` under `SOFTWARE\Microsoft\Avalon.Graphics\<display>` ([ClearType Registry Settings — WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/cleartype-registry-settings)), while GDI font-smoothing orientation is a single `SystemParametersInfo` value ([`SPI_*FONTSMOOTHINGORIENTATION`](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa)) with no monitor handle. DirectWrite can load per-`HMONITOR` defaults only when apps call [`CreateMonitorRenderingParams`](https://learn.microsoft.com/en-us/windows/win32/api/dwrite/nf-dwrite-idwritefactory-createmonitorrenderingparams) ([multi-monitor how-to](https://learn.microsoft.com/en-us/windows/win32/directwrite/how-to-add-support-for-multiple-monitors)).
* Independent mapping of what each `cttune.exe` step writes: [Reupen — What does each step in the ClearType Text Tuner do?](https://blog.yuo.be/2025/05/20/what-does-each-step-in-the-cleartype-tuner-do/) (2025).
* Longer discussion of ClearType (setting planes, tools, and mixed RGB/BGR desks as a corollary): [ClearType](https://docs.devcentr.org/general-knowledge/explanation/cleartype.html) · [HCI Nerdz topic](https://hci-nerdz.github.io/docs/hci-nerdz/cleartype.html) · [essay](https://hci-nerdz.github.io/blog/cleartype-tuner-in-windows/).

## Shout-out to MacType

For those who want more advanced text rendering tweaks, I strongly recommend [MacType](https://www.mactype.net/).  MacType enables much deeper customization of text rendering (although it doesn't work with everything) and is particularly useful on [displays that use subpixel layouts not natively supported in Windows](https://github.com/snowie2000/mactype/issues/720).
