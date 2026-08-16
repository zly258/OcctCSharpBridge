using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
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
                case WmKillFocus:
                case WmCancelMode:
                    CancelInteraction();
                    break;
                case WmEraseBkgnd:
                    if (_engine?.IsInitialized == true) return new IntPtr(1);
                    break;
                case WmLButtonDown:
                    HandleWindowsPointerPressed(hwnd, wParam, lParam, OcctPointerButton.Left);
                    break;
                case WmLButtonUp:
                    HandleWindowsPointerReleased(wParam, lParam, OcctPointerButton.Left);
                    break;
                case WmRButtonDown:
                    HandleWindowsPointerPressed(hwnd, wParam, lParam, OcctPointerButton.Right);
                    break;
                case WmRButtonUp:
                    HandleWindowsPointerReleased(wParam, lParam, OcctPointerButton.Right);
                    break;
                case WmMButtonDown:
                    HandleWindowsPointerPressed(hwnd, wParam, lParam, OcctPointerButton.Middle);
                    break;
                case WmMButtonUp:
                    HandleWindowsPointerReleased(wParam, lParam, OcctPointerButton.Middle);
                    break;
                case WmMouseMove:
                    HandleWindowsPointerMoved(wParam, lParam);
                    break;
                case WmMouseWheel:
                    HandleWindowsPointerWheel(wParam, lParam);
                    break;
                case WmKeyDown:
                case WmSysKeyDown:
                    if (HandleWindowsKey(OcctKeyInputKind.Pressed, wParam, lParam)) return IntPtr.Zero;
                    break;
                case WmKeyUp:
                case WmSysKeyUp:
                    if (HandleWindowsKey(OcctKeyInputKind.Released, wParam, lParam)) return IntPtr.Zero;
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

    private void HandleWindowsPointerPressed(IntPtr hwnd, IntPtr wParam, IntPtr lParam, OcctPointerButton button)
    {
        SetFocus(hwnd);
        Focus();
        var point = GetPoint(lParam);
        _lastMouseX = point.X;
        _lastMouseY = point.Y;

        var input = CreateWindowsPointerInput(OcctPointerInputKind.Pressed, button, wParam, point.X, point.Y, 0);
        PreviewPointerInput?.Invoke(this, input);
        if (!input.Handled)
            ProcessWindowsDefaultPointerPressed(hwnd, button, point.X, point.Y);
        PointerInput?.Invoke(this, input);
    }

    private void HandleWindowsPointerReleased(IntPtr wParam, IntPtr lParam, OcctPointerButton button)
    {
        var point = GetPoint(lParam);
        var input = CreateWindowsPointerInput(OcctPointerInputKind.Released, button, wParam, point.X, point.Y, 0);
        PreviewPointerInput?.Invoke(this, input);
        if (!input.Handled)
            ProcessWindowsDefaultPointerReleased(button, point.X, point.Y);
        else
            CancelHandledWindowsRelease(button);
        PointerInput?.Invoke(this, input);
    }

    private void HandleWindowsPointerMoved(IntPtr wParam, IntPtr lParam)
    {
        var point = GetPoint(lParam);
        var input = CreateWindowsPointerInput(OcctPointerInputKind.Moved, OcctPointerButton.None, wParam, point.X, point.Y, 0);
        PreviewPointerInput?.Invoke(this, input);
        if (!input.Handled)
            ProcessWindowsDefaultPointerMoved(wParam, point.X, point.Y);
        else
        {
            _lastMouseX = point.X;
            _lastMouseY = point.Y;
        }
        PointerInput?.Invoke(this, input);
    }

    private void HandleWindowsPointerWheel(IntPtr wParam, IntPtr lParam)
    {
        var delta = GetHighWordSigned(wParam);
        var point = new NativePoint(GetLowWordSigned(lParam), GetHighWordSigned(lParam));
        if (_nativeHandle != IntPtr.Zero) ScreenToClient(_nativeHandle, ref point);

        var input = CreateWindowsPointerInput(
            OcctPointerInputKind.Wheel,
            OcctPointerButton.None,
            wParam,
            point.X,
            point.Y,
            delta);
        PreviewPointerInput?.Invoke(this, input);

        if (!input.Handled
            && delta != 0
            && HasInteractionFeature(OcctViewportInteractionFeatures.Zoom)
            && _engine?.IsInitialized == true)
        {
            var scaledDelta = OcctViewportInteractionPolicy.ScaleWheelDelta(delta, ZoomSensitivity);
            TryInvoke(() => _engine.ZoomAtPoint(point.X, point.Y, scaledDelta));
        }

        PointerInput?.Invoke(this, input);
    }

    private void ProcessWindowsDefaultPointerPressed(IntPtr hwnd, OcctPointerButton button, int x, int y)
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

        BeginSelection(x, y);
        if (_selectingRectangle) SetCapture(hwnd);
    }

    private void ProcessWindowsDefaultPointerReleased(OcctPointerButton button, int x, int y)
    {
        if (button == OcctPointerButton.Right)
        {
            _rotating = false;
            ReleaseNativeCapture();
            return;
        }

        if (button == OcctPointerButton.Middle)
        {
            _panning = false;
            ReleaseNativeCapture();
            return;
        }

        if (button == OcctPointerButton.Left)
            CompleteSelection(x, y, IsKeyDown(VkControl));
    }

    private void ProcessWindowsDefaultPointerMoved(IntPtr wParam, int x, int y)
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
            UpdateHoverAndWorldPoint(x, y);
        }

        _lastMouseX = x;
        _lastMouseY = y;
    }

    private void CancelHandledWindowsRelease(OcctPointerButton button)
    {
        switch (button)
        {
            case OcctPointerButton.Right:
                _rotating = false;
                ReleaseNativeCapture();
                break;
            case OcctPointerButton.Middle:
                _panning = false;
                ReleaseNativeCapture();
                break;
            case OcctPointerButton.Left when _leftSelectionGesture:
                CancelRectangleSelection();
                break;
        }
    }

    private bool HandleWindowsKey(OcctKeyInputKind kind, IntPtr wParam, IntPtr lParam)
    {
        var virtualKey = unchecked((int)wParam.ToInt64());
        var input = new OcctKeyInputEventArgs(
            kind,
            MapWindowsKey(virtualKey),
            ResolveWindowsModifiers(),
            isRepeat: kind == OcctKeyInputKind.Pressed && (lParam.ToInt64() & (1L << 30)) != 0);

        PreviewKeyInput?.Invoke(this, input);
        KeyInput?.Invoke(this, input);
        return input.Handled;
    }

    private static OcctPointerInputEventArgs CreateWindowsPointerInput(
        OcctPointerInputKind kind,
        OcctPointerButton button,
        IntPtr wParam,
        int x,
        int y,
        int wheelDelta) => new(
            kind,
            button,
            MapWindowsPointerButtons(unchecked((int)wParam.ToInt64())),
            x,
            y,
            wheelDelta,
            ResolveWindowsModifiers());

    private static OcctPointerButtons MapWindowsPointerButtons(int buttons)
    {
        var result = OcctPointerButtons.None;
        if ((buttons & MkLButton) != 0) result |= OcctPointerButtons.Left;
        if ((buttons & MkMButton) != 0) result |= OcctPointerButtons.Middle;
        if ((buttons & MkRButton) != 0) result |= OcctPointerButtons.Right;
        return result;
    }

    private static OcctInputModifiers ResolveWindowsModifiers()
    {
        var result = OcctInputModifiers.None;
        if (IsKeyDown(VkShift)) result |= OcctInputModifiers.Shift;
        if (IsKeyDown(VkControl)) result |= OcctInputModifiers.Control;
        if (IsKeyDown(VkMenu)) result |= OcctInputModifiers.Alt;
        if (IsKeyDown(VkLWin) || IsKeyDown(VkRWin)) result |= OcctInputModifiers.Meta;
        return result;
    }

    private static OcctKey MapWindowsKey(int virtualKey)
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
