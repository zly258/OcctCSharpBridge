#include "OcctInternal.hxx"
#include "OcctSelectionState.h"

#include <StdSelect_BRepOwner.hxx>
#include <TopExp_Explorer.hxx>

#include <unordered_set>

using namespace OcctBridge;

namespace
{
    std::vector<ObjectEntry*> requireSelectableObjects(
        Engine* engine,
        const OcctObjectId* objectIds,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");

        std::vector<ObjectEntry*> result;
        std::unordered_set<OcctObjectId> uniqueIds;
        result.reserve(static_cast<std::size_t>(count));
        uniqueIds.reserve(static_cast<std::size_t>(count));
        for (int index = 0; index < count; ++index)
        {
            const OcctObjectId id = objectIds[index];
            if (!uniqueIds.insert(id).second) continue;
            ObjectEntry* entry = engine->findObject(id);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (!entry->selectable)
                throw std::invalid_argument("A non-selectable object cannot be added to the selection.");
            result.push_back(entry);
        }
        return result;
    }

    TopoDS_Shape ownerShape(const Handle(SelectMgr_EntityOwner)& owner)
    {
        const Handle(StdSelect_BRepOwner) brepOwner = Handle(StdSelect_BRepOwner)::DownCast(owner);
        return brepOwner.IsNull() ? TopoDS_Shape() : brepOwner->Shape();
    }

    int findSubshapeIndex(const TopoDS_Shape& root, const TopoDS_Shape& selected)
    {
        if (root.IsNull() || selected.IsNull() || root.IsSame(selected)) return -1;

        int index = 0;
        for (TopExp_Explorer explorer(root, selected.ShapeType()); explorer.More(); explorer.Next(), ++index)
        {
            if (explorer.Current().IsSame(selected)) return index;
        }
        return -2;
    }

    bool tryCreateSelectionHit(
        Engine* engine,
        const Handle(SelectMgr_EntityOwner)& owner,
        OcctSelectionHit& result)
    {
        result = {};
        result.subshapeType = OcctShape_Shape;
        result.subshapeIndex = -1;
        if (owner.IsNull()) return false;

        const Handle(AIS_InteractiveObject) interactive =
            Handle(AIS_InteractiveObject)::DownCast(owner->Selectable());
        const OcctObjectId objectId = engine->findPresentation(interactive);
        if (objectId <= 0) return false;

        result.ownerObjectId = objectId;
        const ObjectEntry* entry = engine->findObject(objectId);
        if (entry == nullptr || entry->kind != OcctObject_Shape || entry->shape.IsNull()) return true;

        const TopoDS_Shape selected = ownerShape(owner);
        if (selected.IsNull() || selected.IsSame(entry->shape)) return true;

        const int subshapeIndex = findSubshapeIndex(entry->shape, selected);
        if (subshapeIndex == -2)
            throw std::runtime_error("Selected subshape could not be mapped to the owner shape topology.");

        result.subshapeType = shapeTypeValue(selected);
        result.subshapeIndex = subshapeIndex;
        return true;
    }

    std::vector<OcctSelectionHit> collectSelectedHits(Engine* engine)
    {
        std::vector<OcctSelectionHit> hits;
        for (engine->context->InitSelected(); engine->context->MoreSelected(); engine->context->NextSelected())
        {
            OcctSelectionHit hit{};
            if (tryCreateSelectionHit(engine, engine->context->SelectedOwner(), hit))
                hits.push_back(hit);
        }
        return hits;
    }
}

extern "C"
{
    int occt_set_selected_objects_ex(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int operation)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            if (operation < OcctSelection_Replace || operation > OcctSelection_Clear)
                throw std::invalid_argument("Selection operation is out of range.");

            if (operation == OcctSelection_Clear)
            {
                engine->context->ClearSelected(Standard_False);
                engine->context->UpdateCurrentViewer();
                return;
            }

            const auto entries = requireSelectableObjects(engine, objectIds, count);
            if (operation == OcctSelection_Replace)
                engine->context->ClearSelected(Standard_False);

            for (ObjectEntry* entry : entries)
            {
                const bool isSelected = engine->context->IsSelected(entry->presentation);
                switch (operation)
                {
                    case OcctSelection_Replace:
                    case OcctSelection_Add:
                        if (!isSelected) engine->context->SetSelected(entry->presentation, Standard_False);
                        break;
                    case OcctSelection_Remove:
                        if (isSelected) engine->context->AddOrRemoveSelected(entry->presentation, Standard_False);
                        break;
                    case OcctSelection_Toggle:
                        engine->context->AddOrRemoveSelected(entry->presentation, Standard_False);
                        break;
                    default:
                        break;
                }
            }
            engine->context->HilightSelected(Standard_False);
            engine->context->UpdateCurrentViewer();
        });
    }

    int occt_selected_hits(
        OcctHandle handle,
        OcctSelectionHit* items,
        int capacity,
        int* count)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine) || count == nullptr) return 0;
        return execute(engine, [&]
        {
            if (capacity < 0)
                throw std::invalid_argument("Selection hit capacity must not be negative.");

            const auto hits = collectSelectedHits(engine);
            *count = static_cast<int>(hits.size());

            if (items == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Selection hit output is null but capacity is non-zero.");
                return;
            }
            if (capacity < *count)
                throw std::out_of_range("Selection hit output capacity is too small.");

            std::copy(hits.begin(), hits.end(), items);
        });
    }

    int occt_detected_hit(
        OcctHandle handle,
        OcctSelectionHit* result,
        int* hasHit)
    {
        Engine* engine = engineOf(handle);
        if (!validateInitialized(engine) || result == nullptr || hasHit == nullptr) return 0;
        return execute(engine, [&]
        {
            *result = {};
            result->subshapeType = OcctShape_Shape;
            result->subshapeIndex = -1;
            *hasHit = 0;

            if (!engine->context->HasDetected()) return;
            if (tryCreateSelectionHit(engine, engine->context->DetectedOwner(), *result))
                *hasHit = 1;
        });
    }
}
