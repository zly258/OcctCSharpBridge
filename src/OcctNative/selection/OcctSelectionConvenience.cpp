#include "selection/OcctSelection.h"
#include "core/OcctInternal.hxx"

#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeSelectionStatus(Engine* engine, Function&& function)
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
    OcctStatus occt_engine_selection_all_visible(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            engine->viewerContext.context->ClearSelected(Standard_False);
            for (const auto& pair : engine->scene.objects)
            {
                if (!pair.second.presentation.IsNull() &&
                    pair.second.selectable &&
                    engine->viewerContext.context->IsDisplayed(pair.second.presentation))
                {
                    engine->viewerContext.context->AddSelect(pair.second.presentation);
                }
            }
            engine->viewerContext.context->HilightSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_invert(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            for (const auto& pair : engine->scene.objects)
            {
                if (!pair.second.presentation.IsNull() &&
                    pair.second.selectable &&
                    engine->viewerContext.context->IsDisplayed(pair.second.presentation))
                {
                    engine->viewerContext.context->AddOrRemoveSelected(
                        pair.second.presentation,
                        Standard_False);
                }
            }
            engine->viewerContext.context->HilightSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_hide_selected(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            std::vector<Handle(AIS_InteractiveObject)> selected;
            for (engine->viewerContext.context->InitSelected();
                 engine->viewerContext.context->MoreSelected();
                 engine->viewerContext.context->NextSelected())
            {
                const Handle(AIS_InteractiveObject) value =
                    engine->viewerContext.context->SelectedInteractive();
                if (!value.IsNull()) selected.push_back(value);
            }
            for (const auto& value : selected)
                engine->viewerContext.context->Erase(value, Standard_False);
            engine->viewerContext.context->ClearSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_automatic_highlight_set(
        OcctEngineHandle handle,
        int enabled)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            engine->viewerContext.context->SetAutomaticHilight(enabled != 0);
        });
    }
}
