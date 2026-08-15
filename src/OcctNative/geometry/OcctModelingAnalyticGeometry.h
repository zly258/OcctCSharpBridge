#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_edge_line_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_edge_circle_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_edge_ellipse_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_plane_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_cylinder_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_cone_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_sphere_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_torus_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);
}
