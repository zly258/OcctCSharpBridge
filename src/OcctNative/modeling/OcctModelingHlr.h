#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctModelHlrResult
    {
        OcctObjectId visibleShapeId;
        OcctObjectId hiddenShapeId;
        OcctObjectId outlineShapeId;
        OcctObjectId visibleSharpShapeId;
        OcctObjectId hiddenSharpShapeId;
    };

    OCCTBRIDGE_API OcctStatus occt_model_hlr_project(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctVector3d viewDirection,
        OcctVector3d upDirection,
        OcctModelHlrResult* result);
}
