# OcctScript

OcctScript is the application layer on top of `OcctCSharpBridge`. It adds a JSON document, parameter expressions, an extensible command registry, dependency-driven geometry rebuild, undo/redo and a WPF editor.

## Layering

```text
OcctScript.Editor
        ↓
OcctScript.Application
        ↓
OcctScript.Domain ── OcctScript.Expressions
        ↓
OcctScript.Geometry
        ↓
      OcctNet
        ↓
   OcctNative / OCCT
```

The bridge remains independent from the document model. OcctScript does not introduce OCAF/XDE.

## Rebuild flow

1. Validate parameter names and command metadata.
2. Evaluate numeric parameters.
3. Resolve command references and build a dependency graph.
4. Topologically sort commands.
5. Execute the registered `ICommandShapeBuilder` for each enabled command.
6. Validate produced OCCT topology against the command definition.
7. Apply the command's generic transform.
8. Copy resulting shapes into the interactive WPF viewer.
9. Report OCCT algorithm warnings/errors in the build output panel.

## Extending commands

A command is added in three independent pieces: `CommandDefinition` metadata in `BuiltInCommandCatalog`, an `ICommandShapeBuilder` that creates the real `OcctModelShape`, and registration in `ScriptBuildCoordinator.CreateDefaultBuilderRegistry()`.

The editor reads command metadata dynamically, so ordinary commands do not need a custom property panel.

See [COMMANDS.md](COMMANDS.md) and [JSON_FORMAT.md](JSON_FORMAT.md).
