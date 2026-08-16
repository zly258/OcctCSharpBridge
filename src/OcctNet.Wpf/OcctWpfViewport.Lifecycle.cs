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

        OcctWpfRenderWindowClass.EnsureRegistered();
        var handle = CreateWindowExW(
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
            throw new InvalidOperationException($"Unable to create the WPF OCCT child HWND. Win32 error: {Marshal.GetLastWin32Error()}.");

        _nativeHandle = handle;
        try
        {
            _engine = new OcctEngine();
            _engine.Initialize(handle);
            SynchronizeDpi();
            _engine.ResizeSurface();
            _engine.Redraw();
            _lastHoverTimestamp = 0;
            _lastWorldPointTimestamp = 0;
            EngineInitialized?.Invoke(this, EventArgs.Empty);

            // HwndHost receives its final arranged size after BuildWindowCore.
            // Keep one render-priority refresh to cover that first layout pass.
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(RefreshNativeView));
            return new HandleRef(this, handle);
        }
        catch
        {
            DisposeNativeHost(handle);
            throw;
        }
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        DisposeNativeHost(hwnd.Handle);
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

    private void DisposeNativeHost(IntPtr handle)
    {
        CancelInteraction();
        _engine?.Dispose();
        _engine = null;
        _nativeHandle = IntPtr.Zero;
        _nativeRefreshScheduled = false;
        _nativeRenderScheduled = false;
        _lastRenderDpi = 0;
        if (handle != IntPtr.Zero) DestroyWindow(handle);
    }
}
