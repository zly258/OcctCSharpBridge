# Architecture and Boundaries

`main` keeps the reusable Bridge separate from product-level CAD/BIM architecture.

```text
Application
   │
   ├─ OcctNet.WinForms ─┐
   ├─ OcctNet.Wpf ──────┤
   └─ OcctNet.Avalonia ─┤
                         ▼
                      OcctNet
                         │
                     ABI5 C API
                         │
                     OcctNative
                         │
                      OCCT 7.9
```

`OcctNet` owns strongly typed managed semantics. `OcctNative` owns the C++/OCCT boundary. UI adapters depend on Core and never depend on each other.

Application documents, feature trees, commands/tools, undo/redo, snapping, grips and project persistence remain application responsibilities.

XDE may be used internally for STEP assembly/product structure and presentation metadata, but OCAF/XDE is not the consuming application's document architecture.

`main` owns the formal WinForms/WPF/Avalonia adapters and the shared Windows/Linux Native Core. The unified `demo` branch consumes generated Binary SDK artifacts: Windows x64 provides WinForms, WPF and Avalonia hosts; Linux x64 provides Avalonia only. Demo does not carry a private Bridge implementation.

## Domain layout

Managed and Native source trees use the same **domain-level ownership model** instead of a flat source directory or a file-for-file mirror. Public managed namespaces remain `OcctNet`; folders express internal source ownership and do not create artificial public namespaces.

```text
src/OcctNet/                 src/OcctNative/
├─ Core/                     ├─ core/
├─ Exchange/                 ├─ exchange/
├─ Geometry/                 ├─ geometry/
├─ Mesh/                     ├─ mesh/
├─ Modeling/                 ├─ modeling/
├─ Platform/                 ├─ platform/
├─ Presentation/             ├─ presentation/
├─ Scene/                    ├─ scene/
├─ Selection/                ├─ selection/
└─ Topology/                 └─ topology/
                              └─ viewer/
```

Managed P/Invoke declarations live below the owning domain's `Interop/` directory. Generic root-level `NativeMethods.*` dumping grounds, migration names such as `*.Current.*`, and compatibility/legacy source names are not part of the ABI5 architecture.

The Managed root keeps only project-level files such as `AssemblyInfo.cs`, `GlobalUsings.cs` and `OcctNet.csproj`. The Native root keeps only build/ABI umbrella files such as `CMakeLists.txt`, `OcctNative.h` and `OcctStatus.h`; domain implementations belong in domain directories.

## Native boundaries

Native operating-system integration is confined to `src/OcctNative/platform`. Window-system types and calls must not enter the public ABI or other native domains.

Exchange implementation, STEP/XCAF document state, and import source metadata belong to `src/OcctNative/exchange`. Triangulation extraction belongs to `src/OcctNative/mesh`. Neither domain owns Viewer state; headless Modeling reaches them through the shared Modeling Session.

Selection state, detection, selection overlays, and manipulators belong to `src/OcctNative/selection`. Visual appearance, overlays, annotations and custom presentations belong to `src/OcctNative/presentation`; both use Viewer services without taking ownership of the Viewer context.

Engine coordination and structured native error state belong to `src/OcctNative/core`. Core composes ViewerContext, SceneRegistry, DocumentStore and rendering state; it does not own UI-framework adapters or application-level document and command models.

Shared geometric queries, point collections, analytic and differential geometry, planar construction, transforms, and B-Spline implementation belong to `src/OcctNative/geometry`. Viewer-facing entry points may use Engine coordination, while headless entry points use only ModelingSession state.

Shape traversal, topology queries, topology history, face analysis, and persistent topology references belong to `src/OcctNative/topology`. They operate on ModelingSession-owned shapes and remain independent of Viewer and AIS state.

ModelingSession lifecycle, algorithm execution, Boolean/features/healing, analysis, inertia, and intersection implementation belong to `src/OcctNative/modeling`. Recursive contract checks reject Viewer/Core/AIS dependencies in headless Modeling sources. Engine/Modeling presentation integration is implemented through the Scene interop boundary rather than as part of Modeling Core.
