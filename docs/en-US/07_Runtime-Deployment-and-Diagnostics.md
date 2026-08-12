# Runtime Deployment and Diagnostics

`main` is Windows x64 only.

Runtime bridge library:

```text
OcctNative.dll
```

Default developer OCCT layout:

```text
D:\tools\occt-vc144-64\inc
D:\tools\occt-vc144-64\win64\vc14\lib
D:\tools\occt-vc144-64\win64\vc14\bin
```

`OcctRuntime` resolves the configured Native bridge and OCCT runtime paths and reports diagnostics when required binaries cannot be loaded.

The tracked `dist/win-x64` is a Bridge Binary SDK, not a complete application runtime closure. Consumers remain responsible for deploying the required OCCT/third-party runtime according to their distribution strategy and licenses.

Linux runtime behavior belongs to the `avalonia` branch, where the Native bridge is `libOcctNative.so` and the default OCCT library path is `/usr/local/lib`.