using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace OcctNet;

public sealed partial class OcctWpfViewport
{
    private Window? _hostWindow;
    private WindowState _lastHostWindowState = WindowState.Normal;

    /// <summary>
    /// Synchronizes the OCCT render target with the current child HWND size and
    /// coalesces presentation into one WPF render-priority callback.
    /// </summary>
    public void RefreshNativeView()
    {
        if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero || !HasUsableRenderSize()) return;

        SynchronizeDpi();
        TryInvoke(() =>
        {
            if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero || !HasUsableRenderSize()) return;
            _engine.ResizeSurface();
            _engine.Redraw();
            CompleteFirstFrameIfNeeded();
        });
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("OcctNet.Wpf supports Windows HWND hosting only.");

        ResetRenderReady();
        SetHostState(OcctViewportHostState.Initializing);
        var generation = ++_engineGeneration;
        IntPtr handle = IntPtr.Zero;
        try
        {
            OcctWpfRenderWindowClass.EnsureRegistered();
            handle = CreateWindowExW(
                0,
                OcctWpfRenderWindowClass.Name,
                string.Empty,
                WsChild | WsClipSiblings | WsClipChildren,
                0,
                0,
                100,
                100,
                hwndParent.Handle,
                IntPtr.Zero,
                OcctWpfRenderWindowClass.ModuleHandle,
                IntPtr.Zero);

            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    $"Unable to create the WPF OCCT child HWND. Win32 error: {Marshal.GetLastWin32Error()}.");
            }

            SetNativeHandle(handle, generation);
            OcctWpfRenderWindowClass.RegisterCursorHandler(handle, ApplyCurrentCursor);
            ApplyCurrentCursor();

            var engine = new OcctEngine();
            _engine = engine;
            engine.InitializeNativeSurface(
                OcctNativeSurfaceKind.Win32Window,
                NativeHandle,
                redrawAfterInitialize: false);
            using (engine.BeginDisplayBatch())
            {
                SynchronizeDpi();
                engine.ResizeSurface();
                _initialOptions.Apply(engine);
                NotifyEngineRecreated(engine, generation);
            }
            _lastHoverTimestamp = 0;
            _lastWorldPointTimestamp = 0;

            AttachHostWindow();

            // HwndHost receives its final arranged size after BuildWindowCore. Attach to the
            // containing Window again at render priority in case the visual tree was not yet
            // fully connected, then cover the first layout pass with one native refresh.
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
                AttachHostWindow();
                RefreshNativeView();
            }));
            return new HandleRef(this, handle);
        }
        catch (Exception exception)
        {
            SetHostFault(exception);
            DisposeNativeHost(handle, transitionToDisposed: false);
            throw;
        }
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        DisposeNativeHost(hwnd.Handle, transitionToDisposed: true);
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        ScheduleNativeViewRefresh();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == CursorProperty && _nativeHandle != IntPtr.Zero)
            ApplyCurrentCursor();
    }

    private void AttachHostWindow()
    {
        var hostWindow = Window.GetWindow(this);
        if (ReferenceEquals(_hostWindow, hostWindow)) return;

        DetachHostWindow();
        _hostWindow = hostWindow;
        if (_hostWindow is null) return;

        _lastHostWindowState = _hostWindow.WindowState;
        _hostWindow.StateChanged += OnHostWindowStateChanged;
    }

    private void DetachHostWindow()
    {
        if (_hostWindow is not null)
            _hostWindow.StateChanged -= OnHostWindowStateChanged;
        _hostWindow = null;
        _lastHostWindowState = WindowState.Normal;
    }

    private void OnHostWindowStateChanged(object? sender, EventArgs e)
    {
        if (!ReferenceEquals(sender, _hostWindow) || _hostWindow is null) return;

        var previousState = _lastHostWindowState;
        var currentState = _hostWindow.WindowState;
        _lastHostWindowState = currentState;

        if (previousState == WindowState.Minimized && currentState != WindowState.Minimized)
        {
            // V3d_View requires an explicit redraw after deiconification. Run this through the
            // existing coalesced resize/render path so no pointer input is needed to expose a frame.
            ScheduleNativeViewRefresh();
        }
    }

    private bool ApplyCurrentCursor()
    {
        var cursor = Cursor ?? Cursors.Arrow;
        return Mouse.SetCursor(cursor);
    }

    private bool HasUsableRenderSize() =>
        IsVisible &&
        ActualWidth > 0.5 &&
        ActualHeight > 0.5;

    private void CompleteFirstFrameIfNeeded()
    {
        if (_renderReady) return;
        MarkFirstFrameRendered(_engineGeneration);
        SetHostState(OcctViewportHostState.Ready);
    }

    private void ScheduleNativeViewRefresh()
    {
        if (_nativeRefreshScheduled || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero || !IsVisible) return;
        _nativeRefreshScheduled = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
        {
            _nativeRefreshScheduled = false;
            RefreshNativeView();
        }));
    }

    private void SynchronizeDpi()
    {
        if (!SynchronizeRenderDpi || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
        var dpi = GetDpiForWindow(_nativeHandle);
        if (dpi == 0 || dpi == _lastRenderDpi) return;
        _lastRenderDpi = dpi;
        _engine.SetRenderResolution(dpi);
    }

    private void DisposeNativeHost(IntPtr handle, bool transitionToDisposed)
    {
        DetachHostWindow();
        ResetRenderReady();
        CancelInteraction();
        var engine = _engine;
        _engine = null;
        if (engine is not null)
        {
            NotifyEngineDisposing(engine, _engineGeneration);
            try { engine.Dispose(); }
            catch (Exception exception) { ReportLifecycleError(exception); }
        }

        OcctWpfRenderWindowClass.UnregisterCursorHandler(handle);
        SetNativeHandle(IntPtr.Zero, _engineGeneration);
        _nativeRefreshScheduled = false;
        _lastRenderDpi = 0;
        if (handle != IntPtr.Zero) DestroyWindow(handle);
        if (transitionToDisposed) SetHostState(OcctViewportHostState.Disposed);
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
        try { ErrorOccurred?.Invoke(this, new OcctWpfErrorEventArgs(exception)); }
        catch (Exception handlerException) { System.Diagnostics.Debug.WriteLine(handlerException); }
    }
}
