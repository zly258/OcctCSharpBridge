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

    // -------------------------------------------------------------------------
    // Async wrappers — offload blocking I/O to the thread pool so that UI
    // threads remain responsive during large file imports and exports.
    // OcctEngine is NOT thread-safe; caller must serialize concurrent access.
    // -------------------------------------------------------------------------

    /// <summary>Asynchronously imports a CAD file using automatic format detection.</summary>
    public Task<OcctShape> ImportAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Import(filePath), cancellationToken);

    /// <summary>Asynchronously imports a STEP file.</summary>
    public Task<OcctShape> ImportStepAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ImportStep(filePath), cancellationToken);

    /// <summary>Asynchronously imports an IGES file.</summary>
    public Task<OcctShape> ImportIgesAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ImportIges(filePath), cancellationToken);

    /// <summary>Asynchronously imports a BRep file.</summary>
    public Task<OcctShape> ImportBrepAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ImportBrep(filePath), cancellationToken);

    /// <summary>Asynchronously imports an STL file.</summary>
    public Task<OcctShape> ImportStlAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ImportStl(filePath), cancellationToken);

    /// <summary>Asynchronously exports a shape to a STEP file.</summary>
    public Task ExportStepAsync(
        OcctShape shape,
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ExportStep(shape, filePath), cancellationToken);

    /// <summary>Asynchronously exports all shapes to a STEP file.</summary>
    public Task ExportAllStepAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ExportAllStep(filePath), cancellationToken);

    /// <summary>Asynchronously exports a shape to an IGES file.</summary>
    public Task ExportIgesAsync(
        OcctShape shape,
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ExportIges(shape, filePath), cancellationToken);

    /// <summary>Asynchronously exports a shape to a BRep file.</summary>
    public Task ExportBrepAsync(
        OcctShape shape,
        string filePath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ExportBrep(shape, filePath), cancellationToken);

    /// <summary>Asynchronously exports a shape to an STL file.</summary>
    public Task ExportStlAsync(
        OcctShape shape,
        string filePath,
        double linearDeflection = 0.1,
        double angularDeflection = 0.5,
        bool ascii = false,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ExportStl(shape, filePath, linearDeflection, angularDeflection, ascii), cancellationToken);

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
