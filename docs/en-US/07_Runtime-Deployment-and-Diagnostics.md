# 07 Runtime Deployment and Diagnostics

## Runtime layout

Managed applications load `OcctNet*.dll` and `OcctNative.dll`; `OcctNative.dll` then requires the OCCT 7.9.0 toolkit and third-party runtime dependencies used by the build.

`OcctRuntime` can resolve/configure the runtime from an explicit path, `OCCT_ROOT`, or `CASROOT`.

## Typical startup failures

For `DllNotFoundException`, Win32 error 126, or a process that exits before creating a viewport, verify:

1. `OcctNative.dll` is beside the managed application or otherwise resolvable;
2. the matching OCCT 7.9.0 `TK*.dll` files are on the runtime search path;
3. required third-party DLLs are present;
4. x64 architecture matches all Native binaries;
5. the managed Bridge version is compatible with the Native Bridge version/ABI.

## UI hosts

WinForms, WPF and Avalonia use Windows HWND-based OCCT rendering. Avalonia may publish its shell `MainWindowHandle` later than process startup, so process-liveness and actual application exceptions are more reliable than a fixed shell-window timeout.

## Portable demo publishing

The demo `publish.ps1` collects the Bridge and the OCCT runtime dependencies required by the demo applications. The local `demo/dist` SDK itself remains ignored by Git.
