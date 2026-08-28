#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_surface_plane_face_create(OcctModelingSessionHandle handle, OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection, double uMin, double uMax, double vMin, double vMax, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_model_surface_cylinder_face_create(OcctModelingSessionHandle handle, OcctPoint3d origin, OcctVector3d axis, OcctVector3d xDirection, double radius, double uMin, double uMax, double vMin, double vMax, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_model_surface_cone_face_create(OcctModelingSessionHandle handle, OcctPoint3d referenceOrigin, OcctVector3d axis, OcctVector3d xDirection, double referenceRadius, double semiAngleRadians, double uMin, double uMax, double vMin, double vMax, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_model_surface_sphere_face_create(OcctModelingSessionHandle handle, OcctPoint3d center, OcctVector3d axis, OcctVector3d xDirection, double radius, double uMin, double uMax, double vMin, double vMax, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_model_surface_torus_face_create(OcctModelingSessionHandle handle, OcctPoint3d center, OcctVector3d axis, OcctVector3d xDirection, double majorRadius, double minorRadius, double uMin, double uMax, double vMin, double vMax, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_model_edge_line_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_edge_circle_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_edge_ellipse_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_plane_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_cylinder_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_cone_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_sphere_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
    OCCTBRIDGE_API OcctStatus occt_model_face_torus_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);
}
