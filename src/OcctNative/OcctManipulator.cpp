#include "OcctInternal.hxx"
#include "OcctManipulator.h"

#include <AIS_Manipulator.hxx>
#include <AIS_ManipulatorMode.hxx>
#include <NCollection_HSequence.hxx>

#include <unordered_set>

using namespace OcctBridge;

namespace
{
    constexpr int AllManipulatorModesMask =
        (1 << OcctManipulator_Translation) |
        (1 << OcctManipulator_Rotation) |
        (1 << OcctManipulator_Scaling) |
        (1 << OcctManipulator_TranslationPlane);

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
        if (manipulator.IsNull()) throw std::runtime_error("Manipulator presentation has an invalid native type.");
        return manipulator;
    }

    void redisplayManipulator(Engine* engine, const Handle(AIS_Manipulator)& manipulator)
    {
        if (!manipulator.IsNull() && engine->context->IsDisplayed(manipulator))
            engine->context->Redisplay(manipulator, Standard_False, Standard_True);
        engine->requestRedraw();
    }

    void cancelActiveTransformation(const Handle(AIS_Manipulator)& manipulator)
    {
        if (manipulator->HasActiveTransformation()) manipulator->StopTransform(Standard_False);
        if (manipulator->HasActiveMode()) manipulator->DeactivateCurrentMode();
    }
}

extern "C"
{
    OcctObjectId occt_add_manipulator(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]() -> OcctObjectId
        {
            Handle(AIS_Manipulator) manipulator = new AIS_Manipulator();
            const OcctObjectId id = e->nextId++;
            ObjectEntry entry{OcctManipulatorObjectKind, TopoDS_Shape(), manipulator, "Manipulator"};
            entry.selectable = true;
            entry.presentationSubtype = 0;
            e->objects.emplace(id, std::move(entry));
            return id;
        });
    }

    int occt_attach_manipulator(
        OcctHandle h,
        OcctObjectId manipulatorId,
        const OcctObjectId* objectIds,
        int count,
        const OcctManipulatorAttachOptions* options)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (objectIds == nullptr) throw std::invalid_argument("Manipulator target array is null.");
            requireCount(count, 1, "Manipulator target array");

            ObjectEntry& manipulatorEntry = requiredManipulatorEntry(e, manipulatorId);
            Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(manipulatorEntry.presentation);
            if (manipulator.IsNull()) throw std::runtime_error("Manipulator presentation has an invalid native type.");

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
                ObjectEntry* target = e->findObject(id);
                if (target == nullptr || target->presentation.IsNull())
                    throw std::invalid_argument("Manipulator target object does not exist.");
                if (target->kind == OcctManipulatorObjectKind)
                    throw std::invalid_argument("Manipulator cannot be attached to another manipulator.");
                targets->Append(target->presentation);
            }
            if (targets->IsEmpty()) throw std::invalid_argument("Manipulator target array is empty.");

            AIS_Manipulator::OptionsForAttach nativeOptions;
            if (options != nullptr)
            {
                nativeOptions.SetAdjustPosition(options->adjustPosition != 0)
                    .SetAdjustSize(options->adjustSize != 0)
                    .SetEnableModes(options->enableModes != 0);
            }

            manipulator->Attach(targets, nativeOptions);
            manipulatorEntry.presentationSubtype =
                (options == nullptr || options->enableModes != 0) ? AllManipulatorModesMask : 0;
            if (!manipulatorEntry.selectable) e->context->Deactivate(manipulator);
            e->requestRedraw();
        });
    }

    int occt_detach_manipulator(OcctHandle h, OcctObjectId manipulatorId)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            if (manipulator->IsAttached())
            {
                cancelActiveTransformation(manipulator);
                manipulator->Detach();
            }
            e->requestRedraw();
        });
    }

    int occt_set_manipulator_part(
        OcctHandle h,
        OcctObjectId manipulatorId,
        int axisIndex,
        int mode,
        int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            const AIS_ManipulatorMode nativeMode = manipulatorMode(mode);
            if (axisIndex == -1)
                manipulator->SetPart(nativeMode, enabled != 0);
            else
            {
                if (axisIndex < 0 || axisIndex > 2)
                    throw std::invalid_argument("Manipulator axis index must be -1 or between 0 and 2.");
                manipulator->SetPart(axisIndex, nativeMode, enabled != 0);
            }
            redisplayManipulator(e, manipulator);
        });
    }

    int occt_set_manipulator_mode_enabled(
        OcctHandle h,
        OcctObjectId manipulatorId,
        int mode,
        int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredManipulatorEntry(e, manipulatorId);
            Handle(AIS_Manipulator) manipulator = Handle(AIS_Manipulator)::DownCast(entry.presentation);
            const AIS_ManipulatorMode nativeMode = manipulatorMode(mode);
            const int modeBit = 1 << mode;
            if (enabled != 0)
            {
                entry.presentationSubtype |= modeBit;
                if (entry.selectable && manipulator->IsAttached()) manipulator->EnableMode(nativeMode);
            }
            else
            {
                if (manipulator->HasActiveMode() && manipulator->ActiveMode() == nativeMode)
                    cancelActiveTransformation(manipulator);
                entry.presentationSubtype &= ~modeBit;
                e->context->Deactivate(manipulator, mode);
            }
            e->requestRedraw();
        });
    }

    int occt_set_manipulator_mode_activation_on_detection(
        OcctHandle h,
        OcctObjectId manipulatorId,
        int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            manipulator->SetModeActivationOnDetection(enabled != 0);
            e->requestRedraw();
        });
    }

    int occt_set_manipulator_position(
        OcctHandle h,
        OcctObjectId manipulatorId,
        OcctPoint3d origin,
        OcctVector3d normal,
        OcctVector3d xDirection)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            manipulator->SetPosition(axis2(origin, normal, xDirection));
            e->requestRedraw();
        });
    }

    int occt_set_manipulator_size(OcctHandle h, OcctObjectId manipulatorId, double size)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            requirePositive(size, "Manipulator size");
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            manipulator->SetSize(static_cast<Standard_ShortReal>(size));
            redisplayManipulator(e, manipulator);
        });
    }

    int occt_set_manipulator_gap(OcctHandle h, OcctObjectId manipulatorId, double gap)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(gap) || gap < 0.0)
                throw std::invalid_argument("Manipulator gap must be finite and non-negative.");
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            manipulator->SetGap(static_cast<Standard_ShortReal>(gap));
            redisplayManipulator(e, manipulator);
        });
    }

    int occt_set_manipulator_zoom_persistence(OcctHandle h, OcctObjectId manipulatorId, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            manipulator->SetZoomPersistence(enabled != 0);
            redisplayManipulator(e, manipulator);
        });
    }

    int occt_set_manipulator_skin(OcctHandle h, OcctObjectId manipulatorId, int skinMode)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            manipulator->SetSkinMode(manipulatorSkin(skinMode));
            redisplayManipulator(e, manipulator);
        });
    }

    int occt_get_manipulator_state(
        OcctHandle h,
        OcctObjectId manipulatorId,
        OcctManipulatorState* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            const gp_Ax2& position = manipulator->Position();
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

    int occt_get_manipulator_objects(
        OcctHandle h,
        OcctObjectId manipulatorId,
        OcctObjectId* objectIds,
        int capacity,
        int* count)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || count == nullptr) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            if (!manipulator->IsAttached())
            {
                *count = 0;
                return;
            }

            const Handle(AIS_ManipulatorObjectSequence) objects = manipulator->Objects();
            const int total = objects.IsNull() ? 0 : objects->Size();
            *count = total;
            if (objectIds == nullptr || capacity == 0) return;
            if (capacity < total) throw std::out_of_range("Manipulator target output capacity is too small.");

            int outputIndex = 0;
            for (int index = objects->Lower(); index <= objects->Upper(); ++index)
            {
                const OcctObjectId objectId = e->findPresentation(objects->Value(index));
                if (objectId <= 0) throw std::runtime_error("Manipulator target is not registered in this engine.");
                objectIds[outputIndex++] = objectId;
            }
        });
    }

    int occt_start_manipulator_transform(OcctHandle h, OcctObjectId manipulatorId, int x, int y)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            if (!manipulator->IsAttached()) throw std::logic_error("Manipulator is not attached.");
            if (!manipulator->HasActiveMode()) throw std::logic_error("Manipulator has no active mode.");
            manipulator->StartTransform(x, y, e->view);
        });
    }

    int occt_update_manipulator_transform(OcctHandle h, OcctObjectId manipulatorId, int x, int y)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            if (!manipulator->IsAttached()) throw std::logic_error("Manipulator is not attached.");
            if (!manipulator->HasActiveMode()) throw std::logic_error("Manipulator has no active mode.");
            manipulator->Transform(x, y, e->view);
            e->requestRedraw();
        });
    }

    int occt_stop_manipulator_transform(OcctHandle h, OcctObjectId manipulatorId, int apply)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            if (manipulator->HasActiveTransformation()) manipulator->StopTransform(apply != 0);
            e->requestRedraw();
        });
    }

    int occt_deactivate_manipulator_mode(OcctHandle h, OcctObjectId manipulatorId)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            Handle(AIS_Manipulator) manipulator = requiredManipulator(e, manipulatorId);
            if (manipulator->HasActiveTransformation())
                throw std::logic_error("Stop the active manipulator transformation before deactivating its mode.");
            manipulator->DeactivateCurrentMode();
            e->requestRedraw();
        });
    }
}
