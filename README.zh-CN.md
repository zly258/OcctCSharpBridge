# OcctCSharpBridge · Avalonia

[English](README.md) · [main 分支](https://github.com/zly258/OcctCSharpBridge/tree/main) · [Windows Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo) · [Website](https://github.com/zly258/OcctCSharpBridge/tree/website)

`avalonia` 是 OcctCSharpBridge 的**独立跨平台源码分支**，同时面向 **Windows x64 + Linux x64**。该分支只保留可复用 Core、Native Bridge 与 Avalonia Viewport Host：

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

该分支没有 sync、没有跟踪 `dist`、没有分支内 Binary SDK 发布流程，也不依赖 WinForms/WPF。

## 源码契约

| 项目 | avalonia 分支 |
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
| 平台 | **Windows x64 + Linux x64** |

`bridge-contract.json` 是机器可读的源码事实源。

## 平台模型

Windows 和 Linux 都使用同一个公开控件：

```csharp
var viewport = new OcctAvaloniaViewport();
```

底层内部实现：

```text
Windows x64
Avalonia NativeControlHost → HWND → WNT_Window → OCCT Viewer

Linux x64
Avalonia NativeControlHost → XID → Xw_Window → OCCT Viewer
```

Linux 当前 Viewer Backend 支持 X11/XWayland；暂不宣称 Native Wayland Viewer 已完成。

## Windows

默认 OCCT SDK：

```text
D:\tools\occt-vc144-64
```

完整非 GUI 验证：

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

`all` 统一执行 Native + Managed + ManagedTests + Headless Smoke。完整 Avalonia Viewer Host 单独验证：

```powershell
.\build.ps1 avalonia-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

其它保留 Target：`validate`、`native`、`managed`、`test`、`smoke`、`docs`、`clean`。

## Linux

默认 OCCT 路径：

```text
/usr/local/include/opencascade
/usr/local/lib
```

完整非 GUI 验证：

```bash
./build.sh all Release
```

Linux Viewer Smoke 需要 X11/XWayland 桌面会话：

```bash
./build.sh avalonia-smoke Release
```

其它保留 Target：`validate`、`native`、`managed`、`test`、`smoke`、`docs`、`clean`。

## Runtime

`OcctRuntime` 按操作系统解析 Native Bridge：

```text
Windows: OcctNative.dll
Linux:   libOcctNative.so
```

非默认部署可以配置 `OCCT_ROOT`、`OCCT_BRIDGE_NATIVE_DIR` 与对应平台的 Dynamic Loader 环境。

## 分支职责

- `main`：Windows Bridge 与 Windows 发布工作。
- `demo`：Windows Demo 应用。
- `avalonia`：源码型跨平台 `OcctNet + OcctNet.Avalonia`，面向 Windows/Linux。
- `website`：项目公开官网。

## 许可证

OcctCSharpBridge 使用 **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**。Open CASCADE Technology 与其它第三方组件继续遵循各自许可证。详见 [LICENSE](LICENSE)、[OcctCSharpBridge_LGPL_EXCEPTION.txt](OcctCSharpBridge_LGPL_EXCEPTION.txt)、[COMMERCIAL.md](COMMERCIAL.md) 与 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
