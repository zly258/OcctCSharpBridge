# Bridge 3 ABI5 Migration

Bridge 3 makes `main` the only formal SDK source. The branch migration is now consolidated around one SDK line and one Demo consumer line:

1. `main-dev` stabilized the ABI5-only Native Core, `OcctNet`, WinForms, WPF and Avalonia adapters.
2. The validated SDK rewrite was squash-merged to `main`.
3. `demo-dev` migrated WinForms/WPF and absorbed Avalonia Windows/Linux examples and packaging.
4. The unified Demo was squash-merged to `demo`.
5. Standalone `avalonia` / `avalonia-dev` branches are retired after migration; Avalonia remains a first-class SDK adapter and Demo host.

## ABI policy

ABI 5 is the only supported native ABI. Pre-ABI5 exports, generic handles, compatibility shims, fixed old-consumer tests and compatibility metadata are removed rather than frozen or forwarded.

Public native entry points use semantic names. Extensible data structures use `structSize` and `apiVersion` where versioned layout is required; exported function names do not use migration suffixes such as `V1`, `V2` or `Ex`.

Managed production interop uses source-generated `LibraryImport` with C calling convention and maps one-to-one to the canonical ABI5 declarations and definitions.

## Current architecture

- typed Engine, Shape, Mesh and Algorithm resource ownership;
- `OcctStatus` and structured native error state;
- caller-owned snapshot/buffer APIs for bulk Viewer and Modeling data;
- Viewer/Scene/Document contexts separated from headless Modeling state;
- OS window-system integration isolated under `src/OcctNative/platform`;
- topology history and persistent topology references owned by the Modeling Session;
- `main` / `main-dev` own SDK implementation;
- `demo` / `demo-dev` consume generated Binary SDKs only;
- Windows Demo hosts: WinForms, WPF, Avalonia;
- Linux Demo host: Avalonia only.

## Binary SDK policy

Generated `dist/win-x64` and `dist/linux-x64` payloads are local/Release artifacts and are not committed to SDK or Demo source branches. Package freshness is established through schema-3 contract metadata, schema-2 manifest metadata, `sourceCommit` and SHA-256 hashes.

## Validation gates

Every standard validation run checks that:

- `bridge-contract.json` declares ABI 5 as both current and minimum supported ABI with `api.policy = abi5-only`;
- retired compatibility files and old version-specific documentation are not tracked;
- Native declarations, implementations and managed `LibraryImport` bindings are identical sets;
- `DllImport` is not used by production managed ABI5 interop;
- Native C++ inventory, module boundaries and platform isolation remain valid;
- bulk collections use snapshot/buffer APIs rather than borrowed legacy handles.

The Demo consumer guards additionally reject SDK implementation source, direct `occt_*` calls and retired managed consumer APIs.
