#include "core/OcctInternal.hxx"
#include "OcctPoints.h"

#include <AIS_Point.hxx>
#include <Aspect_TypeOfMarker.hxx>
#include <Geom_CartesianPoint.hxx>
#include <Graphic3d_AspectMarker3d.hxx>
#include <Image_AlienPixMap.hxx>
#include <Image_Format.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_PointAspect.hxx>

#include <cstring>
#include <limits>

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

    Handle(Image_AlienPixMap) markerPixmap(
        int width,
        int height,
        const unsigned char* pixels,
        int pixelCount,
        int pixelFormat)
    {
        if (width <= 0 || height <= 0)
            throw std::invalid_argument("Pixmap marker dimensions must be greater than zero.");
        if (width > 4096 || height > 4096)
            throw std::invalid_argument("Pixmap marker dimensions are unreasonably large.");
        if (pixels == nullptr)
            throw std::invalid_argument("Pixmap marker pixel buffer is null.");
        if (pixelFormat != OcctPixelFormat_Bgra32 && pixelFormat != OcctPixelFormat_Rgba32)
            throw std::invalid_argument("Pixmap marker pixel format is out of range.");

        const std::int64_t required =
            static_cast<std::int64_t>(width) * static_cast<std::int64_t>(height) * 4;
        if (required > std::numeric_limits<int>::max() || pixelCount != static_cast<int>(required))
            throw std::invalid_argument("Pixmap marker pixel buffer size does not match its dimensions.");

        const Image_Format format =
            pixelFormat == OcctPixelFormat_Bgra32 ? Image_Format_BGRA : Image_Format_RGBA;
        Handle(Image_AlienPixMap) pixmap = new Image_AlienPixMap();
        const std::size_t packedRowBytes = static_cast<std::size_t>(width) * 4U;
        if (!pixmap->InitTrash(
                format,
                static_cast<std::size_t>(width),
                static_cast<std::size_t>(height),
                packedRowBytes))
        {
            throw std::runtime_error("Failed to allocate pixmap marker image.");
        }

        pixmap->SetTopDown(true);
        for (int row = 0; row < height; ++row)
        {
            std::memcpy(
                pixmap->ChangeRow(static_cast<std::size_t>(row)),
                pixels + static_cast<std::size_t>(row) * packedRowBytes,
                packedRowBytes);
        }
        return pixmap;
    }

    void applyPixmapStyle(
        const Handle(AIS_Point)& presentation,
        int width,
        int height,
        const unsigned char* pixels,
        int pixelCount,
        int pixelFormat)
    {
        Handle(Image_AlienPixMap) pixmap =
            markerPixmap(width, height, pixels, pixelCount, pixelFormat);
        Handle(Graphic3d_AspectMarker3d) markerAspect =
            new Graphic3d_AspectMarker3d(pixmap);
        presentation->Attributes()->SetPointAspect(
            new Prs3d_PointAspect(markerAspect));
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

            engine->viewerContext.context->Redisplay(presentation, Standard_False);
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
            engine->viewerContext.context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }

    OcctObjectId occt_add_point_pixmap(
        OcctHandle h,
        OcctPoint3d position,
        int width,
        int height,
        const unsigned char* pixels,
        int pixelCount,
        int pixelFormat)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return executeObject(engine, [&]
        {
            Handle(Geom_CartesianPoint) component = new Geom_CartesianPoint(point(position));
            Handle(AIS_Point) presentation = new AIS_Point(component);
            applyPixmapStyle(presentation, width, height, pixels, pixelCount, pixelFormat);
            return engine->addPresentation(presentation, OcctPointObjectKind, "Point");
        });
    }

    int occt_set_point_pixmap_style(
        OcctHandle h,
        OcctObjectId pointId,
        int width,
        int height,
        const unsigned char* pixels,
        int pixelCount,
        int pixelFormat)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return execute(engine, [&]
        {
            Handle(AIS_Point) presentation = requiredPoint(engine, pointId);
            applyPixmapStyle(presentation, width, height, pixels, pixelCount, pixelFormat);
            engine->viewerContext.context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }
}
