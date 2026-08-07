using System.Runtime.InteropServices;

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
        Configure(null, null);
    }

    /// <summary>
    /// Configures the runtime using explicit locations.
    /// Call this before creating the first <see cref="OcctEngine"/> instance.
    /// </summary>
    /// <param name="occtRoot">OCCT installation root. When omitted, the portable layout and environment variables are used.</param>
    /// <param name="nativeBridgeDirectory">Directory containing OcctNative.dll.</param>
    public static void Configure(string? occtRoot, string? nativeBridgeDirectory = null)
    {
        lock (SyncRoot)
        {
            if (_configured)
            {
                return;
            }

            InitializeNativeSearchPolicy();
            AddRuntimeSearchPath(AppContext.BaseDirectory);

            ConfiguredNativeDirectory = ResolveNativeBridgeDirectory(nativeBridgeDirectory);
            if (!string.IsNullOrWhiteSpace(ConfiguredNativeDirectory))
            {
                Environment.SetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR", ConfiguredNativeDirectory);
                AddRuntimeSearchPath(ConfiguredNativeDirectory);
            }

            ConfiguredRoot = ResolveOcctRoot(occtRoot);
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

    internal static IReadOnlyList<string> GetNativeLibraryCandidates()
    {
        Configure();

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

        var repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory);
        if (!string.IsNullOrWhiteSpace(repositoryRoot))
        {
            candidates.Add(Path.Combine(repositoryRoot, "build", "native", "bin", "Release", NativeLibraryFileName));
            candidates.Add(Path.Combine(repositoryRoot, "build", "native", "bin", "Debug", NativeLibraryFileName));
            candidates.Add(Path.Combine(repositoryRoot, "build", "native", "bin", "RelWithDebInfo", NativeLibraryFileName));
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string? ResolveNativeBridgeDirectory(string? explicitDirectory)
    {
        var appLocalNativeDirectory = AppContext.BaseDirectory;
        var appLocalRuntimeDirectory = Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native");
        var portableRuntimeDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "runtime"));

        foreach (var candidate in new[]
                 {
                     explicitDirectory,
                     appLocalNativeDirectory,
                     appLocalRuntimeDirectory,
                     Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR"),
                     portableRuntimeDirectory
                 })
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(Path.Combine(fullPath, NativeLibraryFileName)))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static string? ResolveOcctRoot(string? explicitRoot)
    {
        var portableOcctRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "occt"));

        foreach (var candidate in new[]
                 {
                     explicitRoot,
                     portableOcctRoot,
                     Environment.GetEnvironmentVariable("OCCT_ROOT"),
                     Environment.GetEnvironmentVariable("CASROOT")
                 })
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var fullPath = Path.GetFullPath(candidate);
            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static void InitializeNativeSearchPolicy()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _useNativeDirectoryApi = SetDefaultDllDirectories(LoadLibrarySearchDefaultDirs);
    }

    private static void AddRuntimeSearchPath(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        var fullPath = Path.GetFullPath(directory);
        PrependPath(fullPath);

        if (!OperatingSystem.IsWindows())
        {
            return;
        }

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
        {
            return;
        }

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
            {
                return directory.FullName;
            }

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
        {
            return;
        }

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
        {
            SetIfMissing(variableName, path);
        }
    }

    private static void SetFileIfExists(string variableName, params string[] parts)
    {
        var path = Path.Combine(parts);
        if (File.Exists(path))
        {
            SetIfMissing(variableName, path);
        }
    }

    private static void SetIfMissing(string variableName, string value)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variableName)))
        {
            Environment.SetEnvironmentVariable(variableName, value);
        }
    }

    private static void PrependPath(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

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
