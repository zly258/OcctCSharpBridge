#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_shape_hash(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        std::int64_t* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_type(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctShapeType* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_orientation(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelOrientation* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_is_closed(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_is_valid(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_tolerance(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_bounds(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBounds* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_linear_properties(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_surface_properties(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_volume_properties(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_distance(
        OcctModelingSessionHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId,
        OcctDistanceResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_distances(
        OcctModelingSessionHandle handle,
        const OcctObjectId* firstIds,
        const OcctObjectId* secondIds,
        int count,
        OcctDistanceResult* results);

    OCCTBRIDGE_API OcctStatus occt_model_shape_check_report_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        char* buffer,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_shape_location_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelLocation* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_location_set(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const OcctModelLocation* location,
        OcctBool copyShape,
        OcctObjectId* result);
}
