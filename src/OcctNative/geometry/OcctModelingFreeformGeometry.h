#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctModelParabolaGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double focalLength;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelHyperbolaGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double majorRadius;
        double minorRadius;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelBezierCurveInfo
    {
        int degree;
        int poleCount;
        int rational;
        int closed;
    };

    struct OcctModelBezierSurfaceInfo
    {
        int uDegree;
        int vDegree;
        int uPoleCount;
        int vPoleCount;
        int uRational;
        int vRational;
    };

    struct OcctModelExtrusionSurfaceGeometry
    {
        OcctVector3d direction;
        OcctCurveType basisCurveType;
    };

    struct OcctModelRevolutionSurfaceGeometry
    {
        OcctPoint3d origin;
        OcctVector3d axis;
        OcctCurveType basisCurveType;
    };

    struct OcctModelOffsetSurfaceGeometry
    {
        double offset;
        OcctSurfaceType basisSurfaceType;
    };

    OCCTBRIDGE_API OcctStatus occt_model_edge_parabola_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctModelParabolaGeometry* result);

    OCCTBRIDGE_API OcctStatus occt_model_edge_hyperbola_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctModelHyperbolaGeometry* result);

    OCCTBRIDGE_API OcctStatus occt_model_edge_bezier_info(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctModelBezierCurveInfo* result);

    OCCTBRIDGE_API OcctStatus occt_model_edge_bezier_poles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* poles,
        double* weights,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_face_bezier_info(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelBezierSurfaceInfo* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_bezier_poles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctPoint3d* poles,
        double* weights,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_face_extrusion_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelExtrusionSurfaceGeometry* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_revolution_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelRevolutionSurfaceGeometry* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_offset_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelOffsetSurfaceGeometry* result);
}
