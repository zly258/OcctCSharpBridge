# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge/tree/main)

`demo` is the single Binary SDK consumer branch. `main` is the sole Bridge SDK source.

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

Platform matrix:

| Platform | WinForms | WPF | Avalonia |
|---|---:|---:|---:|
| Windows x64 | yes | yes | yes |
| Linux x64 | no | no | yes |

The Demo is a strict Bridge 3 / ABI5 consumer. It does not track `OcctNative` or `OcctNet*` implementation sources and does not call the native `occt_*` ABI directly.

## Binary SDK workflow

`dist/` is local build state and is intentionally ignored by Git. Both synchronization scripts validate contract schema 3, manifest schema 2, ABI5-only metadata, .NET SDK 10.0.303, C# 14 and SDK file hashes. A matching `manifest.sourceCommit` is reused instead of rebuilding the SDK.

Windows:

```powershell
.\sync.ps1
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux:

```bash
./sync.sh
./build.sh all Release
./run.sh Release
./publish.sh Release
```

Linux builds only `OcctDemo.Common` and `OcctDemo.Avalonia`. WinForms and WPF are never part of the Linux build. The current Avalonia Viewer backend requires X11/XWayland for interactive running.

See [LINUX.md](LINUX.md) and [docs/platform-matrix.md](docs/platform-matrix.md) for platform-specific details.

## Demo previews

- WinForms / Windows: `assets/previews/winform-demo-en.png`
- WPF / Windows: `assets/previews/wpf-demo-en.png`
- Avalonia / Windows: `assets/previews/avalonia-win-demo-en.png`
- Avalonia / Linux: `assets/previews/avalonia-linux-demo-en.png`

## Branch responsibilities

- `main` / `main-dev`: Bridge SDK source and development.
- `demo` / `demo-dev`: unified Windows/Linux Demo consumer.
- `website`: bilingual project website.
- `backup/*`: retained historical backups; not modified by the migration.

Standalone `avalonia` and `avalonia-dev` branches are retired after their useful content has been absorbed into `demo`.

The project uses GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0; see the repository license files.
