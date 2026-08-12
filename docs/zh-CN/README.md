# OcctCSharpBridge 中文文档

当前**源码契约**：**Bridge 2.7.0 · ABI 4 · OCCT 7.9.0 · .NET 10 / C# 14 · Windows x64**。

`bridge-contract.json` 是源码事实源：**349 Native exports、349 P/Invoke、117 public .NET types、Viewer 215、Modeling 134**。

> Published Binary SDK 状态只以仓库实际跟踪的 `main/dist/win-x64` 为准。请读取 `dist/win-x64/bridge-contract.json` 获取真实 Bridge/ABI/API 契约，读取 `dist/win-x64/bridge-manifest.json` 获取对应 Source Commit 与文件 Hash；本文档不再重复硬编码发布版本，避免下一次发布后立即过期。

## 文档导航

1. [快速开始](01_快速开始.md)
2. [架构与边界](02_架构与边界.md)
3. [API 覆盖与设计约定](03_API覆盖与设计约定.md)
4. [几何建模与拓扑分析](04_几何建模与拓扑分析.md)
5. [Viewer 选择与交互](05_Viewer选择与交互.md)
6. [网格与数据交换](06_网格与数据交换.md)
7. [运行时部署与诊断](07_运行时部署与诊断.md)
8. [构建、测试与发布](08_构建测试与发布.md)
9. [生成式 API 参考](api/README.md)

## 重要边界

XDE 只在 STEP 装配/产品结构和样式交换内部使用，不作为应用层 Document/持久化模型。上层读取的是 `OcctAssemblyDocument`；Document、Undo/Redo、Feature Tree 与 JSON 项目文件仍由业务应用负责。

## 许可证

OcctCSharpBridge 采用 **GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0**。

商业应用和闭源应用可以通过 .NET Assembly Reference、Dynamic Linking、P/Invoke 或等效 Runtime Linking 使用 Bridge，应用不会仅因为这种使用方式而被要求采用 GNU LGPL。GNU LGPL 2.1 仍适用于 OcctCSharpBridge 本身以及对外分发的 Bridge 修改/衍生版本。

正式条款见 [`LICENSE`](../../LICENSE)、[`LICENSE_LGPL_21.txt`](../../LICENSE_LGPL_21.txt)、[`OcctCSharpBridge_LGPL_EXCEPTION.txt`](../../OcctCSharpBridge_LGPL_EXCEPTION.txt) 与 [`COMMERCIAL.md`](../../COMMERCIAL.md)。

Open CASCADE Technology 及其它第三方组件继续分别遵循其自身许可证；OCCT 仍适用其自身 GNU LGPL 2.1 + Open CASCADE Exception。
