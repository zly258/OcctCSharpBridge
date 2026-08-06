namespace OcctScript.Domain;

public enum ScriptParameterType
{
    Number,
    Length,
    Angle,
    Integer,
    Boolean,
    Text
}

public sealed class ScriptParameter
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ScriptParameterType Type { get; set; } = ScriptParameterType.Number;
    public string Expression { get; set; } = "0";
    public string Unit { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
    public bool IsHidden { get; set; }
}
