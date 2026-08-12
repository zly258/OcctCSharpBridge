using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace OcctNet;

/// <summary>
/// Linux X11 NativeControlHost for an OCCT viewer surface.
/// </summary>
/// <remarks>
/// This first Linux host targets Avalonia's X11/XWayland backend. Native Wayland is intentionally
/// out of scope until the X11 path has been validated on a real Linux desktop.
/// </remarks>
public sealed class OcctAvaloniaX11Viewport : NativeControlHost
{
    private OcctEngine? _engine;
    private IntPtr _display;
    private IntPtr _nativeHandle;
    private bool _refreshScheduled;

    public OcctAvaloniaX11Viewport()
    {
        Focusable = true;
        SizeChanged += (_, _) => ScheduleNativeRefresh();
    }

    /// <summary>Gets the initialized OCCT engine.</summary>
    public OcctEngine Engine => _engine ?? throw new InvalidOperationException(
        "The Avalonia X11 OCCT viewport has not been created yet.");

    /// <summary>Gets the native X11 Window ID.</summary>
    public IntPtr NativeHandle => _nativeHandle;

    /// <summary>Gets whether the native OCCT viewer has been initialized.</summary>
    public bool IsEngineInitialized => _engine?.IsInitialized == true;

    /// <summary>Raised after the X11 surface and OCCT viewer have been initialized.</summary>
    public event EventHandler? EngineInitialized;

    /// <summary>Raised when native host initialization or refresh fails.</summary>
    public event EventHandler<OcctAvaloniaErrorEventArgs>? ErrorOccurred;

    /// <summary>Requests OCCT to synchronize its OpenGL viewport with the X11 child window.</summary>
    public void RefreshNativeView()
    {
        if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero)
            return;

        TryInvoke(_engine.Resize);
        TryInvoke(_engine.Redraw);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("OcctAvaloniaX11Viewport supports Linux only.");

        if (!string.Equals(parent.HandleDescriptor, "XID", StringComparison.Ordinal))
        {
            throw new PlatformNotSupportedException(
                $"The Linux OCCT viewport requires Avalonia's X11/XWayland backend; received '{parent.HandleDescriptor}'.");
        }

        _display = XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero)
            throw new InvalidOperationException("Unable to open the X11 display. Verify the DISPLAY environment variable.");

        try
        {
            _nativeHandle = XCreateSimpleWindow(
                _display,
                parent.Handle,
                0,
                0,
                1,
                1,
                0,
                UIntPtr.Zero,
                UIntPtr.Zero);

            if (_nativeHandle == IntPtr.Zero)
                throw new InvalidOperationException("Unable to create the OCCT X11 child window.");

            XMapWindow(_display, _nativeHandle);
            XFlush(_display);

            _engine = new OcctEngine();
            _engine.Initialize(_nativeHandle);
            _engine.Resize();
            _engine.Redraw();

            EngineInitialized?.Invoke(this, EventArgs.Empty);
            Dispatcher.UIThread.Post(RefreshNativeView, DispatcherPriority.Background);
            return new PlatformHandle(_nativeHandle, "XID");
        }
        catch
        {
            DisposeNativeHost();
            throw;
        }
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (OperatingSystem.IsLinux() && control.Handle == _nativeHandle)
        {
            DisposeNativeHost();
            return;
        }

        base.DestroyNativeControlCore(control);
    }

    private void ScheduleNativeRefresh()
    {
        if (_refreshScheduled || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero)
            return;

        _refreshScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            _refreshScheduled = false;
            RefreshNativeView();
        }, DispatcherPriority.Background);
    }

    private void DisposeNativeHost()
    {
        _refreshScheduled = false;

        var engine = _engine;
        _engine = null;
        if (engine is not null)
        {
            try
            {
                engine.Dispose();
            }
            catch (Exception exception)
            {
                ReportError(exception);
            }
        }

        if (_display != IntPtr.Zero && _nativeHandle != IntPtr.Zero)
        {
            XDestroyWindow(_display, _nativeHandle);
            XFlush(_display);
        }

        _nativeHandle = IntPtr.Zero;

        if (_display != IntPtr.Zero)
        {
            XCloseDisplay(_display);
            _display = IntPtr.Zero;
        }
    }

    private void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
    }

    private void ReportError(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine(exception);
        try
        {
            ErrorOccurred?.Invoke(this, new OcctAvaloniaErrorEventArgs(exception));
        }
        catch (Exception handlerException)
        {
            System.Diagnostics.Debug.WriteLine(handlerException);
        }
    }

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XCreateSimpleWindow(
        IntPtr display,
        IntPtr parent,
        int x,
        int y,
        uint width,
        uint height,
        uint borderWidth,
        UIntPtr border,
        UIntPtr background);

    [DllImport("libX11.so.6")]
    private static extern int XMapWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XDestroyWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XFlush(IntPtr display);
}
