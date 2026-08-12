# 02 Architecture and Boundaries

## Layers

```text
Application
  Document / Feature Tree / Command / Tool / Undo-Redo / Persistence
       │
       ▼
UI host adapters
  OcctNet.WinForms / OcctNet.Wpf / OcctNet.Avalonia
       │
       ▼
OcctNet
  Viewer + modeling managed API
       │ P/Invoke / stable C ABI 4
       ▼
OcctNative (C++17)
       │
       ▼
Open CASCADE Technology 7.9.0
```

## Ownership

`OcctModelingSession` owns headless OCCT shapes and algorithms. `OcctEngine` owns AIS/viewer presentations, selection and scene state. Handles are session/engine scoped and must not be treated as application entity IDs.

Use `ApplicationTag` or your own domain ID mapping to associate application entities with viewer objects.

## OCAF/XDE boundary

OcctCSharpBridge does not prescribe OCAF/XDE as the consuming application's document architecture. Product documents, command history, feature trees and project persistence remain application concerns.

The Bridge **does** use XDE internally for STEP assembly exchange because STEP product structure, occurrences, colors, visibility and subshape styles are XDE responsibilities in OCCT. The managed projection is `OcctAssemblyDocument`, not an exposed `TDocStd_Document`.

## Branch boundary

- `main`: Bridge producer and tracked Binary SDK.
- `demo`: UI applications consuming a local ignored SDK copy.
- `website`: static documentation/marketing site.

This separation avoids duplicated Bridge source and stale binary payloads.
