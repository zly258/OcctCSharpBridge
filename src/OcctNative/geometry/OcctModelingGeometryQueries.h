#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_vertex_point(
        OcctModelingSessionHandle handle,
        OcctObjectId vertexId,
        OcctPoint3d* result);

    OCCTBRIDGE_API OcctStatus occt_model_edge_endpoints(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* start,
        OcctPoint3d* end);

    OCCTBRIDGE_API OcctStatus occt_model_edge_point_at(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double normalizedParameter,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultTangent);

    OCCTBRIDGE_API OcctStatus occt_model_edge_curve_type(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctCurveType* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_surface_type(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctSurfaceType* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_uv_bounds(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctUvBounds* result);

    OCCTBRIDGE_API OcctStatus occt_model_face_point_normal(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultNormal);
}
