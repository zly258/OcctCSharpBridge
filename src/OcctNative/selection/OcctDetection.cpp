#include "selection/OcctDetection.h"
#include "core/OcctInternal.hxx"

#include <Graphic3d_Camera.hxx>
#include <Graphic3d_Vec2.hxx>
#include <SelectMgr_SortCriterion.hxx>
#include <StdSelect_BRepOwner.hxx>
#include <StdSelect_ViewerSelector3d.hxx>
#include <TopExp_Explorer.hxx>

#include <algorithm>
#include <stdexcept>
#include <unordered_set>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t DetectionOptionsApiVersion = 1;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeDetectionStatus(Engine* engine, Function&& function)
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

    OcctObjectId ownerObjectId(
        Engine* engine,
        const Handle(SelectMgr_EntityOwner)& owner)
    {
        if (owner.IsNull()) return 0;
        const Handle(AIS_InteractiveObject) interactive =
            Handle(AIS_InteractiveObject)::DownCast(owner->Selectable());
        return interactive.IsNull() ? 0 : engine->findPresentation(interactive);
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
        const OcctObjectId objectId = ownerObjectId(engine, owner);
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

    void validateOptions(const OcctViewerDetectionOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Detection options are null.");
        if (options->structSize < sizeof(OcctViewerDetectionOptions) ||
            options->apiVersion != DetectionOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported detection options size or version.");
        }
        if (options->maxHits <= 0 || options->maxHits > 1024)
            throw std::invalid_argument("Maximum hit count must be between 1 and 1024.");
        if (options->ownerCount < 0)
            throw std::invalid_argument("Owner count must not be negative.");
        if (options->ownerCount > 0 && options->ownerIds == nullptr)
            throw std::invalid_argument("Owner filter array is null.");
    }
}

extern "C"
{
    OcctStatus occt_engine_selection_detect_filtered(
        OcctEngineHandle handle,
        const OcctViewerDetectionOptions* options,
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
        return executeDetectionStatus(engine, [&]
        {
            validateOptions(options);
            if (capacity < options->maxHits)
                throw std::out_of_range("Detection output capacity is smaller than maxHits.");
            if (items == nullptr) throw std::invalid_argument("Detection output buffer is null.");

            std::unordered_set<OcctObjectId> owners;
            owners.reserve(static_cast<std::size_t>(options->ownerCount));
            for (int index = 0; index < options->ownerCount; ++index)
                owners.insert(options->ownerIds[index]);

            const Handle(StdSelect_ViewerSelector3d)& selector =
                engine->viewerContext.context->MainSelector();
            selector->Pick(options->x, options->y, engine->viewerContext.view);

            int filled = 0;
            for (int rank = 1; rank <= selector->NbPicked() && filled < options->maxHits; ++rank)
            {
                OcctSelectionHitDetail hit{};
                if (!createHit(
                    engine,
                    selector->Picked(rank),
                    selector->PickedPoint(rank),
                    selector->PickedData(rank).Depth,
                    hit))
                {
                    continue;
                }
                if (!passesFilter(
                    engine,
                    hit,
                    owners,
                    options->objectKindMask,
                    options->shapeTypeMask,
                    options->includeWholeObjects != 0))
                {
                    continue;
                }
                items[filled++] = hit;
            }
            *count = filled;
        });
    }

    OcctStatus occt_engine_selection_rectangle_query(
        OcctEngineHandle handle,
        int x1,
        int y1,
        int x2,
        int y2,
        int allowOverlap,
        OcctObjectId* objectIds,
        int capacity,
        int* count)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (count == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Rectangle-query count output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }

        return executeDetectionStatus(engine, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Rectangle-query capacity must not be negative.");
            if (capacity > 0 && objectIds == nullptr)
                throw std::invalid_argument("Rectangle-query output buffer is null.");

            const Handle(StdSelect_ViewerSelector3d)& selector =
                engine->viewerContext.context->MainSelector();
            const Graphic3d_Vec2i minPoint(std::min(x1, x2), std::min(y1, y2));
            const Graphic3d_Vec2i maxPoint(std::max(x1, x2), std::max(y1, y2));

            selector->AllowOverlapDetection(allowOverlap != 0);
            try
            {
                selector->Pick(minPoint, maxPoint, engine->viewerContext.view);
            }
            catch (...)
            {
                selector->AllowOverlapDetection(Standard_False);
                throw;
            }
            selector->AllowOverlapDetection(Standard_False);

            std::vector<OcctObjectId> results;
            std::unordered_set<OcctObjectId> seen;
            results.reserve(static_cast<std::size_t>(selector->NbPicked()));
            seen.reserve(static_cast<std::size_t>(selector->NbPicked()));
            for (int rank = 1; rank <= selector->NbPicked(); ++rank)
            {
                const OcctObjectId objectId = ownerObjectId(engine, selector->Picked(rank));
                if (objectId > 0 && seen.insert(objectId).second)
                    results.push_back(objectId);
            }

            *count = static_cast<int>(results.size());
            if (capacity == 0) return;
            if (capacity < *count)
                throw std::out_of_range("Rectangle-query output capacity is smaller than the result count.");
            std::copy(results.begin(), results.end(), objectIds);
        });
    }
}
