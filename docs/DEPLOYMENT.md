# Deployment and Runtime Layout

## Runtime components

A Viewer or OCAF application needs more than the managed assemblies. A distributable Windows x64 package normally contains:

1. Published .NET application files.
2. `OcctNet.dll` and application assemblies.
3. `OcctNative.dll`.
4. OCCT toolkit DLLs linked by the native bridge.
5. Third-party DLLs required by OCCT, such as FreeType, TBB, FreeImage, or other components present in the selected OCCT build.
6. Microsoft Visual C++ redistributable runtime DLLs or an installed compatible redistributable.
7. OCCT resource directories used by exchange, persistence, units, shaders, messages, and textures.
8. Project, OCCT, Microsoft, and third-party license notices.

The exact dependency set is determined by the OCCT build used to compile `OcctNative.dll`. Do not mix DLLs from another OCCT version or compiler toolset.

## Native bridge discovery

`OcctNet` searches for `OcctNative.dll` through:

- the application base directory;
- the directory configured by `OCCT_BRIDGE_NATIVE_DIR`;
- candidates configured through `OcctRuntime.Configure(...)`.

After `OcctNative.dll` is found, Windows must resolve its dependent DLLs. Put them beside the bridge or add their runtime directory to `PATH` before the first P/Invoke call.

## Resource discovery

OCCT data exchange and persistence use resource files in addition to DLLs. A portable package should set:

```cmd
set "OCCT_ROOT=%~dp0occt"
set "CASROOT=%~dp0occt"
```

and preserve required resource directories under:

```text
occt\src\StdResource
occt\src\UnitsAPI
occt\src\SHMessage
occt\src\XSMessage
occt\src\XSTEPResource
occt\src\Shaders
occt\src\Textures
occt\src\XmlOcafResource
```

The packaging script should copy only directories that exist in the selected OCCT installation.

## Recommended portable layout

```text
OcctCSharpBridge-Demo-win-x64
├─ apps
│  ├─ winform
│  └─ wpf
├─ runtime
│  ├─ OcctNative.dll
│  ├─ TK*.dll
│  ├─ TKernel.dll
│  ├─ third-party DLLs
│  └─ VC++ runtime DLLs
├─ occt
│  └─ src\...
├─ licenses
├─ Start-WinForms.cmd
├─ Start-WPF.cmd
├─ runtime-manifest.txt
└─ README.txt
```

Launchers should set `PATH`, `OCCT_BRIDGE_NATIVE_DIR`, `OCCT_ROOT`, and `CASROOT` relative to their own location before starting the application.

## Framework-dependent versus self-contained

A framework-dependent publish is smaller but requires a compatible .NET Desktop Runtime on the target machine.

A self-contained publish includes the .NET runtime and is preferred for a package intended for users who should not configure an environment. It does not automatically include OCCT, third-party native libraries, OCCT resources, or VC++ runtime components; those still need to be copied.

## `demo` branch publishing

The `demo` branch includes `publish.ps1`.

```powershell
.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -OutputDirectory ".\artifacts\publish" `
  -Zip
```

The script:

- builds `OcctNative.dll`;
- publishes WinForms and/or WPF for `win-x64`;
- uses self-contained .NET deployment by default;
- copies OCCT runtime DLLs and detected third-party DLLs;
- copies available OCCT resource directories;
- copies redistributable VC++ runtime files when available;
- creates relative-path launchers;
- writes a manifest with file size, version, and SHA-256;
- creates a ZIP when requested.

## Validation before distribution

Test the package on a clean Windows x64 computer or virtual machine that does not have the development OCCT tree in `PATH`.

Minimum checks:

- WinForms and WPF start from the generated launchers.
- Viewer initialization succeeds.
- Primitive creation and selection work.
- Rectangle selection overlay is stable.
- STEP import/export works.
- BinXCAF save/reopen works.
- Fonts and text rendering work.
- The package does not accidentally depend on the developer's absolute OCCT path.

Use Process Monitor or a dependency inspection tool if the target machine reports a missing DLL.

## Redistribution and licensing

The script can assemble files but cannot decide whether a particular third-party binary may be redistributed in a specific product. Review:

- this repository's PolyForm Noncommercial License;
- OCCT LGPL 2.1 with the OCCT exception;
- Microsoft Visual C++ redistributable terms;
- licenses for every third-party component copied from the OCCT installation.

Keep license texts in the distributed package and remove components that are not actually permitted or required.
