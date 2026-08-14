#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerHighlightUpdateMask : std::uint32_t
    {
        OcctViewerHighlightUpdate_Selection = 1u << 0,
        OcctViewerHighlightUpdate_Hover = 1u << 1
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

    OCCTBRIDGE_API OcctStatus occt_engine_scene_lighting_set(
        OcctEngineHandle handle,
        const OcctViewerLightingOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_highlight_colors_set(
        OcctEngineHandle handle,
        const OcctViewerHighlightOptions* options);
}
