#pragma once

#include <cstdint>

#if defined(_WIN32)
#define OCCTBRIDGE_API __declspec(dllexport)
#else
#define OCCTBRIDGE_API
#endif

extern "C"
{
    using OcctHandle = void*;
    using OcctObjectId = std::int64_t;

    struct OcctPoint3d { double x; double y; double z; };
    struct OcctVector3d { double x; double y; double z; };
    struct OcctBounds { double minX; double minY; double minZ; double maxX; double maxY; double maxZ; };
    struct OcctMassProperties { double mass; double centerX; double centerY; double centerZ; };
    struct OcctDistanceResult { double distance; OcctPoint3d pointOnFirst; OcctPoint3d pointOnSecond; };
    struct OcctCameraState { OcctPoint3d eye; OcctPoint3d center; OcctVector3d up; OcctVector3d direction; double scale; };
    struct OcctProjectionRay { OcctPoint3d origin; OcctVector3d direction; };
    struct OcctAutoZFitSettings { int enabled; double scaleFactor; };
    struct OcctPolygonOffsetSettings { int mode; double factor; double units; };
    struct OcctColorRgb { double r; double g; double b; };
    struct OcctSceneLightingSettings
    {
        OcctColorRgb ambientColor;
        double ambientIntensity;
        int cameraLightEnabled;
        OcctColorRgb cameraLightColor;
        double cameraLightIntensity;
        OcctVector3d cameraLightDirection;
        int sunLightEnabled;
        OcctColorRgb sunLightColor;
        double sunLightIntensity;
        OcctVector3d sunLightDirection;
        int fillLightEnabled;
        OcctColorRgb fillLightColor;
        double fillLightIntensity;
        OcctVector3d fillLightDirection;
    };
    struct OcctUvBounds { double uMin; double uMax; double vMin; double vMax; };

    enum OcctObjectKind { OcctObject_Unknown = 0, OcctObject_Shape = 1, OcctObject_Text = 2, OcctObject_Dimension = 3 };
    enum OcctShapeType { OcctShape_Compound = 0, OcctShape_CompSolid = 1, OcctShape_Solid = 2, OcctShape_Shell = 3, OcctShape_Face = 4, OcctShape_Wire = 5, OcctShape_Edge = 6, OcctShape_Vertex = 7, OcctShape_Shape = 8 };
    enum OcctViewOrientation { OcctView_Isometric = 0, OcctView_Front = 1, OcctView_Back = 2, OcctView_Left = 3, OcctView_Right = 4, OcctView_Top = 5, OcctView_Bottom = 6 };
    enum OcctProjectionType { OcctProjection_Orthographic = 0, OcctProjection_Perspective = 1 };
    enum OcctDisplayMode { OcctDisplay_Wireframe = 0, OcctDisplay_Shaded = 1 };
    enum OcctRenderingMethod { OcctRendering_Rasterization = 0, OcctRendering_RayTracing = 1 };
    enum OcctZUpViewOrientation
    {
        OcctZUp_Front = 0, OcctZUp_Back = 1, OcctZUp_Left = 2, OcctZUp_Right = 3,
        OcctZUp_Top = 4, OcctZUp_Bottom = 5,
        OcctZUp_XNegativeYNegative = 6, OcctZUp_XPositiveYNegative = 7,
        OcctZUp_XNegativeYPositive = 8, OcctZUp_XPositiveYPositive = 9
    };
    enum OcctSelectionMode { OcctSelection_Object = 0, OcctSelection_Vertex = 1, OcctSelection_Edge = 2, OcctSelection_Wire = 3, OcctSelection_Face = 4, OcctSelection_Shell = 5, OcctSelection_Solid = 6 };
    enum OcctBooleanOperation { OcctBoolean_Fuse = 0, OcctBoolean_Cut = 1, OcctBoolean_Common = 2, OcctBoolean_Section = 3 };
    enum OcctCurveType { OcctCurve_Line = 0, OcctCurve_Circle = 1, OcctCurve_Ellipse = 2, OcctCurve_Hyperbola = 3, OcctCurve_Parabola = 4, OcctCurve_Bezier = 5, OcctCurve_BSpline = 6, OcctCurve_Offset = 7, OcctCurve_Other = 8 };
    enum OcctSurfaceType { OcctSurface_Plane = 0, OcctSurface_Cylinder = 1, OcctSurface_Cone = 2, OcctSurface_Sphere = 3, OcctSurface_Torus = 4, OcctSurface_Bezier = 5, OcctSurface_BSpline = 6, OcctSurface_Revolution = 7, OcctSurface_Extrusion = 8, OcctSurface_Offset = 9, OcctSurface_Other = 10 };
    enum OcctMaterial
    {
        OcctMaterial_Brass = 0, OcctMaterial_Bronze = 1, OcctMaterial_Copper = 2, OcctMaterial_Gold = 3,
        OcctMaterial_Pewter = 4, OcctMaterial_Plastered = 5, OcctMaterial_Plastified = 6, OcctMaterial_Silver = 7,
        OcctMaterial_Steel = 8, OcctMaterial_Stone = 9, OcctMaterial_ShinyPlastified = 10, OcctMaterial_Satin = 11,
        OcctMaterial_Metalized = 12, OcctMaterial_Ionized = 13, OcctMaterial_Chrome = 14, OcctMaterial_Aluminum = 15,
        OcctMaterial_Obsidian = 16, OcctMaterial_Neon = 17, OcctMaterial_Jade = 18, OcctMaterial_Charcoal = 19,
        OcctMaterial_Water = 20, OcctMaterial_Glass = 21, OcctMaterial_Diamond = 22, OcctMaterial_Transparent = 23,
        OcctMaterial_Default = 24
    };

    OCCTBRIDGE_API OcctHandle occt_create();
    OCCTBRIDGE_API void occt_destroy(OcctHandle handle);
    OCCTBRIDGE_API const char* occt_last_error(OcctHandle handle);
    OCCTBRIDGE_API const char* occt_version();
    OCCTBRIDGE_API int occt_bridge_abi_version();
    OCCTBRIDGE_API const char* occt_bridge_version();
    OCCTBRIDGE_API const char* occt_bridge_build_info();

    // Viewer and interaction.
    OCCTBRIDGE_API int occt_initialize(OcctHandle handle, void* windowHandle);
    OCCTBRIDGE_API int occt_resize(OcctHandle handle);
    OCCTBRIDGE_API int occt_redraw(OcctHandle handle);
    OCCTBRIDGE_API int occt_begin_update(OcctHandle handle);
    OCCTBRIDGE_API int occt_end_update(OcctHandle handle, int fitAll);
    OCCTBRIDGE_API int occt_is_updating(OcctHandle handle);
    OCCTBRIDGE_API int occt_fit_all(OcctHandle handle);
    OCCTBRIDGE_API int occt_fit_object(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_window_fit(OcctHandle handle, int x1, int y1, int x2, int y2);
    OCCTBRIDGE_API int occt_set_view(OcctHandle handle, int orientation);
    OCCTBRIDGE_API int occt_set_projection(OcctHandle handle, int projectionType);
    OCCTBRIDGE_API int occt_set_perspective_fov(OcctHandle handle, double degrees);
    OCCTBRIDGE_API int occt_set_background(OcctHandle handle, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_display_mode(OcctHandle handle, int displayMode);
    OCCTBRIDGE_API int occt_set_triedron_visible(OcctHandle handle, int visible);
    OCCTBRIDGE_API int occt_set_view_cube_visible(OcctHandle handle, int visible);
    OCCTBRIDGE_API int occt_set_computed_mode(OcctHandle handle, int enabled);
    OCCTBRIDGE_API int occt_dump_view(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_screen_to_world(OcctHandle handle, int x, int y, OcctPoint3d* result);
    OCCTBRIDGE_API int occt_world_to_screen(OcctHandle handle, OcctPoint3d point, int* x, int* y);
    OCCTBRIDGE_API int occt_move_to(OcctHandle handle, int x, int y);
    OCCTBRIDGE_API int occt_select(OcctHandle handle, int x, int y, int appendSelection);
    OCCTBRIDGE_API int occt_select_rectangle(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection);
    OCCTBRIDGE_API int occt_select_rectangle_ex(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection, int allowOverlap);
    OCCTBRIDGE_API int occt_select_object(OcctHandle handle, OcctObjectId objectId, int appendSelection);
    OCCTBRIDGE_API int occt_set_selection_mode(OcctHandle handle, int selectionMode);
    OCCTBRIDGE_API int occt_selected_count(OcctHandle handle);
    OCCTBRIDGE_API OcctObjectId occt_selected_at(OcctHandle handle, int index);
    OCCTBRIDGE_API OcctObjectId occt_first_selected(OcctHandle handle);
    OCCTBRIDGE_API int occt_clear_selection(OcctHandle handle);
    OCCTBRIDGE_API int occt_start_rotation(OcctHandle handle, int x, int y);
    OCCTBRIDGE_API int occt_rotation(OcctHandle handle, int x, int y);
    OCCTBRIDGE_API int occt_pan(OcctHandle handle, int deltaX, int deltaY);
    OCCTBRIDGE_API int occt_zoom(OcctHandle handle, double factor);
    OCCTBRIDGE_API int occt_get_camera(OcctHandle handle, OcctCameraState* result);
    OCCTBRIDGE_API int occt_set_camera(OcctHandle handle, const OcctCameraState* state);
    OCCTBRIDGE_API double occt_get_view_scale(OcctHandle handle);
    OCCTBRIDGE_API int occt_set_view_scale(OcctHandle handle, double scale);
    OCCTBRIDGE_API int occt_set_antialiasing(OcctHandle handle, int enabled);
    OCCTBRIDGE_API int occt_set_gradient_background(OcctHandle handle, double r1, double g1, double b1, double r2, double g2, double b2, int fillMethod);
    OCCTBRIDGE_API int occt_set_display_precision(OcctHandle handle, double deviationCoefficient, double deviationAngleDegrees, int applyExisting);
    OCCTBRIDGE_API int occt_set_default_material(OcctHandle handle, int material, int applyExisting);
    OCCTBRIDGE_API int occt_set_scene_lighting(OcctHandle handle, double ambientIntensity, double directionalIntensity, OcctVector3d direction, int headlight);
    OCCTBRIDGE_API int occt_set_scene_lighting_ex(OcctHandle handle, const OcctSceneLightingSettings* settings);
    OCCTBRIDGE_API int occt_set_selection_highlight_color(OcctHandle handle, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_hover_highlight_color(OcctHandle handle, double r, double g, double b);
    OCCTBRIDGE_API int occt_reset_scene_lighting(OcctHandle handle);
    OCCTBRIDGE_API int occt_set_selection_tolerance(OcctHandle handle, int pixelTolerance);
    OCCTBRIDGE_API int occt_set_auto_z_fit_mode(OcctHandle handle, int enabled, double scaleFactor);
    OCCTBRIDGE_API int occt_get_auto_z_fit_mode(OcctHandle handle, OcctAutoZFitSettings* result);
    OCCTBRIDGE_API int occt_auto_z_fit(OcctHandle handle);
    OCCTBRIDGE_API int occt_set_default_polygon_offsets(OcctHandle handle, int mode, double factor, double units, int applyExisting);
    OCCTBRIDGE_API int occt_get_default_polygon_offsets(OcctHandle handle, OcctPolygonOffsetSettings* result);
    OCCTBRIDGE_API int occt_set_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, int mode, double factor, double units);
    OCCTBRIDGE_API int occt_get_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, OcctPolygonOffsetSettings* result);
    OCCTBRIDGE_API int occt_reset_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId);

    OCCTBRIDGE_API int occt_fit_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, double margin);
    OCCTBRIDGE_API int occt_set_zup_view(OcctHandle handle, int orientation, int fitAll);
    OCCTBRIDGE_API int occt_screen_to_ray(OcctHandle handle, int x, int y, OcctProjectionRay* result);
    OCCTBRIDGE_API int occt_zoom_at_point(OcctHandle handle, int x, int y, double delta);
    OCCTBRIDGE_API int occt_select_all_visible(OcctHandle handle);
    OCCTBRIDGE_API int occt_invert_selection(OcctHandle handle);
    OCCTBRIDGE_API int occt_hide_selected(OcctHandle handle);
    OCCTBRIDGE_API int occt_set_automatic_highlight(OcctHandle handle, int enabled);
    OCCTBRIDGE_API int occt_set_msaa_samples(OcctHandle handle, int samples);
    OCCTBRIDGE_API int occt_set_render_resolution_scale(OcctHandle handle, double scale);
    OCCTBRIDGE_API int occt_set_render_resolution(OcctHandle handle, double dpi);
    OCCTBRIDGE_API int occt_set_rendering_method(OcctHandle handle, int method);
    OCCTBRIDGE_API int occt_set_shadows_enabled(OcctHandle handle, int enabled);
    OCCTBRIDGE_API int occt_set_immediate_update(OcctHandle handle, int enabled);
    OCCTBRIDGE_API int occt_set_frustum_culling(OcctHandle handle, int enabled);
    OCCTBRIDGE_API int occt_set_face_boundaries_visible(OcctHandle handle, int visible, int applyExisting);

    // Registry, AIS attributes and lifecycle.
    OCCTBRIDGE_API int occt_object_count(OcctHandle handle);
    OCCTBRIDGE_API OcctObjectId occt_object_id_at(OcctHandle handle, int index);
    OCCTBRIDGE_API OcctObjectId occt_shape_id_at(OcctHandle handle, int index);
    OCCTBRIDGE_API int occt_object_exists(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_object_kind(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_set_object_name(OcctHandle handle, OcctObjectId objectId, const char* utf8Name);
    OCCTBRIDGE_API const char* occt_get_object_name(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_set_object_color(OcctHandle handle, OcctObjectId objectId, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_object_transparency(OcctHandle handle, OcctObjectId objectId, double transparency);
    OCCTBRIDGE_API int occt_set_object_visible(OcctHandle handle, OcctObjectId objectId, int visible);
    OCCTBRIDGE_API int occt_set_object_display_mode(OcctHandle handle, OcctObjectId objectId, int displayMode);
    OCCTBRIDGE_API int occt_set_object_line_width(OcctHandle handle, OcctObjectId objectId, double width);
    OCCTBRIDGE_API int occt_set_object_material(OcctHandle handle, OcctObjectId objectId, int material);
    OCCTBRIDGE_API int occt_set_objects_color(OcctHandle handle, const OcctObjectId* objectIds, int count, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_objects_transparency(OcctHandle handle, const OcctObjectId* objectIds, int count, double transparency);
    OCCTBRIDGE_API int occt_set_objects_visible(OcctHandle handle, const OcctObjectId* objectIds, int count, int visible);
    OCCTBRIDGE_API int occt_set_objects_display_mode(OcctHandle handle, const OcctObjectId* objectIds, int count, int displayMode);
    OCCTBRIDGE_API int occt_set_objects_line_width(OcctHandle handle, const OcctObjectId* objectIds, int count, double width);
    OCCTBRIDGE_API int occt_set_objects_material(OcctHandle handle, const OcctObjectId* objectIds, int count, int material);
    OCCTBRIDGE_API int occt_redisplay_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
    OCCTBRIDGE_API int occt_select_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, int appendSelection);
    OCCTBRIDGE_API int occt_object_is_visible(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_object_is_selected(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_delete_object(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_delete_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
    OCCTBRIDGE_API int occt_clear(OcctHandle handle);
    OCCTBRIDGE_API int occt_show_all(OcctHandle handle);
    OCCTBRIDGE_API int occt_hide_all(OcctHandle handle);
    OCCTBRIDGE_API int occt_redisplay_object(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_highlight_object(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_unhighlight_object(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API OcctObjectId occt_copy_selected_subshape(OcctHandle handle);
    OCCTBRIDGE_API OcctObjectId occt_copy_selected_subshape_at(OcctHandle handle, int index);

    // Shape query and analysis.
    OCCTBRIDGE_API int occt_shape_type(OcctHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_shape_is_valid(OcctHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_shape_bounds(OcctHandle handle, OcctObjectId shapeId, OcctBounds* result);
    OCCTBRIDGE_API int occt_shape_linear_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
    OCCTBRIDGE_API int occt_shape_surface_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
    OCCTBRIDGE_API int occt_shape_volume_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
    OCCTBRIDGE_API int occt_shape_distance(OcctHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
    OCCTBRIDGE_API int occt_topology_count(OcctHandle handle, OcctObjectId shapeId, int shapeType);
    OCCTBRIDGE_API OcctObjectId occt_get_subshape(OcctHandle handle, OcctObjectId shapeId, int shapeType, int index);
    OCCTBRIDGE_API OcctObjectId occt_copy_shape(OcctHandle handle, OcctObjectId shapeId, int hideInput);
    OCCTBRIDGE_API std::int64_t occt_shape_hash(OcctHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_vertex_point(OcctHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
    OCCTBRIDGE_API int occt_edge_endpoints(OcctHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
    OCCTBRIDGE_API int occt_edge_point_at(OcctHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
    OCCTBRIDGE_API int occt_edge_curve_type(OcctHandle handle, OcctObjectId edgeId);
    OCCTBRIDGE_API int occt_face_surface_type(OcctHandle handle, OcctObjectId faceId);
    OCCTBRIDGE_API int occt_face_uv_bounds(OcctHandle handle, OcctObjectId faceId, OcctUvBounds* result);
    OCCTBRIDGE_API int occt_face_point_normal(OcctHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);

    // Shape transformations.
    OCCTBRIDGE_API OcctObjectId occt_translate(OcctHandle handle, OcctObjectId shapeId, OcctVector3d vector, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_rotate(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_scale(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_mirror_plane(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal, int hideInput);

    // Basic points, 2D/3D curves and planar elements.
    OCCTBRIDGE_API OcctObjectId occt_make_vertex(OcctHandle handle, OcctPoint3d point);
    OCCTBRIDGE_API OcctObjectId occt_make_line(OcctHandle handle, OcctPoint3d start, OcctPoint3d end);
    OCCTBRIDGE_API OcctObjectId occt_make_polyline(OcctHandle handle, const OcctPoint3d* points, int count, int closed);
    OCCTBRIDGE_API OcctObjectId occt_make_circle(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
    OCCTBRIDGE_API OcctObjectId occt_make_arc_three_points(OcctHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
    OCCTBRIDGE_API OcctObjectId occt_make_arc_center(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
    OCCTBRIDGE_API OcctObjectId occt_make_regular_polygon(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
    OCCTBRIDGE_API OcctObjectId occt_make_ellipse(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
    OCCTBRIDGE_API OcctObjectId occt_make_bezier(OcctHandle handle, const OcctPoint3d* poles, int count);
    OCCTBRIDGE_API OcctObjectId occt_make_bspline_interpolated(OcctHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
    OCCTBRIDGE_API OcctObjectId occt_make_rectangle_wire(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    OCCTBRIDGE_API OcctObjectId occt_make_face_from_wire(OcctHandle handle, OcctObjectId wireId, int onlyPlane);
    OCCTBRIDGE_API OcctObjectId occt_make_plane_face(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);

    // Primitive solids.
    OCCTBRIDGE_API OcctObjectId occt_make_box(OcctHandle handle, double x, double y, double z, double dx, double dy, double dz);
    OCCTBRIDGE_API OcctObjectId occt_make_cylinder(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
    OCCTBRIDGE_API OcctObjectId occt_make_sphere(OcctHandle handle, OcctPoint3d center, double radius);
    OCCTBRIDGE_API OcctObjectId occt_make_cone(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
    OCCTBRIDGE_API OcctObjectId occt_make_torus(OcctHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
    OCCTBRIDGE_API OcctObjectId occt_make_wedge(OcctHandle handle, double dx, double dy, double dz, double ltx);

    // Topology assembly.
    OCCTBRIDGE_API OcctObjectId occt_make_compound(OcctHandle handle, const OcctObjectId* shapeIds, int count, int hideInputs);
    OCCTBRIDGE_API OcctObjectId occt_make_wire(OcctHandle handle, const OcctObjectId* edgeIds, int count, int hideInputs);
    OCCTBRIDGE_API OcctObjectId occt_sew_shapes(OcctHandle handle, const OcctObjectId* shapeIds, int count, double tolerance, int hideInputs);
    OCCTBRIDGE_API OcctObjectId occt_make_solid_from_shell(OcctHandle handle, OcctObjectId shellId, int hideInput);

    // Boolean and feature operations.
    OCCTBRIDGE_API OcctObjectId occt_boolean(OcctHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, int hideInputs);
    OCCTBRIDGE_API OcctObjectId occt_extrude(OcctHandle handle, OcctObjectId profileId, OcctVector3d vector, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_revolve(OcctHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_sweep(OcctHandle handle, OcctObjectId spineWireId, OcctObjectId profileId, int hideInputs);
    OCCTBRIDGE_API OcctObjectId occt_loft(OcctHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance, int hideInputs);
    OCCTBRIDGE_API OcctObjectId occt_fillet_all_edges(OcctHandle handle, OcctObjectId shapeId, double radius, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_chamfer_all_edges(OcctHandle handle, OcctObjectId shapeId, double distance, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_fillet_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_chamfer_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_offset_shape(OcctHandle handle, OcctObjectId shapeId, double offset, double tolerance, int hideInput);
    OCCTBRIDGE_API OcctObjectId occt_thick_solid(OcctHandle handle, OcctObjectId solidId, int faceIndexToRemove, double thickness, double tolerance, int hideInput);

    // Text and dimensional annotations.
    OCCTBRIDGE_API OcctObjectId occt_make_text_shape(OcctHandle handle, const char* utf8Text, OcctPoint3d position, OcctVector3d normal, OcctVector3d xDirection, double height, double extrusionDepth, const char* utf8FontName, int bold, int italic);
    OCCTBRIDGE_API OcctObjectId occt_make_length_annotation_shape(OcctHandle handle, OcctObjectId edgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
    OCCTBRIDGE_API OcctObjectId occt_make_angle_annotation_shape(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double radius, double textHeight, double arrowSize, const char* utf8FontName);
    OCCTBRIDGE_API OcctObjectId occt_make_radius_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
    OCCTBRIDGE_API OcctObjectId occt_make_diameter_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
    OCCTBRIDGE_API OcctObjectId occt_add_text(OcctHandle handle, const char* utf8Text, OcctPoint3d position, double height, double r, double g, double b, int zoomable);
    OCCTBRIDGE_API int occt_set_text(OcctHandle handle, OcctObjectId textId, const char* utf8Text);
    OCCTBRIDGE_API int occt_set_text_position(OcctHandle handle, OcctObjectId textId, OcctPoint3d position);
    OCCTBRIDGE_API int occt_set_text_height(OcctHandle handle, OcctObjectId textId, double height);
    OCCTBRIDGE_API int occt_set_text_font(OcctHandle handle, OcctObjectId textId, const char* utf8FontName);
    OCCTBRIDGE_API int occt_set_text_angle(OcctHandle handle, OcctObjectId textId, double angleDegrees);
    OCCTBRIDGE_API int occt_set_text_zoomable(OcctHandle handle, OcctObjectId textId, int zoomable);
    OCCTBRIDGE_API int occt_set_dimension_flyout(OcctHandle handle, OcctObjectId dimensionId, double flyout);
    OCCTBRIDGE_API OcctObjectId occt_add_length_dimension(OcctHandle handle, OcctObjectId edgeId, double flyout, double r, double g, double b);
    OCCTBRIDGE_API OcctObjectId occt_add_angle_dimension(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double flyout, double r, double g, double b);
    OCCTBRIDGE_API OcctObjectId occt_add_radius_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);
    OCCTBRIDGE_API OcctObjectId occt_add_diameter_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);

    // BREP / STEP / IGES / STL IO.
    OCCTBRIDGE_API OcctObjectId occt_import_file(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_import_step(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_import_iges(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_import_brep(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API OcctObjectId occt_import_stl(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_export_step(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
    OCCTBRIDGE_API int occt_export_all_step(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_export_iges(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
    OCCTBRIDGE_API int occt_export_all_iges(OcctHandle handle, const char* utf8Path);
    OCCTBRIDGE_API int occt_export_brep(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
    OCCTBRIDGE_API int occt_export_stl(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);

    // Compatibility aliases retained for v1-v4 callers.
    OCCTBRIDGE_API int occt_set_shape_color(OcctHandle handle, OcctObjectId shapeId, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_shape_transparency(OcctHandle handle, OcctObjectId shapeId, double transparency);
    OCCTBRIDGE_API int occt_set_shape_visible(OcctHandle handle, OcctObjectId shapeId, int visible);
    OCCTBRIDGE_API int occt_delete_shape(OcctHandle handle, OcctObjectId shapeId);
    OCCTBRIDGE_API int occt_shape_count(OcctHandle handle);
}
