namespace OcctDemo.Common;

internal sealed record DemoHistoryEntry(
    DemoCommandId? CommandId,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlyList<long> SelectedObjectIds,
    string Description,
    string? ImportFilePath = null,
    string? ModelingTestId = null)
{
    public bool IsImport => !string.IsNullOrWhiteSpace(ImportFilePath);
    public bool IsModelingTest => !string.IsNullOrWhiteSpace(ModelingTestId);

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

    public static DemoHistoryEntry ModelingTest(string testId, string description) => new(
        null,
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
        Array.Empty<long>(),
        description,
        null,
        testId);
}
