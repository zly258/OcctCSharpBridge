# Linux Avalonia Demo

Linux x64 builds and publishes `OcctDemo.Common` and `OcctDemo.Avalonia`. WinForms and WPF remain Windows-only.

## Installed Bridge SDK

The Demo consumes the SDK installed by Bridge `main`; it does not clone, synchronize, or rebuild Bridge.

Default Linux SDK root:

```text
$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64
```

Install or update it from Bridge `main` as the current user:

```bash
./publish.sh
```

The installed SDK is complete:

```text
linux-x64/
├─ libOcctNative.so
├─ OcctNet.dll
├─ OcctNet.Avalonia.dll
├─ bridge-contract.json
├─ bridge-manifest.json
└─ portable/
   ├─ package-manifest.json
   ├─ runtime/
   └─ occt/resources/
```

Override the SDK root when needed:

```bash
OCCTCSHARPBRIDGE_SDK=/custom/path ./build.sh all Release
```

`build.sh`, `run.sh`, `publish.sh`, and direct MSBuild consumption resolve the same installed SDK root. There is no `sync.sh` workflow and no Demo-local Bridge SDK cache.

## Build

```bash
./build.sh common Release
./build.sh avalonia Release
./build.sh all Release
```

## Run

```bash
./run.sh Release
```

The Avalonia viewer backend requires an X11/XWayland display.

## Publish

```bash
./publish.sh Release
```

Demo publication reads both the Binary SDK and the matching runtime closure directly from the installed SDK. The default output is:

```text
artifacts/publish/CAD-Avalonia-linux-x64/
artifacts/publish/CAD-Avalonia-linux-x64.tar.gz
```

## Linux ABI compatibility

OCCT and `libOcctNative.so` remain constrained by the glibc/libstdc++ ABI baseline used to build them. Packaging does not make a newer native build compatible with older distributions. For broad distro support, build OCCT and Bridge on the oldest supported Linux ABI baseline and validate on the target matrix.
