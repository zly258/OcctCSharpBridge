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

The Demo itself targets **.NET 10** to exercise the latest supported consumer runtime. Its build tooling uses a stable .NET 10 SDK with a `10.0.100` baseline and `latestFeature` roll-forward, so later stable 10.0.x SDKs are accepted. The Bridge Binary SDK currently targets the .NET 8 minimum baseline, but Demo compatibility is decided from the contract's `supportedConsumerFrameworks` / `supportedDesktopConsumerFrameworks` lists rather than by assuming a particular Bridge minimum TFM.

Demo runtime paths are also independent from the Bridge minimum TFM: `run.ps1` resolves each application's `TargetFramework` from its own `.csproj`, so the current WPF/WinForms output is `net10.0-windows` even though the consumed Bridge assemblies target `net8.0-windows`.

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

Windows source synchronization uses the Bridge **`build.ps1 sdk Release`** gate when the selected source revision provides it. For older source revisions that predate the `sdk` target, `sync.ps1` runs the equivalent validated sequence **`all Release` → `dist Release`** instead, so the default `SourceBranch=main` remains usable during the rollout without falling back to an unvalidated package. The `sdk` gate compiles the .NET 8/9/10 consumer matrix, runs ManagedTests, Core Native Smoke and all three Windows Viewport Host smokes, and only then produces `dist/win-x64` from the already validated Bridge outputs.

The Windows Binary SDK payload is strict and flat. Demo accepts exactly these seven files and rejects any extra file or directory, including when `-SdkRoot` is supplied:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

This prevents an old or unhashed DLL from being mixed into an otherwise valid manifest-controlled SDK. Contract schema 3, manifest schema 2, ABI5-only metadata, supported consumer TFMs, C# 14, `sourceCommit` and every declared SHA-256 are validated before the SDK is copied. A matching local `manifest.sourceCommit` is reused instead of rebuilding.

Formal Windows consumption from `main`:

```powershell
.\sync.ps1
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1 all` produces one shared **framework-dependent** directory:

```text
artifacts/publish/CAD-Demo-win-x64/
├─ CAD-Winform.exe
├─ CAD-WPF.exe
├─ CAD-Avalonia.exe
├─ run-winform.cmd
├─ run-wpf.cmd
├─ run-avalonia.cmd
├─ Bridge / OCCT / application dependencies (one shared copy)
├─ occt/resources/...
└─ package-manifest.json
```

The unified package does **not** bundle the .NET runtime. The target machine must provide the **.NET 10 Desktop Runtime x64**. The three applications share one copy of application dependencies, Bridge DLLs, OCCT DLLs and OCCT resources instead of three duplicate directories.

Before publishing, the script runs the Demo build gate once (`all` for the unified package, or the selected target for a standalone package); individual staging publishes no longer call `build.ps1` again. `package-manifest.json` records the Bridge source commit, package mode/runtime requirement and SHA-256/size for every packaged file.

Publishing a single target still produces a standalone package. Single-target publishing is self-contained by default and can be made framework-dependent explicitly:

```powershell
.\publish.ps1 wpf Release -SelfContained -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 avalonia Release -FrameworkDependent -OcctRoot "D:\tools\occt-vc144-64"
```

Unified `all` publishing cannot be self-contained because the WinForms/WPF/Avalonia Windows Desktop runtime closures contain conflicting same-name framework DLLs; the wrapper rejects that combination before publishing.

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
