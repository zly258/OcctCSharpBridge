#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctModelCurveCurveExtremum
    {
        OcctPoint3d pointOnFirst;
        OcctPoint3d pointOnSecond;
        double distance;
        double firstParameter;
        double secondParameter;
    };

    OCCTBRIDGE_API OcctStatus occt_model_edge_extrema_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        OcctModelCurveCurveExtremum* results,
        int capacity,
        int* required);
}
