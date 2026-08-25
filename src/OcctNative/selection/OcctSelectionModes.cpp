#include "selection/OcctSelection.h"
#include "core/OcctInternal.hxx"

#include <AIS_SelectionModesConcurrency.hxx>
#include <AIS_Shape.hxx>
#include <TopAbs_ShapeEnum.hxx>

#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    int selectionModeValue(const ObjectEntry& entry, int mode)
    {
        if (mode < OcctSelection_Object || mode > OcctSelection_Solid)
            throw std::invalid_argument("Selection mode is out of range.");
        if (mode == OcctSelection_Object) return 0;

        const Handle(AIS_Shape) shape = Handle(AIS_Shape)::DownCast(entry.presentation);
        if (shape.IsNull())
            throw std::invalid_argument("Subshape selection modes require an AIS_Shape object.");

        TopAbs_ShapeEnum type = TopAbs_SHAPE;
        switch (mode)
        {
            case OcctSelection_Vertex: type = TopAbs_VERTEX; break;
            case OcctSelection_Edge: type = TopAbs_EDGE; break;
            case OcctSelection_Wire: type = TopAbs_WIRE; break;
            case OcctSelection_Face: type = TopAbs_FACE; break;
            case OcctSelection_Shell: type = TopAbs_SHELL; break;
            case OcctSelection_Solid: type = TopAbs_SOLID; break;
            default: break;
        }
        return AIS_Shape::SelectionMode(type);
    }

    AIS_SelectionModesConcurrency selectionConcurrency(int value)
    {
        switch (value)
        {
            case OcctSelectionConcurrency_Single: return AIS_SelectionModesConcurrency_Single;
            case OcctSelectionConcurrency_GlobalOrLocal: return AIS_SelectionModesConcurrency_GlobalOrLocal;
            case OcctSelectionConcurrency_Multiple: return AIS_SelectionModesConcurrency_Multiple;
            default: throw std::invalid_argument("Selection mode concurrency is out of range.");
        }
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeSelectionModeStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }
}

extern "C"
{
    OcctStatus occt_engine_selection_object_mode_set_active(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int mode,
        int active,
        int concurrency,
        int force)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionModeStatus(engine, [&]
        {
            ObjectEntry& entry = requiredObject(engine, objectId);
            if (active != 0 && !entry.selectable)
                throw std::invalid_argument("A non-selectable object cannot activate a selection mode.");

            engine->viewerContext.context->SetSelectionModeActive(
                entry.presentation,
                selectionModeValue(entry, mode),
                active != 0,
                selectionConcurrency(concurrency),
                force != 0);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_object_sensitivity_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int mode,
        int sensitivity)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionModeStatus(engine, [&]
        {
            if (sensitivity <= 0)
                throw std::invalid_argument("Selection sensitivity must be greater than zero.");
            ObjectEntry& entry = requiredObject(engine, objectId);
            engine->viewerContext.context->SetSelectionSensitivity(
                entry.presentation,
                selectionModeValue(entry, mode),
                sensitivity);
        });
    }
}
