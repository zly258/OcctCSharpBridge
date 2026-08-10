# OcctCSharpBridge 完整 API 参考

本目录用于维护 OcctCSharpBridge 全部公开 .NET API 的中文参考文档。API Reference 不是手工维护的方法清单，而是由 `tools/OcctApiDocsGenerator` 从 `OcctNet.dll`、`OcctNet.WinForms.dll`、`OcctNet.Wpf.dll`、`OcctNet.Avalonia.dll` 及其 XML Documentation 自动生成。

生成命令：

```powershell
.\build.ps1 docs Release
```

生成后 `reference/` 下每个公开类型对应一个独立 Markdown 文件，包含程序集、命名空间、类型声明、构造函数、属性、事件、方法、参数、返回类型、枚举值和 XML Documentation 说明。

API 按四个程序集覆盖：

- `OcctNet`：`OcctEngine`、`OcctModelingSession`、几何/拓扑值类型、Mesh、Exchange、Runtime、Diagnostics 等核心能力；
- `OcctNet.WinForms`：WinForms Viewer/Viewport Host；
- `OcctNet.Wpf`：WPF Viewer/Viewport Host；
- `OcctNet.Avalonia`：Avalonia Windows HWND Viewer/Viewport Host。

专题性的所有权、生命周期、线程模型、Viewer 交互和部署约束仍以 `docs/zh-CN` 对应章节为准；API Reference 用于精确查询公开类型和成员签名。