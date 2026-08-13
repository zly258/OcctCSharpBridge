using System.Drawing;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctPoint AddPoint(OcctPoint3d position, OcctMarkerPixmap marker)
    {
        OcctGuard.Finite(position, nameof(position));
        ValidateMarkerPixmap(marker);
        EnsureInitialized();
        return CheckPoint(ViewerInteractionNativeMethods.occt_add_point_pixmap(
            _handle,
            position,
            marker.Width,
            marker.Height,
            marker.Pixels,
            marker.Pixels.Length,
            (int)marker.PixelFormat));
    }

    public void SetPointStyle(OcctPoint point, OcctMarkerPixmap marker)
    {
        EnsurePoint(point);
        ValidateMarkerPixmap(marker);
        CheckInitialized(() => ViewerInteractionNativeMethods.occt_set_point_pixmap_style(
            _handle,
            point.Id,
            marker.Width,
            marker.Height,
            marker.Pixels,
            marker.Pixels.Length,
            (int)marker.PixelFormat));
    }

    public void SetZLayer(IOcctObject value, OcctZLayer layer)
    {
        EnsureObject(value);
        ValidateZLayer(layer);
        CheckInitialized(() => ViewerInteractionNativeMethods.occt_set_object_z_layer(_handle, value.Id, (int)layer));
    }

    public void SetZLayer(IEnumerable<IOcctObject> values, OcctZLayer layer)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateZLayer(layer);
        EnsureInitialized();

        var items = values.ToArray();
        var ids = new long[items.Length];
        for (var index = 0; index < items.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(items[index]);
            EnsureObject(items[index]);
            ids[index] = items[index].Id;
        }

        Check(ViewerInteractionNativeMethods.occt_set_objects_z_layer(_handle, ids, ids.Length, (int)layer));
    }

    public OcctZLayer GetZLayer(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        Check(ViewerInteractionNativeMethods.occt_get_object_z_layer(_handle, value.Id, out var layer));
        if (!Enum.IsDefined(typeof(OcctZLayer), layer))
            throw new InvalidOperationException($"Native Z-layer value {layer} is not supported by the managed bridge.");
        return (OcctZLayer)layer;
    }

    public void SetTriedron(OcctTriedronOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.Position)) throw new ArgumentOutOfRangeException(nameof(options), "Triedron position is invalid.");
        OcctGuard.Positive(options.Scale, nameof(options.Scale));

        var native = new NativeOcctTriedronOptions
        {
            Visible = options.Visible ? 1 : 0,
            Position = (int)options.Position,
            Scale = options.Scale,
            R = options.Color.R / 255.0,
            G = options.Color.G / 255.0,
            B = options.Color.B / 255.0
        };
        CheckInitialized(() => ViewerInteractionNativeMethods.occt_set_triedron_options(_handle, in native));
    }

    public void SetViewCubeOptions(OcctViewCubeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.Position)) throw new ArgumentOutOfRangeException(nameof(options), "View cube position is invalid.");
        if (options.SizePixels <= 0) throw new ArgumentOutOfRangeException(nameof(options), "View cube size must be greater than zero.");
        if (options.OffsetX < 0 || options.OffsetY < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "View cube offsets must not be negative.");

        var native = new NativeOcctViewCubeOptions
        {
            Visible = options.Visible ? 1 : 0,
            Position = (int)options.Position,
            SizePixels = options.SizePixels,
            OffsetX = options.OffsetX,
            OffsetY = options.OffsetY
        };
        CheckInitialized(() => ViewerInteractionNativeMethods.occt_set_view_cube_options(_handle, in native));
    }

    public void SetFaceBoundaryStyle(OcctShape shape, bool visible, Color color, double width = 1.0)
    {
        EnsureShape(shape);
        OcctGuard.Positive(width, nameof(width));
        CheckInitialized(() => ViewerInteractionNativeMethods.occt_set_face_boundary_style(
            _handle,
            shape.Id,
            visible ? 1 : 0,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0,
            width));
    }

    public void SetFaceBoundaryStyle(IEnumerable<OcctShape> shapes, bool visible, Color color, double width = 1.0)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        OcctGuard.Positive(width, nameof(width));
        EnsureInitialized();

        var items = shapes.ToArray();
        var ids = new long[items.Length];
        for (var index = 0; index < items.Length; index++)
        {
            EnsureShape(items[index]);
            ids[index] = items[index].Id;
        }

        Check(ViewerInteractionNativeMethods.occt_set_face_boundary_styles(
            _handle,
            ids,
            ids.Length,
            visible ? 1 : 0,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0,
            width));
    }

    public void SetDefaultFaceBoundaryStyle(
        bool visible,
        Color color,
        double width = 1.0,
        bool applyExisting = true)
    {
        OcctGuard.Positive(width, nameof(width));
        CheckInitialized(() => ViewerInteractionNativeMethods.occt_set_default_face_boundary_style(
            _handle,
            visible ? 1 : 0,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0,
            width,
            applyExisting ? 1 : 0));
    }

    public bool TryGetDetectedHitDetail(out OcctSelectionHitDetail hit)
    {
        EnsureInitialized();
        Check(ViewerInteractionNativeMethods.occt_detected_hit_detail(_handle, out var native, out var hasHit));
        if (hasHit == 0)
        {
            hit = default;
            return false;
        }
        if (hasHit != 1)
            throw new InvalidOperationException("Native detected-hit detail state is invalid.");

        hit = CreateSelectionHitDetail(native);
        return true;
    }

    public IReadOnlyList<OcctSelectionHitDetail> DetectAt(int x, int y, int maxHits = 16)
    {
        if (maxHits <= 0 || maxHits > 1024)
            throw new ArgumentOutOfRangeException(nameof(maxHits), "Maximum hit count must be between 1 and 1024.");
        EnsureInitialized();

        var native = new NativeOcctSelectionHitDetail[maxHits];
        Check(ViewerInteractionNativeMethods.occt_detect_at(_handle, x, y, maxHits, native, native.Length, out var count));
        if (count < 0 || count > native.Length)
            throw new InvalidOperationException("Native detection result count is invalid.");

        var result = new OcctSelectionHitDetail[count];
        for (var index = 0; index < count; index++)
            result[index] = CreateSelectionHitDetail(native[index]);
        return result;
    }

    public OcctPoint3d GetVertexPoint(OcctShape owner, int vertexIndex)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(vertexIndex, nameof(vertexIndex));
        EnsureInitialized();
        Check(ViewerInteractionNativeMethods.occt_indexed_vertex_point(_handle, owner.Id, vertexIndex, out var result));
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctShape owner, int edgeIndex)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(edgeIndex, nameof(edgeIndex));
        EnsureInitialized();
        Check(ViewerInteractionNativeMethods.occt_indexed_edge_endpoints(
            _handle,
            owner.Id,
            edgeIndex,
            out var start,
            out var end));
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdge(OcctShape owner, int edgeIndex, double normalizedParameter)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(edgeIndex, nameof(edgeIndex));
        OcctGuard.UnitInterval(normalizedParameter, nameof(normalizedParameter));
        EnsureInitialized();
        Check(ViewerInteractionNativeMethods.occt_indexed_edge_point_at(
            _handle,
            owner.Id,
            edgeIndex,
            normalizedParameter,
            out var point,
            out var tangent));
        return new(point, tangent);
    }

    public OcctFaceEvaluation EvaluateFace(OcctShape owner, int faceIndex, double u, double v)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(faceIndex, nameof(faceIndex));
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        EnsureInitialized();
        Check(ViewerInteractionNativeMethods.occt_indexed_face_point_normal(
            _handle,
            owner.Id,
            faceIndex,
            u,
            v,
            out var point,
            out var normal));
        return new(point, normal);
    }

    public OcctPoint3d GetFaceCenter(OcctShape owner, int faceIndex)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(faceIndex, nameof(faceIndex));
        EnsureInitialized();
        Check(ViewerInteractionNativeMethods.occt_indexed_face_center(_handle, owner.Id, faceIndex, out var result));
        return result;
    }

    private OcctSelectionHitDetail CreateSelectionHitDetail(NativeOcctSelectionHitDetail native)
    {
        var identity = CreateSelectionHit(new NativeOcctSelectionHit
        {
            OwnerObjectId = native.OwnerObjectId,
            SubshapeType = native.SubshapeType,
            SubshapeIndex = native.SubshapeIndex
        });
        if (!native.Point.IsFinite || !double.IsFinite(native.Depth) || !double.IsFinite(native.DistanceToEye))
            throw new InvalidOperationException("Native selection hit detail contains non-finite geometry.");

        return new(
            identity.Owner,
            identity.SubshapeType,
            identity.SubshapeIndex,
            native.Point,
            native.Depth,
            native.DistanceToEye);
    }

    private static void ValidateMarkerPixmap(OcctMarkerPixmap marker)
    {
        ArgumentNullException.ThrowIfNull(marker);
        ArgumentNullException.ThrowIfNull(marker.Pixels);
        if (!Enum.IsDefined(marker.PixelFormat))
            throw new ArgumentOutOfRangeException(nameof(marker), "Marker pixel format is invalid.");
        if (marker.Width <= 0 || marker.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(marker), "Marker dimensions must be greater than zero.");

        var requiredLength = checked(marker.Width * marker.Height * 4);
        if (marker.Pixels.Length != requiredLength)
            throw new ArgumentException($"Marker pixel buffer must contain exactly {requiredLength} bytes.", nameof(marker));
    }

    private static void ValidateZLayer(OcctZLayer layer)
    {
        if (!Enum.IsDefined(layer)) throw new ArgumentOutOfRangeException(nameof(layer));
    }
}
