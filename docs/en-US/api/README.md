# OcctCSharpBridge main API Reference

The previously generated API reference belonged to the pre-split four-assembly layout and has been removed because it no longer matches `main`.

Current `main` source contract:

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

Regenerate the complete per-type and Native ABI reference from a real Windows build:

```powershell
.\build.ps1 docs Release
```

`tools/OcctApiDocsGenerator` discovers the public projects that actually exist on the current branch, so regenerated main output will not include Avalonia.