#include "geometry/OcctPoints.h"
#include "core/OcctInternal.hxx"

#include <AIS_Point.hxx>
#include <Aspect_TypeOfMarker.hxx>
#include <Geom_CartesianPoint.hxx>
#include <Graphic3d_AspectMarker3d.hxx>
#include <Image_AlienPixMap.hxx>
#include <Image_Format.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_PointAspect.hxx>

#include <cmath>
#include <cstring>
#include <limits>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t PointOptionsApiVersion = 1;
    constexpr std::uint32_t PointPixmapOptionsApiVersion = 1;
    constexpr std::uint32_t AllPointUpdateBits =
        OcctViewerPointUpdate_Position |
        OcctViewerPointUpdate_Style;
    constexpr std::uint32_t AllPointPixmapUpdateBits =
        OcctViewerPointPixmapUpdate_Position |
        OcctViewerPointPixmapUpdate_Image;

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

    void requireFinitePoint(OcctPoint3d position)
    {
        if (!std::isfinite(position.x) || !std::isfinite(position.y) || !std::isfinite(position.z))
            throw std::invalid_argument("Point position must be finite.");
    }

    void validatePixmapData(
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
    }

    Handle(Image_AlienPixMap) createMarkerPixmap(
        int width,
        int height,
        const unsigned char* pixels,
        int pixelFormat)
    {
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

    void applyPixmapStyle(
        const Handle(AIS_Point)& presentation,
        int width,
        int height,
        const unsigned char* pixels,
        int pixelFormat)
    {
        Handle(Image_AlienPixMap) pixmap = createMarkerPixmap(width, height, pixels, pixelFormat);
        Handle(Graphic3d_AspectMarker3d) markerAspect = new Graphic3d_AspectMarker3d(pixmap);
        presentation->Attributes()->SetPointAspect(new Prs3d_PointAspect(markerAspect));
    }

    void validatePointOptions(const OcctViewerPointOptions* options, bool isUpdate)
    {
        if (options == nullptr) throw std::invalid_argument("Viewer point options are null.");
        if (options->structSize < sizeof(OcctViewerPointOptions) ||
            options->apiVersion != PointOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported viewer point options size or version.");
        }
        if ((options->updateMask & ~AllPointUpdateBits) != 0 ||
            (isUpdate && options->updateMask == 0))
        {
            throw std::invalid_argument("Viewer point update mask is invalid.");
        }
        if (!isUpdate && options->updateMask != AllPointUpdateBits)
            throw std::invalid_argument("Point creation requires position and style.");

        if (!isUpdate || (options->updateMask & OcctViewerPointUpdate_Position) != 0)
            requireFinitePoint(options->position);
        if (!isUpdate || (options->updateMask & OcctViewerPointUpdate_Style) != 0)
        {
            requirePositive(options->scale, "Point marker scale");
            (void)pointMarker(options->marker);
            (void)color(options->red, options->green, options->blue);
        }
    }

    void validatePointPixmapOptions(const OcctViewerPointPixmapOptions* options, bool isUpdate)
    {
        if (options == nullptr) throw std::invalid_argument("Viewer point pixmap options are null.");
        if (options->structSize < sizeof(OcctViewerPointPixmapOptions) ||
            options->apiVersion != PointPixmapOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported viewer point pixmap options size or version.");
        }
        if ((options->updateMask & ~AllPointPixmapUpdateBits) != 0 ||
            (isUpdate && options->updateMask == 0))
        {
            throw std::invalid_argument("Viewer point pixmap update mask is invalid.");
        }
        if (!isUpdate && options->updateMask != AllPointPixmapUpdateBits)
            throw std::invalid_argument("Pixmap point creation requires position and image.");

        if (!isUpdate || (options->updateMask & OcctViewerPointPixmapUpdate_Position) != 0)
            requireFinitePoint(options->position);
        if (!isUpdate || (options->updateMask & OcctViewerPointPixmapUpdate_Image) != 0)
        {
            validatePixmapData(
                options->width,
                options->height,
                options->pixels,
                options->pixelCount,
                options->pixelFormat);
        }
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executePointStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    template<typename Function>
    OcctStatus executePointObjectStatus(
        Engine* engine,
        OcctObjectId* output,
        Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        if (output == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Result point ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }

        *output = 0;
        const OcctObjectId value = executeObject(engine, std::forward<Function>(function));
        if (value == 0) return engine->currentErrorCode();
        *output = value;
        return OcctStatus_Ok;
    }
}

extern "C"
{
    OcctStatus occt_engine_point_create(
        OcctEngineHandle handle,
        const OcctViewerPointOptions* options,
        OcctObjectId* resultPointId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePointObjectStatus(engine, resultPointId, [&]
        {
            validatePointOptions(options, false);
            Handle(Geom_CartesianPoint) component = new Geom_CartesianPoint(point(options->position));
            Handle(AIS_Point) presentation = new AIS_Point(component);
            applyPointStyle(
                presentation,
                options->marker,
                options->scale,
                options->red,
                options->green,
                options->blue);
            return engine->addPresentation(presentation, OcctPointObjectKind, "Point");
        });
    }

    OcctStatus occt_engine_point_update(
        OcctEngineHandle handle,
        OcctObjectId pointId,
        const OcctViewerPointOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePointStatus(engine, [&]
        {
            validatePointOptions(options, true);
            Handle(AIS_Point) presentation = requiredPoint(engine, pointId);

            if ((options->updateMask & OcctViewerPointUpdate_Position) != 0)
            {
                Handle(Geom_CartesianPoint) component =
                    Handle(Geom_CartesianPoint)::DownCast(presentation->Component());
                if (component.IsNull())
                    presentation->SetComponent(new Geom_CartesianPoint(point(options->position)));
                else
                    component->SetPnt(point(options->position));
            }
            if ((options->updateMask & OcctViewerPointUpdate_Style) != 0)
            {
                applyPointStyle(
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

    OcctStatus occt_engine_point_pixmap_create(
        OcctEngineHandle handle,
        const OcctViewerPointPixmapOptions* options,
        OcctObjectId* resultPointId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePointObjectStatus(engine, resultPointId, [&]
        {
            validatePointPixmapOptions(options, false);
            Handle(Geom_CartesianPoint) component = new Geom_CartesianPoint(point(options->position));
            Handle(AIS_Point) presentation = new AIS_Point(component);
            applyPixmapStyle(
                presentation,
                options->width,
                options->height,
                options->pixels,
                options->pixelFormat);
            return engine->addPresentation(presentation, OcctPointObjectKind, "Point");
        });
    }

    OcctStatus occt_engine_point_pixmap_update(
        OcctEngineHandle handle,
        OcctObjectId pointId,
        const OcctViewerPointPixmapOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executePointStatus(engine, [&]
        {
            validatePointPixmapOptions(options, true);
            Handle(AIS_Point) presentation = requiredPoint(engine, pointId);

            if ((options->updateMask & OcctViewerPointPixmapUpdate_Position) != 0)
            {
                Handle(Geom_CartesianPoint) component =
                    Handle(Geom_CartesianPoint)::DownCast(presentation->Component());
                if (component.IsNull())
                    presentation->SetComponent(new Geom_CartesianPoint(point(options->position)));
                else
                    component->SetPnt(point(options->position));
            }
            if ((options->updateMask & OcctViewerPointPixmapUpdate_Image) != 0)
            {
                applyPixmapStyle(
                    presentation,
                    options->width,
                    options->height,
                    options->pixels,
                    options->pixelFormat);
            }

            engine->viewerContext.context->Redisplay(presentation, Standard_False);
            engine->requestRedraw();
        });
    }
}
