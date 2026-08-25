#include "presentation/OcctViewerDecorations.h"
#include "core/OcctInternal.hxx"

#include <Aspect_TypeOfLine.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_Vec2.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_LineAspect.hxx>

#include <cmath>
#include <stdexcept>
#include <unordered_set>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t DecorationsApiVersion = 1;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeDecorationsStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

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
        throw std::runtime_error("Object uses a Z-layer outside the supported predefined set.");
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

    std::vector<ObjectEntry*> requireObjects(
        Engine* engine,
        const OcctObjectId* ids,
        int count,
        bool shapesOnly)
    {
        if (count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (count > 0 && ids == nullptr) throw std::invalid_argument("Object ID array is null.");

        std::vector<ObjectEntry*> entries;
        std::unordered_set<OcctObjectId> seen;
        entries.reserve(static_cast<std::size_t>(count));
        seen.reserve(static_cast<std::size_t>(count));
        for (int index = 0; index < count; ++index)
        {
            const OcctObjectId id = ids[index];
            if (!seen.insert(id).second) continue;
            entries.push_back(shapesOnly ? &requiredShape(engine, id) : &requiredObject(engine, id));
        }
        return entries;
    }

    void validateColor(const OcctColorRgb& value)
    {
        const double components[] = { value.r, value.g, value.b };
        for (double component : components)
        {
            if (!std::isfinite(component) || component < 0.0 || component > 1.0)
                throw std::invalid_argument("Color components must be finite and between 0 and 1.");
        }
    }

    void validateTriedronOptions(const OcctViewerTriedronOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Triedron options are null.");
        if (options->structSize < sizeof(OcctViewerTriedronOptions) ||
            options->apiVersion != DecorationsApiVersion)
        {
            throw std::invalid_argument("Unsupported triedron options size or version.");
        }
        (void)cornerPosition(options->position);
        if (!std::isfinite(options->scale) || options->scale <= 0.0)
            throw std::invalid_argument("Triedron scale must be finite and greater than zero.");
        validateColor(options->color);
    }

    void validateViewCubeOptions(const OcctViewerViewCubeOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("View cube options are null.");
        if (options->structSize < sizeof(OcctViewerViewCubeOptions) ||
            options->apiVersion != DecorationsApiVersion)
        {
            throw std::invalid_argument("Unsupported view cube options size or version.");
        }
        (void)cornerPosition(options->position);
        if (options->sizePixels <= 0 || options->sizePixels > 4096)
            throw std::invalid_argument("View cube size must be between 1 and 4096 pixels.");
        if (options->offsetX < 0 || options->offsetY < 0)
            throw std::invalid_argument("View cube offsets must not be negative.");
        if (options->hasTextColor != 0) validateColor(options->textColor);
        if (options->hasBoxColor != 0) validateColor(options->boxColor);
        if (options->hasFacetColor != 0) validateColor(options->facetColor);
    }

    void validateFaceBoundaryOptions(const OcctViewerFaceBoundaryOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Face boundary options are null.");
        if (options->structSize < sizeof(OcctViewerFaceBoundaryOptions) ||
            options->apiVersion != DecorationsApiVersion)
        {
            throw std::invalid_argument("Unsupported face boundary options size or version.");
        }
        validateColor(options->color);
        if (!std::isfinite(options->width) || options->width <= 0.0)
            throw std::invalid_argument("Face boundary width must be finite and greater than zero.");
    }

    void applyFaceBoundaryStyle(
        Engine* engine,
        ObjectEntry& entry,
        const OcctViewerFaceBoundaryOptions& options)
    {
        const Handle(AIS_Shape) aisShape = Handle(AIS_Shape)::DownCast(entry.presentation);
        if (aisShape.IsNull()) throw std::invalid_argument("Object is not an AIS_Shape.");

        const Handle(Prs3d_Drawer)& drawer = aisShape->Attributes();
        drawer->SetFaceBoundaryDraw(options.visible != 0);
        drawer->SetFaceBoundaryAspect(new Prs3d_LineAspect(
            color(options.color.r, options.color.g, options.color.b),
            Aspect_TOL_SOLID,
            options.width));
        engine->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
    }
}

extern "C"
{
    OcctStatus occt_engine_objects_z_layer_set(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int layer)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDecorationsStatus(engine, [&]
        {
            const Graphic3d_ZLayerId nativeLayer = zLayerId(layer);
            const auto entries = requireObjects(engine, objectIds, count, false);
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetZLayer(entry->presentation, nativeLayer);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_z_layer_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int* layer)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDecorationsStatus(engine, [&]
        {
            if (layer == nullptr) throw std::invalid_argument("Z-layer output is null.");
            ObjectEntry& entry = requiredObject(engine, objectId);
            *layer = zLayerValue(engine->viewerContext.context->GetZLayer(entry.presentation));
        });
    }

    OcctStatus occt_engine_triedron_update(
        OcctEngineHandle handle,
        const OcctViewerTriedronOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDecorationsStatus(engine, [&]
        {
            validateTriedronOptions(options);
            if (options->visible != 0)
            {
                engine->viewerContext.view->TriedronDisplay(
                    cornerPosition(options->position),
                    color(options->color.r, options->color.g, options->color.b),
                    options->scale,
                    V3d_ZBUFFER);
            }
            else
            {
                engine->viewerContext.view->TriedronErase();
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_view_cube_update(
        OcctEngineHandle handle,
        const OcctViewerViewCubeOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDecorationsStatus(engine, [&]
        {
            validateViewCubeOptions(options);
            if (engine->viewerContext.viewCube.IsNull())
                throw std::runtime_error("The view cube has not been initialized.");

            const Aspect_TypeOfTriedronPosition position = cornerPosition(options->position);
            engine->viewerContext.viewCube->SetSize(static_cast<double>(options->sizePixels));
            const int halfSize = options->sizePixels / 2;
            engine->viewerContext.viewCube->SetTransformPersistence(
                new Graphic3d_TransformPers(
                    Graphic3d_TMF_TriedronPers,
                    position,
                    Graphic3d_Vec2i(
                        halfSize + options->offsetX,
                        halfSize + options->offsetY)));

            if (options->fontHeight > 0.0)
            {
                engine->viewerContext.viewCube->SetFontHeight(options->fontHeight);
            }
            if (options->fontName != nullptr && options->fontName[0] != '\0')
            {
                engine->viewerContext.viewCube->SetFont(TCollection_AsciiString(options->fontName));
            }
            if (options->hasTextColor != 0)
            {
                engine->viewerContext.viewCube->SetTextColor(color(options->textColor.r, options->textColor.g, options->textColor.b));
            }
            if (options->hasBoxColor != 0)
            {
                engine->viewerContext.viewCube->SetBoxColor(color(options->boxColor.r, options->boxColor.g, options->boxColor.b));
            }
            if (options->hasFacetColor != 0)
            {
                const Quantity_Color facetCol = color(options->facetColor.r, options->facetColor.g, options->facetColor.b);
                engine->viewerContext.viewCube->DynamicHilightAttributes()->SetColor(facetCol);
                if (!engine->viewerContext.viewCube->DynamicHilightAttributes()->ShadingAspect().IsNull())
                {
                    engine->viewerContext.viewCube->DynamicHilightAttributes()->ShadingAspect()->SetColor(facetCol);
                }
            }
            if (options->cornerRadius >= 0.0 && options->cornerRadius <= 0.5)
            {
                engine->viewerContext.viewCube->SetRoundRadius(options->cornerRadius);
            }

            engine->viewerContext.viewCube->SynchronizeAspects();
            engine->viewerContext.viewCube->SetToUpdate();

            if (options->visible != 0)
            {
                engine->viewerContext.context->Display(engine->viewerContext.viewCube, Standard_False);
                engine->viewerContext.context->Redisplay(engine->viewerContext.viewCube, Standard_False, Standard_True);
                engine->viewerContext.context->RecomputePrsOnly(engine->viewerContext.viewCube, Standard_False, Standard_True);
            }
            else
            {
                engine->viewerContext.context->Erase(engine->viewerContext.viewCube, Standard_False);
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_face_boundary_update(
        OcctEngineHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        const OcctViewerFaceBoundaryOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeDecorationsStatus(engine, [&]
        {
            validateFaceBoundaryOptions(options);
            if (options->setDefault != 0)
            {
                if (count != 0 || shapeIds != nullptr)
                    throw std::invalid_argument("Default face boundary update must not include explicit shape IDs.");
                const Handle(Prs3d_Drawer)& drawer = engine->viewerContext.context->DefaultDrawer();
                drawer->SetFaceBoundaryDraw(options->visible != 0);
                drawer->SetFaceBoundaryAspect(new Prs3d_LineAspect(
                    color(options->color.r, options->color.g, options->color.b),
                    Aspect_TOL_SOLID,
                    options->width));

                if (options->applyExisting != 0)
                {
                    for (auto& pair : engine->scene.objects)
                    {
                        if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                        applyFaceBoundaryStyle(engine, pair.second, *options);
                    }
                }
                engine->requestRedraw();
                return;
            }

            if (options->applyExisting != 0)
                throw std::invalid_argument("applyExisting is valid only for default face boundary updates.");
            const auto entries = requireObjects(engine, shapeIds, count, true);
            for (ObjectEntry* entry : entries)
                applyFaceBoundaryStyle(engine, *entry, *options);
            if (!entries.empty()) engine->requestRedraw();
        });
    }
}
