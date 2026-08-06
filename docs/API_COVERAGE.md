# OCCT Bridge API Inventory

This source-derived inventory lists the current native C ABI, C# P/Invoke mapping, and public .NET types.

OCAF/XDE is intentionally excluded; documents, undo/redo, and JSON persistence are application-layer responsibilities.

- OCCT: `7.9.0`
- Native exports: `297`
- Managed P/Invoke declarations: `297`
- Public .NET types: `59`

## Native C ABI

### OcctNative.h (7)

- `occt_create`
- `occt_destroy`
- `occt_last_error`
- `occt_version`
- `occt_bridge_abi_version`
- `occt_bridge_version`
- `occt_bridge_build_info`

### OcctNative.h — Viewer and interaction (72)

- `occt_auto_z_fit`
- `occt_begin_update`
- `occt_clear_selection`
- `occt_dump_view`
- `occt_end_update`
- `occt_first_selected`
- `occt_fit_all`
- `occt_fit_object`
- `occt_get_auto_z_fit_mode`
- `occt_get_camera`
- `occt_get_default_polygon_offsets`
- `occt_get_object_polygon_offsets`
- `occt_get_view_scale`
- `occt_initialize`
- `occt_is_updating`
- `occt_move_to`
- `occt_pan`
- `occt_redraw`
- `occt_reset_object_polygon_offsets`
- `occt_reset_scene_lighting`
- `occt_resize`
- `occt_rotation`
- `occt_screen_to_world`
- `occt_select`
- `occt_select_object`
- `occt_select_rectangle`
- `occt_select_rectangle_ex`
- `occt_selected_at`
- `occt_selected_count`
- `occt_set_antialiasing`
- `occt_set_auto_z_fit_mode`
- `occt_set_background`
- `occt_set_camera`
- `occt_set_computed_mode`
- `occt_set_default_material`
- `occt_set_default_polygon_offsets`
- `occt_set_display_mode`
- `occt_set_display_precision`
- `occt_set_gradient_background`
- `occt_set_hover_highlight_color`
- `occt_set_object_polygon_offsets`
- `occt_set_perspective_fov`
- `occt_set_projection`
- `occt_set_scene_lighting`
- `occt_set_scene_lighting_ex`
- `occt_set_selection_highlight_color`
- `occt_set_selection_mode`
- `occt_set_selection_tolerance`
- `occt_set_triedron_visible`
- `occt_set_view`
- `occt_set_view_cube_visible`
- `occt_set_view_scale`
- `occt_start_rotation`
- `occt_window_fit`
- `occt_world_to_screen`
- `occt_fit_objects`
- `occt_set_zup_view`
- `occt_screen_to_ray`
- `occt_zoom_at_point`
- `occt_select_all_visible`
- `occt_invert_selection`
- `occt_hide_selected`
- `occt_set_automatic_highlight`
- `occt_set_msaa_samples`
- `occt_set_render_resolution_scale`
- `occt_set_render_resolution`
- `occt_set_rendering_method`
- `occt_set_shadows_enabled`
- `occt_set_immediate_update`
- `occt_set_frustum_culling`
- `occt_set_face_boundaries_visible`
- `occt_zoom`

### OcctNative.h — Registry, AIS attributes and lifecycle (23)

- `occt_clear`
- `occt_copy_selected_subshape`
- `occt_copy_selected_subshape_at`
- `occt_delete_object`
- `occt_delete_objects`
- `occt_get_object_name`
- `occt_hide_all`
- `occt_highlight_object`
- `occt_object_count`
- `occt_object_exists`
- `occt_object_id_at`
- `occt_object_kind`
- `occt_redisplay_object`
- `occt_set_object_color`
- `occt_set_object_display_mode`
- `occt_set_object_line_width`
- `occt_set_object_material`
- `occt_set_object_name`
- `occt_set_object_transparency`
- `occt_set_object_visible`
- `occt_shape_id_at`
- `occt_show_all`
- `occt_unhighlight_object`

### OcctNative.h — Shape query and analysis (18)

- `occt_copy_shape`
- `occt_edge_curve_type`
- `occt_edge_endpoints`
- `occt_edge_point_at`
- `occt_face_point_normal`
- `occt_face_surface_type`
- `occt_face_uv_bounds`
- `occt_get_subshape`
- `occt_shape_bounds`
- `occt_shape_distance`
- `occt_shape_hash`
- `occt_shape_is_valid`
- `occt_shape_linear_properties`
- `occt_shape_surface_properties`
- `occt_shape_type`
- `occt_shape_volume_properties`
- `occt_topology_count`
- `occt_vertex_point`

### OcctNative.h — Shape transformations (4)

- `occt_mirror_plane`
- `occt_rotate`
- `occt_scale`
- `occt_translate`

### OcctNative.h — Basic points, 2D/3D curves and planar elements (13)

- `occt_make_arc_center`
- `occt_make_arc_three_points`
- `occt_make_bezier`
- `occt_make_bspline_interpolated`
- `occt_make_circle`
- `occt_make_ellipse`
- `occt_make_face_from_wire`
- `occt_make_line`
- `occt_make_plane_face`
- `occt_make_polyline`
- `occt_make_rectangle_wire`
- `occt_make_regular_polygon`
- `occt_make_vertex`

### OcctNative.h — Primitive solids (6)

- `occt_make_box`
- `occt_make_cone`
- `occt_make_cylinder`
- `occt_make_sphere`
- `occt_make_torus`
- `occt_make_wedge`

### OcctNative.h — Topology assembly (4)

- `occt_make_compound`
- `occt_make_solid_from_shell`
- `occt_make_wire`
- `occt_sew_shapes`

### OcctNative.h — Boolean and feature operations (11)

- `occt_boolean`
- `occt_chamfer_all_edges`
- `occt_chamfer_edges`
- `occt_extrude`
- `occt_fillet_all_edges`
- `occt_fillet_edges`
- `occt_loft`
- `occt_offset_shape`
- `occt_revolve`
- `occt_sweep`
- `occt_thick_solid`

### OcctNative.h — Text and dimensional annotations (17)

- `occt_add_angle_dimension`
- `occt_add_diameter_dimension`
- `occt_add_length_dimension`
- `occt_add_radius_dimension`
- `occt_add_text`
- `occt_make_angle_annotation_shape`
- `occt_make_diameter_annotation_shape`
- `occt_make_length_annotation_shape`
- `occt_make_radius_annotation_shape`
- `occt_make_text_shape`
- `occt_set_dimension_flyout`
- `occt_set_text`
- `occt_set_text_angle`
- `occt_set_text_font`
- `occt_set_text_height`
- `occt_set_text_position`
- `occt_set_text_zoomable`

### OcctNative.h — BREP / STEP / IGES / STL IO (11)

- `occt_export_all_iges`
- `occt_export_all_step`
- `occt_export_brep`
- `occt_export_iges`
- `occt_export_step`
- `occt_export_stl`
- `occt_import_brep`
- `occt_import_file`
- `occt_import_iges`
- `occt_import_step`
- `occt_import_stl`

### OcctNative.h — Compatibility aliases retained for v1-v4 callers (5)

- `occt_delete_shape`
- `occt_set_shape_color`
- `occt_set_shape_transparency`
- `occt_set_shape_visible`
- `occt_shape_count`

### OcctSelectionOverlay.h — Coordinates use the host window client coordinate system (origin at left/top) (2)

- `occt_hide_selection_rectangle`
- `occt_show_selection_rectangle`

### OcctModeling.h (104)

- `occt_model_ancestor_at`
- `occt_model_ancestor_count`
- `occt_model_boolean`
- `occt_model_capabilities`
- `occt_model_chamfer_edges`
- `occt_model_check_report`
- `occt_model_classify_point`
- `occt_model_clear`
- `occt_model_clear_mesh`
- `occt_model_copy_shape`
- `occt_model_create`
- `occt_model_delete_shape`
- `occt_model_destroy`
- `occt_model_display_in_engine`
- `occt_model_edge_curve_type`
- `occt_model_edge_endpoints`
- `occt_model_edge_point_at`
- `occt_model_export_brep`
- `occt_model_export_iges`
- `occt_model_export_step`
- `occt_model_export_stl`
- `occt_model_extrude`
- `occt_model_face_mesh_counts`
- `occt_model_face_mesh_node`
- `occt_model_face_mesh_triangle`
- `occt_model_face_point_normal`
- `occt_model_face_surface_type`
- `occt_model_face_uv_bounds`
- `occt_model_fillet_edges`
- `occt_model_fix_shape`
- `occt_model_get_location`
- `occt_model_get_subshape`
- `occt_model_history_generated_at`
- `occt_model_history_generated_count`
- `occt_model_history_is_removed`
- `occt_model_history_modified_at`
- `occt_model_history_modified_count`
- `occt_model_import_brep`
- `occt_model_import_file`
- `occt_model_import_iges`
- `occt_model_import_step`
- `occt_model_import_stl`
- `occt_model_inner_wire_at`
- `occt_model_inner_wire_count`
- `occt_model_last_error`
- `occt_model_loft`
- `occt_model_make_arc_center`
- `occt_model_make_arc_three_points`
- `occt_model_make_bezier`
- `occt_model_make_box`
- `occt_model_make_bspline_interpolated`
- `occt_model_make_circle`
- `occt_model_make_compound`
- `occt_model_make_cone`
- `occt_model_make_cylinder`
- `occt_model_make_ellipse`
- `occt_model_make_face_from_wire`
- `occt_model_make_line`
- `occt_model_make_plane_face`
- `occt_model_make_polyline`
- `occt_model_make_rectangle_wire`
- `occt_model_make_regular_polygon`
- `occt_model_make_solid_from_shell`
- `occt_model_make_sphere`
- `occt_model_make_torus`
- `occt_model_make_vertex`
- `occt_model_make_wedge`
- `occt_model_make_wire`
- `occt_model_mesh`
- `occt_model_mirror_plane`
- `occt_model_offset`
- `occt_model_operation_report`
- `occt_model_outer_wire`
- `occt_model_project_point_on_edge`
- `occt_model_project_point_on_face`
- `occt_model_ray_hit_at`
- `occt_model_ray_hit_count`
- `occt_model_ray_intersections`
- `occt_model_revolve`
- `occt_model_rotate`
- `occt_model_scale`
- `occt_model_set_location`
- `occt_model_sew`
- `occt_model_shape_bounds`
- `occt_model_shape_count`
- `occt_model_shape_distance`
- `occt_model_shape_exists`
- `occt_model_shape_hash`
- `occt_model_shape_id_at`
- `occt_model_shape_is_closed`
- `occt_model_shape_is_valid`
- `occt_model_shape_linear_properties`
- `occt_model_shape_orientation`
- `occt_model_shape_surface_properties`
- `occt_model_shape_tolerance`
- `occt_model_shape_type`
- `occt_model_shape_volume_properties`
- `occt_model_split`
- `occt_model_sweep`
- `occt_model_thick_solid`
- `occt_model_topology_count`
- `occt_model_translate`
- `occt_model_unify_same_domain`
- `occt_model_vertex_point`


## Native data types

- `OcctAutoZFitSettings`
- `OcctBooleanOperation`
- `OcctBounds`
- `OcctCameraState`
- `OcctColorRgb`
- `OcctCurveType`
- `OcctDisplayMode`
- `OcctDistanceResult`
- `OcctMassProperties`
- `OcctMaterial`
- `OcctModelAlgorithmResult`
- `OcctModelBooleanGlue`
- `OcctModelBooleanOperation`
- `OcctModelBooleanOptions`
- `OcctModelLocation`
- `OcctModelMeshNode`
- `OcctModelMeshParameters`
- `OcctModelMeshTriangle`
- `OcctModelOrientation`
- `OcctModelProjectionResult`
- `OcctModelRayHit`
- `OcctModelState`
- `OcctObjectKind`
- `OcctPoint3d`
- `OcctPolygonOffsetSettings`
- `OcctProjectionType`
- `OcctProjectionRay`
- `OcctRenderingMethod`
- `OcctSceneLightingSettings`
- `OcctSelectionMode`
- `OcctShapeType`
- `OcctSurfaceType`
- `OcctUvBounds`
- `OcctVector3d`
- `OcctViewOrientation`
- `OcctZUpViewOrientation`

## Public .NET types

- `OcctBridgeInfo`
- `IOcctObject`
- `OcctAutoZFitSettings`
- `OcctBooleanOperation`
- `OcctBounds`
- `OcctCameraState`
- `OcctCurveType`
- `OcctDimension`
- `OcctDirectionalLightSettings`
- `OcctDisplayBatch`
- `OcctDisplayMode`
- `OcctDistanceResult`
- `OcctEdgeEvaluation`
- `OcctEngine`
- `OcctException`
- `OcctFaceEvaluation`
- `OcctFaceMesh`
- `OcctGradientFillMethod`
- `OcctLightingPreset`
- `OcctLightingPresets`
- `OcctMassProperties`
- `OcctMaterial`
- `OcctModelAlgorithmResult`
- `OcctModelBooleanGlue`
- `OcctModelBooleanOptions`
- `OcctModelLocation`
- `OcctModelMeshNode`
- `OcctModelMeshParameters`
- `OcctModelMeshTriangle`
- `OcctModelOrientation`
- `OcctModelProjectionResult`
- `OcctModelRayHit`
- `OcctModelShape`
- `OcctModelState`
- `OcctModelingSession`
- `OcctObject`
- `OcctObjectKind`
- `OcctPoint3d`
- `OcctPolygonOffsetMode`
- `OcctPolygonOffsetSettings`
- `OcctProjectionType`
- `OcctProjectionRay`
- `OcctRenderingMethod`
- `OcctRectangleSelectionBehavior`
- `OcctRuntime`
- `OcctSceneLightingSettings`
- `OcctSelectionMode`
- `OcctShape`
- `OcctShapeType`
- `OcctSurfaceType`
- `OcctText`
- `OcctUvBounds`
- `OcctVector3d`
- `OcctViewOrientation`
- `OcctZUpViewOrientation`
- `OcctViewportControl`
- `OcctViewportErrorEventArgs`
- `OcctViewportSelectionEventArgs`
- `OcctViewportWorldPointEventArgs`

The `OcctViewport*` types above are provided by the optional `OcctNet.WinForms` assembly; all other managed types remain in the UI-independent `OcctNet` assembly.

## Bridge ABI contract

- Managed expected ABI: `2`
- Native bridge version: `2.1.0`
- `OcctBridgeInfo` validates the loaded `OcctNative.dll` before creating viewer or modeling sessions.
- Managed and native binaries should always be deployed from the same build.

## Consistency rule

`tests/check-api-surface.ps1` verifies that every native declaration has both a C++ definition and a C# P/Invoke declaration.
