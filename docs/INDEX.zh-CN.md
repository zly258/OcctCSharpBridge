# OcctCSharpBridge 文档索引

本索引只组织 `main` 与 `demo` 共享的可复用 Bridge 文档。NuGet 产品化仍然只属于 `main`；完整桌面应用与发布脚本仍然只属于 `demo`。

## 建议阅读顺序

| 文档 | 适合解决的问题 |
|---|---|
| [API 覆盖说明](API_COVERAGE.zh-CN.md) | 当前 Bridge/ABI/API 范围、所有权规则、Facade 职责和校验边界 |
| [Viewer 结构化选择命中](SELECTION_HITS.zh-CN.md) | Selected/Detected AIS 实体的注册对象与 Subshape 结构化身份 |
| [Managed 几何与变换工具](GEOMETRY_UTILITIES.zh-CN.md) | 点/向量、包围盒、UV 范围、仿射矩阵、Location 与 Transform |
| [B-Spline 曲线与曲面检查](BSPLINE_CURVES.zh-CN.md) | Degree、Pole、Weight、Knot、Multiplicity 与曲面控制网格 |
| [拓扑邻接与自由边界分析](TOPOLOGY_ANALYSIS.zh-CN.md) | 批量邻接、流形/非流形检查、严格自由边界分析 |
| [批量 Face 分析与 Shape 检查](SHAPE_INSPECTION.zh-CN.md) | 大模型 Face 批量统计与不绑定业务规则的结构化审模快照 |
| [Shape Mesh Face 来源追溯](MESH_PROVENANCE.zh-CN.md) | 合并 Mesh 的源 Face 区间、拾取与 CAD/BIM 属性映射 |
| [结构化 Runtime 诊断](RUNTIME_DIAGNOSTICS.zh-CN.md) | 启动/部署排查、配置路径、实际 Loaded DLL、Win32 126 分析 |

## API 分层

Bridge 明确保持三层职责：

1. **交互式 Viewer / 文档对象层**：`OcctEngine` 与可复用 UI Host；
2. **Headless 建模层**：`OcctModelingSession`，负责几何、拓扑、算法、网格、分析与文件交换；
3. **纯 Managed 工具层**：不要求加载 OCCT 的值类型与轻量级计算。

应用层 Document、Entity、Command、Tool、Undo/Redo、JSON 持久化等职责不要重新塞回 Bridge。

## 校验层级

仓库明确区分不同校验能证明什么：

- **静态契约检查**：文件组织、C ABI/PInvoke 对应、命名、数量和文档要求；
- **Managed 回归测试**：不需要 OCCT SDK，验证所有权、值类型、Runtime 与纯 Managed 工具；
- **Smoke 项目编译**：保证真实 Native 场景与托管接口保持源码兼容；
- **本地 Native Smoke**：真正加载 OCCT 7.9.0 并执行几何/拓扑算法，是正式发布前的 Native 门禁。

各测试工程与 PowerShell 契约检查的保留职责见 [`tests/README.md`](../tests/README.md)。

执行与云端一致的 Managed 门禁：

```powershell
.\build.ps1 ci Release
```

在安装 OCCT SDK 的 Windows 机器上执行真实 Native 发布门禁：

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 根目录>"
```

## 分支职责

### `main`

只保留可复用 Native/.NET Bridge、WinForms/WPF Host、测试、API 文档，以及 **main-only NuGet** 产品化。

### `demo`

在同一套可复用 Bridge 源码之上增加 CadCommon、完整 WinForms/WPF/Avalonia 参考应用、运行/发布脚本和应用包校验。该分支中的可复用项目继续保持 `IsPackable=false`。

### `website`

静态项目网站。公开 API 统计由 CI 对照 `main/bridge-contract.json` 校验。

## 兼容性规则

Bridge `2.6.0` 使用 Native ABI `3`。新增 Native 能力采用 ABI 3 增量接口，不静默复用或改变已有 ABI 3 函数签名；Mesh 来源追溯与 ShapeInspection 组合层这类 Managed 增强本身不额外改变 Native ABI。部署时仍应保证 `OcctNet`、对应 UI Host、`OcctNative.dll`、OCCT Runtime 和第三方 DLL 来自同一套兼容构建。
