# Runtime Deployment and Diagnostics

Bridge 3 is an **ABI5-only x64 SDK** for Windows and Linux. Runtime deployment has two distinct layers:

1. the consuming application's .NET deployment (`framework-dependent`, `self-contained`, or an application-specific private runtime);
2. the Bridge native deployment (`OcctNative` + OCCT/third-party closure + OCCT resources).

The Portable SDK addresses the second layer. It does not choose the application's .NET deployment mode.

## Native library names

```text
Windows x64: OcctNative.dll   / TKernel.dll
Linux x64:   libOcctNative.so / libTKernel.so
```

`OcctRuntime` resolves the current platform only. It probes the application directory, `runtime/`, `runtimes/<rid>/native`, explicit configuration, `OCCT_BRIDGE_NATIVE_DIR`, supported portable relative layouts, and repository output only when repository probing is enabled.

## Recommended Portable SDK application layout

```text
<app>/
  <application executable and managed files>
  OcctNet.dll
  OcctNet.WinForms.dll        # Windows only, if used
  OcctNet.Wpf.dll             # Windows only, if used
  OcctNet.Avalonia.dll        # if used
  bridge-contract.json
  bridge-manifest.json
  package-manifest.json       # optional to keep in the application package, recommended
  runtime/
    OcctNative.dll            # Windows
    libOcctNative.so          # Linux
    <OCCT and packaged third-party native closure>
  occt/
    resources/
      ...
```

Call runtime configuration before creating the first native-backed object:

```csharp
OcctRuntime.Configure();
```

This must happen before the first `OcctEngine` or `OcctModelingSession`. Explicit configuration is also available when an application deliberately uses another layout.

## Do not mix minimal and portable native payloads

The minimal `dist/<rid>` SDK exists primarily for compile-time references, sourceCommit/hash validation, and controlled consumer builds. Its flat `OcctNative` file does **not** carry the complete OCCT runtime closure.

When publishing an application with the Portable SDK layout:

- keep the managed `OcctNet*.dll` assemblies required by the project;
- use the native Bridge from `runtime/` together with that runtime directory;
- do not leave an unrelated or stale flat `OcctNative.dll` / `libOcctNative.so` beside the application executable;
- keep `runtime/`, `occt/`, the managed assemblies, and metadata from one coherent SDK build.

`OcctNet.dll` and the native Bridge must come from the same Bridge build. ABI 5 is mandatory, and Bridge version/source identity must not be reconstructed by manually editing manifests.

## Windows runtime closure

The Windows Portable SDK collects the Bridge native DLL, the OCCT DLLs it depends on, required third-party runtime files, and the relevant VC Runtime dependencies. The target application should copy the Portable SDK runtime directory as a unit rather than resolving OCCT from a developer installation.

## Linux runtime closure and ABI baseline

The Linux Portable SDK collects the Bridge native library, OCCT/TBB/FreeImage dependencies selected by the packager, and OCCT resources. Packaged ELF shared libraries use `$ORIGIN` so peer libraries in `runtime/` can resolve each other.

The Portable SDK intentionally does **not** bundle the Linux C runtime or the complete desktop stack. In particular, the target system still supplies system ABI/runtime components such as:

```text
glibc / libm
libstdc++ / libgcc_s
OpenGL / X11 / XWayland related system libraries
other platform desktop libraries
```

Therefore a Linux package is only as portable as the native ABI baseline used to build `libOcctNative.so` and OCCT. Building on a very new Linux distribution can produce requirements such as a newer `GLIBC_*`, `GLIBCXX_*`, or `CXXABI_*` than an older target distribution provides.

For broad Linux compatibility, build OCCT and `OcctNative` on the **oldest Linux/glibc baseline that the project intends to support**, then test the resulting Portable SDK on each supported distribution family. Wrapping a newer native build in an AppImage does not by itself lower its glibc/libstdc++ ABI requirements.

Useful target-machine diagnostics:

```bash
LD_LIBRARY_PATH="$PWD/runtime" ldd runtime/libOcctNative.so
readelf -d runtime/libOcctNative.so | grep -E 'RPATH|RUNPATH'
```

For every packaged shared library:

```bash
for f in runtime/*.so*; do
  result=$(LD_LIBRARY_PATH="$PWD/runtime" ldd "$f" 2>&1)
  if echo "$result" | grep -Eq 'not found|version .* not found'; then
    echo "===== $f ====="
    echo "$result" | grep -E 'not found|version .* not found'
  fi
done
```

`GLIBC_x.y not found` means the native build baseline is newer than the target libc. `GLIBCXX_*` / `CXXABI_* not found` indicates a C++ runtime ABI mismatch. These are native deployment issues, not .NET/Avalonia assembly-resolution problems.

## OCCT resources

The Portable SDK carries OCCT resources under `occt/resources`. `OcctRuntime.Configure()` configures available resource paths, including STEP/IGES defaults, message catalogs, shaders, textures, and unit resources. A package that contains the native libraries but omits required resources can load successfully and still fail later in import/export or presentation operations.

## Diagnostics

When native loading fails, inspect in this order:

1. process architecture is x64;
2. `runtime/OcctNative.dll` or `runtime/libOcctNative.so` exists;
3. the native dependency closure is complete;
4. Binary/Portable manifests point to the expected source commit and hashes;
5. OCCT resources exist under `occt/resources`;
6. Linux system ABI versions satisfy the packaged binaries;
7. the application called `OcctRuntime.Configure()` before the first native-backed object.

The diagnostic API reports configured native/root paths, candidate native Bridge locations, resource variables, and loaded runtime modules. Treat root-level “app-local” diagnostics separately from the portable `runtime/` candidate: the Portable SDK is expected to load from `runtime/`, not from a flat developer layout.

For a complete external application layout and MSBuild reference examples, see [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md).
