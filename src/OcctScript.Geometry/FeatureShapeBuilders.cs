using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed class ExtrudeCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Extrude;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var profile = context.RequiredShape(command, "profile");
            var direction = context.Fields.RequiredVector(command, "direction", context.Parameters, normalize: true);
            var distance = context.Fields.RequiredNumber(command, "distance", context.Parameters);
            if (Math.Abs(distance) <= 1e-12)
                throw new InvalidOperationException("Extrude distance must not be zero.");
            return ShapeBuilderUtilities.FromAlgorithm(
                context.Session.Extrude(profile, direction * distance),
                "EXTRUDE_BUILD_FAILED");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("EXTRUDE_BUILD_FAILED", ex.Message);
        }
    }
}

public sealed class RevolveCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Revolve;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var profile = context.RequiredShape(command, "profile");
            var axisPoint = context.Fields.RequiredPoint(command, "axisPoint", context.Parameters);
            var axisDirection = context.Fields.RequiredVector(command, "axisDirection", context.Parameters, normalize: true);
            var angle = context.Fields.RequiredNumber(command, "angle", context.Parameters, -360000, 360000);
            if (Math.Abs(angle) <= 1e-12)
                throw new InvalidOperationException("Revolve angle must not be zero.");
            return ShapeBuilderUtilities.FromAlgorithm(
                context.Session.Revolve(profile, axisPoint, axisDirection, angle),
                "REVOLVE_BUILD_FAILED");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("REVOLVE_BUILD_FAILED", ex.Message);
        }
    }
}

public sealed class SweepCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Sweep;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        OcctModelShape spineWire = default;
        OcctModelShape profileShape = default;
        var ownsSpine = false;
        var ownsProfile = false;
        try
        {
            var spine = context.RequiredShape(command, "spine");
            var profile = context.RequiredShape(command, "profile");
            (spineWire, ownsSpine) = ShapeBuilderUtilities.ToWire(context.Session, spine);
            if (context.Session.GetShapeType(profile) == OcctShapeType.Edge)
                (profileShape, ownsProfile) = ShapeBuilderUtilities.ToWire(context.Session, profile);
            else
                profileShape = profile;

            return ShapeBuilderUtilities.FromAlgorithm(
                context.Session.Sweep(spineWire, profileShape),
                "SWEEP_BUILD_FAILED");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("SWEEP_BUILD_FAILED", ex.Message);
        }
        finally
        {
            ShapeBuilderUtilities.DeleteTemporary(context.Session, profileShape, ownsProfile);
            ShapeBuilderUtilities.DeleteTemporary(context.Session, spineWire, ownsSpine);
        }
    }
}

public sealed class LoftCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Loft;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        var temporary = new List<OcctModelShape>();
        try
        {
            var sections = context.RequiredShapes(command, "sections", minimumCount: 2);
            var wires = new List<OcctModelShape>(sections.Count);
            foreach (var section in sections)
            {
                var (wire, owned) = ShapeBuilderUtilities.ToWire(context.Session, section);
                wires.Add(wire);
                if (owned) temporary.Add(wire);
            }

            var makeSolid = context.Fields.RequiredBoolean(command, "makeSolid");
            var ruled = context.Fields.RequiredBoolean(command, "ruled");
            var tolerance = context.Fields.RequiredNumber(command, "tolerance", context.Parameters, 1e-12);
            return ShapeBuilderUtilities.FromAlgorithm(
                context.Session.Loft(wires, makeSolid, ruled, tolerance),
                "LOFT_BUILD_FAILED");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("LOFT_BUILD_FAILED", ex.Message);
        }
        finally
        {
            foreach (var shape in temporary)
                ShapeBuilderUtilities.DeleteTemporary(context.Session, shape, owned: true);
        }
    }
}

public sealed class BooleanCommandBuilder(string commandType, OcctBooleanOperation operation) : ICommandShapeBuilder
{
    public string CommandType { get; } = commandType;

    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context)
    {
        try
        {
            var left = context.RequiredShape(command, "left");
            var right = context.RequiredShape(command, "right");
            var fuzzy = context.Fields.RequiredNumber(command, "fuzzyValue", context.Parameters, 0);
            var options = OcctModelBooleanOptions.Default;
            options.FuzzyValue = fuzzy;
            return ShapeBuilderUtilities.FromAlgorithm(
                context.Session.Boolean(operation, left, right, options),
                "BOOLEAN_BUILD_FAILED");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OcctException)
        {
            return ShapeBuildResult.Failed("BOOLEAN_BUILD_FAILED", ex.Message);
        }
    }
}
