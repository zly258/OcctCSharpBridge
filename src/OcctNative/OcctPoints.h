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
}
