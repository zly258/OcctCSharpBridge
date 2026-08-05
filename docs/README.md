# Documentation

[简体中文](README.zh-CN.md)

This directory documents the reusable `main` branch. The WinForms/WPF applications and portable packaging workflow live on the `demo` branch.

## Recommended reading order

1. [Getting started](GETTING_STARTED.md): environment, build, runtime configuration, first Viewer, headless and OCAF programs.
2. [Viewer and display](VIEWER_AND_DISPLAY.md): HWND lifecycle, camera policy, explicit Fit/FitAll, display batches, selection and rubber-band behavior.
3. [Deployment](DEPLOYMENT.md): native DLL discovery, OCCT resources, runtime folder layout and redistribution checklist.
4. [API coverage](API_COVERAGE.md): public capability matrix and intentional boundaries.
5. [OCAF/XDE coverage](OCAF_COVERAGE.md): documents, labels, TNaming, XDE and persistence.
6. [Extended OCAF API](OCAF_EXTENDED_API.md): variables, expressions, relations and extended document operations.

## Branches

| Branch | Purpose |
|---|---|
| `main` | Reusable `OcctNative` and `OcctNet` SDK, tests and SDK documentation |
| `demo` | `main` plus `CadCommon`, WinForms, WPF, API Center, scenarios and `publish.ps1` |

## Core behavioral contracts

- OCCT version is exactly 7.9.0.
- All high-level objects own native lifetimes and implement `IDisposable` where required.
- Native pointers never become public managed API.
- Creating a Viewer shape displays and redraws it but does not change the camera.
- `Fit`, `FitAll`, and `WindowFit` are explicit view commands.
- `BeginDisplayBatch()` combines multiple display changes into one final redraw.
- Headless modeling never requires a window.
- OCAF mutations should be grouped in document commands and explicitly committed or aborted.
