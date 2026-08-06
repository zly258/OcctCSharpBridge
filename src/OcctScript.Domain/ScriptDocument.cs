namespace OcctScript.Domain;

public sealed class ScriptDocument
{
    public const string CurrentFormat = "OcctScript.Document";
    public const int CurrentVersion = 1;

    public string Format { get; init; } = CurrentFormat;
    public int Version { get; init; } = CurrentVersion;
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Untitled";
    public string Description { get; set; } = string.Empty;
    public string LengthUnit { get; set; } = "mm";
    public string AngleUnit { get; set; } = "deg";
    public List<ScriptParameter> Parameters { get; init; } = [];
    public List<ScriptCommand> Commands { get; init; } = [];
    public List<Guid> OutputCommandIds { get; init; } = [];

    public ScriptParameter? FindParameter(Guid id) => Parameters.FirstOrDefault(x => x.Id == id);
    public ScriptCommand? FindCommand(Guid id) => Commands.FirstOrDefault(x => x.Id == id);
}
