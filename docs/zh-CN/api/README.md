# OcctCSharpBridge main API 参考

分支拆分前生成的四程序集 API Reference 已删除，因为它已经不符合当前 `main`。

当前 `main` 源码契约：

```text
Bridge:            2.7.0
Native ABI:        4
Native/PInvoke:    349 / 349
Public .NET types: 113
Viewer/Modeling:   215 / 134
Target:            net10.0-windows
Platform:          windows-x64
Assemblies:        OcctNet, OcctNet.WinForms, OcctNet.Wpf
```

通过真实 Windows 构建重新生成完整逐类型文档和 Native ABI：

```powershell
.\build.ps1 docs Release
```

`tools/OcctApiDocsGenerator` 会按当前分支实际存在项目自动发现，因此 main 重新生成后不会再包含 Avalonia。