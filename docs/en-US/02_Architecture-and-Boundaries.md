# Architecture and Boundaries

The `avalonia` branch intentionally has only two managed public layers:

```text
Application
    │
OcctNet.Avalonia
    │
  OcctNet
    │
stable C ABI
    │
OcctNative
 ├─ Windows: WNT_Window
 └─ Linux:   Xw_Window
```

`OcctNet` remains UI-framework independent. `OcctNet.Avalonia` depends directly on Core and does not reference WinForms or WPF.

The public host is platform-neutral. Native window-system differences stay behind the adapter/native-surface boundary.

Application Document, Feature Tree, Command/Tool, Undo/Redo, snapping, grips and persistence remain above the Bridge.

Native Wayland hosting is a future backend concern; it must not require a new public viewport type.