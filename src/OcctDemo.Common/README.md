# OcctDemo.Common

`OcctDemo.Common` is the demo-only scenario and orchestration layer shared with the WinForms/WPF demo baseline and consumed by `OcctDemo.Avalonia`. It is not part of the reusable Bridge SDK and remains intentionally non-packable.

## Current baseline

```text
Author: zly258
Bridge: 2.7.0
Native ABI: 4
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0
C#: 14.0
Avalonia: 12.1.0
Platforms: Windows x64 / Linux x64
```

The project contains command metadata, parameter parsing, localization, replay-based history, analysis helpers, product metadata and the lightweight `DemoSession` facade. UI-framework-specific controls, file pickers and dialogs belong in the host demo project rather than this common layer.

The reusable boundary remains `OcctNet`, `OcctNet.Avalonia` and `OcctNative`. Product-level document models, feature trees, tools, snapping, grips, persistence and domain entities belong in consuming CAD/BIM applications rather than the Bridge SDK.
