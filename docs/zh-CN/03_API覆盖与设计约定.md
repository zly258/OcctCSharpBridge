# API 覆盖与设计约定

`main` 当前源码契约：

```text
Native exports:     349
P/Invoke mappings:  349
Public .NET types:  113
Viewer API:         215
Modeling API:       134
```

精确数值由 `bridge-contract.json` 定义，并由静态契约检查验证。

main 的公开程序集只有 `OcctNet`、`OcctNet.WinForms`、`OcctNet.Wpf`；Avalonia 类型不计入 main，而由 `avalonia` 分支独立统计。

设计约定：Native 声明/实现/PInvoke 一一对应；统一 Cdecl + ExactSpelling；高基数数据优先 Bulk ABI；Core 不引用 UI Framework；Owner/Identity 明确；不把应用层 Document/Command/Tool 架构塞入 Bridge。

`tools/OcctApiDocsGenerator` 已改为按当前分支真实存在的公开项目自动发现，不再硬编码四个 UI 程序集。