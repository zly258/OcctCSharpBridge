# OcctCSharpBridge Demo

[Main SDK branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [简体中文](README.zh-CN.md) · [API inventory and usage guide](docs/API_COVERAGE.md)

The `demo` branch adds WinForms and WPF reference applications to the reusable OCCT C# bridge. `src/OcctNative`, `src/OcctNet`, `src/OcctNet.WinForms`, `src/OcctNet.Wpf`, the reusable smoke project, and the two API inventories stay synchronized with `main`; application UI, scenarios, run scripts, publishing, and package validation remain demo-only.

The managed wrapper is split into:

- `OcctNet`: UI-independent viewer, modeling, topology, analytic geometry, differential geometry, analysis, healing, mesh, and exchange APIs.
- `OcctNet.WinForms`: reusable `OcctViewportControl` bound directly to a Win32 HWND.
- `OcctNet.Wpf`: reusable `OcctWpfViewport` with WPF dependency properties, event forwarding, DPI synchronization, and native resize coordination.

`OcctModelingSession` is organized by responsibility: lifecycle, shape queries, topology, geometry queries, analytic geometry, differential geometry, construction, algorithms, analysis, mesh, exchange, and operation history. Canonical names describe the subject and parameter semantics; existing ambiguous names remain compatibility aliases.

OCAF/XDE is not included. Documents, JSON persistence, undo/redo, and command history belong to the consuming application.

## Preview

<table>
  <tr>
    <th>WinForms · English</th>
    <th>WPF · English</th>
  </tr>
  <tr>
    <td><img src="assets/previews/winform-demo-en.webp" alt="OCCT CAD WinForms English demo" width="100%"></td>
    <td><img src="assets/previews/wpf-demo-en.webp" alt="OCCT CAD WPF English demo" width="100%"></td>
  </tr>
</table>

## Features

- Dedicated WinForms and WPF OCCT viewport hosts
- Point, rectangle, directional crossing, multi-selection, and subshape selection
- Viewport-state snapshots, camera persistence, Z-up views, selected-object fitting, and screen-to-plane projection
- Batch color, transparency, visibility, display mode, line width, material, redisplay, and selection operations
- Exact line, circle, ellipse, plane, cylinder, cone, sphere, and torus parameter queries
- Exact edge parameter ranges, first/second derivatives, tangent, normal, curvature, and center of curvature
- Surface periodicity, first/second partial derivatives, oriented normals, principal/mean/Gaussian curvature, principal directions, and umbilic state
- Configurable selected and hover highlight colors
- Solid or gradient backgrounds, MSAA, render resolution, shadows, ray tracing, and multi-light presets
- Curves, primitive solids, Boolean operations, features, transforms, topology queries, mesh access, and analysis
- Complex gear, multi-port manifold, and twisted-duct scenarios
- BRep vector text plus length, angle, radius, and diameter annotations
- STEP, IGES, BREP, and STL exchange
- English and Simplified Chinese UI

Complex scenarios use display batching and remove profiles, cutters, paths, and construction geometry after completion so only final results remain in the scene.

## Compatibility

- OCCT: exactly `7.9.0`
- .NET: `8.0`, Windows x64
- Bridge version: `2.5.0`
- Bridge ABI: `2`
- API count: Native `327`, P/Invoke `327`
- Viewer and interaction APIs: `209`
- Modeling APIs: `118`
- Public .NET types: `75`
- Deploy `OcctNet.dll`, the selected UI host assembly, and `OcctNative.dll` from the same build
- A native session owns mutable state and must be used from one application thread at a time

## Build and run

Set the OCCT SDK location once or pass it explicitly:

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
```

The managed-only path does not require an OCCT SDK:

```powershell
.\build.ps1 managed Release
```

Validation covers the bridge version, API organization, analytic geometry, differential geometry, native declarations and implementations, Cdecl and exact symbol spelling, selection, viewport hosts, native source boundaries, and deployment package contract.

Native compilation and runtime smoke testing:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Publish

The default command publishes both WinForms and WPF as self-contained Windows x64 applications. Target computers do not need a separate .NET installation.

The package is deployment-complete: each executable embeds its .NET runtime, while `runtime` contains `OcctNative.dll`, the recursively resolved OCCT/third-party/Visual C++ DLL closure, and `occt/src` contains the required OCCT resources. Publishing fails when a required native dependency or OCCT resource is missing. `package-contract.json` and `native-dependencies.txt` describe the generated package.

```powershell
.\publish.ps1 -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Publish only one application:

```powershell
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Create a smaller framework-dependent package only for computers that already have the .NET 8 Desktop Runtime:

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

`dotnet publish` includes `OcctNet`, `OcctNet.WinForms`, and `OcctNet.Wpf` through project references. `publish.ps1` then adds the complete native dependency closure and required OCCT resources. Enable `-FullResources` or `-Diagnostics` only when needed.
