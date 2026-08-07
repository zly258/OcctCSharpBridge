using System.Collections.ObjectModel;

namespace OcctScript.Domain;

[Flags]
public enum CommandTopologyKind
{
    None = 0,
    Vertex = 1 << 0,
    Edge = 1 << 1,
    Wire = 1 << 2,
    Face = 1 << 3,
    Shell = 1 << 4,
    Solid = 1 << 5,
    CompSolid = 1 << 6,
    Compound = 1 << 7,
    Curve = Edge | Wire,
    Surface = Face | Shell,
    Body = Solid | CompSolid,
    Any = Vertex | Edge | Wire | Face | Shell | Solid | CompSolid | Compound
}

public enum CommandFieldType
{
    Number,
    Integer,
    Boolean,
    Text,
    Enum,
    Point3,
    Vector3,
    PointList,
    Expression,
    CommandReference,
    CommandReferenceList
}

public sealed record CommandFieldDefinition(
    string Name,
    string DisplayNameKey,
    CommandFieldType FieldType,
    string DefaultValue = "",
    bool IsRequired = false,
    string UnitType = "",
    CommandTopologyKind AcceptedTopology = CommandTopologyKind.Any,
    int MinReferences = 0,
    int MaxReferences = 0,
    IReadOnlyList<string>? Choices = null)
{
    public IReadOnlyList<string> AvailableChoices { get; } =
        new ReadOnlyCollection<string>((Choices ?? Array.Empty<string>()).ToArray());
}

public sealed record CommandDefinition(
    string Type,
    string DisplayNameKey,
    string DescriptionKey,
    string CategoryKey,
    int Order,
    CommandTopologyKind OutputTopology,
    IReadOnlyList<CommandFieldDefinition> Fields);

public sealed class CommandRegistry
{
    private readonly Dictionary<string, CommandDefinition> definitions = new(StringComparer.Ordinal);

    public void Register(CommandDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (string.IsNullOrWhiteSpace(definition.Type))
            throw new ArgumentException("Command type must not be empty.", nameof(definition));
        if (!definitions.TryAdd(definition.Type, definition))
            throw new InvalidOperationException($"Command type '{definition.Type}' is already registered.");
    }

    public bool TryGet(string type, out CommandDefinition? definition) =>
        definitions.TryGetValue(type, out definition);

    public CommandDefinition GetRequired(string type) =>
        definitions.TryGetValue(type, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Command type '{type}' is not registered.");

    public IReadOnlyCollection<CommandDefinition> GetAll() =>
        definitions.Values.OrderBy(x => x.Order).ThenBy(x => x.Type, StringComparer.Ordinal).ToArray();
}
