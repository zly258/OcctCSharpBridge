#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctOrientedBounds
    {
        OcctPoint3d center;
        OcctVector3d xDirection;
        OcctVector3d yDirection;
        OcctVector3d zDirection;
        double halfSizeX;
        double halfSizeY;
        double halfSizeZ;
    };

    OCCTBRIDGE_API OcctStatus occt_model_shape_is_same(
        OcctModelingSessionHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId,
        OcctBool* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_is_partner(
        OcctModelingSessionHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId,
        OcctBool* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_oriented_bounds(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool optimal,
        OcctOrientedBounds* result);
}
