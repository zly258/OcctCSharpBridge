#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_boolean_execute(
        OcctModelingSessionHandle handle,
        int operation,
        OcctObjectId leftId,
        OcctObjectId rightId,
        const OcctModelBooleanOptions* options,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_boolean_split_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* objectIds,
        int objectCount,
        const OcctObjectId* toolIds,
        int toolCount,
        const OcctModelBooleanOptions* options,
        OcctModelAlgorithmResult* result);
}
