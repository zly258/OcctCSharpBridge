namespace OcctNet;

public sealed partial class OcctEngine
{
    private readonly OcctViewportHoverTracker _detectedHoverTracker = new();
    private EventHandler<OcctViewportHoverHitChangedEventArgs>? _detectedHitChanged;

    internal event EventHandler<OcctViewportHoverHitChangedEventArgs>? DetectedHitChanged
    {
        add
        {
            if (_detectedHitChanged is null) _detectedHoverTracker.Clear();
            _detectedHitChanged += value;
        }
        remove
        {
            _detectedHitChanged -= value;
            if (_detectedHitChanged is null) _detectedHoverTracker.Clear();
        }
    }

    private void UpdateDetectedHit(int screenX, int screenY)
    {
        if (_detectedHitChanged is null) return;

        OcctSelectionHitDetail? hit = TryGetDetectedHitDetail(out var detected)
            ? detected
            : null;
        if (!_detectedHoverTracker.Update(hit)) return;

        _detectedHitChanged.Invoke(
            this,
            new OcctViewportHoverHitChangedEventArgs(screenX, screenY, hit));
    }
}
