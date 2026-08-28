#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerViewStateUpdateMask : std::uint32_t
    {
        OcctViewerViewStateUpdate_Orientation = 1u << 0,
        OcctViewerViewStateUpdate_Projection = 1u << 1,
        OcctViewerViewStateUpdate_PerspectiveFov = 1u << 2,
        OcctViewerViewStateUpdate_SolidBackground = 1u << 3,
        OcctViewerViewStateUpdate_GradientBackground = 1u << 4,
        OcctViewerViewStateUpdate_DisplayMode = 1u << 5,
        OcctViewerViewStateUpdate_TriedronVisible = 1u << 6,
        OcctViewerViewStateUpdate_ViewCubeVisible = 1u << 7,
        OcctViewerViewStateUpdate_ComputedMode = 1u << 8,
        OcctViewerViewStateUpdate_Antialiasing = 1u << 9,
        OcctViewerViewStateUpdate_Scale = 1u << 10
    };

    struct OcctViewerViewStateOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        int orientation;
        int projectionType;
        double perspectiveFovDegrees;
        OcctColorRgb backgroundFirst;
        OcctColorRgb backgroundSecond;
        int gradientFillMethod;
        int displayMode;
        int triedronVisible;
        int viewCubeVisible;
        int computedMode;
        int antialiasingEnabled;
        double scale;
        int fitAfterOrientation;
    };

    enum OcctViewerDisplayQualityUpdateMask : std::uint32_t
    {
        OcctViewerDisplayQualityUpdate_Precision = 1u << 0,
        OcctViewerDisplayQualityUpdate_DefaultMaterial = 1u << 1
    };

    struct OcctViewerDisplayQualityOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        double deviationCoefficient;
        double deviationAngleDegrees;
        int material;
        int applyPrecisionToExisting;
        int applyMaterialToExisting;
    };

    enum OcctViewerNavigationAction
    {
        OcctViewerNavigation_StartRotation = 0,
        OcctViewerNavigation_Rotation = 1,
        OcctViewerNavigation_Pan = 2,
        OcctViewerNavigation_Zoom = 3
    };

    struct OcctViewerNavigationOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int action;
        int x;
        int y;
        int deltaX;
        int deltaY;
        double factor;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_view_state_update(
        OcctEngineHandle handle,
        const OcctViewerViewStateOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_view_display_quality_update(
        OcctEngineHandle handle,
        const OcctViewerDisplayQualityOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_view_camera_get(
        OcctEngineHandle handle,
        OcctCameraState* result);

    OCCTBRIDGE_API OcctStatus occt_engine_view_camera_set(
        OcctEngineHandle handle,
        const OcctCameraState* state);

    OCCTBRIDGE_API OcctStatus occt_engine_view_fit_all(OcctEngineHandle handle);

    OCCTBRIDGE_API OcctStatus occt_engine_view_fit_object(
        OcctEngineHandle handle,
        OcctObjectId objectId);

    OCCTBRIDGE_API OcctStatus occt_engine_view_window_fit(
        OcctEngineHandle handle,
        int x1,
        int y1,
        int x2,
        int y2);

    OCCTBRIDGE_API OcctStatus occt_engine_view_screen_to_world(
        OcctEngineHandle handle,
        int x,
        int y,
        OcctPoint3d* result);

    OCCTBRIDGE_API OcctStatus occt_engine_view_projection_ray(
        OcctEngineHandle handle,
        int x,
        int y,
        OcctProjectionRay* result);

    OCCTBRIDGE_API OcctStatus occt_engine_view_world_to_screen(
        OcctEngineHandle handle,
        OcctPoint3d point,
        int* x,
        int* y);

    OCCTBRIDGE_API OcctStatus occt_engine_view_navigation(
        OcctEngineHandle handle,
        const OcctViewerNavigationOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_view_dump(
        OcctEngineHandle handle,
        const char* utf8Path);
}
