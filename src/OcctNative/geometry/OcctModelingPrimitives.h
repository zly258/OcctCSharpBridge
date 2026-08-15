#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_primitive_box_create(OcctModelingSessionHandle, double, double, double, double, double, double, OcctObjectId*);
    OCCTBRIDGE_API OcctStatus occt_model_primitive_cylinder_create(OcctModelingSessionHandle, OcctPoint3d, OcctVector3d, double, double, OcctObjectId*);
    OCCTBRIDGE_API OcctStatus occt_model_primitive_cone_create(OcctModelingSessionHandle, OcctPoint3d, OcctVector3d, double, double, double, OcctObjectId*);
    OCCTBRIDGE_API OcctStatus occt_model_primitive_sphere_create(OcctModelingSessionHandle, OcctPoint3d, double, OcctObjectId*);
    OCCTBRIDGE_API OcctStatus occt_model_primitive_torus_create(OcctModelingSessionHandle, OcctPoint3d, OcctVector3d, double, double, OcctObjectId*);
    OCCTBRIDGE_API OcctStatus occt_model_primitive_wedge_create(OcctModelingSessionHandle, double, double, double, double, OcctObjectId*);
}
