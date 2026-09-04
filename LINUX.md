# Linux Avalonia Demo

Linux x64 builds and publishes only `OcctDemo.Common` and `OcctDemo.Avalonia`. WinForms and WPF remain Windows-only.

## Shared Bridge SDK

Linux consumes the installed Bridge Binary SDK directly. The default SDK root is user-local and does not require root privileges:

```text
$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64
```

Install/update it from Bridge `main` as the current user:

```bash
./publish.sh
```

Override the SDK root when needed:

```bash
OCCTCSHARPBRIDGE_SDK=/custom/path ./build.sh all Release
```

`build.sh`, `run.sh`, `publish.sh`, `sync.sh`, and direct MSBuild consumption all resolve the same default SDK root. The Demo does not silently rebuild Bridge when the installed Binary SDK is missing.

## Portable runtime synchronization

Demo publication additionally needs the matching Bridge Portable SDK payload under:

```text
external/OcctCSharpBridge/portable/linux-x64
```

`sync.sh` remains available for preparing that matching Portable SDK payload when publishing the Demo. It validates Bridge version, source commit and hashes so the portable runtime cannot be mixed with a different installed Binary SDK.

## Build

```bash
./build.sh common Release
./build.sh avalonia Release
./build.sh all Release
```

`all` builds the Demo Common layer and Avalonia host. The Demo does not contain or build Bridge implementation source directly.

## Run

```bash
./run.sh Release
```

The Avalonia viewer backend requires an X11/XWayland display.

## Publish

```bash
./publish.sh Release
```

The default output is:

```text
artifacts/publish/CAD-Avalonia-linux-x64/
artifacts/publish/CAD-Avalonia-linux-x64.tar.gz
```

The Demo publish step consumes the installed Binary SDK and reuses the matching Bridge Portable SDK native closure and OCCT resources instead of maintaining another runtime dependency collector.

## Linux ABI compatibility

The package does not make native ABI requirements independent of the build distribution. OCCT and `libOcctNative.so` still depend on the glibc/libstdc++ ABI baseline used to compile them. A package built on a newer Linux may fail on an older Kylin/Debian/Ubuntu system with `GLIBC_*`, `GLIBCXX_*`, or `CXXABI_* not found` even when all files are present.

For broad distro support, rebuild OCCT and Bridge native code on the oldest supported Linux ABI baseline and validate the resulting package on the target matrix. AppImage packaging alone does not lower an already-linked glibc requirement.
