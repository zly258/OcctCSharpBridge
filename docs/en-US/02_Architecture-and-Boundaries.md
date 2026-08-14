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
                    stable C ABI
                         │
                     OcctNative
                         │
                      OCCT 7.9
```

`OcctNet` owns strongly typed managed semantics. `OcctNative` owns the C++/OCCT boundary. UI adapters depend on Core and never depend on each other.

Application documents, feature trees, commands/tools, undo/redo, snapping, grips and project persistence remain application responsibilities.

XDE may be used internally for STEP assembly/product structure and presentation metadata, but OCAF/XDE is not the consuming application's document architecture.

`main` owns the formal Avalonia host and the shared Windows/Linux Native Core. The `demo` and `avalonia` branches consume published SDK artifacts and do not carry private Bridge implementations.

Native operating-system integration is confined to `src/OcctNative/platform`. Window-system types and calls (Win32 or X11) must not enter the public ABI or other native domains. Contract checks scan native sources recursively and enforce this boundary as the remaining sources are moved into their domain directories.

Exchange implementation, STEP/XCAF document state, and import source metadata belong to `src/OcctNative/exchange`. Triangulation extraction belongs to `src/OcctNative/mesh`. Neither domain owns Viewer state; headless Modeling reaches them through the shared Modeling Session.
