#pragma once

#include "OcctNative.h"
#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctModelingSessionHandle occt_model_session_create();
    OCCTBRIDGE_API void occt_model_session_destroy(OcctModelingSessionHandle handle);
    OCCTBRIDGE_API OcctStatus occt_model_session_last_error_code(OcctModelingSessionHandle handle);
    OCCTBRIDGE_API OcctStatus occt_model_session_last_error_message(
        OcctModelingSessionHandle handle,
        char* buffer,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_capabilities_get(
        char* buffer,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_shapes_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_shape_exists(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool* result);

    OCCTBRIDGE_API OcctStatus occt_model_shape_delete(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId);

    OCCTBRIDGE_API OcctStatus occt_model_clear(
        OcctModelingSessionHandle handle);

    OCCTBRIDGE_API OcctStatus occt_model_operation_report_get(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        char* buffer,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_shape_copy(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctObjectId* result);
}
