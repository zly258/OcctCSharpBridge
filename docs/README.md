# Unified Demo Branch Notes

The `demo` branch is the single Binary SDK consumer branch. It never contains Bridge implementation source.

## Projects and platforms

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → OcctNet.WinForms   (Windows x64)
├─ OcctDemo.Wpf       → OcctNet.Wpf        (Windows x64)
└─ OcctDemo.Avalonia  → OcctNet.Avalonia   (Windows x64 / Linux x64)
```

Windows builds three UI hosts. Linux builds Avalonia only.

## Bridge 3 / ABI5 boundary

- No `src/OcctNative` or `src/OcctNet*` implementation source is tracked.
- Demo C# code must not declare or call `occt_*` Native ABI entry points.
- Pre-ABI5 generic handles and compatibility metadata are forbidden.
- Retired object snapshot, appearance alias, Viewer BRep annotation and old Modeling-to-Viewer APIs are rejected by the consumer checks.
- Retired Viewport lifecycle flags/events (`EngineInitialized`, `EnableDefaultInteraction`, `EnableRectangleSelection`) are rejected as well.
- Current public APIs remain authoritative; compatibility guards must not invent replacement APIs.

## Unified Viewport usage

All three hosts use the same current Bridge contract:

- generation-aware `HostState`, `EngineRecreated`, `EngineDisposing`, `Faulted`;
- `InitialOptions`, `RenderReady`, `FirstFrameRendered` for first-frame readiness;
- platform-neutral Pointer/Key input and `OcctViewportInteractionFeatures`;
- `NativeHandleChanged` only for advanced HWND/XID hosting diagnostics;
- shared `OcctKeyInputEventArgs` shortcut mapping in `OcctDemo.Common`;
- `BeginDisplayBatch()` for first-frame/runtime grouped configuration;
- a shared Viewer Projection sample validating `ProjectPointToEdge` and `ProjectPointToFace`.

Host-specific key events remain only as window-focus fallback. New Viewport-focused command handling must use the Bridge key contract rather than framework key enums.

## Binary SDK workflow

`dist/` is ignored by Git and is not a second source of truth.

Formal Windows flow:

```powershell
.\sync.ps1
.\build.ps1 validate Release
.\build.ps1 all Release
```

Development validation of `demo-dev` against `main-dev`:

```powershell
.\sync.ps1 -SourceBranch main-dev -ForceRebuild
.\build.ps1 validate Release
.\build.ps1 all Release
```

The `sync.ps1` default stays `main`; only development validation explicitly selects `main-dev`.

Linux:

```bash
./sync.sh
./build.sh validate Release
./build.sh all Release
```

Both formal sync paths validate the Binary SDK and reuse it when `manifest.sourceCommit` matches the requested SDK source commit. Source worktrees used to regenerate the SDK are created beside the repository rather than under a system temporary directory.

## Publish

Windows produces three independent packages: WinForms, WPF and Avalonia.
Linux produces one package: `CAD-Avalonia-linux-x64`.

There are no standalone Avalonia branches. Historical backup branches, when present, are intentionally outside the normal development flow and remain unchanged.

No GitHub Actions or NuGet publication flow is used by this Demo branch.
