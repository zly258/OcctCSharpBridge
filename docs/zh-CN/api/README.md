# OcctCSharpBridge Avalonia API 参考

拆分分支前生成的旧 API Reference 已删除，因为其中仍错误描述“四个 Managed Assembly、`net10.0-windows`、117 个公开类型”。

当前 `avalonia` 源码契约：

```text
Bridge:            2.7.0
Native ABI:        4
Native/PInvoke:    350 / 350
Public .NET types: 109
Viewer/Modeling:   216 / 134
Target:            net10.0
Platforms:         windows-x64, linux-x64
Assemblies:        OcctNet, OcctNet.Avalonia
```

通过真实构建重新生成完整逐类型文档和 Native ABI：

Windows：

```powershell
.\build.ps1 docs Release
```

Linux：

```bash
./build.sh docs Release
```

`tools/OcctApiDocsGenerator` 会按当前分支实际存在的公开项目自动发现，因此重新生成后只会包含 Core + Avalonia。