using System.Runtime.CompilerServices;
using OcctNet;

internal static class RuntimeDiagnosticTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        var info = OcctRuntime.GetDiagnosticInfo();

        if (string.IsNullOrWhiteSpace(info.FrameworkDescription))
            throw new InvalidOperationException("Runtime diagnostics did not report the .NET framework.");
        if (string.IsNullOrWhiteSpace(info.OperatingSystemDescription))
            throw new InvalidOperationException("Runtime diagnostics did not report the operating system.");
        if (string.IsNullOrWhiteSpace(info.BaseDirectory) || string.IsNullOrWhiteSpace(info.CurrentDirectory))
            throw new InvalidOperationException("Runtime diagnostics did not report process directories.");
        if (string.IsNullOrWhiteSpace(info.DiagnosticReport))
            throw new InvalidOperationException("Runtime diagnostics did not preserve the text diagnostic report.");

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
