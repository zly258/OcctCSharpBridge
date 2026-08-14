#pragma once

#include "OcctNative.h"

constexpr int OcctOverlayObjectKind = 5;

extern "C"
{
    enum OcctOverlayPrimitiveType
    {
        OcctOverlay_Line = 0,
        OcctOverlay_Polyline = 1,
        OcctOverlay_Marker = 2,
        OcctOverlay_Text = 3
    };

    enum OcctOverlayLinePattern
    {
        OcctOverlayLine_Solid = 0,
        OcctOverlayLine_Dashed = 1,
        OcctOverlayLine_Dotted = 2,
        OcctOverlayLine_DashDot = 3
    };

    enum OcctOverlayLineUpdateMask : std::uint32_t
    {
        OcctOverlayLineUpdate_Geometry = 1u << 0,
        OcctOverlayLineUpdate_Style = 1u << 1
    };

    enum OcctOverlayMarkerUpdateMask : std::uint32_t
    {
        OcctOverlayMarkerUpdate_Position = 1u << 0,
        OcctOverlayMarkerUpdate_Style = 1u << 1
    };

    enum OcctOverlayTextUpdateMask : std::uint32_t
    {
        OcctOverlayTextUpdate_Content = 1u << 0,
        OcctOverlayTextUpdate_Position = 1u << 1,
        OcctOverlayTextUpdate_Style = 1u << 2
    };

    struct OcctOverlayLineOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        int primitiveType;
        const OcctPoint3d* points;
        int pointCount;
        int pattern;
        double width;
        double red;
        double green;
        double blue;
    };

    struct OcctOverlayMarkerOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        OcctPoint3d position;
        int marker;
        double scale;
        double red;
        double green;
        double blue;
    };

    struct OcctOverlayTextOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        const char* text;
        OcctPoint3d position;
        double height;
        double red;
        double green;
        double blue;
        int zoomable;
        const char* fontName;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_overlay_line_create(
        OcctEngineHandle handle,
        const OcctOverlayLineOptions* options,
        OcctObjectId* resultOverlayId);

    OCCTBRIDGE_API OcctStatus occt_engine_overlay_line_update(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        const OcctOverlayLineOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_overlay_marker_create(
        OcctEngineHandle handle,
        const OcctOverlayMarkerOptions* options,
        OcctObjectId* resultOverlayId);

    OCCTBRIDGE_API OcctStatus occt_engine_overlay_marker_update(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        const OcctOverlayMarkerOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_overlay_text_create(
        OcctEngineHandle handle,
        const OcctOverlayTextOptions* options,
        OcctObjectId* resultOverlayId);

    OCCTBRIDGE_API OcctStatus occt_engine_overlay_text_update(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        const OcctOverlayTextOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_overlay_primitive_type_get(
        OcctEngineHandle handle,
        OcctObjectId overlayId,
        int* primitiveType);
}
