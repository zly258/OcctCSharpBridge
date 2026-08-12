namespace OcctNet;

public sealed partial class OcctEngine
{
    public IReadOnlyList<OcctSelectionHit> GetSelectedHits()
    {
        EnsureInitialized();
        Check(SelectionStateNativeMethods.occt_selected_hits(_handle, null, 0, out var count));
        if (count == 0) return Array.Empty<OcctSelectionHit>();
        if (count < 0)
            throw new InvalidOperationException("Native selection hit count is invalid.");

        var nativeHits = new NativeOcctSelectionHit[count];
        Check(SelectionStateNativeMethods.occt_selected_hits(_handle, nativeHits, nativeHits.Length, out var filledCount));
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
        Check(SelectionStateNativeMethods.occt_detected_hit(_handle, out var native, out var hasHit));
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
}
