# Bridge 迁移

Bridge 3 将 `main` 定义为唯一正式 SDK 源。仓库按以下顺序迁移：

1. 在 `main-dev` 稳定 Native Core、`OcctNet`、WinForms 和 WPF。
2. 在 `demo-dev` 将 Windows 示例改为消费正式 SDK。
3. 在 `avalonia-dev` 将 Avalonia 示例与打包流程改为消费同一套 SDK。

## ABI 规则

ABI 5 是当前契约。ABI 4 固定为 419 个导出并保持二进制兼容，计划在 Bridge 4.0 删除。新增功能只能扩展当前 ABI。

原有 `occt_bridge_abi_version` 入口为固定的 2.7 Consumer 保留 ABI 4 语义；当前 SDK 使用 `occt_bridge_current_abi_version` 查询当前 ABI。

公开 API 和文件名使用语义化名称。版本信息放在 `structSize` 与 `apiVersion` 中；新 API 不使用 `V1`、`V2`、`Ex` 等后缀。

## 当前预览范围

本预览引入 Engine 与 Modeling Session 类型化句柄、`SafeHandle` 所有权、`OcctStatus`、调用方持有的错误缓冲区、Viewer/Scene/Document 上下文、平台窗口隔离、拓扑历史和持久拓扑引用。

正式托管 SDK 使用类型化生命周期和语义化 native surface API。旧生命周期与 surface 入口仅作为兼容适配器，并由固定旧 Consumer 可执行程序验证。
正式 current ABI 的托管声明使用 source-generated `LibraryImport`，并明确指定 C calling convention。冻结的 ABI 4 声明与兼容扩展继续隔离在 `DllImport`；契约检查会阻止两组声明越界。
Current ABI 提供 opaque `OcctShapeHandle` 与 `OcctMeshHandle` 资源。Shape snapshot 与独立分配的连续 Mesh buffer 都由 Managed `SafeHandle` wrapper 持有，即使源 Session registry entry 已删除仍然有效。Mesh 创建参数采用可扩展的 `structSize`/`apiVersion` 结构，节点和三角形通过 caller-owned bulk buffer 复制；调用方不得让资源查询与 Dispose 并发发生。



## 兼容性门禁

每次标准构建都会验证：

- ABI 4 固定的 419 个符号全部保留；
- Native 声明、实现与 P/Invoke 集合完全一致；
- 19 个正式 current ABI 声明全部使用 `LibraryImport`，兼容扩展保持隔离；
- 新导出与受跟踪文件名遵守语义化命名；
- WinForms 与 WPF 通过平台无关的托管 Engine API 工作；
- 固定 ABI 4 Consumer 和当前 ABI 5 Native smoke 都能成功运行。

`demo-dev` 与 `avalonia-dev` 继续作为外部 SDK 消费者；Core 演进与 ABI 所有权始终留在 `main-dev`。
