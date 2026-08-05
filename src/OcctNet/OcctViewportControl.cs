using System.Drawing;
using System.Windows.Forms;

namespace OcctNet;

public sealed class OcctViewportSelectionEventArgs : EventArgs
{
    public OcctViewportSelectionEventArgs(OcctObject? selectedObject, IReadOnlyList<OcctObject> selectedObjects)
    {
        SelectedObject = selectedObject;
        SelectedObjects = selectedObjects;
    }

    public OcctObject? SelectedObject { get; }
    public IReadOnlyList<OcctObject> SelectedObjects { get; }
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
    private bool _rectangleDragStarted;
    private bool _rotating;
    private bool _panning;
    private bool _selectingRectangle;
    private bool _releasingMouseCapture;
    private Rectangle? _selectionFrameClient;

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
    public bool EnableRectangleSelection { get; set; } = true;
    public int RectangleSelectionThreshold { get; set; } = 5;
    public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; } = OcctRectangleSelectionBehavior.Overlap;
    public Color RectangleSelectionLineColor { get; set; } = Color.FromArgb(35, 120, 210);
    public Color RectangleSelectionFillColor { get; set; } = Color.FromArgb(95, 165, 230);
    public double RectangleSelectionFillTransparency { get; set; } = 0.82;
    public double RectangleSelectionLineWidth { get; set; } = 1.0;

    public event EventHandler<OcctShape?>? SelectionChanged;
    public event EventHandler<OcctViewportSelectionEventArgs>? ObjectSelectionChanged;
    public event EventHandler<OcctViewportWorldPointEventArgs>? WorldPointChanged;
    public event EventHandler? EngineInitialized;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (DesignMode) return;
        _engine = new OcctEngine();
        _engine.Initialize(Handle);
        _lastNativeSize = ClientSize;
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
        CancelRectangleSelection();
        ResizeNativeView();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            ResizeNativeView(force: true);
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
        if (_engine?.IsInitialized != true) return;

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
        else if (e.Button == MouseButtons.Left)
        {
            CancelRectangleSelection();
            _selectionStart = e.Location;
            _selectionCurrent = e.Location;
            _rectangleDragStarted = false;
            _selectingRectangle = EnableRectangleSelection;
            Capture = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_engine?.IsInitialized != true) return;

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
        else if (_selectingRectangle
                 && (Capture || Control.MouseButtons.HasFlag(MouseButtons.Left)))
        {
            _selectionCurrent = e.Location;
            UpdateSelectionFrame(e.Location);
        }
        else
        {
            TryInvoke(() => _engine.MoveTo(e.X, e.Y));
            TryInvoke(() => WorldPointChanged?.Invoke(
                this,
                new OcctViewportWorldPointEventArgs(e.X, e.Y, _engine.ScreenToWorld(e.X, e.Y))));
        }
        _lastMouse = e.Location;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
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
        if (e.Button != MouseButtons.Left) return;

        var end = e.Location;
        var eventDistance = Math.Max(
            Math.Abs(end.X - _selectionStart.X),
            Math.Abs(end.Y - _selectionStart.Y));
        var trackedDistance = Math.Max(
            Math.Abs(_selectionCurrent.X - _selectionStart.X),
            Math.Abs(_selectionCurrent.Y - _selectionStart.Y));
        if (_rectangleDragStarted && trackedDistance > eventDistance)
            end = _selectionCurrent;

        var dx = Math.Abs(end.X - _selectionStart.X);
        var dy = Math.Abs(end.Y - _selectionStart.Y);
        var useRectangle = EnableRectangleSelection
                           && (_rectangleDragStarted
                               || dx >= RectangleSelectionThreshold
                               || dy >= RectangleSelectionThreshold);
        var append = ModifierKeys.HasFlag(Keys.Control);
        var allowOverlap = RectangleSelectionBehavior switch
        {
            OcctRectangleSelectionBehavior.Overlap => true,
            OcctRectangleSelectionBehavior.Directional => end.X < _selectionStart.X,
            _ => false
        };

        // MouseCaptureChanged may be raised before MouseUp, especially when this WinForms
        // control is hosted by WPF. The recognized drag is stored independently so the
        // gesture cannot silently degrade into a point selection.
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
        if (_engine?.IsInitialized == true) TryInvoke(() => _engine.Zoom(e.Delta > 0 ? 1.15 : 0.87));
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        if (!Capture && !_releasingMouseCapture)
        {
            // Do not clear a recognized rectangle here. WinFormsHost and DPI/layout changes
            // can deliver CaptureChanged before MouseUp; MouseUp must still finalize the box.
            HideSelectionFrame();
        }
        base.OnMouseCaptureChanged(e);
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
        {
            return;
        }

        _lastNativeSize = ClientSize;
        TryInvoke(_engine.Resize);
    }

    private void UpdateSelectionFrame(Point current)
    {
        if (_engine?.IsInitialized != true) return;

        _selectionCurrent = current;
        var dx = Math.Abs(current.X - _selectionStart.X);
        var dy = Math.Abs(current.Y - _selectionStart.Y);
        if (dx < RectangleSelectionThreshold && dy < RectangleSelectionThreshold)
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

    private static void TryInvoke(Action action)
    {
        try { action(); }
        catch (Exception exception) { System.Diagnostics.Debug.WriteLine(exception); }
    }
}
