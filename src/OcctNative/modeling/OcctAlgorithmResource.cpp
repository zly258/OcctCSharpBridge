#include "OcctAlgorithmResource.h"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <cstring>
#include <limits>
#include <string>

struct OcctAlgorithmHandle_t
{
    OcctOperationId operationId = 0;
    std::string report;
    OcctBool hasWarnings = 0;
    OcctBool hasErrors = 0;
};

using namespace OcctModelingInternal;

namespace
{
    constexpr std::uint32_t AlgorithmSummaryApiVersion = 1;
}

extern "C"
{
    OcctStatus occt_model_algorithm_acquire(
        OcctModelingSessionHandle session,
        OcctOperationId operationId,
        OcctAlgorithmHandle* result)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeStatus(model, [&]
        {
            if (result == nullptr)
                throw std::invalid_argument("Algorithm handle output is null.");
            *result = nullptr;

            const OperationRecord& operation = requireOperation(model, operationId);
            auto handle = new OcctAlgorithmHandle_t();
            handle->operationId = operationId;
            handle->report = operation.report;
            handle->hasWarnings = operation.hasWarnings ? 1 : 0;
            handle->hasErrors = operation.hasErrors ? 1 : 0;
            *result = handle;
        });
    }

    void occt_algorithm_release(OcctAlgorithmHandle handle)
    {
        delete handle;
    }

    OcctStatus occt_algorithm_get_summary(
        OcctAlgorithmHandle handle,
        OcctAlgorithmSummary* result)
    {
        if (handle == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        if (result->structSize < sizeof(OcctAlgorithmSummary) ||
            result->apiVersion != AlgorithmSummaryApiVersion)
            return OcctStatus_ErrorInvalidArgument;

        result->operationId = handle->operationId;
        result->hasWarnings = handle->hasWarnings;
        result->hasErrors = handle->hasErrors;
        return OcctStatus_Ok;
    }

    OcctStatus occt_algorithm_report_copy(
        OcctAlgorithmHandle handle,
        char* result,
        int capacity,
        int* written)
    {
        if (handle == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (written == nullptr || capacity < 0) return OcctStatus_ErrorInvalidArgument;
        if (handle->report.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
            return OcctStatus_ErrorInvalidState;

        const int required = static_cast<int>(handle->report.size());
        *written = required;
        if (required == 0) return OcctStatus_Ok;
        if (result == nullptr)
            return capacity == 0 ? OcctStatus_ErrorBufferTooSmall : OcctStatus_ErrorInvalidArgument;
        if (capacity < required) return OcctStatus_ErrorBufferTooSmall;

        std::memcpy(result, handle->report.data(), static_cast<std::size_t>(required));
        return OcctStatus_Ok;
    }
}
