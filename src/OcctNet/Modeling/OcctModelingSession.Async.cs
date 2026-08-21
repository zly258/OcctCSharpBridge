namespace OcctNet;

public sealed partial class OcctModelingSession
{
    // -------------------------------------------------------------------------
    // Async wrappers for CPU-intensive modeling operations.
    // These offload the blocking native call to the thread pool so that
    // UI threads remain responsive during long-running CAD operations.
    // IMPORTANT: OcctModelingSession is NOT thread-safe; the caller must ensure
    // that no other operation runs concurrently on the same session.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Asynchronously executes a boolean operation on the thread pool.
    /// </summary>
    /// <remarks>
    /// The modeling session is not thread-safe. Ensure no other calls are made
    /// on this session while the task is running.
    /// </remarks>
    public Task<OcctModelAlgorithmResult> BooleanAsync(
        OcctBooleanOperation operation,
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Boolean(operation, left, right, options), cancellationToken);

    /// <inheritdoc cref="Fuse(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> FuseAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Fuse(left, right, options), cancellationToken);

    /// <inheritdoc cref="Cut(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> CutAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Cut(left, right, options), cancellationToken);

    /// <inheritdoc cref="Common(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> CommonAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Common(left, right, options), cancellationToken);

    /// <inheritdoc cref="Section(OcctModelShape, OcctModelShape, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> SectionAsync(
        OcctModelShape left,
        OcctModelShape right,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Section(left, right, options), cancellationToken);

    /// <inheritdoc cref="Split(System.Collections.Generic.IEnumerable{OcctModelShape}, System.Collections.Generic.IEnumerable{OcctModelShape}, OcctModelBooleanOptions?)"/>
    public Task<OcctModelAlgorithmResult> SplitAsync(
        IEnumerable<OcctModelShape> objects,
        IEnumerable<OcctModelShape> tools,
        OcctModelBooleanOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Split(objects, tools, options), cancellationToken);

    /// <summary>
    /// Asynchronously exports shapes to a STEP file on the thread pool.
    /// </summary>
    public Task ExportStepAsync(
        OcctModelShape shape,
        string path,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ExportStep(shape, path), cancellationToken);

    /// <summary>
    /// Asynchronously imports a STEP file on the thread pool.
    /// </summary>
    public Task<OcctModelShape> ImportStepAsync(
        string path,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => ImportStep(path), cancellationToken);
}
