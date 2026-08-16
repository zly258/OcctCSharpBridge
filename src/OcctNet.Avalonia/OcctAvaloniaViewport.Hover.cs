namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
    private EventHandler<OcctViewportHoverHitChangedEventArgs>? _hoverHitChanged;
    private OcctEngine? _hoverEngine;
    private bool _hoverLifecycleHooked;

    public event EventHandler<OcctViewportHoverHitChangedEventArgs>? HoverHitChanged
    {
        add
        {
            _hoverHitChanged += value;
            EnsureHoverLifecycleHook();
            if (_engine is not null) AttachHoverEngine(_engine);
        }
        remove
        {
            _hoverHitChanged -= value;
            if (_hoverHitChanged is null) DetachHoverEngine();
        }
    }

    private void EnsureHoverLifecycleHook()
    {
        if (_hoverLifecycleHooked) return;
        _hoverLifecycleHooked = true;
        EngineRecreated += OnHoverEngineRecreated;
        EngineDisposing += OnHoverEngineDisposing;
    }

    private void OnHoverEngineRecreated(object? sender, OcctEngineLifecycleEventArgs e) =>
        AttachHoverEngine(e.Engine);

    private void OnHoverEngineDisposing(object? sender, OcctEngineLifecycleEventArgs e)
    {
        if (ReferenceEquals(_hoverEngine, e.Engine)) DetachHoverEngine();
    }

    private void AttachHoverEngine(OcctEngine engine)
    {
        if (_hoverHitChanged is null || ReferenceEquals(_hoverEngine, engine)) return;
        DetachHoverEngine();
        _hoverEngine = engine;
        _hoverEngine.DetectedHitChanged += RelayHoverHitChanged;
    }

    private void DetachHoverEngine()
    {
        if (_hoverEngine is null) return;
        _hoverEngine.DetectedHitChanged -= RelayHoverHitChanged;
        _hoverEngine = null;
    }

    private void RelayHoverHitChanged(object? sender, OcctViewportHoverHitChangedEventArgs e) =>
        _hoverHitChanged?.Invoke(this, e);
}
