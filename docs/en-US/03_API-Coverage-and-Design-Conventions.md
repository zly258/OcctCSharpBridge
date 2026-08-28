# API Coverage and Design Conventions

Bridge 3 treats the **current source tree and actual build result** as the source of truth. The repository does not maintain hand-written API counts or freeze implementation details through extra source-scanning policy scripts.

Current public managed assemblies:

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

Core principles:

- Native C ABI and managed interop use matching semantic entry points;
- Core Bridge P/Invoke uses source-generated `LibraryImport` with Cdecl;
- high-cardinality data prefers Snapshot/Buffer/Bulk ABI to N+1 interop;
- Bridge 3 currently supports ABI 5 only;
- `OcctModelingSession` owns headless modeling/topology resources;
- `OcctEngine` owns AIS/viewer presentation and interaction;
- `OcctNet` Core does not depend on UI frameworks;
- WinForms, WPF, and Avalonia remain host adapters only;
- application documents, feature trees, commands/tools, undo/redo, snapping, grips, and persistence remain application responsibilities.

Validation relies primarily on compilation and tests:

```powershell
.\build.ps1 build Release
.\build.ps1 test Release
.\build.ps1 smoke Release
```

No source-scanning policy target is maintained; compilation and tests are the correctness gates.
