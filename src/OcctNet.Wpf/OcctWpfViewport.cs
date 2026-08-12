using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;

namespace OcctNet;

/// <summary>Selection state reported by the native WPF viewport host.</summary>
public sealed class OcctWpfSelectionEventArgs : EventArgs
{
    public OcctWpfSelectionEventArgs(IOcctObject? selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
    {
        SelectedObject = selectedObject;
        SelectedObjects = selectedObjects ?? throw new ArgumentNullException(nameof(selectedObjects));
    }

    public IOcctObject? SelectedObject { get; }
    public IReadOnlyList<IOcctObject> SelectedObjects { get; }
}

/// <summary>World-space point corresponding to a WPF viewport screen position.</summary>
public sealed class OcctWpfWorldPointEventArgs : EventArgs
{
    public OcctWpfWorldPointEventArgs(int screenX, int screenY, OcctPoint3d worldPoint)
    {
        ScreenX = screenX;
        ScreenY = screenY;
        WorldPoint = worldPoint;
    }

    public int ScreenX { get; }
    public int ScreenY { get; }
    public OcctPoint3d WorldPoint { get; }
}

/// <summary>Error reported by the WPF viewport adapter without terminating the UI event loop.</summary>
public sealed class OcctWpfErrorEventArgs : EventArgs
{
    public OcctWpfErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public Exception Exception { get; }
}

/// <summary>
/// Native WPF host for the OCCT HWND viewport. The WPF adapter owns its child HWND directly
/// through <see cref="HwndHost"/> and has no dependency on Windows Forms.
/// </summary>
public sealed class OcctWpfViewport : HwndHost
{
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsClipSiblings = 0x04000000;
    private const int WsClipChildren = 0x02000000;
    private const int HtClient = 1;

    private const int WmSize = 0x0005;
    private const int WmSetFocus = 0x0007;
    private const int WmKillFocus = 0x0008;
    private const int WmPaint = 0x000F;
    private const int WmEraseBkgnd = 0x0014;
    private const int WmCancelMode = 0x001F;
    private const int WmNcHitTest = 0x0084;
    private const int WmMouseMove = 0x0200;
    private const int WmLButtonDown = 0x0201;
    private const int WmLButtonUp = 0x0202;
    private const int WmRButtonDown = 0x0204;
    private const int WmRButtonUp = 0x0205;
    private const int WmMButtonDown = 0x0207;
    private const int WmMButtonUp = 0x0208;
    private const int WmMouseWheel = 0x020A;
    private const int WmCaptureChanged = 0x0215;
    private const int WmDpiChanged = 0x02E0;

    private const int MkLButton = 0x0001;
    private const int MkRButton = 0x0002;
    private const int MkMButton = 0x0010;
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;

    private OcctEngine? _engine;
    private IntPtr _nativeHandle;
    private bool _enableDefaultInteraction = true;
    private double _zoomSensitivity = 1.0;
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
    private bool _nativeRenderScheduled;
    private uint _lastRenderDpi;

    public static readonly DependencyProperty EnableDefaultInteractionProperty =
        DependencyProperty.Register(
            nameof(EnableDefaultInteraction),
            typeof(bool),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(true, OnInteractionPropertyChanged));

    public static readonly DependencyProperty ZoomSensitivityProperty =
        DependencyProperty.Register(
            nameof(ZoomSensitivity),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(1.0, OnInteractionPropertyChanged, CoerceZoomSensitivity));

    public static readonly DependencyProperty EnableRectangleSelectionProperty =
        DependencyProperty.Register(
            nameof(EnableRectangleSelection),
            typeof(bool),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty RectangleSelectionThresholdProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionThreshold),
            typeof(int),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(3, null, CoercePositiveInteger));

    public static readonly DependencyProperty RectangleSelectionBehaviorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionBehavior),
            typeof(OcctRectangleSelectionBehavior),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(OcctRectangleSelectionBehavior.Inclusive));

    public static readonly DependencyProperty RectangleSelectionLineColorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionLineColor),
            typeof(MediaColor),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(MediaColors.DodgerBlue));

    public static readonly DependencyProperty RectangleSelectionFillColorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionFillColor),
            typeof(MediaColor),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(MediaColors.LightSkyBlue));

    public static readonly DependencyProperty RectangleSelectionFillTransparencyProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionFillTransparency),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(0.82, null, CoerceUnitInterval));

    public static readonly DependencyProperty RectangleSelectionLineWidthProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionLineWidth),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(1.0, null, CoercePositiveDouble));

    public static readonly DependencyProperty SynchronizeRenderDpiProperty =
        DependencyProperty.Register(
            nameof(SynchronizeRenderDpi),
            typeof(bool),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(true, OnDpiSynchronizationChanged));

    public OcctWpfViewport()
    {
        Focusable = true;
        IsVisibleChanged += (_, _) => ScheduleNativeViewRefresh();
    }

    public OcctEngine Engine => _engine ?? throw new InvalidOperationException("The WPF OCCT viewport has not been created yet.");
    public IntPtr NativeHandle => _nativeHandle;
    public bool IsEngineInitialized => _engine?.IsInitialized == true;

    public bool EnableDefaultInteraction
    {
        get => (bool)GetValue(EnableDefaultInteractionProperty);
        set => SetValue(EnableDefaultInteractionProperty, value);
    }

    public double ZoomSensitivity
    {
        get => (double)GetValue(ZoomSensitivityProperty);
        set => SetValue(ZoomSensitivityProperty, value);
    }

    public bool EnableRectangleSelection
    {
        get => (bool)GetValue(EnableRectangleSelectionProperty);
        set => SetValue(EnableRectangleSelectionProperty, value);
    }

    public int RectangleSelectionThreshold
    {
        get => (int)GetValue(RectangleSelectionThresholdProperty);
        set => SetValue(RectangleSelectionThresholdProperty, value);
    }

    public OcctRectangleSelectionBehavior RectangleSelectionBehavior
    {
        get => (OcctRectangleSelectionBehavior)GetValue(RectangleSelectionBehaviorProperty);
        set => SetValue(RectangleSelectionBehaviorProperty, value);
    }

    public MediaColor RectangleSelectionLineColor
    {
        get => (MediaColor)GetValue(RectangleSelectionLineColorProperty);
        set => SetValue(RectangleSelectionLineColorProperty, value);
    }

    public MediaColor RectangleSelectionFillColor
    {
        get => (MediaColor)GetValue(RectangleSelectionFillColorProperty);
        set => SetValue(RectangleSelectionFillColorProperty, value);
    }

    public double RectangleSelectionFillTransparency
    {
        get => (double)GetValue(RectangleSelectionFillTransparencyProperty);
        set => SetValue(RectangleSelectionFillTransparencyProperty, value);
    }

    public double RectangleSelectionLineWidth
    {
        get => (double)GetValue(RectangleSelectionLineWidthProperty);
        set => SetValue(RectangleSelectionLineWidthProperty, value);
    }

    public bool SynchronizeRenderDpi
    {
        get => (bool)GetValue(SynchronizeRenderDpiProperty);
        set => SetValue(SynchronizeRenderDpiProperty, value);
    }

    public event EventHandler<OcctShape?>? SelectionChanged;
    public event EventHandler<OcctWpfSelectionEventArgs>? ObjectSelectionChanged;
    public event EventHandler<OcctWpfWorldPointEventArgs>? WorldPointChanged;
    public event EventHandler<OcctWpfErrorEventArgs>? ErrorOccurred;
    public event EventHandler? EngineInitialized;

    public void FocusViewport()
    {
        Focus();
        if (_nativeHandle != IntPtr.Zero) SetFocus(_nativeHandle);
    }

    public void RaiseSelectionChanged()
    {
        if (_engine?.IsInitialized != true) return;
        var selected = _engine.FirstSelectedObject;
        SelectionChanged?.Invoke(this, _engine.FirstSelected);
        ObjectSelectionChanged?.Invoke(this, new OcctWpfSelectionEventArgs(selected, _engine.SelectedObjects));
    }

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

    protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        try
        {
            switch (msg)
            {
                case WmNcHitTest:
                    handled = true;
                    return new IntPtr(HtClient);
                case WmSize:
                    // WM_SIZE may fire dozens of times during one interactive resize.
                    // Resize the native surface now, but present at most once per WPF frame.
                    RefreshNativeView();
                    break;
                case WmDpiChanged:
                    ScheduleNativeViewRefresh();
                    break;
                case WmSetFocus:
                    break;
                case WmKillFocus:
                case WmCancelMode:
                    CancelInteraction();
                    break;
                case WmPaint:
                    // Do not redraw OCCT from WM_PAINT. DefWindowProc remains responsible
                    // for validating the paint region; OCCT presentation is frame-coalesced.
                    break;
                case WmEraseBkgnd:
                    // OpenGL owns the complete child surface; the dedicated window class
                    // also has no background brush, so Windows must never erase it.
                    handled = true;
                    return new IntPtr(1);
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
                    HandleMouseWheel(wParam);
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

        return IntPtr.Zero;
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        ScheduleNativeViewRefresh();
    }

    private void HandleLeftButtonDown(IntPtr hwnd, IntPtr lParam)
    {
        SetFocus(hwnd);
        Focus();
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
        Focus();
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
        Focus();
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
            TryInvoke(() => _engine.Pan(x - _lastMouseX, -(y - _lastMouseY)));
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
                TryInvoke(() => WorldPointChanged.Invoke(this, new OcctWpfWorldPointEventArgs(x, y, _engine.ScreenToWorld(x, y))));
            }
        }

        _lastMouseX = x;
        _lastMouseY = y;
    }

    private void HandleMouseWheel(IntPtr wParam)
    {
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction) return;
        var delta = GetHighWordSigned(wParam);
        if (delta == 0) return;
        TryInvoke(() => _engine.Zoom(OcctViewportInteractionPolicy.ZoomFactor(delta, ZoomSensitivity)));
    }

    private void UpdateSelectionFrame(int x, int y)
    {
        if (_engine?.IsInitialized != true) return;

        _selectionCurrentX = x;
        _selectionCurrentY = y;
        var threshold = Math.Max(0, RectangleSelectionThreshold);
        var dx = Math.Abs(x - _selectionStartX);
        var dy = Math.Abs(y - _selectionStartY);
        if (dx < threshold && dy < threshold)
        {
            HideSelectionFrame();
            return;
        }

        _rectangleDragStarted = true;
        var frame = new SelectionFrame(
            Math.Min(_selectionStartX, x),
            Math.Min(_selectionStartY, y),
            Math.Max(_selectionStartX, x),
            Math.Max(_selectionStartY, y));
        if (_selectionFrame == frame) return;

        TryInvoke(() => _engine.ShowSelectionRectangle(
            frame.Left,
            frame.Top,
            frame.Right,
            frame.Bottom,
            ToDrawingColor(RectangleSelectionLineColor),
            ToDrawingColor(RectangleSelectionFillColor),
            RectangleSelectionFillTransparency,
            RectangleSelectionLineWidth));
        _selectionFrame = frame;
    }

    private void HideSelectionFrame()
    {
        if (_selectionFrame is null) return;
        if (_engine?.IsInitialized == true)
            TryInvoke(_engine.HideSelectionRectangle);
        _selectionFrame = null;
    }

    private void CancelRectangleSelection(bool releaseCapture = true)
    {
        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        _selectionCurrentX = 0;
        _selectionCurrentY = 0;
        HideSelectionFrame();
        if (releaseCapture) ReleaseCapture();
    }

    private void CancelInteraction()
    {
        _rotating = false;
        _panning = false;
        CancelRectangleSelection();
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
            // DPI/layout refresh is rare. Resize the surface without presentation;
            // repeated layout messages share the same coalesced render callback.
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
            ErrorOccurred?.Invoke(this, new OcctWpfErrorEventArgs(exception));
        }
        catch (Exception handlerException)
        {
            System.Diagnostics.Debug.WriteLine(handlerException);
        }
    }

    private static void OnInteractionPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs _)
    {
        var viewport = (OcctWpfViewport)dependencyObject;
        viewport._enableDefaultInteraction = viewport.EnableDefaultInteraction;
        viewport._zoomSensitivity = viewport.ZoomSensitivity;
        if (!viewport._enableDefaultInteraction) viewport.CancelInteraction();
    }

    private static void OnDpiSynchronizationChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs _)
    {
        ((OcctWpfViewport)dependencyObject).ScheduleNativeViewRefresh();
    }

    private static object CoercePositiveInteger(DependencyObject _, object value) => Math.Max(1, (int)value);

    private static object CoercePositiveDouble(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number) && number > 0.0 ? number : 1.0;
    }

    private static object CoerceZoomSensitivity(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number) ? Math.Clamp(number, 0.1, 5.0) : 1.0;
    }

    private static object CoerceUnitInterval(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number) ? Math.Clamp(number, 0.0, 1.0) : 0.82;
    }

    private static DrawingColor ToDrawingColor(MediaColor value) => DrawingColor.FromArgb(value.A, value.R, value.G, value.B);

    private static bool IsKeyDown(int virtualKey) => (GetKeyState(virtualKey) & 0x8000) != 0;

    private static (int X, int Y) GetPoint(IntPtr lParam) => (GetLowWordSigned(lParam), GetHighWordSigned(lParam));

    private static int GetLowWordSigned(IntPtr value) => unchecked((short)(value.ToInt64() & 0xFFFF));

    private static int GetHighWordSigned(IntPtr value) => unchecked((short)((value.ToInt64() >> 16) & 0xFFFF));

    private readonly record struct SelectionFrame(int Left, int Top, int Right, int Bottom);

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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCapture(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int virtualKey);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);
}
