namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public IReadOnlyList<OcctCurveCurveExtremum> GetEdgeExtrema(
        OcctModelShape firstEdge,
        OcctModelShape secondEdge)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);

        CheckStatus(ModelNativeMethods.occt_model_edge_extrema_snapshot_get(
            _handle, firstEdge.Id, secondEdge.Id, null, 0, out var count));
        if (count == 0) return Array.Empty<OcctCurveCurveExtremum>();

        var native = new NativeModelCurveCurveExtremum[count];
        CheckStatus(ModelNativeMethods.occt_model_edge_extrema_snapshot_get(
            _handle, firstEdge.Id, secondEdge.Id, native, native.Length, out var required));
        if (required != count)
            throw new InvalidOperationException("Native edge-extrema count changed during snapshot copy.");

        var result = new OcctCurveCurveExtremum[count];
        for (var index = 0; index < count; ++index)
            result[index] = native[index].ToManaged();
        return result;
    }

    public IReadOnlyList<OcctCurveSurfaceExtremum> GetEdgeFaceExtrema(
        OcctModelShape edge,
        OcctModelShape face)
    {
        EnsureShape(edge);
        EnsureShape(face);

        CheckStatus(ModelNativeMethods.occt_model_edge_face_extrema_snapshot_get(
            _handle, edge.Id, face.Id, null, 0, out var count));
        if (count == 0) return Array.Empty<OcctCurveSurfaceExtremum>();

        var native = new NativeModelCurveSurfaceExtremum[count];
        CheckStatus(ModelNativeMethods.occt_model_edge_face_extrema_snapshot_get(
            _handle, edge.Id, face.Id, native, native.Length, out var required));
        if (required != count)
            throw new InvalidOperationException("Native edge/face extrema count changed during snapshot copy.");

        var result = new OcctCurveSurfaceExtremum[count];
        for (var index = 0; index < count; ++index)
            result[index] = native[index].ToManaged();
        return result;
    }

    public IReadOnlyList<OcctSurfaceSurfaceExtremum> GetFaceExtrema(
        OcctModelShape firstFace,
        OcctModelShape secondFace)
    {
        EnsureShape(firstFace);
        EnsureShape(secondFace);

        CheckStatus(ModelNativeMethods.occt_model_face_extrema_snapshot_get(
            _handle, firstFace.Id, secondFace.Id, null, 0, out var count));
        if (count == 0) return Array.Empty<OcctSurfaceSurfaceExtremum>();

        var native = new NativeModelSurfaceSurfaceExtremum[count];
        CheckStatus(ModelNativeMethods.occt_model_face_extrema_snapshot_get(
            _handle, firstFace.Id, secondFace.Id, native, native.Length, out var required));
        if (required != count)
            throw new InvalidOperationException("Native face-extrema count changed during snapshot copy.");

        var result = new OcctSurfaceSurfaceExtremum[count];
        for (var index = 0; index < count; ++index)
            result[index] = native[index].ToManaged();
        return result;
    }
}
