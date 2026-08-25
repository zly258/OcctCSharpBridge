#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_view_cube_language_set(
        OcctEngineHandle handle,
        int language);

    OCCTBRIDGE_API OcctStatus occt_engine_view_cube_axes_set(
        OcctEngineHandle handle,
        OcctBool visible);

    // Gives the native AIS_ViewCube first chance to consume a pointer click.
    // handled is set to 1 only when the detected owner belongs to the current view cube.
    OCCTBRIDGE_API OcctStatus occt_engine_view_cube_try_click(
        OcctEngineHandle handle,
        int x,
        int y,
        int* handled);
}
