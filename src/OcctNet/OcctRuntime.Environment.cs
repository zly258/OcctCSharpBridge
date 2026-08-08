using System.Runtime.InteropServices;

namespace OcctNet;

public static partial class OcctRuntime
{
    private const uint LoadLibrarySearchDefaultDirs = 0x00001000;

    private static readonly List<IntPtr> NativeDirectoryCookies = new();
    private static bool _useNativeDirectoryApi;

    private static void InitializeNativeSearchPolicy()
    {
        if (!OperatingSystem.IsWindows()) return;
        _useNativeDirectoryApi = SetDefaultDllDirectories(LoadLibrarySearchDefaultDirs);
    }

    private static void AddRuntimeSearchPath(string directory)
    {
        if (!Directory.Exists(directory)) return;

        var fullPath = Path.GetFullPath(directory);
        PrependPath(fullPath);

        if (!OperatingSystem.IsWindows()) return;

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
        if (!Directory.Exists(thirdPartyDirectory)) return;

        foreach (var componentDirectory in Directory.EnumerateDirectories(thirdPartyDirectory)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            AddRuntimeSearchPath(Path.Combine(componentDirectory, "bin", "x64"));
            AddRuntimeSearchPath(Path.Combine(componentDirectory, "bin", "win64"));
            AddRuntimeSearchPath(Path.Combine(componentDirectory, "bin"));
        }
    }

    private static void ConfigureResources(string? resourceDirectory)
    {
        if (string.IsNullOrWhiteSpace(resourceDirectory) || !Directory.Exists(resourceDirectory)) return;

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
        if (Directory.Exists(path)) SetIfMissing(variableName, path);
    }

    private static void SetFileIfExists(string variableName, params string[] parts)
    {
        var path = Path.Combine(parts);
        if (File.Exists(path)) SetIfMissing(variableName, path);
    }

    private static void SetIfMissing(string variableName, string value)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variableName)))
            Environment.SetEnvironmentVariable(variableName, value);
    }

    private static void PrependPath(string directory)
    {
        if (!Directory.Exists(directory)) return;

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
