#include "core/OcctInternal.hxx"
#include "OcctPresentation.h"
#include "OcctViewerInteraction.h"

#include <Aspect_TypeOfLine.hxx>
#include <Graphic3d_ClipPlane.hxx>
#include <Graphic3d_SequenceOfHClipPlane.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_LineAspect.hxx>
#include <Prs3d_TypeOfHighlight.hxx>
#include <gp_Pln.hxx>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t PresentationStateApiVersion = 1;
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
            case OcctViewerZLayer_Bottom: return Graphic3d_ZLayerId_BotOSD;
            case OcctViewerZLayer_Default: return Graphic3d_ZLayerId_Default;
            case OcctViewerZLayer_Top: return Graphic3d_ZLayerId_Top;
            case OcctViewerZLayer_Topmost: return Graphic3d_ZLayerId_Topmost;
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

    void applyHighlightStyle(
        const Handle(Prs3d_Drawer)& drawer,
        const OcctHighlightStyleSettings& settings)
    {
        if (drawer.IsNull()) throw std::runtime_error("Highlight drawer is null.");
        if (!std::isfinite(settings.transparency) || settings.transparency < 0.0 || settings.transparency > 1.0)
            throw std::invalid_argument("Highlight transparency must be between 0 and 1.");
        requirePositive(settings.lineWidth, "Highlight line width");
        if (settings.displayMode < -1 || settings.displayMode > 1)
            throw std::invalid_argument("Highlight display mode is out of range.");
        if (settings.zLayer < -1 || settings.zLayer > OcctViewerZLayer_Topmost)
            throw std::invalid_argument("Highlight Z-layer is out of range.");

        const Quantity_Color value = color(settings.r, settings.g, settings.b);
        drawer->SetColor(value);
        drawer->SetTransparency(static_cast<float>(settings.transparency));
        drawer->SetLineAspect(new Prs3d_LineAspect(value, Aspect_TOL_SOLID, settings.lineWidth));
        drawer->SetDisplayMode(settings.displayMode);
        drawer->SetZLayer(settings.zLayer >= 0 ? zLayer(settings.zLayer) : Graphic3d_ZLayerId_UNKNOWN);
    }

    Handle(Graphic3d_SequenceOfHClipPlane) clipPlanes(
        const OcctViewClipPlane* planes,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Clip plane count must not be negative.");
        if (count > 0 && planes == nullptr) throw std::invalid_argument("Clip plane array is null.");

        Handle(Graphic3d_SequenceOfHClipPlane) sequence = new Graphic3d_SequenceOfHClipPlane();
        for (int index = 0; index < count; ++index)
        {
            const OcctViewClipPlane& source = planes[index];
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
        const Handle(Graphic3d_SequenceOfHClipPlane)& viewPlanes = engine->viewerContext.view->ClipPlanes();
        if (!viewPlanes.IsNull()) viewPlaneCount = viewPlanes->Size();
        return std::max(0, engine->viewerContext.view->PlaneLimit() - viewPlaneCount);
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executePresentationStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    void validatePresentationStateOptions(const OcctViewerPresentationStateOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Presentation state options are null.");
        if (options->structSize < sizeof(OcctViewerPresentationStateOptions) ||
            options->apiVersion != PresentationStateApiVersion)
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

    OcctViewerPresentationStateOptions presentationStateOptions(
        std::uint32_t updateMask,
        int displayMode = OcctDisplay_Shaded,
        int autoHighlight = 0,
        int infinite = 0)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctViewerPresentationStateOptions)),
            PresentationStateApiVersion,
            updateMask,
            displayMode,
            autoHighlight,
            infinite };
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
            validatePresentationStateOptions(options);
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
            result->apiVersion = PresentationStateApiVersion;
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

    int occt_set_object_clip_planes(
        OcctHandle h,
        OcctObjectId objectId,
        const OcctViewClipPlane* planes,
        int count)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count > availableObjectClipPlanes(e))
                throw std::invalid_argument("Object clip plane count exceeds the remaining view plane limit.");
            ObjectEntry& entry = requiredObject(e, objectId);
            entry.presentation->SetClipPlanes(clipPlanes(planes, count));
            e->viewerContext.context->Redisplay(entry.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_global_highlight_style(
        OcctHandle h,
        int kind,
        const OcctHighlightStyleSettings* settings)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || settings == nullptr) return 0;
        return execute(e, [&]
        {
            const Prs3d_TypeOfHighlight type = highlightKind(kind);
            applyHighlightStyle(e->viewerContext.context->HighlightStyle(type), *settings);
            if (type == Prs3d_TypeOfHighlight_Selected || type == Prs3d_TypeOfHighlight_LocalSelected)
                e->viewerContext.context->UpdateSelected(Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_object_highlight_style(
        OcctHandle h,
        OcctObjectId objectId,
        int dynamic,
        const OcctHighlightStyleSettings* settings)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || settings == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            Handle(Prs3d_Drawer) drawer = new Prs3d_Drawer();
            drawer->SetLink(e->viewerContext.context->DefaultDrawer());
            applyHighlightStyle(drawer, *settings);
            if (dynamic != 0) entry.presentation->SetDynamicHilightAttributes(drawer);
            else entry.presentation->SetHilightAttributes(drawer);
            e->requestRedraw();
        });
    }

    int occt_clear_object_highlight_style(
        OcctHandle h,
        OcctObjectId objectId,
        int dynamic)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            Handle(Prs3d_Drawer) empty;
            if (dynamic != 0) entry.presentation->SetDynamicHilightAttributes(empty);
            else entry.presentation->SetHilightAttributes(empty);
            e->requestRedraw();
        });
    }

    int occt_reset_object_display_mode(OcctHandle h, OcctObjectId objectId)
    {
        const OcctViewerPresentationStateOptions options = presentationStateOptions(
            OcctViewerPresentationStateUpdate_ResetDisplayMode);
        return occt_engine_presentation_state_update(
                   reinterpret_cast<OcctEngineHandle>(h), objectId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_get_object_display_mode(
        OcctHandle h,
        OcctObjectId objectId,
        int* hasOverride,
        int* displayMode)
    {
        if (hasOverride == nullptr || displayMode == nullptr)
        {
            Engine* engine = engineOf(h);
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Display mode output is null.");
            return 0;
        }
        OcctViewerPresentationState state{};
        if (occt_engine_presentation_state_get(
                reinterpret_cast<OcctEngineHandle>(h), objectId, &state) != OcctStatus_Ok)
            return 0;
        *hasOverride = state.hasDisplayModeOverride;
        *displayMode = state.displayMode;
        return 1;
    }

    int occt_set_object_auto_highlight(OcctHandle h, OcctObjectId objectId, int enabled)
    {
        const OcctViewerPresentationStateOptions options = presentationStateOptions(
            OcctViewerPresentationStateUpdate_AutoHighlight,
            OcctDisplay_Shaded,
            enabled);
        return occt_engine_presentation_state_update(
                   reinterpret_cast<OcctEngineHandle>(h), objectId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_get_object_auto_highlight(OcctHandle h, OcctObjectId objectId, int* enabled)
    {
        if (enabled == nullptr)
        {
            Engine* engine = engineOf(h);
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "AutoHighlight output is null.");
            return 0;
        }
        OcctViewerPresentationState state{};
        if (occt_engine_presentation_state_get(
                reinterpret_cast<OcctEngineHandle>(h), objectId, &state) != OcctStatus_Ok)
            return 0;
        *enabled = state.autoHighlight;
        return 1;
    }

    int occt_set_object_infinite_state(OcctHandle h, OcctObjectId objectId, int infinite)
    {
        const OcctViewerPresentationStateOptions options = presentationStateOptions(
            OcctViewerPresentationStateUpdate_Infinite,
            OcctDisplay_Shaded,
            0,
            infinite);
        return occt_engine_presentation_state_update(
                   reinterpret_cast<OcctEngineHandle>(h), objectId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_get_object_infinite_state(OcctHandle h, OcctObjectId objectId, int* infinite)
    {
        if (infinite == nullptr)
        {
            Engine* engine = engineOf(h);
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Infinite-state output is null.");
            return 0;
        }
        OcctViewerPresentationState state{};
        if (occt_engine_presentation_state_get(
                reinterpret_cast<OcctEngineHandle>(h), objectId, &state) != OcctStatus_Ok)
            return 0;
        *infinite = state.infinite;
        return 1;
    }
}
