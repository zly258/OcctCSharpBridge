# Getting Started

The `avalonia` branch is self-contained source. There is no sync flow, tracked `dist`, branch-local binary publication, WinForms or WPF dependency.

## Windows

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 avalonia-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## Linux

Required baseline: Linux x64, .NET SDK 10.0.302, OCCT 7.9.0, CMake 3.21+, C++17 compiler, and OpenGL/X11 development libraries for the current Viewer backend.

Default OCCT paths:

```text
/usr/local/include/opencascade
/usr/local/lib
```

```bash
./build.sh all Release
./build.sh avalonia-smoke Release
```

`all` performs Native + Managed + ManagedTests + headless Smoke. `avalonia-smoke` is separate because Linux Viewer validation requires an X11/XWayland desktop session.

## Avalonia use

The application-facing control is identical on Windows and Linux:

```csharp
var viewport = new OcctAvaloniaViewport();
```

No application code should depend on HWND, XID, `WNT_Window`, or `Xw_Window`.
