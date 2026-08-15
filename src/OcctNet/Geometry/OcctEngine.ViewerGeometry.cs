using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctPoint3d GetVertexPoint(OcctShape owner, int vertexIndex)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(vertexIndex, nameof(vertexIndex));
        EnsureInitialized();
        CheckViewerGeometryStatus(ViewerGeometryNativeMethods.occt_engine_indexed_vertex_get(
            _handle,
            owner.Id,
            vertexIndex,
            out var result));
        if (!result.IsFinite) throw new InvalidOperationException("Native vertex point is not finite.");
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctShape owner, int edgeIndex)
    {
        var result = QueryEdge(owner, new NativeViewerIndexedEdgeQueryOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerIndexedEdgeQueryOptions>(),
            ApiVersion = 1,
            QueryMask = NativeViewerIndexedEdgeQueryMask.Endpoints,
            EdgeIndex = edgeIndex
        });
        if (!result.Start.IsFinite || !result.End.IsFinite)
            throw new InvalidOperationException("Native edge endpoints are not finite.");
        return (result.Start, result.End);
    }

    public OcctEdgeEvaluation EvaluateEdge(OcctShape owner, int edgeIndex, double normalizedParameter)
    {
        OcctGuard.UnitInterval(normalizedParameter, nameof(normalizedParameter));
        var result = QueryEdge(owner, new NativeViewerIndexedEdgeQueryOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerIndexedEdgeQueryOptions>(),
            ApiVersion = 1,
            QueryMask = NativeViewerIndexedEdgeQueryMask.Evaluation,
            EdgeIndex = edgeIndex,
            NormalizedParameter = normalizedParameter
        });
        if (!result.Point.IsFinite || !result.Tangent.IsFinite)
            throw new InvalidOperationException("Native edge evaluation contains non-finite geometry.");
        return new(result.Point, result.Tangent);
    }

    public OcctFaceEvaluation EvaluateFace(OcctShape owner, int faceIndex, double u, double v)
    {
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        var result = QueryFace(owner, new NativeViewerIndexedFaceQueryOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerIndexedFaceQueryOptions>(),
            ApiVersion = 1,
            QueryMask = NativeViewerIndexedFaceQueryMask.Evaluation,
            FaceIndex = faceIndex,
            U = u,
            V = v
        });
        if (!result.Point.IsFinite || !result.Normal.IsFinite)
            throw new InvalidOperationException("Native face evaluation contains non-finite geometry.");
        return new(result.Point, result.Normal);
    }

    public OcctPoint3d GetFaceCenter(OcctShape owner, int faceIndex)
    {
        var result = QueryFace(owner, new NativeViewerIndexedFaceQueryOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerIndexedFaceQueryOptions>(),
            ApiVersion = 1,
            QueryMask = NativeViewerIndexedFaceQueryMask.Center,
            FaceIndex = faceIndex
        });
        if (!result.Center.IsFinite) throw new InvalidOperationException("Native face center is not finite.");
        return result.Center;
    }

    private NativeViewerIndexedEdgeQueryResult QueryEdge(
        OcctShape owner,
        NativeViewerIndexedEdgeQueryOptions options)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(options.EdgeIndex, nameof(options.EdgeIndex));
        EnsureInitialized();
        CheckViewerGeometryStatus(ViewerGeometryNativeMethods.occt_engine_indexed_edge_query(
            _handle,
            owner.Id,
            in options,
            out var result));
        if (result.ApiVersion != 1 ||
            result.StructSize < (uint)Marshal.SizeOf<NativeViewerIndexedEdgeQueryResult>())
        {
            throw new OcctException("Native indexed edge query ABI is incompatible with this SDK.");
        }
        return result;
    }

    private NativeViewerIndexedFaceQueryResult QueryFace(
        OcctShape owner,
        NativeViewerIndexedFaceQueryOptions options)
    {
        EnsureShape(owner);
        OcctGuard.PositiveIndex(options.FaceIndex, nameof(options.FaceIndex));
        EnsureInitialized();
        CheckViewerGeometryStatus(ViewerGeometryNativeMethods.occt_engine_indexed_face_query(
            _handle,
            owner.Id,
            in options,
            out var result));
        if (result.ApiVersion != 1 ||
            result.StructSize < (uint)Marshal.SizeOf<NativeViewerIndexedFaceQueryResult>())
        {
            throw new OcctException("Native indexed face query ABI is incompatible with this SDK.");
        }
        return result;
    }

    private void CheckViewerGeometryStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
