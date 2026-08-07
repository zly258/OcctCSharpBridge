namespace OcctNet;

public sealed partial class OcctModelingSession
{
    private delegate long ImportCall(IntPtr handle, string path);
    private delegate int ExportCall(IntPtr handle, long shapeId, string path);

    public OcctModelShape Import(string filePath)
    {
        ValidateExchangePath(filePath);
        return CheckShape(ModelNativeMethods.occt_model_import_file(_handle, Path.GetFullPath(filePath)));
    }

    public OcctModelShape ImportStep(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_import_step);

    public OcctModelShape ImportIges(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_import_iges);

    public OcctModelShape ImportBrep(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_import_brep);

    public OcctModelShape ImportStl(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_import_stl);

    public void ExportStep(OcctModelShape shape, string filePath) =>
        ExportShape(shape, filePath, ModelNativeMethods.occt_model_export_step);

    public void ExportIges(OcctModelShape shape, string filePath) =>
        ExportShape(shape, filePath, ModelNativeMethods.occt_model_export_iges);

    public void ExportBrep(OcctModelShape shape, string filePath) =>
        ExportShape(shape, filePath, ModelNativeMethods.occt_model_export_brep);

    public void ExportStl(
        OcctModelShape shape,
        string filePath,
        double linearDeflection = 0.1,
        double angularDeflection = 0.5,
        bool ascii = false)
    {
        EnsureShape(shape);
        ValidateExchangePath(filePath);
        Check(ModelNativeMethods.occt_model_export_stl(
            _handle,
            shape.Id,
            Path.GetFullPath(filePath),
            linearDeflection,
            angularDeflection,
            ascii ? 1 : 0));
    }

    private OcctModelShape ImportSpecific(string filePath, ImportCall call)
    {
        ValidateExchangePath(filePath);
        return CheckShape(call(_handle, Path.GetFullPath(filePath)));
    }

    private void ExportShape(OcctModelShape shape, string filePath, ExportCall call)
    {
        EnsureShape(shape);
        ValidateExchangePath(filePath);
        Check(call(_handle, shape.Id, Path.GetFullPath(filePath)));
    }

    private static void ValidateExchangePath(string path) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
}
