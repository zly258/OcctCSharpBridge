# OcctCSharpBridge 文档

本目录描述 Bridge 3 ABI5-only SDK 的架构、使用、构建、部署和迁移约定。`bridge-contract.json` 是源码契约事实源。

当前源码契约：

- Bridge：`3.0.0-preview.1`；
- Native ABI：**仅 ABI 5**，`current = 5`、`minimumSupported = 5`；
- API Policy：`abi5-only`；
- OCCT：`7.9.0`；
- .NET SDK：**精确 `10.0.303`**，禁止 roll-forward；
- Target Framework：Core/Avalonia 为 `net10.0`，WinForms/WPF 为 `net10.0-windows`；
- 公开 Managed 程序集：`OcctNet`、`OcctNet.WinForms`、`OcctNet.Wpf`、`OcctNet.Avalonia`；
- 源码平台：Windows x64 / Linux x64。

`demo` 与 `avalonia` 正式分支只作为 SDK Consumer 和打包示例；Bridge SDK 实现统一由 `main` 维护。

## 文档目录

1. [快速开始](01_快速开始.md)
2. [架构与边界](02_架构与边界.md)
3. [API 覆盖与设计约定](03_API覆盖与设计约定.md)
4. [几何建模与拓扑分析](04_几何建模与拓扑分析.md)
5. [Viewer 选择与交互](05_Viewer选择与交互.md)
6. [网格与数据交换](06_网格与数据交换.md)
7. [运行时部署与诊断](07_运行时部署与诊断.md)
8. [构建、测试与发布](08_构建测试与发布.md)
9. [Bridge 3 ABI5 迁移](bridge-migration.md)

本目录不再跟踪生成式逐类型/逐函数 API Reference。Native/Managed API Surface 的完整性由 `tests/check-api-surface.ps1` 直接从当前源码验证，不维护容易失真的硬编码接口数量。
