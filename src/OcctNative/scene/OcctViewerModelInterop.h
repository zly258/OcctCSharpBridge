#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_object_shape_update_from_model(
        OcctEngineHandle engineHandle,
        OcctModelHandle modelHandle,
        OcctObjectId viewerObjectId,
        OcctObjectId modelShapeId,
        std::uint32_t options);
}
