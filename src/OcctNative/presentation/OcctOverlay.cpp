#include "core/OcctInternal.hxx"
#include "OcctOverlay.h"
#include "geometry/OcctPoints.h"

#include <AIS_Point.hxx>
#include <AIS_Shape.hxx>
#include <AIS_TextLabel.hxx>
#include <Aspect_TypeOfLine.hxx>
#include <Aspect_TypeOfMarker.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <Geom_CartesianPoint.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Prs3d_LineAspect.hxx>
#include <Prs3d_PointAspect.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TopoDS_Wire.hxx>

using namespace OcctBridge;

namespace
{
    TCollection_ExtendedString extended(const char* text)
    {
        return TCollection_ExtendedString(text == nullptr ? "" : text, Standard_True);
    }

    Aspect_TypeOfLine linePattern(int value)
    {
        switch (value)
        {
            case OcctOverlayLine_Solid: return Aspect_TOL_SOLID;
            case OcctOverlayLine_Dashed: return Aspect_TOL_DASH;
            case OcctOverlayLine_Dotted: return Aspect_TOL_DOT;
            case OcctOverlayLine_DashDot: return Aspect_TOL_DOTDASH;
            default: throw std::invalid_argument("Overlay line pattern is out of range.");
        }
    }

    Aspect_TypeOfMarker markerType(int value)
    {
        if (value < OcctPointMarker_Point || value > OcctPointMarker_Ball)
            throw std::invalid_argument("Overlay marker type is out of range.");
        return static_cast<Aspect_TypeOfMarker>(value);
    }

    TopoDS_Shape lineShape(OcctPoint3d start, OcctPoint3d end)
    {
        BRepBuilderAPI_MakeEdge builder(point(start), point(end));
        if (!builder.IsDone()) throw std::runtime_error("Unable to create overlay line geometry.");
        return builder.Edge();
    }

    TopoDS_Shape polylineShape(const OcctPoint3d* points, int count)
    {
        if (count < 2) throw std::invalid_argument("Overlay polyline requires at least two points.");
        if (points == nullptr) throw std::invalid_argument("Overlay polyline point array is null.");
        BRepBuilderAPI_MakePolygon builder;
        for (int index = 0; index < count; ++index) builder.Add(point(points[index]));
        if (!builder.IsDone()) throw std::runtime_error("Unable to create overlay polyline geometry.");
        const TopoDS_Wire wire = builder.Wire();
        return wire;
    }

    ObjectEntry& requiredOverlay(Engine* engine, OcctObjectId id, int subtype)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->kind != OcctOverlayObjectKind || entry->presentation.IsNull())
            throw std::invalid_argument("Overlay ID does not exist.");
        if (subtype >= 0 && entry->presentationSubtype != subtype)
            throw std::invalid_argument("Overlay primitive type does not match this operation.");
        return *entry;
    }

    OcctObjectId registerOverlay(Engine* engine, const Handle(AIS_InteractiveObject)& presentation, int subtype, const char* name)
    {
        if (presentation.IsNull()) throw std::runtime_error("Overlay presentation is null.");
        presentation->SetZLayer(Graphic3d_ZLayerId_Topmost);
        presentation->SetInfiniteState(Standard_True);
        const OcctObjectId id = engine->scene.allocateId();
        engine->viewerContext.context->Display(presentation, Standard_False);
        ObjectEntry entry;
        entry.kind = OcctOverlayObjectKind;
        entry.presentation = presentation;
        entry.name = name == nullptr ? "Overlay" : name;
        entry.selectable = false;
        entry.presentationSubtype = subtype;
        engine->scene.objects.emplace(id, std::move(entry));
        engine->viewerContext.context->Deactivate(presentation);
        engine->requestRedraw();
        return id;
    }

    void applyLineStyle(const Handle(AIS_Shape)& presentation, int pattern, double width, double r, double g, double b)
    {
        requirePositive(width, "Overlay line width");
        const Quantity_Color value = color(r, g, b);
        Handle(Prs3d_LineAspect) aspect = new Prs3d_LineAspect(value, linePattern(pattern), width);
        const Handle(Prs3d_Drawer)& drawer = presentation->Attributes();
        drawer->SetLineAspect(aspect);
        drawer->SetWireAspect(aspect);
        drawer->SetSeenLineAspect(aspect);
        drawer->SetFreeBoundaryAspect(aspect);
        drawer->SetUnFreeBoundaryAspect(aspect);
        presentation->SetColor(value);
        presentation->SetWidth(width);
    }

    void applyMarkerStyle(const Handle(AIS_Point)& presentation, int marker, double scale, double r, double g, double b)
    {
        requirePositive(scale, "Overlay marker scale");
        const Aspect_TypeOfMarker nativeMarker = markerType(marker);
        const Quantity_Color markerColor = color(r, g, b);
        presentation->Attributes()->SetPointAspect(new Prs3d_PointAspect(nativeMarker, markerColor, scale));
        presentation->SetMarker(nativeMarker);
        presentation->SetColor(markerColor);
    }

    void applyTextStyle(const Handle(AIS_TextLabel)& presentation, double height, double r, double g, double b, int zoomable, const char* fontName)
    {
        requirePositive(height, "Overlay text height");
        presentation->SetHeight(height);
        presentation->SetColor(color(r, g, b));
        presentation->SetZoomable(zoomable != 0);
        if (fontName != nullptr && *fontName != '\0') presentation->SetFont(fontName);
    }
}

extern "C"
{
    OcctObjectId occt_add_overlay_line(OcctHandle h, OcctPoint3d start, OcctPoint3d end, int pattern, double width, double r, double g, double b)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            Handle(AIS_Shape) presentation = new AIS_Shape(lineShape(start, end));
            applyLineStyle(presentation, pattern, width, r, g, b);
            return registerOverlay(e, presentation, OcctOverlay_Line, "OverlayLine");
        });
    }

    OcctObjectId occt_add_overlay_polyline(OcctHandle h, const OcctPoint3d* points, int count, int pattern, double width, double r, double g, double b)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            Handle(AIS_Shape) presentation = new AIS_Shape(polylineShape(points, count));
            applyLineStyle(presentation, pattern, width, r, g, b);
            return registerOverlay(e, presentation, OcctOverlay_Polyline, "OverlayPolyline");
        });
    }

    OcctObjectId occt_add_overlay_marker(OcctHandle h, OcctPoint3d position, int marker, double scale, double r, double g, double b)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            Handle(AIS_Point) presentation = new AIS_Point(new Geom_CartesianPoint(point(position)));
            applyMarkerStyle(presentation, marker, scale, r, g, b);
            return registerOverlay(e, presentation, OcctOverlay_Marker, "OverlayMarker");
        });
    }

    OcctObjectId occt_add_overlay_text(OcctHandle h, const char* text, OcctPoint3d position, double height, double r, double g, double b, int zoomable, const char* fontName)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            Handle(AIS_TextLabel) presentation = new AIS_TextLabel();
            presentation->SetText(extended(text));
            presentation->SetPosition(point(position));
            applyTextStyle(presentation, height, r, g, b, zoomable, fontName);
            return registerOverlay(e, presentation, OcctOverlay_Text, "OverlayText");
        });
    }

    int occt_update_overlay_line(OcctHandle h, OcctObjectId overlayId, OcctPoint3d start, OcctPoint3d end)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, OcctOverlay_Line);
            Handle(AIS_Shape) presentation = Handle(AIS_Shape)::DownCast(entry.presentation);
            if (presentation.IsNull()) throw std::runtime_error("Overlay line presentation type is invalid.");
            presentation->SetShape(lineShape(start, end));
            e->viewerContext.context->Redisplay(presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_update_overlay_polyline(OcctHandle h, OcctObjectId overlayId, const OcctPoint3d* points, int count)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, OcctOverlay_Polyline);
            Handle(AIS_Shape) presentation = Handle(AIS_Shape)::DownCast(entry.presentation);
            if (presentation.IsNull()) throw std::runtime_error("Overlay polyline presentation type is invalid.");
            presentation->SetShape(polylineShape(points, count));
            e->viewerContext.context->Redisplay(presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_update_overlay_marker(OcctHandle h, OcctObjectId overlayId, OcctPoint3d position)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, OcctOverlay_Marker);
            Handle(AIS_Point) presentation = Handle(AIS_Point)::DownCast(entry.presentation);
            if (presentation.IsNull()) throw std::runtime_error("Overlay marker presentation type is invalid.");
            Handle(Geom_CartesianPoint) component = Handle(Geom_CartesianPoint)::DownCast(presentation->Component());
            if (component.IsNull()) presentation->SetComponent(new Geom_CartesianPoint(point(position)));
            else component->SetPnt(point(position));
            e->viewerContext.context->Redisplay(presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_update_overlay_text(OcctHandle h, OcctObjectId overlayId, const char* text, OcctPoint3d position)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, OcctOverlay_Text);
            Handle(AIS_TextLabel) presentation = Handle(AIS_TextLabel)::DownCast(entry.presentation);
            if (presentation.IsNull()) throw std::runtime_error("Overlay text presentation type is invalid.");
            presentation->SetText(extended(text));
            presentation->SetPosition(point(position));
            e->viewerContext.context->Redisplay(presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_overlay_line_style(OcctHandle h, OcctObjectId overlayId, int pattern, double width, double r, double g, double b)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, -1);
            if (entry.presentationSubtype != OcctOverlay_Line && entry.presentationSubtype != OcctOverlay_Polyline)
                throw std::invalid_argument("Overlay is not a line primitive.");
            Handle(AIS_Shape) presentation = Handle(AIS_Shape)::DownCast(entry.presentation);
            if (presentation.IsNull()) throw std::runtime_error("Overlay line presentation type is invalid.");
            applyLineStyle(presentation, pattern, width, r, g, b);
            e->viewerContext.context->Redisplay(presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_overlay_marker_style(OcctHandle h, OcctObjectId overlayId, int marker, double scale, double r, double g, double b)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, OcctOverlay_Marker);
            Handle(AIS_Point) presentation = Handle(AIS_Point)::DownCast(entry.presentation);
            if (presentation.IsNull()) throw std::runtime_error("Overlay marker presentation type is invalid.");
            applyMarkerStyle(presentation, marker, scale, r, g, b);
            e->viewerContext.context->Redisplay(presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_overlay_text_style(OcctHandle h, OcctObjectId overlayId, double height, double r, double g, double b, int zoomable, const char* fontName)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, OcctOverlay_Text);
            Handle(AIS_TextLabel) presentation = Handle(AIS_TextLabel)::DownCast(entry.presentation);
            if (presentation.IsNull()) throw std::runtime_error("Overlay text presentation type is invalid.");
            applyTextStyle(presentation, height, r, g, b, zoomable, fontName);
            e->viewerContext.context->Redisplay(presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_get_overlay_primitive_type(OcctHandle h, OcctObjectId overlayId, int* primitiveType)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || primitiveType == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredOverlay(e, overlayId, -1);
            *primitiveType = entry.presentationSubtype;
        });
    }
}
