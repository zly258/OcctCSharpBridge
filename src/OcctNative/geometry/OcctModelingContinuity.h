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
}
