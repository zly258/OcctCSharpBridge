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

    OCCTBRIDGE_API int occt_model_intersect_edges(
        OcctModelHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double tolerance);

    OCCTBRIDGE_API int occt_model_edge_intersections_copy(
        OcctModelHandle handle,
        OcctModelEdgeIntersection* results,
        int capacity);
}
