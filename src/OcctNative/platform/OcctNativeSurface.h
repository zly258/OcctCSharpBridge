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
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        OcctNativeSurfaceKind kind;
        void* handle;
        void* display;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_initialize_surface(
        OcctEngineHandle handle,
        const OcctNativeSurface* surface);

    OCCTBRIDGE_API OcctStatus occt_engine_surface_resize(
        OcctEngineHandle handle,
        int redraw);

    OCCTBRIDGE_API OcctStatus occt_engine_surface_redraw(OcctEngineHandle handle);
}
