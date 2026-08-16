using System.Drawing;
using System.Windows.Forms;

namespace OcctNet;

public sealed partial class OcctViewportControl
{
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
        _pressedKeys.Clear();
        _rotating = false;
        _panning = false;
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
}
