using System.Runtime.CompilerServices;
using OcctNet;

internal static class RuntimeDiagnosticTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        var variables = new[] { "PATH", "OCCT_BRIDGE_NATIVE_DIR", "OCCT_ROOT", "CASROOT" };
        var before = variables.ToDictionary(name => name, Environment.GetEnvironmentVariable);

        var info = OcctRuntime.GetDiagnosticInfo();
        var report = OcctRuntime.GetDiagnosticReport();

        foreach (var variable in variables)
        {
            if (!string.Equals(before[variable], Environment.GetEnvironmentVariable(variable), StringComparison.Ordinal))
                throw new InvalidOperationException($"Runtime diagnostics changed environment variable {variable}.");
        }

        if (string.IsNullOrWhiteSpace(info.FrameworkDescription))
            throw new InvalidOperationException("Runtime diagnostics did not report the .NET framework.");
        if (string.IsNullOrWhiteSpace(info.OperatingSystemDescription))
            throw new InvalidOperationException("Runtime diagnostics did not report the operating system.");
        if (string.IsNullOrWhiteSpace(info.BaseDirectory) || string.IsNullOrWhiteSpace(info.CurrentDirectory))
            throw new InvalidOperationException("Runtime diagnostics did not report process directories.");
        if (string.IsNullOrWhiteSpace(info.DiagnosticReport) || string.IsNullOrWhiteSpace(report))
            throw new InvalidOperationException("Runtime diagnostics did not preserve the text diagnostic report.");

        if (!Path.IsPathFullyQualified(info.ApplicationNativeBridgePath) ||
            !Path.IsPathFullyQualified(info.ApplicationOcctKernelPath))
        {
            throw new InvalidOperationException("App-local runtime diagnostic paths are not fully qualified.");
        }
        if (info.ApplicationNativeBridgeExists != File.Exists(info.ApplicationNativeBridgePath))
            throw new InvalidOperationException("App-local native bridge existence state is inconsistent.");
        if (info.ApplicationOcctKernelExists != File.Exists(info.ApplicationOcctKernelPath))
            throw new InvalidOperationException("App-local OCCT kernel existence state is inconsistent.");

        if ((info.ConfiguredNativeBridgePath is null) != (info.ConfiguredNativeBridgeExists is null))
            throw new InvalidOperationException("Configured native bridge path/existence state is inconsistent.");
        if ((info.ConfiguredOcctKernelPath is null) != (info.ConfiguredOcctKernelExists is null))
            throw new InvalidOperationException("Configured OCCT kernel path/existence state is inconsistent.");
        if (info.NativeBridgeLoaded != (info.LoadedNativeBridgePath is not null))
            throw new InvalidOperationException("Native bridge loaded state is inconsistent.");
        if (info.OcctKernelLoaded != (info.LoadedOcctKernelPath is not null))
            throw new InvalidOperationException("OCCT kernel loaded state is inconsistent.");

        if (!info.Is64BitProcess)
            throw new InvalidOperationException("OcctCSharpBridge managed tests must run as a 64-bit process.");
    }
}
