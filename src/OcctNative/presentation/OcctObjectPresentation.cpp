#include "presentation/OcctPresentation.h"
#include "presentation/OcctViewerDecorations.h"
#include "core/OcctInternal.hxx"

#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Graphic3d_DisplayPriority.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_Vec2.hxx>

#include <stdexcept>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t TransformPersistenceApiVersion = 1;

    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    Graphic3d_DisplayPriority displayPriority(int value)
    {
        if (value < 0 || value > 10)
            throw std::invalid_argument("Display priority must be between 0 and 10.");
        return static_cast<Graphic3d_DisplayPriority>(value);
    }

    Aspect_TypeOfTriedronPosition cornerPosition(int value)
    {
        switch (value)
        {
            case OcctCorner_LeftLower: return Aspect_TOTP_LEFT_LOWER;
            case OcctCorner_LeftUpper: return Aspect_TOTP_LEFT_UPPER;
            case OcctCorner_RightLower: return Aspect_TOTP_RIGHT_LOWER;
            case OcctCorner_RightUpper: return Aspect_TOTP_RIGHT_UPPER;
            default: throw std::invalid_argument("Corner position is out of range.");
        }
    }

    int cornerValue(Aspect_TypeOfTriedronPosition value)
    {
        switch (value)
        {
            case Aspect_TOTP_LEFT_LOWER: return OcctCorner_LeftLower;
            case Aspect_TOTP_LEFT_UPPER: return OcctCorner_LeftUpper;
            case Aspect_TOTP_RIGHT_LOWER: return OcctCorner_RightLower;
            case Aspect_TOTP_RIGHT_UPPER: return OcctCorner_RightUpper;
            default: throw std::runtime_error("Transform persistence uses an unsupported corner position.");
        }
    }

    Graphic3d_TransModeFlags persistenceMode3d(int value)
    {
        switch (value)
        {
            case OcctTransformPersistence_Zoom: return Graphic3d_TMF_ZoomPers;
            case OcctTransformPersistence_Rotate: return Graphic3d_TMF_RotatePers;
            case OcctTransformPersistence_ZoomRotate: return Graphic3d_TMF_ZoomRotatePers;
            default: throw std::invalid_argument("Transform persistence mode is not a 3D anchor mode.");
        }
    }

    Graphic3d_TransModeFlags persistenceMode2d(int value)
    {
        switch (value)
        {
            case OcctTransformPersistence_Screen2d: return Graphic3d_TMF_2d;
            case OcctTransformPersistence_Triedron: return Graphic3d_TMF_TriedronPers;
            default: throw std::invalid_argument("Transform persistence mode is not a screen anchor mode.");
        }
    }

    int persistenceModeValue(Graphic3d_TransModeFlags value)
    {
        if (value == Graphic3d_TMF_ZoomPers) return OcctTransformPersistence_Zoom;
        if (value == Graphic3d_TMF_RotatePers) return OcctTransformPersistence_Rotate;
        if (value == Graphic3d_TMF_ZoomRotatePers) return OcctTransformPersistence_ZoomRotate;
        if (value == Graphic3d_TMF_2d) return OcctTransformPersistence_Screen2d;
        if (value == Graphic3d_TMF_TriedronPers) return OcctTransformPersistence_Triedron;
        throw std::runtime_error("Object uses a transform persistence mode outside the managed set.");
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeObjectPresentationStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    void validateTransformPersistenceOptions(const OcctViewerTransformPersistenceOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Transform persistence options are null.");
        if (options->structSize < sizeof(OcctViewerTransformPersistenceOptions) ||
            options->apiVersion != TransformPersistenceApiVersion)
        {
            throw std::invalid_argument("Unsupported transform persistence options size or version.");
        }
        if (options->mode == OcctTransformPersistence_None)
            throw std::invalid_argument("Use the clear operation to remove transform persistence.");
        if (options->mode == OcctTransformPersistence_Screen2d ||
            options->mode == OcctTransformPersistence_Triedron)
        {
            (void)cornerPosition(options->position);
            if (options->offsetX < 0 || options->offsetY < 0)
                throw std::invalid_argument("Transform persistence offsets must not be negative.");
        }
        else
        {
            (void)persistenceMode3d(options->mode);
            (void)point(options->anchor);
        }
    }
}

extern "C"
{
    OcctStatus occt_engine_object_display_priority_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int priority)
    {
        return occt_engine_objects_display_priority_set(handle, &objectId, 1, priority);
    }

    OcctStatus occt_engine_objects_display_priority_set(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int priority)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectPresentationStatus(engine, [&]
        {
            if (count < 0) throw std::invalid_argument("Object count must not be negative.");
            if (count > 0 && objectIds == nullptr)
                throw std::invalid_argument("Object ID array is null.");

            const Graphic3d_DisplayPriority nativePriority = displayPriority(priority);
            std::vector<ObjectEntry*> entries;
            entries.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
                entries.push_back(&requiredObject(engine, objectIds[index]));

            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetDisplayPriority(entry->presentation, nativePriority);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_display_priority_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int* priority)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectPresentationStatus(engine, [&]
        {
            if (priority == nullptr) throw std::invalid_argument("Display priority result is null.");
            ObjectEntry& entry = requiredObject(engine, objectId);
            *priority = static_cast<int>(engine->viewerContext.context->DisplayPriority(entry.presentation));
        });
    }

    OcctStatus occt_engine_object_transform_persistence_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerTransformPersistenceOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectPresentationStatus(engine, [&]
        {
            validateTransformPersistenceOptions(options);
            ObjectEntry& entry = requiredObject(engine, objectId);

            Handle(Graphic3d_TransformPers) persistence;
            if (options->mode == OcctTransformPersistence_Screen2d ||
                options->mode == OcctTransformPersistence_Triedron)
            {
                persistence = new Graphic3d_TransformPers(
                    persistenceMode2d(options->mode),
                    cornerPosition(options->position),
                    Graphic3d_Vec2i(options->offsetX, options->offsetY));
            }
            else
            {
                persistence = new Graphic3d_TransformPers(
                    persistenceMode3d(options->mode),
                    point(options->anchor));
            }

            engine->viewerContext.context->SetTransformPersistence(entry.presentation, persistence);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_transform_persistence_clear(
        OcctEngineHandle handle,
        OcctObjectId objectId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectPresentationStatus(engine, [&]
        {
            ObjectEntry& entry = requiredObject(engine, objectId);
            Handle(Graphic3d_TransformPers) persistence;
            engine->viewerContext.context->SetTransformPersistence(entry.presentation, persistence);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_transform_persistence_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctTransformPersistenceState* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectPresentationStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Transform persistence result is null.");
            ObjectEntry& entry = requiredObject(engine, objectId);
            *result = {};
            result->mode = OcctTransformPersistence_None;
            result->position = OcctCorner_LeftLower;

            const Handle(Graphic3d_TransformPers)& persistence = entry.presentation->TransformPersistence();
            if (persistence.IsNull()) return;

            result->mode = persistenceModeValue(persistence->Mode());
            if (persistence->IsZoomOrRotate())
            {
                const gp_Pnt anchor = persistence->AnchorPoint();
                result->anchor = {anchor.X(), anchor.Y(), anchor.Z()};
            }
            else if (persistence->IsTrihedronOr2d())
            {
                result->position = cornerValue(persistence->Corner2d());
                const Graphic3d_Vec2i offset = persistence->Offset2d();
                result->offsetX = offset.x();
                result->offsetY = offset.y();
            }
        });
    }
}
