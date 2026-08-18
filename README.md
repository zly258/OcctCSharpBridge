# OcctCSharpBridge Demo Development

[简体中文](README.zh-CN.md) · [Development SDK](https://github.com/zly258/OcctCSharpBridge/tree/main-dev)

`demo-dev` is the development Binary SDK consumer branch. Its default Bridge source is `main-dev`. Formal `demo` continues to consume formal `main`.

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

The Demo targets **.NET 10** to exercise the latest supported consumer runtime. Build tooling uses a stable .NET 10 SDK with a `10.0.100` baseline and `latestFeature` roll-forward. The Bridge Binary SDK targets the .NET 8 minimum baseline, while compatibility is determined from the contract's `supportedConsumerFrameworks` / `supportedDesktopConsumerFrameworks` lists.

## Current viewport contract

All three UI hosts consume the same Bridge viewport model:

- `OcctViewportInteractionFeatures` controls hover, point/rectangle selection, rotate, pan and zoom;
- `PreviewPointerInput / PointerInput` and `PreviewKeyInput / KeyInput` provide platform-neutral input;
- `HostState`, `EngineGeneration`, `EngineRecreated`, `EngineDisposing` and `Faulted` define native-host lifecycle;
- `InitialOptions`, `RenderReady` and `FirstFrameRendered` define first-frame readiness;
- `NativeHandleChanged` exposes HWND/XID changes for advanced hosting/diagnostics;
- `HoverHitChanged` reports owner/subshape identity changes without application-side detection polling;
- `BeginDisplayBatch()` is used for grouped scene/view configuration;
- the Samples menu includes a transient Viewer Projection Test using `ProjectPointToEdge` and `ProjectPointToFace`.

## SDK synchronization model

`dist/` is local generated state and is ignored by Git. `demo-dev` now synchronizes **two matching Bridge artifacts from the same `main-dev` source commit**:

```text
dist/win-x64/                  # strict minimal Binary SDK used for compilation
└─ 7-file ABI5 payload

dist/portable/win-x64/         # validated portable runtime used only for publishing
├─ runtime/                     # OcctNative + OCCT/third-party/VC runtime closure
├─ occt/resources/              # OCCT resources
├─ package-manifest.json
└─ Bridge notices/metadata
```

Linux uses the equivalent `dist/linux-x64` and `dist/portable/linux-x64` pair.

The minimal Windows Binary SDK remains strict and flat. It contains exactly:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

This contract is intentionally unchanged so Demo compilation and Consumer validation remain deterministic.

On Windows, `sync.ps1` keeps the reusable Bridge source clone under `.cache/main-sdk-source/`. For current `main-dev` it runs the full Bridge `build.ps1 sdk Release` gate, then invokes the Bridge-owned `tools/package-portable-sdk.ps1` against that exact Binary SDK. Both cached outputs must carry the same Bridge `sourceCommit`, and all declared hashes are revalidated after copying.

On Linux, `sync.sh` builds the matching linux-x64 SDK and invokes Bridge `tools/package-portable-sdk.sh`. The Bridge packager—not the Demo—owns `ldd`, OCCT dependency selection, `$ORIGIN` RPATH rewriting and OCCT resource collection.

Development synchronization:

```powershell
.\sync.ps1 -ForceRebuild
```

The default source is already `main-dev`; an explicit override is still available:

```powershell
.\sync.ps1 -SourceBranch main-dev -ForceRebuild
```

When supplying prebuilt artifacts, the Binary SDK and matching Portable SDK must both be supplied:

```powershell
.\sync.ps1 -SdkRoot <binary-sdk> -PortableRoot <portable-sdk>
```

Linux equivalent:

```bash
./sync.sh --force-rebuild
./sync.sh --sdk-root <binary-sdk> --portable-root <portable-sdk>
```

## Build and development run

Windows:

```powershell
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

`run.ps1` is a development runner and may still use a local OCCT installation. Portable OCCT deployment is a **publish-time** concern.

Linux:

```bash
./build.sh all Release
./run.sh Release
```

Linux builds only `OcctDemo.Common` and `OcctDemo.Avalonia`. Interactive Avalonia viewing requires X11/XWayland.

## Publishing

Demo publishing no longer contains its own OCCT dependency collector.

Previously:

```text
Demo publish
→ dumpbin / ldd
→ independently discover OCCT/TBB/etc.
→ independently copy OCCT resources
```

Current `demo-dev` flow:

```text
main-dev Bridge sync
→ validated minimal Binary SDK
→ Bridge-owned Portable SDK
→ Demo .NET publish
→ reuse matching Portable runtime/resources
→ Demo package
```

This removes duplicated runtime-closure logic between Bridge and Demo. `publish.ps1` / `publish.sh` validate that the cached Portable SDK has the same `bridgeSourceCommit` and `bridgeVersion` as the synchronized minimal SDK before packaging.

### Windows unified package

```powershell
.\publish.ps1 all Release -Zip
```

Output:

```text
artifacts/publish/CAD-Demo-win-x64/
├─ CAD-Winform.exe
├─ CAD-WPF.exe
├─ CAD-Avalonia.exe
├─ OcctNet*.dll
├─ runtime/
│  ├─ OcctNative.dll
│  ├─ TKernel.dll
│  ├─ TK*.dll
│  └─ required third-party / VC runtime DLLs
├─ occt/resources/...
├─ bridge-contract.json
├─ bridge-manifest.json
├─ bridge-portable-manifest.json
├─ run-winform.cmd
├─ run-wpf.cmd
├─ run-avalonia.cmd
└─ package-manifest.json
```

The root-level minimal `OcctNative.dll` emitted during project publishing is deliberately removed. The application uses the validated `runtime/OcctNative.dll` closure, and the run commands set `OCCT_BRIDGE_NATIVE_DIR`, `OCCT_ROOT`, `CASROOT`, `PATH` and OCCT resource variables accordingly.

Unified `all` remains framework-dependent because the three Windows UI applications cannot safely merge separate self-contained Desktop runtime closures. The target machine therefore needs the **.NET 10 Desktop Runtime x64**.

Single-target packages remain self-contained by default:

```powershell
.\publish.ps1 wpf Release -SelfContained -Zip
.\publish.ps1 avalonia Release -FrameworkDependent -Zip
```

The Demo publish command no longer accepts or needs `-OcctRoot`; OCCT runtime collection already happened in Bridge synchronization.

### Linux package

```bash
./publish.sh Release
```

Linux publish removes any root-level `libOcctNative.so`, reuses `dist/portable/linux-x64/runtime`, reuses the matching OCCT resources, and writes a Demo `package-manifest.json`. The Bridge Portable shared libraries already carry `$ORIGIN` RPATH from the Bridge packager.

## Branch responsibilities

- `main-dev`: development Bridge SDK and Portable Runtime source.
- `demo-dev`: development Demo consumer; default source is `main-dev`.
- `main`: formal Bridge SDK source.
- `demo`: formal Demo consumer; it should continue to default to `main` until the development changes are separately promoted.
- `website`: bilingual project website.

There are no standalone Avalonia source branches. Avalonia is part of the unified Bridge/Demo lines.

The project uses GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0; see the repository license files.
