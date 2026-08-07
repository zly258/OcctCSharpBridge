using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed class BoxCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Box;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var width = context.Fields.RequiredNumber(command, "width", context.Parameters, 1e-9);
            var depth = context.Fields.RequiredNumber(command, "depth", context.Parameters, 1e-9);
            var height = context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9);
            var transform = command.Transform;

            var shape = context.Session.MakeBox(
                width,
                depth,
                height,
                transform.X,
                transform.Y,
                transform.Z);

            shape = ApplyRotationAndScale(context.Session, shape, transform);
            return ShapeBuildResult.Succeeded(shape);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("BOX_BUILD_FAILED", ex.Message);
        }
    }

    private static OcctModelShape ApplyRotationAndScale(
        OcctModelingSession session,
        OcctModelShape source,
        TransformDefinition transform)
    {
        var result = source;
        var origin = new OcctPoint3d(transform.X, transform.Y, transform.Z);

        if (Math.Abs(transform.RotationX) > 1e-12)
            result = Replace(session, result, session.Rotate(result, origin, OcctVector3d.UnitX, transform.RotationX));
        if (Math.Abs(transform.RotationY) > 1e-12)
            result = Replace(session, result, session.Rotate(result, origin, OcctVector3d.UnitY, transform.RotationY));
        if (Math.Abs(transform.RotationZ) > 1e-12)
            result = Replace(session, result, session.Rotate(result, origin, OcctVector3d.UnitZ, transform.RotationZ));
        if (Math.Abs(transform.Scale - 1.0) > 1e-12)
            result = Replace(session, result, session.Scale(result, origin, transform.Scale));

        return result;
    }

    private static OcctModelShape Replace(OcctModelingSession session, OcctModelShape previous, OcctModelShape next)
    {
        if (previous.IsValid && previous.Id != next.Id) session.Delete(previous);
        return next;
    }
}

public sealed class CylinderCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Cylinder;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var radius = context.Fields.RequiredNumber(command, "radius", context.Parameters, 1e-9);
            var height = context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9);
            var transform = command.Transform;
            var origin = new OcctPoint3d(transform.X, transform.Y, transform.Z);
            var shape = context.Session.MakeCylinder(origin, OcctVector3d.UnitZ, radius, height);

            if (Math.Abs(transform.RotationX) > 1e-12)
                shape = Replace(context.Session, shape, context.Session.Rotate(shape, origin, OcctVector3d.UnitX, transform.RotationX));
            if (Math.Abs(transform.RotationY) > 1e-12)
                shape = Replace(context.Session, shape, context.Session.Rotate(shape, origin, OcctVector3d.UnitY, transform.RotationY));
            if (Math.Abs(transform.RotationZ) > 1e-12)
                shape = Replace(context.Session, shape, context.Session.Rotate(shape, origin, OcctVector3d.UnitZ, transform.RotationZ));
            if (Math.Abs(transform.Scale - 1.0) > 1e-12)
                shape = Replace(context.Session, shape, context.Session.Scale(shape, origin, transform.Scale));

            return ShapeBuildResult.Succeeded(shape);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("CYLINDER_BUILD_FAILED", ex.Message);
        }
    }

    private static OcctModelShape Replace(OcctModelingSession session, OcctModelShape previous, OcctModelShape next)
    {
        if (previous.IsValid && previous.Id != next.Id) session.Delete(previous);
        return next;
    }
}
