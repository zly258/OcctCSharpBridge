#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctNativeSurfaceKind
    {
        OcctNativeSurface_Auto = 0,
        OcctNativeSurface_Win32Window = 1,
        OcctNativeSurface_X11Window = 2,
        OcctNativeSurface_WaylandSurface = 3
    };

    struct OcctNativeSurface
    {
        int kind;
        void* handle;
        void* display;
    };

    OCCTBRIDGE_API int occt_initialize_surface(OcctHandle handle, const OcctNativeSurface* surface);
}
