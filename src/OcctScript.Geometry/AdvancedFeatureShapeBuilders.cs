using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed class FilletCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Fillet;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Algorithm("FILLET_BUILD_FAILED", () => context.Session.FilletEdges(
        context.RequiredShape(command, "shape"), context.Fields.RequiredIntegers(command, "edgeIndices"), context.Fields.RequiredNumber(command, "radius", context.Parameters, 1e-9)));
}

public sealed class ChamferCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Chamfer;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Algorithm("CHAMFER_BUILD_FAILED", () => context.Session.ChamferEdges(
        context.RequiredShape(command, "shape"), context.Fields.RequiredIntegers(command, "edgeIndices"), context.Fields.RequiredNumber(command, "distance", context.Parameters, 1e-9)));
}

public sealed class OffsetCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Offset;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Algorithm("OFFSET_BUILD_FAILED", () =>
    {
        var offset = context.Fields.RequiredNumber(command, "offset", context.Parameters);
        if (Math.Abs(offset) <= 1e-12) throw new InvalidOperationException("Offset distance must not be zero.");
        return context.Session.Offset(context.RequiredShape(command, "shape"), offset, context.Fields.RequiredNumber(command, "tolerance", context.Parameters, 1e-12));
    });
}

public sealed class ShellCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Shell;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Algorithm("SHELL_BUILD_FAILED", () =>
    {
        var thickness = context.Fields.RequiredNumber(command, "thickness", context.Parameters);
        if (Math.Abs(thickness) <= 1e-12) throw new InvalidOperationException("Shell thickness must not be zero.");
        return context.Session.MakeThickSolid(context.RequiredShape(command, "solid"), context.Fields.RequiredIntegers(command, "faceIndices"), thickness, context.Fields.RequiredNumber(command, "tolerance", context.Parameters, 1e-12));
    });
}

public sealed class MoveCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Move;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("MOVE_BUILD_FAILED", () => context.Session.Translate(
        context.RequiredShape(command, "shape"), context.Fields.RequiredVector(command, "vector", context.Parameters)));
}

public sealed class RotateShapeCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.RotateShape;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("ROTATE_SHAPE_BUILD_FAILED", () =>
    {
        var angle = context.Fields.RequiredNumber(command, "angle", context.Parameters, -360000, 360000);
        if (Math.Abs(angle) <= 1e-12) throw new InvalidOperationException("Rotation angle must not be zero.");
        return context.Session.Rotate(context.RequiredShape(command, "shape"), context.Fields.RequiredPoint(command, "axisPoint", context.Parameters), context.Fields.RequiredVector(command, "axisDirection", context.Parameters, normalize: true), angle);
    });
}

public sealed class ScaleShapeCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.ScaleShape;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("SCALE_SHAPE_BUILD_FAILED", () => context.Session.Scale(
        context.RequiredShape(command, "shape"), context.Fields.RequiredPoint(command, "center", context.Parameters), context.Fields.RequiredNumber(command, "factor", context.Parameters, 1e-9)));
}

public sealed class MirrorCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Mirror;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("MIRROR_BUILD_FAILED", () => context.Session.MirrorPlane(
        context.RequiredShape(command, "shape"), context.Fields.RequiredPoint(command, "planePoint", context.Parameters), context.Fields.RequiredVector(command, "planeNormal", context.Parameters, normalize: true)));
}
