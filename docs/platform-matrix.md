# Demo Platform Matrix

| Platform | Common | WinForms | WPF | Avalonia |
|---|---:|---:|---:|---:|
| Windows x64 | yes | yes | yes | yes |
| Linux x64 | yes | no | no | yes |

The Demo is an installed-SDK consumer. Bridge `main` owns Bridge implementation, Binary SDK production, Portable Runtime packaging, validation, and installation.

| Capability | Windows x64 | Linux x64 |
|---|---|---|
| Installed SDK root | `C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64` | `$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64` |
| Binary SDK | SDK root | SDK root |
| Matching Portable Runtime | `SDK root\portable` | `SDK root/portable` |
| Demo hosts | WinForms / WPF / Avalonia | Avalonia |
| Native host | HWND | X11/XWayland XID |
| Platform-neutral pointer/key input | yes | yes |
| Host lifecycle / first-frame contract | yes | yes |
| Viewer projection sample | yes | yes |
| Demo Bridge clone/sync/build | no | no |
| Default publish package count | 1 unified package | 1 |
| Default .NET publish mode | one shared private .NET 10 Desktop Runtime | self-contained Avalonia |
| Bridge/OCCT native closure | one shared `runtime/` | one `runtime/` |
| Package integrity | `package-manifest.json` SHA-256 | `package-manifest.json` SHA-256 |

## SDK consumption

Install/update Bridge from `main`, then build or publish Demo directly. There is no Demo-side SDK synchronization step.

Windows:

```powershell
.\build.ps1 all Release
.\publish.ps1 all Release -Zip
```

Linux:

```bash
./build.sh all Release
./publish.sh Release
```

## Branch use

- `demo` consumes an installed Bridge SDK produced by `main`.
- WinForms/WPF remain Windows-only.
- Avalonia is the Linux UI host.
- there are no standalone Avalonia source branches.
