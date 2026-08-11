#include "OcctInternal.hxx"
#include "OcctPoints.h"

#include <AIS_Point.hxx>
#include <Aspect_TypeOfMarker.hxx>
#include <Geom_CartesianPoint.hxx>
#include <Prs3d_PointAspect.hxx>
#include <Prs3d_Drawer.hxx>

using namespace OcctBridge;

namespace
{
    Aspect_TypeOfMarker pointMarker(int marker)
    {
        if (marker < OcctPointMarker_Point || marker > OcctPointMarker_Ball)
            throw std::out_of_range("Point marker is outside the supported range.");
        return static_cast<Aspect_TypeOfMarker>(marker);
    }

    Handle(AIS_Point) requiredPoint(Engine* engine, OcctObjectId pointId)
    {
        ObjectEntry* entry = engine->findObject(pointId);
        if (entry == nullptr || entry->kind != OcctPointObjectKind)
            throw std::invalid_argument("Point ID does not exist.");

        Handle(AIS_Point) result = Handle(AIS_Point)::DownCast(entry->presentation);
        if (result.IsNull()) throw std::runtime_error("Point presentation type is invalid.");
        return result;
    }

    void applyPointStyle(
        const Handle(AIS_Point)& presentation,
        int marker,
        double scale,
        double r,
        double g,
        double b)
    {
        requirePositive(scale, "Point marker scale");
        const Aspect_TypeOfMarker markerType = pointMarker(marker);
        const Quantity_Color markerColor = color(r, g, b);
        presentation->Attributes()->SetPointAspect(
            new Prs3d_PointAspect(markerType, markerColor, scale));
        presentation->SetMarker(markerType);
        presentation->SetColor(markerColor);
    }
}

extern "C"
{
    OcctObjectId occt_add_point(
        OcctHandle h,
        OcctPoint3d position,
        int marker,
        double scale,
        double r,
        double g,
        double b)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return executeObject(engine, [&]
        {
            Handle(Geom_CartesianPoint) component = new Geom_CartesianPoint(point(position));
            Handle(AIS_Point) presentation = new AIS_Point(component);
            applyPointStyle(presentation, marker, scale, r, g, b);
            return engine->addPresentation(presentation, OcctPointObjectKind, "Point");
        });
    }

    int occt_set_point_position(
        OcctHandle h,
        OcctObjectId pointId,
        OcctPoint3d position)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return execute(engine, [&]
        {
            Handle(AIS_Point) presentation = requiredPoint(engine, pointId);
            Handle(Geom_CartesianPoint) component =
                Handle(Geom_CartesianPoint)::DownCast(presentation->Component());
            if (component.IsNull())
                presentation->SetComponent(new Geom_CartesianPoint(point(position)));
            else
                component->SetPnt(point(position));

            engine->context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }

    int occt_set_point_style(
        OcctHandle h,
        OcctObjectId pointId,
        int marker,
        double scale,
        double r,
        double g,
        double b)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return execute(engine, [&]
        {
            Handle(AIS_Point) presentation = requiredPoint(engine, pointId);
            applyPointStyle(presentation, marker, scale, r, g, b);
            engine->context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }
}
