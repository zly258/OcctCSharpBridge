using OcctNet;
using OcctScript.Application;
using OcctScript.Domain;
using OcctScript.Expressions;
using OcctScript.Geometry;

try
{
    VerifyExpressions();
    VerifyRegistration();
    VerifyLineWireFaceAndExtrude();
    VerifyRevolve();
    VerifySweep();
    VerifyLoft();
    VerifyBooleans();
    Console.WriteLine("OcctScript smoke tests passed.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static void VerifyExpressions()
{
    var result = new ExpressionEngine().Evaluate("max(100, 20 * 8) + sind(30) * 20");
    Require(result.Success, result.Error);
    Require(Math.Abs(result.Value - 170) < 1e-9, $"Unexpected expression result: {result.Value}");
}

static void VerifyRegistration()
{
    var definitions = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var builders = ScriptBuildCoordinator.CreateDefaultBuilderRegistry();
    foreach (var definition in definitions.GetAll())
        Require(builders.TryGet(definition.Type, out _), $"Builder is missing for '{definition.Type}'.");
    Require(definitions.GetAll().Count == 19, "The first-stage command catalog must contain 19 commands.");
}

static void VerifyLineWireFaceAndExtrude()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "LineWireFaceExtrude" };
    var line1 = Command(registry, BuiltInCommandCatalog.Line, "Line1", 10);
    Literal(line1, "start", "0,0,0"); Literal(line1, "end", "100,0,0");
    var line2 = Command(registry, BuiltInCommandCatalog.Line, "Line2", 20);
    Literal(line2, "start", "100,0,0"); Literal(line2, "end", "100,80,0");
    var line3 = Command(registry, BuiltInCommandCatalog.Line, "Line3", 30);
    Literal(line3, "start", "100,80,0"); Literal(line3, "end", "0,80,0");
    var line4 = Command(registry, BuiltInCommandCatalog.Line, "Line4", 40);
    Literal(line4, "start", "0,80,0"); Literal(line4, "end", "0,0,0");
    var wire = Command(registry, BuiltInCommandCatalog.Wire, "Wire", 50);
    References(wire, "curves", line1, line2, line3, line4);
    var face = Command(registry, BuiltInCommandCatalog.Face, "Face", 60);
    Reference(face, "profile", wire);
    var edgeExtrude = Command(registry, BuiltInCommandCatalog.Extrude, "EdgeExtrude", 70);
    Reference(edgeExtrude, "profile", line1); Literal(edgeExtrude, "direction", "0,0,1"); Expression(edgeExtrude, "distance", "25");
    var solidExtrude = Command(registry, BuiltInCommandCatalog.Extrude, "SolidExtrude", 80);
    Reference(solidExtrude, "profile", face); Literal(solidExtrude, "direction", "0,0,1"); Expression(solidExtrude, "distance", "50");
    document.Commands.AddRange([line1, line2, line3, line4, wire, face, edgeExtrude, solidExtrude]);

    BuildAndAssert(document,
        (line1, OcctShapeType.Edge),
        (wire, OcctShapeType.Wire),
        (face, OcctShapeType.Face),
        (edgeExtrude, OcctShapeType.Face),
        (solidExtrude, OcctShapeType.Solid));
}

static void VerifyRevolve()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Revolve" };
    var profile = Command(registry, BuiltInCommandCatalog.PlaneFace, "Profile", 10);
    Literal(profile, "origin", "100,0,0"); Literal(profile, "xDirection", "1,0,0"); Literal(profile, "normal", "0,0,1");
    Expression(profile, "width", "100"); Expression(profile, "height", "60");
    var revolve = Command(registry, BuiltInCommandCatalog.Revolve, "Revolve", 20);
    Reference(revolve, "profile", profile); Literal(revolve, "axisPoint", "0,0,0"); Literal(revolve, "axisDirection", "0,1,0"); Expression(revolve, "angle", "360");
    document.Commands.AddRange([profile, revolve]);
    BuildAndAssert(document, (revolve, OcctShapeType.Solid));
}

static void VerifySweep()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Sweep" };
    var spine = Command(registry, BuiltInCommandCatalog.Line, "Spine", 10);
    Literal(spine, "start", "0,0,0"); Literal(spine, "end", "0,0,300");
    var profile = Command(registry, BuiltInCommandCatalog.Circle, "Profile", 20);
    Literal(profile, "center", "0,0,0"); Literal(profile, "normal", "0,0,1"); Expression(profile, "radius", "25");
    var sweep = Command(registry, BuiltInCommandCatalog.Sweep, "Sweep", 30);
    Reference(sweep, "spine", spine); Reference(sweep, "profile", profile);
    document.Commands.AddRange([spine, profile, sweep]);
    BuildAndAssert(document, (sweep, OcctShapeType.Solid));
}

static void VerifyLoft()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Loft" };
    var first = Command(registry, BuiltInCommandCatalog.Circle, "Section1", 10);
    Literal(first, "center", "0,0,0"); Literal(first, "normal", "0,0,1"); Expression(first, "radius", "40");
    var second = Command(registry, BuiltInCommandCatalog.Circle, "Section2", 20);
    Literal(second, "center", "0,0,200"); Literal(second, "normal", "0,0,1"); Expression(second, "radius", "70");
    var loft = Command(registry, BuiltInCommandCatalog.Loft, "Loft", 30);
    References(loft, "sections", first, second); Literal(loft, "makeSolid", "true"); Literal(loft, "ruled", "false"); Expression(loft, "tolerance", "0.000001");
    document.Commands.AddRange([first, second, loft]);
    BuildAndAssert(document, (loft, OcctShapeType.Solid));
}

static void VerifyBooleans()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Booleans" };
    var left = Command(registry, BuiltInCommandCatalog.Box, "Left", 10);
    Expression(left, "width", "100"); Expression(left, "depth", "100"); Expression(left, "height", "100");
    var right = Command(registry, BuiltInCommandCatalog.Box, "Right", 20);
    Expression(right, "width", "100"); Expression(right, "depth", "100"); Expression(right, "height", "100"); right.Transform.X = 50;
    var fuse = Boolean(registry, BuiltInCommandCatalog.Fuse, "Fuse", 30, left, right);
    var cut = Boolean(registry, BuiltInCommandCatalog.Cut, "Cut", 40, left, right);
    var common = Boolean(registry, BuiltInCommandCatalog.Common, "Common", 50, left, right);
    var section = Boolean(registry, BuiltInCommandCatalog.Section, "Section", 60, left, right);
    document.Commands.AddRange([left, right, fuse, cut, common, section]);
    BuildAndAssert(document,
        (fuse, OcctShapeType.Solid),
        (cut, OcctShapeType.Solid),
        (common, OcctShapeType.Solid),
        (section, null));
}

static void BuildAndAssert(ScriptDocument document, params (ScriptCommand Command, OcctShapeType? Type)[] expected)
{
    var expressions = new ExpressionEngine();
    var parameters = new ParameterService(expressions).Evaluate(document);
    Require(parameters.Errors.Count == 0, string.Join(Environment.NewLine, parameters.Errors.Values));
    using var coordinator = new ScriptBuildCoordinator(expressionEngine: expressions);
    var result = coordinator.Build(document, parameters.Values);
    if (!result.Success)
    {
        var messages = result.Commands.SelectMany(x => x.Messages).Select(x => $"{x.Code}: {x.Message}");
        throw new InvalidOperationException(string.Join(Environment.NewLine, messages));
    }
    foreach (var item in expected)
    {
        Require(result.Shapes.TryGetValue(item.Command.Id, out var shape), $"{item.Command.Type} did not produce a shape.");
        Require(coordinator.Session.Exists(shape), $"Shape '{shape}' is not available in the modeling session.");
        if (item.Type.HasValue)
            Require(coordinator.Session.GetShapeType(shape) == item.Type.Value,
                $"{item.Command.Name} produced {coordinator.Session.GetShapeType(shape)}, expected {item.Type.Value}.");
    }
}

static ScriptCommand Command(CommandRegistry registry, string type, string name, int order)
{
    var command = BuiltInCommandCatalog.CreateDefault(registry.GetRequired(type), order);
    command.Name = name;
    return command;
}

static ScriptCommand Boolean(CommandRegistry registry, string type, string name, int order, ScriptCommand left, ScriptCommand right)
{
    var command = Command(registry, type, name, order);
    Reference(command, "left", left); Reference(command, "right", right); Expression(command, "fuzzyValue", "0");
    return command;
}

static void Expression(ScriptCommand command, string field, string value) => command.Fields[field].Expression = value;
static void Literal(ScriptCommand command, string field, string value) => command.Fields[field].Literal = value;
static void Reference(ScriptCommand command, string field, ScriptCommand value) => command.Fields[field].ReferenceId = value.Id;
static void References(ScriptCommand command, string field, params ScriptCommand[] values) => command.Fields[field].ReferenceIds = values.Select(x => x.Id).ToList();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
