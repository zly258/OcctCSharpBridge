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

## SDK synchronization

On Windows, `sync.ps1` is the source-driven Binary SDK entry point:

- clones `OcctCSharpBridge` when the source cache is missing;
- fetches `origin/main` by default, or the branch passed with `-BridgeBranch`;
- checks out the resolved commit in a clean detached source cache;
- runs `build.ps1 dist Release` (Native + Managed + Binary SDK packaging only);
- validates `bridge-contract.json`, `bridge-manifest.json`, hashes, and `sourceCommit`;
- installs the generated Binary SDK under `external/OcctCSharpBridge/win-x64`.

```powershell
.\sync.ps1
.\sync.ps1 -BridgeBranch main-dev
```

`sync.ps1` does not run the Bridge `sdk` / `all` QA gate. Demo build/run validation remains consumer-side.

On Linux, `./sync.sh` now has the same fresh-clone behavior: it follows `main` by default, keeps a clean Bridge source cache under `external/.cache`, runs only `./build.sh dist Release`, packages the matching Portable SDK with the Bridge-owned packager, validates both artifacts, and installs them under `external/OcctCSharpBridge`. It does not run Bridge tests or smoke tests. Prebuilt artifacts can still be supplied with `--sdk-root` and `--portable-root`.

```bash
./sync.sh
./sync.sh --source main-dev
./sync.sh --force-rebuild
```
## Build the Demo

On a fresh clone, `build.ps1` automatically runs the existing `sync.ps1` workflow when the Binary SDK cache is missing. If `external/OcctCSharpBridge/win-x64` is already complete, the Bridge is not synchronized or rebuilt again. Use `sync.ps1` explicitly when you want to refresh or change the Bridge branch.

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

The Validation menu keeps a small set of representative SDK checks. Geometry Inspection covers analytic/free-form reads and the bulk Bezier/B-Spline data path; Geometry Algorithms covers extrema and parameterized intersections. Existing B-Spline Surface and Mesh Generation checks remain the dedicated control-grid and mesh-buffer validations.

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

`tests/check-sdk-consumer.ps1` enforces the Windows clone/build/consume contract and unified `external/` layout; Linux checks retain their platform-specific synchronization contract.

The Demo is an SDK consumer example, not a third-party application framework. External projects should follow the Bridge SDK consumption guide for their own repository layout, runtime packaging, and version pinning.
