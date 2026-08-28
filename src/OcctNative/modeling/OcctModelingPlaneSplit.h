#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctModelPlaneSplitResult
    {
        OcctObjectId positiveShapeId;
        OcctObjectId negativeShapeId;
        OcctObjectId sectionShapeId;
    };

    OCCTBRIDGE_API OcctStatus occt_model_split_by_plane(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d origin,
        OcctVector3d normal,
        OcctModelPlaneSplitResult* result);
}
