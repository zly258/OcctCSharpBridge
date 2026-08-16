using System.Drawing;
using System.Windows.Forms;

namespace OcctNet;

public sealed partial class OcctViewportControl
{
    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        if (!Capture && !_releasingMouseCapture)
        {
            if (IsActiveRectangleGesture)
            {
                // WindowsFormsHost and first-focus activation can transiently take capture.
                // Recover it asynchronously instead of losing the first drag.
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
}
