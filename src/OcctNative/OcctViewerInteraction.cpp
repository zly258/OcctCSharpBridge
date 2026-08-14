#include "core/OcctInternal.hxx"
#include "OcctViewerInteraction.h"

#include <Aspect_TypeOfLine.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_LineAspect.hxx>

using namespace OcctBridge;

namespace
{
    Aspect_TypeOfTriedronPosition cornerPosition(int value)
    {
        switch (value)
        {
            case OcctCorner_LeftLower: return Aspect_TOTP_LEFT_LOWER;
            case OcctCorner_LeftUpper: return Aspect_TOTP_LEFT_UPPER;
            case OcctCorner_RightLower: return Aspect_TOTP_RIGHT_LOWER;
            case OcctCorner_RightUpper: return Aspect_TOTP_RIGHT_UPPER;
            default: throw std::invalid_argument("Corner position is out of range.");
        }
    }

    Graphic3d_ZLayerId zLayerId(int value)
    {
        switch (value)
        {
            case OcctViewerZLayer_Bottom: return Graphic3d_ZLayerId_BotOSD;
            case OcctViewerZLayer_Default: return Graphic3d_ZLayerId_Default;
            case OcctViewerZLayer_Top: return Graphic3d_ZLayerId_Top;
            case OcctViewerZLayer_Topmost: return Graphic3d_ZLayerId_Topmost;
            default: throw std::invalid_argument("Z-layer value is out of range.");
        }
    }

    int zLayerValue(Graphic3d_ZLayerId value)
    {
        if (value == Graphic3d_ZLayerId_BotOSD) return OcctViewerZLayer_Bottom;
        if (value == Graphic3d_ZLayerId_Default) return OcctViewerZLayer_Default;
        if (value == Graphic3d_ZLayerId_Top) return OcctViewerZLayer_Top;
        if (value == Graphic3d_ZLayerId_Topmost) return OcctViewerZLayer_Topmost;
        throw std::runtime_error("Object uses a Z-layer outside the managed predefined set.");
    }

    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }

    void applyFaceBoundaryStyle(
        Engine* engine,
        ObjectEntry& entry,
        int visible,
        double r,
        double g,
        double b,
        double width)
    {
        requirePositive(width, "Face boundary width");
        const Handle(AIS_Shape) aisShape = Handle(AIS_Shape)::DownCast(entry.presentation);
        if (aisShape.IsNull()) throw std::invalid_argument("Object is not an AIS_Shape.");

        const Handle(Prs3d_Drawer)& drawer = aisShape->Attributes();
        drawer->SetFaceBoundaryDraw(visible != 0);
        drawer->SetFaceBoundaryAspect(
            new Prs3d_LineAspect(color(r, g, b), Aspect_TOL_SOLID, width));
        engine->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
    }
}

extern "C"
{
    int occt_set_object_z_layer(OcctHandle h, OcctObjectId objectId, int layer)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            e->viewerContext.context->SetZLayer(entry.presentation, zLayerId(layer));
            e->requestRedraw();
        });
    }

    int occt_set_objects_z_layer(
        OcctHandle h,
        const OcctObjectId* objectIds,
        int count,
        int layer)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count < 0) throw std::invalid_argument("Object count must not be negative.");
            if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");
            const Graphic3d_ZLayerId nativeLayer = zLayerId(layer);

            std::vector<ObjectEntry*> entries;
            entries.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
                entries.push_back(&requiredObject(e, objectIds[index]));

            for (ObjectEntry* entry : entries)
                e->viewerContext.context->SetZLayer(entry->presentation, nativeLayer);
            if (!entries.empty()) e->requestRedraw();
        });
    }

    int occt_get_object_z_layer(OcctHandle h, OcctObjectId objectId, int* layer)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || layer == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, objectId);
            *layer = zLayerValue(e->viewerContext.context->GetZLayer(entry.presentation));
        });
    }

    int occt_set_triedron_options(OcctHandle h, const OcctTriedronOptions* options)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || options == nullptr) return 0;
        return execute(e, [&]
        {
            requirePositive(options->scale, "Triedron scale");
            const Aspect_TypeOfTriedronPosition position = cornerPosition(options->position);
            if (options->visible != 0)
            {
                e->viewerContext.view->TriedronDisplay(
                    position,
                    color(options->r, options->g, options->b),
                    options->scale,
                    V3d_ZBUFFER);
            }
            else
            {
                e->viewerContext.view->TriedronErase();
            }
            e->requestRedraw();
        });
    }

    int occt_set_view_cube_options(OcctHandle h, const OcctViewCubeOptions* options)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || options == nullptr) return 0;
        return execute(e, [&]
        {
            if (e->viewerContext.viewCube.IsNull()) throw std::runtime_error("The view cube has not been initialized.");
            if (options->sizePixels <= 0 || options->sizePixels > 4096)
                throw std::invalid_argument("View cube size must be between 1 and 4096 pixels.");
            if (options->offsetX < 0 || options->offsetY < 0)
                throw std::invalid_argument("View cube offsets must not be negative.");

            const Aspect_TypeOfTriedronPosition position = cornerPosition(options->position);
            e->viewerContext.viewCube->SetSize(static_cast<double>(options->sizePixels));
            const int halfSize = options->sizePixels / 2;
            e->viewerContext.viewCube->SetTransformPersistence(
                new Graphic3d_TransformPers(
                    Graphic3d_TMF_TriedronPers,
                    position,
                    Graphic3d_Vec2i(
                        halfSize + options->offsetX,
                        halfSize + options->offsetY)));
            e->viewerContext.viewCube->SetToUpdate();

            if (options->visible != 0)
            {
                e->viewerContext.context->Display(e->viewerContext.viewCube, Standard_False);
                e->viewerContext.context->Redisplay(e->viewerContext.viewCube, Standard_False);
            }
            else
            {
                e->viewerContext.context->Erase(e->viewerContext.viewCube, Standard_False);
            }
            e->requestRedraw();
        });
    }

    int occt_set_face_boundary_style(
        OcctHandle h,
        OcctObjectId shapeId,
        int visible,
        double r,
        double g,
        double b,
        double width)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredShape(e, shapeId);
            applyFaceBoundaryStyle(e, entry, visible, r, g, b, width);
            e->requestRedraw();
        });
    }

    int occt_set_face_boundary_styles(
        OcctHandle h,
        const OcctObjectId* shapeIds,
        int count,
        int visible,
        double r,
        double g,
        double b,
        double width)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count < 0) throw std::invalid_argument("Shape count must not be negative.");
            if (count > 0 && shapeIds == nullptr) throw std::invalid_argument("Shape ID array is null.");
            requirePositive(width, "Face boundary width");

            std::vector<ObjectEntry*> entries;
            entries.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
                entries.push_back(&requiredShape(e, shapeIds[index]));

            for (ObjectEntry* entry : entries)
                applyFaceBoundaryStyle(e, *entry, visible, r, g, b, width);
            if (!entries.empty()) e->requestRedraw();
        });
    }

    int occt_set_default_face_boundary_style(
        OcctHandle h,
        int visible,
        double r,
        double g,
        double b,
        double width,
        int applyExisting)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(width, "Face boundary width");
            const Handle(Prs3d_Drawer)& drawer = e->viewerContext.context->DefaultDrawer();
            drawer->SetFaceBoundaryDraw(visible != 0);
            drawer->SetFaceBoundaryAspect(
                new Prs3d_LineAspect(color(r, g, b), Aspect_TOL_SOLID, width));

            if (applyExisting != 0)
            {
                for (auto& pair : e->scene.objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    applyFaceBoundaryStyle(e, pair.second, visible, r, g, b, width);
                }
            }
            e->requestRedraw();
        });
    }
}
