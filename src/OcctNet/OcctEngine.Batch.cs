using System.Threading;

namespace OcctNet;

/// <summary>
/// Defers OCCT viewer updates until the batch is disposed. Batches can be nested.
/// </summary>
public sealed class OcctDisplayBatch : IDisposable
{
    private OcctEngine? _engine;

    internal OcctDisplayBatch(OcctEngine engine, bool fitAllOnDispose)
    {
        _engine = engine;
        FitAllOnDispose = fitAllOnDispose;
    }

    /// <summary>Fits all displayed objects before the final redraw when this outermost batch ends.</summary>
    public bool FitAllOnDispose { get; set; }

    public void Dispose()
    {
        var engine = Interlocked.Exchange(ref _engine, null);
        if (engine is not null) engine.EndDisplayBatch(FitAllOnDispose);
    }
}

public sealed partial class OcctEngine
{
    /// <summary>Returns true while one or more display update batches are active.</summary>
    public bool IsDisplayBatchActive
    {
        get
        {
            EnsureInitialized();
            return BatchNativeMethods.occt_is_updating(_handle) != 0;
        }
    }

    /// <summary>
    /// Defers Display, Redisplay and view redraw work until the returned scope is disposed.
    /// Use this when creating or changing several objects in one operation.
    /// </summary>
    public OcctDisplayBatch BeginDisplayBatch(bool fitAllOnDispose = false)
    {
        EnsureInitialized();
        Check(BatchNativeMethods.occt_begin_update(_handle));
        return new OcctDisplayBatch(this, fitAllOnDispose);
    }

    internal void EndDisplayBatch(bool fitAll)
    {
        if (_handle == IntPtr.Zero || !_initialized) return;
        Check(BatchNativeMethods.occt_end_update(_handle, fitAll ? 1 : 0));
    }
}
