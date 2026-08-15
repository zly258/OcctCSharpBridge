#pragma once

#include "modeling/OcctModeling.h"

#include <cstdint>

extern "C"
{
    struct OcctAlgorithmSummary
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        OcctOperationId operationId;
        OcctBool hasWarnings;
        OcctBool hasErrors;
    };

    OCCTBRIDGE_API OcctStatus occt_model_algorithm_acquire(
        OcctModelingSessionHandle session,
        OcctOperationId operationId,
        OcctAlgorithmHandle* result);

    OCCTBRIDGE_API void occt_algorithm_release(OcctAlgorithmHandle handle);

    OCCTBRIDGE_API OcctStatus occt_algorithm_get_summary(
        OcctAlgorithmHandle handle,
        OcctAlgorithmSummary* result);

    OCCTBRIDGE_API OcctStatus occt_algorithm_report_copy(
        OcctAlgorithmHandle handle,
        char* result,
        int capacity,
        int* written);
}
