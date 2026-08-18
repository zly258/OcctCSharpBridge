# Runtime Deployment and Diagnostics

Bridge 3 is an **ABI5-only, cross-platform x64 SDK**. The source contract supports `windows-x64` and `linux-x64`; platform-specific UI adapters remain separated by framework.

Native bridge and OCCT kernel names are platform-specific and are not compatibility aliases:

```text
Windows x64: OcctNative.dll     / TKernel.dll
Linux x64:   libOcctNative.so   / libTKernel.so
```

`OcctRuntime` resolves only the current platform layout. Native bridge candidates include the application directory, the application-local `runtime/` directory, `runtimes/<rid>/native`, the configured bridge directory, `OCCT_BRIDGE_NATIVE_DIR`, the legacy relative portable runtime directory, and—when repository probing is enabled—the current repository build output. The RID is `win-x64` on Windows and `linux-x64` on Linux; no old library-name fallback is used.

The Portable SDK is intended to be copied as one layout beside the application executable:

```text
<app>/
  OcctNet.dll
  OcctNet.WinForms.dll        # Windows only
  OcctNet.Wpf.dll             # Windows only
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
  package-manifest.json
  runtime/
    OcctNative.dll            # Windows
    libOcctNative.so          # Linux
    <OCCT/third-party native dependency closure>
  occt/
    resources/
      ...
```

With this layout, call the following before creating the first `OcctEngine` or `OcctModelingSession`:

```csharp
OcctRuntime.Configure();
```

`OcctRuntime` automatically resolves `<app>/runtime` and `<app>/occt`, so a target machine does not need the developer machine's `OCCT_ROOT` / `CASROOT`. The Windows Portable SDK also collects the VC Runtime DLLs actually imported by the native closure. The Linux Portable SDK collects OCCT/TBB/FreeImage and other non-system runtime libraries and rewrites the packaged ELF RPATH to `$ORIGIN`. Linux system libraries such as glibc, OpenGL and X11/Wayland components remain target-system dependencies rather than private SDK payload.

Default Windows developer OCCT layout:

```text
D:\tools\occt-vc144-64\inc
D:\tools\occt-vc144-64\win64\vc14\lib
D:\tools\occt-vc144-64\win64\vc14\bin
```

Default Linux OCCT layout uses `/usr/local/include/opencascade` and `/usr/local/lib` unless overridden by the build environment. Linux runtime probing uses the configured OCCT library directories rather than Windows `win64/vc14` paths.

`OcctRuntime` requires a supported 64-bit process and reports the effective platform, configured Native bridge/OCCT paths, candidate bridge paths and loaded runtime modules through its diagnostic APIs.

`OcctNet.dll` and the Native bridge must come from the **same Bridge build**. Runtime validation requires ABI 5 and an exact Bridge version match; newer or older Native bridge versions are not accepted as compatibility substitutes.

`dist/win-x64` and `dist/linux-x64` remain the **minimal Binary SDK** used by machine validation and Demo synchronization, and intentionally do not contain the OCCT runtime closure. Human distribution should use `publish.ps1` / `publish.sh`; after the complete Release gate they create `artifacts/publish/OcctCSharpBridge-<version>-<rid>-portable` with licenses/notices and a recursive SHA-256 `package-manifest.json`.

The Portable SDK does **not** bundle the .NET runtime. It makes the Bridge + OCCT native runtime portable; applications can independently choose framework-dependent or self-contained .NET publishing.

WinForms and WPF adapters are Windows-only. Core `OcctNet` and the Avalonia adapter are the cross-platform managed surfaces used by Linux consumers.