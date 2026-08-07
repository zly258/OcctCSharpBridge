namespace OcctScript.Domain;

public static class BuiltInCommandCatalog
{
    public const string Vertex = "Vertex";
    public const string Line = "Line";
    public const string Polyline = "Polyline";
    public const string Circle = "Circle";
    public const string Arc = "Arc";
    public const string Ellipse = "Ellipse";
    public const string RegularPolygon = "RegularPolygon";
    public const string Bezier = "Bezier";
    public const string BSpline = "BSpline";
    public const string Rectangle = "Rectangle";
    public const string Wire = "Wire";

    public const string Face = "Face";
    public const string PlaneFace = "PlaneFace";

    public const string Box = "Box";
    public const string Cylinder = "Cylinder";
    public const string Cone = "Cone";
    public const string Sphere = "Sphere";
    public const string Torus = "Torus";
    public const string Wedge = "Wedge";
    public const string Compound = "Compound";
    public const string Sew = "Sew";
    public const string SolidFromShell = "SolidFromShell";

    public const string Extrude = "Extrude";
    public const string Revolve = "Revolve";
    public const string Sweep = "Sweep";
    public const string Loft = "Loft";
    public const string Fillet = "Fillet";
    public const string Chamfer = "Chamfer";
    public const string Offset = "Offset";
    public const string Shell = "Shell";

    public const string Fuse = "Fuse";
    public const string Cut = "Cut";
    public const string Common = "Common";
    public const string Section = "Section";

    public const string Move = "Move";
    public const string RotateShape = "RotateShape";
    public const string ScaleShape = "ScaleShape";
    public const string Mirror = "Mirror";

    public static void RegisterAll(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RegisterCurves(registry);
        RegisterSurfaces(registry);
        RegisterSolids(registry);
        RegisterFeatures(registry);
        RegisterBooleans(registry);
        RegisterTransforms(registry);
    }

    private static void RegisterCurves(CommandRegistry registry)
    {
        registry.Register(Definition(Vertex, "Command.Vertex", "Description.Vertex", "Category.Curves", 10, CommandTopologyKind.Vertex,
            Field("point", "Field.Point", CommandFieldType.Point3, "0, 0, 0", true, "length")));
        registry.Register(Definition(Line, "Command.Line", "Description.Line", "Category.Curves", 20, CommandTopologyKind.Edge,
            Field("start", "Field.Start", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("end", "Field.End", CommandFieldType.Point3, "1000, 0, 0", true, "length")));
        registry.Register(Definition(Polyline, "Command.Polyline", "Description.Polyline", "Category.Curves", 30, CommandTopologyKind.Wire,
            Field("points", "Field.Points", CommandFieldType.PointList, "0,0,0; 1000,0,0; 1000,800,0", true, "length"),
            Field("closed", "Field.Closed", CommandFieldType.Boolean, "false")));
        registry.Register(Definition(Circle, "Command.Circle", "Description.Circle", "Category.Curves", 40, CommandTopologyKind.Edge,
            Field("center", "Field.Center", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("radius", "Field.Radius", CommandFieldType.Expression, "250", true, "length")));
        registry.Register(Definition(Arc, "Command.Arc", "Description.Arc", "Category.Curves", 50, CommandTopologyKind.Edge,
            Field("start", "Field.Start", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("middle", "Field.Middle", CommandFieldType.Point3, "500, 250, 0", true, "length"),
            Field("end", "Field.End", CommandFieldType.Point3, "1000, 0, 0", true, "length")));
        registry.Register(Definition(Ellipse, "Command.Ellipse", "Description.Ellipse", "Category.Curves", 60, CommandTopologyKind.Edge,
            Field("center", "Field.Center", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("majorRadius", "Field.MajorRadius", CommandFieldType.Expression, "400", true, "length"),
            Field("minorRadius", "Field.MinorRadius", CommandFieldType.Expression, "200", true, "length")));
        registry.Register(Definition(RegularPolygon, "Command.RegularPolygon", "Description.RegularPolygon", "Category.Curves", 70, CommandTopologyKind.Wire | CommandTopologyKind.Face,
            Field("center", "Field.Center", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("xDirection", "Field.XDirection", CommandFieldType.Vector3, "1, 0, 0", true),
            Field("radius", "Field.Radius", CommandFieldType.Expression, "300", true, "length"),
            Field("sideCount", "Field.SideCount", CommandFieldType.Integer, "6", true),
            Field("makeFace", "Field.MakeFace", CommandFieldType.Boolean, "false")));
        registry.Register(Definition(Bezier, "Command.Bezier", "Description.Bezier", "Category.Curves", 80, CommandTopologyKind.Edge,
            Field("poles", "Field.Poles", CommandFieldType.PointList, "0,0,0; 300,500,0; 700,500,0; 1000,0,0", true, "length")));
        registry.Register(Definition(BSpline, "Command.BSpline", "Description.BSpline", "Category.Curves", 90, CommandTopologyKind.Edge,
            Field("points", "Field.Points", CommandFieldType.PointList, "0,0,0; 250,400,0; 600,300,0; 1000,0,0", true, "length"),
            Field("periodic", "Field.Periodic", CommandFieldType.Boolean, "false"),
            Field("tolerance", "Field.Tolerance", CommandFieldType.Expression, "0.0000001", true, "length")));
        registry.Register(Definition(Rectangle, "Command.Rectangle", "Description.Rectangle", "Category.Curves", 100, CommandTopologyKind.Wire,
            Field("origin", "Field.Origin", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("xDirection", "Field.XDirection", CommandFieldType.Vector3, "1, 0, 0", true),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("width", "Field.Width", CommandFieldType.Expression, "1000", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "800", true, "length")));
        registry.Register(Definition(Wire, "Command.Wire", "Description.Wire", "Category.Curves", 110, CommandTopologyKind.Wire,
            ReferenceList("curves", "Field.Curves", CommandTopologyKind.Curve, 1)));
    }

    private static void RegisterSurfaces(CommandRegistry registry)
    {
        registry.Register(Definition(Face, "Command.Face", "Description.Face", "Category.Surfaces", 200, CommandTopologyKind.Face,
            Reference("profile", "Field.Profile", CommandTopologyKind.Curve)));
        registry.Register(Definition(PlaneFace, "Command.PlaneFace", "Description.PlaneFace", "Category.Surfaces", 210, CommandTopologyKind.Face,
            Field("origin", "Field.Origin", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("xDirection", "Field.XDirection", CommandFieldType.Vector3, "1, 0, 0", true),
            Field("normal", "Field.Normal", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("width", "Field.Width", CommandFieldType.Expression, "1000", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "800", true, "length")));
    }

    private static void RegisterSolids(CommandRegistry registry)
    {
        registry.Register(Definition(Box, "Command.Box", "Description.Box", "Category.Solids", 300, CommandTopologyKind.Solid,
            Field("width", "Field.Width", CommandFieldType.Expression, "1000", true, "length"),
            Field("depth", "Field.Depth", CommandFieldType.Expression, "800", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "500", true, "length")));
        registry.Register(Definition(Cylinder, "Command.Cylinder", "Description.Cylinder", "Category.Solids", 310, CommandTopologyKind.Solid,
            Field("origin", "Field.Origin", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("axis", "Field.Axis", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("radius", "Field.Radius", CommandFieldType.Expression, "250", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "500", true, "length")));
        registry.Register(Definition(Cone, "Command.Cone", "Description.Cone", "Category.Solids", 320, CommandTopologyKind.Solid,
            Field("origin", "Field.Origin", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("axis", "Field.Axis", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("radius1", "Field.Radius1", CommandFieldType.Expression, "300", true, "length"),
            Field("radius2", "Field.Radius2", CommandFieldType.Expression, "100", true, "length"),
            Field("height", "Field.Height", CommandFieldType.Expression, "600", true, "length")));
        registry.Register(Definition(Sphere, "Command.Sphere", "Description.Sphere", "Category.Solids", 330, CommandTopologyKind.Solid,
            Field("center", "Field.Center", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("radius", "Field.Radius", CommandFieldType.Expression, "300", true, "length")));
        registry.Register(Definition(Torus, "Command.Torus", "Description.Torus", "Category.Solids", 340, CommandTopologyKind.Solid,
            Field("center", "Field.Center", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("axis", "Field.Axis", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("majorRadius", "Field.MajorRadius", CommandFieldType.Expression, "400", true, "length"),
            Field("minorRadius", "Field.MinorRadius", CommandFieldType.Expression, "100", true, "length")));
        registry.Register(Definition(Wedge, "Command.Wedge", "Description.Wedge", "Category.Solids", 350, CommandTopologyKind.Solid,
            Field("dx", "Field.Dx", CommandFieldType.Expression, "1000", true, "length"),
            Field("dy", "Field.Dy", CommandFieldType.Expression, "700", true, "length"),
            Field("dz", "Field.Dz", CommandFieldType.Expression, "500", true, "length"),
            Field("ltx", "Field.Ltx", CommandFieldType.Expression, "350", true, "length")));
        registry.Register(Definition(Compound, "Command.Compound", "Description.Compound", "Category.Solids", 360, CommandTopologyKind.Compound,
            ReferenceList("shapes", "Field.Shapes", CommandTopologyKind.Any, 1)));
        registry.Register(Definition(Sew, "Command.Sew", "Description.Sew", "Category.Solids", 370, CommandTopologyKind.Any,
            ReferenceList("shapes", "Field.Shapes", CommandTopologyKind.Surface, 2),
            Field("tolerance", "Field.Tolerance", CommandFieldType.Expression, "0.000001", true, "length")));
        registry.Register(Definition(SolidFromShell, "Command.SolidFromShell", "Description.SolidFromShell", "Category.Solids", 380, CommandTopologyKind.Solid,
            Reference("shell", "Field.Shell", CommandTopologyKind.Shell)));
    }

    private static void RegisterFeatures(CommandRegistry registry)
    {
        registry.Register(Definition(Extrude, "Command.Extrude", "Description.Extrude", "Category.Features", 400, CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            Reference("profile", "Field.Profile", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face),
            Field("direction", "Field.Direction", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("distance", "Field.Distance", CommandFieldType.Expression, "500", true, "length")));
        registry.Register(Definition(Revolve, "Command.Revolve", "Description.Revolve", "Category.Features", 410, CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            Reference("profile", "Field.Profile", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face),
            Field("axisPoint", "Field.AxisPoint", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("axisDirection", "Field.AxisDirection", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("angle", "Field.Angle", CommandFieldType.Expression, "360", true, "angle")));
        registry.Register(Definition(Sweep, "Command.Sweep", "Description.Sweep", "Category.Features", 420, CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            Reference("spine", "Field.Spine", CommandTopologyKind.Curve),
            Reference("profile", "Field.Profile", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face)));
        registry.Register(Definition(Loft, "Command.Loft", "Description.Loft", "Category.Features", 430, CommandTopologyKind.Face | CommandTopologyKind.Shell | CommandTopologyKind.Solid,
            ReferenceList("sections", "Field.Sections", CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Face, 2),
            Field("makeSolid", "Field.MakeSolid", CommandFieldType.Boolean, "true"),
            Field("ruled", "Field.Ruled", CommandFieldType.Boolean, "false"),
            Field("tolerance", "Field.Tolerance", CommandFieldType.Expression, "0.000001", true, "length")));
        registry.Register(Definition(Fillet, "Command.Fillet", "Description.Fillet", "Category.Features", 440, CommandTopologyKind.Any,
            Reference("shape", "Field.Shape", CommandTopologyKind.Body),
            Field("edgeIndices", "Field.EdgeIndices", CommandFieldType.Text, "0", true),
            Field("radius", "Field.Radius", CommandFieldType.Expression, "50", true, "length")));
        registry.Register(Definition(Chamfer, "Command.Chamfer", "Description.Chamfer", "Category.Features", 450, CommandTopologyKind.Any,
            Reference("shape", "Field.Shape", CommandTopologyKind.Body),
            Field("edgeIndices", "Field.EdgeIndices", CommandFieldType.Text, "0", true),
            Field("distance", "Field.Distance", CommandFieldType.Expression, "50", true, "length")));
        registry.Register(Definition(Offset, "Command.Offset", "Description.Offset", "Category.Features", 460, CommandTopologyKind.Any,
            Reference("shape", "Field.Shape", CommandTopologyKind.Surface | CommandTopologyKind.Body),
            Field("offset", "Field.Offset", CommandFieldType.Expression, "20", true, "length"),
            Field("tolerance", "Field.Tolerance", CommandFieldType.Expression, "0.0001", true, "length")));
        registry.Register(Definition(Shell, "Command.Shell", "Description.Shell", "Category.Features", 470, CommandTopologyKind.Solid,
            Reference("solid", "Field.Solid", CommandTopologyKind.Solid),
            Field("faceIndices", "Field.FaceIndices", CommandFieldType.Text, "0", true),
            Field("thickness", "Field.Thickness", CommandFieldType.Expression, "20", true, "length"),
            Field("tolerance", "Field.Tolerance", CommandFieldType.Expression, "0.0001", true, "length")));
    }

    private static void RegisterBooleans(CommandRegistry registry)
    {
        registry.Register(BooleanDefinition(Fuse, "Command.Fuse", "Description.Fuse", 500, CommandTopologyKind.Any));
        registry.Register(BooleanDefinition(Cut, "Command.Cut", "Description.Cut", 510, CommandTopologyKind.Any));
        registry.Register(BooleanDefinition(Common, "Command.Common", "Description.Common", 520, CommandTopologyKind.Any));
        registry.Register(BooleanDefinition(Section, "Command.Section", "Description.Section", 530, CommandTopologyKind.Edge | CommandTopologyKind.Wire | CommandTopologyKind.Compound));
    }

    private static void RegisterTransforms(CommandRegistry registry)
    {
        registry.Register(Definition(Move, "Command.Move", "Description.Move", "Category.Transform", 600, CommandTopologyKind.Any,
            Reference("shape", "Field.Shape", CommandTopologyKind.Any),
            Field("vector", "Field.Vector", CommandFieldType.Vector3, "100, 0, 0", true, "length")));
        registry.Register(Definition(RotateShape, "Command.RotateShape", "Description.RotateShape", "Category.Transform", 610, CommandTopologyKind.Any,
            Reference("shape", "Field.Shape", CommandTopologyKind.Any),
            Field("axisPoint", "Field.AxisPoint", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("axisDirection", "Field.AxisDirection", CommandFieldType.Vector3, "0, 0, 1", true),
            Field("angle", "Field.Angle", CommandFieldType.Expression, "90", true, "angle")));
        registry.Register(Definition(ScaleShape, "Command.ScaleShape", "Description.ScaleShape", "Category.Transform", 620, CommandTopologyKind.Any,
            Reference("shape", "Field.Shape", CommandTopologyKind.Any),
            Field("center", "Field.Center", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("factor", "Field.Factor", CommandFieldType.Expression, "1.5", true)));
        registry.Register(Definition(Mirror, "Command.Mirror", "Description.Mirror", "Category.Transform", 630, CommandTopologyKind.Any,
            Reference("shape", "Field.Shape", CommandTopologyKind.Any),
            Field("planePoint", "Field.PlanePoint", CommandFieldType.Point3, "0, 0, 0", true, "length"),
            Field("planeNormal", "Field.PlaneNormal", CommandFieldType.Vector3, "1, 0, 0", true)));
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

    private static CommandDefinition Definition(string type, string displayNameKey, string descriptionKey, string categoryKey, int order, CommandTopologyKind outputTopology, params CommandFieldDefinition[] fields) =>
        new(type, displayNameKey, descriptionKey, categoryKey, order, outputTopology, fields);

    private static CommandDefinition BooleanDefinition(string type, string displayNameKey, string descriptionKey, int order, CommandTopologyKind outputTopology) =>
        Definition(type, displayNameKey, descriptionKey, "Category.Boolean", order, outputTopology,
            Reference("left", "Field.Left", CommandTopologyKind.Any),
            Reference("right", "Field.Right", CommandTopologyKind.Any),
            Field("fuzzyValue", "Field.FuzzyValue", CommandFieldType.Expression, "0", false, "length"));

    private static CommandFieldDefinition Field(string name, string displayNameKey, CommandFieldType type, string defaultValue = "", bool required = false, string unitType = "") =>
        new(name, displayNameKey, type, defaultValue, required, unitType);

    private static CommandFieldDefinition Reference(string name, string displayNameKey, CommandTopologyKind acceptedTopology) =>
        new(name, displayNameKey, CommandFieldType.CommandReference, "", true, "", acceptedTopology, 1, 1);

    private static CommandFieldDefinition ReferenceList(string name, string displayNameKey, CommandTopologyKind acceptedTopology, int minReferences) =>
        new(name, displayNameKey, CommandFieldType.CommandReferenceList, "", true, "", acceptedTopology, minReferences, 0);

    private static string DefaultColor(string categoryKey) => categoryKey switch
    {
        "Category.Curves" => "#2563EB",
        "Category.Surfaces" => "#38BDF8",
        "Category.Features" => "#8B5CF6",
        "Category.Boolean" => "#F59E0B",
        "Category.Transform" => "#14B8A6",
        _ => "#94A3B8"
    };
}
