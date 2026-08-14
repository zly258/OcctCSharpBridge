#pragma once

#include <cstdint>

extern "C"
{
    struct OcctEngineHandle_t;
    struct OcctModelingSessionHandle_t;
    struct OcctShapeHandle_t;
    struct OcctMeshHandle_t;

    using OcctEngineHandle = OcctEngineHandle_t*;
    using OcctModelingSessionHandle = OcctModelingSessionHandle_t*;
    using OcctShapeHandle = OcctShapeHandle_t*;
    using OcctMeshHandle = OcctMeshHandle_t*;
    using OcctBool = std::int32_t;

    enum OcctStatus : std::int32_t
    {
        OcctStatus_Ok = 0,
        OcctStatus_ErrorUnknown = -1,
        OcctStatus_ErrorInvalidArgument = -2,
        OcctStatus_ErrorInvalidHandle = -3,
        OcctStatus_ErrorNotInitialized = -4,
        OcctStatus_ErrorNotFound = -5,
        OcctStatus_ErrorInvalidState = -6,
        OcctStatus_ErrorBufferTooSmall = -7,
        OcctStatus_ErrorGeometry = -20,
        OcctStatus_ErrorTopology = -21,
        OcctStatus_ErrorAlgorithm = -22,
        OcctStatus_ErrorIo = -30,
        OcctStatus_ErrorFormat = -31,
        OcctStatus_ErrorPlatform = -40,
        OcctStatus_ErrorNotSupported = -41,
        OcctStatus_ErrorCancelled = -42,
        OcctStatus_ErrorOutOfMemory = -50,
        OcctStatus_ErrorOcct = -60
    };
}
