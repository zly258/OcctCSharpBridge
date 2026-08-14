using System.Drawing;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Defers OCCT viewer updates until the batch is disposed. Batches can be nested.
/// </summary>
public sealed class OcctDisplayBatch : IDisposable
{
    private OcctEngine? _engine;

    internal OcctDisplayBatch(OcctEngine engine, bool fitAllOnDispose)
    {
        _engine = engine;
        FitAllOnDispose = fitAllOnDispose;
    }

    /// <summary>Fits all displayed objects before the final redraw when this outermost batch ends.</summary>
    public bool FitAllOnDispose { get; set; }

    public void Dispose()
    {
        var engine = Interlocked.Exchange(ref _engine, null);
        if (engine is not null) engine.EndDisplayBatch(FitAllOnDispose);
    }
}

public sealed partial class OcctEngine
{
    /// <summary>Returns true while one or more display update batches are active.</summary>
    public bool IsDisplayBatchActive
    {
        get
        {
            EnsureInitialized();
            return BatchNativeMethods.occt_is_updating(_handle) != 0;
        }
    }

    /// <summary>
    /// Defers Display, Redisplay and view redraw work until the returned scope is disposed.
    /// Use this when creating or changing several objects in one operation.
    /// </summary>
    public OcctDisplayBatch BeginDisplayBatch(bool fitAllOnDispose = false)
    {
        EnsureInitialized();
        Check(BatchNativeMethods.occt_begin_update(_handle));
        return new OcctDisplayBatch(this, fitAllOnDispose);
    }

    internal void EndDisplayBatch(bool fitAll)
    {
        if (IsDisposed || !_initialized) return;
        Check(BatchNativeMethods.occt_end_update(_handle, fitAll ? 1 : 0));
    }

    public void SetColor(IEnumerable<IOcctObject> values, Color color)
    {
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_set_objects_color(
            _handle,
            ids,
            ids.Length,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0));
    }

    public void SetTransparency(IEnumerable<IOcctObject> values, double transparency)
    {
        OcctGuard.UnitInterval(transparency, nameof(transparency));
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_set_objects_transparency(
            _handle,
            ids,
            ids.Length,
            transparency));
    }

    public void SetVisible(IEnumerable<IOcctObject> values, bool visible)
    {
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_set_objects_visible(
            _handle,
            ids,
            ids.Length,
            visible ? 1 : 0));
    }

    public void SetDisplayMode(IEnumerable<IOcctObject> values, OcctDisplayMode displayMode)
    {
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_set_objects_display_mode(
            _handle,
            ids,
            ids.Length,
            (int)displayMode));
    }

    public void SetLineWidth(IEnumerable<IOcctObject> values, double width)
    {
        OcctGuard.Positive(width, nameof(width));
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_set_objects_line_width(
            _handle,
            ids,
            ids.Length,
            width));
    }

    public void SetMaterial(IEnumerable<IOcctObject> values, OcctMaterial material)
    {
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_set_objects_material(
            _handle,
            ids,
            ids.Length,
            (int)material));
    }

    public void Redisplay(IEnumerable<IOcctObject> values)
    {
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_redisplay_objects(_handle, ids, ids.Length));
    }

    public void SelectObjects(IEnumerable<IOcctObject> values, bool appendSelection = false)
    {
        var ids = GetObjectIds(values, nameof(values));
        CheckInitialized(() => NativeMethods.occt_select_objects(
            _handle,
            ids,
            ids.Length,
            appendSelection ? 1 : 0));
    }

    public bool IsVisible(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        return NativeMethods.occt_object_is_visible(_handle, value.Id) != 0;
    }

    public bool IsSelected(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        return NativeMethods.occt_object_is_selected(_handle, value.Id) != 0;
    }
}
