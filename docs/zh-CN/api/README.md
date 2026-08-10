# OcctCSharpBridge 完整 API 参考

本目录维护 OcctCSharpBridge 完整的中英文对应 **Managed + Native** API Reference。

## 当前契约

```text
Author: Liaoyuan Zhang
Bridge: 2.6.0
Native ABI: 4
Native exports: 344
Managed P/Invoke: 344
Public .NET types: 105
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0-windows
C#: 14.0
Native Bridge: C++17
Avalonia: 12.1.0
Platform: Windows x64
```

生成或刷新：

```powershell
.\build.ps1 docs Release
```

`tools/OcctApiDocsGenerator` 一次生成两层文档：

```text
api/reference/**     每个公开 Managed Type 独立页面
api/native-abi.md    完整 Native C ABI Reference
```

Managed 覆盖：

- `OcctNet.dll`
- `OcctNet.WinForms.dll`
- `OcctNet.Wpf.dll`
- `OcctNet.Avalonia.dll`

每个公开 Managed Type 页面记录程序集、命名空间、声明、继承、构造函数、属性、事件、方法、参数、返回值、异常、Remarks、公开字段与枚举值，并尽可能读取 XML Documentation 中的详细说明。

Native Reference 从 `src/OcctNative/OcctNative.h` 生成，覆盖 ABI 类型与全部 `344` 个 `OCCTBRIDGE_API occt_*` 导出。若公开 .NET 类型数量或 Native Export 数量与 `bridge-contract.json` 不一致，文档生成直接失败。

所有权、生命周期、线程模型、Viewer 交互、Runtime 与部署等专题语义仍以 `docs/zh-CN` 对应章节为准；本目录用于精确查询公开 Managed 签名和 Native ABI 声明。

Author 在中英文 API 文档中统一写作 **Liaoyuan Zhang**。
