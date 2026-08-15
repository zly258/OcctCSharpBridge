using System.Drawing;
using System.Runtime.InteropServices;
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
            CheckBatchStatus(BatchNativeMethods.occt_engine_update_state_get(_handle, out var isUpdating));
            if (isUpdating is not 0 and not 1)
                throw new InvalidOperationException("Native viewer update state is invalid.");
            return isUpdating != 0;
        }
    }

    /// <summary>
    /// Defers Display, Redisplay and view redraw work until the returned scope is disposed.
    /// Use this when creating or changing several objects in one operation.
    /// </summary>
    public OcctDisplayBatch BeginDisplayBatch(bool fitAllOnDispose = false)
    {
        EnsureInitialized();
        CheckBatchStatus(BatchNativeMethods.occt_engine_update_begin(_handle));
        return new OcctDisplayBatch(this, fitAllOnDispose);
    }

    internal void EndDisplayBatch(bool fitAll)
    {
        if (IsDisposed || !_initialized) return;
        CheckBatchStatus(BatchNativeMethods.occt_engine_update_end(_handle, fitAll ? 1 : 0));
    }

    public void SetColor(IEnumerable<IOcctObject> values, Color color)
    {
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        UpdateObjects(ids, new NativeViewerObjectUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerObjectUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerObjectUpdateMask.Color,
            Color = new NativeViewColorRgb
            {
                R = color.R / 255.0,
                G = color.G / 255.0,
                B = color.B / 255.0
            }
        });
    }

    public void SetTransparency(IEnumerable<IOcctObject> values, double transparency)
    {
        OcctGuard.UnitInterval(transparency, nameof(transparency));
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        UpdateObjects(ids, new NativeViewerObjectUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerObjectUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerObjectUpdateMask.Transparency,
            Transparency = transparency
        });
    }

    public void SetVisible(IEnumerable<IOcctObject> values, bool visible)
    {
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        UpdateObjects(ids, new NativeViewerObjectUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerObjectUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerObjectUpdateMask.Visibility,
            Visible = visible ? 1 : 0
        });
    }

    public void SetDisplayMode(IEnumerable<IOcctObject> values, OcctDisplayMode displayMode)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        var items = values.ToArray();
        if (items.Length == 0) return;
        using var batch = BeginDisplayBatch();
        foreach (var value in items)
        {
            ArgumentNullException.ThrowIfNull(value);
            SetDisplayModeOverride(value, displayMode);
        }
    }

    public void SetLineWidth(IEnumerable<IOcctObject> values, double width)
    {
        OcctGuard.Positive(width, nameof(width));
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        UpdateObjects(ids, new NativeViewerObjectUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerObjectUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerObjectUpdateMask.LineWidth,
            LineWidth = width
        });
    }

    public void SetMaterial(IEnumerable<IOcctObject> values, OcctMaterial material)
    {
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        UpdateObjects(ids, new NativeViewerObjectUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerObjectUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerObjectUpdateMask.Material,
            Material = (int)material
        });
    }

    public void Redisplay(IEnumerable<IOcctObject> values)
    {
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        WithPinnedIds(ids, pointer => CheckBatchStatus(
            ObjectNativeMethods.occt_engine_objects_presentation_action(
                _handle,
                pointer,
                ids.Length,
                NativeViewerObjectPresentationAction.Redisplay)));
    }

    public void SelectObjects(IEnumerable<IOcctObject> values, bool appendSelection = false) =>
        SetSelection(
            values,
            appendSelection ? OcctSelectionOperation.Add : OcctSelectionOperation.Replace);

    public bool IsVisible(IOcctObject value) => GetObjectState(value).Visible != 0;

    public bool IsSelected(IOcctObject value) => GetObjectState(value).Selected != 0;

    private void UpdateObjects(long[] ids, NativeViewerObjectUpdateOptions options)
    {
        EnsureInitialized();
        WithPinnedIds(ids, pointer => CheckBatchStatus(ObjectNativeMethods.occt_engine_objects_update(
            _handle,
            pointer,
            ids.Length,
            in options)));
    }

    private static void WithPinnedIds(long[] ids, Action<IntPtr> action)
    {
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(action);
        if (ids.Length == 0)
        {
            action(IntPtr.Zero);
            return;
        }

        var pinned = GCHandle.Alloc(ids, GCHandleType.Pinned);
        try
        {
            action(pinned.AddrOfPinnedObject());
        }
        finally
        {
            pinned.Free();
        }
    }

    private void CheckBatchStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
