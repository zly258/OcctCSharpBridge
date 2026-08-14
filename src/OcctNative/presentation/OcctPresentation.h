#pragma once

#include "OcctNative.h"
#include "OcctViewerInteractionExtensions.h"

extern "C"
{
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

    OCCTBRIDGE_API OcctStatus occt_engine_presentation_state_update(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerPresentationStateOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_presentation_state_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctViewerPresentationState* result);

    OCCTBRIDGE_API int occt_set_object_clip_planes(
        OcctHandle handle,
        OcctObjectId objectId,
        const OcctViewClipPlane* planes,
        int count);

    OCCTBRIDGE_API int occt_set_global_highlight_style(
        OcctHandle handle,
        int kind,
        const OcctHighlightStyleSettings* settings);

    OCCTBRIDGE_API int occt_set_object_highlight_style(
        OcctHandle handle,
        OcctObjectId objectId,
        int dynamic,
        const OcctHighlightStyleSettings* settings);

    OCCTBRIDGE_API int occt_clear_object_highlight_style(
        OcctHandle handle,
        OcctObjectId objectId,
        int dynamic);

    OCCTBRIDGE_API int occt_reset_object_display_mode(
        OcctHandle handle,
        OcctObjectId objectId);

    OCCTBRIDGE_API int occt_get_object_display_mode(
        OcctHandle handle,
        OcctObjectId objectId,
        int* hasOverride,
        int* displayMode);

    OCCTBRIDGE_API int occt_set_object_auto_highlight(
        OcctHandle handle,
        OcctObjectId objectId,
        int enabled);

    OCCTBRIDGE_API int occt_get_object_auto_highlight(
        OcctHandle handle,
        OcctObjectId objectId,
        int* enabled);

    OCCTBRIDGE_API int occt_set_object_infinite_state(
        OcctHandle handle,
        OcctObjectId objectId,
        int infinite);

    OCCTBRIDGE_API int occt_get_object_infinite_state(
        OcctHandle handle,
        OcctObjectId objectId,
        int* infinite);
}
