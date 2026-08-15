using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    private delegate OcctStatus ImportCall(
        OcctModelingSafeHandle session,
        string path,
        out long resultShapeId);

    private delegate OcctStatus ExportCall(
        OcctModelingSafeHandle session,
        long shapeId,
        string path);

    public OcctModelShape Import(string filePath)
    {
        ValidateExchangePath(filePath);
        EnsureNotDisposed();
        var status = ModelNativeMethods.occt_model_file_import(
            _handle,
            Path.GetFullPath(filePath),
            out var shapeId);
        return CheckExchangeShape(status, shapeId, nameof(Import));
    }

    public OcctModelShape ImportStep(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_step_import, nameof(ImportStep));

    public OcctModelShape ImportIges(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_iges_import, nameof(ImportIges));

    public OcctModelShape ImportBrep(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_brep_import, nameof(ImportBrep));

    public OcctModelShape ImportStl(string filePath) =>
        ImportSpecific(filePath, ModelNativeMethods.occt_model_stl_import, nameof(ImportStl));

    public void ExportStep(OcctModelShape shape, string filePath) =>
        ExportShape(shape, filePath, ModelNativeMethods.occt_model_step_export, nameof(ExportStep));

    public void ExportIges(OcctModelShape shape, string filePath) =>
        ExportShape(shape, filePath, ModelNativeMethods.occt_model_iges_export, nameof(ExportIges));

    public void ExportBrep(OcctModelShape shape, string filePath) =>
        ExportShape(shape, filePath, ModelNativeMethods.occt_model_brep_export, nameof(ExportBrep));

    public void ExportStl(
        OcctModelShape shape,
        string filePath,
        double linearDeflection = 0.1,
        double angularDeflection = 0.5,
        bool ascii = false)
    {
        EnsureShape(shape);
        ValidateExchangePath(filePath);
        OcctGuard.Positive(linearDeflection, nameof(linearDeflection));
        OcctGuard.Positive(angularDeflection, nameof(angularDeflection));
        var options = new NativeStlExportOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeStlExportOptions>(),
            ApiVersion = 1,
            LinearDeflection = linearDeflection,
            AngularDeflection = angularDeflection,
            Ascii = ascii ? 1 : 0
        };
        var status = ModelNativeMethods.occt_model_stl_export(
            _handle,
            shape.Id,
            Path.GetFullPath(filePath),
            in options);
        CheckExchangeStatus(status, nameof(ExportStl));
    }

    private OcctModelShape ImportSpecific(string filePath, ImportCall call, string operation)
    {
        ValidateExchangePath(filePath);
        EnsureNotDisposed();
        var status = call(_handle, Path.GetFullPath(filePath), out var shapeId);
        return CheckExchangeShape(status, shapeId, operation);
    }

    private void ExportShape(OcctModelShape shape, string filePath, ExportCall call, string operation)
    {
        EnsureShape(shape);
        ValidateExchangePath(filePath);
        CheckExchangeStatus(call(_handle, shape.Id, Path.GetFullPath(filePath)), operation);
    }

    private OcctModelShape CheckExchangeShape(OcctStatus status, long shapeId, string operation)
    {
        if (status != OcctStatus.Ok || shapeId <= 0)
            throw CreateException(operation);
        return new OcctModelShape(shapeId, _ownerId);
    }

    private void CheckExchangeStatus(OcctStatus status, string operation)
    {
        if (status != OcctStatus.Ok)
            throw CreateException(operation);
    }

    private static void ValidateExchangePath(string path) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
}
