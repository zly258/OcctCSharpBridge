using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using DrawingColor = System.Drawing.Color;

namespace OcctNet;

public sealed class OcctAvaloniaSelectionEventArgs : EventArgs
{
    public OcctAvaloniaSelectionEventArgs(IOcctObject? selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
    {
        SelectedObject = selectedObject;
        SelectedObjects = selectedObjects;
    }

    public IOcctObject? SelectedObject { get; }
    public IReadOnlyList<IOcctObject> SelectedObjects { get; }
}

public sealed class OcctAvaloniaErrorEventArgs : EventArgs
{
    public OcctAvaloniaErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public Exception Exception { get; }
}

public sealed class OcctAvaloniaWorldPointEventArgs : EventArgs
{
    public OcctAvaloniaWorldPointEventArgs(int screenX, int screenY, OcctPoint3d worldPoint)
    {
        ScreenX = screenX;
        ScreenY = screenY;
        WorldPoint = worldPoint;
    }

    public int ScreenX { get; }
    public int ScreenY { get; }
    public OcctPoint3d WorldPoint { get; }
}

/// <summary>
/// Reusable Avalonia host for the OCCT Windows HWND viewer.
/// This adapter is Windows-only; it does not make the native OCCT bridge cross-platform.
/// </summary>
public sealed class OcctAvaloniaViewport : NativeControlHost
{
    private const int GwlpWndProc = -4;
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsClipSiblings = 0x04000000;
    private const int WsClipChildren = 0x02000000;
    private const int SsNotify = 0x00000100;
    private const int HtClient = 1;

    private const uint WmSize = 0x0005;
    private const uint WmSetFocus = 0x0007;
    private const uint WmKillFocus = 0x0008;
    private const uint WmEraseBkgnd = 0x0014;
    private const uint WmCancelMode = 0x001F;
    private const uint WmWindowPosChanged = 0x0047;
    private const uint WmNcHitTest = 0x0084;
    private const uint WmMouseMove = 0x0200;
    private const uint WmLButtonDown = 0x0201;
    private const uint WmLButtonUp = 0x0202;
    private const uint WmRButtonDown = 0x0204;
    private const uint WmRButtonUp = 0x0205;
    private const uint WmMButtonDown = 0x0207;
    private const uint WmMButtonUp = 0x0208;
    private const uint WmMouseWheel = 0x020A;
    private const uint WmCaptureChanged = 0x0215;
    private const uint WmDpiChanged = 0x02E0;

    private const int MkLButton = 0x0001;
    private const int MkRButton = 0x0002;
    private const int MkMButton = 0x0010;
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;

    private readonly WndProcDelegate _windowProcedure;
    private OcctEngine? _engine;
    private IntPtr _nativeHandle;
    private IntPtr _previousWindowProcedure;
    private bool _enableDefaultInteraction = true;
    private bool _rotating;
    private bool _panning;
    private bool _leftSelectionGesture;
    private bool _selectingRectangle;
    private bool _rectangleDragStarted;
    private int _lastMouseX;
    private int _lastMouseY;
    private int _selectionStartX;
    private int _selectionStartY;
    private int _selectionCurrentX;
    private int _selectionCurrentY;
    private SelectionFrame? _selectionFrame;
    private long _lastHoverTimestamp;
    private long _lastWorldPointTimestamp;
    private bool _nativeRefreshScheduled;

    public OcctAvaloniaViewport()
    {
        _windowProcedure = WindowProcedure;
        Focusable = true;
        SizeChanged += OnHostSizeChanged;
    }

    public OcctEngine Engine => _engine ?? throw new InvalidOperationException("The Avalonia OCCT viewport has not been created yet.");
    public IntPtr NativeHandle => _nativeHandle;
    public bool IsEngineInitialized => _engine?.IsInitialized == true;

    public bool EnableDefaultInteraction
    {
        get => _enableDefaultInteraction;
        set
        {
            if (_enableDefaultInteraction == value) return;
            _enableDefaultInteraction = value;
            if (!value) CancelInteraction();
        }
    }

    public bool EnableRectangleSelection { get; set; } = true;
    public int RectangleSelectionThreshold { get; set; } = 3;
    public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; } = OcctRectangleSelectionBehavior.Inclusive;
    public Color RectangleSelectionLineColor { get; set; } = Colors.DodgerBlue;
    public Color RectangleSelectionFillColor { get; set; } = Colors.LightSkyBlue;
    public double RectangleSelectionFillTransparency { get; set; } = 0.82;
    public double RectangleSelectionLineWidth { get; set; } = 1.0;
    public bool SynchronizeRenderDpi { get; set; } = true;

    public event EventHandler<OcctShape?>? SelectionChanged;
    public event EventHandler<OcctAvaloniaSelectionEventArgs>? ObjectSelectionChanged;
    public event EventHandler<OcctAvaloniaWorldPointEventArgs>? WorldPointChanged;
    public event EventHandler<OcctAvaloniaErrorEventArgs>? ErrorOccurred;
    public event EventHandler? EngineInitialized;

    public void RaiseSelectionChanged()
    {
        if (_engine?.IsInitialized != true) return;
        var selected = _engine.FirstSelectedObject;
        SelectionChanged?.Invoke(this, _engine.FirstSelected);
        ObjectSelectionChanged?.Invoke(this, new OcctAvaloniaSelectionEventArgs(selected, _engine.SelectedObjects));
    }

    public void RefreshNativeView()
    {
        if (_engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
        SynchronizeDpi();
        TryInvoke(_engine.Resize);
        TryInvoke(_engine.Redraw);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("OcctNet.Avalonia supports the Windows HWND backend only.");
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
            _engine.Initialize(handle);
            SynchronizeDpi();
            _engine.Resize();
            _engine.Redraw();
            _lastHoverTimestamp = 0;
            _lastWorldPointTimestamp = 0;
            EngineInitialized?.Invoke(this, EventArgs.Empty);
            Dispatcher.UIThread.Post(RefreshNativeView, DispatcherPriority.Background);
            return new PlatformHandle(handle, "HWND");
        }
        catch
        {
            DisposeNativeHost(handle);
            throw;
        }
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (OperatingSystem.IsWindows() && control.Handle == _nativeHandle)
        {
            DisposeNativeHost(control.Handle);
            return;
        }
        base.DestroyNativeControlCore(control);
    }

    private void InstallInputWindowProcedure(IntPtr handle)
    {
        var procedurePointer = Marshal.GetFunctionPointerForDelegate(_windowProcedure);
        Marshal.SetLastPInvokeError(0);
        var previous = SetWindowLongPtrW(handle, GwlpWndProc, procedurePointer);
        var error = Marshal.GetLastPInvokeError();
        if (previous == IntPtr.Zero && error != 0)
            throw new InvalidOperationException($"Unable to subclass the Avalonia OCCT child HWND. Win32 error: {error}.");
        _previousWindowProcedure = previous;
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

    private IntPtr WindowProcedure(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            switch (message)
            {
                case WmNcHitTest:
                    return new IntPtr(HtClient);
                case WmSize:
                case WmWindowPosChanged:
                case WmDpiChanged:
                    ScheduleNativeViewRefresh();
                    break;
                case WmSetFocus:
                    break;
                case WmKillFocus:
                case WmCancelMode:
                    CancelInteraction();
                    break;
                case WmEraseBkgnd:
                    if (_engine?.IsInitialized == true) return new IntPtr(1);
                    break;
                case WmLButtonDown:
                    HandleLeftButtonDown(hwnd, lParam);
                    break;
                case WmLButtonUp:
                    HandleLeftButtonUp(lParam);
                    break;
                case WmRButtonDown:
                    HandleRightButtonDown(hwnd, lParam);
                    break;
                case WmRButtonUp:
                    _rotating = false;
                    ReleaseCapture();
                    break;
                case WmMButtonDown:
                    HandleMiddleButtonDown(hwnd, lParam);
                    break;
                case WmMButtonUp:
                    _panning = false;
                    ReleaseCapture();
                    break;
                case WmMouseMove:
                    HandleMouseMove(wParam, lParam);
                    break;
                case WmMouseWheel:
                    HandleMouseWheel(wParam, lParam);
                    break;
                case WmCaptureChanged:
                    if (lParam != hwnd)
                    {
                        _rotating = false;
                        _panning = false;
                        CancelRectangleSelection(releaseCapture: false);
                    }
                    break;
            }
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
        return CallPreviousWindowProcedure(hwnd, message, wParam, lParam);
    }

    private void HandleLeftButtonDown(IntPtr hwnd, IntPtr lParam)
    {
        SetFocus(hwnd);
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction || IsKeyDown(VkShift)) return;

        CancelRectangleSelection();
        (_selectionStartX, _selectionStartY) = GetPoint(lParam);
        _selectionCurrentX = _selectionStartX;
        _selectionCurrentY = _selectionStartY;
        _lastMouseX = _selectionStartX;
        _lastMouseY = _selectionStartY;
        _leftSelectionGesture = true;
        _rectangleDragStarted = false;
        _selectingRectangle = EnableRectangleSelection;
        if (_selectingRectangle) SetCapture(hwnd);
    }

    private void HandleLeftButtonUp(IntPtr lParam)
    {
        if (!EnableDefaultInteraction || !_leftSelectionGesture) return;

        var eventPoint = GetPoint(lParam);
        var end = OcctViewportInteractionPolicy.ResolveSelectionEnd(
            _selectionStartX,
            _selectionStartY,
            eventPoint.X,
            eventPoint.Y,
            _selectionCurrentX,
            _selectionCurrentY,
            _rectangleDragStarted);
        var useRectangle = OcctViewportInteractionPolicy.ShouldUseRectangle(
            EnableRectangleSelection,
            _rectangleDragStarted,
            RectangleSelectionThreshold,
            _selectionStartX,
            _selectionStartY,
            end.X,
            end.Y);
        var append = IsKeyDown(VkControl);
        var allowOverlap = OcctViewportInteractionPolicy.AllowsOverlap(RectangleSelectionBehavior, _selectionStartX, end.X);

        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        ReleaseCapture();

        if (_engine?.IsInitialized != true) return;
        TryInvoke(() =>
        {
            if (useRectangle)
                _engine.SelectRectangle(_selectionStartX, _selectionStartY, end.X, end.Y, append, allowOverlap);
            else
                _engine.Select(end.X, end.Y, append);
            RaiseSelectionChanged();
        });
    }

    private void HandleRightButtonDown(IntPtr hwnd, IntPtr lParam)
    {
        SetFocus(hwnd);
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction) return;
        CancelRectangleSelection();
        (_lastMouseX, _lastMouseY) = GetPoint(lParam);
        _rotating = true;
        SetCapture(hwnd);
        TryInvoke(() => _engine.StartRotation(_lastMouseX, _lastMouseY));
    }

    private void HandleMiddleButtonDown(IntPtr hwnd, IntPtr lParam)
    {
        SetFocus(hwnd);
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction) return;
        CancelRectangleSelection();
        (_lastMouseX, _lastMouseY) = GetPoint(lParam);
        _panning = true;
        SetCapture(hwnd);
    }

    private void HandleMouseMove(IntPtr wParam, IntPtr lParam)
    {
        var (x, y) = GetPoint(lParam);
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction)
        {
            _lastMouseX = x;
            _lastMouseY = y;
            return;
        }

        var buttons = unchecked((int)wParam.ToInt64());
        if (_rotating && (buttons & MkRButton) != 0)
        {
            TryInvoke(() => _engine.Rotation(x, y));
        }
        else if (_panning && (buttons & MkMButton) != 0)
        {
            TryInvoke(() => _engine.Pan(x - _lastMouseX, - (y - _lastMouseY)));
        }
        else if (_leftSelectionGesture && _selectingRectangle && (buttons & MkLButton) != 0)
        {
            UpdateSelectionFrame(x, y);
        }
        else
        {
            var now = System.Diagnostics.Stopwatch.GetTimestamp();
            if (OcctViewportInteractionPolicy.HasElapsed(_lastHoverTimestamp, now, OcctViewportInteractionPolicy.HoverIntervalTicks))
            {
                _lastHoverTimestamp = now;
                TryInvoke(() => _engine.MoveTo(x, y));
            }

            if (WorldPointChanged is not null
                && OcctViewportInteractionPolicy.HasElapsed(_lastWorldPointTimestamp, now, OcctViewportInteractionPolicy.WorldPointIntervalTicks))
            {
                _lastWorldPointTimestamp = now;
                TryInvoke(() => WorldPointChanged.Invoke(this, new OcctAvaloniaWorldPointEventArgs(x, y, _engine.ScreenToWorld(x, y))));
            }
        }

        _lastMouseX = x;
        _lastMouseY = y;
    }

    private void HandleMouseWheel(IntPtr wParam, IntPtr lParam)
    {
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction) return;
        var delta = GetHighWordSigned(wParam);
        if (delta == 0) return;

        var point = new NativePoint(GetLowWordSigned(lParam), GetHighWordSigned(lParam));
        if (_nativeHandle != IntPtr.Zero && ScreenToClient(_nativeHandle, ref point))
            TryInvoke(() => _engine.ZoomAtPoint(point.X, point.Y, delta));
        else
            TryInvoke(() => _engine.Zoom(OcctViewportInteractionPolicy.ZoomFactor(delta)));
    }

    private void UpdateSelectionFrame(int currentX, int currentY)
    {
        if (_engine?.IsInitialized != true) return;

        _selectionCurrentX = currentX;
        _selectionCurrentY = currentY;
        var threshold = Math.Max(0, RectangleSelectionThreshold);
        var dx = Math.Abs(currentX - _selectionStartX);
        var dy = Math.Abs(currentY - _selectionStartY);
        if (dx < threshold && dy < threshold)
        {
            HideSelectionFrame();
            return;
        }

        _rectangleDragStarted = true;
        var frame = new SelectionFrame(
            Math.Min(_selectionStartX, currentX),
            Math.Min(_selectionStartY, currentY),
            Math.Max(_selectionStartX, currentX),
            Math.Max(_selectionStartY, currentY));
        if (_selectionFrame == frame) return;

        TryInvoke(() => _engine.ShowSelectionRectangle(
            frame.Left,
            frame.Top,
            frame.Right,
            frame.Bottom,
            ToDrawingColor(RectangleSelectionLineColor),
            ToDrawingColor(RectangleSelectionFillColor),
            Math.Clamp(RectangleSelectionFillTransparency, 0.0, 1.0),
            double.IsFinite(RectangleSelectionLineWidth) && RectangleSelectionLineWidth > 0.0
                ? RectangleSelectionLineWidth
                : 1.0));
        _selectionFrame = frame;
    }

    private void HideSelectionFrame()
    {
        if (_selectionFrame is null) return;
        if (_engine?.IsInitialized == true) TryInvoke(_engine.HideSelectionRectangle);
        _selectionFrame = null;
    }

    private void CancelInteraction()
    {
        _rotating = false;
        _panning = false;
        CancelRectangleSelection();
    }

    private void CancelRectangleSelection(bool releaseCapture = true)
    {
        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        if (releaseCapture) ReleaseCapture();
    }

    private void SynchronizeDpi()
    {
        if (!SynchronizeRenderDpi || _engine?.IsInitialized != true || _nativeHandle == IntPtr.Zero) return;
        var dpi = GetDpiForWindow(_nativeHandle);
        if (dpi > 0) TryInvoke(() => _engine.SetRenderResolution(dpi));
    }

    private void DisposeNativeHost(IntPtr handle)
    {
        CancelInteraction();

        var engine = _engine;
        _engine = null;
        if (engine is not null)
        {
            try { engine.Dispose(); }
            catch (Exception exception) { ReportError(exception); }
        }

        if (handle != IntPtr.Zero && _previousWindowProcedure != IntPtr.Zero)
        {
            SetWindowLongPtrW(handle, GwlpWndProc, _previousWindowProcedure);
            _previousWindowProcedure = IntPtr.Zero;
        }

        if (handle != IntPtr.Zero) DestroyWindow(handle);
        _nativeHandle = IntPtr.Zero;
        _selectionFrame = null;
        _nativeRefreshScheduled = false;
    }

    private IntPtr CallPreviousWindowProcedure(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam) =>
        _previousWindowProcedure != IntPtr.Zero
            ? CallWindowProcW(_previousWindowProcedure, hwnd, message, wParam, lParam)
            : DefWindowProcW(hwnd, message, wParam, lParam);

    private void TryInvoke(Action action)
    {
        try { action(); }
        catch (Exception exception) { ReportError(exception); }
    }

    private void ReportError(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine(exception);
        try { ErrorOccurred?.Invoke(this, new OcctAvaloniaErrorEventArgs(exception)); }
        catch (Exception handlerException) { System.Diagnostics.Debug.WriteLine(handlerException); }
    }

    private static bool IsKeyDown(int virtualKey) => (GetKeyState(virtualKey) & 0x8000) != 0;
    private static (int X, int Y) GetPoint(IntPtr lParam) => (GetLowWordSigned(lParam), GetHighWordSigned(lParam));
    private static int GetLowWordSigned(IntPtr value) => unchecked((short)(value.ToInt64() & 0xFFFF));
    private static int GetHighWordSigned(IntPtr value) => unchecked((short)((value.ToInt64() >> 16) & 0xFFFF));
    private static DrawingColor ToDrawingColor(Color value) => DrawingColor.FromArgb(value.A, value.R, value.G, value.B);

    private readonly record struct SelectionFrame(int Left, int Top, int Right, int Bottom);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public NativePoint(int x, int y) { X = x; Y = y; }
        public int X;
        public int Y;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(
        int extendedStyle,
        string className,
        string windowName,
        int style,
        int x,
        int y,
        int width,
        int height,
        IntPtr parent,
        IntPtr menu,
        IntPtr instance,
        IntPtr parameter);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtrW(IntPtr hwnd, int index, IntPtr newValue);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CallWindowProcW(IntPtr previous, IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr DefWindowProcW(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCapture(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int virtualKey);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hwnd, ref NativePoint point);
}
