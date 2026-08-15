namespace OcctNet;

public static partial class OcctRuntime
{
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
            Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeIdentifier, "native", NativeLibraryFileName)
        };

        if (!string.IsNullOrWhiteSpace(ConfiguredNativeDirectory))
            candidates.Add(Path.Combine(ConfiguredNativeDirectory, NativeLibraryFileName));

        var configuredDirectory = Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR");
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
            candidates.Add(Path.Combine(configuredDirectory, NativeLibraryFileName));

        var portableRuntimeDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "runtime"));
        candidates.Add(Path.Combine(portableRuntimeDirectory, NativeLibraryFileName));

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

    private static string? ResolveNativeBridgeDirectory(string? explicitDirectory)
    {
        if (!string.IsNullOrWhiteSpace(explicitDirectory))
            return Path.GetFullPath(explicitDirectory);

        var portableRuntimeDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "runtime"));

        foreach (var candidate in new[]
                 {
                     AppContext.BaseDirectory,
                     Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeIdentifier, "native"),
                     Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR"),
                     portableRuntimeDirectory
                 })
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;

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
            if (string.IsNullOrWhiteSpace(candidate)) continue;

            var fullPath = Path.GetFullPath(candidate);
            if (Directory.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    private static string? ResolveOcctKernelPath(string? occtRoot)
    {
        if (string.IsNullOrWhiteSpace(occtRoot)) return null;

        var candidates = OperatingSystem.IsWindows()
            ? new[] { Path.Combine(occtRoot, "win64", "vc14", "bin", OcctKernelFileName) }
            : new[]
            {
                Path.Combine(occtRoot, "lib", OcctKernelFileName),
                Path.Combine(occtRoot, "lib64", OcctKernelFileName)
            };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
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
}
