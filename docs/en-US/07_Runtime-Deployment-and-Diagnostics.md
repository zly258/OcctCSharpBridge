# Runtime Deployment and Diagnostics

Bridge 3 is an **ABI5-only, cross-platform x64 SDK**. The source contract supports `windows-x64` and `linux-x64`; platform-specific UI adapters remain separated by framework.

Native bridge and OCCT kernel names are platform-specific and are not compatibility aliases:

```text
Windows x64: OcctNative.dll     / TKernel.dll
Linux x64:   libOcctNative.so   / libTKernel.so
```

`OcctRuntime` resolves only the current platform layout. Native bridge candidates include the application directory, `runtimes/<rid>/native`, the configured bridge directory, `OCCT_BRIDGE_NATIVE_DIR`, the portable runtime directory, and—when repository probing is enabled—the current repository build output. The RID is `win-x64` on Windows and `linux-x64` on Linux; no old library-name fallback is used.

Default Windows developer OCCT layout:

```text
D:\tools\occt-vc144-64\inc
D:\tools\occt-vc144-64\win64\vc14\lib
D:\tools\occt-vc144-64\win64\vc14\bin
```

Default Linux OCCT layout uses `/usr/local/include/opencascade` and `/usr/local/lib` unless overridden by the build environment. Linux runtime probing uses the configured OCCT library directories rather than Windows `win64/vc14` paths.

`OcctRuntime` requires a supported 64-bit process and reports the effective platform, configured Native bridge/OCCT paths, candidate bridge paths and loaded runtime modules through its diagnostic APIs.

`OcctNet.dll` and the Native bridge must come from the **same Bridge build**. Runtime validation requires ABI 5 and an exact Bridge version match; newer or older Native bridge versions are not accepted as compatibility substitutes.

`dist/win-x64` and `dist/linux-x64` are Bridge Binary SDK payloads, not complete application runtime closures. Consumers remain responsible for deploying the required OCCT/third-party runtime according to their platform, distribution strategy, and licenses.

WinForms and WPF adapters are Windows-only. Core `OcctNet` and the Avalonia adapter are the cross-platform managed surfaces used by Linux consumers.
