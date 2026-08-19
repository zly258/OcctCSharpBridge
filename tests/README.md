# Tests and Validation

Bridge 3.0 keeps validation in distinct layers so fast SDK consumers do not accidentally execute the expensive release gate.

## Runtime policy

Routine regression and smoke execution defaults to **.NET 10**:

```text
OcctNet.ManagedTests       net10.0
OcctNet.Smoke              net10.0
OcctNet.AvaloniaSmoke      net10.0
OcctNet.WinFormsSmoke      net10.0-windows
OcctNet.WpfSmoke           net10.0-windows
```

Published managed assemblies intentionally remain on the minimum compatibility TFMs:

```text
OcctNet / Avalonia         net8.0
WinForms / WPF             net8.0-windows
```

Compatibility with .NET 8 and .NET 9 is not inferred from the default .NET 10 smoke. It is covered explicitly by the Consumer Matrix and, for Stable release validation, by Native Runtime Smoke executed on actual .NET 8, 9 and 10 runtimes.

## Validation layers

1. **Repository/static contracts** — architecture, version, ABI5-only, API binding parity, build inventory, consumer matrix metadata and repository hygiene.
2. **Consumer compilation matrix** — verifies the net8-based SDK can be referenced by .NET 8/9/10 consumers.
3. **Managed regression tests** — default .NET 10 managed semantics/lifecycle validation.
4. **Core Native Smoke** — default .NET 10 real OCCT modeling/exchange/native lifecycle execution.
5. **Viewport Host Smoke** — default .NET 10 WinForms/WPF/Avalonia host lifecycle/render validation.
6. **Stable Runtime Matrix / Portable Smoke** — native execution on actual .NET 8/9/10 runtimes plus isolated execution from the extracted Portable ZIP.

Bridge 3 supports ABI 5 only; no ABI4 compatibility line is maintained.

## Static contracts

Windows:

```powershell
.\build.ps1 validate Release
```

Linux:

```bash
./build.sh validate Release
```

Checks include version/platform/TFM policy, architecture boundaries, ABI5-only rules, bulk ABI rules, native build structure, API surface parity, Consumer Matrix metadata and repository hygiene.

## Consumer Matrix

```text
OcctNet.ConsumerMatrix
  net8.0
  net9.0
  net10.0

OcctNet.DesktopConsumerMatrix
  net8.0-windows
  net9.0-windows
  net10.0-windows
```

Run:

```powershell
.\build.ps1 consumer Release
```

This proves compile-time compatibility only.

## Managed regression tests

```powershell
.\build.ps1 test Release
```

They run on .NET 10 by default. Managed warnings/analyzer diagnostics are treated as errors.

## Core Native Smoke

Windows:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux:

```bash
./build.sh smoke Release
```

The smoke runs on .NET 10 by default and uses the real ABI5 Native Bridge and OCCT 7.9.0.

## Viewport Host Smoke

Windows:

```powershell
.\build.ps1 viewport-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Projects:

```text
OcctNet.WinFormsSmoke      net10.0-windows
OcctNet.WpfSmoke           net10.0-windows
OcctNet.AvaloniaSmoke      net10.0
```

Linux Avalonia:

```bash
./build.sh avalonia-smoke Release
```

## Stable Runtime Matrix

`tests/OcctNet.RuntimeSmoke` multi-targets:

```text
net8.0
net9.0
net10.0
```

Formal Stable publishing requires actual Microsoft.NETCore.App 8.x, 9.x and 10.x x64 runtimes and runs the same lightweight native scenario under each runtime. The process checks `Environment.Version.Major`, so a missing older runtime cannot be hidden by rolling to .NET 10.

## Isolated Portable Smoke

Formal Stable publishing extracts the Windows Portable ZIP outside the repository, clears Bridge/OCCT runtime variables and removes development repository/OCCT paths from `PATH`, then runs native smoke using only the extracted SDK payload.

## Recommended commands

```powershell
# static validation
.\build.ps1 validate Release

# complete local Windows validation, default execution on .NET 10
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"

# formal Stable publish + full Stable Runtime Matrix + Portable isolation smoke
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Linux source line:

```bash
./build.sh validate Release
./build.sh all Release
./build.sh avalonia-smoke Release   # graphical environment only
```

Official 3.x prebuilt artifacts are Windows x64 only.
