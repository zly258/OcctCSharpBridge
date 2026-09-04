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

## Shared Bridge SDK

The Demo consumes an installed Bridge Binary SDK directly.

Windows default:

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

Linux default (user-local, no root required):

```text
$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64
```

Install/update it from Bridge `main`:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

```bash
./publish.sh
```

Set `OCCTCSHARPBRIDGE_SDK` to override the SDK root. The Demo no longer clones Bridge or keeps a synchronized Binary SDK under `external/`. If the shared SDK is missing or incomplete, the build fails with the expected install path. Demo publication may still use `external/OcctCSharpBridge/portable/...` for the matching Portable SDK runtime closure.

## Build the Demo

Windows:

```powershell
.\build.ps1 all Release

.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

Linux:

```bash
./build.sh all Release
./run.sh Release
```

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

Linux publishes the Avalonia self-contained application from the installed Binary SDK and merges the matching Bridge Portable Runtime/resources:

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

The Demo is an SDK consumer example, not a third-party application framework. External projects should follow the Bridge SDK consumption guide for their own repository layout, runtime packaging, and version pinning.
