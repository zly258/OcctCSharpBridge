#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_update_begin(OcctEngineHandle handle);

    OCCTBRIDGE_API OcctStatus occt_engine_update_end(
        OcctEngineHandle handle,
        int fitAll);

    OCCTBRIDGE_API OcctStatus occt_engine_update_state_get(
        OcctEngineHandle handle,
        int* isUpdating);
}
