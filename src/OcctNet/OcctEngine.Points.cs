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

    public OcctPoint AddPoint(OcctPoint3d position, OcctMarkerPixmap marker)
    {
        ValidateMarkerPixmap(marker);
        return AddPointPixmap(position, marker.Width, marker.Height, marker.Pixels, marker.PixelFormat);
    }

    public unsafe OcctPoint AddPointPixmap(
        OcctPoint3d position,
        int width,
        int height,
        ReadOnlySpan<byte> pixels,
        OcctPixelFormat pixelFormat = OcctPixelFormat.Bgra32)
    {
        OcctGuard.Finite(position, nameof(position));
        ValidatePointPixmap(width, height, pixels, pixelFormat);
        EnsureInitialized();

        fixed (byte* pixelPointer = pixels)
        {
            var options = PointPixmapOptions(
                NativeViewerPointPixmapUpdateMask.Position | NativeViewerPointPixmapUpdateMask.Image,
                position,
                width,
                height,
                (IntPtr)pixelPointer,
                pixels.Length,
                pixelFormat);
            var status = NativeMethods.occt_engine_point_pixmap_create(_handle, in options, out var pointId);
            if (status != OcctStatus.Ok) throw CreateException(nameof(AddPointPixmap));
            return CheckPoint(pointId, nameof(AddPointPixmap));
        }
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

    public void SetPointStyle(OcctPoint point, OcctMarkerPixmap marker)
    {
        ValidateMarkerPixmap(marker);
        SetPointPixmapStyle(point, marker.Width, marker.Height, marker.Pixels, marker.PixelFormat);
    }

    public unsafe void SetPointPixmapStyle(
        OcctPoint point,
        int width,
        int height,
        ReadOnlySpan<byte> pixels,
        OcctPixelFormat pixelFormat = OcctPixelFormat.Bgra32)
    {
        EnsurePoint(point);
        ValidatePointPixmap(width, height, pixels, pixelFormat);

        fixed (byte* pixelPointer = pixels)
        {
            var options = PointPixmapOptions(
                NativeViewerPointPixmapUpdateMask.Image,
                default,
                width,
                height,
                (IntPtr)pixelPointer,
                pixels.Length,
                pixelFormat);
            CheckPointStatus(NativeMethods.occt_engine_point_pixmap_update(_handle, point.Id, in options));
        }
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

    private static NativeViewerPointPixmapOptions PointPixmapOptions(
        NativeViewerPointPixmapUpdateMask updateMask,
        OcctPoint3d position,
        int width,
        int height,
        IntPtr pixels,
        int pixelCount,
        OcctPixelFormat pixelFormat) => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerPointPixmapOptions>(),
        ApiVersion = 1,
        UpdateMask = updateMask,
        Position = position,
        Width = width,
        Height = height,
        Pixels = pixels,
        PixelCount = pixelCount,
        PixelFormat = (int)pixelFormat
    };

    private static void ValidatePointPixmap(
        int width,
        int height,
        ReadOnlySpan<byte> pixels,
        OcctPixelFormat pixelFormat)
    {
        if (width <= 0 || width > 4096) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0 || height > 4096) throw new ArgumentOutOfRangeException(nameof(height));
        if (!Enum.IsDefined(pixelFormat)) throw new ArgumentOutOfRangeException(nameof(pixelFormat));

        int required;
        try { required = checked(width * height * 4); }
        catch (OverflowException) { throw new ArgumentOutOfRangeException(nameof(width), "Pixmap dimensions are too large."); }
        if (pixels.Length != required)
        {
            throw new ArgumentException(
                $"Pixmap buffer must contain exactly {required} bytes for a {width}x{height} 32-bit image.",
                nameof(pixels));
        }
    }

    private static void ValidateMarkerPixmap(OcctMarkerPixmap marker)
    {
        ArgumentNullException.ThrowIfNull(marker);
        ArgumentNullException.ThrowIfNull(marker.Pixels);
        ValidatePointPixmap(marker.Width, marker.Height, marker.Pixels, marker.PixelFormat);
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
        if (QueryObjectKind(point.Id) != OcctObjectKind.Point)
            throw new ArgumentException("Object is not a point object in this OcctEngine.", nameof(point));
    }
}
