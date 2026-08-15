# Runtime Deployment and Diagnostics

Bridge 3 is an **ABI5-only, cross-platform x64 SDK**. The source contract supports `windows-x64` and `linux-x64`; platform-specific UI adapters remain separated by framework.

Native bridge libraries:

```text
Windows x64: OcctNative.dll
Linux x64:   libOcctNative.so
```

Default Windows developer OCCT layout:

```text
D:\tools\occt-vc144-64\inc
D:\tools\occt-vc144-64\win64\vc14\lib
D:\tools\occt-vc144-64\win64\vc14\bin
```

Default Linux OCCT layout uses `/usr/local/include/opencascade` and `/usr/local/lib` unless overridden by the build environment.

`OcctRuntime` resolves the configured Native bridge and OCCT runtime paths and reports diagnostics when required binaries cannot be loaded.

`OcctNet.dll` and the Native bridge must come from the **same Bridge build**. Runtime validation requires ABI 5 and an exact Bridge version match; newer or older Native bridge versions are not accepted as compatibility substitutes.

`dist/win-x64` and `dist/linux-x64` are Bridge Binary SDK payloads, not complete application runtime closures. Consumers remain responsible for deploying the required OCCT/third-party runtime according to their platform, distribution strategy, and licenses.

WinForms and WPF adapters are Windows-only. Core `OcctNet` and the Avalonia adapter are the cross-platform managed surfaces used by Linux consumers.
