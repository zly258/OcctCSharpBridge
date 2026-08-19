# Unified Demo Branch Notes

`demo` / `demo-dev` are reference consumers of the OcctCSharpBridge Binary/Portable SDK. They do not contain Bridge implementation source.

## Projects

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

## SDK workflow

`dist/` is disposable local cache state. A synchronization cache hit validates `sourceCommit` and package hashes and performs no Bridge build.

A cache miss uses the **Bridge `dist Release` consumer fast path** only:

```text
Bridge dist Release
→ minimal Binary SDK
→ Bridge-owned Portable SDK packager
→ contract/sourceCommit/hash validation
→ Demo dist cache
```

Demo synchronization must not run Bridge `sdk`, `all`, ManagedTests, Core Smoke, or viewport/window smokes. Those are Bridge release-validation responsibilities.

Windows:

```powershell
.\sync.ps1
.\sync.ps1 -ForceRebuild
```

Linux:

```bash
./sync.sh
./sync.sh --force-rebuild
```

Prebuilt matching Binary + Portable SDKs can be supplied directly, avoiding Bridge compilation entirely.

## Consumer boundary

- no tracked Bridge Native/Core implementation source;
- no direct `occt_*` ABI imports;
- no pre-ABI5 compatibility APIs;
- no duplicate OCCT dependency collector;
- no Bridge full release gate hidden inside consumer synchronization.

The consumer checks enforce these boundaries.

## Publication

Windows `publish.ps1 all Release` defaults to a unified package with nested `apps/`, one shared private .NET 10 Desktop Runtime under `dotnet/`, one Bridge/OCCT `runtime/`, and shared OCCT resources. `-SelfContained` explicitly requests separate per-app runtime closures; `-FrameworkDependent` explicitly requires a machine runtime.

Linux publishes Avalonia and merges the matching Bridge Portable Runtime/resources. Linux native compatibility still depends on the glibc/libstdc++ ABI baseline used to build OCCT and `libOcctNative.so`.

For third-party project architecture and deployment guidance, use the formal Bridge documentation under `main` / `main-dev`, especially `docs/*/09_Third-Party-SDK-Consumption`.
