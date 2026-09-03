#include "core/OcctInternal.hxx"
#include "OcctNativeSurface.h"
#include "OcctPlatformWindow.hxx"

#include <AIS_DisplayMode.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Prs3d_LineAspect.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Prs3d_Drawer.hxx>
#include <V3d_TypeOfOrientation.hxx>

namespace OcctBridge
{
    void initializeViewer(Engine* engine, void* windowHandle, void* displayHandle)
    {
        if (engine == nullptr) throw std::invalid_argument("The OCCT engine handle is null.");
        if (windowHandle == nullptr) throw std::invalid_argument("The target native window handle is null.");

        engine->viewerContext.displayConnection = createPlatformDisplayConnection(displayHandle);
        engine->viewerContext.graphicDriver = new OpenGl_GraphicDriver(engine->viewerContext.displayConnection);
        engine->viewerContext.viewer = new V3d_Viewer(engine->viewerContext.graphicDriver);
        engine->viewerContext.viewer->SetDefaultLights();
        engine->viewerContext.viewer->SetLightOn();
        engine->viewerContext.viewer->SetDefaultTypeOfView(V3d_ORTHOGRAPHIC);
        engine->viewerContext.context = new AIS_InteractiveContext(engine->viewerContext.viewer);
        engine->viewerContext.context->SetDisplayMode(AIS_Shaded, Standard_False);
        engine->viewerContext.displayMode = AIS_Shaded;
        engine->viewerContext.view = engine->viewerContext.viewer->CreateView();

        // Keep OCCT rendering defaults (including depth pre-pass disabled) and
        // make the CAD default explicitly "shaded with face boundaries".
        const Handle(Prs3d_Drawer)& defaultDrawer = engine->viewerContext.context->DefaultDrawer();
        defaultDrawer->SetFaceBoundaryDraw(Standard_True);
        defaultDrawer->SetFaceBoundaryAspect(new Prs3d_LineAspect(
            Quantity_NOC_BLACK,
            Aspect_TOL_SOLID,
            1.0));

        engine->viewerContext.window = createPlatformWindow(
            engine->viewerContext.displayConnection,
            windowHandle);

        engine->viewerContext.view->SetWindow(engine->viewerContext.window);
        // Mapping is intentionally deferred until the first real redraw. UI adapters can
        // configure background/view/projection/decorations inside a display batch while
        // the native child remains hidden, so the first presented frame is already final.
        engine->viewerContext.view->SetBackgroundColor(color(0.94, 0.96, 0.98));
        engine->viewerContext.view->TriedronDisplay(
            Aspect_TOTP_RIGHT_LOWER,
            Quantity_NOC_GRAY40,
            0.08,
            V3d_ZBUFFER);

        engine->viewerContext.viewCube = new AIS_ViewCube();
        engine->viewerContext.viewCube->SetSize(55.0);
        engine->viewerContext.viewCube->SetBoxFacetExtension(6.0);
        engine->viewerContext.viewCube->SetFontHeight(14.0);
        engine->viewerContext.viewCube->SetAutoStartAnimation(true);
        engine->viewerContext.viewCube->SetResetCamera(true);
        engine->viewerContext.viewCube->SetFitSelected(false);
        engine->viewerContext.viewCube->SetTransformPersistence(
            new Graphic3d_TransformPers(
                Graphic3d_TMF_TriedronPers,
                Aspect_TOTP_RIGHT_UPPER,
                Graphic3d_Vec2i(85, 85)));
        engine->viewerContext.context->Display(engine->viewerContext.viewCube, Standard_False);

        engine->viewerContext.view->SetProj(V3d_XposYnegZpos);
        engine->viewerContext.view->MustBeResized();
    }
}

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }
}

extern "C"
{
    OcctStatus occt_engine_initialize_surface(
        OcctEngineHandle handle,
        const OcctNativeSurface* surface)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        engine->clearError();

        if (surface == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "The native surface descriptor is null.");
            return engine->currentErrorCode();
        }
        if (surface->structSize < sizeof(OcctNativeSurface) || surface->apiVersion != 1)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Unsupported native surface descriptor size or version.");
            return engine->currentErrorCode();
        }
        if (surface->handle == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "The native surface handle is null.");
            return engine->currentErrorCode();
        }

        std::string platformError;
        const OcctStatus platformStatus = validatePlatformSurfaceKind(
            static_cast<int>(surface->kind),
            platformError);
        if (platformStatus != OcctStatus_Ok)
        {
            engine->setError(platformStatus, platformError);
            return engine->currentErrorCode();
        }

        const int succeeded = execute(engine, [&]
        {
            initializeViewer(engine, surface->handle, surface->display);
        });
        return succeeded != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    OcctStatus occt_engine_surface_resize(OcctEngineHandle handle, int redraw)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, [&]
        {
            engine->viewerContext.view->MustBeResized();
            if (redraw != 0) engine->viewerContext.requestRedraw();
        }) != 0 ? OcctStatus_Ok : engine->currentErrorCode();
    }

    OcctStatus occt_engine_surface_redraw(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, [&] { engine->requestRedraw(); }) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }
}
