#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_transform_translate(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctVector3d vector,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_transform_rotate(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_transform_scale(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d center,
        double factor,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_transform_affine(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctTransform3d transform,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_transform_mirror_plane(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        OcctObjectId* result);
}
