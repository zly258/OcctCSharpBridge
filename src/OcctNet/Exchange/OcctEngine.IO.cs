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
    // Viewer exchange must execute on the native surface thread. Async wrappers
    // post work to the SynchronizationContext captured during initialization.
    // Use OcctModelingSession for truly parallel, headless file processing.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Imports a CAD file on an isolated headless session and creates its viewer shape
    /// on the native surface thread.
    /// </summary>
    public Task<OcctShape> ImportAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        ImportInBackgroundAsync(filePath, static (session, path) => session.Import(path), cancellationToken);

    /// <summary>Imports a STEP file through an isolated headless session.</summary>
    public Task<OcctShape> ImportStepAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        ImportInBackgroundAsync(filePath, static (session, path) => session.ImportStep(path), cancellationToken);

    /// <summary>Imports an IGES file through an isolated headless session.</summary>
    public Task<OcctShape> ImportIgesAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        ImportInBackgroundAsync(filePath, static (session, path) => session.ImportIges(path), cancellationToken);

    /// <summary>Imports a BRep file through an isolated headless session.</summary>
    public Task<OcctShape> ImportBrepAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        ImportInBackgroundAsync(filePath, static (session, path) => session.ImportBrep(path), cancellationToken);

    /// <summary>Imports an STL file through an isolated headless session.</summary>
    public Task<OcctShape> ImportStlAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        ImportInBackgroundAsync(filePath, static (session, path) => session.ImportStl(path), cancellationToken);

    /// <summary>Asynchronously exports a shape to a STEP file.</summary>
    public Task ExportStepAsync(
        OcctShape shape,
        string filePath,
        CancellationToken cancellationToken = default) =>
        RunOnSurfaceThreadAsync(() => ExportStep(shape, filePath), cancellationToken);

    /// <summary>Asynchronously exports all shapes to a STEP file.</summary>
    public Task ExportAllStepAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        RunOnSurfaceThreadAsync(() => ExportAllStep(filePath), cancellationToken);

    /// <summary>Asynchronously exports a shape to an IGES file.</summary>
    public Task ExportIgesAsync(
        OcctShape shape,
        string filePath,
        CancellationToken cancellationToken = default) =>
        RunOnSurfaceThreadAsync(() => ExportIges(shape, filePath), cancellationToken);

    /// <summary>Asynchronously exports a shape to a BRep file.</summary>
    public Task ExportBrepAsync(
        OcctShape shape,
        string filePath,
        CancellationToken cancellationToken = default) =>
        RunOnSurfaceThreadAsync(() => ExportBrep(shape, filePath), cancellationToken);

    /// <summary>Asynchronously exports a shape to an STL file.</summary>
    public Task ExportStlAsync(
        OcctShape shape,
        string filePath,
        double linearDeflection = 0.1,
        double angularDeflection = 0.5,
        bool ascii = false,
        CancellationToken cancellationToken = default) =>
        RunOnSurfaceThreadAsync(() => ExportStl(shape, filePath, linearDeflection, angularDeflection, ascii), cancellationToken);

    private delegate OcctStatus ImportCall(OcctEngineSafeHandle handle, string path, out long result);
    private delegate OcctModelShape BackgroundImport(OcctModelingSession session, string path);

    private async Task<OcctShape> ImportInBackgroundAsync(
        string filePath,
        BackgroundImport import,
        CancellationToken cancellationToken)
    {
        ValidatePath(filePath);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the OCCT engine before starting a background import.");
        ArgumentNullException.ThrowIfNull(import);
        cancellationToken.ThrowIfCancellationRequested();

        using var session = new OcctModelingSession();
        var fullPath = Path.GetFullPath(filePath);
        var modelShape = await Task.Run(
            () => import(session, fullPath),
            CancellationToken.None).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        return await RunOnSurfaceThreadAsync(
            () => CreateShapeFromModel(session, modelShape),
            cancellationToken).ConfigureAwait(false);
    }

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
