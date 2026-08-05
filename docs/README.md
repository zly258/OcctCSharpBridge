# Demo Documentation

[简体中文](README.zh-CN.md) · [Repository README](../README.md) · [Reusable SDK on `main`](https://github.com/zly258/OcctCSharpBridge/tree/main)

This directory documents the complete `demo` branch: reusable bridge APIs, shared WinForms/WPF behavior, executable API scenarios, development builds and portable package generation.

## Recommended reading order

1. [Getting started](GETTING_STARTED.md): environment, build, runtime configuration and first Viewer, Headless and OCAF programs.
2. [Viewer, selection and display](VIEWER_AND_DISPLAY.md): HWND lifecycle, camera preservation, explicit Fit/FitAll, display batches and rubber-band selection.
3. [Portable Demo publishing](PUBLISHING_DEMO.md): one-command WinForms/WPF self-contained package creation.
4. [Deployment and runtime layout](DEPLOYMENT.md): native dependency discovery, OCCT resources, clean-machine testing and redistribution.
5. [API coverage](API_COVERAGE.md): public capability matrix and intentional ABI boundaries.
6. [OCAF/XDE coverage](OCAF_COVERAGE.md): documents, labels, TNaming, XDE and persistence.
7. [Extended OCAF API](OCAF_EXTENDED_API.md): variables, expressions, relations and extended document operations.

## Demo-specific components

| Component | Purpose |
|---|---|
| `CadCommon` | Shared commands, session state, examples, localization, undo/redo and API scenarios |
| `CadWinForms` | WinForms host and desktop UI |
| `CadWpf` | WPF host reusing the shared OCCT viewport |
| API Center | Searchable reflection-based catalog of every public `OcctNet` member plus executable scenarios |
| `publish.ps1` | Self-contained Windows x64 package generation with native runtimes and resources |

## Core behavioral contracts

- OCCT version is exactly 7.9.0.
- Creating a Viewer Shape displays and redraws it without changing the current camera.
- `Fit`, `FitAll` and `WindowFit` are explicit view commands.
- Multi-object examples use `BeginDisplayBatch()` and perform at most one final redraw.
- WinForms and WPF share `OcctViewportControl`; selection and rubber-band fixes belong in `OcctNet`.
- Public API coverage is automatic through reflection; execution is separated from discoverability.
- Headless modeling does not require a window.
- OCAF mutations should be grouped in document commands and explicitly committed or aborted.
- Portable packages must be tested on a clean Windows x64 machine before distribution.

## Build, run and publish

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"

.\run.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"

.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

Development commands need the OCCT SDK. The generated self-contained package is designed for recipients who do not have a development environment or OCCT configuration.
