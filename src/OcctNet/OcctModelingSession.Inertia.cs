namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctInertiaProperties GetLinearInertiaProperties(OcctModelShape shape) =>
        GetInertiaProperties(shape, ModelNativeMethods.occt_model_shape_linear_inertia);

    public OcctInertiaProperties GetSurfaceInertiaProperties(OcctModelShape shape) =>
        GetInertiaProperties(shape, ModelNativeMethods.occt_model_shape_surface_inertia);

    public OcctInertiaProperties GetVolumeInertiaProperties(OcctModelShape shape) =>
        GetInertiaProperties(shape, ModelNativeMethods.occt_model_shape_volume_inertia);

    private OcctInertiaProperties GetInertiaProperties(
        OcctModelShape shape,
        InertiaPropertiesQuery query)
    {
        EnsureShape(shape);
        Check(query(_handle, shape.Id, out var result));
        return result.ToManaged();
    }

    private delegate int InertiaPropertiesQuery(IntPtr handle, long shapeId, out NativeModelInertiaProperties result);
}
