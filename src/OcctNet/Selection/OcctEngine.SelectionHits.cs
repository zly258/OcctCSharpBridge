namespace OcctNet;

public sealed partial class OcctEngine
{
    public IReadOnlyList<OcctSelectionHit> GetSelectedHits()
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionStateNativeMethods.occt_engine_selection_hits_get(
            _handle,
            null,
            0,
            out var count));
        if (count == 0) return Array.Empty<OcctSelectionHit>();
        if (count < 0) throw new InvalidOperationException("Native selection hit count is invalid.");

        var nativeHits = new NativeOcctSelectionHit[count];
        CheckSelectionStatus(SelectionStateNativeMethods.occt_engine_selection_hits_get(
            _handle,
            nativeHits,
            nativeHits.Length,
            out var filledCount));
        if (filledCount < 0 || filledCount > nativeHits.Length)
            throw new InvalidOperationException("Native selection hit result count is invalid.");

        var result = new OcctSelectionHit[filledCount];
        for (var index = 0; index < filledCount; index++)
            result[index] = CreateSelectionHit(nativeHits[index]);
        return result;
    }

    public bool TryGetDetectedHit(out OcctSelectionHit hit)
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionStateNativeMethods.occt_engine_selection_detected_hit_get(
            _handle,
            out var native,
            out var hasHit));
        if (hasHit == 0)
        {
            hit = default;
            return false;
        }
        if (hasHit != 1)
            throw new InvalidOperationException("Native detected-hit state is invalid.");

        hit = CreateSelectionHit(native);
        return true;
    }

    public bool TryGetDetectedHitDetail(out OcctSelectionHitDetail hit)
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionStateNativeMethods.occt_engine_selection_detected_hit_detail_get(
            _handle,
            out var native,
            out var hasHit));
        if (hasHit == 0)
        {
            hit = default;
            return false;
        }
        if (hasHit != 1)
            throw new InvalidOperationException("Native detected-hit detail state is invalid.");

        hit = CreateSelectionHitDetail(native);
        return true;
    }

    public IReadOnlyList<OcctSelectionHitDetail> DetectAt(int x, int y, int maxHits = 16)
    {
        if (maxHits <= 0 || maxHits > 1024)
            throw new ArgumentOutOfRangeException(nameof(maxHits), "Maximum hit count must be between 1 and 1024.");
        EnsureInitialized();

        var native = new NativeOcctSelectionHitDetail[maxHits];
        CheckSelectionStatus(SelectionStateNativeMethods.occt_engine_selection_detect_at(
            _handle,
            x,
            y,
            maxHits,
            native,
            native.Length,
            out var count));
        if (count < 0 || count > native.Length)
            throw new InvalidOperationException("Native detection result count is invalid.");

        var result = new OcctSelectionHitDetail[count];
        for (var index = 0; index < count; index++)
            result[index] = CreateSelectionHitDetail(native[index]);
        return result;
    }

    private OcctSelectionHit CreateSelectionHit(NativeOcctSelectionHit native)
    {
        if (native.OwnerObjectId <= 0)
            throw new InvalidOperationException("Selection hit does not contain a valid owner object ID.");
        if (!Enum.IsDefined(typeof(OcctShapeType), native.SubshapeType))
            throw new InvalidOperationException($"Selection hit contains unknown subshape type {native.SubshapeType}.");
        if (native.SubshapeIndex < -1)
            throw new InvalidOperationException("Selection hit contains an invalid subshape index.");

        var subshapeType = (OcctShapeType)native.SubshapeType;
        if (native.SubshapeIndex == -1 && subshapeType != OcctShapeType.Shape)
            throw new InvalidOperationException("Whole-object selection hits must use Shape with subshape index -1.");
        if (native.SubshapeIndex >= 0 && subshapeType == OcctShapeType.Shape)
            throw new InvalidOperationException("Subshape selection hits must contain a concrete topology type.");

        var owner = GetObject(native.OwnerObjectId);
        if (native.SubshapeIndex >= 0 && owner.Kind != OcctObjectKind.Shape)
            throw new InvalidOperationException("Subshape selection hits must be owned by a shape object.");

        return new OcctSelectionHit(owner, subshapeType, native.SubshapeIndex);
    }

    private OcctSelectionHitDetail CreateSelectionHitDetail(NativeOcctSelectionHitDetail native)
    {
        var identity = CreateSelectionHit(new NativeOcctSelectionHit
        {
            OwnerObjectId = native.OwnerObjectId,
            SubshapeType = native.SubshapeType,
            SubshapeIndex = native.SubshapeIndex
        });
        if (!native.Point.IsFinite || !double.IsFinite(native.Depth) || !double.IsFinite(native.DistanceToEye))
            throw new InvalidOperationException("Native selection hit detail contains non-finite geometry.");

        return new(
            identity.Owner,
            identity.SubshapeType,
            identity.SubshapeIndex,
            native.Point,
            native.Depth,
            native.DistanceToEye);
    }
}
