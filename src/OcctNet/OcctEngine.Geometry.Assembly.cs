namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeCompound(IEnumerable<OcctShape> shapes, bool hideInputs = false)
    {
        var ids = ShapeIds(shapes);
        EnsureInitialized();
        return CreateGeometryFromIds(
            ids,
            (IntPtr buffer, int count, out long result) => ViewerGeometryCreationNativeMethods.occt_engine_shape_compound_create(
                _handle,
                buffer,
                count,
                hideInputs ? 1 : 0,
                out result));
    }

    public OcctShape MakeWire(IEnumerable<OcctShape> edges, bool hideInputs = false)
    {
        var ids = ShapeIds(edges);
        EnsureInitialized();
        return CreateGeometryFromIds(
            ids,
            (IntPtr buffer, int count, out long result) => ViewerGeometryCreationNativeMethods.occt_engine_shape_wire_create(
                _handle,
                buffer,
                count,
                hideInputs ? 1 : 0,
                out result));
    }

    public OcctShape Sew(IEnumerable<OcctShape> shapes, double tolerance = 1e-6, bool hideInputs = false)
    {
        var ids = ShapeIds(shapes);
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        return CreateGeometryFromIds(
            ids,
            (IntPtr buffer, int count, out long result) => ViewerGeometryCreationNativeMethods.occt_engine_shape_sew(
                _handle,
                buffer,
                count,
                tolerance,
                hideInputs ? 1 : 0,
                out result));
    }

    public OcctShape MakeSolidFromShell(OcctShape shell, bool hideInput = false)
    {
        EnsureShape(shell);
        EnsureInitialized();
        var status = ViewerGeometryCreationNativeMethods.occt_engine_shape_solid_from_shell_create(
            _handle,
            shell.Id,
            hideInput ? 1 : 0,
            out var result);
        return GeometryResult(status, result);
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
