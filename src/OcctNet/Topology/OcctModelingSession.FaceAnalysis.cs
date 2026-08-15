namespace OcctNet;

public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Analyzes all faces in one native batch call and returns a stable managed snapshot.
    /// </summary>
    public OcctFaceAnalysisResult AnalyzeFaces(OcctModelShape root)
    {
        EnsureShape(root);
        CheckStatus(ModelNativeMethods.occt_model_shape_face_analysis_snapshot_get(
            _handle,
            root.Id,
            null,
            0,
            out var faceCount));
        if (faceCount == 0)
            return new OcctFaceAnalysisResult(root, Array.Empty<OcctFaceAnalysisInfo>());

        var nativeItems = new NativeModelFaceAnalysis[faceCount];
        CheckStatus(ModelNativeMethods.occt_model_shape_face_analysis_snapshot_get(
            _handle,
            root.Id,
            nativeItems,
            nativeItems.Length,
            out var required));
        if (required != faceCount)
            throw new InvalidOperationException($"Native face analysis count changed during analysis: expected {faceCount}, returned {required}.");

        var items = new OcctFaceAnalysisInfo[faceCount];
        for (var index = 0; index < nativeItems.Length; index++)
        {
            var native = nativeItems[index];
            if (native.FaceId <= 0 ||
                native.EdgeCount < 0 ||
                native.WireCount < 0 ||
                !double.IsFinite(native.Area) || native.Area < 0 ||
                !double.IsFinite(native.MaximumTolerance) || native.MaximumTolerance < 0 ||
                !native.UvBounds.IsValid() ||
                !native.Bounds.IsValid())
            {
                throw new InvalidOperationException("Native face analysis data is invalid.");
            }

            var surfaceType = (OcctSurfaceType)native.SurfaceType;
            if (!Enum.IsDefined(surfaceType))
                surfaceType = OcctSurfaceType.Other;
            var orientation = (OcctModelOrientation)native.Orientation;
            if (!Enum.IsDefined(orientation))
                throw new InvalidOperationException("Native face orientation is invalid.");

            items[index] = new OcctFaceAnalysisInfo(
                new OcctModelShape(native.FaceId, _ownerId),
                surfaceType,
                orientation,
                native.Area,
                native.MaximumTolerance,
                native.UvBounds,
                native.Bounds,
                native.EdgeCount,
                native.WireCount);
        }

        return new OcctFaceAnalysisResult(root, items);
    }
}
