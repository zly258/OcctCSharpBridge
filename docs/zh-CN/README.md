# OcctCSharpBridge 文档

本目录描述 **`main` 分支**：基于 OCCT 7.9.0、.NET 10、C# 14 的 Windows x64 Bridge。

当前源码契约：

- Bridge 3.0.0-preview.1 / 当前 Native ABI 5，兼容 ABI 4
- Native exports / P/Invoke：431 / 431
- Public .NET types：141
- Viewer / Modeling API：292 / 139
- Target Framework：`net10.0-windows`
- 公开程序集：`OcctNet`、`OcctNet.WinForms`、`OcctNet.Wpf`

Avalonia 已从 `main` 独立出去。跨平台版本位于 [`avalonia` 分支](https://github.com/zly258/OcctCSharpBridge/tree/avalonia)，只包含 `OcctNet + OcctNet.Avalonia`，Target 为 `net10.0`，面向 Windows x64 + Linux x64。

## 文档目录

1. [快速开始](01_快速开始.md)
2. [架构与边界](02_架构与边界.md)
3. [API 覆盖与设计约定](03_API覆盖与设计约定.md)
4. [几何建模与拓扑分析](04_几何建模与拓扑分析.md)
5. [Viewer 选择与交互](05_Viewer选择与交互.md)
6. [网格与数据交换](06_网格与数据交换.md)
7. [运行时部署与诊断](07_运行时部署与诊断.md)
8. [构建、测试与发布](08_构建测试与发布.md)
9. [Generated API Reference](api/README.md)
10. [Bridge 迁移](bridge-migration.md)

`bridge-contract.json` 是源码契约事实源；`dist/win-x64/bridge-manifest.json` 描述 main 实际发布的 Windows SDK。