# Getting Started

OcctCSharpBridge is distributed as a platform Binary SDK plus an optional Portable SDK runtime closure. Application teams normally consume the SDK as binaries; they do not need to build or vendor Bridge implementation source.

## 1. Choose the artifact

Use the **minimal Binary SDK** for compile-time references and controlled build automation:

```text
win-x64: OcctNative.dll + OcctNet*.dll + contract/manifest
linux-x64: libOcctNative.so + OcctNet.dll + OcctNet.Avalonia.dll + contract/manifest
```

Use the **Portable SDK** for application deployment. It adds the OCCT/native runtime closure and resources:

```text
runtime/
occt/resources/
package-manifest.json
licenses/notices
```

For formal third-party use, prefer a reviewed Portable SDK produced from `main`. For detailed integration, see [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md).

## 2. Compatibility

Current Bridge contract:

- Bridge `3.0.0-preview.1`
- ABI 5 only
- OCCT 7.9.0
- Windows x64 / Linux x64
- Binary SDK minimum TFM: .NET 8
- supported consumer runtimes: .NET 8, 9 and 10
- WinForms/WPF: Windows only
- Avalonia/Core: Windows and Linux managed surface

Bridge source is built with a stable .NET 10 SDK using baseline `10.0.100` and `latestFeature` roll-forward. An exact SDK patch such as `10.0.303` is not required.

## 3. Reference the managed assemblies

A headless/Core project needs only `OcctNet.dll`:

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)/OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Avalonia adds `OcctNet.Avalonia.dll`; WinForms adds `OcctNet.WinForms.dll`; WPF adds `OcctNet.Wpf.dll`. Do not reference UI adapters that the application does not use.

Example property:

```xml
<PropertyGroup>
  <OcctBridgeSdkRoot>$(MSBuildThisFileDirectory)external/OcctCSharpBridge/win-x64</OcctBridgeSdkRoot>
</PropertyGroup>
```

## 4. Configure the runtime

When using the Portable SDK layout, copy the managed assemblies plus `runtime/` and `occt/` into the final application package and configure Bridge before creating the first native-backed object:

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 10);
model.ExportStep(box, "box.step");
```

`OcctRuntime.Configure()` resolves the application-local runtime and OCCT resource layout. Do not mix the native library or runtime directory from one SDK with managed assemblies from another source commit.

## 5. Building Bridge from source

Bridge contributors or controlled internal consumers may generate a fresh Binary SDK without running the full release QA gate:

Windows:

```powershell
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux:

```bash
./build.sh dist Release
```

This is the **consumer artifact fast path**: Native + Managed + manifest/package construction, with no ManagedTests, consumer matrix, Core Smoke, or viewport/window smokes.

Bridge validation/publication remains separate:

```powershell
.\build.ps1 sdk Release -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

```bash
./build.sh all Release
./publish.sh
```

## Next steps

- External application integration: [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md)
- Runtime packaging and diagnostics: [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
- Bridge contributor workflow: [Build, Test and Publish](08_Build-Test-and-Publish.md)
- Architecture: [Architecture and Boundaries](02_Architecture-and-Boundaries.md)
