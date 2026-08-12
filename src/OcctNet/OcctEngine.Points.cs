using System.Drawing;

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
        return CheckPoint(NativeMethods.occt_add_point(
            _handle,
            position,
            (int)marker,
            scale,
            value.R / 255.0,
            value.G / 255.0,
            value.B / 255.0));
    }

    public void SetPointPosition(OcctPoint point, OcctPoint3d position)
    {
        EnsurePoint(point);
        OcctGuard.Finite(position, nameof(position));
        CheckInitialized(() => NativeMethods.occt_set_point_position(_handle, point.Id, position));
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
        CheckInitialized(() => NativeMethods.occt_set_point_style(
            _handle,
            point.Id,
            (int)marker,
            scale,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0));
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
