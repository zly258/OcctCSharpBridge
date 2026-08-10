# OcctDemo.Common

`OcctDemo.Common` is demo-only application support shared by the WinForms, WPF and Avalonia sample programs. It is **not** part of the reusable Bridge SDK and is intentionally non-packable.

## Current baseline

```text
Author: Liaoyuan Zhang
Demo / Bridge: 2.6.0
Native ABI: 4
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0-windows
C#: 14.0
Avalonia: 12.1.0
Platform: Windows x64
```

`DemoProductInfo.cs` is the single metadata source used by all three About dialogs. The author is always displayed as **Liaoyuan Zhang**, including Chinese UI mode.

The project contains only the small orchestration layer required by the demos: command metadata, parameter parsing, localization, replay-based history, analysis helpers, product metadata and the lightweight `DemoSession` facade.

The reusable boundary remains the Binary SDK in `dist/win-x64`: `OcctNet`, `OcctNet.WinForms`, `OcctNet.Wpf`, `OcctNet.Avalonia` and `OcctNative`. Product-level document models, feature trees, tools, snapping, grips, persistence and domain entities belong in consuming CAD/BIM applications rather than the Bridge SDK.
