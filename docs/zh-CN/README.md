# OcctCSharpBridge Avalonia 文档

本目录描述独立的 **`avalonia` 分支**，同时面向 Windows x64 与 Linux x64。

当前源码契约：

- Bridge 2.7.0 / Native ABI 4
- Native exports / P/Invoke：**420 / 420**
- Public .NET types：**135**
- Viewer / Modeling API：**286 / 134**
- Target Framework：`net10.0`
- Platforms：`windows-x64`、`linux-x64`
- Avalonia 12.1.0

公开 Viewport 始终是 `OcctAvaloniaViewport`。Windows 内部使用 HWND/WNT_Window；Linux 当前使用 X11/XWayland XID/Xw_Window，暂不宣称 Native Wayland Viewer 已完成。

Linux 下通过 X11 原生子窗口处理选择、平移、旋转和缩放，连续鼠标 Motion 事件在进入 OCCT 交互更新前合并。UI 使用项目内置 Inter，OCCT 矢量文字/标注使用跨平台 `sans-serif` 字体别名。

## 构建 / 运行

Windows：

```powershell
.\build.ps1
.\run.ps1
```

Linux：

```bash
./build.sh
./run.sh
```

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
