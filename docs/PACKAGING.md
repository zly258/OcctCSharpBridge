# Packaging and Runtime Deployment

OcctCSharpBridge intentionally separates the **managed SDK** from the **native OCCT runtime**. `main` packages only reusable bridge/host assemblies; complete CAD application publishing belongs to `demo`.

## Managed packages

```powershell
.\build.ps1 pack Release
```

Four SDK packages are produced:

```text
artifacts/packages/
├─ OcctNet.<version>.nupkg
├─ OcctNet.<version>.snupkg
├─ OcctNet.WinForms.<version>.nupkg
├─ OcctNet.WinForms.<version>.snupkg
├─ OcctNet.Wpf.<version>.nupkg
├─ OcctNet.Wpf.<version>.snupkg
├─ OcctNet.Avalonia.<version>.nupkg
└─ OcctNet.Avalonia.<version>.snupkg
```

The version comes from `bridge-contract.json`.

## Package responsibilities

- `OcctNet`: core bridge API with no WinForms/WPF/Avalonia dependency.
- `OcctNet.WinForms`: WinForms HWND host.
- `OcctNet.Wpf`: WPF host.
- `OcctNet.Avalonia`: Avalonia + Windows HWND host.

The Avalonia package is currently Windows-only and does not imply a cross-platform native viewer.

## Managed package contents

Included:

- managed assemblies;
- XML IntelliSense documentation;
- NuGet dependency relationships;
- README and LICENSE;
- portable PDB/symbol packages.

Explicitly excluded:

- `OcctNative.dll`;
- OCCT `TK*.dll` libraries;
- third-party OCCT runtime DLLs;
- OCCT resource directories;
- CadCommon or complete CAD application code.

`tests/check-sdk-package.ps1` and `build.ps1 pack` enforce these boundaries.

## Why native runtime deployment stays separate

The OCCT runtime depends on the exact OCCT build, MSVC runtime, third-party dependency closure, and license obligations. Managed NuGet packages therefore do not pretend to be self-contained OCCT redistributions. Applications deploy the `OcctNative.dll` and OCCT runtime matching Bridge 2.6 / ABI 3 explicitly.

## Runtime resolution and diagnostics

Published applications should prefer app-local runtime deployment. Explicit configuration remains available:

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\runtime\occt-7.9.0",
    nativeBridgeDirectory: @"D:\runtime\bridge");
```

Diagnostics:

```csharp
var info = OcctRuntime.GetDiagnosticInfo();
var report = OcctRuntime.GetDiagnosticReport();
```

## Application publishing

Complete desktop publishing belongs to `demo`, because only the application layer knows the actual executables, CadCommon, resources, and app-local native dependency closure.

On a Windows machine with OCCT 7.9.0, run the native release gate first:

```powershell
.\build.ps1 smoke Release
```

If OCCT is outside the conventional `D:\tools\occt-vc144-64` root:

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

Demo publishing:

```powershell
.\publish.ps1 all Release -Zip
```

## Local NuGet feed

After `build.ps1 pack`, add `artifacts/packages` as a local NuGet source. The consuming application still deploys the matching native runtime.

Do not publish to a public package feed until native-runtime distribution, license review, and the formal release process are intentionally defined.
