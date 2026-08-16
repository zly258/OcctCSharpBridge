#include "presentation/OcctView.h"
#include "core/OcctInternal.hxx"
#include "exchange/OcctExchangePath.hxx"

#include <Aspect_GradientFillMethod.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_MaterialAspect.hxx>
#include <Graphic3d_RenderingParams.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>
#include <V3d_TypeOfOrientation.hxx>

#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t ViewOptionsApiVersion = 1;
    constexpr std::uint32_t DisplayQualityOptionsApiVersion = 1;
    constexpr std::uint32_t NavigationOptionsApiVersion = 1;
    constexpr std::uint32_t AllViewStateBits =
        OcctViewerViewStateUpdate_Orientation |
        OcctViewerViewStateUpdate_Projection |
        OcctViewerViewStateUpdate_PerspectiveFov |
        OcctViewerViewStateUpdate_SolidBackground |
        OcctViewerViewStateUpdate_GradientBackground |
        OcctViewerViewStateUpdate_DisplayMode |
        OcctViewerViewStateUpdate_TriedronVisible |
        OcctViewerViewStateUpdate_ViewCubeVisible |
        OcctViewerViewStateUpdate_ComputedMode |
        OcctViewerViewStateUpdate_Antialiasing |
        OcctViewerViewStateUpdate_Scale;
    constexpr std::uint32_t AllDisplayQualityBits =
        OcctViewerDisplayQualityUpdate_Precision |
        OcctViewerDisplayQualityUpdate_DefaultMaterial;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeViewStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    V3d_TypeOfOrientation viewOrientation(int value)
    {
        switch (value)
        {
            case OcctView_Isometric: return V3d_TypeOfOrientation_Zup_AxoRight;
            case OcctView_Front: return V3d_Yneg;
            case OcctView_Back: return V3d_Ypos;
            case OcctView_Left: return V3d_Xneg;
            case OcctView_Right: return V3d_Xpos;
            case OcctView_Top: return V3d_Zpos;
            case OcctView_Bottom: return V3d_Zneg;
            default: throw std::invalid_argument("View orientation is out of range.");
        }
    }

    Graphic3d_Camera::Projection projectionType(int value)
    {
        switch (value)
        {
            case OcctProjection_Orthographic: return Graphic3d_Camera::Projection_Orthographic;
            case OcctProjection_Perspective: return Graphic3d_Camera::Projection_Perspective;
            default: throw std::invalid_argument("Projection type is out of range.");
        }
    }

    Aspect_GradientFillMethod gradientMethod(int value)
    {
        if (value < static_cast<int>(Aspect_GradientFillMethod_None) ||
            value > static_cast<int>(Aspect_GradientFillMethod_Elliptical))
        {
            throw std::invalid_argument("Gradient fill method is out of range.");
        }
        return static_cast<Aspect_GradientFillMethod>(value);
    }

    int displayMode(int value)
    {
        switch (value)
        {
            case OcctDisplay_Wireframe: return AIS_WireFrame;
            case OcctDisplay_Shaded: return AIS_Shaded;
            default: throw std::invalid_argument("Display mode is out of range.");
        }
    }

    void requireFinitePoint(OcctPoint3d value, const char* name)
    {
        if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
            throw std::invalid_argument(std::string(name) + " must be finite.");
    }

    void validateViewOptions(const OcctViewerViewStateOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("View state options are null.");
        if (options->structSize < sizeof(OcctViewerViewStateOptions) ||
            options->apiVersion != ViewOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported view state options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllViewStateBits) != 0)
            throw std::invalid_argument("View state update mask is invalid.");
        if ((options->updateMask & OcctViewerViewStateUpdate_SolidBackground) != 0 &&
            (options->updateMask & OcctViewerViewStateUpdate_GradientBackground) != 0)
        {
            throw std::invalid_argument("Solid and gradient background updates cannot be combined.");
        }
        if ((options->updateMask & OcctViewerViewStateUpdate_Orientation) != 0)
            (void)viewOrientation(options->orientation);
        if ((options->updateMask & OcctViewerViewStateUpdate_Projection) != 0)
            (void)projectionType(options->projectionType);
        if ((options->updateMask & OcctViewerViewStateUpdate_PerspectiveFov) != 0 &&
            (!std::isfinite(options->perspectiveFovDegrees) ||
             options->perspectiveFovDegrees <= 1.0 || options->perspectiveFovDegrees >= 179.0))
        {
            throw std::invalid_argument("FOV must be between 1 and 179 degrees.");
        }
        if ((options->updateMask & OcctViewerViewStateUpdate_SolidBackground) != 0)
            (void)color(options->backgroundFirst.r, options->backgroundFirst.g, options->backgroundFirst.b);
        if ((options->updateMask & OcctViewerViewStateUpdate_GradientBackground) != 0)
        {
            (void)color(options->backgroundFirst.r, options->backgroundFirst.g, options->backgroundFirst.b);
            (void)color(options->backgroundSecond.r, options->backgroundSecond.g, options->backgroundSecond.b);
            (void)gradientMethod(options->gradientFillMethod);
        }
        if ((options->updateMask & OcctViewerViewStateUpdate_DisplayMode) != 0)
            (void)displayMode(options->displayMode);
        if ((options->updateMask & OcctViewerViewStateUpdate_Scale) != 0)
            requirePositive(options->scale, "View scale");
    }

    void applyViewOptions(Engine* engine, const OcctViewerViewStateOptions& options)
    {
        if ((options.updateMask & OcctViewerViewStateUpdate_Orientation) != 0)
        {
            engine->viewerContext.view->SetProj(viewOrientation(options.orientation));
            if (options.fitAfterOrientation != 0)
            {
                engine->viewerContext.view->FitAll(0.01, Standard_False);
                engine->viewerContext.view->ZFitAll();
            }
        }
        if ((options.updateMask & OcctViewerViewStateUpdate_Projection) != 0)
            engine->viewerContext.view->Camera()->SetProjectionType(projectionType(options.projectionType));
        if ((options.updateMask & OcctViewerViewStateUpdate_PerspectiveFov) != 0)
            engine->viewerContext.view->Camera()->SetFOVy(options.perspectiveFovDegrees);
        if ((options.updateMask & OcctViewerViewStateUpdate_SolidBackground) != 0)
        {
            engine->viewerContext.view->SetBgGradientStyle(Aspect_GradientFillMethod_None, Standard_False);
            engine->viewerContext.view->SetBackgroundColor(color(
                options.backgroundFirst.r,
                options.backgroundFirst.g,
                options.backgroundFirst.b));
        }
        if ((options.updateMask & OcctViewerViewStateUpdate_GradientBackground) != 0)
        {
            engine->viewerContext.view->SetBgGradientColors(
                color(options.backgroundFirst.r, options.backgroundFirst.g, options.backgroundFirst.b),
                color(options.backgroundSecond.r, options.backgroundSecond.g, options.backgroundSecond.b),
                gradientMethod(options.gradientFillMethod),
                Standard_False);
        }
        if ((options.updateMask & OcctViewerViewStateUpdate_DisplayMode) != 0)
        {
            engine->viewerContext.displayMode = displayMode(options.displayMode);
            for (auto& pair : engine->scene.objects)
            {
                if (pair.second.kind == OcctObject_Shape && !pair.second.presentation.IsNull())
                {
                    engine->viewerContext.context->SetDisplayMode(
                        pair.second.presentation,
                        engine->viewerContext.displayMode,
                        Standard_False);
                }
            }
        }
        if ((options.updateMask & OcctViewerViewStateUpdate_TriedronVisible) != 0)
        {
            if (options.triedronVisible != 0)
            {
                engine->viewerContext.view->TriedronDisplay(
                    Aspect_TOTP_RIGHT_LOWER,
                    Quantity_NOC_GRAY40,
                    0.08,
                    V3d_ZBUFFER);
            }
            else
            {
                engine->viewerContext.view->TriedronErase();
            }
        }
        if ((options.updateMask & OcctViewerViewStateUpdate_ViewCubeVisible) != 0)
        {
            if (engine->viewerContext.viewCube.IsNull())
                throw std::runtime_error("The view cube has not been initialized.");
            if (options.viewCubeVisible != 0)
                engine->viewerContext.context->Display(engine->viewerContext.viewCube, Standard_False);
            else
                engine->viewerContext.context->Erase(engine->viewerContext.viewCube, Standard_False);
        }
        if ((options.updateMask & OcctViewerViewStateUpdate_ComputedMode) != 0)
            engine->viewerContext.view->SetComputedMode(options.computedMode != 0);
        if ((options.updateMask & OcctViewerViewStateUpdate_Antialiasing) != 0)
            engine->viewerContext.view->ChangeRenderingParams().IsAntialiasingEnabled = options.antialiasingEnabled != 0;
        if ((options.updateMask & OcctViewerViewStateUpdate_Scale) != 0)
            engine->viewerContext.view->Camera()->SetScale(options.scale);

        engine->requestRedraw();
    }

    void validateDisplayQualityOptions(const OcctViewerDisplayQualityOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Display quality options are null.");
        if (options->structSize < sizeof(OcctViewerDisplayQualityOptions) ||
            options->apiVersion != DisplayQualityOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported display quality options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllDisplayQualityBits) != 0)
            throw std::invalid_argument("Display quality update mask is invalid.");
        if ((options->updateMask & OcctViewerDisplayQualityUpdate_Precision) != 0)
        {
            requirePositive(options->deviationCoefficient, "Deviation coefficient");
            if (!std::isfinite(options->deviationAngleDegrees) ||
                options->deviationAngleDegrees <= 0.0 || options->deviationAngleDegrees >= 90.0)
            {
                throw std::invalid_argument("Deviation angle must be between 0 and 90 degrees.");
            }
        }
        if ((options->updateMask & OcctViewerDisplayQualityUpdate_DefaultMaterial) != 0)
            (void)materialName(options->material);
    }

    void applyDisplayQualityOptions(Engine* engine, const OcctViewerDisplayQualityOptions& options)
    {
        if ((options.updateMask & OcctViewerDisplayQualityUpdate_Precision) != 0)
        {
            const double angleRadians =
                options.deviationAngleDegrees * 3.14159265358979323846 / 180.0;
            const Handle(Prs3d_Drawer)& drawer = engine->viewerContext.context->DefaultDrawer();
            drawer->SetDeviationCoefficient(options.deviationCoefficient);
            drawer->SetDeviationAngle(angleRadians);
            if (options.applyPrecisionToExisting != 0)
            {
                for (auto& pair : engine->scene.objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    engine->viewerContext.context->SetDeviationCoefficient(
                        pair.second.presentation,
                        options.deviationCoefficient,
                        Standard_False);
                    engine->viewerContext.context->SetDeviationAngle(
                        pair.second.presentation,
                        angleRadians,
                        Standard_False);
                    engine->viewerContext.context->Redisplay(
                        pair.second.presentation,
                        Standard_False,
                        Standard_True);
                }
            }
        }

        if ((options.updateMask & OcctViewerDisplayQualityUpdate_DefaultMaterial) != 0)
        {
            const Graphic3d_MaterialAspect aspect(materialName(options.material));
            const Handle(Prs3d_Drawer)& drawer = engine->viewerContext.context->DefaultDrawer();
            drawer->SetupOwnShadingAspect();
            drawer->ShadingAspect()->SetMaterial(aspect);
            if (options.applyMaterialToExisting != 0)
            {
                for (auto& pair : engine->scene.objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    engine->viewerContext.context->SetMaterial(
                        pair.second.presentation,
                        aspect,
                        Standard_False);
                }
            }
        }
        engine->requestRedraw();
    }

    void validateNavigationOptions(const OcctViewerNavigationOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Navigation options are null.");
        if (options->structSize < sizeof(OcctViewerNavigationOptions) ||
            options->apiVersion != NavigationOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported navigation options size or version.");
        }
        if (options->action < OcctViewerNavigation_StartRotation ||
            options->action > OcctViewerNavigation_Zoom)
        {
            throw std::invalid_argument("Navigation action is out of range.");
        }
        if (options->action == OcctViewerNavigation_Zoom)
            requirePositive(options->factor, "Zoom factor");
    }
}

extern "C"
{
    OcctStatus occt_engine_view_state_update(
        OcctEngineHandle handle,
        const OcctViewerViewStateOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewStatus(engine, [&]
        {
            validateViewOptions(options);
            applyViewOptions(engine, *options);
        });
    }

    OcctStatus occt_engine_view_display_quality_update(
        OcctEngineHandle handle,
        const OcctViewerDisplayQualityOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewStatus(engine, [&]
        {
            validateDisplayQualityOptions(options);
            applyDisplayQualityOptions(engine, *options);
        });
    }

    OcctStatus occt_engine_view_fit_all(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewStatus(engine, [&] { engine->requestFitAll(); });
    }

    OcctStatus occt_engine_view_fit_object(OcctEngineHandle handle, OcctObjectId objectId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewStatus(engine, [&]
        {
            ObjectEntry* entry = engine->findShape(objectId);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            Bnd_Box box;
            BRepBndLib::Add(shapeWithPresentationTransformation(*entry), box);
            if (box.IsVoid()) throw std::runtime_error("Shape has no finite bounds.");
            engine->viewerContext.view->FitAll(box, 0.05, Standard_False);
            engine->viewerContext.view->ZFitAll();
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_view_window_fit(
        OcctEngineHandle handle,
        int x1,
        int y1,
        int x2,
        int y2)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewStatus(engine, [&]
        {
            engine->viewerContext.view->WindowFit(x1, y1, x2, y2);
        });
    }

    OcctStatus occt_engine_view_screen_to_world(
        OcctEngineHandle handle,
        int x,
        int y,
        OcctPoint3d* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "World point output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeViewStatus(engine, [&]
        {
            engine->viewerContext.view->Convert(x, y, result->x, result->y, result->z);
        });
    }

    OcctStatus occt_engine_view_world_to_screen(
        OcctEngineHandle handle,
        OcctPoint3d worldPoint,
        int* x,
        int* y)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (x == nullptr || y == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Screen coordinate output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeViewStatus(engine, [&]
        {
            requireFinitePoint(worldPoint, "World point");
            engine->viewerContext.view->Convert(worldPoint.x, worldPoint.y, worldPoint.z, *x, *y);
        });
    }

    OcctStatus occt_engine_view_navigation(
        OcctEngineHandle handle,
        const OcctViewerNavigationOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewStatus(engine, [&]
        {
            validateNavigationOptions(options);
            switch (options->action)
            {
                case OcctViewerNavigation_StartRotation:
                    engine->viewerContext.view->StartRotation(options->x, options->y, 0.4);
                    break;
                case OcctViewerNavigation_Rotation:
                    engine->viewerContext.view->Rotation(options->x, options->y);
                    break;
                case OcctViewerNavigation_Pan:
                    engine->viewerContext.view->Pan(options->deltaX, options->deltaY);
                    break;
                case OcctViewerNavigation_Zoom:
                    engine->viewerContext.view->SetZoom(options->factor, Standard_True);
                    break;
                default:
                    throw std::invalid_argument("Navigation action is out of range.");
            }
        });
    }

    OcctStatus occt_engine_view_dump(OcctEngineHandle handle, const char* utf8Path)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewStatus(engine, [&]
        {
            const auto path = pathFromUtf8(utf8Path);
            if (path.empty()) throw std::invalid_argument("Path is empty.");
            if (!engine->viewerContext.view->Dump(path.string().c_str()))
                throw std::runtime_error("View image export failed.");
        });
    }

    OcctStatus occt_engine_view_camera_get(
        OcctEngineHandle handle,
        OcctCameraState* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Camera state output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeViewStatus(engine, [&]
        {
            const Handle(Graphic3d_Camera)& camera = engine->viewerContext.view->Camera();
            const gp_Pnt eye = camera->Eye();
            const gp_Pnt center = camera->Center();
            const gp_Dir up = camera->Up();
            const gp_Dir cameraDirection = camera->Direction();
            result->eye = {eye.X(), eye.Y(), eye.Z()};
            result->center = {center.X(), center.Y(), center.Z()};
            result->up = {up.X(), up.Y(), up.Z()};
            result->direction = {cameraDirection.X(), cameraDirection.Y(), cameraDirection.Z()};
            result->scale = camera->Scale();
        });
    }

    OcctStatus occt_engine_view_camera_set(
        OcctEngineHandle handle,
        const OcctCameraState* state)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (state == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Camera state is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeViewStatus(engine, [&]
        {
            requireFinitePoint(state->eye, "Camera eye");
            requireFinitePoint(state->center, "Camera center");
            requirePositive(state->scale, "Camera scale");
            const Handle(Graphic3d_Camera)& camera = engine->viewerContext.view->Camera();
            camera->SetEyeAndCenter(point(state->eye), point(state->center));
            camera->SetUp(direction(state->up));
            camera->OrthogonalizeUp();
            camera->SetScale(state->scale);
            engine->requestRedraw();
        });
    }
}