# Better ClearType Tuner
A better way to configure ClearType font smoothing on Windows 10.

## Features

Quickly set font-smoothing settings in Windows 10 and know what you are getting, unlike using the broken ClearType tuner that is built-in to the OS.  This program includes all font-smoothing settings that I have found to work in modern Windows, and does not expose settings that are non-functional.

* Enable or disable font antialiasing.
* Choose between Grayscale antialiasing or subpixel antialiasing using RGB or BGR subpixel layouts.
* Edit the contrast of font rendering (when using RGB or BGR subpixel antialiasing).
* Preview the results at several font sizes and see a zoomed-in view to better-understand what is going on internally!

![Main Application Screenshot](https://i.imgur.com/1dMqenI.png)

## Install

### winget (once published)

```powershell
winget install bp2008.BetterClearTypeTuner
```

After the first package lands in the [Windows Package Manager community repository](https://github.com/microsoft/winget-pkgs), each new GitHub Release can open an update PR automatically via [`.github/workflows/winget.yml`](.github/workflows/winget.yml) (requires a `WINGET_TOKEN` repository secret). Setup guide: [Winget registration](https://github.com/dev-centr/general-knowledge/blob/main/docs/modules/ROOT/pages/how-to/winget-registration.adoc) (also published at [docs.devcentr.org](https://docs.devcentr.org/general-knowledge/how-to/winget-registration.html) when the docs site is current).

### Manual download

Download from the [Releases Section](https://github.com/bp2008/BetterClearTypeTuner/releases), extract, and run.

## Releasing (maintainers)

Builds and release zips are produced by GitHub Actions — you do **not** need to upload binaries by hand.

1. Merge your changes to `master`.
2. Create and push a **version tag** matching the release number (same style as existing tags: `1.4.0.2`, optional `v` prefix also works):

```powershell
git checkout master
git pull
git tag 1.4.0.3
git push origin 1.4.0.3
```

3. The [Release](.github/workflows/release.yml) workflow builds a Release binary, stamps `AssemblyVersion` / `AssemblyFileVersion` from the tag, zips `BetterClearTypeTuner.exe` as `BetterClearTypeTuner.<version>.zip`, and publishes a GitHub Release with that asset attached.
4. If `WINGET_TOKEN` is configured, [Publish to WinGet](.github/workflows/winget.yml) then opens an update PR to `winget-pkgs` for that release.

Continuous integration: every push/PR to `master` runs [Build](.github/workflows/build.yml) so the solution stays green without cutting a release.

## Caveats

As of Windows 10 1903, pages 3-5 of Windows' built-in ClearType tuner have no effect on text rendering.  Therefore, these settings were omitted from this program.  This program assigns sane default values to the affected registry keys so that if they begin working again in the future, ... they will at least have sane values.

There appears to be some level of support for setting different ClearType settings on different monitors.  However, this appears to be entirely non-functional in modern Windows, so this program sets all monitors the same.

## Shout-out to MacType

For those who want more advanced text rendering tweaks, I strongly recommend [MacType](https://www.mactype.net/).  MacType enables much deeper customization of text rendering (although it doesn't work with everything) and is particularly useful on [displays that use subpixel layouts not natively supported in Windows](https://github.com/snowie2000/mactype/issues/720).
