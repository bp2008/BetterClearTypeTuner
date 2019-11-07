# Better ClearType Tuner
A better way to configure ClearType font smoothing on Windows 10.

## Features

Quickly set font-smoothing settings in Windows 10 and know what you are getting, unlike using the broken ClearType tuner that is built-in to the OS.  This program includes all font-smoothing settings that I have found to work in modern Windows, and does not expose settings that are non-functional.

* Enable or disable font antialiasing.
* Choose between Grayscale antialiasing or subpixel antialiasing using RGB or BGR subpixel layouts.
* Edit the contrast of font rendering (when using RGB or BGR subpixel antialiasing).
* Preview the results at several font sizes and see a zoomed-in view to better-understand what is going on internally!

![Main Application Screenshot](https://i.imgur.com/R9qlQbv.png)

## Usage

Download from the [Releases Section](https://github.com/bp2008/BetterClearTypeTuner/releases), extract, and run.

## Caveats

As of Windows 10 1903, pages 3-5 of Windows' built-in ClearType tuner have no effect on text rendering.  Therefore, these settings were omitted from this program.  This program assigns sane default values to the affected registry keys so that if they begin working again in the future, ... they will at least have sane values.

There appears to be some level of support for setting different ClearType settings on different monitors.  However, this appears to be entirely non-functional in modern Windows, so this program sets all monitors the same.
