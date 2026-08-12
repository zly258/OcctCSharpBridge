# Architecture and Boundaries

`main` keeps the reusable Bridge separate from product-level CAD/BIM architecture.

```text
Application
   │
   ├─ OcctNet.WinForms ─┐
   └─ OcctNet.Wpf ──────┤
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

Avalonia is isolated on the `avalonia` branch so the Windows-only `net10.0-windows` main contract does not constrain the cross-platform `net10.0` host.