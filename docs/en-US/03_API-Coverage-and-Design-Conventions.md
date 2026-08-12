# API Coverage and Design Conventions

The `main` source contract is:

```text
Native exports:     349
P/Invoke mappings:  349
Public .NET types:  113
Viewer API:         215
Modeling API:       134
```

The exact values are stored in `bridge-contract.json` and validated by repository checks.

Public managed assemblies on `main` are `OcctNet`, `OcctNet.WinForms`, and `OcctNet.Wpf`. Avalonia public types are deliberately excluded from this branch and are counted independently by the `avalonia` contract.

Design rules:

- keep Native declarations, definitions and P/Invoke names one-to-one;
- use Cdecl + ExactSpelling for Native exports;
- use bulk transfer for high-cardinality collections instead of N+1 interop loops;
- keep UI frameworks out of `OcctNet`;
- keep ownership and object identity explicit;
- do not reintroduce application-layer Document/Command/Tool abstractions into the Bridge.

`tools/OcctApiDocsGenerator` discovers the public assemblies that actually exist on the current branch instead of hard-coding a shared UI-host list.