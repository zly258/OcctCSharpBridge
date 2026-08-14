namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape Import(string filePath)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_import_file(_handle, Path.GetFullPath(filePath)));
    }

    public OcctShape ImportStep(string filePath) => ImportSpecific(filePath, NativeMethods.occt_import_step);
    public OcctShape ImportIges(string filePath) => ImportSpecific(filePath, NativeMethods.occt_import_iges);
    public OcctShape ImportBrep(string filePath) => ImportSpecific(filePath, NativeMethods.occt_import_brep);
    public OcctShape ImportStl(string filePath) => ImportSpecific(filePath, NativeMethods.occt_import_stl);

    public void ExportStep(OcctShape shape, string filePath) => ExportShape(shape, filePath, NativeMethods.occt_export_step);
    public void ExportIges(OcctShape shape, string filePath) => ExportShape(shape, filePath, NativeMethods.occt_export_iges);
    public void ExportBrep(OcctShape shape, string filePath) => ExportShape(shape, filePath, NativeMethods.occt_export_brep);

    public void ExportAllStep(string filePath)
    {
        ValidatePath(filePath);
        CheckInitialized(() => NativeMethods.occt_export_all_step(_handle, Path.GetFullPath(filePath)));
    }

    public void ExportAllIges(string filePath)
    {
        ValidatePath(filePath);
        CheckInitialized(() => NativeMethods.occt_export_all_iges(_handle, Path.GetFullPath(filePath)));
    }

    public void ExportStl(
        OcctShape shape,
        string filePath,
        double linearDeflection = 0.1,
        double angularDeflection = 0.5,
        bool ascii = false)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        OcctGuard.Positive(linearDeflection, nameof(linearDeflection));
        OcctGuard.Positive(angularDeflection, nameof(angularDeflection));
        CheckInitialized(() => NativeMethods.occt_export_stl(
            _handle,
            shape.Id,
            Path.GetFullPath(filePath),
            linearDeflection,
            angularDeflection,
            ascii ? 1 : 0));
    }

    private delegate long ImportCall(OcctEngineSafeHandle handle, string path);

    private OcctShape ImportSpecific(string filePath, ImportCall call)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        return CheckShape(call(_handle, Path.GetFullPath(filePath)));
    }

    private delegate int ExportCall(OcctEngineSafeHandle handle, long shapeId, string path);

    private void ExportShape(OcctShape shape, string filePath, ExportCall call)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        CheckInitialized(() => call(_handle, shape.Id, Path.GetFullPath(filePath)));
    }
}
