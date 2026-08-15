namespace OcctNet;

public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Captures immutable diagnostics for a completed modeling algorithm.
    /// The returned resource remains valid after this session is disposed.
    /// </summary>
    public OcctAlgorithmResource AcquireAlgorithm(OcctModelAlgorithmResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        EnsureNotDisposed();
        if (result.OperationId <= 0 || result.Shape.OwnerId != _ownerId)
            throw new ArgumentException("Algorithm result does not belong to this modeling session.", nameof(result));

        var status = ModelNativeMethods.occt_model_algorithm_acquire(
            _handle,
            result.OperationId,
            out var nativeHandle);
        if (status != OcctStatus.Ok)
            throw CreateException(nameof(AcquireAlgorithm));

        var safeHandle = OcctAlgorithmSafeHandle.AdoptOwned(nativeHandle);
        if (safeHandle.IsInvalid)
        {
            safeHandle.Dispose();
            throw new OcctException(
                "Native algorithm acquisition returned an invalid handle.",
                OcctStatus.ErrorUnknown,
                nameof(AcquireAlgorithm));
        }

        try
        {
            return new OcctAlgorithmResource(safeHandle);
        }
        catch
        {
            safeHandle.Dispose();
            throw;
        }
    }
}
