using System.Diagnostics;

namespace OcctNet;

/// <summary>
/// Host-neutral interaction decisions shared by reusable OCCT viewport adapters.
/// Window lifetime, input capture and framework event routing remain host-specific.
/// </summary>
internal static class OcctViewportInteractionPolicy
{
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

    internal static double ZoomFactor(int delta) => delta > 0 ? 1.15 : 0.87;
}
