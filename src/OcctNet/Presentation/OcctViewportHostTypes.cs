namespace OcctNet;

public enum OcctViewportHostState
{
    Detached = 0,
    Initializing = 1,
    Ready = 2,
    Faulted = 3,
    Disposed = 4
}

public sealed class OcctViewportHostStateChangedEventArgs : EventArgs
{
    public OcctViewportHostStateChangedEventArgs(
        OcctViewportHostState previousState,
        OcctViewportHostState state,
        long generation)
    {
        if (!Enum.IsDefined(previousState)) throw new ArgumentOutOfRangeException(nameof(previousState));
        if (!Enum.IsDefined(state)) throw new ArgumentOutOfRangeException(nameof(state));
        if (generation < 0) throw new ArgumentOutOfRangeException(nameof(generation));

        PreviousState = previousState;
        State = state;
        Generation = generation;
    }

    public OcctViewportHostState PreviousState { get; }
    public OcctViewportHostState State { get; }
    public long Generation { get; }
}

public sealed class OcctViewportFaultedEventArgs : EventArgs
{
    public OcctViewportFaultedEventArgs(Exception exception, long generation)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        if (generation < 0) throw new ArgumentOutOfRangeException(nameof(generation));
        Generation = generation;
    }

    public Exception Exception { get; }
    public long Generation { get; }
}

public sealed class OcctEngineLifecycleEventArgs : EventArgs
{
    public OcctEngineLifecycleEventArgs(OcctEngine engine, long generation)
    {
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        if (generation <= 0) throw new ArgumentOutOfRangeException(nameof(generation));
        Generation = generation;
    }

    public OcctEngine Engine { get; }
    public long Generation { get; }
}

public interface IOcctViewportHost
{
    OcctEngine Engine { get; }
    bool IsEngineInitialized { get; }
    OcctViewportHostState HostState { get; }
    long EngineGeneration { get; }

    event EventHandler<OcctViewportHostStateChangedEventArgs>? HostStateChanged;
    event EventHandler<OcctViewportFaultedEventArgs>? Faulted;
    event EventHandler<OcctEngineLifecycleEventArgs>? EngineDisposing;
    event EventHandler<OcctEngineLifecycleEventArgs>? EngineRecreated;
}

public interface IOcctViewportInputSource
{
    OcctViewportInteractionFeatures InteractionFeatures { get; set; }

    event EventHandler<OcctPointerInputEventArgs>? PreviewPointerInput;
    event EventHandler<OcctPointerInputEventArgs>? PointerInput;
    event EventHandler<OcctKeyInputEventArgs>? PreviewKeyInput;
    event EventHandler<OcctKeyInputEventArgs>? KeyInput;
}
