using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetZLayer(IOcctObject value, OcctZLayer layer)
    {
        EnsureObject(value);
        ValidateZLayer(layer);
        SetZLayerIds([value.Id], layer);
    }

    public void SetZLayer(IEnumerable<IOcctObject> values, OcctZLayer layer)
    {
        ValidateZLayer(layer);
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        SetZLayerIds(ids, layer);
    }

    public OcctZLayer GetZLayer(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckDecorationsStatus(ViewerDecorationsNativeMethods.occt_engine_object_z_layer_get(
            _handle,
            value.Id,
            out var layer));
        if (!Enum.IsDefined(typeof(OcctZLayer), layer))
            throw new InvalidOperationException($"Native Z-layer value {layer} is not supported by the SDK.");
        return (OcctZLayer)layer;
    }

    public void SetTriedron(OcctTriedronOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.Position))
            throw new ArgumentOutOfRangeException(nameof(options), "Triedron position is invalid.");
        OcctGuard.Positive(options.Scale, nameof(options.Scale));

        var native = new NativeViewerTriedronOptionsV1
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerTriedronOptionsV1>(),
            ApiVersion = 1,
            Visible = options.Visible ? 1 : 0,
            Position = (int)options.Position,
            Scale = options.Scale,
            Color = ToNativeViewColor(options.Color)
        };
        EnsureInitialized();
        CheckDecorationsStatus(ViewerDecorationsNativeMethods.occt_engine_triedron_update(_handle, in native));
    }

    public void SetViewCubeOptions(OcctViewCubeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.Position))
            throw new ArgumentOutOfRangeException(nameof(options), "View cube position is invalid.");
        if (options.SizePixels <= 0 || options.SizePixels > 4096)
            throw new ArgumentOutOfRangeException(nameof(options), "View cube size must be between 1 and 4096 pixels.");
        if (options.OffsetX < 0 || options.OffsetY < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "View cube offsets must not be negative.");

        var native = new NativeViewerViewCubeOptionsV1
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerViewCubeOptionsV1>(),
            ApiVersion = 1,
            Visible = options.Visible ? 1 : 0,
            Position = (int)options.Position,
            SizePixels = options.SizePixels,
            OffsetX = options.OffsetX,
            OffsetY = options.OffsetY
        };
        EnsureInitialized();
        CheckDecorationsStatus(ViewerDecorationsNativeMethods.occt_engine_view_cube_update(_handle, in native));
    }

    public void SetFaceBoundaryStyle(OcctShape shape, bool visible, Color color, double width = 1.0)
    {
        EnsureShape(shape);
        OcctGuard.Positive(width, nameof(width));
        SetFaceBoundaryStyleIds([shape.Id], visible, color, width);
    }

    public void SetFaceBoundaryStyle(
        IEnumerable<OcctShape> shapes,
        bool visible,
        Color color,
        double width = 1.0)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        OcctGuard.Positive(width, nameof(width));
        var items = shapes.ToArray();
        if (items.Length == 0) return;

        var ids = new long[items.Length];
        for (var index = 0; index < items.Length; index++)
        {
            EnsureShape(items[index]);
            ids[index] = items[index].Id;
        }
        SetFaceBoundaryStyleIds(ids, visible, color, width);
    }

    public void SetDefaultFaceBoundaryStyle(
        bool visible,
        Color color,
        double width = 1.0,
        bool applyExisting = true)
    {
        OcctGuard.Positive(width, nameof(width));
        var options = FaceBoundaryOptions(visible, color, width, setDefault: true, applyExisting);
        EnsureInitialized();
        CheckDecorationsStatus(ViewerDecorationsNativeMethods.occt_engine_face_boundary_update(
            _handle,
            IntPtr.Zero,
            0,
            in options));
    }

    private void SetZLayerIds(long[] ids, OcctZLayer layer)
    {
        EnsureInitialized();
        WithPinnedIds(ids, pointer => CheckDecorationsStatus(
            ViewerDecorationsNativeMethods.occt_engine_objects_z_layer_set(
                _handle,
                pointer,
                ids.Length,
                (int)layer)));
    }

    private void SetFaceBoundaryStyleIds(long[] ids, bool visible, Color color, double width)
    {
        var options = FaceBoundaryOptions(visible, color, width, setDefault: false, applyExisting: false);
        EnsureInitialized();
        WithPinnedIds(ids, pointer => CheckDecorationsStatus(
            ViewerDecorationsNativeMethods.occt_engine_face_boundary_update(
                _handle,
                pointer,
                ids.Length,
                in options)));
    }

    private static NativeViewerFaceBoundaryOptions FaceBoundaryOptions(
        bool visible,
        Color color,
        double width,
        bool setDefault,
        bool applyExisting) => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerFaceBoundaryOptions>(),
        ApiVersion = 1,
        Visible = visible ? 1 : 0,
        Color = ToNativeViewColor(color),
        Width = width,
        SetDefault = setDefault ? 1 : 0,
        ApplyExisting = applyExisting ? 1 : 0
    };

    private static void ValidateZLayer(OcctZLayer layer)
    {
        if (!Enum.IsDefined(layer)) throw new ArgumentOutOfRangeException(nameof(layer));
    }

    private void CheckDecorationsStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
