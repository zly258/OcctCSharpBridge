# 02 Architecture and Boundaries

OcctCSharpBridge is intentionally layered:

```text
CAD / BIM Application
        ↓
OcctNet + UI Hosts
        ↓
Stable C ABI
        ↓
Open CASCADE Technology 7.9.0
```

## Native layer

`src/OcctNative` is C++17 code that owns the OCCT integration. It exposes a stable `extern "C"` ABI and does not leak OCCT C++ handles, templates, exceptions or STL containers across the boundary. Collection-heavy operations use bulk buffers instead of repeated `Count + At` calls.

## Managed core

`OcctNet` turns the C ABI into strongly typed .NET APIs. `OcctEngine` owns viewer/AIS interaction; `OcctModelingSession` is the headless modeling facade. Handles are owner-aware: an object ID is valid only within the engine or modeling session that created it.

## UI hosts

`OcctNet.WinForms`, `OcctNet.Wpf` and `OcctNet.Avalonia` adapt native viewer behavior to each UI framework. They may handle HWND lifecycle, resize, mouse capture, pan/rotate/zoom and selection policies, but they do not implement product Document, Command or Feature Tree systems.

## Lifetime and threading

Native root objects are owned through SafeHandle-based managed lifetime. Public calls reject disposed instances. A single `OcctEngine` or `OcctModelingSession` is not documented as generally thread-safe; callers serialize operations on the same instance. Viewer instances remain tied to their creating HWND/UI thread.

## Explicit exclusions

`main` does not contain OCAF/XDE, application Document managers, feature trees, undo/redo, snapping, grips, JSON project persistence, Ribbon/PropertyGrid business UI, or domain-specific BIM/equipment rules.

## Repository roles

- `main`: source, tests, technical documentation, NuGet packages and the validated Binary SDK.
- `demo`: application examples that consume `dist/win-x64`; no Bridge source mirror.
- other applications such as OCStation: consume the Binary SDK rather than cloning or building Bridge source.

No GitHub Actions workflow is used for build or branch synchronization. Native validation stays in the real local Windows/MSVC/OCCT environment.