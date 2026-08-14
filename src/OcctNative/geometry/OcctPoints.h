#pragma once

#include "OcctNative.h"

// Appended object-kind value. Existing OcctObjectKind numeric values remain stable.
constexpr int OcctPointObjectKind = 4;

extern "C"
{
    enum OcctPointMarker
    {
        OcctPointMarker_Point = 0,
        OcctPointMarker_Plus = 1,
        OcctPointMarker_Star = 2,
        OcctPointMarker_X = 3,
        OcctPointMarker_Circle = 4,
        OcctPointMarker_CirclePoint = 5,
        OcctPointMarker_CirclePlus = 6,
        OcctPointMarker_CircleStar = 7,
        OcctPointMarker_CircleX = 8,
        OcctPointMarker_Ring1 = 9,
        OcctPointMarker_Ring2 = 10,
        OcctPointMarker_Ring3 = 11,
        OcctPointMarker_Ball = 12
    };

    enum OcctPixelFormat
    {
        OcctPixelFormat_Bgra32 = 0,
        OcctPixelFormat_Rgba32 = 1
    };

    enum OcctViewerPointUpdateMask : std::uint32_t
    {
        OcctViewerPointUpdate_Position = 1u << 0,
        OcctViewerPointUpdate_Style = 1u << 1
    };

    struct OcctViewerPointOptions
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

    OCCTBRIDGE_API OcctStatus occt_engine_point_create(
        OcctEngineHandle handle,
        const OcctViewerPointOptions* options,
        OcctObjectId* resultPointId);

    OCCTBRIDGE_API OcctStatus occt_engine_point_update(
        OcctEngineHandle handle,
        OcctObjectId pointId,
        const OcctViewerPointOptions* options);

    // Frozen ABI 4 compatibility shell. New SDK code uses occt_engine_point_*.
    OCCTBRIDGE_API OcctObjectId occt_add_point(
        OcctHandle handle,
        OcctPoint3d position,
        int marker,
        double scale,
        double r,
        double g,
        double b);

    OCCTBRIDGE_API int occt_set_point_position(
        OcctHandle handle,
        OcctObjectId pointId,
        OcctPoint3d position);

    OCCTBRIDGE_API int occt_set_point_style(
        OcctHandle handle,
        OcctObjectId pointId,
        int marker,
        double scale,
        double r,
        double g,
        double b);

    OCCTBRIDGE_API OcctObjectId occt_add_point_pixmap(
        OcctHandle handle,
        OcctPoint3d position,
        int width,
        int height,
        const unsigned char* pixels,
        int pixelCount,
        int pixelFormat);

    OCCTBRIDGE_API int occt_set_point_pixmap_style(
        OcctHandle handle,
        OcctObjectId pointId,
        int width,
        int height,
        const unsigned char* pixels,
        int pixelCount,
        int pixelFormat);
}
