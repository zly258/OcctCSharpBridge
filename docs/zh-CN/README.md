# OcctCSharpBridge Avalonia 文档

本目录描述独立的 **`avalonia` 分支**。该分支只包含 `OcctNet + OcctNet.Avalonia`，同时面向 Windows x64 与 Linux x64，不依赖 main、WinForms、WPF，也没有 sync、跟踪式 `dist` 或分支内 Binary SDK 发布流程。

当前源码契约：

- Bridge 2.7.0 / Native ABI 4
- Native exports / P/Invoke：350 / 350
- Public .NET types：109
- Viewer / Modeling API：216 / 134
- Target Framework：`net10.0`
- Platforms：`windows-x64`、`linux-x64`
- Avalonia 12.1.0

Linux 默认 OCCT：

```text
/usr/local/include/opencascade
/usr/local/lib
```

公开 Viewport 始终是 `OcctAvaloniaViewport`。Windows 内部使用 HWND/WNT_Window，Linux 当前内部使用 X11/XWayland XID/Xw_Window；暂不宣称 Native Wayland Viewer 已完成。

## 文档目录

1. [快速开始](01_快速开始.md)
2. [架构与边界](02_架构与边界.md)
3. [API 覆盖与设计约定](03_API覆盖与设计约定.md)
4. [几何建模与拓扑分析](04_几何建模与拓扑分析.md)
5. [Viewer 选择与交互](05_Viewer选择与交互.md)
6. [网格与数据交换](06_网格与数据交换.md)
7. [运行时部署与诊断](07_运行时部署与诊断.md)
8. [构建与测试](08_构建与测试.md)
9. [Generated API Reference](api/README.md)
