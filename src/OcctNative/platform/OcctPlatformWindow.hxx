#pragma once

#include "OcctStatus.h"

#include <Aspect_DisplayConnection.hxx>
#include <Aspect_Window.hxx>

#include <string>

namespace OcctBridge
{
    Handle(Aspect_DisplayConnection) createPlatformDisplayConnection(void* displayHandle);
    Handle(Aspect_Window) createPlatformWindow(
        const Handle(Aspect_DisplayConnection)& displayConnection,
        void* windowHandle);
    OcctStatus validatePlatformSurfaceKind(int kind, std::string& message);
}
