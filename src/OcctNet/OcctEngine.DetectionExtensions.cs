namespace OcctNet;

public sealed partial class OcctEngine
{
    public IReadOnlyList<OcctSelectionHitDetail> DetectAt(
        int x,
        int y,
        OcctDetectionFilter filter,
        int maxHits = 16)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (maxHits <= 0 || maxHits > 1024)
            throw new ArgumentOutOfRangeException(nameof(maxHits), "Maximum hit count must be between 1 and 1024.");
        EnsureInitialized();

        var ownerIds = filter.Owners is null
            ? Array.Empty<long>()
            : GetObjectIds(filter.Owners, nameof(filter.Owners));
        var objectKindMask = BuildObjectKindMask(filter.ObjectKinds);
        var shapeTypeMask = BuildShapeTypeMask(filter.ShapeTypes);

        var native = new NativeOcctSelectionHitDetail[maxHits];
        Check(DetectionNativeMethods.occt_detect_at_filtered(
            _handle,
            x,
            y,
            maxHits,
            ownerIds,
            ownerIds.Length,
            objectKindMask,
            shapeTypeMask,
            filter.IncludeWholeObjects ? 1 : 0,
            native,
            native.Length,
            out var count));
        if (count < 0 || count > native.Length)
            throw new InvalidOperationException("Native filtered detection result count is invalid.");

        var result = new OcctSelectionHitDetail[count];
        for (var index = 0; index < count; index++) result[index] = CreateSelectionHitDetail(native[index]);
        return result;
    }

    public bool TryGetDetectionCandidate(
        int x,
        int y,
        OcctDetectionFilter filter,
        int cycleIndex,
        out OcctSelectionHitDetail hit,
        int maxHits = 16)
    {
        var hits = DetectAt(x, y, filter, maxHits);
        if (hits.Count == 0)
        {
            hit = default;
            return false;
        }

        var normalized = cycleIndex % hits.Count;
        if (normalized < 0) normalized += hits.Count;
        hit = hits[normalized];
        return true;
    }

    private static ulong BuildObjectKindMask(IReadOnlyCollection<OcctObjectKind>? kinds)
    {
        if (kinds is null || kinds.Count == 0) return ulong.MaxValue;
        ulong mask = 0;
        foreach (var kind in kinds)
        {
            if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kinds));
            mask |= 1UL << (int)kind;
        }
        return mask;
    }

    private static ulong BuildShapeTypeMask(IReadOnlyCollection<OcctShapeType>? types)
    {
        if (types is null || types.Count == 0) return ulong.MaxValue;
        ulong mask = 0;
        foreach (var type in types)
        {
            if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(types));
            mask |= 1UL << (int)type;
        }
        return mask;
    }
}
