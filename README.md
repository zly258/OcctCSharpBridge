# OcctCSharpBridge · Avalonia

[简体中文](README.zh-CN.md) · [English Docs](docs/en-US/README.md) · [中文文档](docs/zh-CN/README.md) · [main](https://github.com/zly258/OcctCSharpBridge/tree/main) · [website](https://github.com/zly258/OcctCSharpBridge/tree/website)

The `avalonia` branch is the Windows x64 + Linux x64 cross-platform edition of OcctCSharpBridge. It contains the reusable Core/Native bridge, `OcctNet.Avalonia`, and one CAD-Avalonia demo project shared by both operating systems.

## Contract

- Bridge: **2.7.0**
- Native ABI: **4**
- Native exports / P/Invoke: **420 / 420**
- Public .NET types: **135**
- Viewer / Modeling API: **286 / 134**
- OCCT: **7.9.0**
- .NET: **10 / C# 14**
- Target Framework: **`net10.0`**
- Avalonia: **12.1.0**
- Platforms: **Windows x64 + Linux x64**

`bridge-contract.json` is the machine-readable source of truth.

## Cross-platform demo

`src/OcctDemo.Avalonia` stays on `net10.0` for both platforms. Platform-specific native hosting is isolated inside `OcctNet.Avalonia`:

- Windows: Windows-only application manifest is applied conditionally at build time for Avalonia `NativeControlHost`; the viewer uses HWND/WNT_Window.
- Linux: the viewer uses X11/XWayland XID/Xw_Window; native Wayland hosting is not claimed.
- file open/import/save/export: Avalonia `StorageProvider`.
- messages/confirmations and color selection: Avalonia-native dialogs/controls.
- UI font: bundled Inter; OCCT vector text/dimensions use the cross-platform `sans-serif` alias.
- native bridge: `OcctNative.dll` on Windows, `libOcctNative.so` on Linux.

Linux pointer input is handled by the native X11 child window and forwarded to the same selection/pan/rotate/zoom behavior. Consecutive motion events are coalesced before OCCT interaction updates, preventing high-rate pointer input from flooding the UI thread.

## Build and run

### Windows

Default OCCT SDK: `D:\tools\occt-vc144-64`.

```powershell
.\build.ps1
.\run.ps1

# optional
.\build.ps1 Debug
.\run.ps1 Debug
```

### Linux

Default OCCT layout uses `/usr/local/include/opencascade` and `/usr/local/lib`.

```bash
./build.sh
./run.sh

# optional
./build.sh Debug
./run.sh Debug
```

Linux currently requires an X11/XWayland desktop session with `DISPLAY` set. For a non-default OCCT installation, configure `OCCT_ROOT`, `OCCT_INCLUDE_DIR`, and/or `OCCT_LIB_DIR`.

## Branch responsibilities

- `main`: Windows Bridge source and Windows Binary SDK producer.
- `demo`: WinForms/WPF Windows demos consuming the published Windows SDK.
- `avalonia`: cross-platform Core/Native/Avalonia source plus the Windows/Linux CAD-Avalonia demo.
- `website`: bilingual static project site.

## License

OcctCSharpBridge uses GNU LGPL 2.1 with the OcctCSharpBridge Exception 1.0. Open CASCADE Technology and other third-party components retain their own licenses.
