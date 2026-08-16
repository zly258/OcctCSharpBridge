# Linux Validation Checklist

Run on Linux x64 with OCCT 7.9.0 installed and .NET SDK 10.0.303 available exactly.

```bash
./sync.sh
./build.sh validate Release
./build.sh all Release
./run.sh Release
./publish.sh Release
```

Acceptance:

- `sync.sh` produces or reuses `dist/linux-x64` from `main` and validates manifest hashes.
- `build.sh all Release` builds only Common + Avalonia.
- `CAD-Avalonia` opens under X11/XWayland and basic view/selection/modeling operations work.
- `publish.sh Release` creates `CAD-Avalonia-linux-x64` and resolves the packaged native dependency closure with `ldd`.
- No WinForms/WPF project is built on Linux.
- No Bridge implementation source is tracked or compiled by the Demo branch.
