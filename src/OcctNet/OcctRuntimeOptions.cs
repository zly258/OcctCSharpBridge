namespace OcctNet;

/// <summary>
/// Controls one-time OCCT runtime discovery and native library probing.
/// Configure the runtime before creating the first engine or modeling session.
/// </summary>
public sealed class OcctRuntimeOptions
{
    /// <summary>
    /// Gets or initializes the OCCT installation root. The directory must contain the expected OCCT runtime layout.
    /// </summary>
    public string? OcctRoot { get; init; }

    /// <summary>
    /// Gets or initializes the directory containing OcctNative.dll.
    /// </summary>
    public string? NativeBridgeDirectory { get; init; }

    /// <summary>
    /// Gets or initializes whether development-time repository build outputs may be probed for OcctNative.dll.
    /// Portable applications normally leave this enabled; published packages resolve app-local native files first.
    /// </summary>
    public bool EnableRepositoryProbing { get; init; } = true;

    /// <summary>
    /// Gets or initializes whether a second conflicting runtime configuration should throw instead of being ignored.
    /// </summary>
    public bool ThrowOnConfigurationConflict { get; init; } = true;
}
