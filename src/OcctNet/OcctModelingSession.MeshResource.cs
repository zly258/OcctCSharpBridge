namespace OcctNet;

public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Builds an owned mesh snapshot that remains valid after its source registry entry is deleted.
    /// </summary>
    public OcctMeshResource CreateMeshResource(
        OcctModelShape shape,
        OcctModelMeshParameters? parameters = null)
    {
        EnsureShape(shape);
        var actual = parameters ?? OcctModelMeshParameters.Default;
        OcctGuard.Positive(actual.LinearDeflection, nameof(actual.LinearDeflection));
        OcctGuard.Positive(actual.AngularDeflection, nameof(actual.AngularDeflection));
        OcctGuard.NonNegative(actual.MinimumSize, nameof(actual.MinimumSize));
        var native = actual.ToResourceNative();
        var status = ModelNativeMethods.occt_model_mesh_create(
            _handle,
            shape.Id,
            in native,
            out var nativeHandle);
        if (status != OcctStatus.Ok)
            throw CreateException(nameof(CreateMeshResource));

        var safeHandle = OcctMeshSafeHandle.AdoptOwned(nativeHandle);
        if (safeHandle.IsInvalid)
        {
            safeHandle.Dispose();
            throw new OcctException(
                "Native mesh creation returned an invalid handle.",
                OcctStatus.ErrorUnknown,
                nameof(CreateMeshResource));
        }
        return new OcctMeshResource(safeHandle);
    }
}
