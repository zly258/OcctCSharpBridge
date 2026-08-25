#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_curve_continuity_analyze(
        OcctModelingSessionHandle handle,
        OcctObjectId firstEdgeId,
        OcctBool firstAtEnd,
        OcctObjectId secondEdgeId,
        OcctBool secondAtStart,
        const OcctModelContinuityOptions* options,
        OcctModelCurveContinuityResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_surface_continuity_analyze(
        OcctModelingSessionHandle handle,
        OcctObjectId firstFaceId,
        OcctObjectId secondFaceId,
        OcctObjectId sharedEdgeId,
        int sampleCount,
        const OcctModelContinuityOptions* options,
        OcctModelSurfaceContinuityResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_curvature_comb_copy(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        int sampleCount,
        double scale,
        double resolution,
        OcctModelCurvatureCombSample* samples,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_surface_quality_copy(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        const OcctModelSurfaceQualityOptions* options,
        OcctModelSurfaceQualitySample* samples,
        int capacity,
        int* required,
        OcctModelSurfaceQualitySummary* summary);
}
