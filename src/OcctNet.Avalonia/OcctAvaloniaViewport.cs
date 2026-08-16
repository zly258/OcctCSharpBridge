using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

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
/// Cross-platform Avalonia host for the OCCT viewer.
/// </summary>
/// <remarks>
/// Windows uses HWND/WNT_Window. Linux currently uses the X11/XWayland XID backend/Xw_Window.
/// The public control API is platform-neutral. Native Wayland hosting can therefore be added later
/// without changing application code that consumes <see cref="OcctAvaloniaViewport"/>.
/// </remarks>
public sealed partial class OcctAvaloniaViewport : NativeControlHost
{
    private readonly WndProcDelegate _windowProcedure;
    private readonly HashSet<uint> _x11PressedKeys = [];
    private OcctEngine? _engine;
    private IntPtr _nativeHandle;
    private IntPtr _previousWindowProcedure;
    private IntPtr _x11Display;
    private DispatcherTimer? _x11InputTimer;
    private OcctViewportInteractionFeatures _interactionFeatures = OcctViewportInteractionFeatures.Default;
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

    public OcctAvaloniaViewport()
    {
        _windowProcedure = WindowProcedure;
        Focusable = true;
        SizeChanged += OnHostSizeChanged;
    }

    public OcctEngine Engine => _engine ?? throw new InvalidOperationException("The Avalonia OCCT viewport has not been created yet.");
    public IntPtr NativeHandle => _nativeHandle;
    public bool IsEngineInitialized => _engine?.IsInitialized == true;

    public OcctViewportInteractionFeatures InteractionFeatures
    {
        get => _interactionFeatures;
        set
        {
            const OcctViewportInteractionFeatures allowed = OcctViewportInteractionFeatures.Default;
            if ((value & ~allowed) != 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown viewport interaction feature flags.");
            if (_interactionFeatures == value) return;

            _interactionFeatures = value;
            if (!HasInteractionFeature(OcctViewportInteractionFeatures.Rotate)) _rotating = false;
            if (!HasInteractionFeature(OcctViewportInteractionFeatures.Pan)) _panning = false;
            if (!HasInteractionFeature(OcctViewportInteractionFeatures.RectangleSelection) && _selectingRectangle)
                CancelRectangleSelection();
            else if (!HasInteractionFeature(OcctViewportInteractionFeatures.Selection))
                CancelRectangleSelection();
        }
    }

    public double ZoomSensitivity
    {
        get => _zoomSensitivity;
        set => _zoomSensitivity = OcctViewportInteractionPolicy.NormalizeZoomSensitivity(value);
    }

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
    public event EventHandler<OcctPointerInputEventArgs>? PreviewPointerInput;
    public event EventHandler<OcctPointerInputEventArgs>? PointerInput;
    public event EventHandler<OcctKeyInputEventArgs>? PreviewKeyInput;
    public event EventHandler<OcctKeyInputEventArgs>? KeyInput;

    public void RaiseSelectionChanged()
    {
        if (_engine?.IsInitialized != true) return;
        var selected = _engine.FirstSelectedObject;
        SelectionChanged?.Invoke(this, _engine.FirstSelected);
        ObjectSelectionChanged?.Invoke(this, new OcctAvaloniaSelectionEventArgs(selected, _engine.SelectedObjects));
    }

    private bool HasInteractionFeature(OcctViewportInteractionFeatures feature) =>
        (_interactionFeatures & feature) != 0;

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
}
