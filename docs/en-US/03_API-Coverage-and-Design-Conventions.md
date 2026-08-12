# API Coverage and Design Conventions

The `avalonia` source contract is:

```text
Native exports:     350
P/Invoke mappings:  350
Public .NET types:  109
Viewer API:         216
Modeling API:       134
```

The additional Viewer ABI compared with `main` is the platform-neutral Native Surface initialization entry point.

Public managed assemblies:

```text
OcctNet
OcctNet.Avalonia
```

Rules remain: exact Native/PInvoke parity, Cdecl + ExactSpelling, bulk high-cardinality transfers, no UI framework dependency in Core, explicit ownership/identity, no application Document/Command/Tool framework inside the Bridge.

`tools/OcctApiDocsGenerator` discovers only projects that actually exist on the branch.