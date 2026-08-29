# Getting Started

OcctCSharpBridge 3.0 Stable officially distributes **Windows x64** prebuilt SDK assets. Linux x64 remains a maintained source-build platform for Core and Avalonia, but official 3.x Releases do not provide Linux prebuilt SDK assets.

## 1. Current contract

- Bridge: `3.0.0`
- Native ABI: ABI 5 only
- OCCT: 7.9.0 exact
- official Windows prebuilt platform: x64
- Linux: source-build support
- Binary SDK minimum TFMs: `net8.0` / `net8.0-windows`
- supported consumers: .NET 8 / 9 / 10
- WinForms/WPF: Windows
- Avalonia: Windows; Linux source build

Bridge source uses a stable .NET 10 SDK with baseline `10.0.100` and `rollForward=latestFeature`.

## 2. Windows artifacts

### Binary SDK

Use for compile-time references and contract/manifest/hash validation:

```text
dist/win-x64/
  OcctNative.dll
  OcctNet.dll
  OcctNet.WinForms.dll
  OcctNet.Wpf.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
```

### Portable SDK

Use for formal Windows deployment:

```text
OcctCSharpBridge-<version>-win-x64-portable/
  OcctNet*.dll
  runtime/
  occt/resources/
  bridge-contract.json
  bridge-manifest.json
  package-manifest.json
  licenses / notices
```

Formal third-party applications should prefer the Windows Release asset produced from `main`. See [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md).

## 3. Managed references

Headless/Core:

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Add only the adapter used by the application:

```text
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
```

Consumer applications may target .NET 8, 9, or 10; a separate Bridge managed payload is not required for each runtime major.

## 4. Runtime initialization

With the Portable SDK layout:

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 10);
model.ExportStep(box, "box.step");
```

Call `OcctRuntime.Configure()` before the first `OcctEngine`, `OcctModelingSession`, or other native-backed operation.

Managed assemblies, `runtime/`, `occt/resources/`, and manifests must come from the same Bridge build/source commit.

## 5. No formal Release asset available

Generate a consumer SDK from an explicitly approved formal `main` commit:

```powershell
.\build.ps1 dist Release `
  -OcctRoot "D:\tools\occt-vc144-64"
```

Use the Windows Portable SDK packager when a runtime closure is also required. `dist` builds Native + Managed directly; `publish.ps1` packages the resulting SDK.

Bridge maintainers publish and validate a Stable candidate with:

```powershell
.\publish.ps1 `
  -OcctRoot "D:\tools\occt-vc144-64"
```

## 6. Linux

Linux consumers build from source:

```bash
./build.sh build Release
```

Build Linux native binaries against OCCT 7.9.0 and the C/C++ runtime environment appropriate for the target distribution.

## Next steps

- [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md)
- [Stable Support and Compatibility](10_Stable-Support-and-Compatibility.md)
- [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
- [Build, Test and Publish](08_Build-Test-and-Publish.md)
