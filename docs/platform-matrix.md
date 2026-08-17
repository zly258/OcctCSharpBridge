# Demo Platform Matrix

| Platform | Common | WinForms | WPF | Avalonia |
|---|---:|---:|---:|---:|
| Windows x64 | yes | yes | yes | yes |
| Linux x64 | yes | no | no | yes |

The Demo branch is the single application-consumer branch for both operating systems. `main` remains the sole Bridge SDK source.

| Capability | Windows x64 | Linux x64 |
|---|---|---|
| Binary SDK | `dist/win-x64` | `dist/linux-x64` |
| Demo hosts | WinForms / WPF / Avalonia | Avalonia |
| Native host | HWND | X11/XWayland XID |
| Platform-neutral Pointer/Key input | yes | yes |
| Host lifecycle / first-frame contract | yes | yes |
| Viewer projection sample | yes | yes |
| Default publish package count | **1 unified package** | 1 |
| Default publish runtime mode | framework-dependent | self-contained |
| Package integrity manifest | `package-manifest.json` with SHA-256 | `publish-manifest.txt` with SHA-256 |

- Windows scripts: `sync.ps1`, `build.ps1`, `run.ps1`, `publish.ps1`.
- Linux scripts: `sync.sh`, `build.sh`, `run.sh`, `publish.sh`.
- `dist/` is local, ignored by Git, and validated against the Binary SDK manifest.
- Windows `sync.ps1` runs the Bridge `sdk` Release gate when rebuilding from source and accepts only the exact seven-file `win-x64` Binary SDK payload.
- Windows `run.ps1` reads the Demo TFM from each Demo `.csproj`; Demo output paths are independent from the Bridge minimum TFM.
- Windows `publish.ps1 all` produces `artifacts/publish/CAD-Demo-win-x64/` with WinForms/WPF/Avalonia sharing one copy of application, Bridge, OCCT and resource dependencies. The unified package requires the .NET 10 Desktop Runtime x64 on the target machine.
- A single Windows target (`winform`, `wpf`, `avalonia`) can still be published as a standalone package; it is self-contained by default or can be made framework-dependent explicitly.
- Unified Windows `all` publishing cannot be self-contained because the three Windows Desktop publish closures contain conflicting same-name framework DLLs.
- WinForms and WPF are Windows-only.
- Avalonia is the only Linux UI host.
- There are no standalone Avalonia source branches; Avalonia is part of the formal SDK and unified Demo.
- During `demo-dev` validation against unreleased SDK changes, Windows may explicitly run `sync.ps1 -SourceBranch main-dev -ForceRebuild`; formal `demo` keeps the default `main` source.
