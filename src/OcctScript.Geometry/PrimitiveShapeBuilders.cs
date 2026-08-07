using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

public sealed class BoxCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Box;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("BOX_BUILD_FAILED", () => context.Session.MakeBox(
        context.Fields.RequiredNumber(command, "width", context.Parameters, 1e-9),
        context.Fields.RequiredNumber(command, "depth", context.Parameters, 1e-9),
        context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9)));
}

public sealed class CylinderCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Cylinder;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("CYLINDER_BUILD_FAILED", () => context.Session.MakeCylinder(
        context.Fields.RequiredPoint(command, "origin", context.Parameters),
        context.Fields.RequiredVector(command, "axis", context.Parameters, normalize: true),
        context.Fields.RequiredNumber(command, "radius", context.Parameters, 1e-9),
        context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9)));
}

public sealed class ConeCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Cone;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("CONE_BUILD_FAILED", () =>
    {
        var radius1 = context.Fields.RequiredNumber(command, "radius1", context.Parameters, 0);
        var radius2 = context.Fields.RequiredNumber(command, "radius2", context.Parameters, 0);
        if (radius1 <= 1e-12 && radius2 <= 1e-12) throw new InvalidOperationException("At least one cone radius must be greater than zero.");
        return context.Session.MakeCone(
            context.Fields.RequiredPoint(command, "origin", context.Parameters),
            context.Fields.RequiredVector(command, "axis", context.Parameters, normalize: true),
            radius1, radius2,
            context.Fields.RequiredNumber(command, "height", context.Parameters, 1e-9));
    });
}

public sealed class SphereCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Sphere;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("SPHERE_BUILD_FAILED", () => context.Session.MakeSphere(
        context.Fields.RequiredPoint(command, "center", context.Parameters),
        context.Fields.RequiredNumber(command, "radius", context.Parameters, 1e-9)));
}

public sealed class TorusCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Torus;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("TORUS_BUILD_FAILED", () =>
    {
        var major = context.Fields.RequiredNumber(command, "majorRadius", context.Parameters, 1e-9);
        var minor = context.Fields.RequiredNumber(command, "minorRadius", context.Parameters, 1e-9);
        if (minor >= major) throw new InvalidOperationException("Torus minor radius must be smaller than the major radius.");
        return context.Session.MakeTorus(
            context.Fields.RequiredPoint(command, "center", context.Parameters),
            context.Fields.RequiredVector(command, "axis", context.Parameters, normalize: true),
            major, minor);
    });
}

public sealed class WedgeCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Wedge;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("WEDGE_BUILD_FAILED", () =>
    {
        var dx = context.Fields.RequiredNumber(command, "dx", context.Parameters, 1e-9);
        var dy = context.Fields.RequiredNumber(command, "dy", context.Parameters, 1e-9);
        var dz = context.Fields.RequiredNumber(command, "dz", context.Parameters, 1e-9);
        var ltx = context.Fields.RequiredNumber(command, "ltx", context.Parameters, 0, dx);
        return context.Session.MakeWedge(dx, dy, dz, ltx);
    });
}

public sealed class CompoundCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Compound;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("COMPOUND_BUILD_FAILED", () => context.Session.MakeCompound(context.RequiredShapes(command, "shapes")));
}

public sealed class SewCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.Sew;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("SEW_BUILD_FAILED", () => context.Session.Sew(
        context.RequiredShapes(command, "shapes", minimumCount: 2),
        context.Fields.RequiredNumber(command, "tolerance", context.Parameters, 1e-12)));
}

public sealed class SolidFromShellCommandBuilder : ICommandShapeBuilder
{
    public string CommandType => BuiltInCommandCatalog.SolidFromShell;
    public ShapeBuildResult Build(ScriptCommand command, ShapeBuildContext context) => BuilderExecution.Shape("SOLID_FROM_SHELL_BUILD_FAILED", () => context.Session.MakeSolidFromShell(context.RequiredShape(command, "shell")));
}
