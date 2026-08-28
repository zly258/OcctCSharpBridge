#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_curve_helix_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d origin,
        OcctVector3d axis,
        OcctVector3d xDirection,
        double radius,
        double pitch,
        double turns,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_vertex_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d point,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_line_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d start,
        OcctPoint3d end,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_polyline_create(
        OcctModelingSessionHandle handle,
        const OcctPoint3d* points,
        int count,
        OcctBool closed,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_circle_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        double radius,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_arc_three_points_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d start,
        OcctPoint3d middle,
        OcctPoint3d end,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_arc_center_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_ellipse_create(
        OcctModelingSessionHandle handle,
        OcctPoint3d center,
        OcctVector3d normal,
        double majorRadius,
        double minorRadius,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_bezier_create(
        OcctModelingSessionHandle handle,
        const OcctPoint3d* poles,
        int count,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_curve_bspline_interpolated_create(
        OcctModelingSessionHandle handle,
        const OcctPoint3d* points,
        int count,
        OcctBool periodic,
        double tolerance,
        OcctObjectId* result);
}
