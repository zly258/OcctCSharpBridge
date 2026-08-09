namespace OcctDemo.Common;

internal sealed record DemoHistoryEntry(
    DemoCommandId? CommandId,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlyList<long> SelectedObjectIds,
    string Description,
    string? ImportFilePath = null)
{
    public bool IsImport => !string.IsNullOrWhiteSpace(ImportFilePath);

    public static DemoHistoryEntry Command(
        DemoCommandId commandId,
        IReadOnlyDictionary<string, string> values,
        IReadOnlyList<long> selectedObjectIds,
        string description) => new(commandId, values, selectedObjectIds, description);

    public static DemoHistoryEntry Import(string filePath, string description) => new(
        null,
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
        Array.Empty<long>(),
        description,
        Path.GetFullPath(filePath));
}
