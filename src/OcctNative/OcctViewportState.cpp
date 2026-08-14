#include "presentation/OcctViewport.h"
#include "core/OcctInternal.hxx"

#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_RenderingParams.hxx>
#include <Prs3d_Drawer.hxx>

#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t ViewportStateApiVersion = 1;
    constexpr std::uint32_t AllResetBits =
        OcctViewportReset_All |
        OcctViewportReset_Orientation |
        OcctViewportReset_Mapping;

    void validateMargin(double margin)
    {
        if (!std::isfinite(margin) || margin < 0.0 || margin >= 1.0)
            throw std::invalid_argument("Fit margin must be in the range [0, 1).");
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeViewportStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    void fillViewportState(Engine* engine, OcctViewportState& result)
    {
        Standard_Integer width = 0;
        Standard_Integer height = 0;
        engine->viewerContext.window->Size(width, height);

        const Handle(Graphic3d_Camera)& camera = engine->viewerContext.view->Camera();
        const Graphic3d_RenderingParams& rendering = engine->viewerContext.view->RenderingParams();
        result.width = width;
        result.height = height;
        result.projectionType = camera->ProjectionType() == Graphic3d_Camera::Projection_Perspective
            ? OcctProjection_Perspective
            : OcctProjection_Orthographic;
        result.computedMode = engine->viewerContext.view->ComputedMode() ? 1 : 0;
        result.antialiasingEnabled = rendering.IsAntialiasingEnabled ? 1 : 0;
        result.msaaSamples = rendering.NbMsaaSamples;
        result.renderingMethod = rendering.Method == Graphic3d_RM_RAYTRACING
            ? OcctRendering_RayTracing
            : OcctRendering_Rasterization;
        result.shadowsEnabled = rendering.IsShadowEnabled ? 1 : 0;
        result.frustumCullingEnabled = engine->viewerContext.view->IsCullingEnabled() ? 1 : 0;
        result.faceBoundariesVisible = engine->viewerContext.context->DefaultDrawer()->FaceBoundaryDraw() ? 1 : 0;
        result.selectionTolerance = engine->viewerContext.context->PixelTolerance();
        result.automaticHighlight = engine->viewerContext.context->AutomaticHilight() ? 1 : 0;
        result.perspectiveFov = camera->FOVy();
        result.renderResolutionScale = rendering.RenderResolutionScale;
        result.renderResolutionDpi = rendering.Resolution;
    }
}

extern "C"
{
    OcctStatus occt_engine_viewport_state_get(
        OcctEngineHandle handle,
        OcctViewportStateResult* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Viewport state result is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeViewportStatus(engine, [&]
        {
            result->structSize = static_cast<std::uint32_t>(sizeof(OcctViewportStateResult));
            result->apiVersion = ViewportStateApiVersion;
            fillViewportState(engine, result->state);
        });
    }

    OcctStatus occt_engine_viewport_reset(
        OcctEngineHandle handle,
        std::uint32_t resetMask)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportStatus(engine, [&]
        {
            if (resetMask == 0 || (resetMask & ~AllResetBits) != 0)
                throw std::invalid_argument("Viewport reset mask is invalid.");
            if ((resetMask & OcctViewportReset_All) != 0 &&
                resetMask != OcctViewportReset_All)
            {
                throw std::invalid_argument("Viewport full reset cannot be combined with partial reset bits.");
            }

            if ((resetMask & OcctViewportReset_All) != 0)
            {
                engine->viewerContext.view->Reset(Standard_True);
                return;
            }
            if ((resetMask & OcctViewportReset_Orientation) != 0)
                engine->viewerContext.view->ResetViewOrientation();
            if ((resetMask & OcctViewportReset_Mapping) != 0)
                engine->viewerContext.view->ResetViewMapping();
            engine->viewerContext.view->Redraw();
        });
    }

    OcctStatus occt_engine_viewport_fit_selected(
        OcctEngineHandle handle,
        double margin)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeViewportStatus(engine, [&]
        {
            validateMargin(margin);
            Bnd_Box bounds;
            int shapeCount = 0;
            for (engine->viewerContext.context->InitSelected();
                 engine->viewerContext.context->MoreSelected();
                 engine->viewerContext.context->NextSelected())
            {
                const OcctObjectId id =
                    engine->findPresentation(engine->viewerContext.context->SelectedInteractive());
                const ObjectEntry* entry = engine->findShape(id);
                if (entry == nullptr) continue;
                BRepBndLib::Add(shapeWithPresentationTransformation(*entry), bounds);
                ++shapeCount;
            }
            if (shapeCount == 0 || bounds.IsVoid())
                throw std::runtime_error("The current selection has no finite shape bounds.");
            engine->viewerContext.view->FitAll(bounds, margin, Standard_False);
            engine->viewerContext.view->ZFitAll();
            engine->viewerContext.view->Redraw();
        });
    }

    OcctStatus occt_engine_viewport_gravity_point_get(
        OcctEngineHandle handle,
        OcctPoint3d* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Gravity point result is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeViewportStatus(engine, [&]
        {
            Bnd_Box bounds;
            for (const auto& pair : engine->scene.objects)
            {
                const ObjectEntry& entry = pair.second;
                if (entry.kind != OcctObject_Shape ||
                    entry.presentation.IsNull() ||
                    !engine->viewerContext.context->IsDisplayed(entry.presentation))
                {
                    continue;
                }
                BRepBndLib::Add(shapeWithPresentationTransformation(entry), bounds);
            }
            if (bounds.IsVoid()) throw std::runtime_error("The scene has no finite shape bounds.");
            double minX, minY, minZ, maxX, maxY, maxZ;
            bounds.Get(minX, minY, minZ, maxX, maxY, maxZ);
            *result = {
                (minX + maxX) * 0.5,
                (minY + maxY) * 0.5,
                (minZ + maxZ) * 0.5 };
        });
    }
}
