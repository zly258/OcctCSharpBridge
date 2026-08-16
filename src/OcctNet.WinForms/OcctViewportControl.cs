using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OcctNet;

public sealed class OcctViewportSelectionEventArgs : EventArgs
{
    public OcctViewportSelectionEventArgs(IOcctObject? selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
    {
        SelectedObject = selectedObject;
        SelectedObjects = selectedObjects;
    }

    public IOcctObject? SelectedObject { get; }
    public IReadOnlyList<IOcctObject> SelectedObjects { get; }
}

public sealed class OcctViewportErrorEventArgs : EventArgs
{
    public OcctViewportErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public Exception Exception { get; }
}

public sealed class OcctViewportWorldPointEventArgs : EventArgs
{
    public OcctViewportWorldPointEventArgs(int screenX, int screenY, OcctPoint3d worldPoint)
    {
        ScreenX = screenX;
        ScreenY = screenY;
        WorldPoint = worldPoint;
    }

    public int ScreenX { get; }
    public int ScreenY { get; }
    public OcctPoint3d WorldPoint { get; }
}

public sealed partial class OcctViewportControl : Control
{
    private OcctEngine? _engine;
    private Point _lastMouse;
    private Point _selectionStart;
    private Point _selectionCurrent;
    private Size _lastNativeSize;
    private bool _leftSelectionGesture;
    private bool _rectangleDragStarted;
    private bool _rotating;
    private bool _panning;
    private bool _selectingRectangle;
    private bool _releasingMouseCapture;
    private bool _rectangleRestoreScheduled;
    private Rectangle? _selectionFrameClient;
    private long _lastHoverTimestamp;
    private long _lastWorldPointTimestamp;
    private OcctViewportInteractionFeatures _interactionFeatures = OcctViewportInteractionFeatures.Default;
    private double _zoomSensitivity = 1.0;
    private readonly HashSet<Keys> _pressedKeys = [];

    public OcctViewportControl()
    {
        // OCCT renders directly into this HWND. WinForms UserPaint/double buffering must stay disabled;
        // the selection rectangle is rendered by OCCT as a top-layer AIS_RubberBand instead.
        SetStyle(ControlStyles.UserPaint, false);
        SetStyle(ControlStyles.AllPaintingInWmPaint, false);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
        BackColor = Color.FromArgb(240, 245, 250);
        TabStop = true;
    }

    public OcctEngine Engine => _engine ?? throw new InvalidOperationException("The OCCT viewport handle has not been created yet.");

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
            if (!HasInteractionFeature(OcctViewportInteractionFeatures.Selection)) CancelRectangleSelection();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double ZoomSensitivity
    {
        get => _zoomSensitivity;
        set => _zoomSensitivity = OcctViewportInteractionPolicy.NormalizeZoomSensitivity(value);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int RectangleSelectionThreshold { get; set; } = 3;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; } = OcctRectangleSelectionBehavior.Inclusive;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color RectangleSelectionLineColor { get; set; } = Color.FromArgb(35, 120, 210);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color RectangleSelectionFillColor { get; set; } = Color.FromArgb(95, 165, 230);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double RectangleSelectionFillTransparency { get; set; } = 0.82;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double RectangleSelectionLineWidth { get; set; } = 1.0;

    public event EventHandler<OcctShape?>? SelectionChanged;
    public event EventHandler<OcctViewportSelectionEventArgs>? ObjectSelectionChanged;
    public event EventHandler<OcctViewportWorldPointEventArgs>? WorldPointChanged;
    public event EventHandler<OcctViewportErrorEventArgs>? ErrorOccurred;
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
        ObjectSelectionChanged?.Invoke(this, new OcctViewportSelectionEventArgs(selected, _engine.SelectedObjects));
    }

    private bool HasInteractionFeature(OcctViewportInteractionFeatures feature) =>
        (_interactionFeatures & feature) != 0;

    private void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            try
            {
                ErrorOccurred?.Invoke(this, new OcctViewportErrorEventArgs(exception));
            }
            catch (Exception handlerException)
            {
                System.Diagnostics.Debug.WriteLine(handlerException);
            }
        }
    }
}
