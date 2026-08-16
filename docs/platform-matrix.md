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
| Publish package count | 3 | 1 |

- Windows scripts: `sync.ps1`, `build.ps1`, `run.ps1`, `publish.ps1`.
- Linux scripts: `sync.sh`, `build.sh`, `run.sh`, `publish.sh`.
- `dist/` is local, ignored by Git, and validated against the Binary SDK manifest.
- WinForms and WPF are Windows-only.
- Avalonia is the only Linux UI host.
- There are no standalone Avalonia source branches; Avalonia is part of the formal SDK and unified Demo.
- During `demo-dev` validation against unreleased SDK changes, Windows may explicitly run `sync.ps1 -SourceBranch main-dev -ForceRebuild`; formal `demo` keeps the default `main` source.
