using System.Runtime.InteropServices;

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

        var ownerBuffer = IntPtr.Zero;
        try
        {
            if (ownerIds.Length > 0)
            {
                ownerBuffer = Marshal.AllocHGlobal(sizeof(long) * ownerIds.Length);
                Marshal.Copy(ownerIds, 0, ownerBuffer, ownerIds.Length);
            }

            var options = new NativeViewerDetectionOptions
            {
                StructSize = (uint)Marshal.SizeOf<NativeViewerDetectionOptions>(),
                ApiVersion = 1,
                X = x,
                Y = y,
                MaxHits = maxHits,
                OwnerIds = ownerBuffer,
                OwnerCount = ownerIds.Length,
                ObjectKindMask = objectKindMask,
                ShapeTypeMask = shapeTypeMask,
                IncludeWholeObjects = filter.IncludeWholeObjects ? 1 : 0
            };
            CheckSelectionStatus(DetectionNativeMethods.occt_engine_selection_detect_filtered(
                _handle,
                in options,
                native,
                native.Length,
                out var count));
            if (count < 0 || count > native.Length)
                throw new InvalidOperationException("Native filtered detection result count is invalid.");

            var result = new OcctSelectionHitDetail[count];
            for (var index = 0; index < count; index++)
                result[index] = CreateSelectionHitDetail(native[index]);
            return result;
        }
        finally
        {
            if (ownerBuffer != IntPtr.Zero) Marshal.FreeHGlobal(ownerBuffer);
        }
    }

    /// <summary>
    /// Queries whole viewer objects intersecting or contained by a screen-space rectangle without
    /// mutating the native selection set or highlight state.
    /// </summary>
    public IReadOnlyList<IOcctObject> QueryRectangle(
        int x1,
        int y1,
        int x2,
        int y2,
        bool allowOverlap = false)
    {
        EnsureInitialized();
        var empty = Array.Empty<long>();
        CheckSelectionStatus(DetectionNativeMethods.occt_engine_selection_rectangle_query(
            _handle,
            x1,
            y1,
            x2,
            y2,
            allowOverlap ? 1 : 0,
            empty,
            0,
            out var requiredCount));
        if (requiredCount < 0)
            throw new InvalidOperationException("Native rectangle-query result count is invalid.");
        if (requiredCount == 0) return Array.Empty<IOcctObject>();

        var objectIds = new long[requiredCount];
        CheckSelectionStatus(DetectionNativeMethods.occt_engine_selection_rectangle_query(
            _handle,
            x1,
            y1,
            x2,
            y2,
            allowOverlap ? 1 : 0,
            objectIds,
            objectIds.Length,
            out var filledCount));
        if (filledCount < 0 || filledCount > objectIds.Length)
            throw new InvalidOperationException("Native rectangle-query filled count is invalid.");

        var result = new IOcctObject[filledCount];
        for (var index = 0; index < filledCount; index++)
            result[index] = GetObject(objectIds[index]);
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
