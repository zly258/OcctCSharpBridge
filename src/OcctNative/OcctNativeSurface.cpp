#include "OcctInternal.hxx"
#include "OcctNativeSurface.h"

#include <Aspect_PolygonOffsetMode.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Graphic3d_AspectFillArea3d.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>
#include <V3d_TypeOfOrientation.hxx>

#if defined(_WIN32)
#include <WNT_Window.hxx>
#elif defined(__linux__)
#include <Xw_Window.hxx>
#else
#error Unsupported OCCT viewer platform
#endif

namespace OcctBridge
{
    void initializeViewer(Engine* engine, void* windowHandle, void* displayHandle)
    {
        if (engine == nullptr) throw std::invalid_argument("The OCCT engine handle is null.");
        if (windowHandle == nullptr) throw std::invalid_argument("The target native window handle is null.");

#if defined(_WIN32)
        (void)displayHandle;
        engine->displayConnection = new Aspect_DisplayConnection();
#elif defined(__linux__)
        if (displayHandle != nullptr)
        {
            // Aspect_DisplayConnection does not own an externally supplied X Display.
            // The Avalonia host keeps this Display alive until after Engine disposal.
            engine->displayConnection = new Aspect_DisplayConnection(
                reinterpret_cast<Aspect_XDisplay*>(displayHandle));
        }
        else
        {
            engine->displayConnection = new Aspect_DisplayConnection();
        }
#endif

        engine->graphicDriver = new OpenGl_GraphicDriver(engine->displayConnection);
        engine->viewer = new V3d_Viewer(engine->graphicDriver);
        engine->viewer->SetDefaultLights();
        engine->viewer->SetLightOn();
        engine->viewer->SetDefaultTypeOfView(V3d_ORTHOGRAPHIC);
        engine->context = new AIS_InteractiveContext(engine->viewer);
        engine->view = engine->viewer->CreateView();
        engine->view->SetAutoZFitMode(Standard_True, 1.0);

        const Handle(Prs3d_Drawer)& defaultDrawer = engine->context->DefaultDrawer();
        defaultDrawer->SetupOwnShadingAspect();
        defaultDrawer->ShadingAspect()->Aspect()->SetPolygonOffsets(Aspect_POM_Fill, 1.0f, 1.0f);

#if defined(_WIN32)
        engine->window = new WNT_Window(reinterpret_cast<Aspect_Handle>(windowHandle));
#elif defined(__linux__)
        const Aspect_Drawable drawable = static_cast<Aspect_Drawable>(
            reinterpret_cast<std::uintptr_t>(windowHandle));
        if (drawable == 0) throw std::invalid_argument("The target X11 window ID is null.");
        engine->window = new Xw_Window(engine->displayConnection, drawable);
#endif

        engine->view->SetWindow(engine->window);
        if (!engine->window->IsMapped()) engine->window->Map();
        engine->view->SetBackgroundColor(color(0.94, 0.96, 0.98));
        engine->view->TriedronDisplay(
            Aspect_TOTP_RIGHT_LOWER,
            Quantity_NOC_GRAY40,
            0.08,
            V3d_ZBUFFER);

        engine->viewCube = new AIS_ViewCube();
        engine->viewCube->SetSize(55.0);
        engine->viewCube->SetBoxFacetExtension(6.0);
        engine->viewCube->SetFontHeight(14.0);
        engine->viewCube->SetAutoStartAnimation(true);
        engine->viewCube->SetResetCamera(true);
        engine->viewCube->SetFitSelected(false);
        engine->viewCube->SetTransformPersistence(
            new Graphic3d_TransformPers(
                Graphic3d_TMF_TriedronPers,
                Aspect_TOTP_RIGHT_UPPER,
                Graphic3d_Vec2i(85, 85)));
        engine->context->Display(engine->viewCube, Standard_False);

        engine->view->SetProj(V3d_XposYnegZpos);
        engine->view->MustBeResized();
        engine->view->Redraw();
    }
}

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

        return execute(engine, [&]
        {
            initializeViewer(engine, surface->handle, surface->display);
        });
    }
}
