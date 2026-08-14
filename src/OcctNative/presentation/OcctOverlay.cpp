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

#include <cmath>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t OverlayOptionsApiVersion = 1;
    constexpr std::uint32_t AllLineUpdateBits =
        OcctOverlayLineUpdate_Geometry |
        OcctOverlayLineUpdate_Style;
    constexpr std::uint32_t AllMarkerUpdateBits =
        OcctOverlayMarkerUpdate_Position |
        OcctOverlayMarkerUpdate_Style;
    constexpr std::uint32_t AllTextUpdateBits =
        OcctOverlayTextUpdate_Content |
        OcctOverlayTextUpdate_Position |
        OcctOverlayTextUpdate_Style;

    TCollection_ExtendedString extended(const char* text)
    {
        return TCollection_ExtendedString(text == nullptr ? "" : text, Standard_True);
    }

    void requireFinitePoint(OcctPoint3d value, const char* name)
    {
        if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
            throw std::invalid_argument(std::string(name) + " must be finite.");
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

    void validateLinePrimitive(int value)
    {
        if (value != OcctOverlay_Line && value != OcctOverlay_Polyline)
            throw std::invalid_argument("Overlay line primitive type must be Line or Polyline.");
    }

    TopoDS_Shape lineShape(OcctPoint3d start, OcctPoint3d end)
    {
        requireFinitePoint(start, "Overlay line start");
        requireFinitePoint(end, "Overlay line end");
        BRepBuilderAPI_MakeEdge builder(point(start), point(end));
        if (!builder.IsDone()) throw std::runtime_error("Unable to create overlay line geometry.");
        return builder.Edge();
    }

    TopoDS_Shape polylineShape(const OcctPoint3d* points, int count)
    {
        if (count < 2) throw std::invalid_argument("Overlay polyline requires at least two points.");
        if (points == nullptr) throw std::invalid_argument("Overlay polyline point array is null.");
        BRepBuilderAPI_MakePolygon builder;
        for (int index = 0; index < count; ++index)
        {
            requireFinitePoint(points[index], "Overlay polyline point");
            builder.Add(point(points[index]));
        }
        if (!builder.IsDone()) throw std::runtime_error("Unable to create overlay polyline geometry.");
        return builder.Wire();
    }

    TopoDS_Shape lineGeometry(const OcctOverlayLineOptions& options)
    {
        validateLinePrimitive(options.primitiveType);
        if (options.primitiveType == OcctOverlay_Line)
        {
            if (options.points == nullptr || options.pointCount != 2)
                throw std::invalid_argument("Overlay line requires exactly two points.");
            return lineShape(options.points[0], options.points[1]);
        }
        return polylineShape(options.points, options.pointCount);
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

    OcctObjectId registerOverlay(
        Engine* engine,
        const Handle(AIS_InteractiveObject)& presentation,
        int subtype,
        const char* name)
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

    void applyLineStyle(
        const Handle(AIS_Shape)& presentation,
        int pattern,
        double width,
        double r,
        double g,
        double b)
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

    void applyMarkerStyle(
        const Handle(AIS_Point)& presentation,
        int marker,
        double scale,
        double r,
        double g,
        double b)
    {
        requirePositive(scale, "Overlay marker scale");
        const Aspect_TypeOfMarker nativeMarker = markerType(marker);
        const Quantity_Color markerColor = color(r, g, b);
        presentation->Attributes()->SetPointAspect(
            new Prs3d_PointAspect(nativeMarker, markerColor, scale));
        presentation->SetMarker(nativeMarker);
        presentation->SetColor(markerColor);
    }

    void applyTextStyle(
        const Handle(AIS_TextLabel)& presentation,
        double height,
        double r,
        double g,
        double b,
        int zoomable,
        const char* fontName)
    {
        requirePositive(height, "Overlay text height");
        presentation->SetHeight(height);
        presentation->SetColor(color(r, g, b));
        presentation->SetZoomable(zoomable != 0);
        if (fontName != nullptr && *fontName != '\0') presentation->SetFont(fontName);
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeOverlayStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    template<typename Function>
    OcctStatus executeOverlayObjectStatus(
        Engine* engine,
        OcctObjectId* output,
        Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        if (output == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Result overlay ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }

        *output = 0;
        const OcctObjectId value = executeObject(engine, std::forward<Function>(function));
        if (value == 0) return engine->errors.code;
        *output = value;
        return OcctStatus_Ok;
    }

    void validateLineOptions(const OcctOverlayLineOptions* options, bool isUpdate)
    {
        if (options == nullptr) throw std::invalid_argument("Overlay line options are null.");
        if (options->structSize < sizeof(OcctOverlayLineOptions) ||
            options->apiVersion != OverlayOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported overlay line options size or version.");
        }
        if ((options->updateMask & ~AllLineUpdateBits) != 0 ||
            (isUpdate && options->updateMask == 0))
        {
            throw std::invalid_argument("Overlay line update mask is invalid.");
        }
        if (!isUpdate && options->updateMask != AllLineUpdateBits)
            throw std::invalid_argument("Overlay line creation requires geometry and style.");

        if (!isUpdate || (options->updateMask & OcctOverlayLineUpdate_Geometry) != 0)
            (void)lineGeometry(*options);
        if (!isUpdate || (options->updateMask & OcctOverlayLineUpdate_Style) != 0)
        {
            requirePositive(options->width, "Overlay line width");
            (void)linePattern(options->pattern);
            (void)color(options->red, options->green, options->blue);
        }
    }

    void validateMarkerOptions(const OcctOverlayMarkerOptions* options, bool isUpdate)
    {
        if (options == nullptr) throw std::invalid_argument("Overlay marker options are null.");
        if (options->structSize < sizeof(OcctOverlayMarkerOptions) ||
            options->apiVersion != OverlayOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported overlay marker options size or version.");
        }
        if ((options->updateMask & ~AllMarkerUpdateBits) != 0 ||
            (isUpdate && options->updateMask == 0))
        {
            throw std::invalid_argument("Overlay marker update mask is invalid.");
        }
        if (!isUpdate && options->updateMask != AllMarkerUpdateBits)
            throw std::invalid_argument("Overlay marker creation requires position and style.");

        if (!isUpdate || (options->updateMask & OcctOverlayMarkerUpdate_Position) != 0)
            requireFinitePoint(options->position, "Overlay marker position");
        if (!isUpdate || (options->updateMask & OcctOverlayMarkerUpdate_Style) != 0)
        {
            requirePositive(options->scale, "Overlay marker scale");
            (void)markerType(options->marker);
            (void)color(options->red, options->green, options->blue);
        }
    }

    void validateTextOptions(const OcctOverlayTextOptions* options, bool isUpdate)
    {
        if (options == nullptr) throw std::invalid_argument("Overlay text options are null.");
        if (options->structSize < sizeof(OcctOverlayTextOptions) ||
            options->apiVersion != OverlayOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported overlay text options size or version.");
        }
        if ((options->updateMask & ~AllTextUpdateBits) != 0 ||
            (isUpdate && options->updateMask == 0))
        {
            throw std::invalid_argument("Overlay text update mask is invalid.");
        }
        if (!isUpdate && options->updateMask != AllTextUpdateBits)
            throw std::invalid_argument("Overlay text creation requires content, position and style.");

        if (!isUpdate || (options->updateMask & OcctOverlayTextUpdate_Position) != 0)
            requireFinitePoint(options->position, "Overlay text position");
        if (!isUpdate || (options->updateMask & OcctOverlayTextUpdate_Style) != 0)
        {
            requirePositive(options->height, "Overlay text height");
            (void)color(options->red, options->green, options->blue);
        }
    }

    OcctOverlayLineOptions lineOptions(
        std::uint32_t updateMask,
        int primitiveType,
        const OcctPoint3d* points,
        int pointCount,
        int pattern,
        double width,
        double r,
        double g,
        double b)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctOverlayLineOptions)),
            OverlayOptionsApiVersion,
            updateMask,
            primitiveType,
            points,
            pointCount,
            pattern,
            width,
            r,
            g,
            b };
    }

    OcctOverlayMarkerOptions markerOptions(
        std::uint32_t updateMask,
        OcctPoint3d position,
        int marker,
        double scale,
        double r,
        double g,
        double b)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctOverlayMarkerOptions)),
            OverlayOptionsApiVersion,
            updateMask,
            position,
            marker,
            scale,
            r,
            g,
            b };
    }

    OcctOverlayTextOptions textOptions(
        std::uint32_t updateMask,
        const char* text,
        OcctPoint3d position,
        double height,
        double r,
        double g,
        double b,
        int zoomable,
        const char* fontName)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctOverlayTextOptions)),
            OverlayOptionsApiVersion,
            updateMask,
            text,
            position,
            height,
            r,
            g,
            b,
            zoomable,
            fontName };
    }
}

extern "C"
{
    OcctStatus occt_engine_overlay_line_create(
        OcctEngineHandle handle,
        const OcctOverlayLineOptions* options,
        OcctObjectId* resultOverlayId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayObjectStatus(engine, resultOverlayId, [&]
        {
            validateLineOptions(options, false);
            Handle(AIS_Shape) presentation = new AIS_Shape(lineGeometry(*options));
            applyLineStyle(
                presentation,
                options->pattern,
                options->width,
                options->red,
                options->green,
                options->blue);
            return registerOverlay(
                engine,
                presentation,
                options->primitiveType,
                options->primitiveType == OcctOverlay_Line ? "OverlayLine" : "OverlayPolyline");
        });
    }

    OcctStatus occt_engine_overlay_line_update(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        const OcctOverlayLineOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayStatus(engine, [&]
        {
            validateLineOptions(options, true);
            ObjectEntry& entry = requiredOverlay(engine, overlayId, -1);
            if (entry.presentationSubtype != OcctOverlay_Line &&
                entry.presentationSubtype != OcctOverlay_Polyline)
            {
                throw std::invalid_argument("Overlay is not a line primitive.");
            }
            Handle(AIS_Shape) presentation = Handle(AIS_Shape)::DownCast(entry.presentation);
            if (presentation.IsNull())
                throw std::runtime_error("Overlay line presentation type is invalid.");

            if ((options->updateMask & OcctOverlayLineUpdate_Geometry) != 0)
            {
                if (entry.presentationSubtype != options->primitiveType)
                    throw std::invalid_argument("Overlay line primitive type cannot change during update.");
                presentation->SetShape(lineGeometry(*options));
            }
            if ((options->updateMask & OcctOverlayLineUpdate_Style) != 0)
            {
                applyLineStyle(
                    presentation,
                    options->pattern,
                    options->width,
                    options->red,
                    options->green,
                    options->blue);
            }
            engine->viewerContext.context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_overlay_marker_create(
        OcctEngineHandle handle,
        const OcctOverlayMarkerOptions* options,
        OcctObjectId* resultOverlayId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayObjectStatus(engine, resultOverlayId, [&]
        {
            validateMarkerOptions(options, false);
            Handle(AIS_Point) presentation =
                new AIS_Point(new Geom_CartesianPoint(point(options->position)));
            applyMarkerStyle(
                presentation,
                options->marker,
                options->scale,
                options->red,
                options->green,
                options->blue);
            return registerOverlay(engine, presentation, OcctOverlay_Marker, "OverlayMarker");
        });
    }

    OcctStatus occt_engine_overlay_marker_update(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        const OcctOverlayMarkerOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayStatus(engine, [&]
        {
            validateMarkerOptions(options, true);
            ObjectEntry& entry = requiredOverlay(engine, overlayId, OcctOverlay_Marker);
            Handle(AIS_Point) presentation = Handle(AIS_Point)::DownCast(entry.presentation);
            if (presentation.IsNull())
                throw std::runtime_error("Overlay marker presentation type is invalid.");

            if ((options->updateMask & OcctOverlayMarkerUpdate_Position) != 0)
            {
                Handle(Geom_CartesianPoint) component =
                    Handle(Geom_CartesianPoint)::DownCast(presentation->Component());
                if (component.IsNull())
                    presentation->SetComponent(new Geom_CartesianPoint(point(options->position)));
                else
                    component->SetPnt(point(options->position));
            }
            if ((options->updateMask & OcctOverlayMarkerUpdate_Style) != 0)
            {
                applyMarkerStyle(
                    presentation,
                    options->marker,
                    options->scale,
                    options->red,
                    options->green,
                    options->blue);
            }
            engine->viewerContext.context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_overlay_text_create(
        OcctEngineHandle handle,
        const OcctOverlayTextOptions* options,
        OcctObjectId* resultOverlayId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayObjectStatus(engine, resultOverlayId, [&]
        {
            validateTextOptions(options, false);
            Handle(AIS_TextLabel) presentation = new AIS_TextLabel();
            presentation->SetText(extended(options->text));
            presentation->SetPosition(point(options->position));
            applyTextStyle(
                presentation,
                options->height,
                options->red,
                options->green,
                options->blue,
                options->zoomable,
                options->fontName);
            return registerOverlay(engine, presentation, OcctOverlay_Text, "OverlayText");
        });
    }

    OcctStatus occt_engine_overlay_text_update(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        const OcctOverlayTextOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayStatus(engine, [&]
        {
            validateTextOptions(options, true);
            ObjectEntry& entry = requiredOverlay(engine, overlayId, OcctOverlay_Text);
            Handle(AIS_TextLabel) presentation = Handle(AIS_TextLabel)::DownCast(entry.presentation);
            if (presentation.IsNull())
                throw std::runtime_error("Overlay text presentation type is invalid.");

            if ((options->updateMask & OcctOverlayTextUpdate_Content) != 0)
                presentation->SetText(extended(options->text));
            if ((options->updateMask & OcctOverlayTextUpdate_Position) != 0)
                presentation->SetPosition(point(options->position));
            if ((options->updateMask & OcctOverlayTextUpdate_Style) != 0)
            {
                applyTextStyle(
                    presentation,
                    options->height,
                    options->red,
                    options->green,
                    options->blue,
                    options->zoomable,
                    options->fontName);
            }
            engine->viewerContext.context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_overlay_primitive_type_get(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        int* primitiveType)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayStatus(engine, [&]
        {
            if (primitiveType == nullptr)
                throw std::invalid_argument("Overlay primitive type output is null.");
            ObjectEntry& entry = requiredOverlay(engine, overlayId, -1);
            *primitiveType = entry.presentationSubtype;
        });
    }

    OcctObjectId occt_add_overlay_line(
        OcctHandle h,
        OcctPoint3d start,
        OcctPoint3d end,
        int pattern,
        double width,
        double r,
        double g,
        double b)
    {
        const OcctPoint3d points[] = { start, end };
        const OcctOverlayLineOptions options = lineOptions(
            AllLineUpdateBits, OcctOverlay_Line, points, 2, pattern, width, r, g, b);
        OcctObjectId overlayId = 0;
        return occt_engine_overlay_line_create(
                   reinterpret_cast<OcctEngineHandle>(h), &options, &overlayId) == OcctStatus_Ok
            ? overlayId
            : 0;
    }

    OcctObjectId occt_add_overlay_polyline(
        OcctHandle h,
        const OcctPoint3d* points,
        int count,
        int pattern,
        double width,
        double r,
        double g,
        double b)
    {
        const OcctOverlayLineOptions options = lineOptions(
            AllLineUpdateBits, OcctOverlay_Polyline, points, count, pattern, width, r, g, b);
        OcctObjectId overlayId = 0;
        return occt_engine_overlay_line_create(
                   reinterpret_cast<OcctEngineHandle>(h), &options, &overlayId) == OcctStatus_Ok
            ? overlayId
            : 0;
    }

    OcctObjectId occt_add_overlay_marker(
        OcctHandle h,
        OcctPoint3d position,
        int marker,
        double scale,
        double r,
        double g,
        double b)
    {
        const OcctOverlayMarkerOptions options = markerOptions(
            AllMarkerUpdateBits, position, marker, scale, r, g, b);
        OcctObjectId overlayId = 0;
        return occt_engine_overlay_marker_create(
                   reinterpret_cast<OcctEngineHandle>(h), &options, &overlayId) == OcctStatus_Ok
            ? overlayId
            : 0;
    }

    OcctObjectId occt_add_overlay_text(
        OcctHandle h,
        const char* text,
        OcctPoint3d position,
        double height,
        double r,
        double g,
        double b,
        int zoomable,
        const char* fontName)
    {
        const OcctOverlayTextOptions options = textOptions(
            AllTextUpdateBits, text, position, height, r, g, b, zoomable, fontName);
        OcctObjectId overlayId = 0;
        return occt_engine_overlay_text_create(
                   reinterpret_cast<OcctEngineHandle>(h), &options, &overlayId) == OcctStatus_Ok
            ? overlayId
            : 0;
    }

    int occt_update_overlay_line(
        OcctHandle h,
        OcctObjectId overlayId,
        OcctPoint3d start,
        OcctPoint3d end)
    {
        const OcctPoint3d points[] = { start, end };
        const OcctOverlayLineOptions options = lineOptions(
            OcctOverlayLineUpdate_Geometry,
            OcctOverlay_Line,
            points,
            2,
            OcctOverlayLine_Solid,
            1.0,
            0.0,
            0.0,
            0.0);
        return occt_engine_overlay_line_update(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_update_overlay_polyline(
        OcctHandle h,
        OcctObjectId overlayId,
        const OcctPoint3d* points,
        int count)
    {
        const OcctOverlayLineOptions options = lineOptions(
            OcctOverlayLineUpdate_Geometry,
            OcctOverlay_Polyline,
            points,
            count,
            OcctOverlayLine_Solid,
            1.0,
            0.0,
            0.0,
            0.0);
        return occt_engine_overlay_line_update(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_update_overlay_marker(
        OcctHandle h,
        OcctObjectId overlayId,
        OcctPoint3d position)
    {
        const OcctOverlayMarkerOptions options = markerOptions(
            OcctOverlayMarkerUpdate_Position,
            position,
            OcctPointMarker_Point,
            1.0,
            0.0,
            0.0,
            0.0);
        return occt_engine_overlay_marker_update(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_update_overlay_text(
        OcctHandle h,
        OcctObjectId overlayId,
        const char* text,
        OcctPoint3d position)
    {
        const OcctOverlayTextOptions options = textOptions(
            OcctOverlayTextUpdate_Content | OcctOverlayTextUpdate_Position,
            text,
            position,
            1.0,
            0.0,
            0.0,
            0.0,
            0,
            nullptr);
        return occt_engine_overlay_text_update(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_set_overlay_line_style(
        OcctHandle h,
        OcctObjectId overlayId,
        int pattern,
        double width,
        double r,
        double g,
        double b)
    {
        const OcctOverlayLineOptions options = lineOptions(
            OcctOverlayLineUpdate_Style,
            -1,
            nullptr,
            0,
            pattern,
            width,
            r,
            g,
            b);
        return occt_engine_overlay_line_update(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_set_overlay_marker_style(
        OcctHandle h,
        OcctObjectId overlayId,
        int marker,
        double scale,
        double r,
        double g,
        double b)
    {
        const OcctOverlayMarkerOptions options = markerOptions(
            OcctOverlayMarkerUpdate_Style,
            {},
            marker,
            scale,
            r,
            g,
            b);
        return occt_engine_overlay_marker_update(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_set_overlay_text_style(
        OcctHandle h,
        OcctObjectId overlayId,
        double height,
        double r,
        double g,
        double b,
        int zoomable,
        const char* fontName)
    {
        const OcctOverlayTextOptions options = textOptions(
            OcctOverlayTextUpdate_Style,
            nullptr,
            {},
            height,
            r,
            g,
            b,
            zoomable,
            fontName);
        return occt_engine_overlay_text_update(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, &options) == OcctStatus_Ok
            ? 1
            : 0;
    }

    int occt_get_overlay_primitive_type(
        OcctHandle h,
        OcctObjectId overlayId,
        int* primitiveType)
    {
        return occt_engine_overlay_primitive_type_get(
                   reinterpret_cast<OcctEngineHandle>(h), overlayId, primitiveType) == OcctStatus_Ok
            ? 1
            : 0;
    }
}
