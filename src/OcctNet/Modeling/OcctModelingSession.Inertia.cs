namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctInertiaProperties GetLinearInertiaProperties(OcctModelShape shape) =>
        GetInertiaProperties(shape, ModelNativeMethods.occt_model_shape_linear_inertia);

    public OcctInertiaProperties GetSurfaceInertiaProperties(OcctModelShape shape) =>
        GetInertiaProperties(shape, ModelNativeMethods.occt_model_shape_surface_inertia);

    public OcctInertiaProperties GetVolumeInertiaProperties(OcctModelShape shape) =>
        GetInertiaProperties(shape, ModelNativeMethods.occt_model_shape_volume_inertia);

    private OcctInertiaProperties GetInertiaProperties(OcctModelShape shape, InertiaQuery query)
    {
        EnsureShape(shape);
        CheckStatus(query(_handle, shape.Id, out var result));
        return result.ToManaged();
    }

    private delegate OcctStatus InertiaQuery(
        OcctModelingSafeHandle handle,
        long shapeId,
        out NativeModelInertiaProperties result);
}
