# OcctCSharpBridge Complete API Reference

This directory contains the complete English **Managed + Native** API reference for OcctCSharpBridge.

## Contract

```text
Author: zly258
Bridge: 2.6.0
Native ABI: 4
Native exports: 344
Managed P/Invoke mappings: 344
Public .NET types: 105
OCCT: 7.9.0
.NET SDK: 10.0.302
Target: net10.0-windows
C#: 14.0
Native Bridge: C++17
Avalonia: 12.1.0
Platform: Windows x64
```

Generate or refresh the reference with:

```powershell
.\build.ps1 docs Release
```

`tools/OcctApiDocsGenerator` generates both layers:

```text
api/reference/**     one page per exported managed type
api/native-abi.md    complete Native C ABI reference
```

Managed coverage:

- `OcctNet.dll`
- `OcctNet.WinForms.dll`
- `OcctNet.Wpf.dll`
- `OcctNet.Avalonia.dll`

For every public managed type the generator records assembly, namespace, declaration, inheritance, constructors, properties, events, methods, parameters, returns, exceptions, remarks and public fields/enum values when available from reflection/XML Documentation.

The Native reference is generated from `src/OcctNative/OcctNative.h` and covers the ABI types and all `344` `OCCTBRIDGE_API occt_*` exports. Generation fails when the public .NET type count or Native export count differs from `bridge-contract.json`.

For ownership, lifetime, threading, runtime and architectural semantics, use the conceptual chapters in `docs/en-US`; use this generated reference for exact public signatures and ABI declarations.
