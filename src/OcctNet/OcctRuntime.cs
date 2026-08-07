using System.Runtime.InteropServices;
using System.Text;

namespace OcctNet;

/// <summary>
/// Configures the OCCT runtime before the native bridge is loaded.
/// </summary>
public static class OcctRuntime
{
    private const string NativeLibraryFileName = "OcctNative.dll";
    private const uint LoadLibrarySearchDefaultDirs = 0x00001000;

    private static readonly object SyncRoot = new();
    private static readonly List<IntPtr> NativeDirectoryCookies = new();
    private static bool _configured;
    private static bool _useNativeDirectoryApi;
    private static bool _repositoryProbingEnabled = true;

    /// <summary>
    /// Gets the OCCT root selected during runtime configuration.
    /// </summary>
    public static string? ConfiguredRoot { get; private set; }

    /// <summary>
    /// Gets the directory containing the configured native bridge.
    /// </summary>
    public static string? ConfiguredNativeDirectory { get; private set; }

    /// <summary>
    /// Configures the runtime using the portable package layout, OCCT_ROOT, or CASROOT.
    /// </summary>
    public static void Configure()
    {
        lock (SyncRoot)
        {
            if (_configured) return;
        }

        Configure(new OcctRuntimeOptions());
    }

    /// <summary>
    /// Configures the runtime using explicit locations.
    /// Call this before creating the first <see cref="OcctEngine"/> or <see cref="OcctModelingSession"/> instance.
    /// </summary>
    public static void Configure(string? occtRoot, string? nativeBridgeDirectory = null) =>
        Configure(new OcctRuntimeOptions
        {
            OcctRoot = occtRoot,
            NativeBridgeDirectory = nativeBridgeDirectory
        });

    internal static void Configure(OcctRuntimeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (SyncRoot)
        {
            if (_configured)
            {
                ValidateReconfiguration(options);
                return;
            }

            ValidateExplicitConfiguration(options);
            _repositoryProbingEnabled = options.EnableRepositoryProbing;

            InitializeNativeSearchPolicy();
            AddRuntimeSearchPath(AppContext.BaseDirectory);

            ConfiguredNativeDirectory = ResolveNativeBridgeDirectory(options.NativeBridgeDirectory);
            if (!string.IsNullOrWhiteSpace(ConfiguredNativeDirectory))
            {
                Environment.SetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR", ConfiguredNativeDirectory);
                AddRuntimeSearchPath(ConfiguredNativeDirectory);
            }

            ConfiguredRoot = ResolveOcctRoot(options.OcctRoot);
            if (!string.IsNullOrWhiteSpace(ConfiguredRoot))
            {
                var occtBinDirectory = Path.Combine(ConfiguredRoot, "win64", "vc14", "bin");
                var thirdPartyDirectory = Path.Combine(ConfiguredRoot, "3rdparty-vc14-64");

                AddRuntimeSearchPath(occtBinDirectory);
                AddThirdPartyRuntimePaths(thirdPartyDirectory);
                SetIfMissing("OCCT_ROOT", ConfiguredRoot);
                SetIfMissing("CASROOT", ConfiguredRoot);
                ConfigureResources(FindResourceDirectory(ConfiguredRoot));
            }

            _configured = true;
        }
    }

    /// <summary>
    /// Returns a human-readable runtime report suitable for logs and deployment diagnostics.
    /// </summary>
    public static string GetDiagnosticReport()
    {
        Configure();

        var builder = new StringBuilder();
        builder.AppendLine($"Configured: {_configured}");
        builder.AppendLine($"Base directory: {AppContext.BaseDirectory}");
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

    internal static IReadOnlyList<string> GetNativeLibraryCandidates()
    {
        Configure();
        return GetNativeLibraryCandidatesCore();
    }

    private static IReadOnlyList<string> GetNativeLibraryCandidatesCore()
    {
        var candidates = new List<string>
        {
            Path.Combine(AppContext.BaseDirectory, NativeLibraryFileName),
            Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native", NativeLibraryFileName)
        };

        if (!string.IsNullOrWhiteSpace(ConfiguredNativeDirectory))
        {
            candidates.Add(Path.Combine(ConfiguredNativeDirectory, NativeLibraryFileName));
        }

        var configuredDirectory = Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR");
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
        {
            candidates.Add(Path.Combine(configuredDirectory, NativeLibraryFileName));
        }

        if (_repositoryProbingEnabled)
        {
            var repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory);
            if (!string.IsNullOrWhiteSpace(repositoryRoot))
            {
                candidates.Add(Path.Combine(repositoryRoot, "build", "native", "bin", "Release", NativeLibraryFileName));
                candidates.Add(Path.Combine(repositoryRoot, "build", "native", "bin", "Debug", NativeLibraryFileName));
                candidates.Add(Path.Combine(repositoryRoot, "build", "native", "bin", "RelWithDebInfo", NativeLibraryFileName));
            }
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void ValidateExplicitConfiguration(OcctRuntimeOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.NativeBridgeDirectory))
        {
            var directory = Path.GetFullPath(options.NativeBridgeDirectory);
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Native bridge directory was not found: {directory}");
            var bridge = Path.Combine(directory, NativeLibraryFileName);
            if (!File.Exists(bridge))
                throw new FileNotFoundException($"{NativeLibraryFileName} was not found in the configured native bridge directory.", bridge);
        }

        if (!string.IsNullOrWhiteSpace(options.OcctRoot))
        {
            var root = Path.GetFullPath(options.OcctRoot);
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException($"OCCT root was not found: {root}");
        }
    }

    private static void ValidateReconfiguration(OcctRuntimeOptions options)
    {
        if (!options.ThrowOnConfigurationConflict) return;

        if (!string.IsNullOrWhiteSpace(options.NativeBridgeDirectory))
        {
            var requested = Path.GetFullPath(options.NativeBridgeDirectory);
            if (!PathsEqual(requested, ConfiguredNativeDirectory))
                throw new InvalidOperationException($"OCCT runtime is already configured with native bridge directory '{ConfiguredNativeDirectory ?? "<none>"}', not '{requested}'.");
        }

        if (!string.IsNullOrWhiteSpace(options.OcctRoot))
        {
            var requested = Path.GetFullPath(options.OcctRoot);
            if (!PathsEqual(requested, ConfiguredRoot))
                throw new InvalidOperationException($"OCCT runtime is already configured with root '{ConfiguredRoot ?? "<none>"}', not '{requested}'.");
        }

        if (options.EnableRepositoryProbing != _repositoryProbingEnabled &&
            (!string.IsNullOrWhiteSpace(options.OcctRoot) || !string.IsNullOrWhiteSpace(options.NativeBridgeDirectory)))
        {
            throw new InvalidOperationException("OCCT runtime repository probing policy cannot be changed after configuration.");
        }
    }

    private static bool PathsEqual(string left, string? right) =>
        !string.IsNullOrWhiteSpace(right) && string.Equals(
            left.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);

    private static string? ResolveNativeBridgeDirectory(string? explicitDirectory)
    {
        if (!string.IsNullOrWhiteSpace(explicitDirectory))
            return Path.GetFullPath(explicitDirectory);

        var appLocalNativeDirectory = AppContext.BaseDirectory;
        var appLocalRuntimeDirectory = Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native");
        var portableRuntimeDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "runtime"));

        foreach (var candidate in new[]
                 {
                     appLocalNativeDirectory,
                     appLocalRuntimeDirectory,
                     Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR"),
                     portableRuntimeDirectory
                 })
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(Path.Combine(fullPath, NativeLibraryFileName)))
                return fullPath;
        }

        return null;
    }

    private static string? ResolveOcctRoot(string? explicitRoot)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
            return Path.GetFullPath(explicitRoot);

        var portableOcctRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "occt"));

        foreach (var candidate in new[]
                 {
                     portableOcctRoot,
                     Environment.GetEnvironmentVariable("OCCT_ROOT"),
                     Environment.GetEnvironmentVariable("CASROOT")
                 })
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var fullPath = Path.GetFullPath(candidate);
            if (Directory.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    private static void InitializeNativeSearchPolicy()
    {
        if (!OperatingSystem.IsWindows())
            return;

        _useNativeDirectoryApi = SetDefaultDllDirectories(LoadLibrarySearchDefaultDirs);
    }

    private static void AddRuntimeSearchPath(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        var fullPath = Path.GetFullPath(directory);
        PrependPath(fullPath);

        if (!OperatingSystem.IsWindows())
            return;

        if (_useNativeDirectoryApi)
        {
            var cookie = AddDllDirectory(fullPath);
            if (cookie != IntPtr.Zero)
            {
                NativeDirectoryCookies.Add(cookie);
                return;
            }
        }

        SetDllDirectory(fullPath);
    }

    private static void AddThirdPartyRuntimePaths(string thirdPartyDirectory)
    {
        if (!Directory.Exists(thirdPartyDirectory))
            return;

        foreach (var componentDirectory in Directory.EnumerateDirectories(thirdPartyDirectory)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            AddRuntimeSearchPath(Path.Combine(componentDirectory, "bin", "x64"));
            AddRuntimeSearchPath(Path.Combine(componentDirectory, "bin", "win64"));
            AddRuntimeSearchPath(Path.Combine(componentDirectory, "bin"));
        }
    }

    private static string? FindRepositoryRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "OcctBridge.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        return null;
    }

    private static string? FindResourceDirectory(string casRoot)
    {
        var candidates = new[]
        {
            Path.Combine(casRoot, "src"),
            Path.Combine(casRoot, "share", "opencascade", "resources"),
            Path.Combine(casRoot, "resources")
        };

        return candidates.FirstOrDefault(Directory.Exists);
    }

    private static void ConfigureResources(string? resourceDirectory)
    {
        if (string.IsNullOrWhiteSpace(resourceDirectory) || !Directory.Exists(resourceDirectory))
            return;

        SetIfMissing("CSF_OCCTResourcePath", resourceDirectory);
        SetDirectoryIfExists("CSF_SHMessage", resourceDirectory, "SHMessage");
        SetDirectoryIfExists("CSF_XSMessage", resourceDirectory, "XSMessage");
        SetDirectoryIfExists("CSF_StandardDefaults", resourceDirectory, "StdResource");
        SetDirectoryIfExists("CSF_PluginDefaults", resourceDirectory, "StdResource");
        SetDirectoryIfExists("CSF_IGESDefaults", resourceDirectory, "XSTEPResource");
        SetDirectoryIfExists("CSF_STEPDefaults", resourceDirectory, "XSTEPResource");
        SetDirectoryIfExists("CSF_ShadersDirectory", resourceDirectory, "Shaders");
        SetDirectoryIfExists("CSF_MDTVTexturesDirectory", resourceDirectory, "Textures");
        SetFileIfExists("CSF_UnitsLexicon", resourceDirectory, "UnitsAPI", "Lexi_Expr.dat");
        SetFileIfExists("CSF_UnitsDefinition", resourceDirectory, "UnitsAPI", "Units.dat");
    }

    private static void SetDirectoryIfExists(string variableName, params string[] parts)
    {
        var path = Path.Combine(parts);
        if (Directory.Exists(path))
            SetIfMissing(variableName, path);
    }

    private static void SetFileIfExists(string variableName, params string[] parts)
    {
        var path = Path.Combine(parts);
        if (File.Exists(path))
            SetIfMissing(variableName, path);
    }

    private static void SetIfMissing(string variableName, string value)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variableName)))
            Environment.SetEnvironmentVariable(variableName, value);
    }

    private static void PrependPath(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var entries = currentPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (entries.Any(entry => string.Equals(
                entry.TrimEnd(Path.DirectorySeparatorChar),
                directory.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        Environment.SetEnvironmentVariable("PATH", directory + Path.PathSeparator + currentPath);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDefaultDllDirectories(uint directoryFlags);

    [DllImport("kernel32.dll", EntryPoint = "AddDllDirectory", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr AddDllDirectory(string newDirectory);

    [DllImport("kernel32.dll", EntryPoint = "SetDllDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDllDirectory(string pathName);
}
