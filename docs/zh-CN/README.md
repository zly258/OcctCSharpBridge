# OcctCSharpBridge 中文文档

本目录是 `OcctCSharpBridge/main` 的中文技术文档集；对应英文文档位于 [`docs/en-US`](../en-US/README.md)。两套目录采用对应章节结构，并共同提供完整 Managed + Native API Reference。

## 1. 当前契约

| 项目 | 当前值 |
| --- | --- |
| Author | **zly258** |
| Bridge Version | **2.6.0** |
| Native ABI | **4** |
| Native exports / P/Invoke | **347 / 347** |
| Public .NET types | **110** |
| Viewer / Modeling API | **213 / 134** |
| Open CASCADE Technology | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0-windows`** |
| C# | **14.0** |
| Native Bridge | **C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |

版本、平台和 API 数量的机器可读事实源是仓库根目录 `bridge-contract.json`。Author 在中英文文档和应用界面中统一写作 **zly258**。

## 2. 建议阅读顺序

| 编号 | 文档 | 主要内容 |
| --- | --- | --- |
| 01 | [快速开始](01_快速开始.md) | 环境、本地验证、构建、Headless 建模、Binary SDK |
| 02 | [架构与边界](02_架构与边界.md) | Native/C# 分层、Owner-aware、生命周期、main/demo/应用边界 |
| 03 | [API 覆盖与设计约定](03_API覆盖与设计约定.md) | API 范围、C ABI、Bulk、错误处理、所有权约定 |
| 04 | [几何建模与拓扑分析](04_几何建模与拓扑分析.md) | Primitive、B-Spline、Boolean、Topology、History、Topology Reference |
| 05 | [Viewer 选择与交互](05_Viewer选择与交互.md) | AIS/Viewer、对象、相机、选择、结构化 Hit、Host 交互 |
| 06 | [网格与数据交换](06_网格与数据交换.md) | Mesh、STEP/IGES/BREP/STL、Engine/Modeling 互操作 |
| 07 | [运行时部署与诊断](07_运行时部署与诊断.md) | DLL 部署、路径策略、Win32 126、结构化诊断 |
| 08 | [构建测试与发布](08_构建测试与发布.md) | build/test/smoke/docs/dist/publish 完整门禁 |
| API | [完整 Managed + Native API Reference](api/README.md) | 四个公开 .NET 程序集与 Native C ABI |

## 3. 文档与代码结构

```text
OcctCSharpBridge
├─ bridge-contract.json
├─ src/OcctNative
├─ src/OcctNet
├─ src/OcctNet.WinForms
├─ src/OcctNet.Wpf
├─ src/OcctNet.Avalonia
├─ tests
├─ tools/OcctApiDocsGenerator
├─ docs/
│  ├─ zh-CN/
│  │  └─ api/
│  └─ en-US/
│     └─ api/
├─ dist/win-x64
├─ build.ps1
└─ publish.ps1
```

`main` 只提供可复用 OCCT Bridge 与 Binary SDK，不实现完整 CAD 应用框架。Document、Feature Tree、Command、Tool、Undo/Redo、Snap、Grip、项目 JSON 持久化和具体业务规则属于上层应用。

## 4. API Reference

生成完整中英文 API Reference：

```powershell
.\build.ps1 docs Release
```

生成器读取四个 Managed 程序集及 XML Documentation，同时读取 `src/OcctNative/OcctNative.h`，生成：

```text
docs/zh-CN/api/reference/**
docs/zh-CN/api/native-abi.md
docs/en-US/api/reference/**
docs/en-US/api/native-abi.md
```

生成过程校验当前公开 .NET 类型和 Native C ABI 导出数量，防止接口文档与 `bridge-contract.json` 漂移。