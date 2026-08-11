# 08 Build, Test and Publish

Build, test and publish responsibilities are intentionally separated. Repository contract checks and Release compilation belong to the build/publish path. Managed regression tests and real native smoke scenarios are explicit diagnostic targets and no longer block `dist` or `publish`.

Current release metadata is summarized in the documentation index; the author is **zly258** and the current Bridge contract is 2.6.0 / ABI 4 / OCCT 7.9.0 / .NET 10 / C# 14 / C++17 / Windows x64.

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

`test` runs the managed regression suite explicitly. `smoke` builds the native bridge and executes the real OCCT/native scenarios explicitly. Neither target is invoked by `dist` or `publish`.

`docs` builds the managed SDK and runs `tools/OcctApiDocsGenerator`, producing the bilingual Managed + Native API Reference.

`all` builds the Native Bridge and all four managed SDK projects without implicitly running tests.

## Binary SDK distribution

`dist` is Release-only and requires a clean worktree:

```powershell
.\build.ps1 dist Release
```

The distribution path is intentionally deterministic and test-free:

```text
repository contract checks
→ Native Release build
→ Managed Release build
→ collect Binary SDK files
→ SHA-256
→ bridge-manifest.json
→ staging/backup transaction
→ replace dist/win-x64
```

`dist` does **not** run managed tests or native smoke scenarios. Test failures therefore do not prevent a Binary SDK from being assembled. Run `test` or `smoke` explicitly when those diagnostics are required.

The generated manifest records the exact clean source commit, Bridge/ABI/OCCT/.NET contract metadata and SHA-256 hashes for the distributed files.

## Publishing a release

The default OCCT root is already resolved by the scripts, so the normal command is:

```powershell
.\publish.ps1
```

The publishing workflow is:

```text
clean main worktree
→ build.ps1 docs Release
→ commit generated zh-CN/en-US API Reference when changed
→ clean committed source state
→ build.ps1 dist Release
→ validate Binary SDK contract / manifest / SHA-256
→ commit dist/win-x64
→ push main
→ temporary detached worktree for origin/demo
→ synchronize only dist/win-x64
→ validate contract / manifest / SHA-256 again
→ commit and push demo
```

`publish.ps1` does **not** invoke `test` or `smoke`. `-Fast` only skips API documentation generation; it is unrelated to testing.

This ordering means `bridge-manifest.json.sourceCommit` identifies the same committed source and generated public API Reference that produced the DLLs. The publishing script does not switch the developer's current checkout and does not use GitHub Actions.

## Explicit validation when needed

```powershell
# Managed regression suite
.\build.ps1 test Release

# Real Native / OCCT scenarios
.\build.ps1 smoke Release
```

Native Smoke remains useful for finding native implementation defects, OCCT runtime-loading problems, geometry/topology errors, Viewer/Selection issues, Mesh/Exchange problems and ABI mistakes. It is a diagnostic gate chosen by the developer, not a prerequisite for Binary SDK publishing.

## Release principle

Binary SDK production must be deterministic and reproducible. Tests remain available and should be run when appropriate, but their execution is independent from packaging and branch synchronization.
