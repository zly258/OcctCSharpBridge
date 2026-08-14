#include "selection/OcctSelection.h"
#include "core/OcctInternal.hxx"

#include <AIS_SelectionScheme.hxx>
#include <Graphic3d_Vec2.hxx>
#include <StdSelect_ViewerSelector3d.hxx>

#include <algorithm>
#include <stdexcept>
#include <unordered_set>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t SelectionOptionsApiVersion = 1;
    constexpr std::uint32_t AllSelectionSettingsBits =
        OcctViewerSelectionSettingsUpdate_Mode |
        OcctViewerSelectionSettingsUpdate_Tolerance;

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

    void validateSelectionMode(int mode)
    {
        if (mode < 0 || mode > 6)
            throw std::invalid_argument("Selection mode is out of range.");
    }

    void validateSettings(const OcctViewerSelectionSettingsOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Selection settings are null.");
        if (options->structSize < sizeof(OcctViewerSelectionSettingsOptions) ||
            options->apiVersion != SelectionOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported selection settings size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllSelectionSettingsBits) != 0)
            throw std::invalid_argument("Selection settings update mask is invalid.");
        if ((options->updateMask & OcctViewerSelectionSettingsUpdate_Mode) != 0)
            validateSelectionMode(options->selectionMode);
        if ((options->updateMask & OcctViewerSelectionSettingsUpdate_Tolerance) != 0 &&
            (options->pixelTolerance < 0 || options->pixelTolerance > 100))
        {
            throw std::invalid_argument("Selection tolerance must be between 0 and 100 pixels.");
        }
    }

    void validateRectangleOptions(const OcctViewerRectangleSelectionOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Rectangle selection options are null.");
        if (options->structSize < sizeof(OcctViewerRectangleSelectionOptions) ||
            options->apiVersion != SelectionOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported rectangle selection options size or version.");
        }
    }

    std::vector<ObjectEntry*> requireSelectableObjects(
        Engine* engine,
        const OcctObjectId* objectIds,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");

        std::vector<ObjectEntry*> result;
        std::unordered_set<OcctObjectId> uniqueIds;
        result.reserve(static_cast<std::size_t>(count));
        uniqueIds.reserve(static_cast<std::size_t>(count));
        for (int index = 0; index < count; ++index)
        {
            const OcctObjectId id = objectIds[index];
            if (!uniqueIds.insert(id).second) continue;
            ObjectEntry* entry = engine->findObject(id);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (!entry->selectable)
                throw std::invalid_argument("A non-selectable object cannot be added to the selection.");
            result.push_back(entry);
        }
        return result;
    }

    void validateObjectSelectionOptions(const OcctViewerObjectSelectionOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Object selection options are null.");
        if (options->structSize < sizeof(OcctViewerObjectSelectionOptions) ||
            options->apiVersion != SelectionOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported object selection options size or version.");
        }
        if (options->operation < OcctViewerSelection_Replace ||
            options->operation > OcctViewerSelection_Clear)
        {
            throw std::invalid_argument("Selection operation is out of range.");
        }
        if (options->count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (options->operation != OcctViewerSelection_Clear &&
            options->count > 0 && options->objectIds == nullptr)
        {
            throw std::invalid_argument("Object ID array is null.");
        }
    }
}

extern "C"
{
    OcctStatus occt_engine_selection_settings_update(
        OcctEngineHandle handle,
        const OcctViewerSelectionSettingsOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            validateSettings(options);
            if ((options->updateMask & OcctViewerSelectionSettingsUpdate_Mode) != 0)
            {
                engine->viewerContext.selectionMode = options->selectionMode;
                for (auto& pair : engine->scene.objects)
                    engine->applySelectionMode(pair.second.presentation);
            }
            if ((options->updateMask & OcctViewerSelectionSettingsUpdate_Tolerance) != 0)
                engine->viewerContext.context->SetPixelTolerance(options->pixelTolerance);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_move_to(OcctEngineHandle handle, int x, int y)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            engine->viewerContext.context->MoveTo(x, y, engine->viewerContext.view, Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_point_select(
        OcctEngineHandle handle,
        int x,
        int y,
        int append)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            engine->viewerContext.context->MoveTo(x, y, engine->viewerContext.view, Standard_False);
            if (engine->viewerContext.context->HasDetected())
            {
                engine->viewerContext.context->SelectDetected(
                    append != 0 ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            }
            else if (append == 0)
            {
                engine->viewerContext.context->ClearSelected(Standard_False);
            }
            engine->viewerContext.context->HilightSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_rectangle_select(
        OcctEngineHandle handle,
        const OcctViewerRectangleSelectionOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            validateRectangleOptions(options);
            const Handle(StdSelect_ViewerSelector3d)& selector =
                engine->viewerContext.context->MainSelector();
            selector->AllowOverlapDetection(options->allowOverlap != 0);
            const Graphic3d_Vec2i minPoint(
                std::min(options->x1, options->x2),
                std::min(options->y1, options->y2));
            const Graphic3d_Vec2i maxPoint(
                std::max(options->x1, options->x2),
                std::max(options->y1, options->y2));
            engine->viewerContext.context->SelectRectangle(
                minPoint,
                maxPoint,
                engine->viewerContext.view,
                options->append != 0 ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            selector->AllowOverlapDetection(Standard_False);
            engine->viewerContext.context->HilightSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_object_select(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int append)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            ObjectEntry* entry = engine->findObject(objectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (!entry->selectable) throw std::invalid_argument("Object is not selectable.");
            if (append == 0) engine->viewerContext.context->ClearSelected(Standard_False);
            engine->viewerContext.context->SetSelected(entry->presentation, Standard_False);
            engine->viewerContext.context->HilightSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_objects_update(
        OcctEngineHandle handle,
        const OcctViewerObjectSelectionOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            validateObjectSelectionOptions(options);
            if (options->operation == OcctViewerSelection_Clear)
            {
                engine->viewerContext.context->ClearSelected(Standard_False);
                engine->requestRedraw();
                return;
            }

            const auto entries = requireSelectableObjects(engine, options->objectIds, options->count);
            if (options->operation == OcctViewerSelection_Replace)
                engine->viewerContext.context->ClearSelected(Standard_False);

            for (ObjectEntry* entry : entries)
            {
                const bool isSelected = engine->viewerContext.context->IsSelected(entry->presentation);
                switch (options->operation)
                {
                    case OcctViewerSelection_Replace:
                    case OcctViewerSelection_Add:
                        if (!isSelected)
                            engine->viewerContext.context->SetSelected(entry->presentation, Standard_False);
                        break;
                    case OcctViewerSelection_Remove:
                        if (isSelected)
                            engine->viewerContext.context->AddOrRemoveSelected(entry->presentation, Standard_False);
                        break;
                    case OcctViewerSelection_Toggle:
                        engine->viewerContext.context->AddOrRemoveSelected(entry->presentation, Standard_False);
                        break;
                    default:
                        break;
                }
            }
            engine->viewerContext.context->HilightSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_clear(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeSelectionStatus(engine, [&]
        {
            engine->viewerContext.context->ClearSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_subshape_copy(
        OcctEngineHandle handle,
        int index,
        OcctObjectId* resultShapeId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        if (resultShapeId == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Result shape ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }
        if (index < 0)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Selected subshape index must not be negative.");
            return OcctStatus_ErrorInvalidArgument;
        }

        *resultShapeId = 0;
        const OcctObjectId result = executeObject(engine, [&]() -> OcctObjectId
        {
            int current = 0;
            for (engine->viewerContext.context->InitSelected();
                 engine->viewerContext.context->MoreSelected();
                 engine->viewerContext.context->NextSelected(), ++current)
            {
                if (current != index) continue;
                if (!engine->viewerContext.context->HasSelectedShape())
                    throw std::runtime_error("The selected item has no topological shape.");
                const TopoDS_Shape selected = engine->viewerContext.context->SelectedShape();
                if (selected.IsNull())
                    throw std::runtime_error("The selected topological subshape is null.");
                return engine->addShape(selected, false, "SelectedSubshape");
            }
            throw std::out_of_range("Selected subshape index is out of range.");
        });
        if (result == 0) return engine->errors.code;
        *resultShapeId = result;
        return OcctStatus_Ok;
    }
}
