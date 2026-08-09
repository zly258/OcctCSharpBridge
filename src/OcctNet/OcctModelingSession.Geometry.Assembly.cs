namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeCompound(IEnumerable<OcctModelShape> shapes)
    {
        var ids = ShapeIds(shapes);
        return CheckShape(ModelNativeMethods.occt_model_make_compound(NativeHandle, ids, ids.Length));
    }

    public OcctModelShape MakeWire(IEnumerable<OcctModelShape> edges)
    {
        var ids = ShapeIds(edges);
        return CheckShape(ModelNativeMethods.occt_model_make_wire(NativeHandle, ids, ids.Length));
    }

    public OcctModelShape Sew(IEnumerable<OcctModelShape> shapes, double tolerance = 1e-6)
    {
        var ids = ShapeIds(shapes);
        OcctGuard.Positive(tolerance, nameof(tolerance));
        return CheckShape(ModelNativeMethods.occt_model_sew(NativeHandle, ids, ids.Length, tolerance));
    }

    public OcctModelShape MakeSolidFromShell(OcctModelShape shell)
    {
        EnsureShape(shell);
        return CheckShape(ModelNativeMethods.occt_model_make_solid_from_shell(_handle, shell.Id));
    }
}
