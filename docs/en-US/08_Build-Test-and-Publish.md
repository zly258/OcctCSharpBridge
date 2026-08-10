# 08 Build, Test and Publish

Validation is layered: repository contracts, managed regression tests, and real native smoke tests.

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

`dist` is Release-only and requires a clean worktree. It runs native build, managed build, managed tests and native smoke before replacing `dist/win-x64` through a staging/backup transaction.

## Publishing to demo

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

The publish script runs the `dist` gate, commits the Binary SDK on `main`, pushes `main`, creates a temporary detached worktree for `origin/demo`, synchronizes only `dist/win-x64`, validates contract/hash parity, commits the demo update and pushes it. It does not switch the developer's current checkout and does not use GitHub Actions.

Use `-NoPush` to exercise the local publishing flow without updating remotes.

## API documentation

`build.ps1 docs` builds the managed SDK and runs `tools/OcctApiDocsGenerator`. The generator enumerates every exported managed type and its declared public constructors, properties, events, methods and fields into both language trees.

## Static checks

PowerShell contract checks remain limited to durable repository invariants such as version/ABI parity, architecture boundaries, bulk ABI use, native build structure and package metadata. Source wording and implementation placement are not treated as contracts.

## Release principle

Only a Binary SDK produced after successful local Windows/MSVC/OCCT validation is publishable. The real local native toolchain is the source of truth for release readiness.