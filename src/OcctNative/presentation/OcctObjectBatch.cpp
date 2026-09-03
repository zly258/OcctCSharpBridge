#include "presentation/OcctObjects.h"
#include "core/OcctInternal.hxx"

#include <Graphic3d_MaterialAspect.hxx>

#include <cmath>
#include <stdexcept>
#include <unordered_set>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t ObjectUpdateApiVersion = 1;
    constexpr std::uint32_t BatchObjectUpdateBits =
        OcctViewerObjectUpdate_Color |
        OcctViewerObjectUpdate_Transparency |
        OcctViewerObjectUpdate_Visibility |
        OcctViewerObjectUpdate_LineWidth |
        OcctViewerObjectUpdate_Material;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeBatchStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    std::vector<ObjectEntry*> requireObjects(
        Engine* engine,
        const OcctObjectId* objectIds,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");

        std::vector<ObjectEntry*> entries;
        std::unordered_set<OcctObjectId> uniqueIds;
        entries.reserve(static_cast<std::size_t>(count));
        uniqueIds.reserve(static_cast<std::size_t>(count));
        for (int index = 0; index < count; ++index)
        {
            const OcctObjectId id = objectIds[index];
            if (!uniqueIds.insert(id).second) continue;
            ObjectEntry* entry = engine->findObject(id);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            entries.push_back(entry);
        }
        return entries;
    }

    void validateBatchUpdate(const OcctViewerObjectUpdateOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Object update options are null.");
        if (options->structSize < sizeof(OcctViewerObjectUpdateOptions) ||
            options->apiVersion != ObjectUpdateApiVersion)
        {
            throw std::invalid_argument("Unsupported object update options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~BatchObjectUpdateBits) != 0)
            throw std::invalid_argument("Bulk object update contains unsupported fields.");
        if ((options->updateMask & OcctViewerObjectUpdate_Transparency) != 0 &&
            (!std::isfinite(options->transparency) || options->transparency < 0.0 || options->transparency > 1.0))
        {
            throw std::invalid_argument("Transparency must be between 0 and 1.");
        }
        if ((options->updateMask & OcctViewerObjectUpdate_LineWidth) != 0 &&
            (!std::isfinite(options->lineWidth) || options->lineWidth <= 0.0))
        {
            throw std::invalid_argument("Line width must be finite and greater than zero.");
        }
        if ((options->updateMask & OcctViewerObjectUpdate_Color) != 0)
        {
            const double values[] = { options->color.r, options->color.g, options->color.b };
            for (double value : values)
            {
                if (!std::isfinite(value) || value < 0.0 || value > 1.0)
                    throw std::invalid_argument("Object color components must be between 0 and 1.");
            }
        }
        if ((options->updateMask & OcctViewerObjectUpdate_Material) != 0)
            (void)materialName(options->material);
    }

    void applyBatchUpdate(
        Engine* engine,
        ObjectEntry& entry,
        const OcctViewerObjectUpdateOptions& options)
    {
        if ((options.updateMask & OcctViewerObjectUpdate_Color) != 0)
        {
            const Quantity_Color value = color(options.color.r, options.color.g, options.color.b);
            entry.hasStoredColor = true;
            entry.storedColorR = value.Red();
            entry.storedColorG = value.Green();
            entry.storedColorB = value.Blue();
            if (entry.kind == OcctObject_Shape && !syncStepObjectColor(engine, entry))
                engine->invalidatePristineStepDocument();
            setObjectColorPreservingFaceBoundary(engine, entry, value);
        }
        if ((options.updateMask & OcctViewerObjectUpdate_Transparency) != 0)
        {
            entry.storedColorA = 1.0 - options.transparency;
            entry.hasStoredAlpha = true;
            if (entry.kind == OcctObject_Shape && !syncStepObjectColor(engine, entry))
                engine->invalidatePristineStepDocument();
            engine->viewerContext.context->SetTransparency(
                entry.presentation,
                options.transparency,
                Standard_False);
        }
        if ((options.updateMask & OcctViewerObjectUpdate_Visibility) != 0)
        {
            entry.storedVisible = options.visible != 0;
            if (entry.kind == OcctObject_Shape && !syncStepObjectVisibility(engine, entry))
                engine->invalidatePristineStepDocument();
            if (options.visible != 0)
                engine->viewerContext.context->Display(entry.presentation, Standard_False);
            else
                engine->viewerContext.context->Erase(entry.presentation, Standard_False);
        }
        if ((options.updateMask & OcctViewerObjectUpdate_LineWidth) != 0)
            engine->viewerContext.context->SetWidth(entry.presentation, options.lineWidth, Standard_False);
        if ((options.updateMask & OcctViewerObjectUpdate_Material) != 0)
        {
            if (entry.kind == OcctObject_Shape) engine->invalidatePristineStepDocument();
            engine->viewerContext.context->SetMaterial(
                entry.presentation,
                Graphic3d_MaterialAspect(materialName(options.material)),
                Standard_False);
        }
    }

    void validatePresentationAction(int action)
    {
        if (action < OcctViewerObjectPresentation_Redisplay ||
            action > OcctViewerObjectPresentation_Unhighlight)
        {
            throw std::invalid_argument("Object presentation action is out of range.");
        }
    }

    void applyPresentationAction(Engine* engine, ObjectEntry& entry, int action)
    {
        switch (action)
        {
            case OcctViewerObjectPresentation_Redisplay:
                engine->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
                break;
            case OcctViewerObjectPresentation_Highlight:
                engine->viewerContext.context->HilightWithColor(
                    entry.presentation,
                    engine->viewerContext.context->HighlightStyle(),
                    Standard_False);
                break;
            case OcctViewerObjectPresentation_Unhighlight:
                engine->viewerContext.context->Unhilight(entry.presentation, Standard_False);
                break;
            default:
                break;
        }
    }
}

extern "C"
{
    OcctStatus occt_engine_objects_update(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count,
        const OcctViewerObjectUpdateOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeBatchStatus(engine, [&]
        {
            validateBatchUpdate(options);
            const auto entries = requireObjects(engine, objectIds, count);
            for (ObjectEntry* entry : entries)
                applyBatchUpdate(engine, *entry, *options);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_objects_presentation_action(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int action)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeBatchStatus(engine, [&]
        {
            validatePresentationAction(action);
            const auto entries = requireObjects(engine, objectIds, count);
            for (ObjectEntry* entry : entries)
                applyPresentationAction(engine, *entry, action);
            if (!entries.empty()) engine->requestRedraw();
        });
    }
}
