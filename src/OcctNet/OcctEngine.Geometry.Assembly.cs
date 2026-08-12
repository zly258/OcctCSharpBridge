namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeCompound(IEnumerable<OcctShape> shapes, bool hideInputs = false)
    {
        var ids = ShapeIds(shapes);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_compound(_handle, ids, ids.Length, hideInputs ? 1 : 0));
    }

    public OcctShape MakeWire(IEnumerable<OcctShape> edges, bool hideInputs = false)
    {
        var ids = ShapeIds(edges);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_wire(_handle, ids, ids.Length, hideInputs ? 1 : 0));
    }

    public OcctShape Sew(IEnumerable<OcctShape> shapes, double tolerance = 1e-6, bool hideInputs = false)
    {
        var ids = ShapeIds(shapes);
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_sew_shapes(_handle, ids, ids.Length, tolerance, hideInputs ? 1 : 0));
    }

    public OcctShape MakeSolidFromShell(OcctShape shell, bool hideInput = false)
    {
        EnsureShape(shell);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_solid_from_shell(_handle, shell.Id, hideInput ? 1 : 0));
    }

    private long[] ShapeIds(IEnumerable<OcctShape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        var array = shapes.ToArray();
        if (array.Length == 0) throw new ArgumentException("Collection must not be empty.", nameof(shapes));
        foreach (var shape in array) EnsureShape(shape);
        return array.Select(value => value.Id).ToArray();
    }
}
