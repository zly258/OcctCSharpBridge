# OcctCSharpBridge · Avalonia

[English](README.md) · [中文文档](docs/zh-CN/README.md) · [English Docs](docs/en-US/README.md) · [main](https://github.com/zly258/OcctCSharpBridge/tree/main) · [website](https://github.com/zly258/OcctCSharpBridge/tree/website)

`avalonia` 分支是 OcctCSharpBridge 面向 Windows x64 + Linux x64 的跨平台版本，包含可复用的 Core/Native Bridge、`OcctNet.Avalonia`，以及 Windows/Linux 共用的 CAD-Avalonia Demo。

## 当前契约

- Bridge：**2.7.0**
- Native ABI：**4**
- Native exports / P/Invoke：**420 / 420**
- Public .NET types：**135**
- Viewer / Modeling API：**286 / 134**
- OCCT：**7.9.0**
- .NET：**10 / C# 14**
- Target Framework：**`net10.0`**
- Avalonia：**12.1.0**
- 平台：**Windows x64 + Linux x64**

`bridge-contract.json` 是机器可读事实源。

## 跨平台 Demo

`src/OcctDemo.Avalonia` 在 Windows 与 Linux 均保持 `net10.0`，平台差异封装在 `OcctNet.Avalonia` 内部：

- Windows：仅在 Windows 构建时条件式应用 Application Manifest，以满足 Avalonia `NativeControlHost`；Viewer 使用 HWND/WNT_Window。
- Linux：Viewer 使用 X11/XWayland XID/Xw_Window；暂不宣称 Native Wayland Viewer 已完成。
- 打开、导入、保存、导出：Avalonia `StorageProvider`。
- 消息、确认和颜色选择：Avalonia 原生对话框/控件。
- UI 字体：项目内置 Inter；OCCT 矢量文字/标注使用跨平台 `sans-serif` 字体别名。
- Native Bridge：Windows 使用 `OcctNative.dll`，Linux 使用 `libOcctNative.so`。

Linux 下鼠标事件由真实 X11 子窗口接收并转发到统一的选择、平移、旋转和缩放逻辑；连续 Motion 事件会先合并再进入 OCCT 交互更新，避免高频鼠标输入淹没 UI 线程。

## 构建与运行

### Windows

默认 OCCT SDK：`D:\tools\occt-vc144-64`。

```powershell
.\build.ps1
.\run.ps1

# 可选 Debug
.\build.ps1 Debug
.\run.ps1 Debug
```

### Linux

默认 OCCT 路径：`/usr/local/include/opencascade`、`/usr/local/lib`。

```bash
./build.sh
./run.sh

# 可选 Debug
./build.sh Debug
./run.sh Debug
```

Linux 当前需要 X11/XWayland 桌面会话，并设置 `DISPLAY`。非默认 OCCT 安装可配置 `OCCT_ROOT`、`OCCT_INCLUDE_DIR` 和/或 `OCCT_LIB_DIR`。

## 分支职责

- `main`：Windows Bridge 源码与 Windows Binary SDK 生产分支。
- `demo`：消费 Windows SDK 的 WinForms/WPF Demo。
- `avalonia`：跨平台 Core/Native/Avalonia 源码以及 Windows/Linux CAD-Avalonia Demo。
- `website`：中英文静态官网。

## 许可证

OcctCSharpBridge 使用 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0；Open CASCADE Technology 与其它第三方组件继续遵循各自许可证。
