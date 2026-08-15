# Bridge 3 ABI5 迁移

Bridge 3 将 `main` 定义为唯一正式 SDK 源。迁移顺序为：

1. 在 `main-dev` 完成 ABI5-only Native Core、`OcctNet`、WinForms、WPF 与 Avalonia Host 的稳定和校验。
2. 在 `demo-dev` 将 Windows 示例迁移为消费最终 SDK。
3. 在 `avalonia-dev` 将 Avalonia 示例和打包流程迁移为消费同一套 SDK。
4. 分别提交 `main-dev -> main`、`demo-dev -> demo`、`avalonia-dev -> avalonia` PR。

## ABI 规则

ABI 5 是唯一支持的 Native ABI。pre-ABI5 导出、通用旧 Handle、兼容 Shim、固定旧 Consumer 测试与兼容元数据直接删除，不冻结、不转发。

公开 Native 入口使用语义化名称。需要可扩展布局的数据结构使用 `structSize` 与 `apiVersion`；导出函数名不使用 `V1`、`V2`、`Ex` 等迁移后缀。

正式 Managed Interop 统一使用 source-generated `LibraryImport` 与 C Calling Convention，并与 ABI5 Canonical Declaration / Definition 一一对应。

## 当前架构

- Engine、Shape、Mesh、Algorithm 使用类型化资源所有权；
- 使用 `OcctStatus` 与结构化 Native Error State；
- Viewer/Modeling 批量数据采用 Caller-owned Snapshot/Buffer API；
- Viewer/Scene/Document Context 与 Headless Modeling 状态分离；
- 操作系统窗口集成限制在 `src/OcctNative/platform`；
- Topology History 与 Persistent Topology Reference 归 Modeling Session 管理；
- `demo-dev` 与 `avalonia-dev` 只作为 SDK Consumer，不保存私有 Native/Core 实现。

## 校验门禁

每次标准校验都会检查：

- `bridge-contract.json` 的 Current/Minimum Supported ABI 都是 5，且 `api.policy = abi5-only`；
- 旧兼容文件与旧版本专用文档不得继续跟踪；
- 仓库中存在的平台 Binary SDK 契约必须是 ABI5-only；
- Native Declaration、Definition 与 Managed `LibraryImport` 集合完全一致；
- 正式 Managed ABI5 Interop 不允许 `DllImport`；
- Native C++ Inventory、领域边界与平台隔离保持正确；
- 批量集合使用 Snapshot/Buffer API，不允许回到 borrowed legacy handle。
