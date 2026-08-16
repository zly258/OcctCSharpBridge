# Demo Platform Matrix

| Platform | Common | WinForms | WPF | Avalonia |
|---|---:|---:|---:|---:|
| Windows x64 | yes | yes | yes | yes |
| Linux x64 | yes | no | no | yes |

The Demo branch is the single application-consumer branch for both operating systems.
`main` remains the sole Bridge SDK source.

- Windows scripts: `sync.ps1`, `build.ps1`, `run.ps1`, `publish.ps1`.
- Linux scripts: `sync.sh`, `build.sh`, `run.sh`, `publish.sh`.
- `dist/` is local, ignored by Git, and validated against the Binary SDK manifest.
- WinForms and WPF are Windows-only.
- Avalonia is the only Linux UI host.
- Standalone `avalonia` and `avalonia-dev` branches are retired after migration.
