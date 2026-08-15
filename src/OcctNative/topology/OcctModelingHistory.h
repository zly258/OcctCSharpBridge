#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API int occt_model_history_generated_copy(
        OcctModelHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity);

    OCCTBRIDGE_API int occt_model_history_modified_copy(
        OcctModelHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity);

    OCCTBRIDGE_API int occt_model_history_is_removed(
        OcctModelHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_history_summary(
        OcctModelHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctModelTopologyHistorySummary* result);
}
