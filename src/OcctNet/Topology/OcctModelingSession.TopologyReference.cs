namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctTopologyReference CreateTopologyReference(OcctModelShape root, OcctModelShape subshape)
    {
        EnsureShape(root);
        EnsureShape(subshape);
        CheckStatus(ModelNativeMethods.occt_model_create_topology_reference(
            _handle,
            root.Id,
            subshape.Id,
            out var result));
        return result.ToManaged();
    }

    public OcctTopologyReferenceResult ResolveTopologyReference(
        OcctModelShape root,
        OcctTopologyReference reference,
        double matchingTolerance = 1e-6)
    {
        EnsureShape(root);
        OcctGuard.NonNegative(matchingTolerance, nameof(matchingTolerance));

        var nativeReference = NativeModelTopologyReference.FromManaged(reference);
        CheckStatus(ModelNativeMethods.occt_model_resolve_topology_reference(
            _handle,
            root.Id,
            in nativeReference,
            matchingTolerance,
            out var result));
        return result.ToManaged(_ownerId);
    }

    public OcctTopologyReferenceResult ResolveTopologyReference(
        OcctModelShape root,
        OcctTopologyReference reference,
        long operationId,
        OcctModelShape sourceShape,
        double matchingTolerance = 1e-6)
    {
        EnsureShape(root);
        EnsureShape(sourceShape);
        if (operationId <= 0) throw new ArgumentOutOfRangeException(nameof(operationId));
        OcctGuard.NonNegative(matchingTolerance, nameof(matchingTolerance));

        var nativeReference = NativeModelTopologyReference.FromManaged(reference);
        CheckStatus(ModelNativeMethods.occt_model_resolve_topology_reference_with_history(
            _handle,
            root.Id,
            operationId,
            sourceShape.Id,
            in nativeReference,
            matchingTolerance,
            out var result));
        return result.ToManaged(_ownerId);
    }
}
