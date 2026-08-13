# OcctCSharpBridge Avalonia Documentation

This documentation describes the standalone **`avalonia` branch** for Windows x64 and Linux x64.

Current source contract:

- Bridge 2.7.0 / Native ABI 4
- Native exports / P/Invoke: **420 / 420**
- Public .NET types: **135**
- Viewer / Modeling API: **286 / 134**
- Target framework: `net10.0`
- Platforms: `windows-x64`, `linux-x64`
- Avalonia 12.1.0

The public viewport API is always `OcctAvaloniaViewport`. Windows uses an HWND/WNT_Window backend internally. Linux currently uses X11/XWayland XID/Xw_Window; native Wayland hosting is not claimed.

Linux native-child input handles selection, pan, rotate and zoom through X11. Consecutive pointer motion events are coalesced before OCCT interaction updates. UI text uses bundled Inter and OCCT vector text/dimensions use the portable `sans-serif` alias.

## Build / run

Windows:

```powershell
.\build.ps1
.\run.ps1
```

Linux:

```bash
./build.sh
./run.sh
```

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
