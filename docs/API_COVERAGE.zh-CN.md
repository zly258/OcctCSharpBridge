# OCCT 封装接口详细清单

本文件由源码接口声明整理，列出当前原生 C ABI、C# P/Invoke 映射及公开 .NET 类型。

- OCCT: `7.9.0`
- Native exports: `509`
- Managed P/Invoke declarations: `509`
- Public .NET types: `62`

## 原生 C ABI

### OcctNative.h (4)

- `occt_create`
- `occt_destroy`
- `occt_last_error`
- `occt_version`

### OcctNative.h — Viewer and interaction (56)

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

### OcctNative.h — Text and dimensional annotations (12)

- `occt_add_angle_dimension`
- `occt_add_diameter_dimension`
- `occt_add_length_dimension`
- `occt_add_radius_dimension`
- `occt_add_text`
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

### OcctOcaf.h — Session, document, persistence, transactions and diagnostics (38)

- `occt_ocaf_abort_command`
- `occt_ocaf_available_redos`
- `occt_ocaf_available_undos`
- `occt_ocaf_capabilities`
- `occt_ocaf_change_storage_format`
- `occt_ocaf_clear_redos`
- `occt_ocaf_clear_undos`
- `occt_ocaf_close_document`
- `occt_ocaf_commit_command`
- `occt_ocaf_create`
- `occt_ocaf_destroy`
- `occt_ocaf_document_json`
- `occt_ocaf_document_path`
- `occt_ocaf_empty_labels_saving_mode`
- `occt_ocaf_get_undo_limit`
- `occt_ocaf_has_open_command`
- `occt_ocaf_is_changed`
- `occt_ocaf_is_empty`
- `occt_ocaf_is_open`
- `occt_ocaf_is_saved`
- `occt_ocaf_is_valid`
- `occt_ocaf_last_error`
- `occt_ocaf_modification_mode`
- `occt_ocaf_nested_transaction_mode`
- `occt_ocaf_new_command`
- `occt_ocaf_new_document`
- `occt_ocaf_open_command`
- `occt_ocaf_open_document`
- `occt_ocaf_redo`
- `occt_ocaf_save_as`
- `occt_ocaf_save_document`
- `occt_ocaf_set_empty_labels_saving_mode`
- `occt_ocaf_set_modification_mode`
- `occt_ocaf_set_nested_transaction_mode`
- `occt_ocaf_set_undo_limit`
- `occt_ocaf_storage_format`
- `occt_ocaf_undo`
- `occt_ocaf_version`

### OcctOcaf.h — Labels and generic attribute inspection (20)

- `occt_ocaf_attribute_guid_at`
- `occt_ocaf_attribute_json_at`
- `occt_ocaf_attribute_snapshot`
- `occt_ocaf_attribute_type_at`
- `occt_ocaf_child_at`
- `occt_ocaf_child_snapshot`
- `occt_ocaf_create_label`
- `occt_ocaf_father`
- `occt_ocaf_find_child`
- `occt_ocaf_forget_all_attributes`
- `occt_ocaf_forget_attribute`
- `occt_ocaf_label_depth`
- `occt_ocaf_label_exists`
- `occt_ocaf_label_is_imported`
- `occt_ocaf_label_is_root`
- `occt_ocaf_label_tag`
- `occt_ocaf_main_entry`
- `occt_ocaf_new_child`
- `occt_ocaf_root_entry`
- `occt_ocaf_set_label_imported`

### OcctOcaf.h — Standard scalar, reference, collection and geometric attributes (33)

- `occt_ocaf_array_count`
- `occt_ocaf_array_int_at`
- `occt_ocaf_array_lower`
- `occt_ocaf_array_real_at`
- `occt_ocaf_array_string_at`
- `occt_ocaf_get_ascii_string`
- `occt_ocaf_get_boolean_array`
- `occt_ocaf_get_byte_array`
- `occt_ocaf_get_comment`
- `occt_ocaf_get_integer`
- `occt_ocaf_get_integer_array`
- `occt_ocaf_get_name`
- `occt_ocaf_get_position`
- `occt_ocaf_get_real`
- `occt_ocaf_get_real_array`
- `occt_ocaf_get_reference`
- `occt_ocaf_get_shape_attribute`
- `occt_ocaf_get_string_array`
- `occt_ocaf_has_uattribute`
- `occt_ocaf_set_ascii_string`
- `occt_ocaf_set_boolean_array`
- `occt_ocaf_set_byte_array`
- `occt_ocaf_set_comment`
- `occt_ocaf_set_integer`
- `occt_ocaf_set_integer_array`
- `occt_ocaf_set_name`
- `occt_ocaf_set_position`
- `occt_ocaf_set_real`
- `occt_ocaf_set_real_array`
- `occt_ocaf_set_reference`
- `occt_ocaf_set_shape_attribute`
- `occt_ocaf_set_string_array`
- `occt_ocaf_set_uattribute`

### OcctOcaf.h — TNaming named-shape history and selection (17)

- `occt_ocaf_named_shape_evolution`
- `occt_ocaf_named_shape_exists`
- `occt_ocaf_named_shape_get`
- `occt_ocaf_named_shape_is_empty`
- `occt_ocaf_named_shape_new_at`
- `occt_ocaf_named_shape_old_at`
- `occt_ocaf_named_shape_pair_snapshot`
- `occt_ocaf_named_shape_version`
- `occt_ocaf_naming_delete`
- `occt_ocaf_naming_generated`
- `occt_ocaf_naming_generated_from`
- `occt_ocaf_naming_modify`
- `occt_ocaf_naming_select`
- `occt_ocaf_selector_is_identified`
- `occt_ocaf_selector_select`
- `occt_ocaf_selector_solve`
- `occt_ocaf_set_named_shape_version`

### OcctOcaf.h — XDE shapes and assemblies (33)

- `occt_ocaf_xde_add_component`
- `occt_ocaf_xde_add_shape`
- `occt_ocaf_xde_clipping_planes_entry`
- `occt_ocaf_xde_colors_entry`
- `occt_ocaf_xde_component_at`
- `occt_ocaf_xde_component_snapshot`
- `occt_ocaf_xde_dgts_entry`
- `occt_ocaf_xde_find_shape`
- `occt_ocaf_xde_get_length_unit`
- `occt_ocaf_xde_get_location`
- `occt_ocaf_xde_get_shape`
- `occt_ocaf_xde_is_assembly`
- `occt_ocaf_xde_is_component`
- `occt_ocaf_xde_is_free`
- `occt_ocaf_xde_is_reference`
- `occt_ocaf_xde_is_shape`
- `occt_ocaf_xde_is_simple_shape`
- `occt_ocaf_xde_is_subshape`
- `occt_ocaf_xde_layers_entry`
- `occt_ocaf_xde_materials_entry`
- `occt_ocaf_xde_notes_entry`
- `occt_ocaf_xde_referred_shape`
- `occt_ocaf_xde_remove_component`
- `occt_ocaf_xde_remove_shape`
- `occt_ocaf_xde_set_length_unit`
- `occt_ocaf_xde_set_location`
- `occt_ocaf_xde_set_shape`
- `occt_ocaf_xde_shape_at`
- `occt_ocaf_xde_shape_snapshot`
- `occt_ocaf_xde_shapes_entry`
- `occt_ocaf_xde_update_assemblies`
- `occt_ocaf_xde_views_entry`
- `occt_ocaf_xde_visual_materials_entry`

### OcctOcaf.h — XDE colors, layers, materials and validation properties (38)

- `occt_ocaf_xde_add_layer`
- `occt_ocaf_xde_color_at`
- `occt_ocaf_xde_color_snapshot`
- `occt_ocaf_xde_density_for_shape`
- `occt_ocaf_xde_get_area`
- `occt_ocaf_xde_get_centroid`
- `occt_ocaf_xde_get_color`
- `occt_ocaf_xde_get_color_definition`
- `occt_ocaf_xde_get_volume`
- `occt_ocaf_xde_is_color_by_layer`
- `occt_ocaf_xde_is_layer_visible`
- `occt_ocaf_xde_is_visible`
- `occt_ocaf_xde_layer_at`
- `occt_ocaf_xde_layer_name`
- `occt_ocaf_xde_layer_snapshot`
- `occt_ocaf_xde_material_at`
- `occt_ocaf_xde_material_density`
- `occt_ocaf_xde_material_density_name`
- `occt_ocaf_xde_material_density_value_type`
- `occt_ocaf_xde_material_description`
- `occt_ocaf_xde_material_for_shape`
- `occt_ocaf_xde_material_name`
- `occt_ocaf_xde_material_snapshot`
- `occt_ocaf_xde_remove_color`
- `occt_ocaf_xde_remove_layer`
- `occt_ocaf_xde_set_area`
- `occt_ocaf_xde_set_centroid`
- `occt_ocaf_xde_set_color`
- `occt_ocaf_xde_set_color_by_layer`
- `occt_ocaf_xde_set_layer`
- `occt_ocaf_xde_set_layer_visibility`
- `occt_ocaf_xde_set_material`
- `occt_ocaf_xde_set_visibility`
- `occt_ocaf_xde_set_volume`
- `occt_ocaf_xde_shape_layer_snapshot`
- `occt_ocaf_xde_unset_color`
- `occt_ocaf_xde_unset_layer`
- `occt_ocaf_xde_unset_layers`

### OcctOcaf.h — Metadata-preserving XDE exchange (4)

- `occt_ocaf_export_iges`
- `occt_ocaf_export_step`
- `occt_ocaf_import_iges`
- `occt_ocaf_import_step`

### OcctOcafExtended.h — Advanced TDocStd document state and delta management (9)

- `occt_ocaf_init_delta_compaction`
- `occt_ocaf_mark_modified`
- `occt_ocaf_modified_at`
- `occt_ocaf_modified_snapshot`
- `occt_ocaf_perform_delta_compaction`
- `occt_ocaf_purge_modified`
- `occt_ocaf_remove_first_undo`
- `occt_ocaf_set_storage_format_version`
- `occt_ocaf_storage_format_version`

### OcctOcafExtended.h — Extended TDF label information (6)

- `occt_ocaf_label_attribute_count`
- `occt_ocaf_label_attributes_modified`
- `occt_ocaf_label_child_count`
- `occt_ocaf_label_is_descendant`
- `occt_ocaf_label_may_be_modified`
- `occt_ocaf_label_transaction`

### OcctOcafExtended.h — TDataStd variables, expressions and relations (10)

- `occt_ocaf_assign_variable_expression`
- `occt_ocaf_desassign_variable`
- `occt_ocaf_expression_variable_at`
- `occt_ocaf_expression_variable_snapshot`
- `occt_ocaf_get_expression`
- `occt_ocaf_get_relation`
- `occt_ocaf_get_variable`
- `occt_ocaf_set_expression`
- `occt_ocaf_set_relation`
- `occt_ocaf_set_variable`

### OcctOcafExtended.h — Extended XDE shape and assembly queries (11)

- `occt_ocaf_xde_add_subshape`
- `occt_ocaf_xde_component_count`
- `occt_ocaf_xde_find_subshape`
- `occt_ocaf_xde_is_compound`
- `occt_ocaf_xde_is_top_level`
- `occt_ocaf_xde_new_shape`
- `occt_ocaf_xde_search_shape`
- `occt_ocaf_xde_subshape_at`
- `occt_ocaf_xde_subshape_snapshot`
- `occt_ocaf_xde_user_at`
- `occt_ocaf_xde_user_snapshot`

### OcctOcafExtended.h — Extended XDE color workflows, including instance colors (9)

- `occt_ocaf_xde_add_color`
- `occt_ocaf_xde_color_is_set`
- `occt_ocaf_xde_color_label`
- `occt_ocaf_xde_find_color`
- `occt_ocaf_xde_get_instance_color`
- `occt_ocaf_xde_is_color`
- `occt_ocaf_xde_is_instance_visible`
- `occt_ocaf_xde_set_color_label`
- `occt_ocaf_xde_set_instance_color`

### OcctOcafExtended.h — Extended XDE layer and material workflows (8)

- `occt_ocaf_xde_add_material`
- `occt_ocaf_xde_assign_material`
- `occt_ocaf_xde_find_layer`
- `occt_ocaf_xde_is_layer`
- `occt_ocaf_xde_is_material`
- `occt_ocaf_xde_layer_is_set`
- `occt_ocaf_xde_layer_shape_at`
- `occt_ocaf_xde_layer_shape_snapshot`

## 原生数据类型

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
- `OcctOcafColor`
- `OcctOcafColorType`
- `OcctOcafNamedShapeEvolution`
- `OcctPoint3d`
- `OcctPolygonOffsetSettings`
- `OcctProjectionType`
- `OcctSceneLightingSettings`
- `OcctSelectionMode`
- `OcctShapeType`
- `OcctSurfaceType`
- `OcctUvBounds`
- `OcctVector3d`
- `OcctViewOrientation`

## 公开 .NET 类型

- `IOcctObject`
- `OcafColor`
- `OcafColorType`
- `OcafCommandScope`
- `OcafDocument`
- `OcafDocumentFormats`
- `OcafLabel`
- `OcafNamedShapeEvolution`
- `OcafStorageFormatVersion`
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
- `OcctViewportControl`
- `OcctViewportSelectionEventArgs`
- `OcctViewportWorldPointEventArgs`

## 一致性规则

`tests/check-api-surface.ps1` 校验每个原生声明均存在 C++ 定义和 C# P/Invoke 声明。
