using OcctScript.Domain;

namespace OcctScript.Geometry;

public static class BuiltInCommandDefinitions
{
    public static void RegisterAll(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.Register(new CommandDefinition(
            "Box",
            "Command.Box",
            "Category.Primitives",
            100,
            [
                new("width", CommandFieldType.Expression, "100", true, "length"),
                new("depth", CommandFieldType.Expression, "100", true, "length"),
                new("height", CommandFieldType.Expression, "100", true, "length")
            ]));
        registry.Register(new CommandDefinition(
            "Cylinder",
            "Command.Cylinder",
            "Category.Primitives",
            110,
            [
                new("radius", CommandFieldType.Expression, "50", true, "length"),
                new("height", CommandFieldType.Expression, "100", true, "length")
            ]));
        registry.Register(new CommandDefinition(
            "Cut",
            "Command.Cut",
            "Category.Boolean",
            300,
            [
                new("base", CommandFieldType.CommandReference, IsRequired: true),
                new("tool", CommandFieldType.CommandReference, IsRequired: true)
            ]));
    }

    public static ScriptCommand Create(string type, string name, CommandRegistry registry)
    {
        var definition = registry.GetRequired(type);
        var command = new ScriptCommand { Type = type, Name = name };
        foreach (var field in definition.Fields)
            command.Fields[field.Name] = new CommandValue { Expression = field.DefaultValue };
        return command;
    }
}
