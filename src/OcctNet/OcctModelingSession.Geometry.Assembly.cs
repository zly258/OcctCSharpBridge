namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeCompound(IEnumerable<OcctModelShape> shapes)
    {
        var ids = ShapeIds(shapes);
        var status = ModelNativeMethods.occt_model_assembly_compound_create(
            NativeHandle, ids, ids.Length, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeWire(IEnumerable<OcctModelShape> edges)
    {
        var ids = ShapeIds(edges);
        var status = ModelNativeMethods.occt_model_assembly_wire_create(
            NativeHandle, ids, ids.Length, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape Sew(IEnumerable<OcctModelShape> shapes, double tolerance = 1e-6)
    {
        var ids = ShapeIds(shapes);
        OcctGuard.Positive(tolerance, nameof(tolerance));
        var status = ModelNativeMethods.occt_model_assembly_sew(
            NativeHandle, ids, ids.Length, tolerance, out var result);
        return CheckShape(status, result);
    }

    public OcctModelShape MakeSolidFromShell(OcctModelShape shell)
    {
        EnsureShape(shell);
        var status = ModelNativeMethods.occt_model_assembly_solid_from_shell_create(
            _handle, shell.Id, out var result);
        return CheckShape(status, result);
    }
}
