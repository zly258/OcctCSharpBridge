# Bridge Migration

Bridge 3 makes `main` the only formal SDK source. The repository migration order is:

1. Stabilize Native Core, `OcctNet`, WinForms and WPF on `main-dev`.
2. Move the Windows examples on `demo-dev` to consume the formal SDK.
3. Move Avalonia examples and packaging on `avalonia-dev` to consume the same SDK.

## ABI policy

ABI 5 is the current contract. ABI 4 remains binary compatible and is frozen at 419 exports until its planned removal in Bridge 4.0. New functionality may only extend the current ABI.

The original `occt_bridge_abi_version` entry preserves its ABI 4 behavior for fixed 2.7 consumers. Current SDK code queries `occt_bridge_current_abi_version`.

Public APIs and filenames use semantic names. Version selection belongs in `structSize` and `apiVersion`; names ending in `V1`, `V2`, `Ex` or similar suffixes are not used for new APIs.

## Current preview scope

The preview introduces typed Engine and Modeling Session handles, `SafeHandle` ownership, `OcctStatus`, caller-owned error buffers, Viewer/Scene/Document contexts, platform-window isolation, topology history and persistent topology references.

The formal managed SDK uses the typed lifecycle and semantic native-surface API. Legacy lifecycle and surface entries remain compatibility adapters and are covered by a fixed old-consumer executable.
Current-only managed declarations use source-generated `LibraryImport` with explicit C calling convention. Frozen ABI 4 declarations and compatibility extensions remain isolated on `DllImport`; contract checks prevent either set from crossing that boundary.
The current ABI provides opaque `OcctShapeHandle`, `OcctMeshHandle`, and `OcctAlgorithmHandle` resources. Shape snapshots, independent mesh buffers, and algorithm diagnostic snapshots are owned through managed `SafeHandle` wrappers. Algorithm resources copy the operation ID, warning/error flags, and report so diagnostics remain available after the source Modeling Session is disposed; topology lineage remains owned by the session history API. Mesh creation uses an extensible `structSize`/`apiVersion` options structure, while node and triangle data is copied into caller-owned bulk buffers. Callers must not query a resource concurrently with disposal.

## Compatibility gates

Every standard build validates:

- all 419 frozen ABI 4 symbols remain exported;
- native declarations, implementations and P/Invokes are identical sets;
- all 23 formal current-ABI declarations use `LibraryImport`, while the compatibility extension remains isolated;
- new exports and tracked filenames follow semantic naming;
- WinForms and WPF consume the platform-neutral managed Engine API;
- the fixed ABI 4 consumer and current ABI 5 native smoke both run successfully.

`demo-dev` and `avalonia-dev` remain external SDK consumers; continued core evolution and ABI ownership stay on `main-dev`.
