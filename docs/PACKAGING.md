# Packaging and Runtime Deployment

OcctCSharpBridge intentionally separates **managed SDK packages** from the **native OCCT runtime**. This prevents a local development package from pretending to be a fully self-contained OCCT distribution.

## Managed packages

Create the managed packages with:

```powershell
.\build.ps1 pack Release
```

Output:

```text
artifacts/packages/
├─ OcctNet.<version>.nupkg
├─ OcctNet.<version>.snupkg
├─ OcctNet.WinForms.<version>.nupkg
├─ OcctNet.WinForms.<version>.snupkg
├─ OcctNet.Wpf.<version>.nupkg
└─ OcctNet.Wpf.<version>.snupkg
```

The version is always injected from `bridge-contract.json`.

## What the managed packages contain

The packages contain:

- managed assemblies;
- XML documentation for IntelliSense;
- package dependency relationships;
- README and license metadata;
- portable PDB/symbol packages.

They do **not** contain:

- `OcctNative.dll`;
- OCCT `TK*.dll` runtime libraries;
- third-party OCCT runtime DLLs;
- OCCT resource directories.

## Why native binaries are separate

OCCT runtime deployment depends on the exact OCCT build, compiler runtime, optional third-party dependencies, and license obligations. The Bridge therefore treats native deployment as an explicit application responsibility rather than hiding it inside a managed NuGet package.

## Runtime resolution order

Published applications should prefer app-local deployment. `OcctRuntime` probes the application directory before development/repository locations. Explicit configuration is available when the runtime lives elsewhere:

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\runtime\occt-7.9.0",
    nativeBridgeDirectory: @"D:\runtime\bridge");
```

For diagnostics:

```csharp
var report = OcctRuntime.GetDiagnosticReport();
```

## Application publishing

The `demo` branch owns the complete desktop publishing workflow because it knows the concrete application executables and can build an app-local native dependency closure.

For release testing on a machine with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

For the three demo hosts:

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

## Local NuGet feed example

After running `build.ps1 pack`, add `artifacts/packages` as a local NuGet source and reference the packages normally. The consuming application must still deploy the matching Bridge 2.6 / ABI 3 native runtime.

Do not publish the packages to a public package feed until the native runtime distribution and release process are intentionally defined.
