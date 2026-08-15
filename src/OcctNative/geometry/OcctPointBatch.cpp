#include "geometry/OcctPoints.h"
#include "core/OcctInternal.hxx"

#include <AIS_Point.hxx>
#include <Geom_CartesianPoint.hxx>

#include <stdexcept>
#include <vector>

using namespace OcctBridge;

namespace
{
    struct PointUpdateTarget
    {
        ObjectEntry* entry;
        Handle(AIS_Point) presentation;
        const OcctPointStateUpdate* update;
    };

    PointUpdateTarget pointUpdateTarget(Engine* engine, const OcctPointStateUpdate& update)
    {
        ObjectEntry* entry = engine->findObject(update.pointId);
        if (entry == nullptr || entry->kind != OcctPointObjectKind)
            throw std::invalid_argument("Point ID does not exist.");

        Handle(AIS_Point) presentation = Handle(AIS_Point)::DownCast(entry->presentation);
        if (presentation.IsNull())
            throw std::runtime_error("Point presentation type is invalid.");

        const gp_Pnt position = point(update.position);
        (void)position;
        return {entry, presentation, &update};
    }
}

extern "C"
{
    int occt_update_points(
        OcctHandle handle,
        const OcctPointStateUpdate* updates,
        int count)
    {
        Engine* engine = engineOf(handle);
        if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            if (count < 0) throw std::invalid_argument("Point update count must not be negative.");
            if (count > 0 && updates == nullptr)
                throw std::invalid_argument("Point update array is null.");

            std::vector<PointUpdateTarget> targets;
            targets.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
                targets.push_back(pointUpdateTarget(engine, updates[index]));

            for (PointUpdateTarget& target : targets)
            {
                Handle(Geom_CartesianPoint) component =
                    Handle(Geom_CartesianPoint)::DownCast(target.presentation->Component());
                if (component.IsNull())
                    target.presentation->SetComponent(new Geom_CartesianPoint(point(target.update->position)));
                else
                    component->SetPnt(point(target.update->position));

                engine->viewerContext.context->Redisplay(target.presentation, Standard_False);
                engine->viewerContext.context->RecomputeSelectionOnly(target.presentation);
                target.entry->storedVisible = target.update->visible != 0;
                target.entry->hasStoredVisibility = true;
                if (target.update->visible != 0)
                    engine->viewerContext.context->Display(target.presentation, Standard_False);
                else
                    engine->viewerContext.context->Erase(target.presentation, Standard_False);
            }
            if (!targets.empty()) engine->requestRedraw();
        });
    }
}
