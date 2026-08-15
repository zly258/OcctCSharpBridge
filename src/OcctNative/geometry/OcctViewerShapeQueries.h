#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_shape_type_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        int* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_validity_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctBool* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_bounds_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctBounds* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_linear_properties_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_surface_properties_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_volume_properties_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_distance_get(
        OcctEngineHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId,
        OcctDistanceResult* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_topology_count_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        int shapeType,
        int* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_subshape_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        int shapeType,
        int index,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_translate_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctVector3d value,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_rotate_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_scale_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d center,
        double factor,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_mirror_plane_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_hash_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        std::int64_t* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_vertex_point_get(
        OcctEngineHandle handle,
        OcctObjectId vertexId,
        OcctPoint3d* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_edge_endpoints_get(
        OcctEngineHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* start,
        OcctPoint3d* end);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_edge_evaluate(
        OcctEngineHandle handle,
        OcctObjectId edgeId,
        double normalizedParameter,
        OcctPoint3d* point,
        OcctVector3d* tangent);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_edge_curve_type_get(
        OcctEngineHandle handle,
        OcctObjectId edgeId,
        int* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_face_surface_type_get(
        OcctEngineHandle handle,
        OcctObjectId faceId,
        int* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_face_uv_bounds_get(
        OcctEngineHandle handle,
        OcctObjectId faceId,
        OcctUvBounds* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_face_evaluate(
        OcctEngineHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        OcctPoint3d* point,
        OcctVector3d* normal);
}
