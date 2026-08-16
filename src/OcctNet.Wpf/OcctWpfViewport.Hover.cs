namespace OcctNet;

public sealed partial class OcctWpfViewport
{
    private readonly OcctViewportHoverTracker _hoverTracker = new();

    public event EventHandler<OcctViewportHoverHitChangedEventArgs>? HoverHitChanged;

    private void UpdateHoverHit(int screenX, int screenY)
    {
        if (_engine?.IsInitialized != true || HoverHitChanged is null) return;

        OcctSelectionHitDetail? hit = _engine.TryGetDetectedHitDetail(out var detected)
            ? detected
            : null;
        if (!_hoverTracker.Update(hit)) return;

        HoverHitChanged.Invoke(this, new OcctViewportHoverHitChangedEventArgs(screenX, screenY, hit));
    }

    private void ClearHoverHit(int screenX, int screenY)
    {
        if (!_hoverTracker.Clear() || HoverHitChanged is null) return;
        TryInvoke(() => HoverHitChanged?.Invoke(
            this,
            new OcctViewportHoverHitChangedEventArgs(screenX, screenY, null)));
    }

    private void ResetHoverTracking() => _hoverTracker.Clear();
}
