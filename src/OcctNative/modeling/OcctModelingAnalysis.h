#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API int occt_model_project_point_on_edge(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d pointValue,
        OcctModelProjectionResult* result);

    OCCTBRIDGE_API int occt_model_project_point_on_face(
        OcctModelHandle handle,
        OcctObjectId faceId,
        OcctPoint3d pointValue,
        OcctModelProjectionResult* result);

    OCCTBRIDGE_API int occt_model_ray_intersections(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d origin,
        OcctVector3d directionValue,
        double minimumParameter,
        double maximumParameter,
        double tolerance);

    OCCTBRIDGE_API int occt_model_ray_hits_copy(
        OcctModelHandle handle,
        OcctModelRayHit* results,
        int capacity);

    OCCTBRIDGE_API int occt_model_classify_point(
        OcctModelHandle handle,
        OcctObjectId solidId,
        OcctPoint3d pointValue,
        double tolerance);
}
