#pragma once

#include "OcctNative.h"

extern "C"
{
    struct OcctViewerObjectTransformUpdate
    {
        OcctObjectId objectId;
        OcctTransform3d transformation;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_object_transform_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctTransform3d* transformation);

    OCCTBRIDGE_API OcctStatus occt_engine_object_transform_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctTransform3d* transformation,
        int* hasTransformation);

    OCCTBRIDGE_API OcctStatus occt_engine_object_transform_reset(
        OcctEngineHandle handle,
        OcctObjectId objectId);

    OCCTBRIDGE_API OcctStatus occt_engine_object_transforms_set(
        OcctEngineHandle handle,
        const OcctViewerObjectTransformUpdate* updates,
        int count);
}
