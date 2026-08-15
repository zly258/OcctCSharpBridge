#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API int occt_model_vertex_point(
        OcctModelHandle handle,
        OcctObjectId vertexId,
        OcctPoint3d* result);

    OCCTBRIDGE_API int occt_model_edge_endpoints(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* start,
        OcctPoint3d* end);

    OCCTBRIDGE_API int occt_model_edge_point_at(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        double normalizedParameter,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultTangent);

    OCCTBRIDGE_API int occt_model_edge_curve_type(OcctModelHandle handle, OcctObjectId edgeId);
    OCCTBRIDGE_API int occt_model_face_surface_type(OcctModelHandle handle, OcctObjectId faceId);

    OCCTBRIDGE_API int occt_model_face_uv_bounds(
        OcctModelHandle handle,
        OcctObjectId faceId,
        OcctUvBounds* result);

    OCCTBRIDGE_API int occt_model_face_point_normal(
        OcctModelHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultNormal);
}
