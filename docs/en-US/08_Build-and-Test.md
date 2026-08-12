# Build and Test

The `avalonia` branch has two platform build entry points and one shared source/API contract. It does not track `dist` or provide branch-local binary publishing.

## Windows

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 avalonia-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 docs Release
```

Recommended complete non-GUI validation:

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

`all` runs Native + Managed + ManagedTests + headless Smoke. `avalonia-smoke` is separate and validates the real Avalonia Desktop → `OcctAvaloniaViewport` → Native Surface → OCCT Viewer path.

## Linux

```bash
./build.sh validate
./build.sh managed Release
./build.sh test Release
./build.sh smoke Release
./build.sh avalonia-smoke Release
./build.sh docs Release
```

Recommended complete non-GUI validation:

```bash
./build.sh all Release
```

`all` runs Native + Managed + ManagedTests + headless Smoke. `avalonia-smoke` requires an X11/XWayland desktop session and validates the complete Avalonia Viewer path.

The Linux Native output follows the CMake configuration directory, for example:

```text
build/native/bin/Release/libOcctNative.so
```

No sync step, WinForms/WPF project, tracked Binary SDK, branch-local publish script, NuGet release pipeline, or GitHub Actions workflow is part of this branch.
