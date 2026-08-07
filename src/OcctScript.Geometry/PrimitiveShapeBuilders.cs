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
            return ShapeBuildResult.Succeeded(context.Session.MakeBox(width, depth, height));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("BOX_BUILD_FAILED", ex.Message);
        }
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
            var axis = context.Fields.RequiredVector(command, "axis", context.Parameters, normalize: true);
            return ShapeBuildResult.Succeeded(context.Session.MakeCylinder(OcctPoint3d.Origin, axis, radius, height));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("CYLINDER_BUILD_FAILED", ex.Message);
        }
    }
}
