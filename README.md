# OcctCSharpBridge · Avalonia

The `avalonia` branch is the Windows x64 + Linux x64 Avalonia source edition of OcctCSharpBridge. It contains the reusable Core, Native bridge and Avalonia viewport host, plus an aligned CAD-Avalonia demo that shares `OcctDemo.Common` modeling scenarios with the WinForms/WPF demo baseline.

## Contract

- Bridge: 2.7.0
- Native ABI: 4
- Native exports / P/Invoke: 420 / 420
- Public .NET types: 135
- Viewer / Modeling API: 286 / 134
- OCCT: 7.9.0
- .NET SDK: 10.0.302
- Target Framework: `net10.0`
- Avalonia: 12.1.0
- Platforms: Windows x64 + Linux x64

`bridge-contract.json` is the machine-readable Bridge source of truth.

## Cross-platform demo

`src/OcctDemo.Avalonia` is a single `net10.0` desktop project for Windows and Linux. It does not use `System.Windows.Forms`, a Windows-only manifest, `user32.dll`, or `System.Media.SystemSounds`.

Desktop integration is Avalonia-native:

- file open/import/save/export: `Window.StorageProvider`
- messages and confirmations: modal Avalonia `Window.ShowDialog<T>` dialogs
- color selection: `Avalonia.Controls.ColorPicker`
- runtime bridge: `OcctNative.dll` on Windows, `libOcctNative.so` on Linux

The Linux viewer backend currently uses X11/XWayland through OCCT `Xw_Window`. Native Wayland hosting is not claimed.

## Windows

Default OCCT SDK: `D:\tools\occt-vc144-64`

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 avalonia Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 avalonia Release -OcctRoot "D:\tools\occt-vc144-64"
```

`demo` is an alias for the `avalonia` build target.

## Linux

Default OCCT layout uses `/usr/local/include/opencascade` and `/usr/local/lib`.

```bash
./build.sh all Release
./build.sh avalonia Release
./run.sh avalonia Release
```

`run.sh` requires an X11/XWayland desktop session with `DISPLAY` set. For a non-default OCCT installation, configure `OCCT_ROOT`, `OCCT_INCLUDE_DIR`, and/or `OCCT_LIB_DIR`.

## Runtime

The launch scripts configure the platform runtime environment:

- Windows: `OCCT_ROOT`, `CASROOT`, `OCCT_BRIDGE_NATIVE_DIR`, `PATH`
- Linux: `OCCT_ROOT`, `CASROOT`, `OCCT_BRIDGE_NATIVE_DIR`, `LD_LIBRARY_PATH`

## Branch responsibilities

- `main`: Windows Bridge and Windows distribution work.
- `demo`: WinForms/WPF Windows demos consuming the published Windows SDK.
- `avalonia`: cross-platform Core/Native/Avalonia source and the Windows/Linux CAD-Avalonia demo.

## License

OcctCSharpBridge uses GNU LGPL 2.1 with the OcctCSharpBridge Exception 1.0. Open CASCADE Technology and other third-party components retain their own licenses.
