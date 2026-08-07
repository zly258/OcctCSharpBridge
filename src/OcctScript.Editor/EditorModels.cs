namespace OcctScript.Editor;

internal sealed class CommandFieldRow
{
    public required string Name { get; init; }
    public string Expression { get; set; } = string.Empty;
    public string UnitType { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
}

internal sealed record BuildMessageRow(string Level, string Source, string Message);
