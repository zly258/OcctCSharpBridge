namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Enables or disables automatic adjustment of the camera Z range.
    /// This improves depth precision and prevents clipping, but it does not separate two coplanar objects.
    /// </summary>
    public void SetAutoZFitMode(bool enabled, double scaleFactor = 1.0) =>
        CheckInitialized(() => DepthNativeMethods.occt_set_auto_z_fit_mode(
            _handle,
            enabled ? 1 : 0,
            scaleFactor));

    /// <summary>Returns the current automatic Z-range fitting settings.</summary>
    public OcctAutoZFitSettings GetAutoZFitSettings()
    {
        EnsureInitialized();
        Check(DepthNativeMethods.occt_get_auto_z_fit_mode(_handle, out var result));
        return new OcctAutoZFitSettings(result.Enabled != 0, result.ScaleFactor);
    }

    /// <summary>Recalculates the current camera Z range when automatic Z fitting is enabled.</summary>
    public void AutoZFit() => CheckInitialized(() => DepthNativeMethods.occt_auto_z_fit(_handle));

    /// <summary>
    /// Changes the default polygon offset inherited by future Viewer objects.
    /// OCCT's recommended shaded-view baseline is Fill, factor 1, units 1.
    /// </summary>
    public void SetDefaultPolygonOffsets(
        OcctPolygonOffsetMode mode,
        double factor = 1.0,
        double units = 1.0,
        bool applyExisting = false) =>
        CheckInitialized(() => DepthNativeMethods.occt_set_default_polygon_offsets(
            _handle,
            (int)mode,
            factor,
            units,
            applyExisting ? 1 : 0));

    /// <summary>Returns the polygon offset configured on the Viewer default drawer.</summary>
    public OcctPolygonOffsetSettings GetDefaultPolygonOffsets()
    {
        EnsureInitialized();
        Check(DepthNativeMethods.occt_get_default_polygon_offsets(_handle, out var result));
        return ToManaged(result);
    }

    /// <summary>
    /// Sets a per-object polygon offset. Use a negative Fill offset to draw a coplanar overlay
    /// in front of its reference object, or a larger positive value to push it behind.
    /// </summary>
    public void SetPolygonOffsets(
        IOcctObject value,
        OcctPolygonOffsetMode mode,
        double factor = 1.0,
        double units = 1.0)
    {
        ArgumentNullException.ThrowIfNull(value);
        CheckInitialized(() => DepthNativeMethods.occt_set_object_polygon_offsets(
            _handle,
            value.Id,
            (int)mode,
            factor,
            units));
    }

    /// <summary>Returns the effective polygon offset for a Viewer object.</summary>
    public OcctPolygonOffsetSettings GetPolygonOffsets(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureInitialized();
        Check(DepthNativeMethods.occt_get_object_polygon_offsets(_handle, value.Id, out var result));
        return ToManaged(result);
    }

    /// <summary>Restores a Viewer object's polygon offset to the current default drawer values.</summary>
    public void ResetPolygonOffsets(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        CheckInitialized(() => DepthNativeMethods.occt_reset_object_polygon_offsets(_handle, value.Id));
    }

    private static OcctPolygonOffsetSettings ToManaged(
        DepthNativeMethods.NativePolygonOffsetSettings value) =>
        new((OcctPolygonOffsetMode)value.Mode, value.Factor, value.Units);
}
