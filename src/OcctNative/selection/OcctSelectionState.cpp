#include "selection/OcctSelectionState.h"
#include "core/OcctInternal.hxx"

#include <Graphic3d_Camera.hxx>
#include <SelectMgr_SortCriterion.hxx>
#include <StdSelect_BRepOwner.hxx>
#include <StdSelect_ViewerSelector3d.hxx>
#include <TopExp_Explorer.hxx>

#include <algorithm>
#include <stdexcept>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeSelectionStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
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

    bool tryCreateSelectionHitDetail(
        Engine* engine,
        const Handle(SelectMgr_EntityOwner)& owner,
        const gp_Pnt& pickedPoint,
        double depth,
        OcctSelectionHitDetail& result)
    {
        OcctSelectionHit identity{};
        if (!tryCreateSelectionHit(engine, owner, identity)) return false;

        result = {};
        result.ownerObjectId = identity.ownerObjectId;
        result.subshapeType = identity.subshapeType;
        result.subshapeIndex = identity.subshapeIndex;
        result.point = {pickedPoint.X(), pickedPoint.Y(), pickedPoint.Z()};
        result.depth = depth;
        result.distanceToEye = engine->viewerContext.view->Camera()->Eye().Distance(pickedPoint);
        return true;
    }

    std::vector<OcctSelectionHit> collectSelectedHits(Engine* engine)
    {
        std::vector<OcctSelectionHit> hits;
        for (engine->viewerContext.context->InitSelected();
             engine->viewerContext.context->MoreSelected();
             engine->viewerContext.context->NextSelected())
        {
            OcctSelectionHit hit{};
            if (tryCreateSelectionHit(engine, engine->viewerContext.context->SelectedOwner(), hit))
                hits.push_back(hit);
        }
        return hits;
    }

    bool tryFindPickedDetail(
        Engine* engine,
        const Handle(SelectMgr_EntityOwner)& targetOwner,
        OcctSelectionHitDetail& result)
    {
        const Handle(StdSelect_ViewerSelector3d)& selector =
            engine->viewerContext.context->MainSelector();
        for (int rank = 1; rank <= selector->NbPicked(); ++rank)
        {
            const Handle(SelectMgr_EntityOwner) owner = selector->Picked(rank);
            if (owner != targetOwner) continue;
            const gp_Pnt pickedPoint = selector->PickedPoint(rank);
            const SelectMgr_SortCriterion& criterion = selector->PickedData(rank);
            return tryCreateSelectionHitDetail(engine, owner, pickedPoint, criterion.Depth, result);
        }
        return false;
    }
}

extern "C"
{
    OcctStatus occt_engine_selection_hits_get(
        OcctEngineHandle handle,
        OcctSelectionHit* items,
        int capacity,
        int* count)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (count == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Selection hit count output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeSelectionStatus(engine, [&]
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

    OcctStatus occt_engine_selection_detected_hit_get(
        OcctEngineHandle handle,
        OcctSelectionHit* result,
        int* hasHit)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr || hasHit == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Detected hit output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeSelectionStatus(engine, [&]
        {
            *result = {};
            result->subshapeType = OcctShape_Shape;
            result->subshapeIndex = -1;
            *hasHit = 0;

            if (!engine->viewerContext.context->HasDetected()) return;
            if (tryCreateSelectionHit(engine, engine->viewerContext.context->DetectedOwner(), *result))
                *hasHit = 1;
        });
    }

    OcctStatus occt_engine_selection_detected_hit_detail_get(
        OcctEngineHandle handle,
        OcctSelectionHitDetail* result,
        int* hasHit)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr || hasHit == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Detected hit detail output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeSelectionStatus(engine, [&]
        {
            *result = {};
            result->subshapeType = OcctShape_Shape;
            result->subshapeIndex = -1;
            *hasHit = 0;

            if (!engine->viewerContext.context->HasDetected()) return;
            const Handle(SelectMgr_EntityOwner) owner = engine->viewerContext.context->DetectedOwner();
            if (tryFindPickedDetail(engine, owner, *result)) *hasHit = 1;
        });
    }

    OcctStatus occt_engine_selection_detect_at(
        OcctEngineHandle handle,
        int x,
        int y,
        int maxHits,
        OcctSelectionHitDetail* items,
        int capacity,
        int* count)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (count == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Detection count output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }
        return executeSelectionStatus(engine, [&]
        {
            if (maxHits <= 0 || maxHits > 1024)
                throw std::invalid_argument("Maximum hit count must be between 1 and 1024.");
            if (capacity < maxHits)
                throw std::out_of_range("Detection output capacity is smaller than maxHits.");
            if (items == nullptr)
                throw std::invalid_argument("Detection output buffer is null.");

            const Handle(StdSelect_ViewerSelector3d)& selector =
                engine->viewerContext.context->MainSelector();
            selector->Pick(x, y, engine->viewerContext.view);

            int filled = 0;
            for (int rank = 1; rank <= selector->NbPicked() && filled < maxHits; ++rank)
            {
                const Handle(SelectMgr_EntityOwner) owner = selector->Picked(rank);
                const gp_Pnt pickedPoint = selector->PickedPoint(rank);
                const SelectMgr_SortCriterion& criterion = selector->PickedData(rank);
                OcctSelectionHitDetail hit{};
                if (!tryCreateSelectionHitDetail(engine, owner, pickedPoint, criterion.Depth, hit))
                    continue;
                items[filled++] = hit;
            }
            *count = filled;
        });
    }
}
