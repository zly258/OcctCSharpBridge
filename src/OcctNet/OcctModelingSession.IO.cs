namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape Import(string filePath) =>
        ImportExchange(filePath, ModelNativeMethods.occt_model_import_file);

    public OcctModelShape ImportStep(string filePath) =>
        ImportExchange(filePath, ModelNativeMethods.occt_model_import_step);

    public OcctModelShape ImportIges(string filePath) =>
        ImportExchange(filePath, ModelNativeMethods.occt_model_import_iges);

    public OcctModelShape ImportBrep(string filePath) =>
        ImportExchange(filePath, ModelNativeMethods.occt_model_import_brep);

    public OcctModelShape ImportStl(string filePath) =>
        ImportExchange(filePath, ModelNativeMethods.occt_model_import_stl);

    public void ExportStep(OcctModelShape shape, string filePath) =>
        ExportExchange(shape, filePath, ModelNativeMethods.occt_model_export_step);

    public void ExportIges(OcctModelShape shape, string filePath) =>
        ExportExchange(shape, filePath, ModelNativeMethods.occt_model_export_iges);

    public void ExportBrep(OcctModelShape shape, string filePath) =>
        ExportExchange(shape, filePath, ModelNativeMethods.occt_model_export_brep);

    public void ExportStl(
        OcctModelShape shape,
        string filePath,
        double linearDeflection = 0.1,
        double angularDeflection = 0.5,
        bool ascii = false)
    {
        EnsureShape(shape);
        var fullPath = ValidateExchangePath(filePath);
        Check(ModelNativeMethods.occt_model_export_stl(
            NativeHandle,
            shape.Id,
            fullPath,
            linearDeflection,
            angularDeflection,
            ascii ? 1 : 0));
    }

    private delegate long ModelImportCall(IntPtr handle, string path);

    private OcctModelShape ImportExchange(string filePath, ModelImportCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        var fullPath = ValidateExchangePath(filePath);
        return CheckShape(call(NativeHandle, fullPath));
    }

    private delegate int ModelExportCall(IntPtr handle, long shapeId, string path);

    private void ExportExchange(
        OcctModelShape shape,
        string filePath,
        ModelExportCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        EnsureShape(shape);
        var fullPath = ValidateExchangePath(filePath);
        Check(call(NativeHandle, shape.Id, fullPath));
    }

    private static string ValidateExchangePath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return Path.GetFullPath(filePath);
    }
}
