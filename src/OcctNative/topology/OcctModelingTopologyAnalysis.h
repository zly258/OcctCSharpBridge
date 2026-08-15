#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    enum OcctModelFreeBoundaryKind
    {
        OcctModelFreeBoundary_Closed = 0,
        OcctModelFreeBoundary_Open = 1
    };

    struct OcctModelEdgeAdjacency
    {
        OcctObjectId edgeId;
        int adjacentFaceCount;
    };

    OCCTBRIDGE_API OcctStatus occt_model_shape_free_bounds(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double tolerance,
        int boundaryKind,
        OcctBool splitClosed,
        OcctBool splitOpen,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_edge_adjacency_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelEdgeAdjacency* items,
        int capacity,
        int* required);
}
