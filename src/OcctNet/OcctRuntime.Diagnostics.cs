using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed class OcctRuntimeDiagnosticInfo
{
    internal OcctRuntimeDiagnosticInfo(
        DateTimeOffset capturedAtUtc,
        string frameworkDescription,
        string operatingSystemDescription,
        Architecture processArchitecture,
        Architecture operatingSystemArchitecture,
        bool is64BitProcess,
        string baseDirectory,
        string currentDirectory,
        string? configuredNativeDirectory,
        string? configuredOcctRoot,
        string? configuredCasRoot,
        string? configuredNativeBridgePath,
        bool? configuredNativeBridgeExists,
        string? configuredOcctKernelPath,
        bool? configuredOcctKernelExists,
        string? loadedNativeBridgePath,
        string? loadedOcctKernelPath,
        string diagnosticReport)
    {
        CapturedAtUtc = capturedAtUtc;
        FrameworkDescription = frameworkDescription;
        OperatingSystemDescription = operatingSystemDescription;
        ProcessArchitecture = processArchitecture;
        OperatingSystemArchitecture = operatingSystemArchitecture;
        Is64BitProcess = is64BitProcess;
        BaseDirectory = baseDirectory;
        CurrentDirectory = currentDirectory;
        ConfiguredNativeDirectory = configuredNativeDirectory;
        ConfiguredOcctRoot = configuredOcctRoot;
        ConfiguredCasRoot = configuredCasRoot;
        ConfiguredNativeBridgePath = configuredNativeBridgePath;
        ConfiguredNativeBridgeExists = configuredNativeBridgeExists;
        ConfiguredOcctKernelPath = configuredOcctKernelPath;
        ConfiguredOcctKernelExists = configuredOcctKernelExists;
        LoadedNativeBridgePath = loadedNativeBridgePath;
        LoadedOcctKernelPath = loadedOcctKernelPath;
        DiagnosticReport = diagnosticReport;
    }

    public DateTimeOffset CapturedAtUtc { get; }
    public string FrameworkDescription { get; }
    public string OperatingSystemDescription { get; }
    public Architecture ProcessArchitecture { get; }
    public Architecture OperatingSystemArchitecture { get; }
    public bool Is64BitProcess { get; }
    public string BaseDirectory { get; }
    public string CurrentDirectory { get; }
    public string? ConfiguredNativeDirectory { get; }
    public string? ConfiguredOcctRoot { get; }
    public string? ConfiguredCasRoot { get; }
    public string? ConfiguredNativeBridgePath { get; }
    public bool? ConfiguredNativeBridgeExists { get; }
    public string? ConfiguredOcctKernelPath { get; }
    public bool? ConfiguredOcctKernelExists { get; }
    public string? LoadedNativeBridgePath { get; }
    public string? LoadedOcctKernelPath { get; }
    public bool NativeBridgeLoaded => LoadedNativeBridgePath is not null;
    public bool OcctKernelLoaded => LoadedOcctKernelPath is not null;
    public string DiagnosticReport { get; }
}

public static partial class OcctRuntime
{
    public static OcctRuntimeDiagnosticInfo GetDiagnosticInfo()
    {
        var configuredNativeDirectory = GetEnvironmentPath("OCCT_BRIDGE_NATIVE_DIR");
        var configuredOcctRoot = GetEnvironmentPath("OCCT_ROOT");
        var configuredCasRoot = GetEnvironmentPath("CASROOT");
        var effectiveOcctRoot = configuredOcctRoot ?? configuredCasRoot;

        var configuredNativeBridgePath = configuredNativeDirectory is null
            ? null
            : Path.Combine(configuredNativeDirectory, "OcctNative.dll");
        var configuredOcctKernelPath = effectiveOcctRoot is null
            ? null
            : Path.Combine(effectiveOcctRoot, "win64", "vc14", "bin", "TKernel.dll");

        return new OcctRuntimeDiagnosticInfo(
            DateTimeOffset.UtcNow,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            RuntimeInformation.ProcessArchitecture,
            RuntimeInformation.OSArchitecture,
            Environment.Is64BitProcess,
            AppContext.BaseDirectory,
            Environment.CurrentDirectory,
            configuredNativeDirectory,
            configuredOcctRoot,
            configuredCasRoot,
            configuredNativeBridgePath,
            FileExistsOrNull(configuredNativeBridgePath),
            configuredOcctKernelPath,
            FileExistsOrNull(configuredOcctKernelPath),
            TryFindLoadedRuntimeModule("OcctNative.dll"),
            TryFindLoadedRuntimeModule("TKernel.dll"),
            GetDiagnosticReport());
    }

    private static string? GetEnvironmentPath(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim().Trim('"');
        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch (Exception)
        {
            return trimmed;
        }
    }

    private static bool? FileExistsOrNull(string? path) => path is null ? null : File.Exists(path);

    private static string? TryFindLoadedRuntimeModule(string moduleName)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
            {
                if (string.Equals(module.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                    return module.FileName;
            }
        }
        catch (Exception)
        {
            // Diagnostics must remain available even when module enumeration is restricted.
        }

        return null;
    }
}
