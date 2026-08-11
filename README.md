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

The zip contains two builds of the same program.  Run whichever one suits your machine, and keep its `.config` file next to it:

| Executable | Requires | Notes |
| --- | --- | --- |
| `BetterClearTypeTuner.exe` | **.NET Framework 4.7.2** ([offline installer](https://dotnet.microsoft.com/download/dotnet-framework/net472)) | Works on Windows 7 SP1 and later.  Runs as x86/x64 (under emulation on ARM).  Use this one unless you have a reason not to. |
| `BetterClearTypeTuner-ARM64.exe` | **.NET Framework 4.8.1** ([offline installer](https://dotnet.microsoft.com/download/dotnet-framework/net481)) | Needs Windows 10 or later, but runs natively on Windows on ARM (24H2+) instead of under x64 emulation.  Despite the name it is an AnyCPU build and runs on x86/x64 too. |

## Caveats

As of Windows 10 1903, several pages of Windows' built-in ClearType tuner have little or no effect on **GDI** text rendering.  The settings that only DirectWrite reads — **DirectWrite contrast** (`GammaLevel`), **ClearType Level** and **Enhanced Contrast** — are therefore grouped separately, and the in-app GDI zoom preview will not change when you adjust them.  Use the DirectWrite preview beside it instead.  `TextContrastLevel` and `GrayscaleEnhancedContrastLevel` are not exposed; they are written at their documented defaults so that a value left behind by another tuner is put back.

**Override DirectWrite defaults** decides whether the `Avalon.Graphics` registry key exists at all.  A clean Windows installation does not have it, and that is not the same as having it with default-looking values in it — DirectWrite falls back to settings of its own, and at least some applications behave differently depending on whether the key is there.  Clearing the box removes the key from both hives and returns to that state; the three boxes then show what DirectWrite falls back to, read from DirectWrite itself rather than assumed.  It is deliberately all-or-nothing, because a partly written key produces rendering that matches neither the defaults nor the settings asked for.

Note that Microsoft documents the `GammaLevel` default as 1900, but DirectWrite does not use that number.  Asked what it resolves to with the key absent, it answers with a gamma of 1.8 — a `GammaLevel` of 1800.  Ticking the override box therefore starts from 1800 and leaves text looking unchanged.  You can see this for yourself with `BCT_Tests.exe --dwrite-defaults`.

The RGB/BGR buttons work either way.  With the key absent DirectWrite takes the subpixel order from the Windows font-smoothing setting; with it present, the `PixelStructure` value written here says the same thing — and, being the higher authority, would override the Windows setting if the two ever disagreed.

There appears to be some level of support for setting different ClearType settings on different monitors.  However, this appears to be entirely non-functional in modern Windows, so this program sets all monitors the same.

**Sources / further reading**

* This project's investigation notes: [ClearType Investigations (wiki)](https://github.com/bp2008/BetterClearTypeTuner/wiki/ClearType-Investigations) — and [discussion in #11](https://github.com/bp2008/BetterClearTypeTuner/issues/11).
* Microsoft documents *per-display* WPF/Avalon keys such as `PixelStructure` under `SOFTWARE\Microsoft\Avalon.Graphics\<display>` ([ClearType Registry Settings — WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/cleartype-registry-settings)), while GDI font-smoothing orientation is a single `SystemParametersInfo` value ([`SPI_*FONTSMOOTHINGORIENTATION`](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa)) with no monitor handle. DirectWrite can load per-`HMONITOR` defaults only when apps call [`CreateMonitorRenderingParams`](https://learn.microsoft.com/en-us/windows/win32/api/dwrite/nf-dwrite-idwritefactory-createmonitorrenderingparams) ([multi-monitor how-to](https://learn.microsoft.com/en-us/windows/win32/directwrite/how-to-add-support-for-multiple-monitors)).
* Independent mapping of what each `cttune.exe` step writes: [Reupen — What does each step in the ClearType Text Tuner do?](https://blog.yuo.be/2025/05/20/what-does-each-step-in-the-cleartype-tuner-do/) (2025).
* Longer discussion of ClearType (setting planes, tools, and mixed RGB/BGR desks as a corollary): [ClearType](https://docs.devcentr.org/general-knowledge/explanation/cleartype.html) · [HCI Nerdz topic](https://hci-nerdz.github.io/docs/hci-nerdz/cleartype.html) · [essay](https://hci-nerdz.github.io/blog/cleartype-tuner-in-windows/).

## Shout-out to MacType

For those who want more advanced text rendering tweaks, I strongly recommend [MacType](https://www.mactype.net/).  MacType enables much deeper customization of text rendering (although it doesn't work with everything) and is particularly useful on [displays that use subpixel layouts not natively supported in Windows](https://github.com/snowie2000/mactype/issues/720).
