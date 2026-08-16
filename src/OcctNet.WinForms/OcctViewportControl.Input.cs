using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OcctNet;

public sealed partial class OcctViewportControl
{
    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        _lastMouse = e.Location;

        var input = CreatePointerInput(OcctPointerInputKind.Pressed, e.Button, e.X, e.Y, 0);
        PreviewPointerInput?.Invoke(this, input);
        base.OnMouseDown(e);

        if (!input.Handled)
            ProcessDefaultPointerPressed(e);

        PointerInput?.Invoke(this, input);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var input = CreatePointerInput(OcctPointerInputKind.Moved, MouseButtons.None, e.X, e.Y, 0);
        PreviewPointerInput?.Invoke(this, input);
        base.OnMouseMove(e);

        if (!input.Handled)
            ProcessDefaultPointerMoved(e);
        else
            _lastMouse = e.Location;

        PointerInput?.Invoke(this, input);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        var input = CreatePointerInput(OcctPointerInputKind.Released, e.Button, e.X, e.Y, 0);
        PreviewPointerInput?.Invoke(this, input);
        base.OnMouseUp(e);

        if (!input.Handled)
            ProcessDefaultPointerReleased(e);
        else
            CancelHandledRelease(e.Button);

        PointerInput?.Invoke(this, input);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        var input = CreatePointerInput(OcctPointerInputKind.Wheel, MouseButtons.None, e.X, e.Y, e.Delta);
        PreviewPointerInput?.Invoke(this, input);
        base.OnMouseWheel(e);

        if (!input.Handled
            && HasInteractionFeature(OcctViewportInteractionFeatures.Zoom)
            && _engine?.IsInitialized == true)
        {
            TryInvoke(() => _engine.Zoom(OcctViewportInteractionPolicy.ZoomFactor(e.Delta, ZoomSensitivity)));
        }

        PointerInput?.Invoke(this, input);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var key = e.KeyCode;
        var input = new OcctKeyInputEventArgs(
            OcctKeyInputKind.Pressed,
            MapKey(key),
            ResolveModifiers(),
            isRepeat: !_pressedKeys.Add(key));

        PreviewKeyInput?.Invoke(this, input);
        if (input.Handled)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        base.OnKeyDown(e);
        KeyInput?.Invoke(this, input);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        _pressedKeys.Remove(e.KeyCode);
        var input = new OcctKeyInputEventArgs(
            OcctKeyInputKind.Released,
            MapKey(e.KeyCode),
            ResolveModifiers());

        PreviewKeyInput?.Invoke(this, input);
        if (input.Handled)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        base.OnKeyUp(e);
        KeyInput?.Invoke(this, input);
    }

    private void ProcessDefaultPointerPressed(MouseEventArgs e)
    {
        if (_engine?.IsInitialized != true) return;

        if (e.Button == MouseButtons.Right && HasInteractionFeature(OcctViewportInteractionFeatures.Rotate))
        {
            CancelRectangleSelection();
            _rotating = true;
            TryInvoke(() => _engine.StartRotation(e.X, e.Y));
            return;
        }

        if (e.Button == MouseButtons.Middle && HasInteractionFeature(OcctViewportInteractionFeatures.Pan))
        {
            CancelRectangleSelection();
            _panning = true;
            return;
        }

        if (e.Button != MouseButtons.Left
            || ModifierKeys.HasFlag(Keys.Shift)
            || !HasInteractionFeature(OcctViewportInteractionFeatures.Selection))
        {
            return;
        }

        CancelRectangleSelection();
        _selectionStart = e.Location;
        _selectionCurrent = e.Location;
        _leftSelectionGesture = true;
        _rectangleDragStarted = false;
        _selectingRectangle = HasInteractionFeature(OcctViewportInteractionFeatures.RectangleSelection);
        if (_selectingRectangle) EnsureRectangleCapture();
    }

    private void ProcessDefaultPointerMoved(MouseEventArgs e)
    {
        if (_engine?.IsInitialized != true)
        {
            _lastMouse = e.Location;
            return;
        }

        if (_rotating
            && e.Button.HasFlag(MouseButtons.Right)
            && HasInteractionFeature(OcctViewportInteractionFeatures.Rotate))
        {
            TryInvoke(() => _engine.Rotation(e.X, e.Y));
        }
        else if (_panning
                 && e.Button.HasFlag(MouseButtons.Middle)
                 && HasInteractionFeature(OcctViewportInteractionFeatures.Pan))
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
            var now = Stopwatch.GetTimestamp();
            if (HasInteractionFeature(OcctViewportInteractionFeatures.HoverDetection)
                && OcctViewportInteractionPolicy.HasElapsed(
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

    private void ProcessDefaultPointerReleased(MouseEventArgs e)
    {
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
        var useRectangle =
            HasInteractionFeature(OcctViewportInteractionFeatures.RectangleSelection)
            && OcctViewportInteractionPolicy.ShouldUseRectangle(
                true,
                _rectangleDragStarted,
                RectangleSelectionThreshold,
                _selectionStart.X,
                _selectionStart.Y,
                end.X,
                end.Y);
        var usePoint =
            !useRectangle
            && HasInteractionFeature(OcctViewportInteractionFeatures.PointSelection);
        var append = ModifierKeys.HasFlag(Keys.Control);
        var allowOverlap = OcctViewportInteractionPolicy.AllowsOverlap(
            RectangleSelectionBehavior,
            _selectionStart.X,
            end.X);

        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        ReleaseMouseCapture();

        if (_engine?.IsInitialized != true || (!useRectangle && !usePoint)) return;
        TryInvoke(() =>
        {
            if (useRectangle)
            {
                _engine.SelectRectangle(
                    _selectionStart.X,
                    _selectionStart.Y,
                    end.X,
                    end.Y,
                    append,
                    allowOverlap);
            }
            else
            {
                _engine.Select(e.X, e.Y, append);
            }
            RaiseSelectionChanged();
        });
    }

    private void CancelHandledRelease(MouseButtons button)
    {
        if (button == MouseButtons.Right)
        {
            _rotating = false;
        }
        else if (button == MouseButtons.Middle)
        {
            _panning = false;
        }
        else if (button == MouseButtons.Left && _leftSelectionGesture)
        {
            CancelRectangleSelection();
        }
    }

    private OcctPointerInputEventArgs CreatePointerInput(
        OcctPointerInputKind kind,
        MouseButtons button,
        int x,
        int y,
        int wheelDelta) => new(
            kind,
            MapPointerButton(button),
            MapPointerButtons(Control.MouseButtons),
            x,
            y,
            wheelDelta,
            ResolveModifiers());

    private static OcctPointerButton MapPointerButton(MouseButtons button) => button switch
    {
        MouseButtons.Left => OcctPointerButton.Left,
        MouseButtons.Middle => OcctPointerButton.Middle,
        MouseButtons.Right => OcctPointerButton.Right,
        MouseButtons.XButton1 => OcctPointerButton.X1,
        MouseButtons.XButton2 => OcctPointerButton.X2,
        _ => OcctPointerButton.None
    };

    private static OcctPointerButtons MapPointerButtons(MouseButtons buttons)
    {
        var result = OcctPointerButtons.None;
        if (buttons.HasFlag(MouseButtons.Left)) result |= OcctPointerButtons.Left;
        if (buttons.HasFlag(MouseButtons.Middle)) result |= OcctPointerButtons.Middle;
        if (buttons.HasFlag(MouseButtons.Right)) result |= OcctPointerButtons.Right;
        if (buttons.HasFlag(MouseButtons.XButton1)) result |= OcctPointerButtons.X1;
        if (buttons.HasFlag(MouseButtons.XButton2)) result |= OcctPointerButtons.X2;
        return result;
    }

    private static OcctInputModifiers ResolveModifiers()
    {
        var keys = ModifierKeys;
        var result = OcctInputModifiers.None;
        if (keys.HasFlag(Keys.Shift)) result |= OcctInputModifiers.Shift;
        if (keys.HasFlag(Keys.Control)) result |= OcctInputModifiers.Control;
        if (keys.HasFlag(Keys.Alt)) result |= OcctInputModifiers.Alt;
        if (keys.HasFlag(Keys.LWin) || keys.HasFlag(Keys.RWin)) result |= OcctInputModifiers.Meta;
        return result;
    }

    private static OcctKey MapKey(Keys key)
    {
        var value = (int)key;
        if (value >= (int)Keys.A && value <= (int)Keys.Z)
            return (OcctKey)((int)OcctKey.A + value - (int)Keys.A);
        if (value >= (int)Keys.D0 && value <= (int)Keys.D9)
            return (OcctKey)((int)OcctKey.D0 + value - (int)Keys.D0);
        if (value >= (int)Keys.F1 && value <= (int)Keys.F12)
            return (OcctKey)((int)OcctKey.F1 + value - (int)Keys.F1);

        return key switch
        {
            Keys.Escape => OcctKey.Escape,
            Keys.Enter => OcctKey.Enter,
            Keys.Tab => OcctKey.Tab,
            Keys.Back => OcctKey.Backspace,
            Keys.Space => OcctKey.Space,
            Keys.Delete => OcctKey.Delete,
            Keys.Insert => OcctKey.Insert,
            Keys.Home => OcctKey.Home,
            Keys.End => OcctKey.End,
            Keys.PageUp => OcctKey.PageUp,
            Keys.PageDown => OcctKey.PageDown,
            Keys.Left => OcctKey.Left,
            Keys.Right => OcctKey.Right,
            Keys.Up => OcctKey.Up,
            Keys.Down => OcctKey.Down,
            Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey => OcctKey.Shift,
            Keys.ControlKey or Keys.LControlKey or Keys.RControlKey => OcctKey.Control,
            Keys.Menu or Keys.LMenu or Keys.RMenu => OcctKey.Alt,
            Keys.LWin or Keys.RWin => OcctKey.Meta,
            _ => OcctKey.Unknown
        };
    }
}
