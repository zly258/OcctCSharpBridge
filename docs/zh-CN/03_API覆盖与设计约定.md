# API 覆盖与设计约定

Bridge 3 的公开 API 以**当前源码**为事实源，不再维护手工或生成式的接口数量统计，也不再生成逐类型/逐函数 API Reference。

当前公开 Managed 程序集：

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

Native C ABI 与 Managed Interop 的一致性由 `tests/check-api-surface.ps1` 直接从源码验证：

- 跟踪的 Native Header 声明必须与 Native Definition 一一对应；
- `OcctNet` Core 的 `occt_*` 绑定必须与 Native ABI 一一对应；
- Core Bridge P/Invoke 统一使用 source-generated `LibraryImport` + Cdecl；
- Core 禁止 `DllImport`；
- WinForms/WPF/Avalonia Adapter 可以拥有 Win32/X11 等宿主平台互操作，但不得绕过 `OcctNet` 自行声明 `occt_*` Bridge ABI；
- 高基数数据优先 Snapshot/Buffer/Bulk ABI，禁止恢复 N+1 indexed interop；
- 导出函数保持语义化名称，不使用迁移版本后缀；
- Bridge 3 只支持 ABI 5，不保留 ABI4 Shim、旧 Handle 或兼容入口。

设计边界：

- `OcctModelingSession` 负责 Headless Modeling/Topology 资源；
- `OcctEngine` 负责 AIS/Viewer 展示和交互场景；
- `OcctNet` Core 不依赖 UI Framework；
- WinForms、WPF、Avalonia Adapter 互不引用；
- Owner/Identity 明确，跨 Session/Engine 对象不得混用；
- Document、Feature Tree、Command/Tool、Undo/Redo、捕捉、夹点与项目持久化属于上层应用，而不是 Bridge 公共 API 架构。

需要确认公开 API 是否变化时，应查看当前源码与运行：

```powershell
.\build.ps1 validate Release
```

而不是依赖一份可能落后的生成式接口文档。
