using DrawingColor = System.Drawing.Color;

namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
    private void BeginSelection(int x, int y)
    {
        CancelRectangleSelection(releaseCapture: false);
        _selectionStartX = x;
        _selectionStartY = y;
        _selectionCurrentX = x;
        _selectionCurrentY = y;
        _lastMouseX = x;
        _lastMouseY = y;
        _leftSelectionGesture = true;
        _rectangleDragStarted = false;
        _selectingRectangle = HasInteractionFeature(OcctViewportInteractionFeatures.RectangleSelection);
    }

    private void CompleteSelection(int eventX, int eventY, bool append)
    {
        if (!_leftSelectionGesture) return;

        var end = OcctViewportInteractionPolicy.ResolveSelectionEnd(
            _selectionStartX,
            _selectionStartY,
            eventX,
            eventY,
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
        var allowOverlap = OcctViewportInteractionPolicy.AllowsOverlap(
            RectangleSelectionBehavior,
            _selectionStartX,
            end.X);

        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        if (OperatingSystem.IsWindows()) ReleaseNativeCapture();

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

    private void UpdateHoverAndWorldPoint(int x, int y)
    {
        if (_engine?.IsInitialized != true) return;
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
            TryInvoke(() => WorldPointChanged.Invoke(
                this,
                new OcctAvaloniaWorldPointEventArgs(x, y, _engine.ScreenToWorld(x, y))));
        }
    }

    private void UpdateSelectionFrame(int currentX, int currentY)
    {
        if (_engine?.IsInitialized != true) return;
        _selectionCurrentX = currentX;
        _selectionCurrentY = currentY;
        var threshold = Math.Max(0, RectangleSelectionThreshold);
        if (Math.Abs(currentX - _selectionStartX) < threshold
            && Math.Abs(currentY - _selectionStartY) < threshold)
        {
            HideSelectionFrame();
            return;
        }

        _rectangleDragStarted = true;
        var frame = new SelectionFrame(
            Math.Min(_selectionStartX, currentX),
            Math.Min(_selectionStartY, currentY),
            Math.Max(_selectionStartX, currentX),
            Math.Max(_selectionStartY, currentY));
        if (_selectionFrame == frame) return;

        TryInvoke(() => _engine.ShowSelectionRectangle(
            frame.Left,
            frame.Top,
            frame.Right,
            frame.Bottom,
            ToDrawingColor(RectangleSelectionLineColor),
            ToDrawingColor(RectangleSelectionFillColor),
            Math.Clamp(RectangleSelectionFillTransparency, 0.0, 1.0),
            double.IsFinite(RectangleSelectionLineWidth) && RectangleSelectionLineWidth > 0.0
                ? RectangleSelectionLineWidth
                : 1.0));
        _selectionFrame = frame;
    }

    private void HideSelectionFrame()
    {
        if (_selectionFrame is null) return;
        if (_engine?.IsInitialized == true) TryInvoke(_engine.HideSelectionRectangle);
        _selectionFrame = null;
    }

    private void CancelInteraction()
    {
        _rotating = false;
        _panning = false;
        CancelRectangleSelection();
    }

    private void CancelRectangleSelection(bool releaseCapture = true)
    {
        _leftSelectionGesture = false;
        _selectingRectangle = false;
        _rectangleDragStarted = false;
        HideSelectionFrame();
        if (releaseCapture && OperatingSystem.IsWindows()) ReleaseNativeCapture();
    }

    private void ReleaseNativeCapture()
    {
        if (OperatingSystem.IsWindows()) ReleaseCapture();
    }

    private static DrawingColor ToDrawingColor(Avalonia.Media.Color value) =>
        DrawingColor.FromArgb(value.A, value.R, value.G, value.B);

    private readonly record struct SelectionFrame(int Left, int Top, int Right, int Bottom);
}
