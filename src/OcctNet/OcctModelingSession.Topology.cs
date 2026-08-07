namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        return ModelNativeMethods.occt_model_topology_count(_handle, shape.Id, (int)type);
    }

    public OcctModelShape GetSubshapeAt(OcctModelShape shape, OcctShapeType type, int index)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        return CheckShape(ModelNativeMethods.occt_model_get_subshape(_handle, shape.Id, (int)type, index));
    }

    public OcctModelShape GetSubshape(OcctModelShape shape, OcctShapeType type, int index) => GetSubshapeAt(shape, type, index);

    public IReadOnlyList<OcctModelShape> GetSubshapes(OcctModelShape shape, OcctShapeType type) =>
        Enumerable.Range(0, GetTopologyCount(shape, type))
            .Select(index => GetSubshapeAt(shape, type, index))
            .ToArray();

    public OcctModelShape GetOuterWire(OcctModelShape face)
    {
        EnsureShape(face);
        return CheckShape(ModelNativeMethods.occt_model_outer_wire(_handle, face.Id));
    }

    public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
    {
        EnsureShape(face);
        var count = ModelNativeMethods.occt_model_inner_wire_count(_handle, face.Id);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(ModelNativeMethods.occt_model_inner_wire_at(_handle, face.Id, index)))
            .ToArray();
    }

    public IReadOnlyList<OcctModelShape> GetAncestors(
        OcctModelShape root,
        OcctModelShape child,
        OcctShapeType ancestorType)
    {
        EnsureShape(root);
        EnsureShape(child);
        if (!Enum.IsDefined(ancestorType)) throw new ArgumentOutOfRangeException(nameof(ancestorType));
        var count = ModelNativeMethods.occt_model_ancestor_count(_handle, root.Id, child.Id, (int)ancestorType);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(ModelNativeMethods.occt_model_ancestor_at(
                _handle, root.Id, child.Id, (int)ancestorType, index)))
            .ToArray();
    }
}
