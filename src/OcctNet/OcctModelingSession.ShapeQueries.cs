using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public long GetShapeHash(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_hash(_handle, shape.Id);
    }

    public OcctShapeType GetShapeType(OcctModelShape shape)
    {
        EnsureShape(shape);
        return (OcctShapeType)ModelNativeMethods.occt_model_shape_type(_handle, shape.Id);
    }

    public OcctModelOrientation GetShapeOrientation(OcctModelShape shape)
    {
        EnsureShape(shape);
        return (OcctModelOrientation)ModelNativeMethods.occt_model_shape_orientation(_handle, shape.Id);
    }

    public bool IsShapeClosed(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_is_closed(_handle, shape.Id) != 0;
    }

    public bool IsShapeValid(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_is_valid(_handle, shape.Id) != 0;
    }

    public double GetShapeMaximumTolerance(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_tolerance(_handle, shape.Id);
    }

    public string GetShapeCheckReport(OcctModelShape shape)
    {
        EnsureShape(shape);
        return Marshal.PtrToStringUTF8(ModelNativeMethods.occt_model_check_report(_handle, shape.Id)) ?? string.Empty;
    }

    public OcctBounds GetShapeBounds(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_shape_bounds(_handle, shape.Id, out var result));
        return result;
    }

    public OcctMassProperties GetShapeLinearProperties(OcctModelShape shape) =>
        GetProperties(shape, ModelNativeMethods.occt_model_shape_linear_properties);

    public OcctMassProperties GetShapeSurfaceProperties(OcctModelShape shape) =>
        GetProperties(shape, ModelNativeMethods.occt_model_shape_surface_properties);

    public OcctMassProperties GetShapeVolumeProperties(OcctModelShape shape) =>
        GetProperties(shape, ModelNativeMethods.occt_model_shape_volume_properties);

    public OcctDistanceResult GetShapeDistance(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        Check(ModelNativeMethods.occt_model_shape_distance(_handle, first.Id, second.Id, out var result));
        return result;
    }

    public OcctModelLocation GetShapeLocation(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_get_location(_handle, shape.Id, out var result));
        return result;
    }

    public OcctModelShape SetShapeLocation(
        OcctModelShape shape,
        OcctModelLocation location,
        bool copyShape = true)
    {
        EnsureShape(shape);
        if (!location.IsFinite)
            throw new ArgumentException("Location matrix must contain only finite values.", nameof(location));
        return CheckShape(ModelNativeMethods.occt_model_set_location(
            _handle,
            shape.Id,
            in location,
            copyShape ? 1 : 0));
    }

    /// <summary>
    /// Bridge 2.5 source-compatibility entry point. New code should use
    /// <see cref="SetShapeLocation(OcctModelShape, OcctModelLocation, bool)"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OcctModelShape SetLocation(
        OcctModelShape shape,
        OcctModelLocation location,
        bool copyShape = true) =>
        SetShapeLocation(shape, location, copyShape);
}
