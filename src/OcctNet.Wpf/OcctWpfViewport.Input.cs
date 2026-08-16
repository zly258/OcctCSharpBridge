using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctWpfViewport
{
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
                    break;
                case WmEraseBkgnd:
                    handled = true;
                    return new IntPtr(1);
                case WmLButtonDown:
                    HandlePointerPressed(hwnd, wParam, lParam, OcctPointerButton.Left);
                    break;
                case WmLButtonUp:
                    HandlePointerReleased(wParam, lParam, OcctPointerButton.Left);
                    break;
                case WmRButtonDown:
                    HandlePointerPressed(hwnd, wParam, lParam, OcctPointerButton.Right);
                    break;
                case WmRButtonUp:
                    HandlePointerReleased(wParam, lParam, OcctPointerButton.Right);
                    break;
                case WmMButtonDown:
                    HandlePointerPressed(hwnd, wParam, lParam, OcctPointerButton.Middle);
                    break;
                case WmMButtonUp:
                    HandlePointerReleased(wParam, lParam, OcctPointerButton.Middle);
                    break;
                case WmMouseMove:
                    HandlePointerMoved(wParam, lParam);
                    break;
                case WmMouseWheel:
                    HandlePointerWheel(hwnd, wParam, lParam);
                    break;
                case WmKeyDown:
                case WmSysKeyDown:
                    handled = HandleKeyMessage(OcctKeyInputKind.Pressed, wParam, lParam);
                    break;
                case WmKeyUp:
                case WmSysKeyUp:
                    handled = HandleKeyMessage(OcctKeyInputKind.Released, wParam, lParam);
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

    private void HandlePointerPressed(IntPtr hwnd, IntPtr wParam, IntPtr lParam, OcctPointerButton button)
    {
        SetFocus(hwnd);
        Focus();
        var (x, y) = GetPoint(lParam);
        _lastMouseX = x;
        _lastMouseY = y;

        var input = CreatePointerInput(OcctPointerInputKind.Pressed, button, wParam, x, y, 0);
        PreviewPointerInput?.Invoke(this, input);
        if (!input.Handled)
            ProcessDefaultPointerPressed(hwnd, button, x, y);
        PointerInput?.Invoke(this, input);
    }

    private void HandlePointerReleased(IntPtr wParam, IntPtr lParam, OcctPointerButton button)
    {
        var (x, y) = GetPoint(lParam);
        var input = CreatePointerInput(OcctPointerInputKind.Released, button, wParam, x, y, 0);
        PreviewPointerInput?.Invoke(this, input);
        if (!input.Handled)
            ProcessDefaultPointerReleased(button, x, y);
        else
            CancelHandledRelease(button);
        PointerInput?.Invoke(this, input);
    }

    private void HandlePointerMoved(IntPtr wParam, IntPtr lParam)
    {
        var (x, y) = GetPoint(lParam);
        var input = CreatePointerInput(OcctPointerInputKind.Moved, OcctPointerButton.None, wParam, x, y, 0);
        PreviewPointerInput?.Invoke(this, input);
        if (!input.Handled)
            ProcessDefaultPointerMoved(wParam, x, y);
        else
        {
            _lastMouseX = x;
            _lastMouseY = y;
        }
        PointerInput?.Invoke(this, input);
    }

    private void HandlePointerWheel(IntPtr hwnd, IntPtr wParam, IntPtr lParam)
    {
        var delta = GetHighWordSigned(wParam);
        var (x, y) = GetWheelPoint(hwnd, lParam);
        var input = CreatePointerInput(OcctPointerInputKind.Wheel, OcctPointerButton.None, wParam, x, y, delta);
        PreviewPointerInput?.Invoke(this, input);

        if (!input.Handled
            && delta != 0
            && HasInteractionFeature(OcctViewportInteractionFeatures.Zoom)
            && _engine?.IsInitialized == true)
        {
            TryInvoke(() => _engine.Zoom(OcctViewportInteractionPolicy.ZoomFactor(delta, ZoomSensitivity)));
        }

        PointerInput?.Invoke(this, input);
    }

    private void ProcessDefaultPointerPressed(IntPtr hwnd, OcctPointerButton button, int x, int y)
    {
        if (_engine?.IsInitialized != true) return;

        if (button == OcctPointerButton.Right && HasInteractionFeature(OcctViewportInteractionFeatures.Rotate))
        {
            CancelRectangleSelection();
            _rotating = true;
            SetCapture(hwnd);
            TryInvoke(() => _engine.StartRotation(x, y));
            return;
        }

        if (button == OcctPointerButton.Middle && HasInteractionFeature(OcctViewportInteractionFeatures.Pan))
        {
            CancelRectangleSelection();
            _panning = true;
            SetCapture(hwnd);
            return;
        }

        if (button != OcctPointerButton.Left
            || IsKeyDown(VkShift)
            || !HasInteractionFeature(OcctViewportInteractionFeatures.Selection))
        {
            return;
        }

        CancelRectangleSelection();
        _selectionStartX = x;
        _selectionStartY = y;
        _selectionCurrentX = x;
        _selectionCurrentY = y;
        _leftSelectionGesture = true;
        _rectangleDragStarted = false;
        _selectingRectangle = HasInteractionFeature(OcctViewportInteractionFeatures.RectangleSelection);
        if (_selectingRectangle) SetCapture(hwnd);
    }

    private void ProcessDefaultPointerReleased(OcctPointerButton button, int x, int y)
    {
        if (button == OcctPointerButton.Right)
        {
            _rotating = false;
            ReleaseCapture();
            return;
        }

        if (button == OcctPointerButton.Middle)
        {
            _panning = false;
            ReleaseCapture();
            return;
        }

        if (button != OcctPointerButton.Left || !_leftSelectionGesture) return;

        var end = OcctViewportInteractionPolicy.ResolveSelectionEnd(
            _selectionStartX,
            _selectionStartY,
            x,
            y,
            _selectionCurrentX,
            _selectionCurrentY,
            _rectangleDragStarted);
        var useRectangle =
            HasInteractionFeature(OcctViewportInteractionFeatures.RectangleSelection)
            && OcctViewportInteractionPolicy.ShouldUseRectangle(
                true,
                _rectangleDragStarted,
                RectangleSelectionThreshold,
                _selectionStartX,
                _selectionStartY,
                end.X,
                end.Y);
        var usePoint =
            !useRectangle
            && HasInteractionFeature(OcctViewportInteractionFeatures.PointSelection);
        var append = IsKeyDown(VkControl);
        var allowOverlap = OcctViewportInteractionPolicy.AllowsOverlap(RectangleSelectionBehavior, _selectionStartX, end.X);

        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        ReleaseCapture();

        if (_engine?.IsInitialized != true || (!useRectangle && !usePoint)) return;
        TryInvoke(() =>
        {
            if (useRectangle)
                _engine.SelectRectangle(_selectionStartX, _selectionStartY, end.X, end.Y, append, allowOverlap);
            else
                _engine.Select(end.X, end.Y, append);
            RaiseSelectionChanged();
        });
    }

    private void ProcessDefaultPointerMoved(IntPtr wParam, int x, int y)
    {
        if (_engine?.IsInitialized != true)
        {
            _lastMouseX = x;
            _lastMouseY = y;
            return;
        }

        var buttons = unchecked((int)wParam.ToInt64());
        if (_rotating
            && (buttons & MkRButton) != 0
            && HasInteractionFeature(OcctViewportInteractionFeatures.Rotate))
        {
            TryInvoke(() => _engine.Rotation(x, y));
        }
        else if (_panning
                 && (buttons & MkMButton) != 0
                 && HasInteractionFeature(OcctViewportInteractionFeatures.Pan))
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
            if (HasInteractionFeature(OcctViewportInteractionFeatures.HoverDetection)
                && OcctViewportInteractionPolicy.HasElapsed(_lastHoverTimestamp, now, OcctViewportInteractionPolicy.HoverIntervalTicks))
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

    private void CancelHandledRelease(OcctPointerButton button)
    {
        switch (button)
        {
            case OcctPointerButton.Right:
                _rotating = false;
                ReleaseCapture();
                break;
            case OcctPointerButton.Middle:
                _panning = false;
                ReleaseCapture();
                break;
            case OcctPointerButton.Left when _leftSelectionGesture:
                CancelRectangleSelection();
                break;
        }
    }

    private bool HandleKeyMessage(OcctKeyInputKind kind, IntPtr wParam, IntPtr lParam)
    {
        var virtualKey = unchecked((int)wParam.ToInt64());
        var input = new OcctKeyInputEventArgs(
            kind,
            MapKey(virtualKey),
            ResolveModifiers(),
            isRepeat: kind == OcctKeyInputKind.Pressed && (lParam.ToInt64() & (1L << 30)) != 0);

        PreviewKeyInput?.Invoke(this, input);
        KeyInput?.Invoke(this, input);
        return input.Handled;
    }

    private static OcctPointerInputEventArgs CreatePointerInput(
        OcctPointerInputKind kind,
        OcctPointerButton button,
        IntPtr wParam,
        int x,
        int y,
        int wheelDelta) => new(
            kind,
            button,
            MapPointerButtons(unchecked((int)wParam.ToInt64())),
            x,
            y,
            wheelDelta,
            ResolveModifiers());

    private static OcctPointerButtons MapPointerButtons(int buttons)
    {
        var result = OcctPointerButtons.None;
        if ((buttons & MkLButton) != 0) result |= OcctPointerButtons.Left;
        if ((buttons & MkMButton) != 0) result |= OcctPointerButtons.Middle;
        if ((buttons & MkRButton) != 0) result |= OcctPointerButtons.Right;
        return result;
    }

    private static OcctInputModifiers ResolveModifiers()
    {
        var result = OcctInputModifiers.None;
        if (IsKeyDown(VkShift)) result |= OcctInputModifiers.Shift;
        if (IsKeyDown(VkControl)) result |= OcctInputModifiers.Control;
        if (IsKeyDown(VkMenu)) result |= OcctInputModifiers.Alt;
        if (IsKeyDown(VkLWin) || IsKeyDown(VkRWin)) result |= OcctInputModifiers.Meta;
        return result;
    }

    private static OcctKey MapKey(int virtualKey)
    {
        if (virtualKey >= 'A' && virtualKey <= 'Z')
            return (OcctKey)((int)OcctKey.A + virtualKey - 'A');
        if (virtualKey >= '0' && virtualKey <= '9')
            return (OcctKey)((int)OcctKey.D0 + virtualKey - '0');
        if (virtualKey >= VkF1 && virtualKey <= VkF12)
            return (OcctKey)((int)OcctKey.F1 + virtualKey - VkF1);

        return virtualKey switch
        {
            VkEscape => OcctKey.Escape,
            VkReturn => OcctKey.Enter,
            VkTab => OcctKey.Tab,
            VkBack => OcctKey.Backspace,
            VkSpace => OcctKey.Space,
            VkDelete => OcctKey.Delete,
            VkInsert => OcctKey.Insert,
            VkHome => OcctKey.Home,
            VkEnd => OcctKey.End,
            VkPageUp => OcctKey.PageUp,
            VkPageDown => OcctKey.PageDown,
            VkLeft => OcctKey.Left,
            VkRight => OcctKey.Right,
            VkUp => OcctKey.Up,
            VkDown => OcctKey.Down,
            VkShift => OcctKey.Shift,
            VkControl => OcctKey.Control,
            VkMenu => OcctKey.Alt,
            VkLWin or VkRWin => OcctKey.Meta,
            _ => OcctKey.Unknown
        };
    }
}
