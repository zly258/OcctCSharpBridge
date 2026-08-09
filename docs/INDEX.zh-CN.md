# OcctCSharpBridge 文档索引

本索引组织 `main` 与 `demo` 共享的 **OCCT Bridge 文档**。NuGet 产品化只属于 `main`；完整 CAD 应用、CadCommon、运行/发布脚本属于 `demo`。

## 建议阅读顺序

| 文档 | 适合解决的问题 |
|---|---|
| [架构边界：Bridge 与 CAD 应用层](ARCHITECTURE_BOUNDARIES.zh-CN.md) | `main` / `demo` 职责、为什么 Document/Command/Tool 不进入 Bridge、UI Host 的共享边界 |
| [API 覆盖说明](API_COVERAGE.zh-CN.md) | 当前 Bridge/ABI/API 范围、所有权规则、Facade 职责和校验边界 |
| [快速开始](GETTING_STARTED.zh-CN.md) | 引用核心包与 WinForms/WPF/Avalonia Host、部署 Runtime |
| [打包说明](PACKAGING.zh-CN.md) | main-only NuGet、包内容与 Native Runtime 边界 |
| [Viewer 结构化选择命中](SELECTION_HITS.zh-CN.md) | Selected/Detected AIS 实体的注册对象与 Subshape 结构化身份 |
| [Managed 几何与变换工具](GEOMETRY_UTILITIES.zh-CN.md) | 点/向量、包围盒、UV 范围、仿射矩阵、Location 与 Transform |
| [B-Spline 曲线与曲面检查](BSPLINE_CURVES.zh-CN.md) | Degree、Pole、Weight、Knot、Multiplicity 与曲面控制网格 |
| [拓扑邻接与自由边界分析](TOPOLOGY_ANALYSIS.zh-CN.md) | 批量邻接、流形/非流形检查、严格自由边界分析 |
| [批量 Face 分析与 Shape 检查](SHAPE_INSPECTION.zh-CN.md) | 大模型 Face 批量统计与不绑定业务规则的结构化审模快照 |
| [Shape Mesh Face 来源追溯](MESH_PROVENANCE.zh-CN.md) | 合并 Mesh 的源 Face 区间、拾取与 CAD/BIM 属性映射 |
| [结构化 Runtime 诊断](RUNTIME_DIAGNOSTICS.zh-CN.md) | 启动/部署排查、配置路径、实际 Loaded DLL、Win32 126 分析 |

## API 分层

Bridge 明确保持三层职责：

1. **交互式 Viewer/Object 层**：`OcctEngine`；
2. **Headless 建模层**：`OcctModelingSession`，负责几何、拓扑、算法、网格、分析与文件交换；
3. **可复用 UI Host 层**：WinForms、WPF、Windows-HWND Avalonia，仅负责把 UI 窗口与输入连接到 `OcctEngine`。

Document、Entity、Feature Tree、Command、Tool、Undo/Redo、Snap/Grip、JSON 持久化等应用职责不进入 Bridge。

## 校验层级

- **静态契约检查**：文件组织、C ABI/PInvoke 对应、命名、数量、分支边界和文档要求；
- **Managed 回归测试**：不加载 OCCT，验证所有权、值类型、Runtime、纯 Managed 工具与公共 API 签名快照；
- **Smoke 项目编译**：保证真实 Native 场景与托管接口保持源码兼容；
- **本地 Native Smoke**：真正加载 OCCT 7.9.0 并执行几何/拓扑算法，是正式发布前的 Native 门禁。

各测试工程与契约检查的职责见 [`tests/README.md`](../tests/README.md)。

```powershell
.\build.ps1 ci Release
```

真实 Native 发布门禁：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```

## 分支职责

### `main`

只保留可复用 Native/.NET Bridge、`OcctNet.WinForms`、`OcctNet.Wpf`、`OcctNet.Avalonia`、测试、API 文档和 **main-only NuGet** 产品化。不得加入 CadCommon、完整 CAD 应用或应用级 Document/Command/Tool 框架。

### `demo`

在同一套可复用 Bridge 源码之上增加 `CadCommon` 和完整 WinForms/WPF/Avalonia 参考应用、运行/发布脚本及应用包校验。该分支中的可复用项目保持 `IsPackable=false`。

### `website`

静态项目网站。公开 API 统计由 CI 对照 `main/bridge-contract.json` 校验。

## 兼容性规则

Bridge `2.6.0` 使用 Native ABI `3`。内部 cpp/header 拆分不改变已有 ABI 3 函数签名。Bridge 2.5 的 `OcctObject` 兼容类型在 2.x 中继续保留，但不再扩展新的 legacy API；新代码使用 owner-aware 对象接口。部署时应保证 `OcctNet`、对应 UI Host、`OcctNative.dll`、OCCT Runtime 与第三方 DLL 来自同一兼容构建。
