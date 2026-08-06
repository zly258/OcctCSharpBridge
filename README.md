# OcctCSharpBridge

[简体中文](README.zh-CN.md) · [WinForms/WPF demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

A Windows x64 bridge from Open CASCADE Technology 7.9.0 to .NET 8. The reusable `main` branch contains only the native C ABI, managed wrapper, API checks, and interface inventory. WinForms and WPF applications are maintained on the `demo` branch.

## Structure

```text
src/OcctNative   C++17 native bridge and stable C ABI
src/OcctNet      Type-safe .NET wrapper
tests            API consistency and native smoke scenarios
docs             English and Chinese API inventories
```

The wrapper provides three session types:

- `OcctEngine`: HWND viewer, AIS objects, selection, camera, display attributes, text, and dimensions.
- `OcctModelingSession`: headless geometry, topology, algorithms, mesh, analysis, healing, and exchange.
- `OcafDocument`: OCAF/TNaming/XDE documents, assemblies, metadata, persistence, and undo/redo.

## Compatibility contract

- Required OCCT version: exactly `7.9.0`.
- Managed target: `.NET 8`, Windows x64.
- Native bridge ABI: validated at runtime through `OcctBridgeInfo`.
- Deploy `OcctNet.dll` and `OcctNative.dll` from the same build.
- Native OCCT and third-party DLLs must be discoverable through the application directory or configured runtime paths.

## Build

```powershell
# Validate declarations, definitions, P/Invoke calling conventions, and API inventories.
.\build.ps1 validate Release

# Build the reusable managed wrapper.
.\build.ps1 managed Release

# Build native and managed components.
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"

# Build and run native modeling/OCAF smoke scenarios.
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

## API inventory

- [English interface inventory](docs/API_COVERAGE.md)
- [中文接口清单](docs/API_COVERAGE.zh-CN.md)

`build.ps1 validate` fails when declarations, P/Invoke mappings, calling conventions, or inventory counts are stale. A scheduled workflow also verifies that reusable wrapper files remain identical between `main` and `demo`.

## License

The project is provided under the [PolyForm Noncommercial License 1.0.0](LICENSE). OCCT and third-party components remain subject to their own licenses.
