using System;
using System.Collections.Generic;
using System.Linq;

namespace OcctNet;

/// <summary>Provides OBJ, glTF and batch STL exchange for the modeling session.</summary>
public sealed partial class OcctModelingSession
{
    // ------- OBJ -------
    public OcctModelShape ImportObj(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        EnsureNotDisposed();
        var status = ModelNativeMethods.occt_model_obj_import(_handle, path, out var id);
        return CheckShape(status, id);
    }

    public void ExportObj(OcctModelShape shape, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_obj_export(_handle, shape.Id, path));
    }

    // ------- glTF -------
    public OcctModelShape ImportGltf(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        EnsureNotDisposed();
        var status = ModelNativeMethods.occt_model_gltf_import(_handle, path, out var id);
        return CheckShape(status, id);
    }

    public void ExportGltf(OcctModelShape shape, string path, OcctGltfExportOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        EnsureShape(shape);
        var opts = options ?? OcctGltfExportOptions.Default;
        var native = opts.ToNative();
        CheckStatus(ModelNativeMethods.occt_model_gltf_export(_handle, shape.Id, path, in native));
    }

    // ------- Batch STL -------
    public void ExportStlMultiple(
        IEnumerable<OcctModelShape> shapes,
        string path,
        OcctStlExportOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        EnsureNotDisposed();
        var ids = ShapeIds(shapes);
        var opts = options ?? OcctStlExportOptions.Default;
        var native = opts.ToNative();
        CheckStatus(ModelNativeMethods.occt_model_stl_export_multiple(
            _handle, ids, ids.Length, path, in native));
    }
}
