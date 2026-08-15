#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API int occt_model_edge_line_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
    OCCTBRIDGE_API int occt_model_edge_circle_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
    OCCTBRIDGE_API int occt_model_edge_ellipse_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
    OCCTBRIDGE_API int occt_model_face_plane_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
    OCCTBRIDGE_API int occt_model_face_cylinder_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
    OCCTBRIDGE_API int occt_model_face_cone_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
    OCCTBRIDGE_API int occt_model_face_sphere_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
    OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);
}
