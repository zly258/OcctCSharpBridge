# API Coverage and Design Conventions

Bridge 3 treats the **current source tree** as the source of truth for the public API. The repository no longer maintains hand-written or generated API-count statistics and no longer generates per-type/per-function API reference pages.

Current public managed assemblies:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

Native C ABI and managed interop parity is checked directly from source by `tests/check-api-surface.ps1`:

- tracked Native Header declarations must match Native definitions one-to-one;
- `occt_*` bindings in `OcctNet` Core must match the Native ABI one-to-one;
- Core Bridge P/Invoke uses source-generated `LibraryImport` + Cdecl;
- `DllImport` is forbidden in Core;
- WinForms/WPF/Avalonia adapters may use host-platform interop such as Win32/X11, but must not bypass `OcctNet` by declaring `occt_*` Bridge ABI entries themselves;
- high-cardinality data uses Snapshot/Buffer/Bulk ABI instead of N+1 indexed interop;
- exported functions use semantic names rather than migration-version suffixes;
- Bridge 3 supports ABI 5 only and retains no ABI4 shim, retired handle, or compatibility entry point.

Design boundaries:

- `OcctModelingSession` owns headless modeling/topology resources;
- `OcctEngine` owns AIS/viewer presentation and interactive scene state;
- `OcctNet` Core does not depend on UI frameworks;
- WinForms, WPF, and Avalonia adapters do not reference each other;
- ownership and identity remain explicit; objects from different sessions/engines must not be mixed;
- application documents, feature trees, commands/tools, undo/redo, snapping, grips, and project persistence remain application responsibilities rather than Bridge public architecture.

To verify whether the public API changed, inspect the current source and run:

```powershell
.\build.ps1 validate Release
```

instead of relying on a generated reference that may have drifted from source.
