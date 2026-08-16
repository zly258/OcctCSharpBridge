using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace OcctNet;

public sealed partial class OcctWpfViewport
{
    /// <summary>
    /// Synchronizes the OCCT render target with the current child HWND size and
    /// coalesces presentation into one WPF render-priority callback.
    /// </summary>
    public void RefreshNativeView()
    {
        if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
        TryInvoke(_engine.ResizeSurface);
        ScheduleRender();
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("OcctNet.Wpf supports Windows HWND hosting only.");

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
                WsChild | WsVisible | WsClipSiblings | WsClipChildren,
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

            _nativeHandle = handle;
            var engine = new OcctEngine();
            _engine = engine;
            engine.Initialize(handle);
            SynchronizeDpi();
            engine.ResizeSurface();
            engine.Redraw();
            _lastHoverTimestamp = 0;
            _lastWorldPointTimestamp = 0;
            NotifyEngineRecreated(engine, generation);
            SetHostState(OcctViewportHostState.Ready);

            // HwndHost receives its final arranged size after BuildWindowCore.
            // Keep one render-priority refresh to cover that first layout pass.
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(RefreshNativeView));
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

    private void ScheduleRender()
    {
        if (_nativeRenderScheduled || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero || !IsVisible) return;
        _nativeRenderScheduled = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
        {
            _nativeRenderScheduled = false;
            if (_engine?.IsInitialized == true && _nativeHandle != IntPtr.Zero && IsVisible)
                TryInvoke(_engine.Redraw);
        }));
    }

    private void ScheduleNativeViewRefresh()
    {
        if (_nativeRefreshScheduled || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero || !IsVisible) return;
        _nativeRefreshScheduled = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
        {
            _nativeRefreshScheduled = false;
            SynchronizeDpi();
            if (_engine?.IsInitialized == true && _nativeHandle != IntPtr.Zero)
                TryInvoke(_engine.ResizeSurface);
            ScheduleRender();
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
        CancelInteraction();
        var engine = _engine;
        _engine = null;
        if (engine is not null)
        {
            NotifyEngineDisposing(engine, _engineGeneration);
            try { engine.Dispose(); }
            catch (Exception exception) { ReportLifecycleError(exception); }
        }

        _nativeHandle = IntPtr.Zero;
        _nativeRefreshScheduled = false;
        _nativeRenderScheduled = false;
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
