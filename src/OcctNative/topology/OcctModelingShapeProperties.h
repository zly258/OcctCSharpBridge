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

    OCCTBRIDGE_API int occt_model_shape_is_same(
        OcctModelHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId);

    OCCTBRIDGE_API int occt_model_shape_is_partner(
        OcctModelHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId);

    OCCTBRIDGE_API int occt_model_shape_oriented_bounds(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        int optimal,
        OcctOrientedBounds* result);
}
