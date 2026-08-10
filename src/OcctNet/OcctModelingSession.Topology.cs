namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        var count = ModelNativeMethods.occt_model_subshapes_copy(_handle, shape.Id, (int)type, null, 0);
        if (count < 0) throw CreateException();
        return count;
    }

    public IReadOnlyList<OcctModelShape> GetSubshapes(OcctModelShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        return ReadShapeCollection(
            (buffer, capacity) => ModelNativeMethods.occt_model_subshapes_copy(
                _handle,
                shape.Id,
                (int)type,
                buffer,
                capacity));
    }

    public OcctModelShape GetOuterWire(OcctModelShape face)
    {
        EnsureShape(face);
        return CheckShape(ModelNativeMethods.occt_model_outer_wire(_handle, face.Id));
    }

    public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
    {
        EnsureShape(face);
        return ReadShapeCollection(
            (buffer, capacity) => ModelNativeMethods.occt_model_inner_wires_copy(
                _handle,
                face.Id,
                buffer,
                capacity));
    }

    public IReadOnlyList<OcctModelShape> GetAncestors(
        OcctModelShape root,
        OcctModelShape child,
        OcctShapeType ancestorType)
    {
        EnsureShape(root);
        EnsureShape(child);
        if (!Enum.IsDefined(ancestorType)) throw new ArgumentOutOfRangeException(nameof(ancestorType));
        return ReadShapeCollection(
            (buffer, capacity) => ModelNativeMethods.occt_model_ancestors_copy(
                _handle,
                root.Id,
                child.Id,
                (int)ancestorType,
                buffer,
                capacity));
    }

    private delegate int ShapeCollectionCopy(long[]? buffer, int capacity);

    private IReadOnlyList<OcctModelShape> ReadShapeCollection(ShapeCollectionCopy copy)
    {
        var count = copy(null, 0);
        if (count < 0) throw CreateException();
        if (count == 0) return Array.Empty<OcctModelShape>();

        var ids = new long[count];
        var copied = copy(ids, ids.Length);
        if (copied < 0) throw CreateException();
        if (copied != count) throw new InvalidOperationException("Native topology result count changed during bulk copy.");

        var result = new OcctModelShape[count];
        for (var index = 0; index < count; index++)
            result[index] = CheckShape(ids[index]);
        return result;
    }
}
