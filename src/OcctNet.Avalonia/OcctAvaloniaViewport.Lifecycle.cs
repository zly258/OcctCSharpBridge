using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
    public void RefreshNativeView()
    {
        if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero || !HasUsableRenderSize()) return;

        try
        {
            SynchronizeDpi();
            if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero || !HasUsableRenderSize()) return;
            _engine.ResizeSurface();
            _engine.Redraw();
            CompleteFirstFrameIfNeeded();
        }
        catch (Exception exception)
        {
            ReportLifecycleError(exception);
        }
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        ResetRenderReady();
        SetHostState(OcctViewportHostState.Initializing);
        var generation = ++_engineGeneration;
        try
        {
            return OperatingSystem.IsWindows()
                ? CreateWindowsHost(parent, generation)
                : OperatingSystem.IsLinux()
                    ? CreateLinuxHost(parent, generation)
                    : throw new PlatformNotSupportedException(
                        "OcctNet.Avalonia currently supports Windows x64 and Linux x64.");
        }
        catch (Exception exception)
        {
            SetHostFault(exception);
            DisposeNativeHost(_nativeHandle, transitionToDisposed: false);
            throw;
        }
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (control.Handle == _nativeHandle)
        {
            DisposeNativeHost(control.Handle, transitionToDisposed: true);
            return;
        }
        base.DestroyNativeControlCore(control);
    }

    private IPlatformHandle CreateWindowsHost(IPlatformHandle parent, long generation)
    {
        if (!string.Equals(parent.HandleDescriptor, "HWND", StringComparison.OrdinalIgnoreCase))
            throw new PlatformNotSupportedException($"Expected an HWND parent but received '{parent.HandleDescriptor}'.");

        var handle = CreateWindowExW(
            0,
            "STATIC",
            string.Empty,
            WsChild | WsVisible | WsClipSiblings | WsClipChildren | SsOwnerDraw | SsNotify,
            0,
            0,
            100,
            100,
            parent.Handle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                $"Unable to create the Avalonia OCCT child HWND. Win32 error: {Marshal.GetLastWin32Error()}.");
        }

        SetNativeHandle(handle, generation);
        InstallInputWindowProcedure(handle);
        _engine = new OcctEngine();
        _engine.InitializeNativeSurface(
            OcctNativeSurfaceKind.Win32Window,
            NativeHandle,
            redrawAfterInitialize: false);
        FinishEngineInitialization(generation);
        return new PlatformHandle(NativeHandle, "HWND");
    }

    private IPlatformHandle CreateLinuxHost(IPlatformHandle parent, long generation)
    {
        if (!string.Equals(parent.HandleDescriptor, "XID", StringComparison.OrdinalIgnoreCase))
        {
            throw new PlatformNotSupportedException(
                $"Avalonia Linux viewer currently requires the X11/XWayland backend (XID); received '{parent.HandleDescriptor}'. " +
                "Native Wayland hosting is not implemented yet.");
        }

        _x11Display = XOpenDisplay(IntPtr.Zero);
        if (_x11Display == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "Unable to open the X11 display. Ensure DISPLAY is configured and X11/XWayland is available.");
        }

        var screen = XDefaultScreen(_x11Display);
        var black = XBlackPixel(_x11Display, screen);
        var parentWindow = unchecked((nuint)parent.Handle.ToInt64());
        var window = XCreateSimpleWindow(_x11Display, parentWindow, 0, 0, 100, 100, 0, black, black);
        if (window == 0)
        {
            XCloseDisplay(_x11Display);
            _x11Display = IntPtr.Zero;
            throw new InvalidOperationException("Unable to create the Avalonia OCCT X11 child window.");
        }

        XMapWindow(_x11Display, window);
        XFlush(_x11Display);
        SetNativeHandle(new IntPtr(unchecked((long)window)), generation);
        _engine = new OcctEngine();
        _engine.InitializeNativeSurface(
            OcctNativeSurfaceKind.X11Window,
            NativeHandle,
            _x11Display,
            redrawAfterInitialize: false);
        FinishEngineInitialization(generation);
        InstallX11Input(window);
        StartX11InputPump();
        return new PlatformHandle(NativeHandle, "XID");
    }

    private void FinishEngineInitialization(long generation)
    {
        var engine = _engine ?? throw new InvalidOperationException("The OCCT engine has not been created.");
        using (engine.BeginDisplayBatch())
        {
            SynchronizeDpi();
            engine.ResizeSurface();
            _initialOptions.Apply(engine);
            NotifyEngineRecreated(engine, generation);
        }
        _lastHoverTimestamp = 0;
        _lastWorldPointTimestamp = 0;
        Dispatcher.UIThread.Post(RefreshNativeView, DispatcherPriority.Loaded);
    }

    private bool HasUsableRenderSize()
    {
        if (!IsVisible) return false;

        if (OperatingSystem.IsWindows() && _nativeHandle != IntPtr.Zero)
        {
            return GetClientRect(_nativeHandle, out var rect)
                && rect.Right > rect.Left
                && rect.Bottom > rect.Top;
        }

        return Bounds.Width > 0.5 && Bounds.Height > 0.5;
    }

    private void CompleteFirstFrameIfNeeded()
    {
        if (_renderReady) return;
        MarkFirstFrameRendered(_engineGeneration);
        SetHostState(OcctViewportHostState.Ready);
    }

    private void OnHostSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_engine?.IsInitialized == true && _nativeHandle != IntPtr.Zero)
            ScheduleNativeViewRefresh();
    }

    private void ScheduleNativeViewRefresh()
    {
        if (_nativeRefreshScheduled || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
        _nativeRefreshScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            _nativeRefreshScheduled = false;
            RefreshNativeView();
        }, DispatcherPriority.Loaded);
    }

    private void SynchronizeDpi()
    {
        if (!SynchronizeRenderDpi || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
        if (OperatingSystem.IsWindows())
        {
            var dpi = GetDpiForWindow(_nativeHandle);
            if (dpi > 0) TryInvoke(() => _engine.SetRenderResolution(dpi));
            return;
        }

        if (OperatingSystem.IsLinux())
        {
            var scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
            var dpi = Math.Max(1.0, 96.0 * scale);
            TryInvoke(() => _engine.SetRenderResolution(dpi));
        }
    }

    private void DisposeNativeHost(IntPtr handle, bool transitionToDisposed)
    {
        ResetRenderReady();
        StopX11InputPump();
        CancelInteraction();
        _x11PressedKeys.Clear();

        var engine = _engine;
        _engine = null;
        if (engine is not null)
        {
            NotifyEngineDisposing(engine, _engineGeneration);
            try { engine.Dispose(); }
            catch (Exception exception) { ReportLifecycleError(exception); }
        }

        SetNativeHandle(IntPtr.Zero, _engineGeneration);
        if (OperatingSystem.IsWindows())
        {
            if (handle != IntPtr.Zero && _previousWindowProcedure != IntPtr.Zero)
            {
                SetWindowLongPtrW(handle, GwlpWndProc, _previousWindowProcedure);
                _previousWindowProcedure = IntPtr.Zero;
            }
            if (handle != IntPtr.Zero) DestroyWindow(handle);
        }
        else if (OperatingSystem.IsLinux())
        {
            if (_x11Display != IntPtr.Zero && handle != IntPtr.Zero)
            {
                XDestroyWindow(_x11Display, unchecked((nuint)handle.ToInt64()));
                XFlush(_x11Display);
            }
            if (_x11Display != IntPtr.Zero)
            {
                XCloseDisplay(_x11Display);
                _x11Display = IntPtr.Zero;
            }
        }

        _selectionFrame = null;
        _nativeRefreshScheduled = false;
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
        try { ErrorOccurred?.Invoke(this, new OcctAvaloniaErrorEventArgs(exception)); }
        catch (Exception handlerException) { System.Diagnostics.Debug.WriteLine(handlerException); }
    }
}
