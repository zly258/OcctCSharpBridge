# OcctCSharpBridge 文档

本目录描述 Bridge 3 ABI5-only 的受支持架构、SDK 消费方式、运行时部署、构建/测试边界和迁移约定。`bridge-contract.json` 是机器可读的唯一契约事实源。

## 当前契约

- Bridge：`3.0.0-preview.1`
- Native ABI：**仅 ABI 5**（`current = 5`、`minimumSupported = 5`）
- API Policy：`abi5-only`
- OCCT：`7.9.0`
- 构建 SDK：稳定版 **.NET 10**，基线 `10.0.100`，`rollForward=latestFeature`，禁止预览 SDK
- Managed Binary SDK：Core/Avalonia 为 `net8.0`，WinForms/WPF 为 `net8.0-windows`
- Consumer 支持：.NET 8 / 9 / 10；Windows Desktop 支持对应的 .NET 8 / 9 / 10
- 公开 Managed 程序集：`OcctNet`、`OcctNet.WinForms`、`OcctNet.Wpf`、`OcctNet.Avalonia`
- 平台：Windows x64 / Linux x64

`main` / `main-dev` 是 Bridge SDK 源码线；`demo` / `demo-dev` 是参考 Binary SDK Consumer，不维护第二套 Bridge 实现。

## 文档目录

1. [快速开始](01_快速开始.md) — 选择正确 SDK 并完成最小接入。
2. [架构与边界](02_架构与边界.md) — 组件职责和依赖边界。
3. [API 覆盖与设计约定](03_API覆盖与设计约定.md) — API 组织与设计规范。
4. [几何建模与拓扑分析](04_几何建模与拓扑分析.md) — Modeling 侧概念。
5. [Viewer 选择与交互](05_Viewer选择与交互.md) — Host 生命周期和交互契约。
6. [网格与数据交换](06_网格与数据交换.md) — Mesh 与 Exchange 边界。
7. [运行时部署与诊断](07_运行时部署与诊断.md) — Native/OCCT 部署和排障。
8. [构建、测试与发布](08_构建测试与发布.md) — Consumer 快速产物与正式完整 Gate 的职责划分。
9. [第三方项目消费 SDK](09_第三方项目消费SDK.md) — Core、WinForms、WPF、Avalonia 的完整第三方接入说明。
10. [Bridge 3 ABI5 迁移](bridge-migration.md) — 从旧 Bridge 契约迁移。

## 文档维护原则

仓库保留架构、使用、部署和工程流程文档，不生成逐类型/逐函数 API Reference。Native/Managed API Surface 直接从当前源码校验，避免文档形成第二套容易过期的事实源。

普通应用团队建议从 **09 第三方项目消费 SDK** 开始；Bridge 维护者在修改 SDK 生产或发布流程前还应阅读 **08 构建、测试与发布**。
