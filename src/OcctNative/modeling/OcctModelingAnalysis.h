#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_project_point_on_edge(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d pointValue,
        OcctModelProjectionResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_project_point_on_face(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctPoint3d pointValue,
        OcctModelProjectionResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_project_points_on_edge(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        const OcctPoint3d* points,
        int count,
        OcctModelProjectionResult* results);

    OCCTBRIDGE_API OcctStatus occt_model_project_points_on_face(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        const OcctPoint3d* points,
        int count,
        OcctModelProjectionResult* results);

    OCCTBRIDGE_API OcctStatus occt_model_ray_intersections(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d origin,
        OcctVector3d directionValue,
        double minimumParameter,
        double maximumParameter,
        double tolerance,
        int* resultCount);

    OCCTBRIDGE_API OcctStatus occt_model_ray_hits_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctModelRayHit* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_classify_point(
        OcctModelingSessionHandle handle,
        OcctObjectId solidId,
        OcctPoint3d pointValue,
        double tolerance,
        OcctModelState* result);
}
