#include "OcctModelingInternal.hxx"

using namespace OcctBridge;
using namespace OcctModelingInternal;

extern "C"
{
    int occt_update_object_shape_from_model(
        OcctHandle engineHandle,
        OcctModelHandle modelHandle,
        OcctObjectId viewerObjectId,
        OcctObjectId modelShapeId,
        unsigned int options)
    {
        Engine* engine = engineOf(engineHandle); if (!validateInitialized(engine)) return 0;
        ModelSession* model = modelOf(modelHandle);
        if (model == nullptr)
        {
            engine->setError("The modeling session handle is invalid.");
            return 0;
        }

        return execute(engine, [&]
        {
            ObjectEntry* entry = engine->findShape(viewerObjectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Viewer shape ID does not exist.");
            const Handle(AIS_Shape) presentation = Handle(AIS_Shape)::DownCast(entry->presentation);
            if (presentation.IsNull()) throw std::invalid_argument("Viewer object is not an AIS_Shape.");

            const TopoDS_Shape newShape = model->requireShape(modelShapeId);
            const bool wasSelected = engine->context->IsSelected(entry->presentation);
            const bool wasSelectable = entry->selectable;
            const bool hadTransform = entry->presentation->HasTransformation();
            const gp_Trsf transform = entry->presentation->LocalTransformation();

            entry->shape = newShape;
            presentation->SetShape(newShape);

            if ((options & OcctShapeUpdate_PreserveTransformation) != 0 && hadTransform)
                entry->presentation->SetLocalTransformation(transform);
            else if ((options & OcctShapeUpdate_PreserveTransformation) == 0)
                entry->presentation->ResetTransformation();

            if ((options & OcctShapeUpdate_PreserveSelectability) == 0)
                entry->selectable = true;
            else
                entry->selectable = wasSelectable;

            if ((options & OcctShapeUpdate_RecomputePresentation) != 0)
                engine->context->Redisplay(entry->presentation, Standard_False, Standard_True);
            else
                entry->presentation->SetToUpdate();

            if ((options & OcctShapeUpdate_RecomputeSelection) != 0)
                engine->context->RecomputeSelectionOnly(entry->presentation);

            engine->applySelectionMode(entry->presentation);
            if ((options & OcctShapeUpdate_PreserveSelection) != 0 && wasSelected && entry->selectable)
                engine->context->SetSelected(entry->presentation, Standard_False);
            else if (engine->context->IsSelected(entry->presentation))
                engine->context->AddOrRemoveSelected(entry->presentation, Standard_False);

            engine->requestRedraw();
        });
    }
}
