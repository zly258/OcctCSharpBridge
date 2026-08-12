# OcctCSharpBridge Avalonia Documentation

This documentation describes the standalone **`avalonia` branch**. It contains only `OcctNet + OcctNet.Avalonia` and targets Windows x64 + Linux x64 without depending on `main`, WinForms, WPF, sync, tracked `dist`, or branch-local binary publication.

Current source contract:

- Bridge 2.7.0 / Native ABI 4
- 350 Native exports / 350 P/Invoke mappings
- 109 public .NET types
- Viewer / Modeling API: 216 / 134
- Target framework: `net10.0`
- Platforms: `windows-x64`, `linux-x64`
- Avalonia 12.1.0

Linux development defaults:

```text
/usr/local/include/opencascade
/usr/local/lib
```

The public viewport API is always `OcctAvaloniaViewport`. Windows uses an HWND/WNT_Window backend internally; Linux currently uses X11/XWayland XID/Xw_Window internally. Native Wayland hosting is not claimed yet.

## Guide

1. [Getting Started](01_Getting-Started.md)
2. [Architecture and Boundaries](02_Architecture-and-Boundaries.md)
3. [API Coverage and Design Conventions](03_API-Coverage-and-Design-Conventions.md)
4. [Geometry, Modeling and Topology](04_Geometry-Modeling-and-Topology.md)
5. [Viewer, Selection and Interaction](05_Viewer-Selection-and-Interaction.md)
6. [Mesh and Data Exchange](06_Mesh-and-Data-Exchange.md)
7. [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
8. [Build and Test](08_Build-and-Test.md)
9. [Generated API Reference](api/README.md)
