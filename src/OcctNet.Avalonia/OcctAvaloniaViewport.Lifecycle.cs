using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
    public void RefreshNativeView()
    {
        if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
        SynchronizeDpi();
        TryInvoke(_engine.ResizeSurface);
        TryInvoke(_engine.Redraw);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (OperatingSystem.IsWindows()) return CreateWindowsHost(parent);
        if (OperatingSystem.IsLinux()) return CreateLinuxHost(parent);
        throw new PlatformNotSupportedException("OcctNet.Avalonia currently supports Windows x64 and Linux x64.");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (control.Handle == _nativeHandle)
        {
            DisposeNativeHost(control.Handle);
            return;
        }
        base.DestroyNativeControlCore(control);
    }

    private IPlatformHandle CreateWindowsHost(IPlatformHandle parent)
    {
        if (!string.Equals(parent.HandleDescriptor, "HWND", StringComparison.OrdinalIgnoreCase))
            throw new PlatformNotSupportedException($"Expected an HWND parent but received '{parent.HandleDescriptor}'.");

        var handle = CreateWindowExW(
            0,
            "STATIC",
            "OCCT_Render_Target",
            WsChild | WsVisible | WsClipSiblings | WsClipChildren | SsNotify,
            0,
            0,
            100,
            100,
            parent.Handle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException($"Unable to create the Avalonia OCCT child HWND. Win32 error: {Marshal.GetLastWin32Error()}.");

        _nativeHandle = handle;
        try
        {
            InstallInputWindowProcedure(handle);
            _engine = new OcctEngine();
            _engine.InitializeNativeSurface(OcctNativeSurfaceKind.Win32Window, handle);
            FinishEngineInitialization();
            return new PlatformHandle(handle, "HWND");
        }
        catch
        {
            DisposeNativeHost(handle);
            throw;
        }
    }

    private IPlatformHandle CreateLinuxHost(IPlatformHandle parent)
    {
        if (!string.Equals(parent.HandleDescriptor, "XID", StringComparison.OrdinalIgnoreCase))
        {
            throw new PlatformNotSupportedException(
                $"Avalonia Linux viewer currently requires the X11/XWayland backend (XID); received '{parent.HandleDescriptor}'. " +
                "Native Wayland hosting is not implemented yet.");
        }

        _x11Display = XOpenDisplay(IntPtr.Zero);
        if (_x11Display == IntPtr.Zero)
            throw new InvalidOperationException("Unable to open the X11 display. Ensure DISPLAY is configured and X11/XWayland is available.");

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

        _nativeHandle = new IntPtr(unchecked((long)window));
        try
        {
            XMapWindow(_x11Display, window);
            XFlush(_x11Display);
            _engine = new OcctEngine();
            _engine.InitializeNativeSurface(OcctNativeSurfaceKind.X11Window, _nativeHandle, _x11Display);
            FinishEngineInitialization();
            InstallX11Input(window);
            StartX11InputPump();
            return new PlatformHandle(_nativeHandle, "XID");
        }
        catch
        {
            DisposeNativeHost(_nativeHandle);
            throw;
        }
    }

    private void FinishEngineInitialization()
    {
        SynchronizeDpi();
        _engine!.ResizeSurface();
        _engine.Redraw();
        _lastHoverTimestamp = 0;
        _lastWorldPointTimestamp = 0;
        EngineInitialized?.Invoke(this, EventArgs.Empty);
        Dispatcher.UIThread.Post(RefreshNativeView, DispatcherPriority.Background);
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
        }, DispatcherPriority.Background);
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

    private void DisposeNativeHost(IntPtr handle)
    {
        StopX11InputPump();
        CancelInteraction();
        _x11PressedKeys.Clear();

        var engine = _engine;
        _engine = null;
        if (engine is not null)
        {
            try { engine.Dispose(); }
            catch (Exception exception) { ReportError(exception); }
        }

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

        _nativeHandle = IntPtr.Zero;
        _selectionFrame = null;
        _nativeRefreshScheduled = false;
    }
}
