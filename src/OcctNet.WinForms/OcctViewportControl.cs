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

public sealed class OcctViewportControl : Control
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
    private bool _enableDefaultInteraction = true;

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
    public bool EnableDefaultInteraction
    {
        get => _enableDefaultInteraction;
        set
        {
            if (_enableDefaultInteraction == value) return;
            _enableDefaultInteraction = value;
            if (!value)
            {
                _rotating = false;
                _panning = false;
                CancelRectangleSelection();
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EnableRectangleSelection { get; set; } = true;

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

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (DesignMode) return;
        _engine = new OcctEngine();
        _engine.Initialize(Handle);
        _lastNativeSize = ClientSize;
        _lastHoverTimestamp = 0;
        _lastWorldPointTimestamp = 0;
        EngineInitialized?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        HideSelectionFrame();
        _engine?.Dispose();
        _engine = null;
        _lastNativeSize = Size.Empty;
        base.OnHandleDestroyed(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // WindowsFormsHost and first-focus DPI/layout negotiation can resize the HWND while
        // the first rectangle gesture is active. Preserve the gesture and rebuild its overlay.
        var restoreRectangle = IsActiveRectangleGesture && _rectangleDragStarted;
        if (restoreRectangle) HideSelectionFrame();
        ResizeNativeView();
        if (restoreRectangle) ScheduleSelectionFrameRestore();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            ResizeNativeView(force: true);
            if (IsActiveRectangleGesture && _rectangleDragStarted)
                ScheduleSelectionFrameRestore();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        if (_engine?.IsInitialized != true) base.OnPaintBackground(pevent);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        _lastMouse = e.Location;
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction) return;

        if (e.Button == MouseButtons.Right)
        {
            CancelRectangleSelection();
            _rotating = true;
            TryInvoke(() => _engine.StartRotation(e.X, e.Y));
        }
        else if (e.Button == MouseButtons.Middle)
        {
            CancelRectangleSelection();
            _panning = true;
        }
        else if (e.Button == MouseButtons.Left && !ModifierKeys.HasFlag(Keys.Shift))
        {
            CancelRectangleSelection();
            _selectionStart = e.Location;
            _selectionCurrent = e.Location;
            _leftSelectionGesture = true;
            _rectangleDragStarted = false;
            _selectingRectangle = EnableRectangleSelection;
            if (_selectingRectangle) EnsureRectangleCapture();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_engine?.IsInitialized != true || !EnableDefaultInteraction)
        {
            _lastMouse = e.Location;
            return;
        }

        if (_rotating && e.Button.HasFlag(MouseButtons.Right))
        {
            TryInvoke(() => _engine.Rotation(e.X, e.Y));
        }
        else if (_panning && e.Button.HasFlag(MouseButtons.Middle))
        {
            var dx = e.X - _lastMouse.X;
            var dy = e.Y - _lastMouse.Y;
            TryInvoke(() => _engine.Pan(dx, -dy));
        }
        else if (IsActiveRectangleGesture)
        {
            EnsureRectangleCapture();
            _selectionCurrent = e.Location;
            UpdateSelectionFrame(e.Location);
        }
        else
        {
            var now = System.Diagnostics.Stopwatch.GetTimestamp();
            if (OcctViewportInteractionPolicy.HasElapsed(
                    _lastHoverTimestamp,
                    now,
                    OcctViewportInteractionPolicy.HoverIntervalTicks))
            {
                _lastHoverTimestamp = now;
                TryInvoke(() => _engine.MoveTo(e.X, e.Y));
            }

            if (WorldPointChanged is not null
                && OcctViewportInteractionPolicy.HasElapsed(
                    _lastWorldPointTimestamp,
                    now,
                    OcctViewportInteractionPolicy.WorldPointIntervalTicks))
            {
                _lastWorldPointTimestamp = now;
                TryInvoke(() => WorldPointChanged.Invoke(
                    this,
                    new OcctViewportWorldPointEventArgs(e.X, e.Y, _engine.ScreenToWorld(e.X, e.Y))));
            }
        }
        _lastMouse = e.Location;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!EnableDefaultInteraction) return;
        if (e.Button == MouseButtons.Right)
        {
            _rotating = false;
            return;
        }
        if (e.Button == MouseButtons.Middle)
        {
            _panning = false;
            return;
        }
        if (e.Button != MouseButtons.Left || !_leftSelectionGesture) return;

        var resolvedEnd = OcctViewportInteractionPolicy.ResolveSelectionEnd(
            _selectionStart.X,
            _selectionStart.Y,
            e.X,
            e.Y,
            _selectionCurrent.X,
            _selectionCurrent.Y,
            _rectangleDragStarted);
        var end = new Point(resolvedEnd.X, resolvedEnd.Y);
        var useRectangle = OcctViewportInteractionPolicy.ShouldUseRectangle(
            EnableRectangleSelection,
            _rectangleDragStarted,
            RectangleSelectionThreshold,
            _selectionStart.X,
            _selectionStart.Y,
            end.X,
            end.Y);
        var append = ModifierKeys.HasFlag(Keys.Control);
        var allowOverlap = OcctViewportInteractionPolicy.AllowsOverlap(
            RectangleSelectionBehavior,
            _selectionStart.X,
            end.X);

        // MouseCaptureChanged may be raised before MouseUp, especially when this WinForms
        // control is hosted by WPF. Keep a dedicated gesture flag until selection is committed.
        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        ReleaseMouseCapture();

        if (_engine?.IsInitialized != true) return;
        TryInvoke(() =>
        {
            if (useRectangle)
                _engine.SelectRectangle(
                    _selectionStart.X,
                    _selectionStart.Y,
                    end.X,
                    end.Y,
                    append,
                    allowOverlap);
            else
                _engine.Select(e.X, e.Y, append);
            RaiseSelectionChanged();
        });
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (EnableDefaultInteraction && _engine?.IsInitialized == true)
            TryInvoke(() => _engine.Zoom(OcctViewportInteractionPolicy.ZoomFactor(e.Delta)));
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        if (!Capture && !_releasingMouseCapture)
        {
            if (IsActiveRectangleGesture)
            {
                // WPF's WindowsFormsHost and first-focus activation can transiently take
                // capture. Recover it asynchronously instead of losing the first drag.
                ScheduleRectangleCaptureRecovery();
            }
            else
            {
                HideSelectionFrame();
            }
        }
        base.OnMouseCaptureChanged(e);
    }

    private bool IsActiveRectangleGesture =>
        _leftSelectionGesture
        && _selectingRectangle
        && Control.MouseButtons.HasFlag(MouseButtons.Left);

    private void EnsureRectangleCapture()
    {
        if (!IsActiveRectangleGesture || Capture || IsDisposed || Disposing || !IsHandleCreated)
            return;
        Capture = true;
    }

    private void ScheduleRectangleCaptureRecovery()
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return;
        BeginInvoke((Action)(() =>
        {
            if (IsActiveRectangleGesture)
                EnsureRectangleCapture();
        }));
    }

    private void ScheduleSelectionFrameRestore()
    {
        if (_rectangleRestoreScheduled || IsDisposed || Disposing || !IsHandleCreated) return;
        _rectangleRestoreScheduled = true;
        BeginInvoke((Action)(() =>
        {
            _rectangleRestoreScheduled = false;
            if (IsActiveRectangleGesture && _rectangleDragStarted)
                UpdateSelectionFrame(_selectionCurrent);
        }));
    }

    private void ResizeNativeView(bool force = false)
    {
        if (_engine?.IsInitialized != true
            || !Visible
            || ClientSize.Width <= 0
            || ClientSize.Height <= 0)
        {
            return;
        }

        if (!force && _lastNativeSize == ClientSize)
            return;

        _lastNativeSize = ClientSize;
        TryInvoke(_engine.Resize);
    }

    private void UpdateSelectionFrame(Point current)
    {
        if (_engine?.IsInitialized != true) return;

        _selectionCurrent = current;
        var threshold = Math.Max(0, RectangleSelectionThreshold);
        var dx = Math.Abs(current.X - _selectionStart.X);
        var dy = Math.Abs(current.Y - _selectionStart.Y);
        if (dx < threshold && dy < threshold)
        {
            HideSelectionFrame();
            return;
        }

        _rectangleDragStarted = true;
        var rectangle = Rectangle.FromLTRB(
            Math.Min(_selectionStart.X, current.X),
            Math.Min(_selectionStart.Y, current.Y),
            Math.Max(_selectionStart.X, current.X),
            Math.Max(_selectionStart.Y, current.Y));
        if (_selectionFrameClient == rectangle) return;

        TryInvoke(() => _engine.ShowSelectionRectangle(
            rectangle.Left,
            rectangle.Top,
            rectangle.Right,
            rectangle.Bottom,
            RectangleSelectionLineColor,
            RectangleSelectionFillColor,
            RectangleSelectionFillTransparency,
            RectangleSelectionLineWidth));
        _selectionFrameClient = rectangle;
    }

    private void HideSelectionFrame()
    {
        if (_selectionFrameClient is null) return;
        if (_engine?.IsInitialized == true)
            TryInvoke(() => _engine.HideSelectionRectangle());
        _selectionFrameClient = null;
    }

    private void CancelRectangleSelection()
    {
        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        _selectionCurrent = Point.Empty;
        HideSelectionFrame();
        if (Capture) ReleaseMouseCapture();
    }

    private void ReleaseMouseCapture()
    {
        if (!Capture) return;
        _releasingMouseCapture = true;
        try
        {
            Capture = false;
        }
        finally
        {
            _releasingMouseCapture = false;
        }
    }

    public void RaiseSelectionChanged()
    {
        if (_engine?.IsInitialized != true) return;
        var selected = _engine.FirstSelectedObject;
        SelectionChanged?.Invoke(this, _engine.FirstSelected);
        ObjectSelectionChanged?.Invoke(this, new OcctViewportSelectionEventArgs(selected, _engine.SelectedObjects));
    }

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
