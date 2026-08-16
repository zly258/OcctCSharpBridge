using System.Drawing;
using System.Windows.Forms;

namespace OcctNet;

public sealed partial class OcctViewportControl
{
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (DesignMode) return;

        SetHostState(OcctViewportHostState.Initializing);
        var generation = ++_engineGeneration;
        try
        {
            var engine = new OcctEngine();
            _engine = engine;
            engine.Initialize(Handle);
            _lastNativeSize = ClientSize;
            _lastHoverTimestamp = 0;
            _lastWorldPointTimestamp = 0;
            NotifyEngineRecreated(engine, generation);
            SetHostState(OcctViewportHostState.Ready);
        }
        catch (Exception exception)
        {
            SetHostFault(exception);
            DisposeCurrentEngine(transitionToDisposed: false);
            throw;
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        HideSelectionFrame();
        _pressedKeys.Clear();
        _rotating = false;
        _panning = false;
        DisposeCurrentEngine(transitionToDisposed: true);
        _lastNativeSize = Size.Empty;
        base.OnHandleDestroyed(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        var restoreRectangle = IsActiveRectangleGesture && _rectangleDragStarted;
        if (restoreRectangle) HideSelectionFrame();
        ResizeNativeView();
        if (restoreRectangle) ScheduleSelectionFrameRestore();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            ResizeNativeView(force: true);
            if (IsActiveRectangleGesture && _rectangleDragStarted)
                ScheduleSelectionFrameRestore();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        if (_engine?.IsInitialized != true) base.OnPaintBackground(pevent);
    }

    private void ResizeNativeView(bool force = false)
    {
        if (_engine?.IsInitialized != true
            || !Visible
            || ClientSize.Width <= 0
            || ClientSize.Height <= 0)
        {
            return;
        }

        if (!force && _lastNativeSize == ClientSize)
            return;

        _lastNativeSize = ClientSize;
        TryInvoke(_engine.Resize);
    }

    private void DisposeCurrentEngine(bool transitionToDisposed)
    {
        var engine = _engine;
        _engine = null;
        if (engine is not null)
        {
            NotifyEngineDisposing(engine, _engineGeneration);
            try { engine.Dispose(); }
            catch (Exception exception) { ReportLifecycleError(exception); }
        }

        if (transitionToDisposed)
            SetHostState(OcctViewportHostState.Disposed);
    }

    private void SetHostState(OcctViewportHostState state)
    {
        if (_hostState == state) return;
        var previous = _hostState;
        _hostState = state;
        try
        {
            HostStateChanged?.Invoke(
                this,
                new OcctViewportHostStateChangedEventArgs(previous, state, _engineGeneration));
        }
        catch (Exception exception)
        {
            ReportLifecycleError(exception);
        }
    }

    private void SetHostFault(Exception exception)
    {
        SetHostState(OcctViewportHostState.Faulted);
        try { Faulted?.Invoke(this, new OcctViewportFaultedEventArgs(exception, _engineGeneration)); }
        catch (Exception handlerException) { ReportLifecycleError(handlerException); }
    }

    private void NotifyEngineRecreated(OcctEngine engine, long generation)
    {
        try { EngineRecreated?.Invoke(this, new OcctEngineLifecycleEventArgs(engine, generation)); }
        catch (Exception exception) { ReportLifecycleError(exception); }
    }

    private void NotifyEngineDisposing(OcctEngine engine, long generation)
    {
        try { EngineDisposing?.Invoke(this, new OcctEngineLifecycleEventArgs(engine, generation)); }
        catch (Exception exception) { ReportLifecycleError(exception); }
    }

    private void ReportLifecycleError(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine(exception);
        try { ErrorOccurred?.Invoke(this, new OcctViewportErrorEventArgs(exception)); }
        catch (Exception handlerException) { System.Diagnostics.Debug.WriteLine(handlerException); }
    }
}
