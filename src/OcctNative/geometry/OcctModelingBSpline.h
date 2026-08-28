#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctModelBSplineCurveInfo
    {
        int degree;
        int poleCount;
        int knotCount;
        int rational;
        int periodic;
    };

    struct OcctModelBSplineSurfaceInfo
    {
        int uDegree;
        int vDegree;
        int uPoleCount;
        int vPoleCount;
        int uKnotCount;
        int vKnotCount;
        int uRational;
        int vRational;
        int uPeriodic;
        int vPeriodic;
    };

    OCCTBRIDGE_API OcctStatus occt_model_edge_bspline_info(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctModelBSplineCurveInfo* result);

    OCCTBRIDGE_API OcctStatus occt_model_edge_bspline_poles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* poles,
        double* weights,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_edge_bspline_knots_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double* knots,
        int* multiplicities,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_info(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelBSplineSurfaceInfo* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_poles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctPoint3d* poles,
        double* weights,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_u_knots_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        double* knots,
        int* multiplicities,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_v_knots_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        double* knots,
        int* multiplicities,
        int capacity,
        int* required);

    struct OcctBSplineCurveDefinition {
        uint32_t structSize;
        uint32_t apiVersion;
        int degree;
        int poleCount;
        int knotCount;    // number of unique knot values
        OcctBool rational;
        OcctBool periodic;
    };

    // Create a BSpline curve edge from explicit control points, weights and knot vector
    OCCTBRIDGE_API OcctStatus occt_model_curve_bspline_explicit_create(
        OcctModelingSessionHandle handle,
        const OcctBSplineCurveDefinition* def,
        const OcctPoint3d* poles,       // [def->poleCount]
        const double* weights,          // [def->poleCount], or NULL for non-rational
        const double* knots,            // [def->knotCount]
        const int* multiplicities,      // [def->knotCount]
        OcctObjectId* result);

    struct OcctBSplineSurfaceDefinition {
        uint32_t structSize;
        uint32_t apiVersion;
        int uDegree;
        int vDegree;
        int uPoleCount;
        int vPoleCount;
        int uKnotCount;
        int vKnotCount;
        OcctBool uRational;
        OcctBool vRational;
        OcctBool uPeriodic;
        OcctBool vPeriodic;
    };

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_explicit_create(
        OcctModelingSessionHandle handle,
        const OcctBSplineSurfaceDefinition* def,
        const OcctPoint3d* poles,
        const double* weights,
        const double* uKnots, const int* uMults,
        const double* vKnots, const int* vMults,
        OcctObjectId* result);
}
