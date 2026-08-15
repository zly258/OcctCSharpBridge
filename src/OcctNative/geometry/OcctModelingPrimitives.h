#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_make_box(
        OcctModelingSessionHandle handle,
        double x,
        double y,
        double z,
        double dx,
        double dy,
        double dz,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_make_cylinder(
        OcctModelingSessionHandle handle,
        OcctPoint3d origin,
        OcctVector3d axis,
        double radius,
        double height,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_make_cone(
        OcctModelingSessionHandle handle,
        OcctPoint3d origin,
        OcctVector3d axis,
        double radius1,
        double radius2,
        double height,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_make_sphere(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        double radius,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_make_torus(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        OcctVector3d axis,
        double majorRadius,
        double minorRadius,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_make_wedge(
        OcctModelingSessionHandle handle,
        double dx,
        double dy,
        double dz,
        double ltx,
        OcctObjectId* result);
}
