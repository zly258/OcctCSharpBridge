using OcctScript.Domain;

namespace OcctScript.Editor;

internal sealed record CommandCatalogItem(
    CommandDefinition Definition,
    string DisplayName,
    string Category,
    string Description)
{
    public string Type => Definition.Type;
    public string DisplayText => $"{Category} · {DisplayName}";
}

internal sealed class CommandFieldRow
{
    public required CommandFieldDefinition Definition { get; init; }
    public required string DisplayName { get; init; }
    public string ValueText { get; set; } = string.Empty;
    public string TypeText { get; init; } = string.Empty;
    public string UnitText { get; init; } = string.Empty;
    public string Hint { get; init; } = string.Empty;
}

internal sealed record BuildMessageRow(string Level, string Source, string Message);
