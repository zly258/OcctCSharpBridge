#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_planar_regular_polygon_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        int sideCount,
        OcctBool makeFace,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_planar_rectangle_wire_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_planar_face_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d origin,
        OcctVector3d xDirection,
        OcctVector3d normal,
        double width,
        double height,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_planar_face_from_wire_create(
        OcctModelingSessionHandle handle,
        OcctObjectId wireId,
        OcctBool onlyPlane,
        OcctObjectId* result);
}
