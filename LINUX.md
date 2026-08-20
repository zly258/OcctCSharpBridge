# Linux Avalonia Demo

Linux x64 builds and publishes only `OcctDemo.Common` and `OcctDemo.Avalonia`. WinForms and WPF remain Windows-only.

## SDK synchronization

Development `demo` follows `main` by default:

```bash
./sync.sh
```

A valid local Binary + Portable SDK cache is reused when its `sourceCommit`, Bridge version and hashes match the selected source branch.

When regeneration is required:

```bash
./sync.sh --force-rebuild
```

The regeneration path runs only:

```text
Bridge build.sh dist Release
→ Bridge package-portable-sdk.sh
→ manifest/sourceCommit/hash validation
```

It does not run Bridge ManagedTests, Core Smoke or graphical Avalonia smoke. Those belong to Bridge release validation.

A formal Demo should consume formal `main` artifacts; while working on `demo`, this can be tested explicitly with:

```bash
./sync.sh --source main --force-rebuild
```

If matching prebuilt Bridge artifacts already exist, avoid recompiling Bridge:

```bash
./sync.sh \
  --sdk-root <binary-sdk> \
  --portable-root <portable-sdk>
```

## Build

```bash
./build.sh validate Release
./build.sh common Release
./build.sh avalonia Release
./build.sh all Release
```

`all` validates/builds the Demo Common layer and Avalonia host. The Demo does not contain or build Bridge implementation source directly.

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

The Demo publish step reuses the matching Bridge Portable SDK native closure and OCCT resources instead of maintaining a second `ldd`/OCCT dependency collector.

## Linux ABI compatibility

The package does not make native ABI requirements independent of the build distribution. OCCT and `libOcctNative.so` still depend on the glibc/libstdc++ ABI baseline used to compile them. A package built on a newer Linux may fail on an older Kylin/Debian/Ubuntu system with `GLIBC_*`, `GLIBCXX_*`, or `CXXABI_* not found` even when all files are present.

For broad distro support, rebuild OCCT and Bridge native code on the oldest supported Linux ABI baseline and validate the resulting package on the target matrix. AppImage packaging alone does not lower an already-linked glibc requirement.
