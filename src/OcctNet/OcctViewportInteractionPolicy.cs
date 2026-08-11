using System.Diagnostics;

namespace OcctNet;

/// <summary>
/// Host-neutral interaction decisions shared by reusable OCCT viewport adapters.
/// Window lifetime, input capture and framework event routing remain host-specific.
/// </summary>
internal static class OcctViewportInteractionPolicy
{
    internal const double MinimumZoomSensitivity = 0.1;
    internal const double MaximumZoomSensitivity = 5.0;
    internal static readonly long HoverIntervalTicks = Math.Max(1, Stopwatch.Frequency / 60);
    internal static readonly long WorldPointIntervalTicks = Math.Max(1, Stopwatch.Frequency / 30);

    internal static bool HasElapsed(long previous, long current, long interval) =>
        previous == 0 || current - previous >= interval;

    internal static (int X, int Y) ResolveSelectionEnd(
        int startX,
        int startY,
        int eventX,
        int eventY,
        int trackedX,
        int trackedY,
        bool rectangleDragStarted)
    {
        if (!rectangleDragStarted)
            return (eventX, eventY);

        var eventDistance = Math.Max(Math.Abs(eventX - startX), Math.Abs(eventY - startY));
        var trackedDistance = Math.Max(Math.Abs(trackedX - startX), Math.Abs(trackedY - startY));
        return trackedDistance > eventDistance ? (trackedX, trackedY) : (eventX, eventY);
    }

    internal static bool ShouldUseRectangle(
        bool enabled,
        bool rectangleDragStarted,
        int threshold,
        int startX,
        int startY,
        int endX,
        int endY)
    {
        if (!enabled)
            return false;
        if (rectangleDragStarted)
            return true;

        var effectiveThreshold = Math.Max(0, threshold);
        var dragDistance = Math.Abs(endX - startX) + Math.Abs(endY - startY);
        return dragDistance > effectiveThreshold;
    }

    internal static bool AllowsOverlap(OcctRectangleSelectionBehavior behavior, int startX, int endX) =>
        behavior switch
        {
            OcctRectangleSelectionBehavior.Overlap => true,
            OcctRectangleSelectionBehavior.Directional => endX < startX,
            _ => false
        };

    internal static double NormalizeZoomSensitivity(double value)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Zoom sensitivity must be a finite number.");
        return Math.Clamp(value, MinimumZoomSensitivity, MaximumZoomSensitivity);
    }

    internal static double ZoomFactor(int delta, double sensitivity = 1.0)
    {
        if (delta == 0) return 1.0;
        var normalizedSensitivity = NormalizeZoomSensitivity(sensitivity);
        var wheelSteps = delta / 120.0;
        return Math.Pow(1.15, wheelSteps * normalizedSensitivity);
    }

    internal static int ScaleWheelDelta(int delta, double sensitivity)
    {
        if (delta == 0) return 0;
        var normalizedSensitivity = NormalizeZoomSensitivity(sensitivity);
        var scaled = (int)Math.Round(delta * normalizedSensitivity, MidpointRounding.AwayFromZero);
        return scaled == 0 ? Math.Sign(delta) : scaled;
    }
}
