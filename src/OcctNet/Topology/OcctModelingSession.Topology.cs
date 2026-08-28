namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        CheckStatus(ModelNativeMethods.occt_model_subshapes_snapshot_get(
            _handle,
            shape.Id,
            (int)type,
            null,
            0,
            out var count));
        return count;
    }

    public IReadOnlyList<OcctModelShape> GetSubshapes(OcctModelShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        return ReadShapeCollection(
            (long[]? buffer, int capacity, out int required) =>
                ModelNativeMethods.occt_model_subshapes_snapshot_get(
                    _handle,
                    shape.Id,
                    (int)type,
                    buffer,
                    capacity,
                    out required));
    }

    public OcctModelShape GetOuterWire(OcctModelShape face)
    {
        EnsureShape(face);
        var status = ModelNativeMethods.occt_model_outer_wire_get(_handle, face.Id, out var result);
        return CheckShape(status, result);
    }

    public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
    {
        EnsureShape(face);
        return ReadShapeCollection(
            (long[]? buffer, int capacity, out int required) =>
                ModelNativeMethods.occt_model_inner_wires_snapshot_get(
                    _handle,
                    face.Id,
                    buffer,
                    capacity,
                    out required));
    }

    public IReadOnlyList<OcctModelShape> GetWireEdges(OcctModelShape wire)
    {
        EnsureShape(wire);
        return ReadShapeCollection(
            (long[]? buffer, int capacity, out int required) =>
                ModelNativeMethods.occt_model_wire_edges_snapshot_get(
                    _handle,
                    wire.Id,
                    buffer,
                    capacity,
                    out required));
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
            (long[]? buffer, int capacity, out int required) =>
                ModelNativeMethods.occt_model_ancestors_snapshot_get(
                    _handle,
                    root.Id,
                    child.Id,
                    (int)ancestorType,
                    buffer,
                    capacity,
                    out required));
    }

    private delegate OcctStatus ShapeCollectionSnapshot(long[]? buffer, int capacity, out int required);

    private IReadOnlyList<OcctModelShape> ReadShapeCollection(ShapeCollectionSnapshot snapshot)
    {
        CheckStatus(snapshot(null, 0, out var count));
        if (count == 0) return Array.Empty<OcctModelShape>();

        var ids = new long[count];
        CheckStatus(snapshot(ids, ids.Length, out var required));
        if (required != count)
            throw new InvalidOperationException("Native topology result count changed during snapshot copy.");

        var result = new OcctModelShape[count];
        for (var index = 0; index < count; index++)
            result[index] = CheckShape(ids[index]);
        return result;
    }
}
