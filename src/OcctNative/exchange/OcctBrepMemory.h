#pragma once

#include "modeling/OcctModeling.h"

#include <cstdint>

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_brep_serialize(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        std::uint8_t* buffer,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_brep_deserialize(
        OcctModelingSessionHandle handle,
        const std::uint8_t* buffer,
        int length,
        OcctObjectId* result);
}
