namespace OcctNet;

/// <summary>
/// Configures the OCCT runtime before the native bridge is loaded.
/// </summary>
public static partial class OcctRuntime
{
    private const string NativeLibraryFileName = "OcctNative.dll";

    private static readonly object SyncRoot = new();
    private static bool _configured;
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
}
