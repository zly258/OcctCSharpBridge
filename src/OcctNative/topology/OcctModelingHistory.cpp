#include "topology/OcctModelingHistory.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_history_generated_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            *required = historyCopy(model, operationId, sourceShapeId, true, results, capacity);
        });
    }

    OcctStatus occt_model_history_modified_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            *required = historyCopy(model, operationId, sourceShapeId, false, results, capacity);
        });
    }

    OcctStatus occt_model_history_is_removed_get(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctBool* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = 0;
        return executeStatus(model, [&]
        {
            HistoryLineage& lineage = materializeHistoryLineage(model, operationId, sourceShapeId);
            *result = lineage.removed ? 1 : 0;
        });
    }

    OcctStatus occt_model_history_summary(
        OcctModelingSessionHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctModelTopologyHistorySummary* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            HistoryLineage& lineage = materializeHistoryLineage(model, operationId, sourceShapeId);
            result->generatedCount = static_cast<int>(lineage.generated.size());
            result->modifiedCount = static_cast<int>(lineage.modified.size());
            result->removed = lineage.removed ? 1 : 0;
        });
    }
}
