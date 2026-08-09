# OcctDemo.Common

`OcctDemo.Common` is demo-only application support shared by the WinForms, WPF, and Avalonia sample programs.

It is intentionally **not** a reusable CAD framework and is not part of the `main` Bridge SDK. The project is non-packable and contains only the small orchestration layer required by the demos: command metadata, parameter parsing, localization, replay-based history, analysis helpers, and the lightweight `DemoSession` facade.

The reusable boundary remains `OcctNet` plus its WinForms, WPF, and Avalonia viewport hosts. Product-level document models, feature trees, tools, snapping, grips, persistence, and domain entities belong in consuming CAD/BIM applications rather than `OcctCSharpBridge`.
