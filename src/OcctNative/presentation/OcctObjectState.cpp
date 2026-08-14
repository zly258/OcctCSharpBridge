#include "presentation/OcctObjects.h"
#include "core/OcctInternal.hxx"

#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t ObjectStateApiVersion = 1;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeObjectStateStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }
}

extern "C"
{
    OcctStatus occt_engine_object_state_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctViewerObjectState* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        if (result == nullptr)
        {
            if (engine != nullptr)
                engine->setError(OcctStatus_ErrorInvalidArgument, "Object state output is null.");
            return engine == nullptr ? OcctStatus_ErrorInvalidHandle : OcctStatus_ErrorInvalidArgument;
        }

        return executeObjectStateStatus(engine, [&]
        {
            const ObjectEntry* entry = engine->findObject(objectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");

            result->structSize = static_cast<std::uint32_t>(sizeof(OcctViewerObjectState));
            result->apiVersion = ObjectStateApiVersion;
            result->kind = entry->kind;
            result->visible = engine->viewerContext.context->IsDisplayed(entry->presentation) ? 1 : 0;
            result->selectable = entry->selectable ? 1 : 0;
            result->selected = engine->viewerContext.context->IsSelected(entry->presentation) ? 1 : 0;
            result->highlighted = engine->viewerContext.context->IsHilighted(entry->presentation) ? 1 : 0;
        });
    }
}
