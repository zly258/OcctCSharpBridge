# 00 文档索引

本目录是 `OcctCSharpBridge/main` 的唯一 Bridge 技术文档源。根目录保留 `README.md` 与 `README.zh-CN.md` 两个项目入口；`docs` 只维护一套中文技术正文，API、类型名、文件名和命令保持英文原名，避免双语正文长期漂移。

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

版本、平台和 API 数量的唯一机器可读来源是仓库根目录 `bridge-contract.json`。文档用于解释设计和使用方式，不作为构建配置的第二份来源。

## 2. 建议阅读顺序

| 编号 | 文档 | 主要内容 |
|---|---|---|
| 01 | [快速开始](01_快速开始.md) | 环境、本地验证、构建、Headless 建模、Viewer、Runtime 配置 |
| 02 | [架构与边界](02_架构与边界.md) | Native/C# 分层、`main`/`demo` 边界、Owner-aware、生命周期、Binary SDK 同步规则 |
| 03 | [API 覆盖与设计约定](03_API覆盖与设计约定.md) | API 范围、C ABI、Bulk、错误处理、所有权约定 |
| 04 | [几何建模与拓扑分析](04_几何建模与拓扑分析.md) | Primitive、B-Spline、Boolean、Topology、History、Topology Reference |
| 05 | [Viewer 选择与交互](05_Viewer选择与交互.md) | AIS/Viewer、对象、相机、选择、结构化 Hit、Host 交互 |
| 06 | [网格与数据交换](06_网格与数据交换.md) | Mesh、STEP/IGES/BREP/STL、Engine/Modeling 互操作 |
| 07 | [运行时部署与诊断](07_运行时部署与诊断.md) | DLL 部署、路径策略、Win32 126、结构化诊断 |
| 08 | [构建测试与发布](08_构建测试与发布.md) | `build.ps1`、静态契约、Managed Test、Native Smoke、发布门禁 |

## 3. 仓库职责

```text
OcctCSharpBridge
├─ bridge-contract.json          版本、平台和 API 数量的机器可读契约
├─ src/OcctNative               C++17 + OCCT + 稳定 C ABI
├─ src/OcctNet                  核心 .NET Bridge
├─ src/OcctNet.WinForms         WinForms HWND Host
├─ src/OcctNet.Wpf              WPF Host
├─ src/OcctNet.Avalonia         Avalonia Windows HWND Host
├─ tests                         静态契约、Managed 回归、Native Smoke
├─ docs                          main 的唯一 Bridge 技术文档源
├─ build.ps1                    日常 validate/build/test/pack/smoke 入口
├─ dist.ps1                     Release 全验证后生成 Binary SDK
└─ dist/win-x64                 可提交、可供其它项目直接消费的已验证 DLL/Contract/Manifest
```

`main` 只提供可复用 OCCT Bridge，不实现完整 CAD 应用框架。Document、Feature Tree、Command、Tool、Undo/Redo、Snap、Grip、项目 JSON 持久化以及具体 BIM/设备业务规则属于 `demo` 或其它上层项目。

## 4. 分支文档规则

- `main/docs`：维护 Bridge 架构、API、构建、部署等长期技术事实。
- `main/dist/win-x64`：维护已通过 Release Build、Managed Test 和 Native Smoke 的 Binary SDK。
- `demo`：只维护 Demo 应用源码，并消费从 `main/dist/win-x64` 同步的 Binary SDK；不再复制 Bridge 源码。
- `demo/docs`：只维护 Demo 构建、运行、发布与 Binary SDK 同步说明。
- Bridge 能力发生变化时先更新 `main` 的代码、测试和文档，再重新生成并提交 `dist/win-x64`，最后由 Demo 同步二进制。

这样避免同一能力在多个分支重复维护源码和技术文档。

## 5. 验证方式

本仓库不依赖 GitHub Actions 作为构建或同步机制，验证以本地 `build.ps1` 和真实 Windows/OCCT 环境为准。

不需要 OCCT SDK：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
```

构建 Native + Managed Bridge：

```powershell
.\build.ps1 all Release
```

真实 Native 门禁：

```powershell
.\build.ps1 smoke Release
```

生成可提交 Binary SDK：

```powershell
.\dist.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`dist.ps1` 只有在 Release Build、Managed Test 和 Native Smoke 全部成功后才刷新 `dist/win-x64`。

## 6. 文档维护原则

1. **代码和 `bridge-contract.json` 是事实来源。**
2. **一个主题只保留一个主文档。**
3. **只写已经存在的能力。** 不把计划接口写成现状。
4. **公共 API 使用真实类型和方法名。**
5. **不记录阶段性开发流水账。** 历史由 Git Commit 承担。
6. **架构边界优先。** 不为方便 Demo 将 CAD Framework 下沉到 Bridge。
7. **不恢复旧兼容层。** 删除的 Alias、Legacy Wrapper、聚合头不以兼容名重新引入。
8. **Demo 通过 Binary SDK 验证真实消费边界。** 不再依赖共享源码镜像。
