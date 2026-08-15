#include "selection/OcctManipulator.h"
#include "core/OcctInternal.hxx"

#include <AIS_Manipulator.hxx>
#include <AIS_ManipulatorMode.hxx>
#include <NCollection_HSequence.hxx>

#include <cmath>
#include <stdexcept>
#include <unordered_set>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t ManipulatorApiVersion = 1;
    constexpr int AllManipulatorModesMask =
        (1 << OcctManipulator_Translation) |
        (1 << OcctManipulator_Rotation) |
        (1 << OcctManipulator_Scaling) |
        (1 << OcctManipulator_TranslationPlane);
    constexpr std::uint32_t AllManipulatorUpdateBits =
        OcctManipulatorUpdate_Part |
        OcctManipulatorUpdate_ModeEnabled |
        OcctManipulatorUpdate_ModeActivationOnDetection |
        OcctManipulatorUpdate_Position |
        OcctManipulatorUpdate_Size |
        OcctManipulatorUpdate_Gap |
        OcctManipulatorUpdate_ZoomPersistence |
        OcctManipulatorUpdate_Skin;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeManipulatorStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    AIS_ManipulatorMode manipulatorMode(int value)
    {
        switch (value)
        {
            case OcctManipulator_Translation: return AIS_MM_Translation;
            case OcctManipulator_Rotation: return AIS_MM_Rotation;
            case OcctManipulator_Scaling: return AIS_MM_Scaling;
            case OcctManipulator_TranslationPlane: return AIS_MM_TranslationPlane;
            default: throw std::invalid_argument("Manipulator mode is out of range.");
        }
    }

    AIS_Manipulator::ManipulatorSkin manipulatorSkin(int value)
    {
        switch (value)
        {
            case OcctManipulatorSkin_Shaded: return AIS_Manipulator::ManipulatorSkin_Shaded;
            case OcctManipulatorSkin_Flat: return AIS_Manipulator::ManipulatorSkin_Flat;
            default: throw std::invalid_argument("Manipulator skin mode is out of range.");
        }
    }

    ObjectEntry& requiredManipulatorEntry(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr || entry->kind != OcctManipulatorObjectKind || entry->presentation.IsNull())
            throw std::invalid_argument("Manipulator ID does not exist.");
        return *entry;
    }

    Handle(AIS_Manipulator) requiredManipulator(Engine* engine, OcctObjectId id)
    {
        ObjectEntry& entry = requiredManipulatorEntry(engine, id);
        Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
        if (manipulator.IsNull())
            throw std::runtime_error("Manipulator presentation has an invalid native type.");
        return manipulator;
    }

    void redisplayManipulator(Engine* engine, const Handle(AIS_Manipulator)& manipulator)
    {
        if (!manipulator.IsNull() && engine->viewerContext.context->IsDisplayed(manipulator))
            engine->viewerContext.context->Redisplay(manipulator, Standard_False, Standard_True);
        engine->requestRedraw();
    }

    void cancelActiveTransformation(const Handle(AIS_Manipulator)& manipulator)
    {
        if (manipulator->HasActiveTransformation()) manipulator->StopTransform(Standard_False);
        if (manipulator->HasActiveMode()) manipulator->DeactivateCurrentMode();
    }

    void validateAttachOptions(const OcctManipulatorAttachOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Manipulator attach options are null.");
        if (options->structSize < sizeof(OcctManipulatorAttachOptions) ||
            options->apiVersion != ManipulatorApiVersion)
        {
            throw std::invalid_argument("Unsupported manipulator attach options size or version.");
        }
    }

    bool finitePoint(OcctPoint3d value)
    {
        return std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z);
    }

    bool finiteVector(OcctVector3d value)
    {
        return std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z);
    }

    void validateUpdateOptions(const OcctManipulatorUpdateOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Manipulator update options are null.");
        if (options->structSize < sizeof(OcctManipulatorUpdateOptions) ||
            options->apiVersion != ManipulatorApiVersion)
        {
            throw std::invalid_argument("Unsupported manipulator update options size or version.");
        }
        if (options->updateMask == 0 || (options->updateMask & ~AllManipulatorUpdateBits) != 0)
            throw std::invalid_argument("Manipulator update mask is invalid.");

        if ((options->updateMask & (OcctManipulatorUpdate_Part | OcctManipulatorUpdate_ModeEnabled)) != 0)
            (void)manipulatorMode(options->mode);
        if ((options->updateMask & OcctManipulatorUpdate_Part) != 0 &&
            (options->axisIndex < -1 || options->axisIndex > 2))
        {
            throw std::invalid_argument("Manipulator axis index must be -1 or between 0 and 2.");
        }
        if ((options->updateMask & OcctManipulatorUpdate_Position) != 0 &&
            (!finitePoint(options->origin) || !finiteVector(options->normal) || !finiteVector(options->xDirection)))
        {
            throw std::invalid_argument("Manipulator position contains non-finite values.");
        }
        if ((options->updateMask & OcctManipulatorUpdate_Size) != 0 &&
            (!std::isfinite(options->size) || options->size <= 0.0))
        {
            throw std::invalid_argument("Manipulator size must be finite and greater than zero.");
        }
        if ((options->updateMask & OcctManipulatorUpdate_Gap) != 0 &&
            (!std::isfinite(options->gap) || options->gap < 0.0))
        {
            throw std::invalid_argument("Manipulator gap must be finite and non-negative.");
        }
        if ((options->updateMask & OcctManipulatorUpdate_Skin) != 0)
            (void)manipulatorSkin(options->skinMode);
    }

    void validateTransformOptions(const OcctManipulatorTransformOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Manipulator transform options are null.");
        if (options->structSize < sizeof(OcctManipulatorTransformOptions) ||
            options->apiVersion != ManipulatorApiVersion)
        {
            throw std::invalid_argument("Unsupported manipulator transform options size or version.");
        }
        if (options->action < OcctManipulatorTransform_Start ||
            options->action > OcctManipulatorTransform_DeactivateMode)
        {
            throw std::invalid_argument("Manipulator transform action is out of range.");
        }
    }
}

extern "C"
{
    OcctStatus occt_engine_manipulator_create(
        OcctEngineHandle handle,
        OcctObjectId* manipulatorId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        if (manipulatorId == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Manipulator ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }

        *manipulatorId = 0;
        const OcctObjectId result = executeObject(engine, [&]() -> OcctObjectId
        {
            Handle(AIS_Manipulator) manipulator = new AIS_Manipulator();
            const OcctObjectId id = engine->scene.allocateId();
            ObjectEntry entry{OcctManipulatorObjectKind, TopoDS_Shape(), manipulator, "Manipulator"};
            entry.selectable = true;
            entry.presentationSubtype = 0;
            engine->scene.objects.emplace(id, std::move(entry));
            return id;
        });
        if (result == 0) return engine->errors.code;
        *manipulatorId = result;
        return OcctStatus_Ok;
    }

    OcctStatus occt_engine_manipulator_attach(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        const OcctObjectId* objectIds,
        int count,
        const OcctManipulatorAttachOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeManipulatorStatus(engine, [&]
        {
            validateAttachOptions(options);
            if (objectIds == nullptr) throw std::invalid_argument("Manipulator target array is null.");
            requireCount(count, 1, "Manipulator target array");

            ObjectEntry& manipulatorEntry = requiredManipulatorEntry(engine, manipulatorId);
            Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(manipulatorEntry.presentation);
            if (manipulator.IsNull())
                throw std::runtime_error("Manipulator presentation has an invalid native type.");

            if (manipulator->IsAttached())
            {
                cancelActiveTransformation(manipulator);
                manipulator->Detach();
            }

            Handle(AIS_ManipulatorObjectSequence) targets = new AIS_ManipulatorObjectSequence();
            std::unordered_set<OcctObjectId> uniqueIds;
            uniqueIds.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
            {
                const OcctObjectId id = objectIds[index];
                if (!uniqueIds.insert(id).second) continue;
                if (id == manipulatorId) throw std::invalid_argument("Manipulator cannot be attached to itself.");
                ObjectEntry* target = engine->findObject(id);
                if (target == nullptr || target->presentation.IsNull())
                    throw std::invalid_argument("Manipulator target object does not exist.");
                if (target->kind == OcctManipulatorObjectKind)
                    throw std::invalid_argument("Manipulator cannot be attached to another manipulator.");
                targets->Append(target->presentation);
            }
            if (targets->IsEmpty()) throw std::invalid_argument("Manipulator target array is empty.");

            AIS_Manipulator::OptionsForAttach nativeOptions;
            nativeOptions.SetAdjustPosition(options->adjustPosition != 0)
                .SetAdjustSize(options->adjustSize != 0)
                .SetEnableModes(options->enableModes != 0);
            manipulator->Attach(targets, nativeOptions);
            manipulatorEntry.presentationSubtype = options->enableModes != 0 ? AllManipulatorModesMask : 0;
            if (!manipulatorEntry.selectable) engine->viewerContext.context->Deactivate(manipulator);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_manipulator_detach(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeManipulatorStatus(engine, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(engine, manipulatorId);
            if (manipulator->IsAttached())
            {
                cancelActiveTransformation(manipulator);
                manipulator->Detach();
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_manipulator_update(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        const OcctManipulatorUpdateOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeManipulatorStatus(engine, [&]
        {
            validateUpdateOptions(options);
            ObjectEntry& entry = requiredManipulatorEntry(engine, manipulatorId);
            Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
            if (manipulator.IsNull())
                throw std::runtime_error("Manipulator presentation has an invalid native type.");

            if ((options->updateMask & OcctManipulatorUpdate_Part) != 0)
            {
                const AIS_ManipulatorMode nativeMode = manipulatorMode(options->mode);
                if (options->axisIndex == -1)
                    manipulator->SetPart(nativeMode, options->enabled != 0);
                else
                    manipulator->SetPart(options->axisIndex, nativeMode, options->enabled != 0);
            }

            if ((options->updateMask & OcctManipulatorUpdate_ModeEnabled) != 0)
            {
                const AIS_ManipulatorMode nativeMode = manipulatorMode(options->mode);
                const int modeBit = 1 << options->mode;
                if (options->enabled != 0)
                {
                    entry.presentationSubtype |= modeBit;
                    if (entry.selectable && manipulator->IsAttached()) manipulator->EnableMode(nativeMode);
                }
                else
                {
                    if (manipulator->HasActiveMode() && manipulator->ActiveMode() == nativeMode)
                        cancelActiveTransformation(manipulator);
                    entry.presentationSubtype &= ~modeBit;
                    engine->viewerContext.context->Deactivate(manipulator, options->mode);
                }
            }

            if ((options->updateMask & OcctManipulatorUpdate_ModeActivationOnDetection) != 0)
                manipulator->SetModeActivationOnDetection(options->enabled != 0);
            if ((options->updateMask & OcctManipulatorUpdate_Position) != 0)
                manipulator->SetPosition(axis2(options->origin, options->normal, options->xDirection));
            if ((options->updateMask & OcctManipulatorUpdate_Size) != 0)
                manipulator->SetSize(static_cast<Standard_ShortReal>(options->size));
            if ((options->updateMask & OcctManipulatorUpdate_Gap) != 0)
                manipulator->SetGap(static_cast<Standard_ShortReal>(options->gap));
            if ((options->updateMask & OcctManipulatorUpdate_ZoomPersistence) != 0)
                manipulator->SetZoomPersistence(options->enabled != 0);
            if ((options->updateMask & OcctManipulatorUpdate_Skin) != 0)
                manipulator->SetSkinMode(manipulatorSkin(options->skinMode));

            redisplayManipulator(engine, manipulator);
        });
    }

    OcctStatus occt_engine_manipulator_state_get(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        OcctManipulatorState* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeManipulatorStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Manipulator state output is null.");
            Handle(AIS_Manipulator) manipulator = requiredManipulator(engine, manipulatorId);
            const gp_Ax2& position = manipulator->Position();
            result->structSize = static_cast<std::uint32_t>(sizeof(OcctManipulatorState));
            result->apiVersion = ManipulatorApiVersion;
            result->attached = manipulator->IsAttached() ? 1 : 0;
            result->activeMode = static_cast<int>(manipulator->ActiveMode());
            result->activeAxisIndex = manipulator->ActiveAxisIndex();
            result->hasActiveTransformation = manipulator->HasActiveTransformation() ? 1 : 0;
            result->modeActivationOnDetection = manipulator->IsModeActivationOnDetection() ? 1 : 0;
            result->zoomPersistence = manipulator->ZoomPersistence() ? 1 : 0;
            result->skinMode = static_cast<int>(manipulator->SkinMode());
            result->origin = {position.Location().X(), position.Location().Y(), position.Location().Z()};
            result->normal = {position.Direction().X(), position.Direction().Y(), position.Direction().Z()};
            result->xDirection = {position.XDirection().X(), position.XDirection().Y(), position.XDirection().Z()};
            result->size = manipulator->Size();
        });
    }

    OcctStatus occt_engine_manipulator_targets_get(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        OcctObjectId* objectIds,
        int capacity,
        int* count)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeManipulatorStatus(engine, [&]
        {
            if (count == nullptr) throw std::invalid_argument("Manipulator target count output is null.");
            if (capacity < 0) throw std::invalid_argument("Manipulator target capacity must not be negative.");
            if (objectIds == nullptr && capacity != 0)
                throw std::invalid_argument("Manipulator target output is null but capacity is non-zero.");

            Handle(AIS_Manipulator) manipulator = requiredManipulator(engine, manipulatorId);
            if (!manipulator->IsAttached())
            {
                *count = 0;
                return;
            }

            const Handle(AIS_ManipulatorObjectSequence) objects = manipulator->Objects();
            const int total = objects.IsNull() ? 0 : objects->Size();
            *count = total;
            if (objectIds == nullptr) return;
            if (capacity < total)
                throw std::out_of_range("Manipulator target output capacity is too small.");

            int outputIndex = 0;
            for (int index = objects->Lower(); index <= objects->Upper(); ++index)
            {
                const OcctObjectId objectId = engine->findPresentation(objects->Value(index));
                if (objectId <= 0)
                    throw std::runtime_error("Manipulator target is not registered in this engine.");
                objectIds[outputIndex++] = objectId;
            }
        });
    }

    OcctStatus occt_engine_manipulator_transform(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        const OcctManipulatorTransformOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeManipulatorStatus(engine, [&]
        {
            validateTransformOptions(options);
            Handle(AIS_Manipulator) manipulator = requiredManipulator(engine, manipulatorId);
            switch (options->action)
            {
                case OcctManipulatorTransform_Start:
                    if (!manipulator->IsAttached()) throw std::logic_error("Manipulator is not attached.");
                    if (!manipulator->HasActiveMode()) throw std::logic_error("Manipulator has no active mode.");
                    manipulator->StartTransform(options->x, options->y, engine->viewerContext.view);
                    break;
                case OcctManipulatorTransform_Update:
                    if (!manipulator->IsAttached()) throw std::logic_error("Manipulator is not attached.");
                    if (!manipulator->HasActiveMode()) throw std::logic_error("Manipulator has no active mode.");
                    manipulator->Transform(options->x, options->y, engine->viewerContext.view);
                    engine->requestRedraw();
                    break;
                case OcctManipulatorTransform_Stop:
                    if (manipulator->HasActiveTransformation()) manipulator->StopTransform(options->apply != 0);
                    engine->requestRedraw();
                    break;
                case OcctManipulatorTransform_DeactivateMode:
                    if (manipulator->HasActiveTransformation())
                        throw std::logic_error("Stop the active manipulator transformation before deactivating its mode.");
                    manipulator->DeactivateCurrentMode();
                    engine->requestRedraw();
                    break;
                default:
                    break;
            }
        });
    }
}
