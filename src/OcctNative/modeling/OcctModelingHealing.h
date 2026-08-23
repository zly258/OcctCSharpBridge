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

    // Fix vertex and edge tolerances
    OCCTBRIDGE_API OcctStatus occt_model_healing_fix_tolerance_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double tolerance,
        OcctModelAlgorithmResult* result);

    // Fix small gaps between edges and faces
    OCCTBRIDGE_API OcctStatus occt_model_healing_fix_gaps_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double gapTolerance,
        OcctModelAlgorithmResult* result);

    // Remove subshapes by topology index
    OCCTBRIDGE_API OcctStatus occt_model_healing_reshape_remove_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const int* subShapeIndices,
        int count,
        OcctModelAlgorithmResult* result);
}
