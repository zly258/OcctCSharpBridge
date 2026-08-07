using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed class FaceCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Face;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        OcctModelShape temporaryWire = default;
        var ownsWire = false;
        try
        {
            var profile = context.RequiredShape(command, "profile");
            (temporaryWire, ownsWire) = ShapeBuilderUtilities.ToWire(context.Session, profile);
            var face = context.Session.MakeFace(temporaryWire, onlyPlane: true);
            return ShapeBuildResult.Succeeded(face);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("FACE_BUILD_FAILED", ex.Message);
        }
        finally
        {
            ShapeBuilderUtilities.DeleteTemporary(context.Session, temporaryWire, ownsWire);
        }
    }
}

public sealed class PlaneFaceCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.PlaneFace;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var origin = context.Fields.RequiredPoint(command, "origin", context.Parameters);
            var xDirection = context.Fields.RequiredVector(command, "xDirection", context.Parameters, normalize: true);
            var normal = context.Fields.RequiredVector(command, "normal", context.Parameters, normalize: true);
            var width = context.Fields.RequiredNumber(command, "width", context.Parameters, 1e-9);
            var height = context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9);
            return ShapeBuildResult.Succeeded(context.Session.MakePlaneFace(width, height, origin, xDirection, normal));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("PLANE_FACE_BUILD_FAILED", ex.Message);
        }
    }
}
