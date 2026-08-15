#pragma once

#include "OcctNative.h"

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
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int adjustPosition;
        int adjustSize;
        int enableModes;
    };

    enum OcctManipulatorUpdateMask : std::uint32_t
    {
        OcctManipulatorUpdate_Part = 1u << 0,
        OcctManipulatorUpdate_ModeEnabled = 1u << 1,
        OcctManipulatorUpdate_ModeActivationOnDetection = 1u << 2,
        OcctManipulatorUpdate_Position = 1u << 3,
        OcctManipulatorUpdate_Size = 1u << 4,
        OcctManipulatorUpdate_Gap = 1u << 5,
        OcctManipulatorUpdate_ZoomPersistence = 1u << 6,
        OcctManipulatorUpdate_Skin = 1u << 7
    };

    struct OcctManipulatorUpdateOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        int axisIndex;
        int mode;
        int enabled;
        OcctPoint3d origin;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double size;
        double gap;
        int skinMode;
    };

    struct OcctManipulatorState
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
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

    enum OcctManipulatorTransformAction
    {
        OcctManipulatorTransform_Start = 0,
        OcctManipulatorTransform_Update = 1,
        OcctManipulatorTransform_Stop = 2,
        OcctManipulatorTransform_DeactivateMode = 3
    };

    struct OcctManipulatorTransformOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int action;
        int x;
        int y;
        int apply;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_manipulator_create(
        OcctEngineHandle handle,
        OcctObjectId* manipulatorId);

    OCCTBRIDGE_API OcctStatus occt_engine_manipulator_attach(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        const OcctObjectId* objectIds,
        int count,
        const OcctManipulatorAttachOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_manipulator_detach(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId);

    OCCTBRIDGE_API OcctStatus occt_engine_manipulator_update(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        const OcctManipulatorUpdateOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_manipulator_state_get(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        OcctManipulatorState* result);

    OCCTBRIDGE_API OcctStatus occt_engine_manipulator_targets_get(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        OcctObjectId* objectIds,
        int capacity,
        int* count);

    OCCTBRIDGE_API OcctStatus occt_engine_manipulator_transform(
        OcctEngineHandle handle,
        OcctObjectId manipulatorId,
        const OcctManipulatorTransformOptions* options);
}
