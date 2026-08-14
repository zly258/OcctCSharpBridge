#pragma once

#include "OcctNative.h"

extern "C"
{
    struct OcctViewerSelectionRectangleOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int x1;
        int y1;
        int x2;
        int y2;
        OcctColorRgb lineColor;
        OcctColorRgb fillColor;
        double fillTransparency;
        double lineWidth;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_selection_rectangle_overlay_show(
        OcctEngineHandle handle,
        const OcctViewerSelectionRectangleOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_rectangle_overlay_hide(
        OcctEngineHandle handle);
}
