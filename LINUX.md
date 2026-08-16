# Linux Avalonia Demo

Linux x64 builds and publishes only `OcctDemo.Common` and `OcctDemo.Avalonia`.
WinForms and WPF are Windows-only and are never built by the Linux scripts.

## Binary SDK

```bash
./sync.sh
```

`sync.sh` consumes `main` as the single SDK source. It reuses `dist/linux-x64` when the manifest `sourceCommit` matches `origin/main`; use `./sync.sh --force-rebuild` only when regeneration is required.

## Build

```bash
./build.sh validate Release
./build.sh common Release
./build.sh avalonia Release
./build.sh all Release
```

`all` is intentionally equivalent to building the shared Common layer plus the Avalonia host. The Demo branch never builds `OcctNative`, `OcctNet`, WinForms adapter, or WPF adapter sources.

## Run

```bash
./run.sh Release
```

The current Avalonia Viewer backend requires an X11/XWayland `DISPLAY`.

## Publish

```bash
./publish.sh Release
```

The default package is `artifacts/publish/CAD-Avalonia-linux-x64` plus a `.tar.gz` archive. The publish workflow uses the validated `dist/linux-x64` Binary SDK and bundles the OCCT/native shared-library closure required by the Bridge while leaving system graphics/X11/glibc libraries to the target OS.
