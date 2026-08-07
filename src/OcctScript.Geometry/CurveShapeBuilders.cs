using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed class VertexCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Vertex;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("VERTEX_BUILD_FAILED", () => context.Session.MakeVertex(context.Fields.RequiredPoint(command, "point", context.Parameters)));
}

public sealed class LineCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Line;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("LINE_BUILD_FAILED", () => context.Session.MakeLine(context.Fields.RequiredPoint(command, "start", context.Parameters), context.Fields.RequiredPoint(command, "end", context.Parameters)));
}

public sealed class PolylineCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Polyline;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("POLYLINE_BUILD_FAILED", () => context.Session.MakePolyline(context.Fields.RequiredPoints(command, "points", context.Parameters), context.Fields.RequiredBoolean(command, "closed")));
}

public sealed class CircleCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Circle;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("CIRCLE_BUILD_FAILED", () => context.Session.MakeCircle(context.Fields.RequiredPoint(command, "center", context.Parameters), context.Fields.RequiredVector(command, "normal", context.Parameters, normalize: true), context.Fields.RequiredNumber(command, "radius", context.Parameters, 1e-9)));
}

public sealed class ArcCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Arc;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("ARC_BUILD_FAILED", () => context.Session.MakeArc(context.Fields.RequiredPoint(command, "start", context.Parameters), context.Fields.RequiredPoint(command, "middle", context.Parameters), context.Fields.RequiredPoint(command, "end", context.Parameters)));
}

public sealed class EllipseCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Ellipse;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("ELLIPSE_BUILD_FAILED", () =>
    {
        var major = context.Fields.RequiredNumber(command, "majorRadius", context.Parameters, 1e-9);
        var minor = context.Fields.RequiredNumber(command, "minorRadius", context.Parameters, 1e-9);
        if (minor > major) throw new InvalidOperationException("Ellipse minor radius must not exceed the major radius.");
        return context.Session.MakeEllipse(context.Fields.RequiredPoint(command, "center", context.Parameters), context.Fields.RequiredVector(command, "normal", context.Parameters, normalize: true), major, minor);
    });
}

public sealed class RegularPolygonCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.RegularPolygon;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("REGULAR_POLYGON_BUILD_FAILED", () => context.Session.MakeRegularPolygon(
        context.Fields.RequiredNumber(command, "radius", context.Parameters, 1e-9),
        context.Fields.RequiredInteger(command, "sideCount", context.Parameters, minimum: 3),
        context.Fields.RequiredBoolean(command, "makeFace"),
        context.Fields.RequiredPoint(command, "center", context.Parameters),
        context.Fields.RequiredVector(command, "normal", context.Parameters, normalize: true),
        context.Fields.RequiredVector(command, "xDirection", context.Parameters, normalize: true)));
}

public sealed class BezierCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Bezier;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("BEZIER_BUILD_FAILED", () => context.Session.MakeBezier(context.Fields.RequiredPoints(command, "poles", context.Parameters, minimumCount: 2)));
}

public sealed class BSplineCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.BSpline;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("BSPLINE_BUILD_FAILED", () => context.Session.MakeInterpolatedBSpline(
        context.Fields.RequiredPoints(command, "points", context.Parameters, minimumCount: 2),
        context.Fields.RequiredBoolean(command, "periodic"),
        context.Fields.RequiredNumber(command, "tolerance", context.Parameters, 1e-12)));
}

public sealed class RectangleCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Rectangle;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("RECTANGLE_BUILD_FAILED", () => context.Session.MakeRectangleWire(
        context.Fields.RequiredNumber(command, "width", context.Parameters, 1e-9),
        context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9),
        context.Fields.RequiredPoint(command, "origin", context.Parameters),
        context.Fields.RequiredVector(command, "xDirection", context.Parameters, normalize: true),
        context.Fields.RequiredVector(command, "normal", context.Parameters, normalize: true)));
}

public sealed class WireCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Wire;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("WIRE_BUILD_FAILED", () =>
    {
        var curves = context.RequiredShapes(command, "curves");
        return context.Session.MakeWire(ShapeBuilderUtilities.ExpandEdges(context.Session, curves));
    });
}

internal static class BuilderExecution
{
    public static ShapeBuildResult Shape(string code, Func<OcctModelShape> build)
    {
        try { return ShapeBuildResult.Succeeded(build()); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException) { return ShapeBuildResult.Failed(code, ex.Message); }
    }

    public static ShapeBuildResult Algorithm(string code, Func<OcctModelAlgorithmResult> build)
    {
        try { return ShapeBuilderUtilities.FromAlgorithm(build(), code); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException) { return ShapeBuildResult.Failed(code, ex.Message); }
    }
}
