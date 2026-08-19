# OcctCSharpBridge 文档

本目录描述 Bridge 3 ABI5-only 的受支持架构、SDK 消费方式、运行时部署、构建/测试边界和 Stable 兼容规则。`bridge-contract.json` 是机器可读事实源。

## 当前 3.0 Stable 契约

- Bridge：`3.0.0`
- Native ABI：**仅 ABI 5**（`current = 5`、`minimumSupported = 5`）
- API Policy：`abi5-only`
- OCCT：`7.9.0`
- 构建 SDK：稳定版 .NET 10，基线 `10.0.100`，`rollForward=latestFeature`
- Managed Binary SDK：Core/Avalonia `net8.0`；WinForms/WPF `net8.0-windows`
- Consumer：.NET 8 / 9 / 10
- 公开 Managed 程序集：`OcctNet`、`OcctNet.WinForms`、`OcctNet.Wpf`、`OcctNet.Avalonia`
- 官方预编译分发：**Windows x64**
- 源码构建支持：Windows x64 / Linux x64
- Linux UI：Avalonia（源码构建，不发布官方预编译包）

`main` / `main-dev` 是 Bridge SDK 源码线；`demo` / `demo-dev` 是参考 SDK Consumer，不维护第二套 Bridge 实现。

## 文档目录

1. [快速开始](01_快速开始.md) — 选择正确 SDK 并完成最小接入。
2. [架构与边界](02_架构与边界.md) — 组件职责和依赖边界。
3. [API 覆盖与设计约定](03_API覆盖与设计约定.md) — API 组织与设计规范。
4. [几何建模与拓扑分析](04_几何建模与拓扑分析.md) — Modeling 侧概念。
5. [Viewer 选择与交互](05_Viewer选择与交互.md) — Host 生命周期和交互契约。
6. [网格与数据交换](06_网格与数据交换.md) — Mesh 与 Exchange 边界。
7. [运行时部署与诊断](07_运行时部署与诊断.md) — Native/OCCT 部署和排障。
8. [构建、测试与发布](08_构建测试与发布.md) — Consumer 快路径、Windows Stable Gate 与 Linux 源码验证。
9. [第三方项目消费 SDK](09_第三方项目消费SDK.md) — Core、WinForms、WPF、Avalonia 的第三方接入、部署和升级。
10. [Stable 支持与兼容策略](10_稳定版支持与兼容策略.md) — 平台、.NET、ABI、线程、生命周期、单位、容差与版本兼容边界。
11. [Bridge 3 ABI5 迁移](bridge-migration.md) — 从旧 Bridge 契约迁移。

根目录 [CHANGELOG](../../CHANGELOG.md) 记录正式版本线变更。

## 阅读建议

第三方应用团队：**01 → 09 → 10 → 07**。

Bridge 维护者：在上述基础上阅读 **02、03、08**，并在 Stable 候选上运行 `tools/validate-stable-release.ps1`。
