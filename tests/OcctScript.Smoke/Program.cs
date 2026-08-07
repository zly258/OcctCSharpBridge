using OcctNet;
using OcctScript.Application;
using OcctScript.Domain;
using OcctScript.Expressions;
using OcctScript.Geometry;

try
{
    VerifyExpressions();
    VerifyRegistration();
    VerifyCurves();
    VerifyPrimitivesAndTransforms();
    VerifyLineWireFaceAndExtrude();
    VerifyRevolve();
    VerifySweep();
    VerifyLoft();
    VerifyBooleans();
    VerifyEdgeFeatures();
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
    Require(definitions.GetAll().Count == 38, $"The initial command catalog must contain 38 commands, actual: {definitions.GetAll().Count}.");
}

static void VerifyCurves()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Curves" };
    var ellipse = Command(registry, BuiltInCommandCatalog.Ellipse, "Ellipse", 10);
    Literal(ellipse, "center", "0,0,0"); Literal(ellipse, "normal", "0,0,1"); Expression(ellipse, "majorRadius", "100"); Expression(ellipse, "minorRadius", "50");
    var polygon = Command(registry, BuiltInCommandCatalog.RegularPolygon, "Polygon", 20);
    Literal(polygon, "center", "300,0,0"); Literal(polygon, "normal", "0,0,1"); Literal(polygon, "xDirection", "1,0,0"); Expression(polygon, "radius", "100"); Expression(polygon, "sideCount", "6"); Literal(polygon, "makeFace", "false");
    var bezier = Command(registry, BuiltInCommandCatalog.Bezier, "Bezier", 30);
    Literal(bezier, "poles", "0,300,0; 100,450,0; 250,450,0; 350,300,0");
    var spline = Command(registry, BuiltInCommandCatalog.BSpline, "BSpline", 40);
    Literal(spline, "points", "500,300,0; 650,450,0; 800,350,0; 950,300,0"); Literal(spline, "periodic", "false"); Expression(spline, "tolerance", "0.0000001");
    document.Commands.AddRange([ellipse, polygon, bezier, spline]);
    BuildAndAssert(document, (ellipse, OcctShapeType.Edge), (polygon, OcctShapeType.Wire), (bezier, OcctShapeType.Edge), (spline, OcctShapeType.Edge));
}

static void VerifyPrimitivesAndTransforms()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "PrimitivesAndTransforms" };
    var cone = Command(registry, BuiltInCommandCatalog.Cone, "Cone", 10);
    Literal(cone, "origin", "0,0,0"); Literal(cone, "axis", "0,0,1"); Expression(cone, "radius1", "100"); Expression(cone, "radius2", "40"); Expression(cone, "height", "200");
    var sphere = Command(registry, BuiltInCommandCatalog.Sphere, "Sphere", 20);
    Literal(sphere, "center", "400,0,100"); Expression(sphere, "radius", "80");
    var torus = Command(registry, BuiltInCommandCatalog.Torus, "Torus", 30);
    Literal(torus, "center", "700,0,100"); Literal(torus, "axis", "0,0,1"); Expression(torus, "majorRadius", "100"); Expression(torus, "minorRadius", "30");
    var wedge = Command(registry, BuiltInCommandCatalog.Wedge, "Wedge", 40);
    Expression(wedge, "dx", "200"); Expression(wedge, "dy", "150"); Expression(wedge, "dz", "120"); Expression(wedge, "ltx", "60"); wedge.Transform.X = 1000;
    var compound = Command(registry, BuiltInCommandCatalog.Compound, "Compound", 50);
    References(compound, "shapes", cone, sphere);
    var move = Command(registry, BuiltInCommandCatalog.Move, "Move", 60);
    Reference(move, "shape", cone); Literal(move, "vector", "0,300,0");
    var rotate = Command(registry, BuiltInCommandCatalog.RotateShape, "Rotate", 70);
    Reference(rotate, "shape", sphere); Literal(rotate, "axisPoint", "400,0,100"); Literal(rotate, "axisDirection", "0,0,1"); Expression(rotate, "angle", "45");
    var scale = Command(registry, BuiltInCommandCatalog.ScaleShape, "Scale", 80);
    Reference(scale, "shape", torus); Literal(scale, "center", "700,0,100"); Expression(scale, "factor", "1.2");
    var mirror = Command(registry, BuiltInCommandCatalog.Mirror, "Mirror", 90);
    Reference(mirror, "shape", wedge); Literal(mirror, "planePoint", "1000,300,0"); Literal(mirror, "planeNormal", "0,1,0");
    document.Commands.AddRange([cone, sphere, torus, wedge, compound, move, rotate, scale, mirror]);
    BuildAndAssert(document, (cone, OcctShapeType.Solid), (sphere, OcctShapeType.Solid), (torus, OcctShapeType.Solid), (wedge, OcctShapeType.Solid), (compound, OcctShapeType.Compound), (move, OcctShapeType.Solid), (rotate, OcctShapeType.Solid), (scale, OcctShapeType.Solid), (mirror, OcctShapeType.Solid));
}

static void VerifyLineWireFaceAndExtrude()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "LineWireFaceExtrude" };
    var line1 = Command(registry, BuiltInCommandCatalog.Line, "Line1", 10); Literal(line1, "start", "0,0,0"); Literal(line1, "end", "100,0,0");
    var line2 = Command(registry, BuiltInCommandCatalog.Line, "Line2", 20); Literal(line2, "start", "100,0,0"); Literal(line2, "end", "100,80,0");
    var line3 = Command(registry, BuiltInCommandCatalog.Line, "Line3", 30); Literal(line3, "start", "100,80,0"); Literal(line3, "end", "0,80,0");
    var line4 = Command(registry, BuiltInCommandCatalog.Line, "Line4", 40); Literal(line4, "start", "0,80,0"); Literal(line4, "end", "0,0,0");
    var wire = Command(registry, BuiltInCommandCatalog.Wire, "Wire", 50); References(wire, "curves", line1, line2, line3, line4);
    var face = Command(registry, BuiltInCommandCatalog.Face, "Face", 60); Reference(face, "profile", wire);
    var edgeExtrude = Command(registry, BuiltInCommandCatalog.Extrude, "EdgeExtrude", 70); Reference(edgeExtrude, "profile", line1); Literal(edgeExtrude, "direction", "0,0,1"); Expression(edgeExtrude, "distance", "25");
    var solidExtrude = Command(registry, BuiltInCommandCatalog.Extrude, "SolidExtrude", 80); Reference(solidExtrude, "profile", face); Literal(solidExtrude, "direction", "0,0,1"); Expression(solidExtrude, "distance", "50");
    document.Commands.AddRange([line1, line2, line3, line4, wire, face, edgeExtrude, solidExtrude]);
    BuildAndAssert(document, (line1, OcctShapeType.Edge), (wire, OcctShapeType.Wire), (face, OcctShapeType.Face), (edgeExtrude, OcctShapeType.Face), (solidExtrude, OcctShapeType.Solid));
}

static void VerifyRevolve()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Revolve" };
    var profile = Command(registry, BuiltInCommandCatalog.PlaneFace, "Profile", 10);
    Literal(profile, "origin", "100,0,0"); Literal(profile, "xDirection", "1,0,0"); Literal(profile, "normal", "0,0,1"); Expression(profile, "width", "100"); Expression(profile, "height", "60");
    var revolve = Command(registry, BuiltInCommandCatalog.Revolve, "Revolve", 20);
    Reference(revolve, "profile", profile); Literal(revolve, "axisPoint", "0,0,0"); Literal(revolve, "axisDirection", "0,1,0"); Expression(revolve, "angle", "360");
    document.Commands.AddRange([profile, revolve]); BuildAndAssert(document, (revolve, OcctShapeType.Solid));
}

static void VerifySweep()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Sweep" };
    var spine = Command(registry, BuiltInCommandCatalog.Line, "Spine", 10); Literal(spine, "start", "0,0,0"); Literal(spine, "end", "0,0,300");
    var profile = Command(registry, BuiltInCommandCatalog.Circle, "Profile", 20); Literal(profile, "center", "0,0,0"); Literal(profile, "normal", "0,0,1"); Expression(profile, "radius", "25");
    var shellSweep = Command(registry, BuiltInCommandCatalog.Sweep, "ShellSweep", 30); Reference(shellSweep, "spine", spine); Reference(shellSweep, "profile", profile);
    var profileFace = Command(registry, BuiltInCommandCatalog.Face, "ProfileFace", 40); Reference(profileFace, "profile", profile);
    var solidSweep = Command(registry, BuiltInCommandCatalog.Sweep, "SolidSweep", 50); Reference(solidSweep, "spine", spine); Reference(solidSweep, "profile", profileFace);
    document.Commands.AddRange([spine, profile, shellSweep, profileFace, solidSweep]);
    BuildAndAssert(document, (shellSweep, OcctShapeType.Shell), (profileFace, OcctShapeType.Face), (solidSweep, OcctShapeType.Solid));
}

static void VerifyLoft()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Loft" };
    var first = Command(registry, BuiltInCommandCatalog.Circle, "Section1", 10); Literal(first, "center", "0,0,0"); Literal(first, "normal", "0,0,1"); Expression(first, "radius", "40");
    var second = Command(registry, BuiltInCommandCatalog.Circle, "Section2", 20); Literal(second, "center", "0,0,200"); Literal(second, "normal", "0,0,1"); Expression(second, "radius", "70");
    var loft = Command(registry, BuiltInCommandCatalog.Loft, "Loft", 30); References(loft, "sections", first, second); Literal(loft, "makeSolid", "true"); Literal(loft, "ruled", "false"); Expression(loft, "tolerance", "0.000001");
    document.Commands.AddRange([first, second, loft]); BuildAndAssert(document, (loft, OcctShapeType.Solid));
}

static void VerifyBooleans()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "Booleans" };
    var left = Command(registry, BuiltInCommandCatalog.Box, "Left", 10); Expression(left, "width", "100"); Expression(left, "depth", "100"); Expression(left, "height", "100");
    var right = Command(registry, BuiltInCommandCatalog.Box, "Right", 20); Expression(right, "width", "100"); Expression(right, "depth", "100"); Expression(right, "height", "100"); right.Transform.X = 50;
    var fuse = Boolean(registry, BuiltInCommandCatalog.Fuse, "Fuse", 30, left, right);
    var cut = Boolean(registry, BuiltInCommandCatalog.Cut, "Cut", 40, left, right);
    var common = Boolean(registry, BuiltInCommandCatalog.Common, "Common", 50, left, right);
    var section = Boolean(registry, BuiltInCommandCatalog.Section, "Section", 60, left, right);
    document.Commands.AddRange([left, right, fuse, cut, common, section]);
    BuildAndAssertBooleanGeometry(document,
        (fuse, 1_500_000.0),
        (cut, 500_000.0),
        (common, 500_000.0),
        section);
}

static void VerifyEdgeFeatures()
{
    var registry = ScriptBuildCoordinator.CreateDefaultCommandRegistry();
    var document = new ScriptDocument { Name = "EdgeFeatures" };
    var filletBase = Box(registry, "FilletBase", 10);
    var fillet = Command(registry, BuiltInCommandCatalog.Fillet, "Fillet", 20); Reference(fillet, "shape", filletBase); Literal(fillet, "edgeIndices", "0"); Expression(fillet, "radius", "5");
    var chamferBase = Box(registry, "ChamferBase", 30); chamferBase.Transform.X = 200;
    var chamfer = Command(registry, BuiltInCommandCatalog.Chamfer, "Chamfer", 40); Reference(chamfer, "shape", chamferBase); Literal(chamfer, "edgeIndices", "0"); Expression(chamfer, "distance", "5");
    var offsetBase = Command(registry, BuiltInCommandCatalog.Sphere, "OffsetBase", 50); Literal(offsetBase, "center", "400,0,50"); Expression(offsetBase, "radius", "40");
    var offset = Command(registry, BuiltInCommandCatalog.Offset, "Offset", 60); Reference(offset, "shape", offsetBase); Expression(offset, "offset", "5"); Expression(offset, "tolerance", "0.0001");
    var shellBase = Box(registry, "ShellBase", 70); shellBase.Transform.X = 600;
    var shell = Command(registry, BuiltInCommandCatalog.Shell, "Shell", 80); Reference(shell, "solid", shellBase); Literal(shell, "faceIndices", "0"); Expression(shell, "thickness", "-5"); Expression(shell, "tolerance", "0.0001");
    document.Commands.AddRange([filletBase, fillet, chamferBase, chamfer, offsetBase, offset, shellBase, shell]);
    BuildAndAssert(document, (fillet, OcctShapeType.Solid), (chamfer, OcctShapeType.Solid), (offset, null), (shell, OcctShapeType.Solid));
}

static ScriptCommand Box(CommandRegistry registry, string name, int order)
{
    var box = Command(registry, BuiltInCommandCatalog.Box, name, order); Expression(box, "width", "100"); Expression(box, "depth", "80"); Expression(box, "height", "60"); return box;
}

static void BuildAndAssertBooleanGeometry(
    ScriptDocument document,
    (ScriptCommand Command, double ExpectedVolume) fuse,
    (ScriptCommand Command, double ExpectedVolume) cut,
    (ScriptCommand Command, double ExpectedVolume) common,
    ScriptCommand section)
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

    foreach (var item in new[] { fuse, cut, common })
    {
        Require(result.Shapes.TryGetValue(item.Command.Id, out var shape), $"{item.Command.Name} did not produce a shape.");
        Require(coordinator.Session.Exists(shape), $"Shape '{shape}' is not available in the modeling session.");
        Require(coordinator.Session.IsValid(shape), $"{item.Command.Name} produced an invalid shape: {coordinator.Session.GetCheckReport(shape)}");
        var solidCount = coordinator.Session.GetTopologyCount(shape, OcctShapeType.Solid);
        Require(solidCount == 1, $"{item.Command.Name} produced {coordinator.Session.GetShapeType(shape)} containing {solidCount} solids; expected exactly one solid.");
        var volume = coordinator.Session.GetVolumeProperties(shape).Mass;
        var tolerance = Math.Max(1e-6, item.ExpectedVolume * 1e-9);
        Require(Math.Abs(volume - item.ExpectedVolume) <= tolerance,
            $"{item.Command.Name} volume was {volume:G17}, expected {item.ExpectedVolume:G17}.");
    }

    Require(result.Shapes.TryGetValue(section.Id, out var sectionShape), "Section did not produce a shape.");
    Require(coordinator.Session.Exists(sectionShape), $"Shape '{sectionShape}' is not available in the modeling session.");
    Require(coordinator.Session.IsValid(sectionShape), $"Section produced an invalid shape: {coordinator.Session.GetCheckReport(sectionShape)}");
    Require(coordinator.Session.GetTopologyCount(sectionShape, OcctShapeType.Edge) > 0,
        $"Section produced {coordinator.Session.GetShapeType(sectionShape)} without section edges.");
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
        if (item.Type.HasValue) Require(coordinator.Session.GetShapeType(shape) == item.Type.Value, $"{item.Command.Name} produced {coordinator.Session.GetShapeType(shape)}, expected {item.Type.Value}.");
    }
}

static ScriptCommand Command(CommandRegistry registry, string type, string name, int order)
{
    var command = BuiltInCommandCatalog.CreateDefault(registry.GetRequired(type), order); command.Name = name; return command;
}

static ScriptCommand Boolean(CommandRegistry registry, string type, string name, int order, ScriptCommand left, ScriptCommand right)
{
    var command = Command(registry, type, name, order); Reference(command, "left", left); Reference(command, "right", right); Expression(command, "fuzzyValue", "0"); return command;
}

static void Expression(ScriptCommand command, string field, string value) => command.Fields[field].Expression = value;
static void Literal(ScriptCommand command, string field, string value) => command.Fields[field].Literal = value;
static void Reference(ScriptCommand command, string field, ScriptCommand value) => command.Fields[field].ReferenceId = value.Id;
static void References(ScriptCommand command, string field, params ScriptCommand[] values) => command.Fields[field].ReferenceIds = values.Select(x => x.Id).ToList();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
