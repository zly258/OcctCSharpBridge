#include "scene/OcctViewerModelInterop.h"
#include "core/OcctInternal.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <stdexcept>
#include <utility>

using namespace OcctBridge;
using namespace OcctModelingInternal;

namespace
{
    constexpr std::uint32_t AllShapeUpdateOptions =
        OcctShapeUpdate_PreserveAppearance |
        OcctShapeUpdate_PreserveTransformation |
        OcctShapeUpdate_PreserveSelection |
        OcctShapeUpdate_PreserveSelectability |
        OcctShapeUpdate_RecomputePresentation |
        OcctShapeUpdate_RecomputeSelection;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }
}

extern "C"
{
    OcctStatus occt_engine_object_shape_create_from_model(
        OcctEngineHandle engineHandle,
        OcctModelingSessionHandle modelHandle,
        OcctObjectId modelShapeId,
        OcctObjectId* viewerObjectId)
    {
        Engine* engine = reinterpret_cast<Engine*>(engineHandle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;

        ModelSession* model = sessionOf(modelHandle);
        if (model == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidHandle, "The modeling session handle is invalid.");
            return OcctStatus_ErrorInvalidHandle;
        }
        if (viewerObjectId == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "The viewer object ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }

        *viewerObjectId = 0;
        const int succeeded = execute(engine, [&]
        {
            TopoDS_Shape sourceShape;
            {
                const std::lock_guard<std::recursive_mutex> guard(model->mutex);
                sourceShape = model->requireShape(modelShapeId);
            }
            *viewerObjectId = engine->addShape(sourceShape);
        });
        return succeeded != 0 ? OcctStatus_Ok : engine->errors.code;
    }

    OcctStatus occt_engine_object_shape_update_from_model(
        OcctEngineHandle engineHandle,
        OcctModelingSessionHandle modelHandle,
        OcctObjectId viewerObjectId,
        OcctObjectId modelShapeId,
        std::uint32_t options)
    {
        Engine* engine = reinterpret_cast<Engine*>(engineHandle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;

        ModelSession* model = sessionOf(modelHandle);
        if (model == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidHandle, "The modeling session handle is invalid.");
            return OcctStatus_ErrorInvalidHandle;
        }
        if ((options & ~AllShapeUpdateOptions) != 0)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Shape update options contain unsupported flags.");
            return OcctStatus_ErrorInvalidArgument;
        }

        const int succeeded = execute(engine, [&]
        {
            ObjectEntry* entry = engine->findShape(viewerObjectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Viewer shape ID does not exist.");
            const Handle(AIS_Shape) presentation = Handle(AIS_Shape)::DownCast(entry->presentation);
            if (presentation.IsNull()) throw std::invalid_argument("Viewer object is not an AIS_Shape.");

            TopoDS_Shape newShape;
            {
                const std::lock_guard<std::recursive_mutex> guard(model->mutex);
                newShape = model->requireShape(modelShapeId);
            }
            const bool wasSelected = engine->viewerContext.context->IsSelected(entry->presentation);
            const bool wasSelectable = entry->selectable;
            const bool hadTransform = entry->presentation->HasTransformation();
            const gp_Trsf transform = entry->presentation->LocalTransformation();

            engine->invalidatePristineStepDocument();
            entry->shape = newShape;
            presentation->SetShape(newShape);

            if ((options & OcctShapeUpdate_PreserveTransformation) != 0 && hadTransform)
                entry->presentation->SetLocalTransformation(transform);
            else if ((options & OcctShapeUpdate_PreserveTransformation) == 0)
                entry->presentation->ResetTransformation();

            entry->selectable = (options & OcctShapeUpdate_PreserveSelectability) != 0
                ? wasSelectable
                : true;

            if ((options & OcctShapeUpdate_RecomputePresentation) != 0)
                engine->viewerContext.context->Redisplay(entry->presentation, Standard_False, Standard_True);
            else
                entry->presentation->SetToUpdate();

            if ((options & OcctShapeUpdate_RecomputeSelection) != 0)
                engine->viewerContext.context->RecomputeSelectionOnly(entry->presentation);

            engine->applySelectionMode(entry->presentation);
            if ((options & OcctShapeUpdate_PreserveSelection) != 0 && wasSelected && entry->selectable)
                engine->viewerContext.context->SetSelected(entry->presentation, Standard_False);
            else if (engine->viewerContext.context->IsSelected(entry->presentation))
                engine->viewerContext.context->AddOrRemoveSelected(entry->presentation, Standard_False);

            engine->requestRedraw();
        });
        return succeeded != 0 ? OcctStatus_Ok : engine->errors.code;
    }
}
