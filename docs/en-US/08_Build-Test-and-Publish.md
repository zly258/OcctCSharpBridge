# 08 Build, Test and Publish

## Build targets

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 docs Release
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

Keep validation focused on real source/ABI/build invariants; generated documentation and smoke tests are explicit targets rather than a substitute for compiling/running the Bridge.

The tracked Binary SDK status is always read from `main/dist/win-x64/bridge-contract.json` and `bridge-manifest.json`; do not manually rewrite release metadata or duplicate a hard-coded published version in documentation.

## Binary SDK publication

`main/dist/win-x64` is the only tracked Binary SDK copy. `publish.ps1` must run from a clean, named `main` branch and verifies that local `main` is based on the current `origin/main`.

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Normal flow:

```text
fetch/verify origin/main ancestry
→ generate bilingual API docs (unless -Fast)
→ build Release dist
→ validate contract/manifest/SHA-256
→ commit dist/win-x64 if changed
→ push main
```

## Demo consumption

`demo/dist` is ignored and is never committed by the release workflow. On the demo branch:

```powershell
.\sync.ps1
.\build.ps1 all Release
```

`sync.ps1` copies the currently published `origin/main/dist/win-x64` payload into the local ignored directory and prints its contract.

## Generated API docs

`build.ps1 docs` owns `docs/zh-CN/api` and `docs/en-US/api`. Source contract metadata remains authoritative for current source API counts; generated reference pages should be refreshed as part of a normal release build.
