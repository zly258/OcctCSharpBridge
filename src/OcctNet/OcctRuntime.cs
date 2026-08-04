namespace OcctNet;

/// <summary>
/// Configures the fixed OCCT 7.9.0 runtime before the native bridge is loaded.
/// </summary>
public static class OcctRuntime
{
    private const string NativeLibraryFileName = "OcctNative.dll";
    private const string OcctRoot = @"D:\tools\occt-vc144-64";
    private const string OcctBinDirectory = @"D:\tools\occt-vc144-64\win64\vc14\bin";
    private const string OcctThirdPartyDirectory = @"D:\tools\occt-vc144-64\3rdparty-vc14-64";

    private static readonly object SyncRoot = new();
    private static bool _configured;

    public static void Configure()
    {
        lock (SyncRoot)
        {
            if (_configured)
            {
                return;
            }

            PrependPath(AppContext.BaseDirectory);
            PrependPath(OcctBinDirectory);
            AddThirdPartyRuntimePaths();
            SetIfMissing("CASROOT", OcctRoot);
            ConfigureResources(FindResourceDirectory(OcctRoot));
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

        var configuredDirectory = Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR");
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
        {
            candidates.Insert(0, Path.Combine(configuredDirectory, NativeLibraryFileName));
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

    private static void AddThirdPartyRuntimePaths()
    {
        if (!Directory.Exists(OcctThirdPartyDirectory))
        {
            return;
        }

        foreach (var componentDirectory in Directory.EnumerateDirectories(OcctThirdPartyDirectory)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            PrependPath(Path.Combine(componentDirectory, "bin", "x64"));
            PrependPath(Path.Combine(componentDirectory, "bin", "win64"));
            PrependPath(Path.Combine(componentDirectory, "bin"));
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
        SetDirectoryIfExists("CSF_TObjMessage", resourceDirectory, "TObj");
        SetDirectoryIfExists("CSF_StandardDefaults", resourceDirectory, "StdResource");
        SetDirectoryIfExists("CSF_PluginDefaults", resourceDirectory, "StdResource");
        SetDirectoryIfExists("CSF_XCAFDefaults", resourceDirectory, "XCAFResources");
        SetDirectoryIfExists("CSF_IGESDefaults", resourceDirectory, "XSTEPResource");
        SetDirectoryIfExists("CSF_STEPDefaults", resourceDirectory, "XSTEPResource");
        SetDirectoryIfExists("CSF_XmlOcafResource", resourceDirectory, "XmlOcafResource");
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
}
