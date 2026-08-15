#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    // Creates an owned shape snapshot independent from the source session's shape registry.
    OCCTBRIDGE_API OcctStatus occt_model_shape_acquire(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        OcctShapeHandle* result);

    // Releases a shape snapshot created by occt_model_shape_acquire.
    OCCTBRIDGE_API void occt_shape_release(OcctShapeHandle handle);

    OCCTBRIDGE_API OcctStatus occt_shape_get_type(
        OcctShapeHandle handle,
        int* result);
}
