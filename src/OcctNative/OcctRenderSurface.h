#pragma once

#include "OcctNative.h"

extern "C"
{
    // Resize the native OCCT render surface without presenting a frame.
    OCCTBRIDGE_API int occt_resize_surface(OcctHandle handle);
}
