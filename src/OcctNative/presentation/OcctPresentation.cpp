#include "presentation/OcctPresentation.h"
#include "core/OcctInternal.hxx"

#include <Aspect_TypeOfLine.hxx>
#include <Graphic3d_ClipPlane.hxx>
#include <Graphic3d_SequenceOfHClipPlane.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_LineAspect.hxx>
#include <Prs3d_TypeOfHighlight.hxx>
#include <gp_Pln.hxx>

#include <algorithm>
#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t PresentationApiVersion = 1;
    constexpr std::uint32_t AllPresentationStateUpdateBits =
        OcctViewerPresentationStateUpdate_DisplayMode |
        OcctViewerPresentationStateUpdate_ResetDisplayMode |
        OcctViewerPresentationStateUpdate_AutoHighlight |
        OcctViewerPresentationStateUpdate_Infinite;

    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    Graphic3d_ZLayerId zLayer(int value)
    {
        switch (value)
        {
            case OcctPresentationZLayer_Bottom: return Graphic3d_ZLayerId_BotOSD;
            case OcctPresentationZLayer_Default: return Graphic3d_ZLayerId_Default;
            case OcctPresentationZLayer_Top: return Graphic3d_ZLayerId_Top;
            case OcctPresentationZLayer_Topmost: return Graphic3d_ZLayerId_Topmost;
            default: throw std::invalid_argument("Highlight Z-layer is out of range.");
        }
    }

    Prs3d_TypeOfHighlight highlightKind(int value)
    {
        switch (value)
        {
            case 0: return Prs3d_TypeOfHighlight_Dynamic;
            case 1: return Prs3d_TypeOfHighlight_Selected;
            case 2: return Prs3d_TypeOfHighlight_LocalDynamic;
            case 3: return Prs3d_TypeOfHighlight_LocalSelected;
            default: throw std::invalid_argument("Highlight style kind is out of range.");
        }
    }

    void validateHighlightStyle(const OcctHighlightStyleSettings& settings)
    {
        (void)color(settings.r, settings.g, settings.b);
        if (!std::isfinite(settings.transparency) ||
            settings.transparency < 0.0 || settings.transparency > 1.0)
        {
            throw std::invalid_argument("Highlight transparency must be between 0 and 1.");
        }
        requirePositive(settings.lineWidth, "Highlight line width");
        if (settings.displayMode < -1 || settings.displayMode > 1)
            throw std::invalid_argument("Highlight display mode is out of range.");
        if (settings.zLayer < -1 || settings.zLayer > OcctPresentationZLayer_Topmost)
            throw std::invalid_argument("Highlight Z-layer is out of range.");
    }

    void applyHighlightStyle(
        const Handle(Prs3d_Drawer)& drawer,
        const OcctHighlightStyleSettings& settings)
    {
        if (drawer.IsNull()) throw std::runtime_error("Highlight drawer is null.");
        validateHighlightStyle(settings);
        const Quantity_Color value = color(settings.r, settings.g, settings.b);
        drawer->SetColor(value);
        drawer->SetTransparency(static_cast<float>(settings.transparency));
        drawer->SetLineAspect(new Prs3d_LineAspect(value, Aspect_TOL_SOLID, settings.lineWidth));
        drawer->SetDisplayMode(settings.displayMode);
        drawer->SetZLayer(settings.zLayer >= 0 ? zLayer(settings.zLayer) : Graphic3d_ZLayerId_UNKNOWN);
    }

    Handle(Graphic3d_SequenceOfHClipPlane) buildClipPlanes(
        const OcctPresentationClipPlane* planes,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Clip plane count must not be negative.");
        if (count > 0 && planes == nullptr) throw std::invalid_argument("Clip plane array is null.");

        Handle(Graphic3d_SequenceOfHClipPlane) sequence = new Graphic3d_SequenceOfHClipPlane();
        for (int index = 0; index < count; ++index)
        {
            const OcctPresentationClipPlane& source = planes[index];
            (void)color(source.cappingR, source.cappingG, source.cappingB);
            Handle(Graphic3d_ClipPlane) plane =
                new Graphic3d_ClipPlane(gp_Pln(point(source.point), direction(source.normal)));
            plane->SetOn(source.enabled != 0);
            plane->SetCapping(source.capping != 0);
            plane->SetCappingColor(color(source.cappingR, source.cappingG, source.cappingB));
            sequence->Append(plane);
        }
        return sequence;
    }

    int availableObjectClipPlanes(Engine* engine)
    {
        int viewPlaneCount = 0;
        const Handle(Graphic3d_SequenceOfHClipPlane)& viewPlanes =
            engine->viewerContext.view->ClipPlanes();
        if (!viewPlanes.IsNull()) viewPlaneCount = viewPlanes->Size();
        return std::max(0, engine->viewerContext.view->PlaneLimit() - viewPlaneCount);
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executePresentationStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    void validateStateOptions(const OcctViewerPresentationStateOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Presentation state options are null.");
        if (options->structSize < sizeof(OcctViewerPresentationStateOptions) ||
            options->apiVersion != PresentationApiVersion)
        {
            throw std::invalid_argument("Unsupported presentation state options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllPresentationStateUpdateBits) != 0)
            throw std::invalid_argument("Presentation state update mask is invalid.");
        if ((options->updateMask & OcctViewerPresentationStateUpdate_DisplayMode) != 0 &&
            (options->updateMask & OcctViewerPresentationStateUpdate_ResetDisplayMode) != 0)
        {
            throw std::invalid_argument("Display mode set and reset cannot be requested together.");
        }
        if ((options->updateMask & OcctViewerPresentationStateUpdate_DisplayMode) != 0 &&
            options->displayMode != OcctDisplay_Wireframe &&
            options->displayMode != OcctDisplay_Shaded)
        {
            throw std::invalid_argument("Object display mode is out of range.");
        }
    }

    void validateClipPlaneOptions(const OcctViewerClipPlanesOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Clip plane options are null.");
        if (options->structSize < sizeof(OcctViewerClipPlanesOptions) ||
            options->apiVersion != PresentationApiVersion)
        {
            throw std::invalid_argument("Unsupported clip plane options size or version.");
        }
        if (options->count < 0)
            throw std::invalid_argument("Clip plane count must not be negative.");
        if (options->count > 0 && options->planes == nullptr)
            throw std::invalid_argument("Clip plane array is null.");
    }

    void validateHighlightOptions(const OcctViewerHighlightStyleOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Highlight style options are null.");
        if (options->structSize < sizeof(OcctViewerHighlightStyleOptions) ||
            options->apiVersion != PresentationApiVersion)
        {
            throw std::invalid_argument("Unsupported highlight style options size or version.");
        }
        (void)highlightKind(options->kind);
        validateHighlightStyle(options->settings);
    }
}

extern "C"
{
    OcctStatus occt_engine_presentation_state_update(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerPresentationStateOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePresentationStatus(engine, [&]
        {
            validateStateOptions(options);
            ObjectEntry& entry = requiredObject(engine, objectId);

            if ((options->updateMask & OcctViewerPresentationStateUpdate_DisplayMode) != 0)
            {
                engine->viewerContext.context->SetDisplayMode(
                    entry.presentation,
                    options->displayMode == OcctDisplay_Wireframe ? AIS_WireFrame : AIS_Shaded,
                    Standard_False);
            }
            if ((options->updateMask & OcctViewerPresentationStateUpdate_ResetDisplayMode) != 0)
                engine->viewerContext.context->UnsetDisplayMode(entry.presentation, Standard_False);
            if ((options->updateMask & OcctViewerPresentationStateUpdate_AutoHighlight) != 0)
            {
                const Standard_Boolean requested = options->autoHighlight != 0;
                entry.presentation->SetAutoHilight(requested);
                if (entry.presentation->IsAutoHilight() != requested)
                    throw std::invalid_argument("Object does not support the requested AutoHighlight state.");
            }
            if ((options->updateMask & OcctViewerPresentationStateUpdate_Infinite) != 0)
            {
                entry.presentation->SetInfiniteState(options->infinite != 0);
                engine->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_presentation_state_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctViewerPresentationState* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePresentationStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Presentation state result is null.");
            const ObjectEntry& entry = requiredObject(engine, objectId);
            result->structSize = static_cast<std::uint32_t>(sizeof(OcctViewerPresentationState));
            result->apiVersion = PresentationApiVersion;
            result->hasDisplayModeOverride = entry.presentation->HasDisplayMode() ? 1 : 0;
            result->displayMode = -1;
            if (result->hasDisplayModeOverride != 0)
            {
                const int nativeMode = entry.presentation->DisplayMode();
                if (nativeMode == AIS_WireFrame) result->displayMode = OcctDisplay_Wireframe;
                else if (nativeMode == AIS_Shaded) result->displayMode = OcctDisplay_Shaded;
                else result->displayMode = nativeMode;
            }
            result->autoHighlight = entry.presentation->IsAutoHilight() ? 1 : 0;
            result->infinite = entry.presentation->IsInfinite() ? 1 : 0;
        });
    }

    OcctStatus occt_engine_presentation_clip_planes_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerClipPlanesOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePresentationStatus(engine, [&]
        {
            validateClipPlaneOptions(options);
            if (options->count > availableObjectClipPlanes(engine))
            {
                throw std::invalid_argument(
                    "Object clip plane count exceeds the remaining view plane limit.");
            }
            ObjectEntry& entry = requiredObject(engine, objectId);
            entry.presentation->SetClipPlanes(buildClipPlanes(options->planes, options->count));
            engine->viewerContext.context->Redisplay(entry.presentation, Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_highlight_style_global_set(
        OcctEngineHandle handle,
        const OcctViewerHighlightStyleOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePresentationStatus(engine, [&]
        {
            validateHighlightOptions(options);
            const Prs3d_TypeOfHighlight type = highlightKind(options->kind);
            applyHighlightStyle(engine->viewerContext.context->HighlightStyle(type), options->settings);
            if (type == Prs3d_TypeOfHighlight_Selected ||
                type == Prs3d_TypeOfHighlight_LocalSelected)
            {
                engine->viewerContext.context->UpdateSelected(Standard_False);
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_highlight_style_object_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerHighlightStyleOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePresentationStatus(engine, [&]
        {
            validateHighlightOptions(options);
            ObjectEntry& entry = requiredObject(engine, objectId);
            Handle(Prs3d_Drawer) drawer = new Prs3d_Drawer();
            drawer->SetLink(engine->viewerContext.context->DefaultDrawer());
            applyHighlightStyle(drawer, options->settings);
            if (options->dynamic != 0) entry.presentation->SetDynamicHilightAttributes(drawer);
            else entry.presentation->SetHilightAttributes(drawer);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_highlight_style_object_clear(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int dynamic)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePresentationStatus(engine, [&]
        {
            ObjectEntry& entry = requiredObject(engine, objectId);
            Handle(Prs3d_Drawer) empty;
            if (dynamic != 0) entry.presentation->SetDynamicHilightAttributes(empty);
            else entry.presentation->SetHilightAttributes(empty);
            engine->requestRedraw();
        });
    }
}
