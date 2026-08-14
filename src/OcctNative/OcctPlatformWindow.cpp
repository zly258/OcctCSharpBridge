#include "OcctPlatformWindow.hxx"
#include "OcctNativeSurface.h"

#include <cstdint>
#include <stdexcept>

#if defined(_WIN32)
#include <WNT_Window.hxx>
#elif defined(__linux__)
#include <Xw_Window.hxx>
#else
#error Unsupported OCCT viewer platform
#endif

namespace OcctBridge
{
    Handle(Aspect_DisplayConnection) createPlatformDisplayConnection(void* displayHandle)
    {
#if defined(_WIN32)
        (void)displayHandle;
        return new Aspect_DisplayConnection();
#elif defined(__linux__)
        if (displayHandle != nullptr)
        {
            // The host owns an externally supplied X Display and keeps it alive
            // until after the viewer context has been destroyed.
            return new Aspect_DisplayConnection(reinterpret_cast<Aspect_XDisplay*>(displayHandle));
        }
        return new Aspect_DisplayConnection();
#endif
    }

    Handle(Aspect_Window) createPlatformWindow(
        const Handle(Aspect_DisplayConnection)& displayConnection,
        void* windowHandle)
    {
        if (windowHandle == nullptr) throw std::invalid_argument("The target native window handle is null.");
#if defined(_WIN32)
        (void)displayConnection;
        return new WNT_Window(reinterpret_cast<Aspect_Handle>(windowHandle));
#elif defined(__linux__)
        const Aspect_Drawable drawable = static_cast<Aspect_Drawable>(
            reinterpret_cast<std::uintptr_t>(windowHandle));
        if (drawable == 0) throw std::invalid_argument("The target X11 window ID is null.");
        return new Xw_Window(displayConnection, drawable);
#endif
    }

    OcctStatus validatePlatformSurfaceKind(int kind, std::string& message)
    {
#if defined(_WIN32)
        if (kind == OcctNativeSurface_Auto || kind == OcctNativeSurface_Win32Window)
            return OcctStatus_Ok;
        message = "This Windows build requires a Win32 native window surface.";
#elif defined(__linux__)
        if (kind == OcctNativeSurface_Auto || kind == OcctNativeSurface_X11Window)
            return OcctStatus_Ok;
        message = kind == OcctNativeSurface_WaylandSurface
            ? "Native Wayland surfaces are not supported by the OCCT 7.9 viewer backend yet. "
              "Use an X11/XWayland surface or a headless modeling session."
            : "This Linux build currently requires an X11/XWayland native window surface.";
#else
        (void)kind;
        message = "The native viewer surface is not supported on this platform.";
#endif
        return OcctStatus_ErrorNotSupported;
    }
}
