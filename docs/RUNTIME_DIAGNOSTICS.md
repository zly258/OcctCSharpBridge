# Structured Runtime Diagnostics

`OcctRuntime.GetDiagnosticReport()` remains the detailed human-readable report. Bridge 2.6 also exposes `OcctRuntime.GetDiagnosticInfo()` for UI, automated checks, support bundles, and startup diagnostics that should not parse report text.

## Capture a snapshot

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

Console.WriteLine(info.ProcessArchitecture);
Console.WriteLine(info.ConfiguredNativeDirectory);
Console.WriteLine(info.ConfiguredNativeBridgeExists);
Console.WriteLine(info.LoadedNativeBridgePath);
Console.WriteLine(info.LoadedOcctKernelPath);
```

`GetDiagnosticInfo()` does **not** force `OcctNative.dll` or OCCT to load. Loaded-module fields only describe modules that are already present in the current process.

## Main fields

`OcctRuntimeDiagnosticInfo` includes:

- capture time, framework description, OS description;
- process and OS architecture;
- `Is64BitProcess`;
- application base directory and current directory;
- configured `OCCT_BRIDGE_NATIVE_DIR`;
- configured `OCCT_ROOT` and `CASROOT`;
- the configured `OcctNative.dll` path and whether it currently exists;
- the configured OCCT `TKernel.dll` path and whether it currently exists;
- the actual loaded `OcctNative.dll` path, when already loaded;
- the actual loaded `TKernel.dll` path, when already loaded;
- `NativeBridgeLoaded` and `OcctKernelLoaded` convenience flags;
- the original `DiagnosticReport` string.

The `Configured...Exists` properties are nullable:

- `null`: the corresponding environment-based path is not configured;
- `false`: a path is configured but the expected file is absent;
- `true`: the expected file exists at that configured location.

## Diagnosing Win32 126

A practical startup check is:

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

if (info.ConfiguredNativeBridgeExists == false)
{
    // OCCT_BRIDGE_NATIVE_DIR points somewhere that does not contain OcctNative.dll.
}

if (info.ConfiguredOcctKernelExists == false)
{
    // OCCT_ROOT/CASROOT does not match the expected OCCT 7.9 VC14 x64 layout.
}

if (info.NativeBridgeLoaded && !info.OcctKernelLoaded)
{
    // The bridge is present but one or more OCCT runtime dependencies may still be unresolved.
}
```

Do not interpret `NativeBridgeLoaded == false` as an error before the first native operation. The snapshot deliberately does not trigger a load.

After a successful native operation, `LoadedNativeBridgePath` and `LoadedOcctKernelPath` are useful for detecting accidental loading from another application directory or an old runtime copy.

## Desktop UI integration

WinForms, WPF, and Avalonia applications can present the structured fields directly in a startup diagnostics dialog or troubleshooting panel while keeping the full `DiagnosticReport` behind a “details” view.

Recommended summary fields are:

1. process architecture;
2. configured bridge path + existence;
3. configured OCCT kernel path + existence;
4. loaded bridge path;
5. loaded OCCT kernel path.

This is more stable than parsing localized console/log text.

## Path privacy

Diagnostic information contains local filesystem paths. Before attaching it to an issue or external support message, review/redact user names, project paths, network shares, or other environment-specific information if needed.

## Relationship to deployment

The structured snapshot does not replace packaging validation. Demo `publish.ps1` still resolves the native dependency closure and runs the restricted `LoadLibraryExW` probe before redistribution. Runtime diagnostics answer a different question: **what configuration and modules does this particular process see now?**

For packaged applications, prefer app-local native runtime deployment. Use `OCCT_BRIDGE_NATIVE_DIR`, `OCCT_ROOT`, or `CASROOT` when an explicit development/runtime layout is intentional.
