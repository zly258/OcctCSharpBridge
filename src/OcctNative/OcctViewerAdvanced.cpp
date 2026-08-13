#include "OcctInternal.hxx"
#include "OcctViewerAdvanced.h"
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
            case OcctZLayer_Bottom: return Graphic3d_ZLayerId_BotOSD;
            case OcctZLayer_Default: return Graphic3d_ZLayerId_Default;
            case OcctZLayer_Top: return Graphic3d_ZLayerId_Top;
            case OcctZLayer_Topmost: return Graphic3d_ZLayerId_Topmost;
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
        if (settings.zLayer < -1 || settings.zLayer > OcctZLayer_Topmost)
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
        const Handle(Graphic3d_SequenceOfHClipPlane)& viewPlanes = engine->view->ClipPlanes();
        if (!viewPlanes.IsNull()) viewPlaneCount = viewPlanes->Size();
        return std::max(0, engine->view->PlaneLimit() - viewPlaneCount);
    }
}

extern "C"
{
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
            e->context->Redisplay(entry.presentation, Standard_False);
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
            applyHighlightStyle(e->context->HighlightStyle(type), *settings);
            if (type == Prs3d_TypeOfHighlight_Selected || type == Prs3d_TypeOfHighlight_LocalSelected)
                e->context->UpdateSelected(Standard_False);
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
            drawer->SetLink(e->context->DefaultDrawer());
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
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            e->context->UnsetDisplayMode(entry.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_get_object_display_mode(
        OcctHandle h,
        OcctObjectId objectId,
        int* hasOverride,
        int* displayMode)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || hasOverride == nullptr || displayMode == nullptr) return 0;
        return execute(e, [&]
        {
            const ObjectEntry& entry = requiredObject(e, objectId);
            *hasOverride = entry.presentation->HasDisplayMode() ? 1 : 0;
            if (*hasOverride == 0)
            {
                *displayMode = -1;
                return;
            }

            const int nativeMode = entry.presentation->DisplayMode();
            if (nativeMode == AIS_WireFrame) *displayMode = OcctDisplay_Wireframe;
            else if (nativeMode == AIS_Shaded) *displayMode = OcctDisplay_Shaded;
            else *displayMode = nativeMode;
        });
    }

    int occt_set_object_auto_highlight(OcctHandle h, OcctObjectId objectId, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            const Standard_Boolean requested = enabled != 0;
            entry.presentation->SetAutoHilight(requested);
            if (entry.presentation->IsAutoHilight() != requested)
                throw std::invalid_argument("Object does not support the requested AutoHighlight state.");
            e->requestRedraw();
        });
    }

    int occt_get_object_auto_highlight(OcctHandle h, OcctObjectId objectId, int* enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || enabled == nullptr) return 0;
        return execute(e, [&]
        {
            const ObjectEntry& entry = requiredObject(e, objectId);
            *enabled = entry.presentation->IsAutoHilight() ? 1 : 0;
        });
    }

    int occt_set_object_infinite_state(OcctHandle h, OcctObjectId objectId, int infinite)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            entry.presentation->SetInfiniteState(infinite != 0);
            e->context->Redisplay(entry.presentation, Standard_False, Standard_True);
            e->requestRedraw();
        });
    }

    int occt_get_object_infinite_state(OcctHandle h, OcctObjectId objectId, int* infinite)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || infinite == nullptr) return 0;
        return execute(e, [&]
        {
            const ObjectEntry& entry = requiredObject(e, objectId);
            *infinite = entry.presentation->IsInfinite() ? 1 : 0;
        });
    }
}
