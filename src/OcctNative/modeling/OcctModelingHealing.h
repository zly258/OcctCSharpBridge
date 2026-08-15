#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_healing_unify_same_domain_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool unifyEdges,
        OcctBool unifyFaces,
        OcctBool concatBsplines,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_healing_fix_shape_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double precision,
        double minTolerance,
        double maxTolerance,
        OcctModelAlgorithmResult* result);
}
