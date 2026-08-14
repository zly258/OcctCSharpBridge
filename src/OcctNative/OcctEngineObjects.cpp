#include "OcctInternal.hxx"
#include "selection/OcctManipulator.h"

#include <AIS_Manipulator.hxx>
#include <Graphic3d_MaterialAspect.hxx>

#include <unordered_set>

using namespace OcctBridge;

namespace
{
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
            bool referencesDeletedObject = false;
            for (int index = targets->Lower(); index <= targets->Upper(); ++index)
            {
                const OcctObjectId targetId = engine->findPresentation(targets->Value(index));
                if (objectIds.find(targetId) != objectIds.end())
                {
                    referencesDeletedObject = true;
                    break;
                }
            }
            if (referencesDeletedObject) stopAndDetachManipulator(manipulator);
        }
    }
}

extern "C"
{
    int occt_object_count(OcctHandle h)
    {
        Engine* e = engineOf(h);
        return e == nullptr ? 0 : static_cast<int>(e->scene.objects.size());
    }

    int occt_object_descriptors(
        OcctHandle h,
        OcctObjectDescriptor* items,
        int capacity,
        int* objectCount,
        int* shapeCount)
    {
        Engine* e = engineOf(h);
        if (e == nullptr || objectCount == nullptr || shapeCount == nullptr) return 0;
        return execute(e, [&]
        {
            if (capacity < 0)
                throw std::invalid_argument("Object descriptor capacity must not be negative.");

            *objectCount = static_cast<int>(e->scene.objects.size());
            *shapeCount = 0;
            for (const auto& pair : e->scene.objects)
            {
                if (pair.second.kind == OcctObject_Shape) ++(*shapeCount);
            }

            if (items == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Object descriptor output is null but capacity is non-zero.");
                return;
            }
            if (capacity < *objectCount)
                throw std::out_of_range("Object descriptor output capacity is too small.");

            std::vector<OcctObjectDescriptor> descriptors;
            descriptors.reserve(e->scene.objects.size());
            for (const auto& pair : e->scene.objects)
                descriptors.push_back(OcctObjectDescriptor{pair.first, pair.second.kind});

            std::sort(descriptors.begin(), descriptors.end(), [](const OcctObjectDescriptor& left, const OcctObjectDescriptor& right)
            {
                return left.objectId < right.objectId;
            });
            std::copy(descriptors.begin(), descriptors.end(), items);
        });
    }

    int occt_object_exists(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        return e != nullptr && e->findObject(id) != nullptr ? 1 : 0;
    }

    int occt_object_kind(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        const ObjectEntry* entry = e == nullptr ? nullptr : e->findObject(id);
        return entry == nullptr ? OcctObject_Unknown : entry->kind;
    }

    int occt_set_object_name(OcctHandle h, OcctObjectId id, const char* name)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(id);
            if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
            entry->name = name == nullptr ? "" : name;
            if (entry->kind == OcctObject_Shape && !syncStepObjectName(e, *entry))
                e->invalidatePristineStepDocument();
        });
    }

    const char* occt_get_object_name(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        if (e == nullptr) return "";
        const ObjectEntry* entry = e->findObject(id);
        e->errors.scratch = entry == nullptr ? "" : entry->name;
        return e->errors.scratch.c_str();
    }

    int occt_set_object_color(OcctHandle h, OcctObjectId id, double r, double g, double b)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(id);
            if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
            const Quantity_Color value = color(r, g, b);
            entry->hasStoredColor = true;
            entry->storedColorR = value.Red();
            entry->storedColorG = value.Green();
            entry->storedColorB = value.Blue();
            if (entry->kind == OcctObject_Shape && !syncStepObjectColor(e, *entry))
                e->invalidatePristineStepDocument();
            e->viewerContext.context->SetColor(entry->presentation, value, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_object_transparency(OcctHandle h, OcctObjectId id, double value)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(id);
            if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
            const double transparency = std::clamp(value, 0.0, 1.0);
            entry->storedColorA = 1.0 - transparency;
            entry->hasStoredAlpha = true;
            if (entry->kind == OcctObject_Shape && !syncStepObjectColor(e, *entry))
                e->invalidatePristineStepDocument();
            e->viewerContext.context->SetTransparency(entry->presentation, transparency, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_object_visible(OcctHandle h, OcctObjectId id, int visible)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(id);
            if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
            entry->storedVisible = visible != 0;
            if (entry->kind == OcctObject_Shape && !syncStepObjectVisibility(e, *entry))
                e->invalidatePristineStepDocument();
            if (visible)
                e->viewerContext.context->Display(entry->presentation, Standard_False);
            else
                e->viewerContext.context->Erase(entry->presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_object_display_mode(OcctHandle h, OcctObjectId id, int mode)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(id);
            if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
            e->viewerContext.context->SetDisplayMode(
                entry->presentation,
                mode == OcctDisplay_Wireframe ? AIS_WireFrame : AIS_Shaded,
                Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_object_line_width(OcctHandle h, OcctObjectId id, double width)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(width, "Line width");
            ObjectEntry* entry = e->findObject(id);
            if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
            e->viewerContext.context->SetWidth(entry->presentation, width, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_set_object_material(OcctHandle h, OcctObjectId id, int material)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(id);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (entry->kind == OcctObject_Shape) e->invalidatePristineStepDocument();
            e->viewerContext.context->SetMaterial(
                entry->presentation,
                Graphic3d_MaterialAspect(materialName(material)),
                Standard_False);
            e->requestRedraw();
        });
    }

    int occt_delete_objects(OcctHandle h, const OcctObjectId* ids, int count)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (count < 0) throw std::invalid_argument("Object count must not be negative.");
            if (count > 0 && ids == nullptr) throw std::invalid_argument("Object ID array is null.");

            std::vector<OcctObjectId> uniqueIds;
            std::unordered_set<OcctObjectId> seenIds;
            uniqueIds.reserve(static_cast<std::size_t>(count));
            seenIds.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
            {
                const OcctObjectId id = ids[index];
                if (e->findObject(id) == nullptr) throw std::invalid_argument("Object ID does not exist.");
                if (seenIds.insert(id).second) uniqueIds.push_back(id);
            }

            detachManipulatorsReferencing(e, seenIds);
            for (const OcctObjectId id : uniqueIds) e->erase(id);
            if (!uniqueIds.empty()) e->requestRedraw();
        });
    }

    int occt_clear(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            std::unordered_set<OcctObjectId> allIds;
            allIds.reserve(e->scene.objects.size());
            for (const auto& pair : e->scene.objects) allIds.insert(pair.first);
            detachManipulatorsReferencing(e, allIds);

            for (auto& pair : e->scene.objects)
            {
                if (!pair.second.presentation.IsNull())
                    e->viewerContext.context->Remove(pair.second.presentation, Standard_False);
            }
            e->scene.clear();
            e->documents.clear();
            e->viewerContext.context->ClearSelected(Standard_False);
            e->requestRedraw();
        });
    }
}
