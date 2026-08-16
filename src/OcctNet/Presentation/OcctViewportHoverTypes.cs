namespace OcctNet;

public sealed class OcctViewportHoverHitChangedEventArgs : EventArgs
{
    public OcctViewportHoverHitChangedEventArgs(int screenX, int screenY, OcctSelectionHitDetail? hit)
    {
        ScreenX = screenX;
        ScreenY = screenY;
        Hit = hit;
    }

    public int ScreenX { get; }
    public int ScreenY { get; }
    public OcctSelectionHitDetail? Hit { get; }
}

internal sealed class OcctViewportHoverTracker
{
    private bool _hasHit;
    private long _ownerId;
    private OcctShapeType _subshapeType;
    private int _subshapeIndex;

    internal bool Update(OcctSelectionHitDetail? hit)
    {
        if (hit is null)
            return Clear();

        var value = hit.Value;
        var changed = !_hasHit
            || _ownerId != value.Owner.Id
            || _subshapeType != value.SubshapeType
            || _subshapeIndex != value.SubshapeIndex;

        _hasHit = true;
        _ownerId = value.Owner.Id;
        _subshapeType = value.SubshapeType;
        _subshapeIndex = value.SubshapeIndex;
        return changed;
    }

    internal bool Clear()
    {
        if (!_hasHit) return false;
        _hasHit = false;
        _ownerId = 0;
        _subshapeType = default;
        _subshapeIndex = -1;
        return true;
    }
}
