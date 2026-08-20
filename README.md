# OcctCSharpBridge Demo Development

[简体中文](README.zh-CN.md) · [Bridge Development SDK](https://github.com/zly258/OcctCSharpBridge/tree/main) · [Third-party SDK Guide](https://github.com/zly258/OcctCSharpBridge/blob/main/docs/en-US/09_Third-Party-SDK-Consumption.md)

`demo` is a development consumer of the Bridge Binary/Portable SDK and follows `main` by default. It does not contain `OcctNative` / `OcctNet*` implementation source and does not call the native `occt_*` ABI directly.

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

The Demo targets .NET 10 to exercise the latest supported consumer runtime. The Bridge managed Binary SDK still targets the .NET 8 minimum baseline and supports .NET 8/9/10 consumers.

## SDK synchronization is now a consumer fast path

`dist/` is disposable local cache state and is ignored by Git. Synchronization maintains two artifacts from the **same Bridge sourceCommit**:

```text
dist/win-x64/                   # minimal Binary SDK for compilation
dist/portable/win-x64/          # Portable Runtime for publication

dist/linux-x64/
dist/portable/linux-x64/
```

### Cache hit

When the local Binary SDK `manifest.sourceCommit` matches the requested Bridge branch and all Binary/Portable hashes validate, synchronization returns immediately:

```text
0 Bridge builds
0 Bridge tests
0 viewport/window smokes
```

### Cache miss

Previously, Windows `sync.ps1` ran the complete Bridge `sdk` gate and Linux `sync.sh` ran `all -> dist`, which repeated Bridge ManagedTests/Core Smoke and, on Windows, WinForms/WPF/Avalonia viewport windows.

The synchronization flow is now:

```text
Bridge build dist Release
        ↓
Native + Managed + Binary SDK
        ↓
Bridge-owned Portable packager
        ↓
contract / sourceCommit / SHA-256 validation
        ↓
Demo local dist cache
```

Synchronization intentionally does **not** run:

- Bridge consumer matrix;
- Bridge ManagedTests;
- Bridge Core Native Smoke;
- WinForms/WPF/Avalonia viewport smoke;
- Linux graphical Avalonia smoke.

Full QA belongs to Bridge `main/main` `sdk` / `publish` workflows, not to downstream SDK refreshes.

Windows:

```powershell
.\sync.ps1
.\sync.ps1 -ForceRebuild
.\sync.ps1 -SourceBranch main -ForceRebuild
```

Linux:

```bash
./sync.sh
./sync.sh --force-rebuild
```

## Reuse prebuilt Bridge artifacts instead of recompiling

When matching Binary and Portable SDKs already exist, Demo can validate/copy them directly with zero Bridge compilation.

Windows:

```powershell
.\sync.ps1 `
  -SdkRoot <binary-sdk> `
  -PortableRoot <portable-sdk>
```

Linux:

```bash
./sync.sh \
  --sdk-root <binary-sdk> \
  --portable-root <portable-sdk>
```

Both artifacts must belong to the same Bridge build; sourceCommit, Bridge version, and package hashes must match.

## Build the Demo

Windows:

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release

.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

Linux:

```bash
./build.sh validate Release
./build.sh all Release
./run.sh Release
```

These commands validate the **Demo consumer itself** and do not rerun the Bridge full QA gate.

## Windows publication

Default unified package:

```powershell
.\publish.ps1 all Release -Zip
```

The default layout carries **one private .NET 10 Desktop Runtime shared by all three apps**, avoiding three duplicate runtime closures:

```text
artifacts/publish/CAD-Demo-win-x64/
├─ apps/
│  ├─ winform/CAD-Winform.exe
│  ├─ wpf/CAD-WPF.exe
│  └─ avalonia/CAD-Avalonia.exe
├─ dotnet/                      # one private .NET 10 Desktop Runtime
├─ runtime/                     # one Bridge + OCCT native closure
├─ occt/resources/
├─ bridge-contract.json
├─ bridge-manifest.json
├─ bridge-portable-manifest.json
├─ run-winform.cmd
├─ run-wpf.cmd
├─ run-avalonia.cmd
└─ package-manifest.json
```

Default `all` therefore does not require a system .NET 10 installation.

Explicit legacy-style per-app self-contained closures remain available:

```powershell
.\publish.ps1 all Release -SelfContained -Zip
```

This is larger because each application carries its own runtime closure.

Explicit framework-dependent publication is also available:

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip
```

That mode requires a compatible machine-installed .NET runtime.

Publication removes any flat `OcctNative.dll` copied from the minimal Binary SDK and uses the validated `runtime/OcctNative.dll` closure instead.

## Linux publication

```bash
./publish.sh Release
```

Linux currently publishes the Avalonia self-contained application and merges the matching Bridge Portable Runtime/resources:

```text
CAD-Avalonia-linux-x64/
├─ CAD-Avalonia
├─ managed/.NET publish files
├─ runtime/
│  ├─ libOcctNative.so
│  ├─ libTKernel.so*
│  └─ libTK*.so* / packaged dependencies
├─ occt/resources/
├─ bridge-portable-manifest.json
├─ package-manifest.json
└─ run.sh
```

Linux distribution compatibility is limited by the glibc/libstdc++ ABI baseline used to build OCCT and the native Bridge. Packaging newer native binaries in a Portable directory or AppImage does not automatically make them compatible with older Kylin/Debian/Ubuntu systems.

## Consumer boundary

The Demo may use Bridge only through its managed SDK:

- no tracked `src/OcctNative` or `src/OcctNet*` implementation source;
- no `LibraryImport/DllImport("OcctNative")` declarations;
- no pre-ABI5 handles/metadata;
- no duplicate OCCT runtime-closure collector;
- no full Bridge release gate inside SDK synchronization.

`tests/check-sdk-consumer.ps1` / `.sh` enforce these boundaries and verify that source synchronization uses the `dist` consumer fast path.

## Branch responsibilities

- `main` — Bridge development source and candidate SDK;
- `demo` — development consumer, defaults to `main`;
- `main` — formal Bridge SDK;
- `demo` — formal consumer and should consume formal `main` artifacts;
- `website` — bilingual website.

The Demo is an SDK consumer example, not a third-party application framework. External projects should follow the Bridge SDK consumption guide for their own repository layout, runtime packaging, and version pinning.
