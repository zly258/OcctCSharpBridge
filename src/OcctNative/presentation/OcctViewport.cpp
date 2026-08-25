#include "presentation/OcctViewport.h"
#include "core/OcctInternal.hxx"

#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <Graphic3d_RenderingParams.hxx>
#include <Precision.hxx>
#include <Prs3d_Drawer.hxx>
#include <V3d_TypeOfOrientation.hxx>

#include <algorithm>
#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t RenderingOptionsApiVersion = 1;
    constexpr std::uint32_t AllRenderingUpdateBits =
        OcctViewportRenderingUpdate_MsaaSamples |
        OcctViewportRenderingUpdate_ResolutionScale |
        OcctViewportRenderingUpdate_ResolutionDpi |
        OcctViewportRenderingUpdate_Method |
        OcctViewportRenderingUpdate_Shadows |
        OcctViewportRenderingUpdate_ImmediateUpdate |
        OcctViewportRenderingUpdate_FrustumCulling |
        OcctViewportRenderingUpdate_FaceBoundaries;

    V3d_TypeOfOrientation zUpOrientation(int value)
    {
        switch (value)
        {
            case OcctZUp_Front: return V3d_TypeOfOrientation_Zup_Front;
            case OcctZUp_Back: return V3d_TypeOfOrientation_Zup_Back;
            case OcctZUp_Left: return V3d_TypeOfOrientation_Zup_Left;
            case OcctZUp_Right: return V3d_TypeOfOrientation_Zup_Right;
            case OcctZUp_Top: return V3d_TypeOfOrientation_Zup_Top;
            case OcctZUp_Bottom: return V3d_TypeOfOrientation_Zup_Bottom;
            case OcctZUp_XNegativeYNegative: return V3d_XnegYnegZpos;
            case OcctZUp_XPositiveYNegative: return V3d_XposYnegZpos;
            case OcctZUp_XNegativeYPositive: return V3d_XnegYposZpos;
            case OcctZUp_XPositiveYPositive: return V3d_XposYposZpos;
            default: throw std::invalid_argument("Z-up view orientation is out of range.");
        }
    }

    void validateMargin(double margin)
    {
        if (!std::isfinite(margin) || margin < 0.0 || margin >= 1.0)
            throw std::invalid_argument("Fit margin must be in the range [0, 1).");
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeViewportStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    void validateRenderingOptions(const OcctViewportRenderingOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Viewport rendering options are null.");
        if (options->structSize < sizeof(OcctViewportRenderingOptions) ||
            options->apiVersion != RenderingOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported viewport rendering options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllRenderingUpdateBits) != 0)
            throw std::invalid_argument("Viewport rendering update mask is invalid.");

        if ((options->updateMask & OcctViewportRenderingUpdate_MsaaSamples) != 0 &&
            (options->msaaSamples < 0 || options->msaaSamples > 16))
        {
            throw std::invalid_argument("MSAA sample count must be between 0 and 16.");
        }
        if ((options->updateMask & OcctViewportRenderingUpdate_ResolutionScale) != 0 &&
            (!std::isfinite(options->resolutionScale) || options->resolutionScale < 0.25 || options->resolutionScale > 4.0))
        {
            throw std::invalid_argument("Render resolution scale must be between 0.25 and 4.0.");
        }
        if ((options->updateMask & OcctViewportRenderingUpdate_ResolutionDpi) != 0 &&
            (!std::isfinite(options->resolutionDpi) || options->resolutionDpi < 36.0 || options->resolutionDpi > 600.0))
        {
            throw std::invalid_argument("Render resolution must be between 36 and 600 DPI.");
        }
        if ((options->updateMask & OcctViewportRenderingUpdate_Method) != 0 &&
            options->renderingMethod != OcctRendering_Rasterization &&
            options->renderingMethod != OcctRendering_RayTracing)
        {
            throw std::invalid_argument("Rendering method is out of range.");
        }
    }

    void applyRenderingOptions(Engine* engine, const OcctViewportRenderingOptions& options)
    {
        Graphic3d_RenderingParams& rendering = engine->viewerContext.view->ChangeRenderingParams();

        if ((options.updateMask & OcctViewportRenderingUpdate_MsaaSamples) != 0)
            rendering.NbMsaaSamples = options.msaaSamples;
        if ((options.updateMask & OcctViewportRenderingUpdate_ResolutionScale) != 0)
            rendering.RenderResolutionScale = static_cast<Standard_ShortReal>(options.resolutionScale);
        if ((options.updateMask & OcctViewportRenderingUpdate_ResolutionDpi) != 0)
            rendering.Resolution = static_cast<unsigned int>(std::lround(options.resolutionDpi));
        if ((options.updateMask & OcctViewportRenderingUpdate_Method) != 0)
        {
            rendering.Method = options.renderingMethod == OcctRendering_RayTracing
                ? Graphic3d_RM_RAYTRACING
                : Graphic3d_RM_RASTERIZATION;
        }
        if ((options.updateMask & OcctViewportRenderingUpdate_Shadows) != 0)
            rendering.IsShadowEnabled = options.shadowsEnabled != 0;
        if ((options.updateMask & OcctViewportRenderingUpdate_ImmediateUpdate) != 0)
            engine->viewerContext.view->SetImmediateUpdate(options.immediateUpdate != 0);
        if ((options.updateMask & OcctViewportRenderingUpdate_FrustumCulling) != 0)
            engine->viewerContext.view->SetFrustumCulling(options.frustumCullingEnabled != 0);
        if ((options.updateMask & OcctViewportRenderingUpdate_FaceBoundaries) != 0)
        {
            const bool visible = options.faceBoundariesVisible != 0;
            engine->viewerContext.context->DefaultDrawer()->SetFaceBoundaryDraw(visible);
            if (options.applyFaceBoundariesToExisting != 0)
            {
                for (auto& pair : engine->scene.objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    pair.second.presentation->Attributes()->SetFaceBoundaryDraw(visible);
                    engine->viewerContext.context->Redisplay(
                        pair.second.presentation,
                        Standard_False,
                        Standard_True);
                }
            }
        }
        engine->requestRedraw();
    }
}

extern "C"
{
    OcctStatus occt_engine_viewport_fit_objects(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count,
        double margin)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportStatus(engine, [&]
        {
            if (objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");
            requireCount(count, 1, "Object ID array");
            validateMargin(margin);

            Bnd_Box bounds;
            for (int index = 0; index < count; ++index)
            {
                const ObjectEntry* entry = engine->findShape(objectIds[index]);
                if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
                BRepBndLib::Add(shapeWithPresentationTransformation(*entry), bounds);
            }
            if (bounds.IsVoid()) throw std::runtime_error("Selected shapes have no finite bounds.");
            engine->viewerContext.view->FitAll(bounds, margin, Standard_False);
            engine->viewerContext.view->ZFitAll();
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_viewport_zup_set(
        OcctEngineHandle handle,
        int orientation,
        int fitAll)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportStatus(engine, [&]
        {
            engine->viewerContext.view->SetProj(zUpOrientation(orientation));
            if (fitAll != 0)
            {
                engine->viewerContext.view->FitAll(0.01, Standard_False);
                engine->viewerContext.view->ZFitAll();
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_viewport_screen_to_ray(
        OcctEngineHandle handle,
        int x,
        int y,
        OcctProjectionRay* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Projection ray result is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeViewportStatus(engine, [&]
        {
            Standard_Real px = 0.0, py = 0.0, pz = 0.0;
            Standard_Real vx = 0.0, vy = 0.0, vz = 0.0;
            engine->viewerContext.view->ConvertWithProj(x, y, px, py, pz, vx, vy, vz);
            const gp_Dir rayDirection(gp_Vec(vx, vy, vz));
            result->origin = {px, py, pz};
            result->direction = {rayDirection.X(), rayDirection.Y(), rayDirection.Z()};
        });
    }

    OcctStatus occt_engine_viewport_zoom_at_point(
        OcctEngineHandle handle,
        int x,
        int y,
        double delta)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportStatus(engine, [&]
        {
            if (!std::isfinite(delta) || std::abs(delta) <= Precision::Confusion())
                throw std::invalid_argument("Zoom delta must be finite and non-zero.");

            const double clampedDelta = std::clamp(delta, -10000.0, 10000.0);
            Standard_Integer zoomDelta = static_cast<Standard_Integer>(std::lround(clampedDelta));
            if (zoomDelta == 0) zoomDelta = delta > 0.0 ? 1 : -1;

            engine->viewerContext.view->StartZoomAtPoint(x, y);
            engine->viewerContext.view->ZoomAtPoint(0, 0, zoomDelta, 0);
        });
    }

    OcctStatus occt_engine_viewport_rendering_update(
        OcctEngineHandle handle,
        const OcctViewportRenderingOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportStatus(engine, [&]
        {
            validateRenderingOptions(options);
            applyRenderingOptions(engine, *options);
        });
    }
}
