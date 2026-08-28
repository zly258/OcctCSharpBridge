#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    enum OcctModelIntersectionKind
    {
        OcctModelIntersection_Point = 0,
        OcctModelIntersection_Overlap = 1
    };

    struct OcctModelEdgeIntersection
    {
        int kind;
        OcctPoint3d startPoint;
        OcctPoint3d endPoint;
        double firstParameterStart;
        double firstParameterEnd;
        double secondParameterStart;
        double secondParameterEnd;
    };

    struct OcctModelEdgeFaceIntersection
    {
        int kind;
        OcctPoint3d startPoint;
        OcctPoint3d endPoint;
        double edgeParameterStart;
        double edgeParameterEnd;
        double uStart;
        double vStart;
        double uEnd;
        double vEnd;
    };

    OCCTBRIDGE_API OcctStatus occt_model_intersect_edges(
        OcctModelingSessionHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double tolerance,
        int* resultCount);

    OCCTBRIDGE_API OcctStatus occt_model_edge_intersections_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctModelEdgeIntersection* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_intersect_edge_face_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctObjectId faceId,
        double tolerance,
        OcctModelEdgeFaceIntersection* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_intersect_surfaces(
        OcctModelingSessionHandle handle,
        OcctObjectId firstFaceId,
        OcctObjectId secondFaceId,
        double tolerance,
        OcctObjectId* result);
}
