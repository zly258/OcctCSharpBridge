#pragma once

#include "OcctNative.h"

extern "C"
{
    // Displays or updates an OCCT-native 2D rubber-band rectangle in the top overlay layer.
    // Coordinates use the host window client coordinate system (origin at left/top).
    OCCTBRIDGE_API int occt_show_selection_rectangle(
        OcctHandle handle,
        int x1,
        int y1,
        int x2,
        int y2,
        double lineR,
        double lineG,
        double lineB,
        double fillR,
        double fillG,
        double fillB,
        double fillTransparency,
        double lineWidth);

    OCCTBRIDGE_API int occt_hide_selection_rectangle(OcctHandle handle);
}
