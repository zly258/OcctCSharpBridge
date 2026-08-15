# Bridge 3 ABI5 Migration

Bridge 3 makes `main` the only formal SDK source. The migration order is:

1. Stabilize and validate the ABI5-only Native Core, `OcctNet`, WinForms, WPF and Avalonia host on `main-dev`.
2. Move the Windows examples on `demo-dev` to consume the resulting SDK.
3. Move Avalonia examples and packaging on `avalonia-dev` to consume the same SDK.
4. Submit `main-dev -> main`, `demo-dev -> demo`, and `avalonia-dev -> avalonia` as independent PRs.

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
- `demo-dev` and `avalonia-dev` remain SDK consumers rather than carrying private Native/Core implementations.

## Validation gates

Every standard validation run checks that:

- `bridge-contract.json` declares ABI 5 as both current and minimum supported ABI with `api.policy = abi5-only`;
- retired compatibility files and old version-specific documentation are not tracked;
- tracked platform Binary SDK contracts, when present, are ABI5-only;
- Native declarations, implementations and managed `LibraryImport` bindings are identical sets;
- `DllImport` is not used by production managed ABI5 interop;
- Native C++ inventory, module boundaries and platform isolation remain valid;
- bulk collections use snapshot/buffer APIs rather than borrowed legacy handles.
