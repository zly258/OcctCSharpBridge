#pragma once
#include "OcctNative.h"
extern "C" {
OCCTBRIDGE_API OcctStatus occt_model_make_vertex(OcctModelingSessionHandle, OcctPoint3d, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_line(OcctModelingSessionHandle, OcctPoint3d, OcctPoint3d, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_polyline(OcctModelingSessionHandle, const OcctPoint3d*, int, OcctBool, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_circle(OcctModelingSessionHandle, OcctPoint3d, OcctVector3d, double, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_arc_three_points(OcctModelingSessionHandle, OcctPoint3d, OcctPoint3d, OcctPoint3d, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_arc_center(OcctModelingSessionHandle, OcctPoint3d, OcctVector3d, OcctVector3d, double, double, double, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_ellipse(OcctModelingSessionHandle, OcctPoint3d, OcctVector3d, double, double, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_bezier(OcctModelingSessionHandle, const OcctPoint3d*, int, OcctObjectId*);
OCCTBRIDGE_API OcctStatus occt_model_make_bspline_interpolated(OcctModelingSessionHandle, const OcctPoint3d*, int, OcctBool, double, OcctObjectId*);
}
