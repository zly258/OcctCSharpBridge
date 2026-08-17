using System.Drawing;
using System.Windows.Forms;

namespace OcctNet;

public sealed partial class OcctViewportControl
{
    private Form? _hostForm;
    private FormWindowState _lastHostFormWindowState = FormWindowState.Normal;
    private bool _hostRestoreRefreshScheduled;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (DesignMode) return;

        AttachHostForm();
        ResetRenderReady();
        SetHostState(OcctViewportHostState.Initializing);
        var generation = ++_engineGeneration;
        SetNativeHandle(Handle, generation);
        try
        {
            var engine = new OcctEngine();
            _engine = engine;
            engine.InitializeNativeSurface(OcctNativeSurfaceKind.Auto, NativeHandle, redrawAfterInitialize: false);
            using (engine.BeginDisplayBatch())
            {
                engine.ResizeSurface();
                _initialOptions.Apply(engine);
                NotifyEngineRecreated(engine, generation);
            }
            _lastNativeSize = ClientSize;
            _lastHoverTimestamp = 0;
            _lastWorldPointTimestamp = 0;
            MarkFirstFrameRendered(generation);
            SetHostState(OcctViewportHostState.Ready);
        }
        catch (Exception exception)
        {
            SetHostFault(exception);
            DisposeCurrentEngine();
            throw;
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        DetachHostForm();
        _hostRestoreRefreshScheduled = false;
        HideSelectionFrame();
        _pressedKeys.Clear();
        _rotating = false;
        _panning = false;
        DisposeCurrentEngine();
        SetNativeHandle(IntPtr.Zero, _engineGeneration);
        SetHostState(OcctViewportHostState.Disposed);
        _lastNativeSize = Size.Empty;
        base.OnHandleDestroyed(e);
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        if (!DesignMode && IsHandleCreated) AttachHostForm();
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

    private void AttachHostForm()
    {
        var hostForm = FindForm();
        if (ReferenceEquals(_hostForm, hostForm)) return;

        DetachHostForm();
        _hostForm = hostForm;
        if (_hostForm is null) return;

        _lastHostFormWindowState = _hostForm.WindowState;
        _hostForm.Resize += OnHostFormResize;
    }

    private void DetachHostForm()
    {
        if (_hostForm is not null)
            _hostForm.Resize -= OnHostFormResize;
        _hostForm = null;
        _lastHostFormWindowState = FormWindowState.Normal;
    }

    private void OnHostFormResize(object? sender, EventArgs e)
    {
        if (!ReferenceEquals(sender, _hostForm) || _hostForm is null) return;

        var previousState = _lastHostFormWindowState;
        var currentState = _hostForm.WindowState;
        _lastHostFormWindowState = currentState;

        if (previousState == FormWindowState.Minimized && currentState != FormWindowState.Minimized)
            ScheduleHostRestoreRefresh();
    }

    private void ScheduleHostRestoreRefresh()
    {
        if (_hostRestoreRefreshScheduled
            || _engine?.IsInitialized != true
            || !IsHandleCreated
            || IsDisposed
            || Disposing)
        {
            return;
        }

        _hostRestoreRefreshScheduled = true;
        try
        {
            BeginInvoke((Action)(() =>
            {
                _hostRestoreRefreshScheduled = false;
                if (_hostForm is null || _hostForm.WindowState == FormWindowState.Minimized) return;

                // OCCT requires an explicit redraw after deiconification. Force the resize/redraw
                // path even when the restored client size is identical to the pre-minimize size.
                ResizeNativeView(force: true);
                if (IsActiveRectangleGesture && _rectangleDragStarted)
                    ScheduleSelectionFrameRestore();
            }));
        }
        catch (InvalidOperationException)
        {
            _hostRestoreRefreshScheduled = false;
        }
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

    private void DisposeCurrentEngine()
    {
        ResetRenderReady();
        var engine = _engine;
        _engine = null;
        if (engine is null) return;

        NotifyEngineDisposing(engine, _engineGeneration);
        try { engine.Dispose(); }
        catch (Exception exception) { ReportLifecycleError(exception); }
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
        ResetRenderReady();
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
