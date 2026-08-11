# Better ClearType Tuner
A better way to configure ClearType font smoothing on Windows.

## Features

Quickly set font-smoothing settings in Windows 7, 10, or 11, and know what you are getting, unlike using the broken ClearType tuner that is built-in to the OS.  This program includes all font-smoothing settings that I have found to work in modern Windows, and does not expose settings that are non-functional.

* Enable or disable font antialiasing.
* Choose between Grayscale antialiasing or subpixel antialiasing using RGB or BGR subpixel layouts.  "ClearType" technology specifically refers to RGB and BGR subpixel antialiasing.
* Edit the text contrast and other text rendering settings.
* Preview the results at several font sizes and see a zoomed-in view to better-understand what is going on internally!
* Every setting is previewed live, through all three of the rendering paths Windows actually uses: GDI, DirectWrite ClearType, and DirectWrite grayscale.
* Compatibility varies between apps. Windows has multiple text rendering APIs and apps typically use one or the other and may not respect all of settings as configured in the operating system.  The three previews show you which is which.

<img width="1487" height="745" alt="image" src="https://github.com/user-attachments/assets/6acb253e-8f22-432b-8f30-fdc48701a76b" />

## Usage

Download from the [Releases Section](https://github.com/bp2008/BetterClearTypeTuner/releases), extract, and run.

The zip contains two builds of the same program.  Run whichever one suits your machine, and keep its `.exe.config` file next to it:

| Executable | Requires | Notes |
| --- | --- | --- |
| `BetterClearTypeTuner.exe` | **.NET Framework 4.7.2** ([offline installer](https://dotnet.microsoft.com/download/dotnet-framework/net472)) | Works on Windows 7 SP1 and later.  Runs as x86/x64 (under emulation on ARM).  Use this one unless you have a reason not to. |
| `BetterClearTypeTuner-ARM64.exe` | **.NET Framework 4.8.1** ([offline installer](https://dotnet.microsoft.com/download/dotnet-framework/net481)) | Needs Windows 10 or later, but runs natively on Windows on ARM (24H2+) instead of under x64 emulation.  Despite the name it is an AnyCPU build and runs on x86/x64 too. |

## Caveats

Windows has two low-level text rendering methods: **GDI** and **DirectWrite**.  Apps typically only use one or the other and some apps behave differently.  For example, Firefox is known to use DirectWrite, but in my tests Firefox ignores the **Enhanced Contrast** setting.

More importantly, **DirectWrite is not one thing**.  It rasterizes glyphs down one of two paths, and each application chooses for itself:

| | ClearType path | Grayscale path |
| --- | --- | --- |
| Coverage | computed per color subpixel | one value per whole pixel |
| Typical apps | Firefox, Edge, WPF | Settings, WinUI, Store apps |
| `GammaLevel` | ✔ | ✔ |
| `EnhancedContrastLevel` | ✔ | — |
| `GrayscaleEnhancedContrastLevel` | — | ✔ |
| `ClearTypeLevel`, `PixelStructure` | ✔ | — |

This is why some apps appear to ignore settings that demonstrably work elsewhere.  A WinUI app draws its text onto composition surfaces that may be transparent, transformed or animated, which cannot carry per-subpixel coverage, so it rasterizes grayscale no matter which antialiasing mode you select.

Better ClearType Tuner previews both paths side by side, live, so you can see which setting moves which.

The selected antialiasing mode controls GDI-based apps fairly reliably, but is only a hint to DirectWrite apps, so apps may end up using a different antialiasing mode than what you selected.

`TextContrastLevel` is the one `Avalon.Graphics` value still not exposed by Better ClearType Tuner.  It is a WPF 3.5-era setting with no reader I could find in modern Windows; the `TextContrastLevel` value is written at its documented default of `1` so the full set of keys exists in case *something I'm not aware of* cares to read it.

**Administrator permission is no longer needed to change text rendering settings.** Everything going into the Windows Registry is written under `HKEY_CURRENT_USER` now. Better ClearType Tuner versions up to 1.6.2 also wrote `GammaLevel` and `PixelStructure` under `HKEY_LOCAL_MACHINE`, and that required administrator permission. It had no benefit because the keys under `HKCU` override the keys under `HKLM` anyway.  A UAC prompt can still appear in one case: clearing **Override DirectWrite defaults** on a machine where an older version of this program (or another tuner) left a machine-wide key behind, since removing that still requires administrator rights.

A checkbox **Override DirectWrite defaults** decides whether the `Avalon.Graphics` registry key exists at all.  A clean Windows installation does not have it, and that is not always the same as having it with default-looking values in it. DirectWrite falls back to settings of its own, and at least some applications behave differently depending on whether the key is there or missing.  Clearing the checkbox removes the keys from the Windows Registry; the settings boxes then show what DirectWrite falls back to, read from DirectWrite itself rather than assumed.

Note that Microsoft documents the `Avalon.Graphics` `GammaLevel` default as 1900, but experiments show that DirectWrite does not use that number.  When the DirectWrite API is asked what it resolves to with the key absent, it answers with a gamma of 1.8 — a `GammaLevel` of 1800.

DirectWrite text rendering appears to have been designed with support for having different text rendering settings on different monitors.  However, this appears to be entirely non-functional in modern Windows, so this program sets `Avalon.Graphics` registry keys for all monitors the same.

**Sources / further reading**

* This project's investigation notes: [ClearType Investigations (wiki)](https://github.com/bp2008/BetterClearTypeTuner/wiki/ClearType-Investigations) — and [discussion in #11](https://github.com/bp2008/BetterClearTypeTuner/issues/11).
* Microsoft documents *per-display* WPF/Avalon keys such as `PixelStructure` under `SOFTWARE\Microsoft\Avalon.Graphics\<display>` ([ClearType Registry Settings — WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/cleartype-registry-settings)), while GDI font-smoothing orientation is a single `SystemParametersInfo` value ([`SPI_*FONTSMOOTHINGORIENTATION`](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa)) with no monitor handle. DirectWrite can load per-`HMONITOR` defaults only when apps call [`CreateMonitorRenderingParams`](https://learn.microsoft.com/en-us/windows/win32/api/dwrite/nf-dwrite-idwritefactory-createmonitorrenderingparams) ([multi-monitor how-to](https://learn.microsoft.com/en-us/windows/win32/directwrite/how-to-add-support-for-multiple-monitors)).
* Independent mapping of what each `cttune.exe` step writes: [Reupen — What does each step in the ClearType Text Tuner do?](https://blog.yuo.be/2025/05/20/what-does-each-step-in-the-cleartype-tuner-do/) (2025).
* Longer discussion of ClearType (setting planes, tools, and mixed RGB/BGR desks as a corollary): [ClearType](https://docs.devcentr.org/general-knowledge/explanation/cleartype.html) · [HCI Nerdz topic](https://hci-nerdz.github.io/docs/hci-nerdz/cleartype.html) · [essay](https://hci-nerdz.github.io/blog/cleartype-tuner-in-windows/).

## Shout-out to MacType

For those who want more advanced text rendering tweaks, I strongly recommend [MacType](https://www.mactype.net/).  MacType enables much deeper customization of text rendering (although it doesn't work with everything) and is particularly useful on [displays that use subpixel layouts not natively supported in Windows](https://github.com/snowie2000/mactype/issues/720).
