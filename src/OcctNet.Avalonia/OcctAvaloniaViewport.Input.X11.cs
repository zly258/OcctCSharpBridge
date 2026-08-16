using Avalonia.Threading;

namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
    private void InstallX11Input(nuint window)
    {
        if (_x11Display == IntPtr.Zero || window == 0) return;
        var mask = X11KeyPressMask
            | X11KeyReleaseMask
            | X11ButtonPressMask
            | X11ButtonReleaseMask
            | X11PointerMotionMask
            | X11ExposureMask
            | X11StructureNotifyMask;
        XSelectInput(_x11Display, window, (nint)mask);
        XFlush(_x11Display);
    }

    private void StartX11InputPump()
    {
        if (!OperatingSystem.IsLinux() || _x11InputTimer is not null) return;
        _x11InputTimer = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _x11InputTimer.Tick += X11InputTimerTick;
        _x11InputTimer.Start();
    }

    private void StopX11InputPump()
    {
        var timer = _x11InputTimer;
        _x11InputTimer = null;
        if (timer is null) return;
        timer.Tick -= X11InputTimerTick;
        timer.Stop();
    }

    private void X11InputTimerTick(object? sender, EventArgs e)
    {
        if (_x11Display == IntPtr.Zero || _nativeHandle == IntPtr.Zero) return;
        try
        {
            X11MotionEvent? pendingMotion = null;
            var processed = 0;
            while (processed++ < X11MaxEventsPerTick && XPending(_x11Display) > 0)
            {
                XNextEvent(_x11Display, out var nativeEvent);
                if (nativeEvent.Type == X11MotionNotify)
                {
                    pendingMotion = nativeEvent.Motion;
                    continue;
                }

                if (pendingMotion is { } motion)
                {
                    HandleX11Motion(motion);
                    pendingMotion = null;
                }
                ProcessX11Event(nativeEvent);
            }

            if (pendingMotion is { } finalMotion)
                HandleX11Motion(finalMotion);
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
    }

    private void ProcessX11Event(X11Event nativeEvent)
    {
        switch (nativeEvent.Type)
        {
            case X11KeyPress:
                HandleX11Key(OcctKeyInputKind.Pressed, nativeEvent.Key);
                break;
            case X11KeyRelease:
                HandleX11Key(OcctKeyInputKind.Released, nativeEvent.Key);
                break;
            case X11ButtonPress:
                HandleX11ButtonPress(nativeEvent.Button);
                break;
            case X11ButtonRelease:
                HandleX11ButtonRelease(nativeEvent.Button);
                break;
            case X11MotionNotify:
                HandleX11Motion(nativeEvent.Motion);
                break;
            case X11Expose:
            case X11ConfigureNotify:
                ScheduleNativeViewRefresh();
                break;
        }
    }

    private void HandleX11ButtonPress(X11ButtonEvent buttonEvent)
    {
        Focus();
        if (_x11Display != IntPtr.Zero && buttonEvent.Window != 0)
        {
            XSetInputFocus(_x11Display, buttonEvent.Window, X11RevertToParent, X11CurrentTime);
            XFlush(_x11Display);
        }

        if (buttonEvent.Button is X11Button4 or X11Button5)
        {
            var delta = buttonEvent.Button == X11Button4 ? 120 : -120;
            var input = CreateX11PointerInput(
                OcctPointerInputKind.Wheel,
                OcctPointerButton.None,
                buttonEvent.State,
                buttonEvent.X,
                buttonEvent.Y,
                delta);
            PreviewPointerInput?.Invoke(this, input);

            if (!input.Handled
                && HasInteractionFeature(OcctViewportInteractionFeatures.Zoom)
                && _engine?.IsInitialized == true)
            {
                var scaledDelta = OcctViewportInteractionPolicy.ScaleWheelDelta(delta, ZoomSensitivity);
                TryInvoke(() => _engine.ZoomAtPoint(buttonEvent.X, buttonEvent.Y, scaledDelta));
            }

            PointerInput?.Invoke(this, input);
            return;
        }

        var button = MapX11PointerButton(buttonEvent.Button);
        var pressedButtons = MapX11PointerButtons(buttonEvent.State) | ToButtonFlag(button);
        var inputEvent = new OcctPointerInputEventArgs(
            OcctPointerInputKind.Pressed,
            button,
            pressedButtons,
            buttonEvent.X,
            buttonEvent.Y,
            0,
            ResolveX11Modifiers(buttonEvent.State));
        PreviewPointerInput?.Invoke(this, inputEvent);

        if (!inputEvent.Handled)
            ProcessX11DefaultPointerPressed(buttonEvent, button);

        PointerInput?.Invoke(this, inputEvent);
    }

    private void HandleX11ButtonRelease(X11ButtonEvent buttonEvent)
    {
        if (buttonEvent.Button is X11Button4 or X11Button5) return;

        var button = MapX11PointerButton(buttonEvent.Button);
        var releasedButtons = MapX11PointerButtons(buttonEvent.State) & ~ToButtonFlag(button);
        var input = new OcctPointerInputEventArgs(
            OcctPointerInputKind.Released,
            button,
            releasedButtons,
            buttonEvent.X,
            buttonEvent.Y,
            0,
            ResolveX11Modifiers(buttonEvent.State));
        PreviewPointerInput?.Invoke(this, input);

        if (!input.Handled)
            ProcessX11DefaultPointerReleased(buttonEvent, button);
        else
            CancelHandledX11Release(button);

        PointerInput?.Invoke(this, input);
    }

    private void HandleX11Motion(X11MotionEvent motionEvent)
    {
        var input = new OcctPointerInputEventArgs(
            OcctPointerInputKind.Moved,
            OcctPointerButton.None,
            MapX11PointerButtons(motionEvent.State),
            motionEvent.X,
            motionEvent.Y,
            0,
            ResolveX11Modifiers(motionEvent.State));
        PreviewPointerInput?.Invoke(this, input);

        if (!input.Handled)
            ProcessX11DefaultPointerMoved(motionEvent);
        else
        {
            _lastMouseX = motionEvent.X;
            _lastMouseY = motionEvent.Y;
        }

        PointerInput?.Invoke(this, input);
    }

    private void ProcessX11DefaultPointerPressed(X11ButtonEvent buttonEvent, OcctPointerButton button)
    {
        if (_engine?.IsInitialized != true) return;

        if (button == OcctPointerButton.Left
            && (buttonEvent.State & X11ShiftMask) == 0
            && HasInteractionFeature(OcctViewportInteractionFeatures.Selection))
        {
            BeginSelection(buttonEvent.X, buttonEvent.Y);
            return;
        }

        if (button == OcctPointerButton.Middle && HasInteractionFeature(OcctViewportInteractionFeatures.Pan))
        {
            CancelRectangleSelection(releaseCapture: false);
            _lastMouseX = buttonEvent.X;
            _lastMouseY = buttonEvent.Y;
            _panning = true;
            return;
        }

        if (button == OcctPointerButton.Right && HasInteractionFeature(OcctViewportInteractionFeatures.Rotate))
        {
            CancelRectangleSelection(releaseCapture: false);
            _lastMouseX = buttonEvent.X;
            _lastMouseY = buttonEvent.Y;
            _rotating = true;
            TryInvoke(() => _engine.StartRotation(buttonEvent.X, buttonEvent.Y));
        }
    }

    private void ProcessX11DefaultPointerReleased(X11ButtonEvent buttonEvent, OcctPointerButton button)
    {
        switch (button)
        {
            case OcctPointerButton.Left:
                CompleteSelection(
                    buttonEvent.X,
                    buttonEvent.Y,
                    (buttonEvent.State & X11ControlMask) != 0);
                break;
            case OcctPointerButton.Middle:
                _panning = false;
                break;
            case OcctPointerButton.Right:
                _rotating = false;
                break;
        }
    }

    private void ProcessX11DefaultPointerMoved(X11MotionEvent motionEvent)
    {
        if (_engine?.IsInitialized != true)
        {
            _lastMouseX = motionEvent.X;
            _lastMouseY = motionEvent.Y;
            return;
        }

        if (_rotating
            && (motionEvent.State & X11Button3Mask) != 0
            && HasInteractionFeature(OcctViewportInteractionFeatures.Rotate))
        {
            TryInvoke(() => _engine.Rotation(motionEvent.X, motionEvent.Y));
        }
        else if (_panning
                 && (motionEvent.State & X11Button2Mask) != 0
                 && HasInteractionFeature(OcctViewportInteractionFeatures.Pan))
        {
            TryInvoke(() => _engine.Pan(
                motionEvent.X - _lastMouseX,
                -(motionEvent.Y - _lastMouseY)));
        }
        else if (_leftSelectionGesture
                 && _selectingRectangle
                 && (motionEvent.State & X11Button1Mask) != 0)
        {
            UpdateSelectionFrame(motionEvent.X, motionEvent.Y);
        }
        else
        {
            UpdateHoverAndWorldPoint(motionEvent.X, motionEvent.Y);
        }

        _lastMouseX = motionEvent.X;
        _lastMouseY = motionEvent.Y;
    }

    private void CancelHandledX11Release(OcctPointerButton button)
    {
        switch (button)
        {
            case OcctPointerButton.Left when _leftSelectionGesture:
                CancelRectangleSelection(releaseCapture: false);
                break;
            case OcctPointerButton.Middle:
                _panning = false;
                break;
            case OcctPointerButton.Right:
                _rotating = false;
                break;
        }
    }

    private void HandleX11Key(OcctKeyInputKind kind, X11KeyEvent keyEvent)
    {
        var lookup = keyEvent;
        var keySym = XLookupKeysym(ref lookup, 0);
        var isRepeat = false;
        if (kind == OcctKeyInputKind.Pressed)
            isRepeat = !_x11PressedKeys.Add(keyEvent.Keycode);
        else
            _x11PressedKeys.Remove(keyEvent.Keycode);

        var input = new OcctKeyInputEventArgs(
            kind,
            MapX11Key(keySym),
            ResolveX11Modifiers(keyEvent.State),
            isRepeat);
        PreviewKeyInput?.Invoke(this, input);
        KeyInput?.Invoke(this, input);
    }

    private static OcctPointerInputEventArgs CreateX11PointerInput(
        OcctPointerInputKind kind,
        OcctPointerButton button,
        uint state,
        int x,
        int y,
        int wheelDelta) => new(
            kind,
            button,
            MapX11PointerButtons(state),
            x,
            y,
            wheelDelta,
            ResolveX11Modifiers(state));

    private static OcctPointerButton MapX11PointerButton(uint button) => button switch
    {
        X11Button1 => OcctPointerButton.Left,
        X11Button2 => OcctPointerButton.Middle,
        X11Button3 => OcctPointerButton.Right,
        _ => OcctPointerButton.None
    };

    private static OcctPointerButtons ToButtonFlag(OcctPointerButton button) => button switch
    {
        OcctPointerButton.Left => OcctPointerButtons.Left,
        OcctPointerButton.Middle => OcctPointerButtons.Middle,
        OcctPointerButton.Right => OcctPointerButtons.Right,
        OcctPointerButton.X1 => OcctPointerButtons.X1,
        OcctPointerButton.X2 => OcctPointerButtons.X2,
        _ => OcctPointerButtons.None
    };

    private static OcctPointerButtons MapX11PointerButtons(uint state)
    {
        var result = OcctPointerButtons.None;
        if ((state & X11Button1Mask) != 0) result |= OcctPointerButtons.Left;
        if ((state & X11Button2Mask) != 0) result |= OcctPointerButtons.Middle;
        if ((state & X11Button3Mask) != 0) result |= OcctPointerButtons.Right;
        return result;
    }

    private static OcctInputModifiers ResolveX11Modifiers(uint state)
    {
        var result = OcctInputModifiers.None;
        if ((state & X11ShiftMask) != 0) result |= OcctInputModifiers.Shift;
        if ((state & X11ControlMask) != 0) result |= OcctInputModifiers.Control;
        if ((state & X11Mod1Mask) != 0) result |= OcctInputModifiers.Alt;
        if ((state & X11Mod4Mask) != 0) result |= OcctInputModifiers.Meta;
        return result;
    }

    private static OcctKey MapX11Key(nuint keySym)
    {
        if (keySym >= 'a' && keySym <= 'z')
            return (OcctKey)((int)OcctKey.A + (int)(keySym - 'a'));
        if (keySym >= 'A' && keySym <= 'Z')
            return (OcctKey)((int)OcctKey.A + (int)(keySym - 'A'));
        if (keySym >= '0' && keySym <= '9')
            return (OcctKey)((int)OcctKey.D0 + (int)(keySym - '0'));
        if (keySym >= XkF1 && keySym <= XkF12)
            return (OcctKey)((int)OcctKey.F1 + (int)(keySym - XkF1));

        return keySym switch
        {
            XkEscape => OcctKey.Escape,
            XkReturn => OcctKey.Enter,
            XkTab => OcctKey.Tab,
            XkBackSpace => OcctKey.Backspace,
            0x20 => OcctKey.Space,
            XkDelete => OcctKey.Delete,
            XkInsert => OcctKey.Insert,
            XkHome => OcctKey.Home,
            XkEnd => OcctKey.End,
            XkPageUp => OcctKey.PageUp,
            XkPageDown => OcctKey.PageDown,
            XkLeft => OcctKey.Left,
            XkRight => OcctKey.Right,
            XkUp => OcctKey.Up,
            XkDown => OcctKey.Down,
            XkShiftL or XkShiftR => OcctKey.Shift,
            XkControlL or XkControlR => OcctKey.Control,
            XkAltL or XkAltR => OcctKey.Alt,
            XkMetaL or XkMetaR => OcctKey.Meta,
            _ => OcctKey.Unknown
        };
    }
}
