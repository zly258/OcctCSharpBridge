#pragma once

#include "OcctModeling.h"

extern "C"
{
    enum OcctModelFreeBoundaryKind
    {
        OcctModelFreeBoundary_Closed = 0,
        OcctModelFreeBoundary_Open = 1
    };

    OCCTBRIDGE_API OcctObjectId occt_model_shape_free_bounds(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        double tolerance,
        int boundaryKind,
        int splitClosed,
        int splitOpen);
}
