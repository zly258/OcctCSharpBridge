namespace OcctScript.Domain;

public static class BuiltInCommandCatalog
{
    public const string Vertex = "Vertex";
    public const string Line = "Line";
    public const string Polyline = "Polyline";
    public const string Circle = "Circle";
    public const string Arc = "Arc";
    public const string Rectangle = "Rectangle";
    public const string Wire = "Wire";
    public const string Face = "Face";
    public const string PlaneFace = "PlaneFace";
    public const string Box = "Box";
    public const string Cylinder = "Cylinder";
    public const string Extrude = "Extrude";
    public const string Revolve = "Revolve";
    public const string Sweep = "Sweep";
    public const string Loft = "Loft";
    public const string Fuse = "Fuse";
    public const string Cut = "Cut";
    public const string Common = "Common";
    public const string Section = "Section";

    public static void RegisterAll(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.Register(Definition(Vertex, "Command.Vertex", "Description.Vertex", "Category.Curves", 10,
            CommandTopologyKind.Vertex,
            Field("point", "Field.Point", CommandFieldType.Point3, "0, 0, 0", true, "length")));

        registry.Register(Definition(Line, "Command.Line", "Description.Line", "Category.Curves", 20,
            CommandTopologyKind.Edge,
            Field("start", "Field.Start", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("end", "Field.End", CommandFieldType.Point3, "1000, 0, 0", true, "length")));

        registry.Register(Definition(Polyline, "Command.Polyline", "Description.Polyline", "Category.Curves", 30,
            CommandTopologyKind.Wire,
            Field("points", "Field.Points", CommandFieldType.PointList, "0, 0, 0; 1000, 0, 0; 1000, 800, 0", true, "length"),
            Field("closed", "Field.Closed", CommandFieldType.Boolean, "false")));

        registry.Register(Definition(Circle, "Command.Circle", "Description.Circle", "Category.Curves", 40,
            CommandTopologyKind.Edge,
            Field("center", "Field.Center", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("radius", "Field.Radius", CommandFieldType.Expression, "250", true, "length")));

        registry.Register(Definition(Arc, "Command.Arc", "Description.Arc", "Category.Curves", 50,
            CommandTopologyKind.Edge,
            Field("start", "Field.Start", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("middle", "Field.Middle", CommandFieldType.Point3, "500, 250, 0", true, "length"),
            Field("end", "Field.End", CommandFieldType.Point3, "1000, 0, 0", true, "length")));

        registry.Register(Definition(Rectangle, "Command.Rectangle", "Description.Rectangle", "Category.Curves", 60,
            CommandTopologyKind.Wire,
            Field("origin", "Field.Origin", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("xDirection", "Field.XDirection", CommandFieldType.Vector3, "1, 0, 0", true),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("width", "Field.Width", CommandFieldType.Expression, "1000", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "800", true, "length")));

        registry.Register(Definition(Wire, "Command.Wire", "Description.Wire", "Category.Curves", 70,
            CommandTopologyKind.Wire,
            ReferenceList("curves", "Field.Curves", CommandTopologyKind.Curve, 1)));

        registry.Register(Definition(Face, "Command.Face", "Description.Face", "Category.Surfaces", 100,
            CommandTopologyKind.Face,
            Reference("profile", "Field.Profile", CommandTopologyKind.Curve)));

        registry.Register(Definition(PlaneFace, "Command.PlaneFace", "Description.PlaneFace", "Category.Surfaces", 110,
            CommandTopologyKind.Face,
            Field("origin", "Field.Origin", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("xDirection", "Field.XDirection", CommandFieldType.Vector3, "1, 0, 0", true),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("width", "Field.Width", CommandFieldType.Expression, "1000", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "800", true, "length")));

        registry.Register(Definition(Box, "Command.Box", "Description.Box", "Category.Solids", 200,
            CommandTopologyKind.Solid,
            Field("width", "Field.Width", CommandFieldType.Expression, "1000", true, "length"),
            Field("depth", "Field.Depth", CommandFieldType.Expression, "800", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "500", true, "length")));

        registry.Register(Definition(Cylinder, "Command.Cylinder", "Description.Cylinder", "Category.Solids", 210,
            CommandTopologyKind.Solid,
            Field("radius", "Field.Radius", CommandFieldType.Expression, "250", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "500", true, "length"),
            Field("axis", "Field.Axis", CommandFieldType.Vector3, "0, 0, 1", true)));

        registry.Register(Definition(Extrude, "Command.Extrude", "Description.Extrude", "Category.Features", 300,
            CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            Reference("profile", "Field.Profile", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face),
            Field("direction", "Field.Direction", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("distance", "Field.Distance", CommandFieldType.Expression, "500", true, "length")));

        registry.Register(Definition(Revolve, "Command.Revolve", "Description.Revolve", "Category.Features", 310,
            CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            Reference("profile", "Field.Profile", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face),
            Field("axisPoint", "Field.AxisPoint", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("axisDirection", "Field.AxisDirection", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("angle", "Field.Angle", CommandFieldType.Expression, "360", true, "angle")));

        registry.Register(Definition(Sweep, "Command.Sweep", "Description.Sweep", "Category.Features", 320,
            CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            Reference("spine", "Field.Spine", CommandTopologyKind.Curve),
            Reference("profile", "Field.Profile", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face)));

        registry.Register(Definition(Loft, "Command.Loft", "Description.Loft", "Category.Features", 330,
            CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            ReferenceList("sections", "Field.Sections", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face, 2),
            Field("makeSolid", "Field.MakeSolid", CommandFieldType.Boolean, "true"),
            Field("ruled", "Field.Ruled", CommandFieldType.Boolean, "false"),
            Field("tolerance", "Field.Tolerance", CommandFieldType.Expression, "0.000001", true, "length")));

        registry.Register(BooleanDefinition(Fuse, "Command.Fuse", "Description.Fuse", 400, CommandTopologyKind.Any));
        registry.Register(BooleanDefinition(Cut, "Command.Cut", "Description.Cut", 410, CommandTopologyKind.Any));
        registry.Register(BooleanDefinition(Common, "Command.Common", "Description.Common", 420, CommandTopologyKind.Any));
        registry.Register(BooleanDefinition(Section, "Command.Section", "Description.Section", 430,
            CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Compound));
    }

    public static ScriptCommand CreateDefault(CommandDefinition definition, int order = 0)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var command = new ScriptCommand
        {
            Type = definition.Type,
            Name = definition.Type,
            Order = order,
            Display = new DisplayDefinition { Color = DefaultColor(definition.CategoryKey) }
        };

        foreach (var field in definition.Fields)
        {
            var value = new CommandValue();
            if (field.FieldType is CommandFieldType.Expression or CommandFieldType.Number or CommandFieldType.Integer)
                value.Expression = field.DefaultValue;
            else if (field.FieldType is not (CommandFieldType.CommandReference or CommandFieldType.CommandReferenceList))
                value.Literal = field.DefaultValue;
            command.Fields[field.Name] = value;
        }

        return command;
    }

    private static CommandDefinition Definition(
        string type,
        string displayNameKey,
        string descriptionKey,
        string categoryKey,
        int order,
        CommandTopologyKind outputTopology,
        params CommandFieldDefinition[] fields) =>
        new(type, displayNameKey, descriptionKey, categoryKey, order, outputTopology, fields);

    private static CommandDefinition BooleanDefinition(
        string type,
        string displayNameKey,
        string descriptionKey,
        int order,
        CommandTopologyKind outputTopology) =>
        Definition(type, displayNameKey, descriptionKey, "Category.Boolean", order, outputTopology,
            Reference("left", "Field.Left", CommandTopologyKind.Any),
            Reference("right", "Field.Right", CommandTopologyKind.Any),
            Field("fuzzyValue", "Field.FuzzyValue", CommandFieldType.Expression, "0", false, "length"));

    private static CommandFieldDefinition Field(
        string name,
        string displayNameKey,
        CommandFieldType type,
        string defaultValue = "",
        bool required = false,
        string unitType = "") =>
        new(name, displayNameKey, type, defaultValue, required, unitType);

    private static CommandFieldDefinition Reference(
        string name,
        string displayNameKey,
        CommandTopologyKind acceptedTopology) =>
        new(name, displayNameKey, CommandFieldType.CommandReference, "", true, "", acceptedTopology, 1, 1);

    private static CommandFieldDefinition ReferenceList(
        string name,
        string displayNameKey,
        CommandTopologyKind acceptedTopology,
        int minReferences) =>
        new(name, displayNameKey, CommandFieldType.CommandReferenceList, "", true, "", acceptedTopology, minReferences, 0);

    private static string DefaultColor(string categoryKey) => categoryKey switch
    {
        "Category.Curves" => "#2563EB",
        "Category.Surfaces" => "#38BDF8",
        "Category.Features" => "#A78BFA",
        "Category.Boolean" => "#F59E0B",
        _ => "#94A3B8"
    };
}
