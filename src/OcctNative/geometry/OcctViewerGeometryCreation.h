#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_shape_vertex_create(OcctEngineHandle handle, OcctPoint3d point, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_line_create(OcctEngineHandle handle, OcctPoint3d start, OcctPoint3d end, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_polyline_create(OcctEngineHandle handle, const OcctPoint3d* points, int count, OcctBool closed, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_triangulated_mesh_create(OcctEngineHandle handle, const OcctPoint3d* vertices, int vertexCount, const int* triangleIndices, int triangleIndexCount, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_circle_create(OcctEngineHandle handle, OcctPoint3d center, OcctVector3d normal, double radius, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_arc_three_points_create(OcctEngineHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_arc_center_create(OcctEngineHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_ellipse_create(OcctEngineHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_bezier_create(OcctEngineHandle handle, const OcctPoint3d* poles, int count, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_bspline_interpolated_create(OcctEngineHandle handle, const OcctPoint3d* points, int count, OcctBool periodic, double tolerance, OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_regular_polygon_create(OcctEngineHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, OcctBool makeFace, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_rectangle_wire_create(OcctEngineHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_face_from_wire_create(OcctEngineHandle handle, OcctObjectId wireId, OcctBool onlyPlane, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_plane_face_create(OcctEngineHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height, OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_box_create(OcctEngineHandle handle, double x, double y, double z, double dx, double dy, double dz, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_cylinder_create(OcctEngineHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_sphere_create(OcctEngineHandle handle, OcctPoint3d center, double radius, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_cone_create(OcctEngineHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_torus_create(OcctEngineHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_wedge_create(OcctEngineHandle handle, double dx, double dy, double dz, double ltx, OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_compound_create(OcctEngineHandle handle, const OcctObjectId* shapeIds, int count, OcctBool hideInputs, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_wire_create(OcctEngineHandle handle, const OcctObjectId* edgeIds, int count, OcctBool hideInputs, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_sew(OcctEngineHandle handle, const OcctObjectId* shapeIds, int count, double tolerance, OcctBool hideInputs, OcctObjectId* result);
    OCCTBRIDGE_API OcctStatus occt_engine_shape_solid_from_shell_create(OcctEngineHandle handle, OcctObjectId shellId, OcctBool hideInput, OcctObjectId* result);
}
