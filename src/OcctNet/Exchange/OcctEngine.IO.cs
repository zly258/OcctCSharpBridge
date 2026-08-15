namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape Import(string filePath)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        var status = ViewerExchangeNativeMethods.occt_engine_exchange_import_file(
            _handle,
            Path.GetFullPath(filePath),
            out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckShape(result);
    }

    public OcctShape ImportStep(string filePath) =>
        ImportSpecific(filePath, ViewerExchangeNativeMethods.occt_engine_exchange_import_step);

    public OcctShape ImportIges(string filePath) =>
        ImportSpecific(filePath, ViewerExchangeNativeMethods.occt_engine_exchange_import_iges);

    public OcctShape ImportBrep(string filePath) =>
        ImportSpecific(filePath, ViewerExchangeNativeMethods.occt_engine_exchange_import_brep);

    public OcctShape ImportStl(string filePath) =>
        ImportSpecific(filePath, ViewerExchangeNativeMethods.occt_engine_exchange_import_stl);

    public void ExportStep(OcctShape shape, string filePath)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        EnsureInitialized();
        EnsureExchangeSuccess(ViewerExchangeNativeMethods.occt_engine_exchange_export_step(
            _handle,
            shape.Id,
            Path.GetFullPath(filePath)));
    }

    public void ExportAllStep(string filePath)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        EnsureExchangeSuccess(ViewerExchangeNativeMethods.occt_engine_exchange_export_all_step(
            _handle,
            Path.GetFullPath(filePath)));
    }

    public void ExportIges(OcctShape shape, string filePath)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        EnsureInitialized();
        EnsureExchangeSuccess(ViewerExchangeNativeMethods.occt_engine_exchange_export_iges(
            _handle,
            shape.Id,
            Path.GetFullPath(filePath)));
    }

    public void ExportAllIges(string filePath)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        EnsureExchangeSuccess(ViewerExchangeNativeMethods.occt_engine_exchange_export_all_iges(
            _handle,
            Path.GetFullPath(filePath)));
    }

    public void ExportBrep(OcctShape shape, string filePath)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        EnsureInitialized();
        EnsureExchangeSuccess(ViewerExchangeNativeMethods.occt_engine_exchange_export_brep(
            _handle,
            shape.Id,
            Path.GetFullPath(filePath)));
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
        EnsureInitialized();
        EnsureExchangeSuccess(ViewerExchangeNativeMethods.occt_engine_exchange_export_stl(
            _handle,
            shape.Id,
            Path.GetFullPath(filePath),
            linearDeflection,
            angularDeflection,
            ascii ? 1 : 0));
    }

    private delegate OcctStatus ImportCall(OcctEngineSafeHandle handle, string path, out long result);

    private OcctShape ImportSpecific(string filePath, ImportCall call)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        var status = call(_handle, Path.GetFullPath(filePath), out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckShape(result);
    }

    private void EnsureExchangeSuccess(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
