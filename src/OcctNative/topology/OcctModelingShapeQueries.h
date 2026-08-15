#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API std::int64_t occt_model_shape_hash(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_type(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_orientation(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_is_closed(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_model_shape_is_valid(OcctModelHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API double occt_model_shape_tolerance(OcctModelHandle handle, OcctObjectId shapeId);

    OCCTBRIDGE_API int occt_model_shape_bounds(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctBounds* result);

    OCCTBRIDGE_API int occt_model_shape_linear_properties(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API int occt_model_shape_surface_properties(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API int occt_model_shape_volume_properties(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API int occt_model_shape_distance(
        OcctModelHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId,
        OcctDistanceResult* result);

    OCCTBRIDGE_API const char* occt_model_check_report(OcctModelHandle handle, OcctObjectId shapeId);

    OCCTBRIDGE_API int occt_model_get_location(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctModelLocation* result);

    OCCTBRIDGE_API OcctObjectId occt_model_set_location(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        const OcctModelLocation* location,
        int copyShape);
}
