namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public bool IsSameShape(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        CheckStatus(ModelNativeMethods.occt_model_shape_is_same(
            _handle,
            first.Id,
            second.Id,
            out var result));
        return result != 0;
    }

    public bool IsPartnerShape(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        CheckStatus(ModelNativeMethods.occt_model_shape_is_partner(
            _handle,
            first.Id,
            second.Id,
            out var result));
        return result != 0;
    }

    public OcctOrientedBounds GetShapeOrientedBounds(OcctModelShape shape, bool optimal = false)
    {
        EnsureShape(shape);
        CheckStatus(ModelNativeMethods.occt_model_shape_oriented_bounds(
            _handle,
            shape.Id,
            optimal ? 1 : 0,
            out var result));
        return result;
    }
}
