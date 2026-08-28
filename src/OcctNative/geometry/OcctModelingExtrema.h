#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    struct OcctModelCurveCurveExtremum
    {
        OcctPoint3d pointOnFirst;
        OcctPoint3d pointOnSecond;
        double distance;
        double firstParameter;
        double secondParameter;
    };

    struct OcctModelCurveSurfaceExtremum
    {
        OcctPoint3d pointOnCurve;
        OcctPoint3d pointOnSurface;
        double distance;
        double curveParameter;
        double u;
        double v;
    };

    struct OcctModelSurfaceSurfaceExtremum
    {
        OcctPoint3d pointOnFirst;
        OcctPoint3d pointOnSecond;
        double distance;
        double firstU;
        double firstV;
        double secondU;
        double secondV;
    };

    OCCTBRIDGE_API OcctStatus occt_model_edge_extrema_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        OcctModelCurveCurveExtremum* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_edge_face_extrema_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctObjectId faceId,
        OcctModelCurveSurfaceExtremum* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_face_extrema_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId firstFaceId,
        OcctObjectId secondFaceId,
        OcctModelSurfaceSurfaceExtremum* results,
        int capacity,
        int* required);
}
