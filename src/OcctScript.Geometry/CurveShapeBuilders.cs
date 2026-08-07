using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed class VertexCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Vertex;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => Execute(() =>
        context.Session.MakeVertex(context.Fields.RequiredPoint(command, "point", context.Parameters)));

    private static ShapeBuildResult Execute(Func<OcctModelShape> action)
    {
        try { return ShapeBuildResult.Succeeded(action()); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        { return ShapeBuildResult.Failed("VERTEX_BUILD_FAILED", ex.Message); }
    }
}

public sealed class LineCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Line;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var start = context.Fields.RequiredPoint(command, "start", context.Parameters);
            var end = context.Fields.RequiredPoint(command, "end", context.Parameters);
            return ShapeBuildResult.Succeeded(context.Session.MakeLine(start, end));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("LINE_BUILD_FAILED", ex.Message);
        }
    }
}

public sealed class PolylineCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Polyline;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var points = context.Fields.RequiredPoints(command, "points", context.Parameters);
            var closed = context.Fields.RequiredBoolean(command, "closed");
            return ShapeBuildResult.Succeeded(context.Session.MakePolyline(points, closed));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("POLYLINE_BUILD_FAILED", ex.Message);
        }
    }
}

public sealed class CircleCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Circle;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var center = context.Fields.RequiredPoint(command, "center", context.Parameters);
            var normal = context.Fields.RequiredVector(command, "normal", context.Parameters, normalize: true);
            var radius = context.Fields.RequiredNumber(command, "radius", context.Parameters, 1e-9);
            return ShapeBuildResult.Succeeded(context.Session.MakeCircle(center, normal, radius));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("CIRCLE_BUILD_FAILED", ex.Message);
        }
    }
}

public sealed class ArcCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Arc;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var start = context.Fields.RequiredPoint(command, "start", context.Parameters);
            var middle = context.Fields.RequiredPoint(command, "middle", context.Parameters);
            var end = context.Fields.RequiredPoint(command, "end", context.Parameters);
            return ShapeBuildResult.Succeeded(context.Session.MakeArc(start, middle, end));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("ARC_BUILD_FAILED", ex.Message);
        }
    }
}

public sealed class RectangleCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Rectangle;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var origin = context.Fields.RequiredPoint(command, "origin", context.Parameters);
            var xDirection = context.Fields.RequiredVector(command, "xDirection", context.Parameters, normalize: true);
            var normal = context.Fields.RequiredVector(command, "normal", context.Parameters, normalize: true);
            var width = context.Fields.RequiredNumber(command, "width", context.Parameters, 1e-9);
            var height = context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9);
            return ShapeBuildResult.Succeeded(context.Session.MakeRectangleWire(width, height, origin, xDirection, normal));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("RECTANGLE_BUILD_FAILED", ex.Message);
        }
    }
}

public sealed class WireCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Wire;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var curves = context.RequiredShapes(command, "curves");
            var edges = ShapeBuilderUtilities.ExpandEdges(context.Session, curves);
            return ShapeBuildResult.Succeeded(context.Session.MakeWire(edges));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("WIRE_BUILD_FAILED", ex.Message);
        }
    }
}
