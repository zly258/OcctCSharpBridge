# Getting Started

OcctCSharpBridge 3.0 Stable officially distributes **Windows x64** prebuilt SDK assets. Linux x64 remains a maintained source-build platform for Core and Avalonia.

## 1. Current contract

- Bridge: `3.0.0`
- Native ABI: ABI 5 only
- OCCT: 7.9.0 exact
- Binary SDK minimum TFMs: `net8.0` / `net8.0-windows`
- supported consumers: .NET 8 / 9 / 10
- WinForms/WPF: Windows
- Avalonia: Windows; Linux source build

## 2. Install the Windows SDK

Bridge development and application consumption are separate operations.

Build Bridge source without touching the installed SDK:

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
```

Install/update the validated shared SDK:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Default location:

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
  OcctNative.dll
  OcctNet.dll
  OcctNet.WinForms.dll
  OcctNet.Wpf.dll
  OcctNet.Avalonia.dll
  bridge-contract.json
  bridge-manifest.json
```

Use `OCCTCSHARPBRIDGE_SDK` to override the SDK root on a development/build machine. Consumer repositories should not keep synchronized Windows SDK copies.

## 3. Managed references

Define one SDK root:

```xml
<PropertyGroup>
  <OcctBridgeSdkRoot Condition="'$(OCCTCSHARPBRIDGE_SDK)' != ''">$(OCCTCSHARPBRIDGE_SDK)</OcctBridgeSdkRoot>
  <OcctBridgeSdkRoot Condition="'$(OcctBridgeSdkRoot)' == ''">$(ProgramFiles)\OcctCSharpBridge\SDK\3.0\win-x64</OcctBridgeSdkRoot>
</PropertyGroup>
```

Then reference only the assemblies the application uses:

```xml
<ItemGroup>
  <Reference Include="OcctNet">
    <HintPath>$(OcctBridgeSdkRoot)\OcctNet.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

Add `OcctNet.WinForms.dll`, `OcctNet.Wpf.dll`, or `OcctNet.Avalonia.dll` as required.

## 4. Runtime initialization

Configure the runtime before the first native-backed operation:

```csharp
using OcctNet;

OcctRuntime.Configure();

using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 10);
model.ExportStep(box, "box.step");
```

## 5. WPF/Avalonia first frame

Add the viewport to the normal visual tree. If startup work depends on the first native frame already being presented, wait for `HostState == Ready`.

`Ready` is reached only after the viewport has a usable arranged size and the Bridge completes `ResizeSurface + Redraw`. Do not simulate mouse motion or add arbitrary startup delays/redraw calls.

## 6. Linux

Linux consumers build from source:

```bash
./build.sh build Release
```

## Next steps

- [Third-party SDK Consumption](09_Third-Party-SDK-Consumption.md)
- [Stable Support and Compatibility](10_Stable-Support-and-Compatibility.md)
- [Runtime Deployment and Diagnostics](07_Runtime-Deployment-and-Diagnostics.md)
- [Build and Publish](08_Build-Test-and-Publish.md)
