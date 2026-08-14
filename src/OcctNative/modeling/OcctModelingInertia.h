#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctModelInertiaProperties
    {
        double mass;
        OcctPoint3d centerOfMass;
        double ixx;
        double iyy;
        double izz;
        double ixy;
        double ixz;
        double iyz;
        double principalMoment1;
        double principalMoment2;
        double principalMoment3;
        OcctVector3d principalAxis1;
        OcctVector3d principalAxis2;
        OcctVector3d principalAxis3;
        double radiusOfGyration1;
        double radiusOfGyration2;
        double radiusOfGyration3;
        int hasSymmetryAxis;
        int hasSymmetryPoint;
    };

    OCCTBRIDGE_API int occt_model_shape_linear_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
    OCCTBRIDGE_API int occt_model_shape_surface_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
    OCCTBRIDGE_API int occt_model_shape_volume_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
}
