#include "OcctModelingAlgorithmInternal.hxx"

using namespace OcctModelingInternal;

extern "C"
{
    int occt_model_history_generated_copy(
        OcctModelHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&] { copied = historyCopy(model, operationId, sourceShapeId, true, results, capacity); }) == 0)
            return -1;
        return copied;
    }

    int occt_model_history_modified_copy(
        OcctModelHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctObjectId* results,
        int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&] { copied = historyCopy(model, operationId, sourceShapeId, false, results, capacity); }) == 0)
            return -1;
        return copied;
    }

    int occt_model_history_is_removed(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId)
    {
        ModelSession* model = modelOf(handle);
        return executeValue(model, 0, [&]
        {
            HistoryLineage& lineage = materializeHistoryLineage(model, operationId, sourceShapeId);
            return lineage.removed ? 1 : 0;
        });
    }
    OcctStatus occt_model_history_summary(
        OcctModelHandle handle,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        OcctModelTopologyHistorySummary* result)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;

        const int succeeded = execute(model, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Topology-history summary result is null.");
            HistoryLineage& lineage = materializeHistoryLineage(model, operationId, sourceShapeId);
            result->generatedCount = static_cast<int>(lineage.generated.size());
            result->modifiedCount = static_cast<int>(lineage.modified.size());
            result->removed = lineage.removed ? 1 : 0;
        });
        return succeeded != 0 ? OcctStatus_Ok : model->errors.code;
    }


}
