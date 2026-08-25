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

    OCCTBRIDGE_API OcctStatus occt_model_boolean_general_fuse_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* shapeIds,
        int shapeCount,
        const OcctModelBooleanOptions* options,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_boolean_cells_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* argumentIds,
        int argumentCount,
        const OcctObjectId* takeIds,
        int takeCount,
        const OcctObjectId* avoidIds,
        int avoidCount,
        int material,
        OcctBool removeInternalBoundaries,
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
