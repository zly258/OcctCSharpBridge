# OcctCSharpBridge · Avalonia

`avalonia` 分支是 OcctCSharpBridge 面向 Windows x64 + Linux x64 的 Avalonia 跨平台源码版本。该分支包含可复用的 Core、Native Bridge、Avalonia Viewport Host，以及与 WinForms/WPF Demo 共用 `OcctDemo.Common` 建模场景的 CAD-Avalonia Demo。

## 契约

- Bridge：2.7.0
- Native ABI：4
- Native exports / P/Invoke：420 / 420
- Public .NET types：135
- Viewer / Modeling API：286 / 134
- OCCT：7.9.0
- .NET SDK：10.0.302
- Target Framework：`net10.0`
- Avalonia：12.1.0
- 平台：Windows x64 + Linux x64

`bridge-contract.json` 是 Bridge 的机器可读事实源。

## 跨平台 Demo

`src/OcctDemo.Avalonia` 现在是同一个 `net10.0` 桌面项目，可在 Windows 和 Linux 构建。Demo 不再依赖 `System.Windows.Forms`、Windows-only manifest、`user32.dll` 或 `System.Media.SystemSounds`。

桌面交互全部使用 Avalonia 原生能力：

- 打开、导入、保存、导出：`Window.StorageProvider`
- 消息和确认：Avalonia `Window.ShowDialog<T>` 模态对话框
- 颜色选择：`Avalonia.Controls.ColorPicker`
- Native Bridge：Windows 使用 `OcctNative.dll`，Linux 使用 `libOcctNative.so`

Linux Viewer 当前通过 OCCT `Xw_Window` 支持 X11/XWayland；暂不宣称 Native Wayland Viewer 已完成。

## Windows

默认 OCCT SDK：`D:\tools\occt-vc144-64`

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 avalonia Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 avalonia Release -OcctRoot "D:\tools\occt-vc144-64"
```

`demo` 保留为 `avalonia` 构建目标的别名。

## Linux

默认 OCCT 路径为 `/usr/local/include/opencascade` 和 `/usr/local/lib`。

```bash
./build.sh all Release
./build.sh avalonia Release
./run.sh avalonia Release
```

`run.sh` 需要 X11/XWayland 桌面会话，并要求已设置 `DISPLAY`。非默认 OCCT 安装可配置 `OCCT_ROOT`、`OCCT_INCLUDE_DIR` 和/或 `OCCT_LIB_DIR`。

## Runtime

启动脚本按平台配置运行环境：

- Windows：`OCCT_ROOT`、`CASROOT`、`OCCT_BRIDGE_NATIVE_DIR`、`PATH`
- Linux：`OCCT_ROOT`、`CASROOT`、`OCCT_BRIDGE_NATIVE_DIR`、`LD_LIBRARY_PATH`

## 分支职责

- `main`：Windows Bridge 与 Windows 发布。
- `demo`：消费 Windows Binary SDK 的 WinForms/WPF Demo。
- `avalonia`：跨平台 Core/Native/Avalonia 源码，以及 Windows/Linux CAD-Avalonia Demo。

## 许可证

OcctCSharpBridge 使用 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0；Open CASCADE Technology 与其它第三方组件继续遵循各自许可证。
