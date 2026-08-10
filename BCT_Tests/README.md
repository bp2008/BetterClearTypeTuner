# BCT_Tests

A measurement harness for the question this repository keeps running into: **which text-rendering
setting actually affects which renderer?**

It renders the same sample text through **three renderers**, once for every combination of

* the four antialiasing modes the tuner offers — off, grayscale, ClearType RGB, ClearType BGR — and
* every `Software\Microsoft\Avalon.Graphics\<display>` value, in both `HKEY_CURRENT_USER` and
  `HKEY_LOCAL_MACHINE`, set one at a time to several values each,

then compares the resulting PNGs pixel for pixel and writes the compatibility matrix as
`Results\CompatibilityMatrix.md` (GitHub-flavored markdown) and `Results\CompatibilityMatrix.html`.

## The three renderers

**GDI** — `CreateFontIndirect` + `TextOut` on a 32bpp DIB with `DEFAULT_QUALITY`, so GDI consults the
system settings instead of being told what to do. No WinForms or GDI+ text layer in between.

The two DirectWrite renderers exist because of a split that is easy to measure wrong:

**DirectWrite (raw defaults)** hands `IDWriteFactory::CreateMonitorRenderingParams` straight to
`IDWriteBitmapRenderTarget::DrawGlyphRun` and changes nothing. Those parameters follow the
Avalon.Graphics registry values and `SPI_SETFONTSMOOTHINGORIENTATION`, but **not**
`SPI_SETFONTSMOOTHING` and **not** `SPI_SETFONTSMOOTHINGTYPE` — they come back reporting
`clearTypeLevel = 1` and `renderingMode = DEFAULT` even with antialiasing switched off system-wide.
So this renderer antialiases in every mode. That is a true fact about DirectWrite, and on its own it
is a misleading answer to "what does the user see".

**DirectWrite (as applications use it)** keeps DirectWrite's tuning parameters — gamma, enhanced
contrast, ClearType level, pixel geometry — but reads the system font-smoothing state itself and
picks the rendering mode from it, because DirectWrite will not: antialiasing off becomes
`DWRITE_RENDERING_MODE_ALIASED`, grayscale becomes antialiased with the ClearType level forced to 0.
That is the same policy `BetterClearTypeTuner\Native\DirectWriteSampleRenderer.cs` applies, and it is
the column that corresponds to what you see in Firefox, WPF and the tuner's own preview.

Reading them side by side is the point: a ✅ in the raw column means DirectWrite itself consumed the
value, while a ✅ only in the application column means the application had to act on it.

## Running it

```
BCT_Tests.exe --run
```

Run it **elevated**. Without administrator rights the harness cannot write `HKEY_LOCAL_MACHINE`,
which means it can neither test those values nor clear the ones already there — and a leftover
`HKLM\...\GammaLevel` silently skews every other row. It refuses to start unelevated unless you pass
`--allow-partial`, which leaves the HKLM rows marked "not tested".

| Arguments | What it does |
| --- | --- |
| *(none)* | Opens a small runner window with a log and a "Run sweep as administrator" button. |
| `--run [--out DIR] [--allow-partial] [--quick] [--pause]` | Runs the sweep and writes the reports. `--quick` is a short self-test of the harness, not a real matrix. |
| `--render --out DIR --name NAME` | Renders one configuration. The sweep uses this internally; there is no reason to call it by hand. |
| `--report-only [--out DIR]` | Rewrites both reports from an earlier run's files, without touching the machine's settings. Needs no elevation. |
| `--restore FILE` | Puts a `settings-backup.txt` back, for the case where a run was killed part way through. |

Results land in `BCT_Tests\Results\` next to the project: the two reports, the `images\` and `raw\`
folders they refer to, `run-log.txt`, `run-meta.txt`, and `settings-backup.txt`.

`--report-only` exists because changing how the report reads should not cost another sweep. It
recomputes every measurement from the PNGs and the `raw\*.txt` files a run left behind, using the
same `TestPlan` the run used, and takes only the things that cannot be recomputed — the timestamp,
the settings snapshot, and the restore outcome — from `run-meta.txt`.

## How the measurement works

* **One value at a time.** Before every render, *every* Avalon.Graphics value is deleted from both
  hives on every display, and then exactly one value is written. Each render is compared against the
  render taken with none of them present, which is the documented default state.
* **One process per configuration.** DirectWrite resolves its default rendering parameters when a
  factory is created, and real applications pick these settings up at startup, so each configuration
  is rendered by a freshly launched child process. That rules out measuring a stale cache and
  matches what an application actually sees.
* **Parameters are read back, not assumed.** Both DirectWrite renderers read the values out of
  `IDWriteRenderingParams` after each registry write, which is what lets the report name the
  DirectWrite parameter each registry value feeds rather than guessing from the picture.
* **A cross-check between the columns.** The registry sweep only ever compares renders taken *within*
  one antialiasing mode, so the report also compares the modes against each other. That is what shows
  the raw-defaults renderer producing identical output whether antialiasing is on or off.
* **A reproducibility check.** Each baseline is rendered twice and the two are compared. If they were
  not byte-identical, "the image changed" would mean nothing, so the report says so loudly.

## Putting the machine back

The harness snapshots the system font-smoothing settings and every Avalon.Graphics key it is about
to touch — including *whether the key or value existed at all* — writes that snapshot to
`Results\settings-backup.txt` before changing anything, restores it in a `finally` block, and then
re-reads everything to verify the machine really is back where it started. Values that appeared
during the run are deleted and keys that did not exist beforehand are removed entirely.

If a run is interrupted before it can restore, run `BCT_Tests.exe --restore Results\settings-backup.txt`.

Subkeys of `Avalon.Graphics` that are not display names — `IgnoreDwmFlushErrors`, for instance — are
never touched.
