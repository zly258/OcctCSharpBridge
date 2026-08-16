using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace OcctNet;

public sealed partial class OcctWpfViewport
{
    private void UpdateSelectionFrame(int x, int y)
    {
        if (_engine?.IsInitialized != true) return;

        _selectionCurrentX = x;
        _selectionCurrentY = y;
        var threshold = Math.Max(0, RectangleSelectionThreshold);
        var dx = Math.Abs(x - _selectionStartX);
        var dy = Math.Abs(y - _selectionStartY);
        if (dx < threshold && dy < threshold)
        {
            HideSelectionFrame();
            return;
        }

        _rectangleDragStarted = true;
        var frame = new SelectionFrame(
            Math.Min(_selectionStartX, x),
            Math.Min(_selectionStartY, y),
            Math.Max(_selectionStartX, x),
            Math.Max(_selectionStartY, y));
        if (_selectionFrame == frame) return;

        TryInvoke(() => _engine.ShowSelectionRectangle(
            frame.Left,
            frame.Top,
            frame.Right,
            frame.Bottom,
            ToDrawingColor(RectangleSelectionLineColor),
            ToDrawingColor(RectangleSelectionFillColor),
            RectangleSelectionFillTransparency,
            RectangleSelectionLineWidth));
        _selectionFrame = frame;
    }

    private void HideSelectionFrame()
    {
        if (_selectionFrame is null) return;
        if (_engine?.IsInitialized == true)
            TryInvoke(_engine.HideSelectionRectangle);
        _selectionFrame = null;
    }

    private void CancelRectangleSelection(bool releaseCapture = true)
    {
        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        _selectionCurrentX = 0;
        _selectionCurrentY = 0;
        HideSelectionFrame();
        if (releaseCapture) ReleaseCapture();
    }

    private void CancelInteraction()
    {
        _rotating = false;
        _panning = false;
        CancelRectangleSelection();
    }

    private static DrawingColor ToDrawingColor(MediaColor value) =>
        DrawingColor.FromArgb(value.A, value.R, value.G, value.B);

    private readonly record struct SelectionFrame(int Left, int Top, int Right, int Bottom);
}
