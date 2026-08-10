# OcctCSharpBridge 中文文档

本目录是 `OcctCSharpBridge/main` 的中文技术文档集；对应英文文档位于 [`docs/en-US`](../en-US/README.md)。两套目录采用对应章节结构，并共同提供完整 API Reference。

版本、平台和 API 数量的唯一机器可读事实源仍是仓库根目录 `bridge-contract.json`。专题文档解释设计、约束和使用方式；`api/` 由程序集和 XML Documentation 自动生成精确公开接口清单。

## 1. 当前基础契约

| 项目 | 当前值 |
|---|---|
| Bridge | `2.6.0` |
| Native ABI | `4` |
| Open CASCADE Technology | `7.9.0` |
| .NET SDK | `10.0.302` |
| Target Framework | `net10.0-windows` |
| C# | `14.0` |
| 平台 | Windows x64 |

## 2. 建议阅读顺序

| 编号 | 文档 | 主要内容 |
|---|---|---|
| 01 | [快速开始](01_快速开始.md) | 环境、本地验证、构建、Headless 建模、Binary SDK |
| 02 | [架构与边界](02_架构与边界.md) | Native/C# 分层、Owner-aware、生命周期、main/demo/应用边界 |
| 03 | [API 覆盖与设计约定](03_API覆盖与设计约定.md) | API 范围、C ABI、Bulk、错误处理、所有权约定 |
| 04 | [几何建模与拓扑分析](04_几何建模与拓扑分析.md) | Primitive、B-Spline、Boolean、Topology、History、Topology Reference |
| 05 | [Viewer 选择与交互](05_Viewer选择与交互.md) | AIS/Viewer、对象、相机、选择、结构化 Hit、Host 交互 |
| 06 | [网格与数据交换](06_网格与数据交换.md) | Mesh、STEP/IGES/BREP/STL、Engine/Modeling 互操作 |
| 07 | [运行时部署与诊断](07_运行时部署与诊断.md) | DLL 部署、路径策略、Win32 126、结构化诊断 |
| 08 | [构建测试与发布](08_构建测试与发布.md) | build/test/smoke/docs/dist/publish 完整门禁 |
| API | [完整 API Reference](api/README.md) | 四个公开程序集的全部类型与成员 |

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

`main` 只提供可复用 OCCT Bridge，不实现完整 CAD 应用框架。Document、Feature Tree、Command、Tool、Undo/Redo、Snap、Grip、项目 JSON 持久化和具体业务规则属于 `demo` 或其它上层应用。

## 4. API Reference

生成完整中英文 API Reference：

```powershell
.\build.ps1 docs Release
```

生成器读取：

```text
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
对应 XML Documentation
```

并在两种语言的 `api/reference/` 下为每个公开类型生成独立页面，包含程序集、命名空间、类型声明、构造函数、属性、事件、方法、参数、返回类型和字段/枚举值。

API 增删后重新运行生成器，不手工维护大规模方法清单。

## 5. Binary SDK 与 demo

Binary SDK 生成：

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

正式发布到 demo：

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`demo` 不再保存 Bridge 源码，也不维护反向同步脚本；Binary SDK 统一由 `main/publish.ps1` 发布。

## 6. 文档维护原则

1. **代码和 `bridge-contract.json` 是机器可读事实源。**
2. **中文与英文目录章节对应。** 能力变化时同步更新语义，不保留互相矛盾的旧说明。
3. **API Reference 自动生成。** 精确成员签名不依赖手工抄写。
4. **只记录已实现能力。** 规划项不伪装成现状。
5. **公共 API 使用真实类型和方法名。**
6. **不记录阶段性开发流水账。** 历史由 Git Commit 承担。
7. **不恢复旧兼容层。** 删除的 Alias、Legacy Wrapper、聚合头不以兼容名重新引入。
8. **不使用 GitHub Actions 替代真实 Windows/MSVC/OCCT 本地验证。**
