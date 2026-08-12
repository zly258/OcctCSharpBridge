# OcctCSharpBridge · Avalonia

[简体中文](README.zh-CN.md) · [Main branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [Windows demos](https://github.com/zly258/OcctCSharpBridge/tree/demo) · [Website](https://github.com/zly258/OcctCSharpBridge/tree/website)

The `avalonia` branch is the standalone cross-platform source edition of OcctCSharpBridge for **Windows x64 + Linux x64**. It contains only the reusable Core, Native bridge and Avalonia viewport host:

```text
OcctNet.Avalonia
       │
       ▼
    OcctNet
       │
       ▼
 stable C ABI
       │
       ▼
  OcctNative
   /      \
Windows   Linux
WNT_Window Xw_Window
```

There is no sync flow, tracked `dist`, branch-local binary publication, WinForms or WPF dependency.

## Source contract

| Item | Avalonia branch |
| --- | --- |
| Bridge | **2.7.0** |
| Native ABI | **4** |
| Native exports / P/Invoke | **350 / 350** |
| Public .NET types | **109** |
| Viewer / Modeling API | **216 / 134** |
| OCCT | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0`** |
| Avalonia | **12.1.0** |
| Platforms | **Windows x64 + Linux x64** |

`bridge-contract.json` is the machine-readable source of truth.

## Platform model

Applications use the same public control on both systems:

```csharp
var viewport = new OcctAvaloniaViewport();
```

Internally:

```text
Windows x64
Avalonia NativeControlHost → HWND → WNT_Window → OCCT Viewer

Linux x64
Avalonia NativeControlHost → XID → Xw_Window → OCCT Viewer
```

The current Linux Viewer backend supports X11/XWayland. Native Wayland hosting is not claimed yet.

## Windows

Default OCCT SDK:

```text
D:\tools\occt-vc144-64
```

Full non-GUI validation:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

This runs Native + Managed + ManagedTests + headless Smoke. The complete Avalonia Viewer host can be validated separately:

```powershell
.\build.ps1 avalonia-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Other useful targets are `validate`, `native`, `managed`, `test`, `smoke`, `docs` and `clean`.

## Linux

Default OCCT layout:

```text
/usr/local/include/opencascade
/usr/local/lib
```

Full non-GUI validation:

```bash
./build.sh all Release
```

The Linux Viewer smoke requires an X11/XWayland desktop session:

```bash
./build.sh avalonia-smoke Release
```

Other useful targets are `validate`, `native`, `managed`, `test`, `smoke`, `docs` and `clean`.

## Runtime

`OcctRuntime` resolves the native bridge by platform:

```text
Windows: OcctNative.dll
Linux:   libOcctNative.so
```

Non-default deployments can configure `OCCT_ROOT`, `OCCT_BRIDGE_NATIVE_DIR` and the platform dynamic-loader environment.

## Branch responsibilities

- `main` — Windows Bridge and Windows distribution work.
- `demo` — Windows demo applications.
- `avalonia` — source-only cross-platform `OcctNet + OcctNet.Avalonia` for Windows/Linux.
- `website` — public project website.

## License

OcctCSharpBridge is licensed under **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**. Open CASCADE Technology and other third-party components keep their own licenses. See [LICENSE](LICENSE), [OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt), [COMMERCIAL.md](COMMERCIAL.md), and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
