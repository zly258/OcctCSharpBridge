using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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
        string applicationNativeBridgePath,
        bool applicationNativeBridgeExists,
        string applicationOcctKernelPath,
        bool applicationOcctKernelExists,
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
        ApplicationNativeBridgePath = applicationNativeBridgePath;
        ApplicationNativeBridgeExists = applicationNativeBridgeExists;
        ApplicationOcctKernelPath = applicationOcctKernelPath;
        ApplicationOcctKernelExists = applicationOcctKernelExists;
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
    public string ApplicationNativeBridgePath { get; }
    public bool ApplicationNativeBridgeExists { get; }
    public string ApplicationOcctKernelPath { get; }
    public bool ApplicationOcctKernelExists { get; }
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
    /// <summary>
    /// Returns a structured, side-effect-free snapshot of runtime paths and loaded modules.
    /// The snapshot does not configure the runtime or force a native library load.
    /// </summary>
    public static OcctRuntimeDiagnosticInfo GetDiagnosticInfo()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var applicationNativeBridgePath = Path.Combine(baseDirectory, NativeLibraryFileName);
        var applicationOcctKernelPath = Path.Combine(baseDirectory, "TKernel.dll");
        var configuredNativeDirectory = DiagnosticGetEnvironmentPath("OCCT_BRIDGE_NATIVE_DIR");
        var configuredOcctRoot = DiagnosticGetEnvironmentPath("OCCT_ROOT");
        var configuredCasRoot = DiagnosticGetEnvironmentPath("CASROOT");
        var effectiveOcctRoot = configuredOcctRoot ?? configuredCasRoot;

        var configuredNativeBridgePath = configuredNativeDirectory is null
            ? null
            : Path.Combine(configuredNativeDirectory, NativeLibraryFileName);
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
            baseDirectory,
            Environment.CurrentDirectory,
            applicationNativeBridgePath,
            File.Exists(applicationNativeBridgePath),
            applicationOcctKernelPath,
            File.Exists(applicationOcctKernelPath),
            configuredNativeDirectory,
            configuredOcctRoot,
            configuredCasRoot,
            configuredNativeBridgePath,
            DiagnosticFileExistsOrNull(configuredNativeBridgePath),
            configuredOcctKernelPath,
            DiagnosticFileExistsOrNull(configuredOcctKernelPath),
            DiagnosticTryFindLoadedRuntimeModule(NativeLibraryFileName),
            DiagnosticTryFindLoadedRuntimeModule("TKernel.dll"),
            GetDiagnosticReport());
    }

    /// <summary>
    /// Returns a human-readable runtime report suitable for logs and deployment diagnostics.
    /// Reading the report does not configure the runtime or load the native bridge.
    /// </summary>
    public static string GetDiagnosticReport()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var appLocalBridge = Path.Combine(baseDirectory, NativeLibraryFileName);
        var appLocalKernel = Path.Combine(baseDirectory, "TKernel.dll");
        var builder = new StringBuilder();
        builder.AppendLine($"Configured: {_configured}");
        builder.AppendLine($"Base directory: {baseDirectory}");
        builder.AppendLine($"App-local bridge: {(File.Exists(appLocalBridge) ? "[found]" : "[missing]")} {appLocalBridge}");
        builder.AppendLine($"App-local TKernel: {(File.Exists(appLocalKernel) ? "[found]" : "[missing]")} {appLocalKernel}");
        builder.AppendLine($"Native bridge directory: {ConfiguredNativeDirectory ?? "<not resolved>"}");
        builder.AppendLine($"OCCT root: {ConfiguredRoot ?? "<not resolved>"}");
        builder.AppendLine($"Repository probing: {_repositoryProbingEnabled}");
        builder.AppendLine($"OCCT_BRIDGE_NATIVE_DIR: {Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR") ?? "<unset>"}");
        builder.AppendLine($"OCCT_ROOT: {Environment.GetEnvironmentVariable("OCCT_ROOT") ?? "<unset>"}");
        builder.AppendLine($"CASROOT: {Environment.GetEnvironmentVariable("CASROOT") ?? "<unset>"}");
        builder.AppendLine("Native bridge candidates:");
        foreach (var candidate in GetNativeLibraryCandidatesCore())
        {
            builder.Append("  ").Append(File.Exists(candidate) ? "[found]   " : "[missing] ").AppendLine(candidate);
        }

        foreach (var variable in new[]
                 {
                     "CSF_OCCTResourcePath",
                     "CSF_SHMessage",
                     "CSF_XSMessage",
                     "CSF_STEPDefaults",
                     "CSF_IGESDefaults",
                     "CSF_ShadersDirectory"
                 })
        {
            builder.Append(variable).Append(": ").AppendLine(Environment.GetEnvironmentVariable(variable) ?? "<unset>");
        }

        return builder.ToString().TrimEnd();
    }

    private static string? DiagnosticGetEnvironmentPath(string variableName)
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

    private static bool? DiagnosticFileExistsOrNull(string? path) =>
        path is null ? null : File.Exists(path);

    private static string? DiagnosticTryFindLoadedRuntimeModule(string moduleName)
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
