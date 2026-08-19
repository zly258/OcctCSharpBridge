#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerHighlightUpdateMask : std::uint32_t
    {
        OcctViewerHighlightUpdate_Selection = 1u << 0,
        OcctViewerHighlightUpdate_Hover = 1u << 1
    };

    enum OcctViewerHighlightStyleUpdateMask : std::uint32_t
    {
        OcctViewerHighlightStyleUpdate_SelectionColor = 1u << 0,
        OcctViewerHighlightStyleUpdate_HoverColor = 1u << 1,
        OcctViewerHighlightStyleUpdate_SelectionMode = 1u << 2,
        OcctViewerHighlightStyleUpdate_HoverMode = 1u << 3
    };

    enum OcctHighlightMode
    {
        OcctHighlight_BoundingBox = 0,
        OcctHighlight_Wireframe = 1,
        OcctHighlight_Shaded = 2
    };

    enum OcctLineStyle
    {
        OcctLineStyle_Solid = 0,
        OcctLineStyle_Dash = 1,
        OcctLineStyle_Dot = 2,
        OcctLineStyle_DotDash = 3,
        OcctLineStyle_Center = 4
    };

    struct OcctViewerLightingOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        OcctSceneLightingSettings settings;
    };

    struct OcctViewerHighlightOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        OcctColorRgb selectionColor;
        OcctColorRgb hoverColor;
    };

    struct OcctViewerHighlightStyleOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        OcctColorRgb selectionColor;
        OcctColorRgb hoverColor;
        int selectionMode;
        int hoverMode;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_scene_lighting_set(
        OcctEngineHandle handle,
        const OcctViewerLightingOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_scene_lighting_reset(
        OcctEngineHandle handle);

    OCCTBRIDGE_API OcctStatus occt_engine_highlight_colors_set(
        OcctEngineHandle handle,
        const OcctViewerHighlightOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_highlight_style_set(
        OcctEngineHandle handle,
        const OcctViewerHighlightStyleOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_object_line_style_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int lineStyle);
}
