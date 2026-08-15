#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctPresentationZLayer
    {
        OcctPresentationZLayer_Bottom = 0,
        OcctPresentationZLayer_Default = 1,
        OcctPresentationZLayer_Top = 2,
        OcctPresentationZLayer_Topmost = 3
    };

    enum OcctTransformPersistenceMode
    {
        OcctTransformPersistence_None = 0,
        OcctTransformPersistence_Zoom = 1,
        OcctTransformPersistence_Rotate = 2,
        OcctTransformPersistence_ZoomRotate = 3,
        OcctTransformPersistence_Screen2d = 4,
        OcctTransformPersistence_Triedron = 5
    };

    struct OcctTransformPersistenceState
    {
        int mode;
        OcctPoint3d anchor;
        int position;
        int offsetX;
        int offsetY;
    };

    struct OcctPresentationClipPlane
    {
        OcctPoint3d point;
        OcctVector3d normal;
        int enabled;
        int capping;
        double cappingR;
        double cappingG;
        double cappingB;
    };

    struct OcctHighlightStyleSettings
    {
        double r;
        double g;
        double b;
        double transparency;
        double lineWidth;
        int displayMode;
        int zLayer;
    };

    enum OcctViewerPresentationStateUpdateMask : std::uint32_t
    {
        OcctViewerPresentationStateUpdate_DisplayMode = 1u << 0,
        OcctViewerPresentationStateUpdate_ResetDisplayMode = 1u << 1,
        OcctViewerPresentationStateUpdate_AutoHighlight = 1u << 2,
        OcctViewerPresentationStateUpdate_Infinite = 1u << 3
    };

    struct OcctViewerPresentationStateOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        int displayMode;
        int autoHighlight;
        int infinite;
    };

    struct OcctViewerPresentationState
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int hasDisplayModeOverride;
        int displayMode;
        int autoHighlight;
        int infinite;
    };

    struct OcctViewerClipPlanesOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        const OcctPresentationClipPlane* planes;
        int count;
    };

    struct OcctViewerHighlightStyleOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int kind;
        int dynamic;
        OcctHighlightStyleSettings settings;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_presentation_state_update(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerPresentationStateOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_presentation_state_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctViewerPresentationState* result);

    OCCTBRIDGE_API OcctStatus occt_engine_presentation_clip_planes_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerClipPlanesOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_highlight_style_global_set(
        OcctEngineHandle handle,
        const OcctViewerHighlightStyleOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_highlight_style_object_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerHighlightStyleOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_highlight_style_object_clear(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int dynamic);

    // Frozen ABI4 compatibility. Implemented by the presentation domain.
    OCCTBRIDGE_API int occt_set_object_display_priority(
        OcctHandle handle,
        OcctObjectId objectId,
        int priority);

    OCCTBRIDGE_API int occt_set_objects_display_priority(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int priority);

    OCCTBRIDGE_API int occt_get_object_display_priority(
        OcctHandle handle,
        OcctObjectId objectId,
        int* priority);

    OCCTBRIDGE_API int occt_set_object_transform_persistence_3d(
        OcctHandle handle,
        OcctObjectId objectId,
        int mode,
        OcctPoint3d anchor);

    OCCTBRIDGE_API int occt_set_object_transform_persistence_2d(
        OcctHandle handle,
        OcctObjectId objectId,
        int mode,
        int position,
        int offsetX,
        int offsetY);

    OCCTBRIDGE_API int occt_clear_object_transform_persistence(
        OcctHandle handle,
        OcctObjectId objectId);

    OCCTBRIDGE_API int occt_get_object_transform_persistence(
        OcctHandle handle,
        OcctObjectId objectId,
        OcctTransformPersistenceState* result);
}
