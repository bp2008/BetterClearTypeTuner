# Better ClearType Tuner
A better way to configure ClearType font smoothing on Windows 10.

## Features

Quickly set font-smoothing settings in Windows 10 and know what you are getting, unlike using the broken ClearType tuner that is built-in to the OS.  This program includes all font-smoothing settings that I have found to work in modern Windows, and does not expose settings that are non-functional.

* Enable or disable font antialiasing.
* Choose between Grayscale antialiasing or subpixel antialiasing using RGB or BGR subpixel layouts.
* Edit the contrast of font rendering (when using RGB or BGR subpixel antialiasing).
* Preview the results at several font sizes and see a zoomed-in view to better-understand what is going on internally!

![Main Application Screenshot](https://i.imgur.com/1dMqenI.png)

## Usage

Download from the [Releases Section](https://github.com/bp2008/BetterClearTypeTuner/releases), extract, and run.

## Caveats

As of Windows 10 1903, pages 3-5 of Windows' built-in ClearType tuner have no effect on text rendering.  Therefore, these settings were omitted from this program.  This program assigns sane default values to the affected registry keys so that if they begin working again in the future, ... they will at least have sane values.

There appears to be some level of support for setting different ClearType settings on different monitors.  However, this appears to be entirely non-functional in modern Windows, so this program sets all monitors the same.

## Shout-out to MacType

For those who want more advanced text rendering tweaks, I strongly recommend [MacType](https://www.mactype.net/).  MacType enables much deeper customization of text rendering (although it doesn't work with everything) and is particularly useful on [displays that use subpixel layouts not natively supported in Windows](https://github.com/snowie2000/mactype/issues/720).
