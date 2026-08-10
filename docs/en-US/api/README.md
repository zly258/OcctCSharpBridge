# OcctCSharpBridge Complete API Reference

This directory contains the English reference for the complete public .NET API surface.

Generate or refresh it with:

```powershell
.\build.ps1 docs Release
```

`tools/OcctApiDocsGenerator` reads the four public assemblies and their XML documentation and writes one Markdown page per exported type under `reference/`. Each page contains assembly, namespace, declaration, inheritance, constructors, properties, events, methods, parameters, return types and public fields/enum values.

Covered assemblies:

- `OcctNet.dll`
- `OcctNet.WinForms.dll`
- `OcctNet.Wpf.dll`
- `OcctNet.Avalonia.dll`

For ownership, lifetime, threading, runtime and architectural semantics, use the conceptual chapters in `docs/en-US`; use this generated Reference for exact signatures.