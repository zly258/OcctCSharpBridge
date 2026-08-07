namespace OcctScript.Domain;

public static class BuiltInCommandCatalog
{
    public const string Box = "Box";
    public const string Cylinder = "Cylinder";

    public static void RegisterAll(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.Register(new CommandDefinition(
            Box,
            "Command.Box",
            "Category.Primitives",
            100,
            [
                new CommandFieldDefinition("width", CommandFieldType.Expression, "1000", true, "length"),
                new CommandFieldDefinition("depth", CommandFieldType.Expression, "800", true, "length"),
                new CommandFieldDefinition("height", CommandFieldType.Expression, "500", true, "length")
            ]));

        registry.Register(new CommandDefinition(
            Cylinder,
            "Command.Cylinder",
            "Category.Primitives",
            110,
            [
                new CommandFieldDefinition("radius", CommandFieldType.Expression, "250", true, "length"),
                new CommandFieldDefinition("height", CommandFieldType.Expression, "500", true, "length")
            ]));
    }

    public static ScriptCommand CreateDefault(CommandDefinition definition, int order = 0)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var command = new ScriptCommand
        {
            Type = definition.Type,
            Name = definition.Type,
            Order = order
        };

        foreach (var field in definition.Fields)
        {
            command.Fields[field.Name] = new CommandValue { Expression = field.DefaultValue };
        }

        return command;
    }
}
