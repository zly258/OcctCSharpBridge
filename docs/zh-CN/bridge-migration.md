# Bridge 3 ABI5 迁移

Bridge 3 将 `main` 定义为唯一正式 SDK 源。当前分支迁移已经收敛为一条 SDK 线和一条统一 Demo Consumer 线：

1. `main-dev` 完成 ABI5-only Native Core、`OcctNet`、WinForms、WPF、Avalonia Adapter 的稳定与校验；
2. 验证后的 SDK Rewrite 通过 Squash PR 合并到 `main`；
3. `demo-dev` 完成 WinForms/WPF 迁移，并吸收 Avalonia Windows/Linux 示例和打包流程；
4. 统一 Demo 通过 Squash PR 合并到 `demo`；
5. 独立 `avalonia` / `avalonia-dev` 分支迁移完成后废弃；Avalonia 本身仍是正式 SDK Adapter 与 Demo Host。

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
- `main` / `main-dev` 负责 SDK 实现；
- `demo` / `demo-dev` 只消费生成的 Binary SDK；
- Windows Demo Host：WinForms、WPF、Avalonia；
- Linux Demo Host：仅 Avalonia。

## Binary SDK 策略

生成的 `dist/win-x64` 与 `dist/linux-x64` 是本地/Release 构建产物，不提交到 SDK 或 Demo 源码分支。Package 是否最新通过 schema-3 Contract、schema-2 Manifest、`sourceCommit` 与 SHA-256 建立，而不是通过 Git 中提交二进制判断。

## 校验门禁

每次标准校验都会检查：

- `bridge-contract.json` 的 Current/Minimum Supported ABI 都是 5，且 `api.policy = abi5-only`；
- 旧兼容文件与旧版本专用文档不得继续跟踪；
- Native Declaration、Definition 与 Managed `LibraryImport` 集合完全一致；
- 正式 Managed ABI5 Interop 不允许 `DllImport`；
- Native C++ Inventory、领域边界与平台隔离保持正确；
- 批量集合使用 Snapshot/Buffer API，不允许回到 borrowed legacy handle。

Demo Consumer Guard 额外拒绝 SDK 实现源码、直接 `occt_*` 调用和已经退休的 Managed Consumer API。
