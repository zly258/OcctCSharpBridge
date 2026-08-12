# API 覆盖与设计约定

`avalonia` 当前源码契约：

```text
Native exports:     350
P/Invoke mappings:  350
Public .NET types:  109
Viewer API:         216
Modeling API:       134
```

相对于 main，多出的 Viewer ABI 是平台无关 Native Surface 初始化入口。

公开 Managed Assembly 只有：

```text
OcctNet
OcctNet.Avalonia
```

继续遵循 Native/PInvoke 精确对等、Cdecl + ExactSpelling、Bulk 高基数传输、Core 不依赖 UI Framework、明确 Owner/Identity、Bridge 不承担应用层 Document/Command/Tool Framework 等规则。

API 文档生成器按当前分支实际存在项目自动发现。