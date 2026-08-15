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

    OCCTBRIDGE_API OcctStatus occt_model_edge_bspline_pole_at(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        int index,
        OcctPoint3d* pole,
        double* weight);

    OCCTBRIDGE_API OcctStatus occt_model_edge_bspline_knot_at(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        int index,
        double* knot,
        int* multiplicity);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_info(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelBSplineSurfaceInfo* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_pole_at(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        int uIndex,
        int vIndex,
        OcctPoint3d* pole,
        double* weight);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_u_knot_at(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        int index,
        double* knot,
        int* multiplicity);

    OCCTBRIDGE_API OcctStatus occt_model_face_bspline_v_knot_at(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        int index,
        double* knot,
        int* multiplicity);
}
