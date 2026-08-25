#include "viewer/OcctViewerUpdate.h"
#include "core/OcctInternal.hxx"

#include <utility>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeUpdateStatus(Engine* engine, Function&& function)
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
    OcctStatus occt_engine_update_begin(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeUpdateStatus(engine, [&] { engine->beginUpdate(); });
    }

    OcctStatus occt_engine_update_end(OcctEngineHandle handle, int fitAll)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeUpdateStatus(engine, [&] { engine->endUpdate(fitAll != 0); });
    }

    OcctStatus occt_engine_update_state_get(OcctEngineHandle handle, int* isUpdating)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        engine->clearError();
        if (isUpdating == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Update state output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }
        if (!engine->isInitialized())
        {
            engine->setError(OcctStatus_ErrorNotInitialized, "The OCCT viewer has not been initialized.");
            return OcctStatus_ErrorNotInitialized;
        }
        *isUpdating = engine->isUpdating() ? 1 : 0;
        return OcctStatus_Ok;
    }
}
