# Structured Runtime Diagnostics

`OcctRuntime.GetDiagnosticReport()` remains the detailed human-readable report. `OcctRuntime.GetDiagnosticInfo()` provides a typed, side-effect-free snapshot for startup diagnostics, UI, automated checks, and support bundles.

Neither diagnostic API configures the runtime, changes DLL search paths, changes OCCT environment variables, or forces `OcctNative.dll` to load.

## Capture a snapshot

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

Console.WriteLine(info.ProcessArchitecture);
Console.WriteLine(info.ApplicationNativeBridgePath);
Console.WriteLine(info.ApplicationNativeBridgeExists);
Console.WriteLine(info.ApplicationOcctKernelPath);
Console.WriteLine(info.ApplicationOcctKernelExists);
Console.WriteLine(info.LoadedNativeBridgePath);
Console.WriteLine(info.LoadedOcctKernelPath);
```

## Diagnostic layers

The snapshot deliberately separates three different questions.

### 1. App-local package state

These fields describe the executable directory without relying on environment variables:

- `ApplicationNativeBridgePath`
- `ApplicationNativeBridgeExists`
- `ApplicationOcctKernelPath`
- `ApplicationOcctKernelExists`

A normal portable Demo package places `OcctNative.dll`, `TKernel.dll`, the required OCCT modules, VC++ runtime components, and third-party native dependencies beside the executable. These fields therefore help diagnose package layout **before the first Native call**.

### 2. Explicit/environment configuration

The snapshot also reports:

- `ConfiguredNativeDirectory` from `OCCT_BRIDGE_NATIVE_DIR`;
- `ConfiguredOcctRoot` from `OCCT_ROOT`;
- `ConfiguredCasRoot` from `CASROOT`;
- `ConfiguredNativeBridgePath` and nullable `ConfiguredNativeBridgeExists`;
- `ConfiguredOcctKernelPath` and nullable `ConfiguredOcctKernelExists`.

The nullable existence properties mean:

- `null`: no corresponding environment-derived path is configured;
- `false`: a path is configured but the expected file is absent;
- `true`: the expected file exists at that configured location.

### 3. Actual process state

When modules have already been loaded, diagnostics report:

- `LoadedNativeBridgePath`;
- `LoadedOcctKernelPath`;
- `NativeBridgeLoaded`;
- `OcctKernelLoaded`.

Do not interpret `NativeBridgeLoaded == false` as an error before the first Native operation. `GetDiagnosticInfo()` intentionally does not trigger a load.

## Diagnosing Win32 126

A useful sequence is:

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

if (!info.ApplicationNativeBridgeExists)
{
    // The executable directory does not contain OcctNative.dll.
}

if (info.ConfiguredNativeBridgeExists == false)
{
    // OCCT_BRIDGE_NATIVE_DIR points to a directory without OcctNative.dll.
}

if (info.ConfiguredOcctKernelExists == false)
{
    // OCCT_ROOT/CASROOT does not match the expected OCCT 7.9 VC14 x64 layout.
}

if (info.NativeBridgeLoaded && !info.OcctKernelLoaded)
{
    // The bridge is in the process but one or more OCCT runtime dependencies may be unresolved.
}
```

After a successful Native operation, loaded-module paths are particularly useful for detecting an old DLL that was loaded from another application directory.

## Text report

`GetDiagnosticReport()` remains useful when a complete diagnostic block is easier to attach to a log. It includes:

- configuration state;
- base directory;
- app-local bridge/kernel presence;
- configured Native/OCCT paths;
- repository probing state;
- Native bridge candidates;
- key OCCT resource environment variables.

The report is observational only. Reading it must not mutate `PATH`, `OCCT_BRIDGE_NATIVE_DIR`, `OCCT_ROOT`, or `CASROOT`.

## Runtime code organization

Runtime responsibilities are intentionally split instead of accumulating in one large file:

```text
OcctRuntime.cs                 configuration state and conflict validation
OcctRuntime.Probing.cs         bridge/OCCT/repository/resource path discovery
OcctRuntime.Environment.cs     DLL search policy, PATH and OCCT resource variables
OcctRuntime.Diagnostics.cs     structured and text diagnostics
```

This is an internal organization change; the public `OcctRuntime` API remains one static partial type.

## Desktop UI integration

WinForms, WPF, and Avalonia applications can show a compact startup diagnostics panel using:

1. process architecture;
2. app-local bridge/kernel existence;
3. configured bridge/kernel existence;
4. loaded bridge/kernel paths;
5. full `DiagnosticReport` behind a details view.

This is more stable than parsing localized console output.

## Path privacy

Diagnostic information contains local filesystem paths. Review or redact user names, project folders, network shares, and other environment-specific details before posting a report externally.

## Relationship to publishing

Runtime diagnostics do not replace package validation. Demo `publish.ps1` still resolves the Native dependency closure and runs a restricted `LoadLibraryExW` probe before redistribution.

- **Publish validation** asks whether the package contains a loadable dependency closure.
- **Runtime diagnostics** asks what paths and modules this particular process sees now.

For redistributed applications, prefer app-local Native runtime deployment. Use `OCCT_BRIDGE_NATIVE_DIR`, `OCCT_ROOT`, or `CASROOT` only when an explicit development/runtime layout is intentional.
