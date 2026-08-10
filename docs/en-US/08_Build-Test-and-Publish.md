# 08 Build, Test and Publish

Validation is layered: repository contracts, managed regression tests, real native smoke tests, generated API Reference, and the validated Binary SDK.

## Build targets

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 native Release
.\build.ps1 smoke Release
.\build.ps1 pack Release
.\build.ps1 docs Release
.\build.ps1 dist Release
.\build.ps1 all Release
.\build.ps1 clean Release
```

`docs` builds the managed SDK and runs `tools/OcctApiDocsGenerator`. The generator enumerates every exported managed type and its declared public constructors, properties, events, methods and fields into both language trees.

`dist` is Release-only and requires a clean worktree. It runs native build, managed build, managed tests and native smoke before replacing `dist/win-x64` through a staging/backup transaction. The generated manifest records the exact clean source commit and SHA-256 hashes.

## Publishing a release

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The publishing workflow intentionally includes public API documentation before the binary build:

```text
clean main worktree
→ build.ps1 docs Release
→ commit generated zh-CN/en-US API Reference when changed
→ clean committed source state
→ build.ps1 dist Release
→ commit dist/win-x64
→ push main
→ temporary detached worktree for origin/demo
→ synchronize only dist/win-x64
→ validate contract / manifest / SHA-256
→ commit and push demo
```

This ordering means `bridge-manifest.json.sourceCommit` identifies the same committed source and generated public API Reference that produced the DLLs. The publishing script does not switch the developer's current checkout and does not use GitHub Actions.

## Static checks

PowerShell contract checks remain limited to durable repository invariants such as version/ABI parity, architecture boundaries, bulk ABI use, native build structure and package metadata. Source wording and implementation placement are not treated as contracts.

## Release principle

Only a Binary SDK produced after successful local Windows/MSVC/OCCT validation is publishable. The real local native toolchain is the source of truth for release readiness. Demo receives only the validated Binary SDK; Bridge source is never synchronized into the demo branch.
