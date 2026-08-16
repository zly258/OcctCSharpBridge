# Windows Validation

The unified Windows Demo has been validated successfully on `demo-dev` with the Bridge 3 / ABI5 Binary SDK.

Validated hosts:

- WinForms
- WPF
- Avalonia

The Windows workflow uses `sync.ps1`, `build.ps1`, `run.ps1` and `publish.ps1`; all three hosts consume the same `dist/win-x64` Binary SDK and the shared `OcctDemo.Common` layer.
