namespace OcctScript.Domain;

public enum CommandFieldType
{
    Number,
    Integer,
    Boolean,
    Text,
    Enum,
    Point3,
    Vector3,
    Expression,
    CommandReference,
    CommandReferenceList
}

public sealed record CommandFieldDefinition(
    string Name,
    CommandFieldType FieldType,
    string DefaultValue = "",
    bool IsRequired = false,
    string UnitType = "");

public sealed record CommandDefinition(
    string Type,
    string DisplayNameKey,
    string CategoryKey,
    int Order,
    IReadOnlyList<CommandFieldDefinition> Fields);

public sealed class CommandRegistry
{
    private readonly Dictionary<string, CommandDefinition> definitions = new(StringComparer.Ordinal);

    public void Register(CommandDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!definitions.TryAdd(definition.Type, definition))
        {
            throw new InvalidOperationException($"Command type '{definition.Type}' is already registered.");
        }
    }

    public CommandDefinition GetRequired(string type) =>
        definitions.TryGetValue(type, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Command type '{type}' is not registered.");

    public IReadOnlyCollection<CommandDefinition> GetAll() =>
        definitions.Values.OrderBy(x => x.Order).ToArray();
}
