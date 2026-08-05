# Portable WinForms/WPF Demo Publishing

[简体中文](PUBLISHING_DEMO.zh-CN.md)

`publish.ps1` assembles a Windows x64 package that can be copied to another computer without installing the OCCT SDK, CMake, Visual Studio, or configuring OCCT environment variables. Self-contained .NET publishing is enabled by default.

## Prerequisites on the publishing computer

- Windows x64
- .NET 8 SDK
- Visual Studio 2022 C++ build tools
- CMake 3.21+
- the exact OCCT 7.9.0 SDK used by this repository
- permission to redistribute every copied runtime component

The target computer does not need these development tools.

## Basic commands

Publish WinForms and WPF and create a ZIP:

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -OutputDirectory ".\artifacts\publish" `
  -Zip
```

Publish only WinForms:

```powershell
.\publish.ps1 winform Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

Publish only WPF:

```powershell
.\publish.ps1 wpf Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

## Parameters

| Parameter | Values/default | Meaning |
|---|---|---|
| `Target` | `all`, `winform`, `wpf`; default `all` | Application set to publish |
| `Configuration` | `Debug`, `Release`, `RelWithDebInfo`; default `Release` | Native and managed build configuration |
| `-OcctRoot` | `OCCT_ROOT` or `D:\tools\occt-vc144-64` | OCCT 7.9.0 SDK root |
| `-OutputDirectory` | `artifacts\publish` | Parent output directory |
| `-FrameworkDependent` | off by default | Do not bundle the .NET runtime |
| `-Zip` | off by default | Create a ZIP after assembling the folder |
| `-KeepExisting` | off by default | Keep the existing package directory instead of recreating it |

For distribution to users who should install nothing, do not use `-FrameworkDependent`.

## What the script does

1. Validates `dotnet`, repository inputs and the OCCT root.
2. Builds `OcctNative.dll` through `build.ps1 native`.
3. Publishes selected desktop projects for `win-x64`.
4. Copies `OcctNative.dll` to the shared runtime directory.
5. Copies OCCT runtime DLLs from `win64\vc14\bin`.
6. Recursively detects third-party DLLs under `3rdparty-vc14-64`.
7. Copies available x64 Visual C++ runtime DLLs.
8. Copies available OCCT resource directories.
9. Copies project, OCCT and detected third-party license files.
10. Generates relative-path launchers.
11. Generates `runtime-manifest.txt` with size, file version and SHA-256.
12. Creates a ZIP when `-Zip` is supplied.

DLLs with the same file name are compared by SHA-256. Identical duplicates are ignored. Different files with the same name stop the package process instead of silently selecting an arbitrary binary.

## Generated directory

```text
artifacts\publish\OcctCSharpBridge-Demo-win-x64
├─ apps
│  ├─ winform
│  │  └─ CAD-Winform.exe
│  └─ wpf
│     └─ CAD-WPF.exe
├─ runtime
│  ├─ OcctNative.dll
│  ├─ TKernel.dll
│  ├─ TK*.dll
│  ├─ third-party DLLs
│  └─ Visual C++ runtime DLLs
├─ occt
│  └─ src
│     ├─ Shaders
│     ├─ StdResource
│     ├─ UnitsAPI
│     ├─ XSMessage
│     ├─ XSTEPResource
│     └─ ...
├─ licenses
├─ Start-WinForms.cmd
├─ Start-WPF.cmd
├─ runtime-manifest.txt
└─ README.txt
```

Only directories that exist in the selected OCCT installation are copied.

## How the launchers work

The generated `.cmd` files calculate all paths relative to the package root and set:

```text
PATH=<package>\runtime;%PATH%
OCCT_BRIDGE_NATIVE_DIR=<package>\runtime
OCCT_ROOT=<package>\occt
CASROOT=<package>\occt
```

They then start the selected application from its published directory. The package can therefore be extracted to any writable path without editing absolute paths.

## Distribution workflow

1. Build the package with `Release` and `-Zip`.
2. Extract the ZIP on a clean Windows x64 computer or VM.
3. Confirm no OCCT or repository development path exists in the target `PATH`.
4. Run `Start-WinForms.cmd` and `Start-WPF.cmd`.
5. Test Viewer startup, primitive creation, selection and rectangle selection.
6. Test STEP import/export and BinXCAF save/reopen.
7. Check `runtime-manifest.txt` against the delivered folder.
8. Review `licenses` and remove any component that cannot be redistributed.
9. Deliver the complete ZIP, not individual executable files.

## Updating a package

By default the existing package folder is removed before publishing. This prevents obsolete DLLs from remaining after dependency changes.

Use `-KeepExisting` only for diagnostics. It is not recommended for a final release package because stale files can hide missing dependency-copy logic.

## Framework-dependent mode

```powershell
.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -FrameworkDependent `
  -Zip
```

This reduces package size but requires the compatible .NET 8 Desktop Runtime on the target computer. OCCT and native dependencies are still packaged.

## Troubleshooting

### `OcctNative.dll` cannot be loaded

- Start through the generated `.cmd`, not directly from an unrelated working directory.
- Check that `runtime\OcctNative.dll` exists.
- Inspect dependent DLLs and compare with `runtime-manifest.txt`.
- Confirm the package is x64 and the operating system is x64.

### A `TK*.dll` or third-party DLL is missing

- Verify the publishing computer used the correct OCCT 7.9.0 root.
- Inspect `3rdparty-vc14-64` for a runtime folder not discovered by the standard recursive copy.
- Check for duplicate names with different hashes; the script reports these as conflicts.

### STEP or OCAF resource errors

- Run through the launcher so `CASROOT` and `OCCT_ROOT` are set.
- Confirm the corresponding directories exist under `occt\src`.
- Compare the package resources with the OCCT SDK used for the native build.

### Visual C++ runtime error

The script copies common x64 runtime DLLs that are available on the publishing computer. For formal redistribution, installing or bundling Microsoft's official Visual C++ Redistributable may be preferable. Follow Microsoft's current redistribution terms.

## Security and licensing

Do not treat “copy every DLL” as automatic permission to redistribute it. Review the license and vulnerability status of each third-party library. Preserve license notices and rebuild the package after updating OCCT or third-party dependencies.

## Native dependency closure

`publish.ps1` does not recursively copy every DLL under `3rdparty-vc14-64`. That directory may contain several compiler generations, static-library variants, debug binaries, SDK tools, and sample-only libraries such as GLFW. Copying all of them creates duplicate file names and can package an ABI-incompatible binary.

The script now locates the Visual C++ `dumpbin.exe`, starts from the built `OcctNative.dll`, reads each PE import table, and recursively copies only the OCCT and third-party DLLs that are actually required. Windows system DLLs are excluded, while the supported Visual C++ redistributable DLLs are copied separately. The selected dependency graph is written to `native-dependencies.txt` in the package root.

When multiple third-party files have the same name, candidates are ranked by runtime location, x64 architecture, and the VC 2022/vc14.4 toolset used by this project. Static and static-UCRT directories are never considered. An ambiguity is reported only when an actually imported DLL still has multiple equally ranked, different binaries.

