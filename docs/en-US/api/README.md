# OcctCSharpBridge Avalonia API Reference

The previously generated API reference belonged to the pre-split Windows layout and has been removed because it incorrectly described four managed assemblies, `net10.0-windows`, and 117 public types.

Current `avalonia` source contract:

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

Generate the complete per-type reference and Native ABI from a real build:

Windows:

```powershell
.\build.ps1 docs Release
```

Linux:

```bash
./build.sh docs Release
```

`tools/OcctApiDocsGenerator` discovers only the public projects that exist on the current branch, so regenerated output will contain Core + Avalonia only.