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

    OCCTBRIDGE_API OcctObjectId occt_model_shape_free_bounds(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        double tolerance,
        int boundaryKind,
        int splitClosed,
        int splitOpen);

    OCCTBRIDGE_API int occt_model_shape_edge_adjacency(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctModelEdgeAdjacency* items,
        int capacity,
        int* count);
}
