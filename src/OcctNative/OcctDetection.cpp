#include "OcctInternal.hxx"
#include "OcctDetection.h"

#include <Graphic3d_Camera.hxx>
#include <SelectMgr_SortCriterion.hxx>
#include <StdSelect_BRepOwner.hxx>
#include <StdSelect_ViewerSelector3d.hxx>
#include <TopExp_Explorer.hxx>

#include <unordered_set>

using namespace OcctBridge;

namespace
{
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

    bool createHit(
        Engine* engine,
        const Handle(SelectMgr_EntityOwner)& owner,
        const gp_Pnt& pickedPoint,
        double depth,
        OcctSelectionHitDetail& result)
    {
        if (owner.IsNull()) return false;
        const Handle(AIS_InteractiveObject) interactive =
            Handle(AIS_InteractiveObject)::DownCast(owner->Selectable());
        const OcctObjectId objectId = engine->findPresentation(interactive);
        if (objectId <= 0) return false;

        result = {};
        result.ownerObjectId = objectId;
        result.subshapeType = OcctShape_Shape;
        result.subshapeIndex = -1;
        result.point = {pickedPoint.X(), pickedPoint.Y(), pickedPoint.Z()};
        result.depth = depth;
        result.distanceToEye = engine->viewerContext.view->Camera()->Eye().Distance(pickedPoint);

        const ObjectEntry* entry = engine->findObject(objectId);
        if (entry == nullptr || entry->kind != OcctObject_Shape || entry->shape.IsNull()) return true;
        const TopoDS_Shape selected = ownerShape(owner);
        if (selected.IsNull() || selected.IsSame(entry->shape)) return true;

        const int subshapeIndex = findSubshapeIndex(entry->shape, selected);
        if (subshapeIndex == -2)
            throw std::runtime_error("Detected subshape could not be mapped to the owner shape topology.");
        result.subshapeType = shapeTypeValue(selected);
        result.subshapeIndex = subshapeIndex;
        return true;
    }

    bool passesFilter(
        Engine* engine,
        const OcctSelectionHitDetail& hit,
        const std::unordered_set<OcctObjectId>& owners,
        std::uint64_t objectKindMask,
        std::uint64_t shapeTypeMask,
        bool includeWholeObjects)
    {
        if (!owners.empty() && owners.find(hit.ownerObjectId) == owners.end()) return false;
        const ObjectEntry* entry = engine->findObject(hit.ownerObjectId);
        if (entry == nullptr || entry->kind < 0 || entry->kind >= 64) return false;
        if ((objectKindMask & (std::uint64_t{1} << entry->kind)) == 0) return false;

        if (hit.subshapeIndex < 0 && !includeWholeObjects) return false;
        if (hit.subshapeType < 0 || hit.subshapeType >= 64) return false;
        return (shapeTypeMask & (std::uint64_t{1} << hit.subshapeType)) != 0;
    }
}

extern "C"
{
    int occt_detect_at_filtered(
        OcctHandle h,
        int x,
        int y,
        int maxHits,
        const OcctObjectId* ownerIds,
        int ownerCount,
        std::uint64_t objectKindMask,
        std::uint64_t shapeTypeMask,
        int includeWholeObjects,
        OcctSelectionHitDetail* items,
        int capacity,
        int* count)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || count == nullptr) return 0;
        return execute(e, [&]
        {
            if (maxHits <= 0 || maxHits > 1024)
                throw std::invalid_argument("Maximum hit count must be between 1 and 1024.");
            if (capacity < maxHits)
                throw std::out_of_range("Detection output capacity is smaller than maxHits.");
            if (items == nullptr) throw std::invalid_argument("Detection output buffer is null.");
            if (ownerCount < 0) throw std::invalid_argument("Owner count must not be negative.");
            if (ownerCount > 0 && ownerIds == nullptr) throw std::invalid_argument("Owner filter array is null.");

            std::unordered_set<OcctObjectId> owners;
            owners.reserve(static_cast<std::size_t>(ownerCount));
            for (int index = 0; index < ownerCount; ++index) owners.insert(ownerIds[index]);

            const Handle(StdSelect_ViewerSelector3d)& selector = e->viewerContext.context->MainSelector();
            selector->Pick(x, y, e->viewerContext.view);

            int filled = 0;
            for (int rank = 1; rank <= selector->NbPicked() && filled < maxHits; ++rank)
            {
                OcctSelectionHitDetail hit{};
                if (!createHit(
                    e,
                    selector->Picked(rank),
                    selector->PickedPoint(rank),
                    selector->PickedData(rank).Depth,
                    hit))
                {
                    continue;
                }
                if (!passesFilter(
                    e,
                    hit,
                    owners,
                    objectKindMask,
                    shapeTypeMask,
                    includeWholeObjects != 0))
                {
                    continue;
                }
                items[filled++] = hit;
            }
            *count = filled;
        });
    }
}
