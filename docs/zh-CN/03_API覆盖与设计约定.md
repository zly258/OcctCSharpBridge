# 03 API 覆盖与设计约定

## 当前契约

来自 `bridge-contract.json`：

- Bridge 2.7.0
- Native ABI 4
- 349 Native exports
- 349 Managed P/Invoke
- 117 Public .NET types
- Viewer API 215
- Modeling API 134

只有不兼容的 Native 契约变化才需要提升 ABI；纯新增 API 可以保持 ABI 不变并提升 Bridge 语义版本。

## API 分组

- Viewer / Camera / Rendering / Appearance；
- Selection、Hit Test、Selectability、Raw Input；
- Shape、Text、Dimension 与真正 `AIS_Point` 支撑的一等 Point；
- Headless Primitive、Boolean、Feature、Healing、History；
- Geometry/Topology、Adjacency、Curvature、Inertia、Intersection、Persistent Topology Reference；
- Mesh 与 Face provenance；
- STEP/IGES/BREP/STL；
- STEP 一等装配快照 `OcctAssemblyDocument`。

## 设计约定

- 优先强类型值对象，避免魔法数组和字符串协议；
- 高基数结果优先 Bulk Native Copy，避免 N+1 P/Invoke；
- 明确 Handle 所有权；
- 不为兼容偶然用法恢复已删除 Legacy Alias；
- 版本与统计统一读取 `bridge-contract.json`，不在文档各处手工硬编码旧数字。

## API Reference

`docs/*/api` 由 `tools/OcctApiDocsGenerator` 和 `build.ps1 docs` 生成。逐类型与 Native ABI 明细属于生成物，不手工维护。
