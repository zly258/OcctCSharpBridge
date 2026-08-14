#include "modeling/OcctModelingSessionInternal.hxx"
#include "core/OcctInternal.hxx"

using namespace OcctModelingInternal;

extern "C"
{
    OcctObjectId occt_model_display_in_engine(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId shapeId, int fit)
    {
        OcctBridge::Engine* engine = OcctBridge::engineOf(engineHandle);
        ModelSession* model = modelOf(modelHandle);
        if (model == nullptr) return 0;
        if (engine == nullptr)
        {
            model->errors.set(OcctStatus_ErrorInvalidHandle, "Invalid OCCT engine handle.");
            return 0;
        }

        return executeValue(model, OcctObjectId{0}, [&]
        {
            return engine->addShape(model->requireShape(shapeId), fit != 0, "ModelShape");
        });
    }
}
