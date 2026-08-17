# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge/tree/main)

`demo` is the single Binary SDK consumer branch. `main` is the sole Bridge SDK source.

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

| Platform | WinForms | WPF | Avalonia |
|---|---:|---:|---:|
| Windows x64 | yes | yes | yes |
| Linux x64 | no | no | yes |

The Demo is a strict Bridge 3 / ABI5 consumer. It does not track `OcctNative` or `OcctNet*` implementation sources and does not call the native `occt_*` ABI directly.

The Demo itself targets **.NET 10** to exercise the latest supported consumer runtime. Its build tooling uses a stable .NET 10 SDK with a `10.0.100` baseline and `latestFeature` roll-forward, so later stable 10.0.x SDKs are accepted. The consumed Bridge Binary SDK may target .NET 8, .NET 9 or .NET 10; the current development contract uses .NET 8 as the minimum Bridge runtime baseline so the same SDK can serve .NET 8-10 applications.

## Current viewport contract

All three UI hosts consume the same Bridge viewport model instead of framework-specific lifecycle logic:

- `OcctViewportInteractionFeatures` controls hover, point/rectangle selection, rotate, pan and zoom;
- `PreviewPointerInput / PointerInput` and `PreviewKeyInput / KeyInput` provide platform-neutral input;
- `HostState`, `EngineGeneration`, `EngineRecreated`, `EngineDisposing` and `Faulted` define native-host lifecycle;
- `InitialOptions`, `RenderReady` and `FirstFrameRendered` define first-frame readiness;
- `NativeHandleChanged` exposes HWND/XID changes only for advanced hosting/diagnostics;
- `HoverHitChanged` reports owner/subshape identity changes without requiring application-side `DetectAt` polling;
- `BeginDisplayBatch()` is used for grouped scene/view configuration;
- the Samples menu includes a transient **Viewer Projection Test** using `ProjectPointToEdge` and `ProjectPointToFace` with parameter round-trip validation.

The shared Demo shortcut mapper consumes `OcctKeyInputEventArgs`, so viewport-focused Ctrl+Z/Y/N/O/S, Delete, F, 0/1/2/3 and Escape no longer depend on WinForms/WPF/Avalonia key enums. Framework window shortcuts remain only as focus fallback.

## Binary SDK workflow

`dist/` is local build state and is intentionally ignored by Git. On Windows, `sync.ps1` keeps one reusable source clone at `.cache/main-sdk-source/`: the first sync clones once, and later syncs only fetch/checkout the requested `main` or `main-dev` commit while retaining ignored build caches. It no longer creates a new sibling `.OcctCSharpBridge-main-sdk-<guid>` worktree for every rebuild. The entire `.cache/` directory is ignored by Git.

Both platform synchronization flows validate contract schema 3, manifest schema 2, ABI5-only metadata, supported Bridge TFMs, C# 14 and SDK file hashes. They validate the Binary SDK's SDK baseline against its own contract rather than requiring it to equal the Demo machine's exact SDK version. A matching `manifest.sourceCommit` is reused instead of rebuilding the SDK.

Formal Windows consumption from `main`:

```powershell
.\sync.ps1
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1 all` now produces one `artifacts/publish/CAD-Demo-win-x64/` directory. The WinForms, WPF and Avalonia executables share one copy of the .NET runtime, Bridge, OCCT DLLs and OCCT resources instead of three complete directories with duplicate dependencies. Use `run-winform.cmd`, `run-wpf.cmd` or `run-avalonia.cmd` to launch each frontend. Publishing a single target (`winform`, `wpf` or `avalonia`) still produces a standalone deployable package.

When validating `demo-dev` against unreleased SDK work on `main-dev`, explicitly regenerate the local SDK from that source branch:

```powershell
.\sync.ps1 -SourceBranch main-dev -ForceRebuild
.\build.ps1 validate Release
.\build.ps1 all Release
```

Do not change the default `SourceBranch=main`; formal `demo` must consume formal `main`.

Linux:

```bash
./sync.sh
./build.sh all Release
./run.sh Release
./publish.sh Release
```

Linux builds only `OcctDemo.Common` and `OcctDemo.Avalonia`. WinForms and WPF are never part of the Linux build. The current Avalonia Viewer backend requires X11/XWayland for interactive running.

See [LINUX.md](LINUX.md) and [docs/platform-matrix.md](docs/platform-matrix.md) for platform-specific details.

## Demo previews

- WinForms / Windows: `assets/previews/winform-demo-en.png`
- WPF / Windows: `assets/previews/wpf-demo-en.png`
- Avalonia / Windows: `assets/previews/avalonia-win-demo-en.png`
- Avalonia / Linux: `assets/previews/avalonia-linux-demo-en.png`

## Branch responsibilities

- `main` / `main-dev`: Bridge SDK source and development.
- `demo` / `demo-dev`: unified Windows/Linux Demo consumer.
- `website`: bilingual project website.
- historical backup branches, when present, are not part of normal development and remain unchanged.

There are no standalone Avalonia source branches. Avalonia is part of `main` and the unified `demo` branch.

The project uses GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0; see the repository license files.
