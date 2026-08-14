#pragma once

#include "OcctNative.h"

// Appended object-kind value. Existing numeric values remain stable.
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

    OCCTBRIDGE_API OcctObjectId occt_add_overlay_line(OcctHandle handle, OcctPoint3d start, OcctPoint3d end, int pattern, double width, double r, double g, double b);
    OCCTBRIDGE_API OcctObjectId occt_add_overlay_polyline(OcctHandle handle, const OcctPoint3d* points, int count, int pattern, double width, double r, double g, double b);
    OCCTBRIDGE_API OcctObjectId occt_add_overlay_marker(OcctHandle handle, OcctPoint3d position, int marker, double scale, double r, double g, double b);
    OCCTBRIDGE_API OcctObjectId occt_add_overlay_text(OcctHandle handle, const char* text, OcctPoint3d position, double height, double r, double g, double b, int zoomable, const char* fontName);
    OCCTBRIDGE_API int occt_update_overlay_line(OcctHandle handle, OcctObjectId overlayId, OcctPoint3d start, OcctPoint3d end);
    OCCTBRIDGE_API int occt_update_overlay_polyline(OcctHandle handle, OcctObjectId overlayId, const OcctPoint3d* points, int count);
    OCCTBRIDGE_API int occt_update_overlay_marker(OcctHandle handle, OcctObjectId overlayId, OcctPoint3d position);
    OCCTBRIDGE_API int occt_update_overlay_text(OcctHandle handle, OcctObjectId overlayId, const char* text, OcctPoint3d position);
    OCCTBRIDGE_API int occt_set_overlay_line_style(OcctHandle handle, OcctObjectId overlayId, int pattern, double width, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_overlay_marker_style(OcctHandle handle, OcctObjectId overlayId, int marker, double scale, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_overlay_text_style(OcctHandle handle, OcctObjectId overlayId, double height, double r, double g, double b, int zoomable, const char* fontName);
    OCCTBRIDGE_API int occt_get_overlay_primitive_type(OcctHandle handle, OcctObjectId overlayId, int* primitiveType);
}
