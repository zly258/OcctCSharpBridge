namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public long GetShapeHash(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_hash(_handle, shape.Id, out var result));
        return result;
    }

    public OcctShapeType GetShapeType(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_type(_handle, shape.Id, out var result));
        return result;
    }

    public OcctModelOrientation GetShapeOrientation(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_orientation(_handle, shape.Id, out var result));
        return result;
    }

    public bool IsShapeClosed(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_is_closed(_handle, shape.Id, out var result));
        return result != 0;
    }

    public bool IsShapeValid(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_is_valid(_handle, shape.Id, out var result));
        return result != 0;
    }

    public double GetShapeMaximumTolerance(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_tolerance(_handle, shape.Id, out var result));
        return result;
    }

    public string GetShapeCheckReport(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ReadUtf8Buffer((byte[]? buffer, int capacity, out int required) =>
            ModelNativeMethods.occt_model_shape_check_report_get(
                _handle,
                shape.Id,
                buffer,
                capacity,
                out required));
    }

    public OcctBounds GetShapeBounds(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_bounds(_handle, shape.Id, out var result));
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
        CheckStatus(ModelNativeMethods.occt_model_shape_distance(_handle, first.Id, second.Id, out var result));
        return result;
    }

    public OcctModelLocation GetShapeLocation(OcctModelShape shape)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_location_get(_handle, shape.Id, out var result));
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

        CheckStatus(ModelNativeMethods.occt_model_shape_location_set(
            _handle,
            shape.Id,
            in location,
            copyShape ? 1 : 0,
            out var result));
        return CheckShape(result);
    }
}
