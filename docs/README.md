# Demo Branch Maintenance

The `demo` branch is a pure application-layer consumer of the OcctCSharpBridge Binary SDK. Bridge Native/Managed source, ABI checks, managed regression tests, native smoke tests, bilingual SDK documentation, and Binary SDK production remain on `main`.

## Current contract

| Item | Current value |
| --- | --- |
| Author | **zly258** |
| Bridge version | **2.6.0** |
| Native ABI | **4** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0-windows`** |
| C# | **14.0** |
| Native Bridge | **C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |
| Public SDK surface | **344 Native / 344 P/Invoke / 105 public .NET types** |

The copied `dist/win-x64/bridge-contract.json` and `bridge-manifest.json` are authoritative for the exact SDK payload consumed by the branch.

## Branch responsibilities

```text
main
├─ src/OcctNative
├─ src/OcctNet*
├─ tests
├─ tools/OcctApiDocsGenerator
├─ docs/zh-CN
├─ docs/en-US
├─ build.ps1
├─ publish.ps1
└─ dist/win-x64        validated Binary SDK

demo
├─ dist/win-x64        Binary SDK published from main
├─ src/OcctDemo.Common
├─ src/OcctDemo.WinForms
├─ src/OcctDemo.Wpf
├─ src/OcctDemo.Avalonia
├─ build.ps1
├─ run.ps1
└─ publish.ps1         demo application packaging only
```

Demo does not contain Bridge producer source, Bridge tests, CMake validation, Native Smoke, or reverse SDK synchronization scripts.

## Binary SDK publication

Synchronization is one-way and starts from `main`:

```powershell
# main branch
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The main publish flow generates bilingual API documentation, runs the complete Release validation gate, creates `dist/win-x64`, validates Contract/Manifest/SHA-256, then updates the demo branch through a temporary detached worktree.

## Binary SDK contents

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

OCCT `TK*.dll` and third-party runtime DLLs are not stored in `dist`. Runtime resolution uses `OCCT_ROOT`, `CASROOT`, or explicit `OcctRuntime` configuration.

## Build

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release
```

Individual targets:

```powershell
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

`Directory.Build.targets` validates the Binary SDK before compilation and copies `OcctNative.dll` beside each executable. Managed `OcctNet*.dll` files are copied through ordinary private assembly references.

## Run and package

```powershell
.\run.ps1 wpf Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Demo `publish.ps1` packages applications only; it never publishes or rebuilds the Bridge SDK.

## Product metadata

`Directory.Build.props` defines the common demo assembly version, product and author. `OcctDemo.Common/DemoProductInfo.cs` is the single About-dialog metadata source for WinForms, WPF and Avalonia. The author is **zly258** in every language mode.

## Documentation rules

- Chinese Bridge documentation: `main/docs/zh-CN`.
- English Bridge documentation: `main/docs/en-US`.
- Complete Managed + Native API Reference: `api/` below both language roots.
- Demo maintains only application, UI, build, run, packaging and screenshot documentation.
- Do not restore deleted `dist.ps1`, `sync-dist.ps1`, Bridge source mirrors, or legacy compatibility wrappers.
- GitHub Actions are not used for build, validation, publication, or branch synchronization.
