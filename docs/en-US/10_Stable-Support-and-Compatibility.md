# 3.x Support Boundaries

`bridge-contract.json` records the Bridge version, ABI, OCCT, .NET, and platform facts.

- Windows x64 is the official prebuilt platform.
- Linux x64 is a source-build platform.
- The current Native ABI is ABI 5 only.
- Managed projects directly multi-target .NET 8 / 9 / 10.
- A single `OcctEngine` or `OcctModelingSession` is not assumed to be concurrently thread-safe.
- Handles/object IDs remain bound to their owning Engine/Session.
- Normal modeling values use the application's consistent unit convention.
- Managed assemblies, Native runtime, OCCT resources, and manifests should come from the same build.

The repository does not keep old ABI shims, migration entry points, or a frozen API inventory. An intentional ABI break should change the Bridge/ABI version explicitly instead of adding compatibility layers.
