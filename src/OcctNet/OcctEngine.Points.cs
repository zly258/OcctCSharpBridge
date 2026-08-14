using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctPoint AddPoint(
        OcctPoint3d position,
        OcctPointMarker marker = OcctPointMarker.CirclePoint,
        double scale = 3.0,
        Color? color = null)
    {
        OcctGuard.Finite(position, nameof(position));
        OcctGuard.Positive(scale, nameof(scale));
        if (!Enum.IsDefined(marker)) throw new ArgumentOutOfRangeException(nameof(marker));

        var value = color ?? Color.FromArgb(255, 190, 0);
        EnsureInitialized();
        var options = PointOptions(
            NativeViewerPointUpdateMask.Position | NativeViewerPointUpdateMask.Style,
            position,
            marker,
            scale,
            value);
        var status = NativeMethods.occt_engine_point_create(_handle, in options, out var pointId);
        if (status != OcctStatus.Ok) throw CreateException(nameof(AddPoint));
        return CheckPoint(pointId);
    }

    public void SetPointPosition(OcctPoint point, OcctPoint3d position)
    {
        EnsurePoint(point);
        OcctGuard.Finite(position, nameof(position));
        var options = PointOptions(NativeViewerPointUpdateMask.Position, position);
        CheckPointStatus(NativeMethods.occt_engine_point_update(_handle, point.Id, in options));
    }

    public void SetPointStyle(
        OcctPoint point,
        OcctPointMarker marker,
        double scale,
        Color color)
    {
        EnsurePoint(point);
        if (!Enum.IsDefined(marker)) throw new ArgumentOutOfRangeException(nameof(marker));
        OcctGuard.Positive(scale, nameof(scale));
        var options = PointOptions(
            NativeViewerPointUpdateMask.Style,
            default,
            marker,
            scale,
            color);
        CheckPointStatus(NativeMethods.occt_engine_point_update(_handle, point.Id, in options));
    }

    private static NativeViewerPointOptions PointOptions(
        NativeViewerPointUpdateMask updateMask,
        OcctPoint3d position = default,
        OcctPointMarker marker = OcctPointMarker.Point,
        double scale = 1.0,
        Color? color = null)
    {
        var actualColor = color ?? Color.Black;
        return new NativeViewerPointOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerPointOptions>(),
            ApiVersion = 1,
            UpdateMask = updateMask,
            Position = position,
            Marker = (int)marker,
            Scale = scale,
            Red = actualColor.R / 255.0,
            Green = actualColor.G / 255.0,
            Blue = actualColor.B / 255.0
        };
    }

    private void CheckPointStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }

    private OcctPoint CheckPoint(long id, string? operation = null)
    {
        if (id <= 0) throw CreateException(operation ?? nameof(AddPoint));
        return new OcctPoint(id, _ownerId);
    }

    private void EnsurePoint(OcctPoint point)
    {
        EnsureObject(point);
        if (NativeMethods.occt_object_kind(_handle, point.Id) != (int)OcctObjectKind.Point)
            throw new ArgumentException("Object is not a point object in this OcctEngine.", nameof(point));
    }
}
