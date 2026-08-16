# Unified Demo Branch Notes

The `demo` branch is the single Binary SDK consumer branch. It never contains Bridge implementation source.

## Projects and platforms

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → OcctNet.WinForms   (Windows x64)
├─ OcctDemo.Wpf       → OcctNet.Wpf        (Windows x64)
└─ OcctDemo.Avalonia  → OcctNet.Avalonia   (Windows x64 / Linux x64)
```

Windows builds three UI hosts. Linux builds Avalonia only.

## Bridge 3 / ABI5 boundary

- No `src/OcctNative` or `src/OcctNet*` implementation source is tracked.
- Demo C# code must not declare or call `occt_*` Native ABI entry points.
- Pre-ABI5 generic handles and compatibility metadata are forbidden.
- Retired object snapshot, appearance alias, Viewer BRep annotation and old Modeling-to-Viewer APIs are rejected by the consumer checks.
- Current public APIs remain authoritative; compatibility guards must not invent replacement APIs.

## Binary SDK workflow

`dist/` is ignored by Git and is not a second source of truth.

Windows:

```powershell
.\sync.ps1
.\build.ps1 validate Release
.\build.ps1 all Release
```

Linux:

```bash
./sync.sh
./build.sh validate Release
./build.sh all Release
```

Both sync paths validate the Binary SDK and reuse it when `manifest.sourceCommit` matches `origin/main`. Source worktrees used to regenerate the SDK are created beside the repository rather than under a system temporary directory.

## Publish

Windows produces three independent packages: WinForms, WPF and Avalonia.
Linux produces one package: `CAD-Avalonia-linux-x64`.

Standalone `avalonia` and `avalonia-dev` branches are retired after migration. `backup/*` branches are intentionally unchanged.

No GitHub Actions or NuGet publication flow is used by this Demo branch.
