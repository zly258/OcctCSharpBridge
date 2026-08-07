namespace OcctNet;

internal sealed class OcctRuntimeOptions
{
    internal string? OcctRoot { get; init; }
    internal string? NativeBridgeDirectory { get; init; }
    internal bool EnableRepositoryProbing { get; init; } = true;
    internal bool ThrowOnConfigurationConflict { get; init; } = true;
}
