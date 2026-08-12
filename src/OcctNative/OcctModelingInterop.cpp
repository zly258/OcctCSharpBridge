#include "OcctModelingSessionInternal.hxx"
#include "OcctInternal.hxx"

using namespace OcctModelingInternal;

extern "C"
{
    OcctObjectId occt_model_display_in_engine(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId shapeId, int fit)
    {
        OcctBridge::Engine* engine = OcctBridge::engineOf(engineHandle);
        ModelSession* model = modelOf(modelHandle);
        if (engine == nullptr || model == nullptr) return 0;
        try
        {
            return engine->addShape(model->requireShape(shapeId), fit != 0, "ModelShape");
        }
        catch (const Standard_Failure& failure)
        {
            const char* message = failure.GetMessageString();
            model->lastError = message == nullptr ? "Displaying model shape failed." : message;
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
        }
        catch (...)
        {
            model->lastError = "Displaying model shape failed.";
        }
        return 0;
    }
}
