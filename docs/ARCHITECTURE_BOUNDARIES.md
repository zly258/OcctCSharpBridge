# Architecture Boundaries: Bridge vs CAD Application

The `main` branch of OcctCSharpBridge is limited to the **Open CASCADE Technology 7.9.0 native/.NET bridge and reusable viewport hosts**. CAD document models, commands, interactive tools, application entities, undo/redo, persistence, and product UI belong to `demo` or another application layer.

## What belongs in `main`

- `OcctNative`: C++17 OCCT bridge and stable C ABI.
- `OcctNet`: strongly typed `OcctEngine`, `OcctModelingSession`, value types, runtime loading, and diagnostics.
- `OcctNet.WinForms`: reusable WinForms HWND viewport host.
- `OcctNet.Wpf`: reusable WPF viewport host through `WindowsFormsHost`.
- `OcctNet.Avalonia`: reusable Avalonia host through `NativeControlHost` and a Windows HWND child.
- Contract checks, managed regression tests, native smoke scenarios, and NuGet packaging policy.

A UI host connects a framework window and pointer events to `OcctEngine`; it is not a CAD application framework.

## What does not belong in `main`

The reusable bridge must not own application concepts such as:

- Document / DocumentManager;
- business Entity or Feature Tree models;
- Command / CommandBus / CommandRegistry;
- Tool / ToolManager / Grip / Snap rules;
- application-level undo/redo transactions;
- JSON project persistence;
- Ribbon, property panels, recent-file lists, workspaces, or other product UI;
- BIM/equipment discipline properties and business rules.

A full CAD product may need these concepts, but they depend on a specific product data model and interaction design. Moving them into the bridge would couple OCCT integration to one application architecture and make the wrapper less reusable.

## What belongs in `demo`

The `demo` branch builds a reference CAD application layer on top of the same reusable bridge source. It may contain:

- `CadCommon`: simple command catalog/dispatch, parameter parsing, history, and shared demo logic;
- `CadWinForms`, `CadWpf`, and `CadAvalonia`: complete runnable examples;
- demo run/publish scripts, native-runtime deployment, and application package validation.

Document, Command, Tool, Undo/Redo, and similar CAD patterns may be demonstrated there, but those implementations are reference application code rather than `OcctNet` public API.

## Layer model

```text
CAD / BIM Application                     demo or external project
├─ Document / Feature / Entity
├─ Command / Tool / Snap / Grip
├─ Undo / Redo / Persistence
└─ Product UI
              │
              ▼
Reusable .NET Bridge                      main
├─ OcctEngine
├─ OcctModelingSession
├─ WinForms / WPF / Avalonia Viewport Host
└─ Runtime / Diagnostics / Value Types
              │
              ▼
Stable C ABI                              main
              │
              ▼
Open CASCADE Technology 7.9.0
```

## `OcctEngine` and `OcctModelingSession`

Some construction operations intentionally exist on both façades because they serve different lifecycles:

- `OcctEngine` creates/manages objects participating in an initialized AIS viewer.
- `OcctModelingSession` provides headless modeling, batch processing, analysis, and exchange.

Do not combine them merely to remove superficial API duplication.

## Shared UI-host boundary

WinForms, WPF, and Avalonia have different window lifetime and input-capture behavior, so the bridge does not introduce a universal UI base class. Only host-neutral interaction decisions are shared, such as hover/world-point throttling, rectangle threshold/direction rules, drag-end recovery, and default zoom factors.

Window creation, DPI handling, mouse capture, and Win32 subclassing remain inside their respective hosts. This removes meaningful duplication without creating a fragile cross-framework abstraction.

## Avalonia scope

`OcctNet.Avalonia` is a supported reusable host, but it is currently a **Windows x64 + HWND** adapter. Its presence does not imply Linux/macOS support. Cross-platform OCCT viewer hosting would require separate native window/graphics backends and is outside the current bridge contract.

## Compatibility policy

Bridge 2.x does not break existing public API merely for internal cleanup:

- existing Native ABI 3 signatures stay unchanged;
- Bridge 2.5 compatibility entry points such as `OcctObject` remain available during 2.x but receive no new legacy surface;
- new code should prefer owner-aware object APIs;
- cpp/header reorganization and UI-host policy sharing do not constitute ABI changes.

Removal of compatibility surface belongs in an explicit future major version.
