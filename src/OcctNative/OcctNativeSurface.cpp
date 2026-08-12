#include "OcctInternal.hxx"
#include "OcctNativeSurface.h"

using namespace OcctBridge;

extern "C"
{
    int occt_initialize_surface(OcctHandle handle, const OcctNativeSurface* surface)
    {
        Engine* engine = engineOf(handle);
        if (engine == nullptr) return 0;

        engine->clearError();
        if (surface == nullptr)
        {
            engine->setError("The native surface descriptor is null.");
            return 0;
        }
        if (surface->handle == nullptr)
        {
            engine->setError("The native surface handle is null.");
            return 0;
        }

#if defined(_WIN32)
        if (surface->kind != OcctNativeSurface_Auto
            && surface->kind != OcctNativeSurface_Win32Window)
        {
            engine->setError("This Windows build requires a Win32 native window surface.");
            return 0;
        }
#elif defined(__linux__)
        if (surface->kind != OcctNativeSurface_Auto
            && surface->kind != OcctNativeSurface_X11Window)
        {
            if (surface->kind == OcctNativeSurface_WaylandSurface)
            {
                engine->setError(
                    "Native Wayland surfaces are not supported by the OCCT 7.9 viewer backend yet. "
                    "Use an X11/XWayland surface or a headless modeling session.");
            }
            else
            {
                engine->setError("This Linux build currently requires an X11/XWayland native window surface.");
            }
            return 0;
        }
#else
        engine->setError("The native viewer surface is not supported on this platform.");
        return 0;
#endif

        return occt_initialize(handle, surface->handle);
    }
}
