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
    private bool _rotating;
    private bool _panning;
    private bool _selectingRectangle;
    private Rectangle? _selectionFrameScreen;

    public OcctViewportControl()
    {
        SetStyle(ControlStyles.UserPaint, false);
        SetStyle(ControlStyles.AllPaintingInWmPaint, false);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
        BackColor = Color.FromArgb(240, 245, 250);
        TabStop = true;
    }

    public OcctEngine Engine => _engine ?? throw new InvalidOperationException("The OCCT viewport handle has not been created yet.");
    public bool EnableRectangleSelection { get; set; } = true;
    public int RectangleSelectionThreshold { get; set; } = 5;

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
        EngineInitialized?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _engine?.Dispose();
        _engine = null;
        base.OnHandleDestroyed(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_engine?.IsInitialized == true && Width > 0 && Height > 0) TryInvoke(() => _engine.Resize());
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
            _rotating = true;
            TryInvoke(() => _engine.StartRotation(e.X, e.Y));
        }
        else if (e.Button == MouseButtons.Middle)
        {
            _panning = true;
        }
        else if (e.Button == MouseButtons.Left)
        {
            EraseSelectionFrame();
            _selectionStart = e.Location;
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
        else if (_selectingRectangle && e.Button.HasFlag(MouseButtons.Left))
        {
            UpdateSelectionFrame(e.Location);
        }
        else
        {
            TryInvoke(() => _engine.MoveTo(e.X, e.Y));
            TryInvoke(() => WorldPointChanged?.Invoke(this, new OcctViewportWorldPointEventArgs(e.X, e.Y, _engine.ScreenToWorld(e.X, e.Y))));
        }
        _lastMouse = e.Location;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_engine?.IsInitialized != true) return;
        if (e.Button == MouseButtons.Right) _rotating = false;
        else if (e.Button == MouseButtons.Middle) _panning = false;
        else if (e.Button == MouseButtons.Left)
        {
            EraseSelectionFrame();
            Capture = false;
            var dx = Math.Abs(e.X - _selectionStart.X);
            var dy = Math.Abs(e.Y - _selectionStart.Y);
            var append = ModifierKeys.HasFlag(Keys.Control);
            TryInvoke(() =>
            {
                if (_selectingRectangle && (dx >= RectangleSelectionThreshold || dy >= RectangleSelectionThreshold))
                    _engine.SelectRectangle(_selectionStart.X, _selectionStart.Y, e.X, e.Y, append);
                else
                    _engine.Select(e.X, e.Y, append);
                RaiseSelectionChanged();
            });
            _selectingRectangle = false;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_engine?.IsInitialized == true) TryInvoke(() => _engine.Zoom(e.Delta > 0 ? 1.15 : 0.87));
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        if (!Capture)
        {
            EraseSelectionFrame();
            _selectingRectangle = false;
        }
        base.OnMouseCaptureChanged(e);
    }

    private void UpdateSelectionFrame(Point current)
    {
        var dx = Math.Abs(current.X - _selectionStart.X);
        var dy = Math.Abs(current.Y - _selectionStart.Y);
        if (dx < RectangleSelectionThreshold && dy < RectangleSelectionThreshold)
        {
            EraseSelectionFrame();
            return;
        }

        EraseSelectionFrame();
        var clientRectangle = Rectangle.FromLTRB(
            Math.Min(_selectionStart.X, current.X),
            Math.Min(_selectionStart.Y, current.Y),
            Math.Max(_selectionStart.X, current.X),
            Math.Max(_selectionStart.Y, current.Y));
        var screenRectangle = RectangleToScreen(clientRectangle);
        ControlPaint.DrawReversibleFrame(screenRectangle, Color.DodgerBlue, FrameStyle.Dashed);
        _selectionFrameScreen = screenRectangle;
    }

    private void EraseSelectionFrame()
    {
        if (_selectionFrameScreen is not { } rectangle) return;
        ControlPaint.DrawReversibleFrame(rectangle, Color.DodgerBlue, FrameStyle.Dashed);
        _selectionFrameScreen = null;
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
