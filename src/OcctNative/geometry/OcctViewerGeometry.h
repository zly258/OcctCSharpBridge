#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerIndexedEdgeQueryMask : std::uint32_t
    {
        OcctViewerIndexedEdgeQuery_Endpoints = 1u << 0,
        OcctViewerIndexedEdgeQuery_Evaluation = 1u << 1
    };

    struct OcctViewerIndexedEdgeQueryOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t queryMask;
        int edgeIndex;
        double normalizedParameter;
    };

    struct OcctViewerIndexedEdgeQueryResult
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        OcctPoint3d start;
        OcctPoint3d end;
        OcctPoint3d point;
        OcctVector3d tangent;
    };

    enum OcctViewerIndexedFaceQueryMask : std::uint32_t
    {
        OcctViewerIndexedFaceQuery_Evaluation = 1u << 0,
        OcctViewerIndexedFaceQuery_Center = 1u << 1
    };

    struct OcctViewerIndexedFaceQueryOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t queryMask;
        int faceIndex;
        double u;
        double v;
    };

    struct OcctViewerIndexedFaceQueryResult
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        OcctPoint3d point;
        OcctVector3d normal;
        OcctPoint3d center;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_indexed_vertex_get(
        OcctEngineHandle handle,
        OcctObjectId ownerId,
        int vertexIndex,
        OcctPoint3d* result);

    OCCTBRIDGE_API OcctStatus occt_engine_indexed_edge_query(
        OcctEngineHandle handle,
        OcctObjectId ownerId,
        const OcctViewerIndexedEdgeQueryOptions* options,
        OcctViewerIndexedEdgeQueryResult* result);

    OCCTBRIDGE_API OcctStatus occt_engine_indexed_face_query(
        OcctEngineHandle handle,
        OcctObjectId ownerId,
        const OcctViewerIndexedFaceQueryOptions* options,
        OcctViewerIndexedFaceQueryResult* result);
}
