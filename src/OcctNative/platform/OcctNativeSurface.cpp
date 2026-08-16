#include "core/OcctInternal.hxx"
#include "OcctNativeSurface.h"
#include "OcctPlatformWindow.hxx"

#include <Aspect_PolygonOffsetMode.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Graphic3d_AspectFillArea3d.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>
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
        engine->viewerContext.view = engine->viewerContext.viewer->CreateView();
        engine->viewerContext.view->SetAutoZFitMode(Standard_True, 1.0);

        const Handle(Prs3d_Drawer)& defaultDrawer = engine->viewerContext.context->DefaultDrawer();
        defaultDrawer->SetupOwnShadingAspect();
        defaultDrawer->ShadingAspect()->Aspect()->SetPolygonOffsets(Aspect_POM_Fill, 1.0f, 1.0f);

        engine->viewerContext.window = createPlatformWindow(
            engine->viewerContext.displayConnection,
            windowHandle);

        engine->viewerContext.view->SetWindow(engine->viewerContext.window);
        if (!engine->viewerContext.window->IsMapped()) engine->viewerContext.window->Map();
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
        // Do not draw here. Public OcctEngine.Initialize preserves the historical immediate
        // first frame, while reusable UI adapters intentionally configure their initial view
        // inside one display batch before presenting the first frame.
    }
}

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
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
            return engine->errors.code;
        }
        if (surface->structSize < sizeof(OcctNativeSurface) || surface->apiVersion != 1)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Unsupported native surface descriptor size or version.");
            return engine->errors.code;
        }
        if (surface->handle == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "The native surface handle is null.");
            return engine->errors.code;
        }

        std::string platformError;
        const OcctStatus platformStatus = validatePlatformSurfaceKind(
            static_cast<int>(surface->kind),
            platformError);
        if (platformStatus != OcctStatus_Ok)
        {
            engine->setError(platformStatus, platformError);
            return engine->errors.code;
        }

        const int succeeded = execute(engine, [&]
        {
            initializeViewer(engine, surface->handle, surface->display);
        });
        return succeeded != 0 ? OcctStatus_Ok : engine->errors.code;
    }

    OcctStatus occt_engine_surface_resize(OcctEngineHandle handle, int redraw)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, [&]
        {
            engine->viewerContext.view->MustBeResized();
            if (redraw != 0) engine->viewerContext.view->Redraw();
        }) != 0 ? OcctStatus_Ok : engine->errors.code;
    }

    OcctStatus occt_engine_surface_redraw(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, [&] { engine->requestRedraw(); }) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }
}
