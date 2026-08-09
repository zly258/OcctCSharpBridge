# OcctDemo.Common

`OcctDemo.Common` is demo-only application support code used by the WinForms, WPF and Avalonia sample programs.

It intentionally is **not** a reusable CAD framework and is not part of the `main` Bridge SDK. It contains only the small orchestration layer needed by the demos: command metadata, parameter parsing, localization, replay-based demo history, analysis helpers and the demo session facade.

Product-level document models, feature trees, tools, snapping, grips, persistence and domain entities belong in consuming CAD/BIM applications rather than `OcctCSharpBridge`.
