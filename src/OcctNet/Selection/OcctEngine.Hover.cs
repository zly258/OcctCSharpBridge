namespace OcctNet;

public sealed partial class OcctEngine
{
    private readonly OcctViewportHoverTracker _detectedHoverTracker = new();

    internal event EventHandler<OcctViewportHoverHitChangedEventArgs>? DetectedHitChanged;

    private void UpdateDetectedHit(int screenX, int screenY)
    {
        if (DetectedHitChanged is null) return;

        OcctSelectionHitDetail? hit = TryGetDetectedHitDetail(out var detected)
            ? detected
            : null;
        if (!_detectedHoverTracker.Update(hit)) return;

        DetectedHitChanged.Invoke(
            this,
            new OcctViewportHoverHitChangedEventArgs(screenX, screenY, hit));
    }
}
