# API 覆盖与设计约定

Bridge 3 的公开 API 以**当前源码和实际编译结果**为事实源，不维护手工 API 数量表，也不通过额外源码扫描脚本冻结实现细节。

当前公开 Managed 程序集：

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

基本原则：

- Native C ABI 与 Managed Interop 使用相同的语义化入口；
- Core Bridge P/Invoke 使用 source-generated `LibraryImport` + Cdecl；
- 高基数数据优先 Snapshot/Buffer/Bulk ABI，避免 N+1 interop；
- Bridge 3 当前只支持 ABI 5；
- `OcctModelingSession` 负责 Headless Modeling/Topology；
- `OcctEngine` 负责 AIS/Viewer 展示与交互；
- `OcctNet` Core 不依赖 UI Framework；
- WinForms、WPF、Avalonia 仅作为宿主适配层；
- Document、Feature Tree、Command/Tool、Undo/Redo、捕捉、夹点和项目持久化属于上层应用。

验证以编译和测试为主：

```powershell
.\build.ps1 build Release
.\build.ps1 test Release
.\build.ps1 smoke Release
```

不再维护源码扫描型 policy target；编译和正式测试是正确性门槛。
