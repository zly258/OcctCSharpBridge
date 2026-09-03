#include "presentation/OcctDepth.h"
#include "core/OcctInternal.hxx"

#include <Aspect_PolygonOffsetMode.hxx>
#include <Graphic3d_AspectFillArea3d.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>

#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t DepthApiVersion = 1;
    constexpr std::uint32_t AllDepthUpdateBits =
        OcctViewerDepthUpdate_AutoZFitSettings |
        OcctViewerDepthUpdate_AutoZFitNow |
        OcctViewerDepthUpdate_DefaultPolygonOffsets;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeDepthStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist or has no presentation.");
        return *entry;
    }

    void validatePolygonOffset(int mode, double factor, double units)
    {
        if (mode < static_cast<int>(Aspect_POM_Off)
            || (mode & ~static_cast<int>(Aspect_POM_All)) != 0)
        {
            throw std::invalid_argument("Polygon offset mode is out of range.");
        }
        if (!std::isfinite(factor) || !std::isfinite(units))
            throw std::invalid_argument("Polygon offset factor and units must be finite.");
    }

    void readDefaultPolygonOffset(
        Engine* engine,
        Standard_Integer& mode,
        Standard_ShortReal& factor,
        Standard_ShortReal& units)
    {
        const Handle(Prs3d_Drawer)& drawer = engine->viewerContext.context->DefaultDrawer();
        if (drawer.IsNull() || drawer->ShadingAspect().IsNull()
            || drawer->ShadingAspect()->Aspect().IsNull())
        {
            mode = Aspect_POM_Fill;
            factor = 1.0f;
            units = 1.0f;
            return;
        }
        drawer->ShadingAspect()->Aspect()->PolygonOffsets(mode, factor, units);
    }

    void redisplayObject(Engine* engine, const Handle(AIS_InteractiveObject)& presentation)
    {
        engine->viewerContext.context->Redisplay(presentation, Standard_False, Standard_True);
    }

    void validateDepthUpdate(const OcctViewerDepthUpdateOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Depth update options are null.");
        if (options->structSize < sizeof(OcctViewerDepthUpdateOptions) ||
            options->apiVersion != DepthApiVersion)
        {
            throw std::invalid_argument("Unsupported depth update options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllDepthUpdateBits) != 0)
            throw std::invalid_argument("Depth update mask is invalid.");
        if ((options->updateMask & OcctViewerDepthUpdate_AutoZFitSettings) != 0 &&
            (!std::isfinite(options->autoZFitScaleFactor) || options->autoZFitScaleFactor <= 0.0))
        {
            throw std::invalid_argument("Auto Z-fit scale factor must be finite and greater than zero.");
        }
        if ((options->updateMask & OcctViewerDepthUpdate_DefaultPolygonOffsets) != 0)
        {
            validatePolygonOffset(
                options->polygonOffsetMode,
                options->polygonOffsetFactor,
                options->polygonOffsetUnits);
        }
    }

    void validateObjectPolygonUpdate(const OcctViewerObjectPolygonOffsetOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Object polygon offset options are null.");
        if (options->structSize < sizeof(OcctViewerObjectPolygonOffsetOptions) ||
            options->apiVersion != DepthApiVersion)
        {
            throw std::invalid_argument("Unsupported object polygon offset options size or version.");
        }
        if (options->resetToDefault == 0)
            validatePolygonOffset(options->mode, options->factor, options->units);
    }
}

extern "C"
{
    OcctStatus occt_engine_depth_update(
        OcctEngineHandle handle,
        const OcctViewerDepthUpdateOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDepthStatus(engine, [&]
        {
            validateDepthUpdate(options);

            if ((options->updateMask & OcctViewerDepthUpdate_AutoZFitSettings) != 0)
            {
                engine->viewerContext.view->SetAutoZFitMode(
                    options->autoZFitEnabled != 0,
                    options->autoZFitScaleFactor);
                if (options->autoZFitEnabled != 0) engine->viewerContext.view->AutoZFit();
            }

            if ((options->updateMask & OcctViewerDepthUpdate_AutoZFitNow) != 0)
                engine->viewerContext.view->AutoZFit();

            if ((options->updateMask & OcctViewerDepthUpdate_DefaultPolygonOffsets) != 0)
            {
                const Standard_ShortReal nativeFactor =
                    static_cast<Standard_ShortReal>(options->polygonOffsetFactor);
                const Standard_ShortReal nativeUnits =
                    static_cast<Standard_ShortReal>(options->polygonOffsetUnits);
                const Handle(Prs3d_Drawer)& drawer = engine->viewerContext.context->DefaultDrawer();
                drawer->SetupOwnShadingAspect();
                drawer->ShadingAspect()->Aspect()->SetPolygonOffsets(
                    options->polygonOffsetMode,
                    nativeFactor,
                    nativeUnits);

                if (options->applyPolygonOffsetsToExisting != 0)
                {
                    for (auto& pair : engine->scene.objects)
                    {
                        if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                        engine->viewerContext.context->SetPolygonOffsets(
                            pair.second.presentation,
                            options->polygonOffsetMode,
                            nativeFactor,
                            nativeUnits,
                            Standard_False);
                        redisplayObject(engine, pair.second.presentation);
                    }
                }
            }

            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_depth_state_get(
        OcctEngineHandle handle,
        OcctViewerDepthState* state)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDepthStatus(engine, [&]
        {
            if (state == nullptr) throw std::invalid_argument("Depth state output is null.");
            Standard_Integer mode = Aspect_POM_Fill;
            Standard_ShortReal factor = 1.0f;
            Standard_ShortReal units = 1.0f;
            readDefaultPolygonOffset(engine, mode, factor, units);

            state->structSize = static_cast<std::uint32_t>(sizeof(OcctViewerDepthState));
            state->apiVersion = DepthApiVersion;
            state->autoZFitEnabled = engine->viewerContext.view->AutoZFitMode() ? 1 : 0;
            state->autoZFitScaleFactor = engine->viewerContext.view->AutoZFitScaleFactor();
            state->polygonOffsetMode = mode;
            state->polygonOffsetFactor = static_cast<double>(factor);
            state->polygonOffsetUnits = static_cast<double>(units);
        });
    }

    OcctStatus occt_engine_object_polygon_offset_update(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerObjectPolygonOffsetOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDepthStatus(engine, [&]
        {
            validateObjectPolygonUpdate(options);
            ObjectEntry& entry = requiredObject(engine, objectId);

            Standard_Integer mode = options->mode;
            Standard_ShortReal factor = static_cast<Standard_ShortReal>(options->factor);
            Standard_ShortReal units = static_cast<Standard_ShortReal>(options->units);
            if (options->resetToDefault != 0)
                readDefaultPolygonOffset(engine, mode, factor, units);

            engine->viewerContext.context->SetPolygonOffsets(
                entry.presentation,
                mode,
                factor,
                units,
                Standard_False);
            redisplayObject(engine, entry.presentation);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_polygon_offset_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctViewerObjectPolygonOffsetState* state)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDepthStatus(engine, [&]
        {
            if (state == nullptr) throw std::invalid_argument("Object polygon offset state output is null.");
            ObjectEntry& entry = requiredObject(engine, objectId);
            Standard_Integer mode = Aspect_POM_Fill;
            Standard_ShortReal factor = 1.0f;
            Standard_ShortReal units = 1.0f;
            if (engine->viewerContext.context->HasPolygonOffsets(entry.presentation))
                engine->viewerContext.context->PolygonOffsets(entry.presentation, mode, factor, units);
            else
                readDefaultPolygonOffset(engine, mode, factor, units);

            state->structSize = static_cast<std::uint32_t>(sizeof(OcctViewerObjectPolygonOffsetState));
            state->apiVersion = DepthApiVersion;
            state->mode = mode;
            state->factor = static_cast<double>(factor);
            state->units = static_cast<double>(units);
        });
    }
}
