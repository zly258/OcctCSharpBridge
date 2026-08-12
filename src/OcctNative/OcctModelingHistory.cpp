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
        if (model == nullptr) return 0;
        try
        {
            const OperationRecord& operation = requireOperation(model, operationId);
            if (operation.history.IsNull()) return 0;
            return operation.history->IsRemoved(model->requireShape(sourceShapeId)) ? 1 : 0;
        }
        catch (...) { return 0; }
    }
}
