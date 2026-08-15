#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_view_cube_language_set(
        OcctEngineHandle handle,
        int language);
}
