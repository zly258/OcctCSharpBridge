#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_history_generated_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_history_modified_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_history_is_removed_get(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctBool* result);

    OCCTBRIDGE_API OcctStatus occt_model_history_summary(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctModelTopologyHistorySummary* result);
}
