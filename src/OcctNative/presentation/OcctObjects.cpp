#include "presentation/OcctObjects.h"
#include "core/OcctInternal.hxx"
#include "selection/OcctManipulator.h"

#include <AIS_Manipulator.hxx>
#include <Graphic3d_MaterialAspect.hxx>

#include <algorithm>
#include <cstring>
#include <stdexcept>
#include <string>
#include <unordered_set>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t ObjectUpdateApiVersion = 1;
    constexpr std::uint32_t AllObjectUpdateBits =
        OcctViewerObjectUpdate_Name |
        OcctViewerObjectUpdate_ApplicationTag |
        OcctViewerObjectUpdate_Color |
        OcctViewerObjectUpdate_Transparency |
        OcctViewerObjectUpdate_Visibility |
        OcctViewerObjectUpdate_LineWidth |
        OcctViewerObjectUpdate_Material |
        OcctViewerObjectUpdate_Selectable;

    OcctStatus requireEngine(Engine* engine)
    {
        return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_Ok;
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeObjectStatus(Engine* engine, bool requireInitialized, Function&& function)
    {
        const OcctStatus status = requireInitialized
            ? requireInitializedEngine(engine)
            : requireEngine(engine);
        if (status != OcctStatus_Ok) return status;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    ObjectEntry& requiredObject(Engine* engine, OcctObjectId objectId)
    {
        ObjectEntry* entry = engine->findObject(objectId);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    const ObjectEntry& requiredObject(const Engine* engine, OcctObjectId objectId)
    {
        const ObjectEntry* entry = engine->findObject(objectId);
        if (entry == nullptr)
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    void stopAndDetachManipulator(const Handle(AIS_Manipulator)& manipulator)
    {
        if (manipulator.IsNull()) return;
        if (manipulator->HasActiveTransformation()) manipulator->StopTransform(Standard_False);
        if (manipulator->IsAttached()) manipulator->Detach();
        else if (manipulator->HasActiveMode()) manipulator->DeactivateCurrentMode();
    }

    void detachManipulatorsReferencing(
        Engine* engine,
        const std::unordered_set<OcctObjectId>& objectIds)
    {
        if (objectIds.empty()) return;
        for (auto& pair : engine->scene.objects)
        {
            ObjectEntry& entry = pair.second;
            if (entry.kind != OcctManipulatorObjectKind || entry.presentation.IsNull()) continue;

            Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
            if (manipulator.IsNull()) continue;
            if (objectIds.find(pair.first) != objectIds.end())
            {
                stopAndDetachManipulator(manipulator);
                continue;
            }
            if (!manipulator->IsAttached()) continue;

            const Handle(AIS_ManipulatorObjectSequence)& targets = manipulator->Objects();
            if (targets.IsNull()) continue;
            for (int index = targets->Lower(); index <= targets->Upper(); ++index)
            {
                const OcctObjectId targetId = engine->findPresentation(targets->Value(index));
                if (objectIds.find(targetId) != objectIds.end())
                {
                    stopAndDetachManipulator(manipulator);
                    break;
                }
            }
        }
    }

    void restoreManipulatorModes(ObjectEntry& entry)
    {
        Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
        if (manipulator.IsNull() || !manipulator->IsAttached()) return;
        if ((entry.presentationSubtype & (1 << OcctManipulator_Translation)) != 0)
            manipulator->EnableMode(AIS_MM_Translation);
        if ((entry.presentationSubtype & (1 << OcctManipulator_Rotation)) != 0)
            manipulator->EnableMode(AIS_MM_Rotation);
        if ((entry.presentationSubtype & (1 << OcctManipulator_Scaling)) != 0)
            manipulator->EnableMode(AIS_MM_Scaling);
        if ((entry.presentationSubtype & (1 << OcctManipulator_TranslationPlane)) != 0)
            manipulator->EnableMode(AIS_MM_TranslationPlane);
    }

    void setSelectable(Engine* engine, ObjectEntry& entry, bool selectable)
    {
        if (entry.selectable == selectable) return;
        entry.selectable = selectable;
        if (!selectable)
        {
            if (engine->viewerContext.context->IsSelected(entry.presentation))
                engine->viewerContext.context->AddOrRemoveSelected(entry.presentation, Standard_False);
            if (entry.kind == OcctManipulatorObjectKind)
            {
                Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
                if (!manipulator.IsNull())
                {
                    if (manipulator->HasActiveTransformation()) manipulator->StopTransform(Standard_False);
                    if (manipulator->HasActiveMode()) manipulator->DeactivateCurrentMode();
                }
            }
            engine->viewerContext.context->Deactivate(entry.presentation);
        }
        else if (entry.kind == OcctManipulatorObjectKind)
        {
            restoreManipulatorModes(entry);
        }
        else
        {
            engine->applySelectionMode(entry.presentation);
        }
    }

    void updateApplicationTag(Engine* engine, OcctObjectId objectId, ObjectEntry& entry, const char* utf8Tag)
    {
        const std::string tag = utf8Tag == nullptr ? std::string() : std::string(utf8Tag);
        if (tag == entry.applicationTag) return;
        if (!tag.empty())
        {
            const auto existing = engine->scene.objectIdByApplicationTag.find(tag);
            if (existing != engine->scene.objectIdByApplicationTag.end() &&
                existing->second != objectId &&
                engine->findObject(existing->second) != nullptr)
            {
                throw std::invalid_argument("ApplicationTag must be unique within an engine.");
            }
        }
        if (!entry.applicationTag.empty())
            engine->scene.objectIdByApplicationTag.erase(entry.applicationTag);
        entry.applicationTag = tag;
        if (!tag.empty()) engine->scene.objectIdByApplicationTag[tag] = objectId;
    }

    void validateObjectUpdate(const OcctViewerObjectUpdateOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Object update options are null.");
        if (options->structSize < sizeof(OcctViewerObjectUpdateOptions) ||
            options->apiVersion != ObjectUpdateApiVersion)
        {
            throw std::invalid_argument("Unsupported object update options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllObjectUpdateBits) != 0)
            throw std::invalid_argument("Object update mask is invalid.");
        if ((options->updateMask & OcctViewerObjectUpdate_Color) != 0)
            (void)color(options->color.r, options->color.g, options->color.b);
        if ((options->updateMask & OcctViewerObjectUpdate_Transparency) != 0 &&
            (!std::isfinite(options->transparency) || options->transparency < 0.0 || options->transparency > 1.0))
        {
            throw std::invalid_argument("Object transparency must be between 0 and 1.");
        }
        if ((options->updateMask & OcctViewerObjectUpdate_LineWidth) != 0)
            requirePositive(options->lineWidth, "Line width");
        if ((options->updateMask & OcctViewerObjectUpdate_Material) != 0)
            (void)materialName(options->material);
    }

    void writeUtf8Buffer(const std::string& value, char* buffer, int capacity, int* requiredBytes)
    {
        if (requiredBytes == nullptr)
            throw std::invalid_argument("Required byte count output is null.");
        if (capacity < 0) throw std::invalid_argument("UTF-8 buffer capacity must not be negative.");
        const std::size_t required = value.size() + 1U;
        if (required > static_cast<std::size_t>(std::numeric_limits<int>::max()))
            throw std::overflow_error("UTF-8 value is too large.");
        *requiredBytes = static_cast<int>(required);
        if (buffer == nullptr)
        {
            if (capacity != 0) throw std::invalid_argument("UTF-8 output buffer is null but capacity is non-zero.");
            return;
        }
        if (capacity < *requiredBytes)
            throw std::out_of_range("UTF-8 output buffer capacity is too small.");
        std::memcpy(buffer, value.c_str(), required);
    }
}

extern "C"
{
    OcctStatus occt_engine_objects_snapshot_get(
        OcctEngineHandle handle,
        OcctObjectDescriptor* items,
        int capacity,
        int* objectCount,
        int* shapeCount)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, false, [&]
        {
            if (objectCount == nullptr || shapeCount == nullptr)
                throw std::invalid_argument("Object snapshot count output is null.");
            if (capacity < 0)
                throw std::invalid_argument("Object descriptor capacity must not be negative.");

            *objectCount = static_cast<int>(engine->scene.objects.size());
            *shapeCount = 0;
            for (const auto& pair : engine->scene.objects)
                if (pair.second.kind == OcctObject_Shape) ++(*shapeCount);

            if (items == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Object descriptor output is null but capacity is non-zero.");
                return;
            }
            if (capacity < *objectCount)
                throw std::out_of_range("Object descriptor output capacity is too small.");

            std::vector<OcctObjectDescriptor> descriptors;
            descriptors.reserve(engine->scene.objects.size());
            for (const auto& pair : engine->scene.objects)
                descriptors.push_back({pair.first, pair.second.kind});
            std::sort(descriptors.begin(), descriptors.end(), [](const auto& left, const auto& right)
            {
                return left.objectId < right.objectId;
            });
            std::copy(descriptors.begin(), descriptors.end(), items);
        });
    }

    OcctStatus occt_engine_object_exists(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int* exists)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (exists == nullptr) return OcctStatus_ErrorInvalidArgument;
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        *exists = engine->findObject(objectId) != nullptr ? 1 : 0;
        return OcctStatus_Ok;
    }

    OcctStatus occt_engine_object_kind_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int* kind)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, false, [&]
        {
            if (kind == nullptr) throw std::invalid_argument("Object kind output is null.");
            *kind = requiredObject(engine, objectId).kind;
        });
    }

    OcctStatus occt_engine_object_update(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerObjectUpdateOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, true, [&]
        {
            validateObjectUpdate(options);
            ObjectEntry& entry = requiredObject(engine, objectId);
            const std::uint32_t mask = options->updateMask;

            if ((mask & OcctViewerObjectUpdate_Name) != 0)
            {
                entry.name = options->name == nullptr ? "" : options->name;
                if (entry.kind == OcctObject_Shape && !syncStepObjectName(engine, entry))
                    engine->invalidatePristineStepDocument();
            }
            if ((mask & OcctViewerObjectUpdate_ApplicationTag) != 0)
                updateApplicationTag(engine, objectId, entry, options->applicationTag);
            if ((mask & OcctViewerObjectUpdate_Color) != 0)
            {
                const Quantity_Color value = color(options->color.r, options->color.g, options->color.b);
                entry.hasStoredColor = true;
                entry.storedColorR = value.Red();
                entry.storedColorG = value.Green();
                entry.storedColorB = value.Blue();
                if (entry.kind == OcctObject_Shape && !syncStepObjectColor(engine, entry))
                    engine->invalidatePristineStepDocument();
                engine->viewerContext.context->SetColor(entry.presentation, value, Standard_False);
            }
            if ((mask & OcctViewerObjectUpdate_Transparency) != 0)
            {
                entry.storedColorA = 1.0 - options->transparency;
                entry.hasStoredAlpha = true;
                if (entry.kind == OcctObject_Shape && !syncStepObjectColor(engine, entry))
                    engine->invalidatePristineStepDocument();
                engine->viewerContext.context->SetTransparency(
                    entry.presentation,
                    options->transparency,
                    Standard_False);
            }
            if ((mask & OcctViewerObjectUpdate_Visibility) != 0)
            {
                entry.storedVisible = options->visible != 0;
                if (entry.kind == OcctObject_Shape && !syncStepObjectVisibility(engine, entry))
                    engine->invalidatePristineStepDocument();
                if (options->visible != 0)
                    engine->viewerContext.context->Display(entry.presentation, Standard_False);
                else
                    engine->viewerContext.context->Erase(entry.presentation, Standard_False);
            }
            if ((mask & OcctViewerObjectUpdate_LineWidth) != 0)
                engine->viewerContext.context->SetWidth(entry.presentation, options->lineWidth, Standard_False);
            if ((mask & OcctViewerObjectUpdate_Material) != 0)
            {
                if (entry.kind == OcctObject_Shape) engine->invalidatePristineStepDocument();
                engine->viewerContext.context->SetMaterial(
                    entry.presentation,
                    Graphic3d_MaterialAspect(materialName(options->material)),
                    Standard_False);
            }
            if ((mask & OcctViewerObjectUpdate_Selectable) != 0)
                setSelectable(engine, entry, options->selectable != 0);

            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_name_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, false, [&]
        {
            writeUtf8Buffer(requiredObject(engine, objectId).name, utf8Buffer, capacity, requiredBytes);
        });
    }

    OcctStatus occt_engine_object_application_tag_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, false, [&]
        {
            writeUtf8Buffer(requiredObject(engine, objectId).applicationTag, utf8Buffer, capacity, requiredBytes);
        });
    }

    OcctStatus occt_engine_object_find_by_application_tag(
        OcctEngineHandle handle,
        const char* utf8Tag,
        OcctObjectId* objectId,
        int* found)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, false, [&]
        {
            if (objectId == nullptr || found == nullptr)
                throw std::invalid_argument("ApplicationTag lookup output is null.");
            *objectId = 0;
            *found = 0;
            if (utf8Tag == nullptr || *utf8Tag == '\0') return;
            const auto iterator = engine->scene.objectIdByApplicationTag.find(utf8Tag);
            if (iterator == engine->scene.objectIdByApplicationTag.end()) return;
            if (engine->findObject(iterator->second) == nullptr)
            {
                engine->scene.objectIdByApplicationTag.erase(iterator);
                return;
            }
            *objectId = iterator->second;
            *found = 1;
        });
    }

    OcctStatus occt_engine_objects_delete(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, true, [&]
        {
            if (count < 0) throw std::invalid_argument("Object count must not be negative.");
            if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");

            std::vector<OcctObjectId> uniqueIds;
            std::unordered_set<OcctObjectId> seenIds;
            uniqueIds.reserve(static_cast<std::size_t>(count));
            seenIds.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
            {
                const OcctObjectId id = objectIds[index];
                if (engine->findObject(id) == nullptr) throw std::invalid_argument("Object ID does not exist.");
                if (seenIds.insert(id).second) uniqueIds.push_back(id);
            }
            detachManipulatorsReferencing(engine, seenIds);
            for (const OcctObjectId id : uniqueIds) engine->erase(id);
            if (!uniqueIds.empty()) engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_objects_clear(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, true, [&]
        {
            std::unordered_set<OcctObjectId> allIds;
            allIds.reserve(engine->scene.objects.size());
            for (const auto& pair : engine->scene.objects) allIds.insert(pair.first);
            detachManipulatorsReferencing(engine, allIds);

            for (auto& pair : engine->scene.objects)
                if (!pair.second.presentation.IsNull())
                    engine->viewerContext.context->Remove(pair.second.presentation, Standard_False);
            engine->scene.clear();
            engine->documents.clear();
            engine->viewerContext.context->ClearSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_objects_visibility_all_set(OcctEngineHandle handle, int visible)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, true, [&]
        {
            for (auto& pair : engine->scene.objects)
            {
                ObjectEntry& entry = pair.second;
                if (entry.presentation.IsNull()) continue;
                entry.storedVisible = visible != 0;
                if (entry.kind == OcctObject_Shape && !syncStepObjectVisibility(engine, entry))
                    engine->invalidatePristineStepDocument();
                if (visible != 0)
                    engine->viewerContext.context->Display(entry.presentation, Standard_False);
                else
                    engine->viewerContext.context->Erase(entry.presentation, Standard_False);
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_presentation_action(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int action)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, true, [&]
        {
            ObjectEntry& entry = requiredObject(engine, objectId);
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
                    throw std::invalid_argument("Object presentation action is out of range.");
            }
            engine->requestRedraw();
        });
    }
}
