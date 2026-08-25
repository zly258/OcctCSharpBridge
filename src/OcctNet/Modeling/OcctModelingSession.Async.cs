namespace OcctNet;

public sealed partial class OcctModelingSession
{
    // Async wrappers serialize work submitted through this API so that callers
    // cannot accidentally execute two native operations on the same session.
    // Synchronous calls are not included in this gate and must not be mixed with
    // an in-flight async operation.
    private readonly SemaphoreSlim _asyncOperationGate = new(1, 1);

    private async Task<T> RunExclusiveAsync<T>(
        Func<T> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _asyncOperationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Once the native OCCT call has started it cannot be interrupted safely.
            // Do not pass the token to Task.Run: cancellation remains cooperative
            // while queued and never reports a running native operation as cancelled.
            return await Task.Run(operation, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            _asyncOperationGate.Release();
        }
    }

    private async Task RunExclusiveAsync(
        Action operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await RunExclusiveAsync(
            () =>
            {
                operation();
                return true;
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes a boolean operation on the thread pool.
    /// </summary>
    /// <remarks>
    /// Async operations submitted through this session are serialized. Cancellation
    /// is honored while the operation is queued; an OCCT call already in progress
    /// runs to completion. Do not invoke synchronous methods on this session while
    /// an async operation is running.
    /// </remarks>
    public Task<OcctModelAlgorithmResult> BooleanAsync(
        OcctBooleanOperation operation,
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => Boolean(operation, left, right, options), cancellationToken);

    /// <inheritdoc cref="Fuse(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> FuseAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => Fuse(left, right, options), cancellationToken);

    /// <inheritdoc cref="Cut(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> CutAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => Cut(left, right, options), cancellationToken);

    /// <inheritdoc cref="Common(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> CommonAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => Common(left, right, options), cancellationToken);

    /// <inheritdoc cref="Section(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> SectionAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => Section(left, right, options), cancellationToken);

    /// <inheritdoc cref="Split(System.Collections.Generic.IEnumerable{OcctModelShape}, System.Collections.Generic.IEnumerable{OcctModelShape}, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> SplitAsync(
        IEnumerable<OcctModelShape> objects,
        IEnumerable<OcctModelShape> tools,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => Split(objects, tools, options), cancellationToken);

    /// <summary>
    /// Asynchronously exports shapes to a STEP file on the thread pool.
    /// </summary>
    public Task ExportStepAsync(
        OcctModelShape shape,
        string path,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => ExportStep(shape, path), cancellationToken);

    /// <summary>
    /// Asynchronously imports a STEP file on the thread pool.
    /// </summary>
    public Task<OcctModelShape> ImportStepAsync(
        string path,
        CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(() => ImportStep(path), cancellationToken);
}
