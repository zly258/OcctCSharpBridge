#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API int occt_model_subshapes_copy(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        int shapeType,
        OcctObjectId* results,
        int capacity);

    OCCTBRIDGE_API OcctObjectId occt_model_outer_wire(OcctModelHandle handle, OcctObjectId faceId);

    OCCTBRIDGE_API int occt_model_inner_wires_copy(
        OcctModelHandle handle,
        OcctObjectId faceId,
        OcctObjectId* results,
        int capacity);

    OCCTBRIDGE_API int occt_model_ancestors_copy(
        OcctModelHandle handle,
        OcctObjectId rootId,
        OcctObjectId childId,
        int ancestorType,
        OcctObjectId* results,
        int capacity);

    OCCTBRIDGE_API OcctObjectId occt_model_sew(
        OcctModelHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        double tolerance);
}
