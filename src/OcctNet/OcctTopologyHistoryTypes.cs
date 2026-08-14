namespace OcctNet;

/// <summary>
/// Aggregate topology lineage for one source shape in a modeling operation.
/// </summary>
public readonly record struct OcctTopologyHistorySummary(
    int GeneratedCount,
    int ModifiedCount,
    bool Removed);

[StructLayout(LayoutKind.Sequential)]
internal struct NativeModelTopologyHistorySummary
{
    internal int GeneratedCount;
    internal int ModifiedCount;
    internal int Removed;

    internal readonly OcctTopologyHistorySummary ToManaged() =>
        new(GeneratedCount, ModifiedCount, Removed != 0);
}
