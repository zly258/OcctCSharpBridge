#pragma once

#include "OcctNative.h"

// Appended object-kind value. Existing numeric values remain stable.
constexpr int OcctManipulatorObjectKind = 6;

extern "C"
{
    enum OcctManipulatorMode
    {
        OcctManipulator_None = 0,
        OcctManipulator_Translation = 1,
        OcctManipulator_Rotation = 2,
        OcctManipulator_Scaling = 3,
        OcctManipulator_TranslationPlane = 4
    };

    enum OcctManipulatorSkin
    {
        OcctManipulatorSkin_Shaded = 0,
        OcctManipulatorSkin_Flat = 1
    };

    struct OcctManipulatorAttachOptions
    {
        int adjustPosition;
        int adjustSize;
        int enableModes;
    };

    struct OcctManipulatorState
    {
        int attached;
        int activeMode;
        int activeAxisIndex;
        int hasActiveTransformation;
        int modeActivationOnDetection;
        int zoomPersistence;
        int skinMode;
        OcctPoint3d origin;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double size;
    };

    OCCTBRIDGE_API OcctObjectId occt_add_manipulator(OcctHandle handle);

    OCCTBRIDGE_API int occt_attach_manipulator(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        const OcctObjectId* objectIds,
        int count,
        const OcctManipulatorAttachOptions* options);

    OCCTBRIDGE_API int occt_detach_manipulator(
        OcctHandle handle,
        OcctObjectId manipulatorId);

    OCCTBRIDGE_API int occt_set_manipulator_part(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int axisIndex,
        int mode,
        int enabled);

    OCCTBRIDGE_API int occt_set_manipulator_mode_enabled(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int mode,
        int enabled);

    OCCTBRIDGE_API int occt_set_manipulator_mode_activation_on_detection(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int enabled);

    OCCTBRIDGE_API int occt_set_manipulator_position(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        OcctPoint3d origin,
        OcctVector3d normal,
        OcctVector3d xDirection);

    OCCTBRIDGE_API int occt_set_manipulator_size(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        double size);

    OCCTBRIDGE_API int occt_set_manipulator_gap(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        double gap);

    OCCTBRIDGE_API int occt_set_manipulator_zoom_persistence(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int enabled);

    OCCTBRIDGE_API int occt_set_manipulator_skin(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int skinMode);

    OCCTBRIDGE_API int occt_get_manipulator_state(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        OcctManipulatorState* result);

    OCCTBRIDGE_API int occt_get_manipulator_objects(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        OcctObjectId* objectIds,
        int capacity,
        int* count);

    OCCTBRIDGE_API int occt_start_manipulator_transform(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int x,
        int y);

    OCCTBRIDGE_API int occt_update_manipulator_transform(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int x,
        int y);

    OCCTBRIDGE_API int occt_stop_manipulator_transform(
        OcctHandle handle,
        OcctObjectId manipulatorId,
        int apply);

    OCCTBRIDGE_API int occt_deactivate_manipulator_mode(
        OcctHandle handle,
        OcctObjectId manipulatorId);
}
