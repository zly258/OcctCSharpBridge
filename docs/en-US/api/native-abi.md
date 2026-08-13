# OcctNative Complete C ABI Reference

- **Bridge:** `2.7.0`
- **Native ABI:** `4`
- **Exports:** `419`

## `OcctNative.h`

### `occt_create`

- **Returns:** `OcctHandle`

```cpp
OCCTBRIDGE_API OcctHandle occt_create();
```

### `occt_destroy`

- **Returns:** `void`

```cpp
OCCTBRIDGE_API void occt_destroy(OcctHandle handle);
```

### `occt_last_error`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_last_error(OcctHandle handle);
```

### `occt_version`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_version();
```

### `occt_bridge_abi_version`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_bridge_abi_version();
```

### `occt_bridge_version`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_bridge_version();
```

### `occt_bridge_build_info`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_bridge_build_info();
```

### `occt_initialize`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_initialize(OcctHandle handle, void* windowHandle);
```

### `occt_resize`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_resize(OcctHandle handle);
```

### `occt_redraw`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_redraw(OcctHandle handle);
```

### `occt_begin_update`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_begin_update(OcctHandle handle);
```

### `occt_end_update`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_end_update(OcctHandle handle, int fitAll);
```

### `occt_is_updating`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_is_updating(OcctHandle handle);
```

### `occt_fit_all`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_all(OcctHandle handle);
```

### `occt_fit_object`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_object(OcctHandle handle, OcctObjectId objectId);
```

### `occt_window_fit`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_window_fit(OcctHandle handle, int x1, int y1, int x2, int y2);
```

### `occt_set_view`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view(OcctHandle handle, int orientation);
```

### `occt_set_projection`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_projection(OcctHandle handle, int projectionType);
```

### `occt_set_perspective_fov`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_perspective_fov(OcctHandle handle, double degrees);
```

### `occt_set_background`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_background(OcctHandle handle, double r, double g, double b);
```

### `occt_set_display_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_display_mode(OcctHandle handle, int displayMode);
```

### `occt_set_triedron_visible`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_triedron_visible(OcctHandle handle, int visible);
```

### `occt_set_view_cube_visible`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_cube_visible(OcctHandle handle, int visible);
```

### `occt_set_computed_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_computed_mode(OcctHandle handle, int enabled);
```

### `occt_dump_view`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_dump_view(OcctHandle handle, const char* utf8Path);
```

### `occt_screen_to_world`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_screen_to_world(OcctHandle handle, int x, int y, OcctPoint3d* result);
```

### `occt_world_to_screen`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_world_to_screen(OcctHandle handle, OcctPoint3d point, int* x, int* y);
```

### `occt_move_to`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_move_to(OcctHandle handle, int x, int y);
```

### `occt_select`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select(OcctHandle handle, int x, int y, int appendSelection);
```

### `occt_select_rectangle_ex`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_rectangle_ex(OcctHandle handle, int x1, int y1, int x2, int y2, int appendSelection, int allowOverlap);
```

### `occt_select_object`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_object(OcctHandle handle, OcctObjectId objectId, int appendSelection);
```

### `occt_set_selection_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_mode(OcctHandle handle, int selectionMode);
```

### `occt_clear_selection`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_clear_selection(OcctHandle handle);
```

### `occt_start_rotation`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_start_rotation(OcctHandle handle, int x, int y);
```

### `occt_rotation`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_rotation(OcctHandle handle, int x, int y);
```

### `occt_pan`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_pan(OcctHandle handle, int deltaX, int deltaY);
```

### `occt_zoom`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_zoom(OcctHandle handle, double factor);
```

### `occt_get_camera`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_camera(OcctHandle handle, OcctCameraState* result);
```

### `occt_set_camera`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_camera(OcctHandle handle, const OcctCameraState* state);
```

### `occt_get_view_scale`

- **Returns:** `double`

```cpp
OCCTBRIDGE_API double occt_get_view_scale(OcctHandle handle);
```

### `occt_set_view_scale`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_scale(OcctHandle handle, double scale);
```

### `occt_set_antialiasing`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_antialiasing(OcctHandle handle, int enabled);
```

### `occt_set_gradient_background`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_gradient_background(OcctHandle handle, double r1, double g1, double b1, double r2, double g2, double b2, int fillMethod);
```

### `occt_set_display_precision`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_display_precision(OcctHandle handle, double deviationCoefficient, double deviationAngleDegrees, int applyExisting);
```

### `occt_set_default_material`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_default_material(OcctHandle handle, int material, int applyExisting);
```

### `occt_set_scene_lighting_ex`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_scene_lighting_ex(OcctHandle handle, const OcctSceneLightingSettings* settings);
```

### `occt_set_selection_highlight_color`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_highlight_color(OcctHandle handle, double r, double g, double b);
```

### `occt_set_hover_highlight_color`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_hover_highlight_color(OcctHandle handle, double r, double g, double b);
```

### `occt_reset_scene_lighting`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_scene_lighting(OcctHandle handle);
```

### `occt_set_selection_tolerance`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selection_tolerance(OcctHandle handle, int pixelTolerance);
```

### `occt_set_auto_z_fit_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_auto_z_fit_mode(OcctHandle handle, int enabled, double scaleFactor);
```

### `occt_get_auto_z_fit_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_auto_z_fit_mode(OcctHandle handle, OcctAutoZFitSettings* result);
```

### `occt_auto_z_fit`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_auto_z_fit(OcctHandle handle);
```

### `occt_set_default_polygon_offsets`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_default_polygon_offsets(OcctHandle handle, int mode, double factor, double units, int applyExisting);
```

### `occt_get_default_polygon_offsets`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_default_polygon_offsets(OcctHandle handle, OcctPolygonOffsetSettings* result);
```

### `occt_set_object_polygon_offsets`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, int mode, double factor, double units);
```

### `occt_get_object_polygon_offsets`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, OcctPolygonOffsetSettings* result);
```

### `occt_reset_object_polygon_offsets`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId);
```

### `occt_fit_objects`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, double margin);
```

### `occt_set_zup_view`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_zup_view(OcctHandle handle, int orientation, int fitAll);
```

### `occt_screen_to_ray`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_screen_to_ray(OcctHandle handle, int x, int y, OcctProjectionRay* result);
```

### `occt_zoom_at_point`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_zoom_at_point(OcctHandle handle, int x, int y, double delta);
```

### `occt_select_all_visible`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_all_visible(OcctHandle handle);
```

### `occt_invert_selection`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_invert_selection(OcctHandle handle);
```

### `occt_hide_selected`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_selected(OcctHandle handle);
```

### `occt_set_automatic_highlight`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_automatic_highlight(OcctHandle handle, int enabled);
```

### `occt_set_msaa_samples`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_msaa_samples(OcctHandle handle, int samples);
```

### `occt_set_render_resolution_scale`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_render_resolution_scale(OcctHandle handle, double scale);
```

### `occt_set_render_resolution`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_render_resolution(OcctHandle handle, double dpi);
```

### `occt_set_rendering_method`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_rendering_method(OcctHandle handle, int method);
```

### `occt_set_shadows_enabled`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_shadows_enabled(OcctHandle handle, int enabled);
```

### `occt_set_immediate_update`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_immediate_update(OcctHandle handle, int enabled);
```

### `occt_set_frustum_culling`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_frustum_culling(OcctHandle handle, int enabled);
```

### `occt_set_face_boundaries_visible`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_face_boundaries_visible(OcctHandle handle, int visible, int applyExisting);
```

### `occt_get_viewport_state`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_viewport_state(OcctHandle handle, OcctViewportState* result);
```

### `occt_reset_view`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view(OcctHandle handle);
```

### `occt_reset_view_orientation`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view_orientation(OcctHandle handle);
```

### `occt_reset_view_mapping`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_view_mapping(OcctHandle handle);
```

### `occt_fit_selected`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_fit_selected(OcctHandle handle, double margin);
```

### `occt_get_scene_gravity_point`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_scene_gravity_point(OcctHandle handle, OcctPoint3d* result);
```

### `occt_object_count`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_count(OcctHandle handle);
```

### `occt_object_descriptors`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_descriptors(OcctHandle handle, OcctObjectDescriptor* items, int capacity, int* objectCount, int* shapeCount);
```

### `occt_object_exists`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_exists(OcctHandle handle, OcctObjectId objectId);
```

### `occt_object_kind`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_kind(OcctHandle handle, OcctObjectId objectId);
```

### `occt_set_object_name`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_name(OcctHandle handle, OcctObjectId objectId, const char* utf8Name);
```

### `occt_get_object_name`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_object_name(OcctHandle handle, OcctObjectId objectId);
```

### `occt_set_object_application_tag`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_application_tag(OcctHandle handle, OcctObjectId objectId, const char* utf8Tag);
```

### `occt_get_object_application_tag`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_object_application_tag(OcctHandle handle, OcctObjectId objectId);
```

### `occt_find_object_by_application_tag`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_find_object_by_application_tag(OcctHandle handle, const char* utf8Tag);
```

### `occt_set_object_selectable`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_selectable(OcctHandle handle, OcctObjectId objectId, int selectable);
```

### `occt_get_object_selectable`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_selectable(OcctHandle handle, OcctObjectId objectId);
```

### `occt_set_objects_selectable`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_selectable(OcctHandle handle, const OcctObjectId* objectIds, int count, int selectable);
```

### `occt_set_selected_objects_ex`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_selected_objects_ex(OcctHandle handle, const OcctObjectId* objectIds, int count, int operation);
```

### `occt_set_object_transform`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transform(OcctHandle handle, OcctObjectId objectId, const double* matrix3x4);
```

### `occt_get_object_transform`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_transform(OcctHandle handle, OcctObjectId objectId, double* matrix3x4, int* hasTransform);
```

### `occt_reset_object_transform`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_object_transform(OcctHandle handle, OcctObjectId objectId);
```

### `occt_set_view_cube_language`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_cube_language(OcctHandle handle, int language);
```

### `occt_set_object_color`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_color(OcctHandle handle, OcctObjectId objectId, double r, double g, double b);
```

### `occt_set_object_transparency`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transparency(OcctHandle handle, OcctObjectId objectId, double transparency);
```

### `occt_set_object_visible`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_visible(OcctHandle handle, OcctObjectId objectId, int visible);
```

### `occt_set_object_display_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_display_mode(OcctHandle handle, OcctObjectId objectId, int displayMode);
```

### `occt_set_object_line_width`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_line_width(OcctHandle handle, OcctObjectId objectId, double width);
```

### `occt_set_object_material`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_material(OcctHandle handle, OcctObjectId objectId, int material);
```

### `occt_set_objects_color`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_color(OcctHandle handle, const OcctObjectId* objectIds, int count, double r, double g, double b);
```

### `occt_set_objects_transparency`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_transparency(OcctHandle handle, const OcctObjectId* objectIds, int count, double transparency);
```

### `occt_set_objects_visible`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_visible(OcctHandle handle, const OcctObjectId* objectIds, int count, int visible);
```

### `occt_set_objects_display_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_display_mode(OcctHandle handle, const OcctObjectId* objectIds, int count, int displayMode);
```

### `occt_set_objects_line_width`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_line_width(OcctHandle handle, const OcctObjectId* objectIds, int count, double width);
```

### `occt_set_objects_material`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_material(OcctHandle handle, const OcctObjectId* objectIds, int count, int material);
```

### `occt_redisplay_objects`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_redisplay_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
```

### `occt_select_objects`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_select_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, int appendSelection);
```

### `occt_object_is_visible`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_is_visible(OcctHandle handle, OcctObjectId objectId);
```

### `occt_object_is_selected`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_object_is_selected(OcctHandle handle, OcctObjectId objectId);
```

### `occt_delete_objects`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_delete_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
```

### `occt_clear`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_clear(OcctHandle handle);
```

### `occt_show_all`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_show_all(OcctHandle handle);
```

### `occt_hide_all`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_all(OcctHandle handle);
```

### `occt_redisplay_object`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_redisplay_object(OcctHandle handle, OcctObjectId objectId);
```

### `occt_highlight_object`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_highlight_object(OcctHandle handle, OcctObjectId objectId);
```

### `occt_unhighlight_object`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_unhighlight_object(OcctHandle handle, OcctObjectId objectId);
```

### `occt_copy_selected_subshape_at`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_copy_selected_subshape_at(OcctHandle handle, int index);
```

### `occt_shape_type`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_type(OcctHandle handle, OcctObjectId shapeId);
```

### `occt_shape_is_valid`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_is_valid(OcctHandle handle, OcctObjectId shapeId);
```

### `occt_shape_bounds`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_bounds(OcctHandle handle, OcctObjectId shapeId, OcctBounds* result);
```

### `occt_shape_linear_properties`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_linear_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

### `occt_shape_surface_properties`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_surface_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

### `occt_shape_volume_properties`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_volume_properties(OcctHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

### `occt_shape_distance`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_shape_distance(OcctHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
```

### `occt_topology_count`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_topology_count(OcctHandle handle, OcctObjectId shapeId, int shapeType);
```

### `occt_get_subshape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_get_subshape(OcctHandle handle, OcctObjectId shapeId, int shapeType, int index);
```

### `occt_copy_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_copy_shape(OcctHandle handle, OcctObjectId shapeId, int hideInput);
```

### `occt_shape_hash`

- **Returns:** `std::int64_t`

```cpp
OCCTBRIDGE_API std::int64_t occt_shape_hash(OcctHandle handle, OcctObjectId shapeId);
```

### `occt_vertex_point`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_vertex_point(OcctHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
```

### `occt_edge_endpoints`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_endpoints(OcctHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
```

### `occt_edge_point_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_point_at(OcctHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
```

### `occt_edge_curve_type`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_edge_curve_type(OcctHandle handle, OcctObjectId edgeId);
```

### `occt_face_surface_type`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_face_surface_type(OcctHandle handle, OcctObjectId faceId);
```

### `occt_face_uv_bounds`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_face_uv_bounds(OcctHandle handle, OcctObjectId faceId, OcctUvBounds* result);
```

### `occt_face_point_normal`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_face_point_normal(OcctHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);
```

### `occt_translate`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_translate(OcctHandle handle, OcctObjectId shapeId, OcctVector3d vector, int hideInput);
```

### `occt_rotate`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_rotate(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
```

### `occt_scale`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_scale(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor, int hideInput);
```

### `occt_mirror_plane`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_mirror_plane(OcctHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal, int hideInput);
```

### `occt_make_vertex`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_vertex(OcctHandle handle, OcctPoint3d point);
```

### `occt_make_line`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_line(OcctHandle handle, OcctPoint3d start, OcctPoint3d end);
```

### `occt_make_polyline`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_polyline(OcctHandle handle, const OcctPoint3d* points, int count, int closed);
```

### `occt_make_circle`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_circle(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
```

### `occt_make_arc_three_points`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_arc_three_points(OcctHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
```

### `occt_make_arc_center`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_arc_center(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
```

### `occt_make_regular_polygon`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_regular_polygon(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
```

### `occt_make_ellipse`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_ellipse(OcctHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
```

### `occt_make_bezier`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_bezier(OcctHandle handle, const OcctPoint3d* poles, int count);
```

### `occt_make_bspline_interpolated`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_bspline_interpolated(OcctHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
```

### `occt_make_rectangle_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_rectangle_wire(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

### `occt_make_face_from_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_face_from_wire(OcctHandle handle, OcctObjectId wireId, int onlyPlane);
```

### `occt_make_plane_face`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_plane_face(OcctHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

### `occt_make_box`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_box(OcctHandle handle, double x, double y, double z, double dx, double dy, double dz);
```

### `occt_make_cylinder`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_cylinder(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
```

### `occt_make_sphere`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_sphere(OcctHandle handle, OcctPoint3d center, double radius);
```

### `occt_make_cone`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_cone(OcctHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
```

### `occt_make_torus`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_torus(OcctHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
```

### `occt_make_wedge`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_wedge(OcctHandle handle, double dx, double dy, double dz, double ltx);
```

### `occt_make_compound`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_compound(OcctHandle handle, const OcctObjectId* shapeIds, int count, int hideInputs);
```

### `occt_make_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_wire(OcctHandle handle, const OcctObjectId* edgeIds, int count, int hideInputs);
```

### `occt_sew_shapes`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_sew_shapes(OcctHandle handle, const OcctObjectId* shapeIds, int count, double tolerance, int hideInputs);
```

### `occt_make_solid_from_shell`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_solid_from_shell(OcctHandle handle, OcctObjectId shellId, int hideInput);
```

### `occt_boolean`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_boolean(OcctHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, int hideInputs);
```

### `occt_extrude`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_extrude(OcctHandle handle, OcctObjectId profileId, OcctVector3d vector, int hideInput);
```

### `occt_revolve`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_revolve(OcctHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, int hideInput);
```

### `occt_sweep`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_sweep(OcctHandle handle, OcctObjectId spineWireId, OcctObjectId profileId, int hideInputs);
```

### `occt_loft`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_loft(OcctHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance, int hideInputs);
```

### `occt_fillet_all_edges`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_fillet_all_edges(OcctHandle handle, OcctObjectId shapeId, double radius, int hideInput);
```

### `occt_chamfer_all_edges`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_chamfer_all_edges(OcctHandle handle, OcctObjectId shapeId, double distance, int hideInput);
```

### `occt_fillet_edges`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_fillet_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius, int hideInput);
```

### `occt_chamfer_edges`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_chamfer_edges(OcctHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance, int hideInput);
```

### `occt_offset_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_offset_shape(OcctHandle handle, OcctObjectId shapeId, double offset, double tolerance, int hideInput);
```

### `occt_thick_solid`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_thick_solid(OcctHandle handle, OcctObjectId solidId, int faceIndexToRemove, double thickness, double tolerance, int hideInput);
```

### `occt_make_text_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_text_shape(OcctHandle handle, const char* utf8Text, OcctPoint3d position, OcctVector3d normal, OcctVector3d xDirection, double height, double extrusionDepth, const char* utf8FontName, int bold, int italic);
```

### `occt_make_length_annotation_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_length_annotation_shape(OcctHandle handle, OcctObjectId edgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

### `occt_make_angle_annotation_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_angle_annotation_shape(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double radius, double textHeight, double arrowSize, const char* utf8FontName);
```

### `occt_make_radius_annotation_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_radius_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

### `occt_make_diameter_annotation_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_make_diameter_annotation_shape(OcctHandle handle, OcctObjectId circularEdgeId, double flyout, double textHeight, double arrowSize, const char* utf8FontName);
```

### `occt_add_text`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_text(OcctHandle handle, const char* utf8Text, OcctPoint3d position, double height, double r, double g, double b, int zoomable);
```

### `occt_set_text`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text(OcctHandle handle, OcctObjectId textId, const char* utf8Text);
```

### `occt_set_text_position`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_position(OcctHandle handle, OcctObjectId textId, OcctPoint3d position);
```

### `occt_set_text_height`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_height(OcctHandle handle, OcctObjectId textId, double height);
```

### `occt_set_text_font`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_font(OcctHandle handle, OcctObjectId textId, const char* utf8FontName);
```

### `occt_set_text_angle`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_angle(OcctHandle handle, OcctObjectId textId, double angleDegrees);
```

### `occt_set_text_zoomable`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_text_zoomable(OcctHandle handle, OcctObjectId textId, int zoomable);
```

### `occt_set_dimension_flyout`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_dimension_flyout(OcctHandle handle, OcctObjectId dimensionId, double flyout);
```

### `occt_add_length_dimension`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_length_dimension(OcctHandle handle, OcctObjectId edgeId, double flyout, double r, double g, double b);
```

### `occt_add_angle_dimension`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_angle_dimension(OcctHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double flyout, double r, double g, double b);
```

### `occt_add_radius_dimension`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_radius_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);
```

### `occt_add_diameter_dimension`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_diameter_dimension(OcctHandle handle, OcctObjectId circularShapeId, double flyout, double r, double g, double b);
```

### `occt_import_file`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_file(OcctHandle handle, const char* utf8Path);
```

### `occt_import_step`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_step(OcctHandle handle, const char* utf8Path);
```

### `occt_import_iges`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_iges(OcctHandle handle, const char* utf8Path);
```

### `occt_import_brep`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_brep(OcctHandle handle, const char* utf8Path);
```

### `occt_import_stl`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_import_stl(OcctHandle handle, const char* utf8Path);
```

### `occt_export_step`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_step(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

### `occt_export_all_step`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_all_step(OcctHandle handle, const char* utf8Path);
```

### `occt_export_iges`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_iges(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

### `occt_export_all_iges`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_all_iges(OcctHandle handle, const char* utf8Path);
```

### `occt_export_brep`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_brep(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

### `occt_export_stl`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_export_stl(OcctHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);
```

## `OcctRenderSurface.h`

### `occt_resize_surface`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_resize_surface(OcctHandle handle);
```

## `OcctStepDocument.h`

### `occt_get_last_step_document_json`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_get_last_step_document_json(OcctHandle handle);
```

## `OcctPoints.h`

### `occt_add_point`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_point( OcctHandle handle, OcctPoint3d position, int marker, double scale, double r, double g, double b);
```

### `occt_set_point_position`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_point_position( OcctHandle handle, OcctObjectId pointId, OcctPoint3d position);
```

### `occt_set_point_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_point_style( OcctHandle handle, OcctObjectId pointId, int marker, double scale, double r, double g, double b);
```

### `occt_add_point_pixmap`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_point_pixmap( OcctHandle handle, OcctPoint3d position, int width, int height, const unsigned char* pixels, int pixelCount, int pixelFormat);
```

### `occt_set_point_pixmap_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_point_pixmap_style( OcctHandle handle, OcctObjectId pointId, int width, int height, const unsigned char* pixels, int pixelCount, int pixelFormat);
```

## `OcctSelectionOverlay.h`

### `occt_show_selection_rectangle`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_show_selection_rectangle( OcctHandle handle, int x1, int y1, int x2, int y2, double lineR, double lineG, double lineB, double fillR, double fillG, double fillB, double fillTransparency, double lineWidth);
```

### `occt_hide_selection_rectangle`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_hide_selection_rectangle(OcctHandle handle);
```

## `OcctSelectionState.h`

### `occt_selected_hits`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_selected_hits( OcctHandle handle, OcctSelectionHit* items, int capacity, int* count);
```

### `occt_detected_hit`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_detected_hit( OcctHandle handle, OcctSelectionHit* result, int* hasHit);
```

### `occt_detected_hit_detail`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_detected_hit_detail( OcctHandle handle, OcctSelectionHitDetail* result, int* hasHit);
```

### `occt_detect_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_detect_at( OcctHandle handle, int x, int y, int maxHits, OcctSelectionHitDetail* items, int capacity, int* count);
```

## `OcctViewerInteraction.h`

### `occt_set_object_z_layer`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_z_layer( OcctHandle handle, OcctObjectId objectId, int layer);
```

### `occt_set_objects_z_layer`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_z_layer( OcctHandle handle, const OcctObjectId* objectIds, int count, int layer);
```

### `occt_get_object_z_layer`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_z_layer( OcctHandle handle, OcctObjectId objectId, int* layer);
```

### `occt_set_triedron_options`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_triedron_options( OcctHandle handle, const OcctTriedronOptions* options);
```

### `occt_set_view_cube_options`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_cube_options( OcctHandle handle, const OcctViewCubeOptions* options);
```

### `occt_set_face_boundary_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_face_boundary_style( OcctHandle handle, OcctObjectId shapeId, int visible, double r, double g, double b, double width);
```

### `occt_set_face_boundary_styles`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_face_boundary_styles( OcctHandle handle, const OcctObjectId* shapeIds, int count, int visible, double r, double g, double b, double width);
```

### `occt_set_default_face_boundary_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_default_face_boundary_style( OcctHandle handle, int visible, double r, double g, double b, double width, int applyExisting);
```

### `occt_indexed_vertex_point`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_indexed_vertex_point( OcctHandle handle, OcctObjectId ownerId, int vertexIndex, OcctPoint3d* result);
```

### `occt_indexed_edge_endpoints`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_indexed_edge_endpoints( OcctHandle handle, OcctObjectId ownerId, int edgeIndex, OcctPoint3d* start, OcctPoint3d* end);
```

### `occt_indexed_edge_point_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_indexed_edge_point_at( OcctHandle handle, OcctObjectId ownerId, int edgeIndex, double normalizedParameter, OcctPoint3d* resultPoint, OcctVector3d* resultTangent);
```

### `occt_indexed_face_point_normal`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_indexed_face_point_normal( OcctHandle handle, OcctObjectId ownerId, int faceIndex, double u, double v, OcctPoint3d* resultPoint, OcctVector3d* resultNormal);
```

### `occt_indexed_face_center`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_indexed_face_center( OcctHandle handle, OcctObjectId ownerId, int faceIndex, OcctPoint3d* result);
```

### `occt_set_object_transforms`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transforms( OcctHandle handle, const OcctObjectTransformUpdate* updates, int count);
```

## `OcctViewerInteractionExtensions.h`

### `occt_set_object_selection_mode_active`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_selection_mode_active( OcctHandle handle, OcctObjectId objectId, int mode, int active, int concurrency, int force);
```

### `occt_set_object_selection_sensitivity`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_selection_sensitivity( OcctHandle handle, OcctObjectId objectId, int mode, int sensitivity);
```

### `occt_set_object_display_priority`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_display_priority( OcctHandle handle, OcctObjectId objectId, int priority);
```

### `occt_set_objects_display_priority`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_objects_display_priority( OcctHandle handle, const OcctObjectId* objectIds, int count, int priority);
```

### `occt_get_object_display_priority`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_display_priority( OcctHandle handle, OcctObjectId objectId, int* priority);
```

### `occt_set_object_transform_persistence_3d`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transform_persistence_3d( OcctHandle handle, OcctObjectId objectId, int mode, OcctPoint3d anchor);
```

### `occt_set_object_transform_persistence_2d`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_transform_persistence_2d( OcctHandle handle, OcctObjectId objectId, int mode, int position, int offsetX, int offsetY);
```

### `occt_clear_object_transform_persistence`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_clear_object_transform_persistence( OcctHandle handle, OcctObjectId objectId);
```

### `occt_get_object_transform_persistence`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_transform_persistence( OcctHandle handle, OcctObjectId objectId, OcctTransformPersistenceState* result);
```

### `occt_set_view_clip_planes`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_view_clip_planes( OcctHandle handle, const OcctViewClipPlane* planes, int count);
```

### `occt_get_view_clip_plane_limit`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_view_clip_plane_limit( OcctHandle handle, int* limit);
```

### `occt_update_points`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_points( OcctHandle handle, const OcctPointStateUpdate* updates, int count);
```

## `OcctOverlay.h`

### `occt_add_overlay_line`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_overlay_line(OcctHandle handle, OcctPoint3d start, OcctPoint3d end, int pattern, double width, double r, double g, double b);
```

### `occt_add_overlay_polyline`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_overlay_polyline(OcctHandle handle, const OcctPoint3d* points, int count, int pattern, double width, double r, double g, double b);
```

### `occt_add_overlay_marker`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_overlay_marker(OcctHandle handle, OcctPoint3d position, int marker, double scale, double r, double g, double b);
```

### `occt_add_overlay_text`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_overlay_text(OcctHandle handle, const char* text, OcctPoint3d position, double height, double r, double g, double b, int zoomable, const char* fontName);
```

### `occt_update_overlay_line`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_overlay_line(OcctHandle handle, OcctObjectId overlayId, OcctPoint3d start, OcctPoint3d end);
```

### `occt_update_overlay_polyline`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_overlay_polyline(OcctHandle handle, OcctObjectId overlayId, const OcctPoint3d* points, int count);
```

### `occt_update_overlay_marker`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_overlay_marker(OcctHandle handle, OcctObjectId overlayId, OcctPoint3d position);
```

### `occt_update_overlay_text`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_overlay_text(OcctHandle handle, OcctObjectId overlayId, const char* text, OcctPoint3d position);
```

### `occt_set_overlay_line_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_overlay_line_style(OcctHandle handle, OcctObjectId overlayId, int pattern, double width, double r, double g, double b);
```

### `occt_set_overlay_marker_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_overlay_marker_style(OcctHandle handle, OcctObjectId overlayId, int marker, double scale, double r, double g, double b);
```

### `occt_set_overlay_text_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_overlay_text_style(OcctHandle handle, OcctObjectId overlayId, double height, double r, double g, double b, int zoomable, const char* fontName);
```

### `occt_get_overlay_primitive_type`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_overlay_primitive_type(OcctHandle handle, OcctObjectId overlayId, int* primitiveType);
```

## `OcctPresentation.h`

### `occt_set_object_clip_planes`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_clip_planes( OcctHandle handle, OcctObjectId objectId, const OcctViewClipPlane* planes, int count);
```

### `occt_set_global_highlight_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_global_highlight_style( OcctHandle handle, int kind, const OcctHighlightStyleSettings* settings);
```

### `occt_set_object_highlight_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_highlight_style( OcctHandle handle, OcctObjectId objectId, int dynamic, const OcctHighlightStyleSettings* settings);
```

### `occt_clear_object_highlight_style`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_clear_object_highlight_style( OcctHandle handle, OcctObjectId objectId, int dynamic);
```

### `occt_reset_object_display_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_reset_object_display_mode( OcctHandle handle, OcctObjectId objectId);
```

### `occt_get_object_display_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_display_mode( OcctHandle handle, OcctObjectId objectId, int* hasOverride, int* displayMode);
```

### `occt_set_object_auto_highlight`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_auto_highlight( OcctHandle handle, OcctObjectId objectId, int enabled);
```

### `occt_get_object_auto_highlight`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_auto_highlight( OcctHandle handle, OcctObjectId objectId, int* enabled);
```

### `occt_set_object_infinite_state`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_object_infinite_state( OcctHandle handle, OcctObjectId objectId, int infinite);
```

### `occt_get_object_infinite_state`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_object_infinite_state( OcctHandle handle, OcctObjectId objectId, int* infinite);
```

## `OcctManipulator.h`

### `occt_add_manipulator`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_add_manipulator(OcctHandle handle);
```

### `occt_attach_manipulator`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_attach_manipulator( OcctHandle handle, OcctObjectId manipulatorId, const OcctObjectId* objectIds, int count, const OcctManipulatorAttachOptions* options);
```

### `occt_detach_manipulator`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_detach_manipulator( OcctHandle handle, OcctObjectId manipulatorId);
```

### `occt_set_manipulator_part`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_part( OcctHandle handle, OcctObjectId manipulatorId, int axisIndex, int mode, int enabled);
```

### `occt_set_manipulator_mode_enabled`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_mode_enabled( OcctHandle handle, OcctObjectId manipulatorId, int mode, int enabled);
```

### `occt_set_manipulator_mode_activation_on_detection`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_mode_activation_on_detection( OcctHandle handle, OcctObjectId manipulatorId, int enabled);
```

### `occt_set_manipulator_position`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_position( OcctHandle handle, OcctObjectId manipulatorId, OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection);
```

### `occt_set_manipulator_size`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_size( OcctHandle handle, OcctObjectId manipulatorId, double size);
```

### `occt_set_manipulator_gap`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_gap( OcctHandle handle, OcctObjectId manipulatorId, double gap);
```

### `occt_set_manipulator_zoom_persistence`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_zoom_persistence( OcctHandle handle, OcctObjectId manipulatorId, int enabled);
```

### `occt_set_manipulator_skin`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_set_manipulator_skin( OcctHandle handle, OcctObjectId manipulatorId, int skinMode);
```

### `occt_get_manipulator_state`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_manipulator_state( OcctHandle handle, OcctObjectId manipulatorId, OcctManipulatorState* result);
```

### `occt_get_manipulator_objects`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_get_manipulator_objects( OcctHandle handle, OcctObjectId manipulatorId, OcctObjectId* objectIds, int capacity, int* count);
```

### `occt_start_manipulator_transform`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_start_manipulator_transform( OcctHandle handle, OcctObjectId manipulatorId, int x, int y);
```

### `occt_update_manipulator_transform`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_manipulator_transform( OcctHandle handle, OcctObjectId manipulatorId, int x, int y);
```

### `occt_stop_manipulator_transform`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_stop_manipulator_transform( OcctHandle handle, OcctObjectId manipulatorId, int apply);
```

### `occt_deactivate_manipulator_mode`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_deactivate_manipulator_mode( OcctHandle handle, OcctObjectId manipulatorId);
```

## `OcctDetection.h`

### `occt_detect_at_filtered`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_detect_at_filtered( OcctHandle handle, int x, int y, int maxHits, const OcctObjectId* ownerIds, int ownerCount, std::uint64_t objectKindMask, std::uint64_t shapeTypeMask, int includeWholeObjects, OcctSelectionHitDetail* items, int capacity, int* count);
```

## `OcctModeling.h`

### `occt_model_create`

- **Returns:** `OcctModelHandle`

```cpp
OCCTBRIDGE_API OcctModelHandle occt_model_create();
```

### `occt_model_destroy`

- **Returns:** `void`

```cpp
OCCTBRIDGE_API void occt_model_destroy(OcctModelHandle handle);
```

### `occt_model_last_error`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_last_error(OcctModelHandle handle);
```

### `occt_model_capabilities`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_capabilities();
```

### `occt_model_shape_ids_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_ids_copy(OcctModelHandle handle, OcctObjectId* results, int capacity);
```

### `occt_model_shape_exists`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_exists(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_delete_shape`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_delete_shape(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_clear`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_clear(OcctModelHandle handle);
```

### `occt_model_operation_report`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_operation_report(OcctModelHandle handle, OcctOperationId operationId);
```

### `occt_model_copy_shape`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_copy_shape(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_shape_hash`

- **Returns:** `std::int64_t`

```cpp
OCCTBRIDGE_API std::int64_t occt_model_shape_hash(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_shape_type`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_type(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_shape_orientation`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_orientation(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_shape_is_closed`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_closed(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_shape_is_valid`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_valid(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_shape_tolerance`

- **Returns:** `double`

```cpp
OCCTBRIDGE_API double occt_model_shape_tolerance(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_shape_bounds`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_bounds(OcctModelHandle handle, OcctObjectId shapeId, OcctBounds* result);
```

### `occt_model_shape_linear_properties`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_linear_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

### `occt_model_shape_surface_properties`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_surface_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

### `occt_model_shape_volume_properties`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_volume_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result);
```

### `occt_model_shape_distance`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_distance(OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result);
```

### `occt_model_check_report`

- **Returns:** `const char*`

```cpp
OCCTBRIDGE_API const char* occt_model_check_report(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_get_location`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_get_location(OcctModelHandle handle, OcctObjectId shapeId, OcctModelLocation* result);
```

### `occt_model_set_location`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_set_location(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelLocation* location, int copyShape);
```

### `occt_model_subshapes_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_subshapes_copy(OcctModelHandle handle, OcctObjectId shapeId, int shapeType, OcctObjectId* results, int capacity);
```

### `occt_model_outer_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_outer_wire(OcctModelHandle handle, OcctObjectId faceId);
```

### `occt_model_inner_wires_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_inner_wires_copy(OcctModelHandle handle, OcctObjectId faceId, OcctObjectId* results, int capacity);
```

### `occt_model_ancestors_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ancestors_copy(OcctModelHandle handle, OcctObjectId rootId, OcctObjectId childId, int ancestorType, OcctObjectId* results, int capacity);
```

### `occt_model_vertex_point`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_vertex_point(OcctModelHandle handle, OcctObjectId vertexId, OcctPoint3d* result);
```

### `occt_model_edge_endpoints`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_endpoints(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end);
```

### `occt_model_edge_point_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_point_at(OcctModelHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* point, OcctVector3d* tangent);
```

### `occt_model_edge_curve_type`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_curve_type(OcctModelHandle handle, OcctObjectId edgeId);
```

### `occt_model_face_surface_type`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_surface_type(OcctModelHandle handle, OcctObjectId faceId);
```

### `occt_model_face_uv_bounds`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_uv_bounds(OcctModelHandle handle, OcctObjectId faceId, OcctUvBounds* result);
```

### `occt_model_face_point_normal`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_point_normal(OcctModelHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);
```

### `occt_model_edge_line_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_line_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
```

### `occt_model_edge_circle_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_circle_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
```

### `occt_model_edge_ellipse_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_ellipse_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
```

### `occt_model_face_plane_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_plane_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
```

### `occt_model_face_cylinder_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_cylinder_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
```

### `occt_model_face_cone_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_cone_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
```

### `occt_model_face_sphere_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_sphere_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
```

### `occt_model_face_torus_geometry`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);
```

### `occt_model_edge_parameter_range`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_parameter_range(OcctModelHandle handle, OcctObjectId edgeId, OcctModelParameterRange* result);
```

### `occt_model_edge_differential`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_differential(OcctModelHandle handle, OcctObjectId edgeId, double parameter, OcctModelCurveDifferential* result);
```

### `occt_model_edge_curvature`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_curvature(OcctModelHandle handle, OcctObjectId edgeId, double parameter, double resolution, OcctModelCurveCurvature* result);
```

### `occt_model_face_periodicity`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_periodicity(OcctModelHandle handle, OcctObjectId faceId, OcctModelSurfacePeriodicity* result);
```

### `occt_model_face_differential`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_differential(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceDifferential* result);
```

### `occt_model_face_curvature`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_curvature(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceCurvature* result);
```

### `occt_model_make_vertex`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_vertex(OcctModelHandle handle, OcctPoint3d point);
```

### `occt_model_make_line`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_line(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d end);
```

### `occt_model_make_polyline`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_polyline(OcctModelHandle handle, const OcctPoint3d* points, int count, int closed);
```

### `occt_model_make_circle`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_circle(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double radius);
```

### `occt_model_make_arc_three_points`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_arc_three_points(OcctModelHandle handle, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end);
```

### `occt_model_make_arc_center`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_arc_center(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, double startAngleDegrees, double endAngleDegrees);
```

### `occt_model_make_regular_polygon`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_regular_polygon(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
```

### `occt_model_make_ellipse`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_ellipse(OcctModelHandle handle, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius);
```

### `occt_model_make_bezier`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_bezier(OcctModelHandle handle, const OcctPoint3d* poles, int count);
```

### `occt_model_make_bspline_interpolated`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_bspline_interpolated(OcctModelHandle handle, const OcctPoint3d* points, int count, int periodic, double tolerance);
```

### `occt_model_make_rectangle_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_rectangle_wire(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

### `occt_model_make_plane_face`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_plane_face(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
```

### `occt_model_make_face_from_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_face_from_wire(OcctModelHandle handle, OcctObjectId wireId, int onlyPlane);
```

### `occt_model_make_box`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_box(OcctModelHandle handle, double x, double y, double z, double dx, double dy, double dz);
```

### `occt_model_make_cylinder`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_cylinder(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius, double height);
```

### `occt_model_make_cone`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_cone(OcctModelHandle handle, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height);
```

### `occt_model_make_sphere`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_sphere(OcctModelHandle handle, OcctPoint3d center, double radius);
```

### `occt_model_make_torus`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_torus(OcctModelHandle handle, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius);
```

### `occt_model_make_wedge`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_wedge(OcctModelHandle handle, double dx, double dy, double dz, double ltx);
```

### `occt_model_make_compound`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_compound(OcctModelHandle handle, const OcctObjectId* shapeIds, int count);
```

### `occt_model_make_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_wire(OcctModelHandle handle, const OcctObjectId* edgeIds, int count);
```

### `occt_model_sew`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_sew(OcctModelHandle handle, const OcctObjectId* shapeIds, int count, double tolerance);
```

### `occt_model_make_solid_from_shell`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_solid_from_shell(OcctModelHandle handle, OcctObjectId shellId);
```

### `occt_model_translate`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_translate(OcctModelHandle handle, OcctObjectId shapeId, OcctVector3d vector);
```

### `occt_model_rotate`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_rotate(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
```

### `occt_model_scale`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_scale(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d center, double factor);
```

### `occt_model_mirror_plane`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_mirror_plane(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal);
```

### `occt_model_boolean`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_boolean(OcctModelHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, const OcctModelBooleanOptions* options);
```

### `occt_model_split`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_split(OcctModelHandle handle, const OcctObjectId* objectIds, int objectCount, const OcctObjectId* toolIds, int toolCount, const OcctModelBooleanOptions* options);
```

### `occt_model_extrude`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_extrude(OcctModelHandle handle, OcctObjectId profileId, OcctVector3d vector);
```

### `occt_model_revolve`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_revolve(OcctModelHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
```

### `occt_model_sweep`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_sweep(OcctModelHandle handle, OcctObjectId spineWireId, OcctObjectId profileId);
```

### `occt_model_loft`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_loft(OcctModelHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance);
```

### `occt_model_fillet_edges`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fillet_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius);
```

### `occt_model_chamfer_edges`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_chamfer_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance);
```

### `occt_model_offset`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_offset(OcctModelHandle handle, OcctObjectId shapeId, double offset, double tolerance);
```

### `occt_model_thick_solid`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_thick_solid(OcctModelHandle handle, OcctObjectId solidId, const int* faceIndicesToRemove, int count, double thickness, double tolerance);
```

### `occt_model_unify_same_domain`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_unify_same_domain(OcctModelHandle handle, OcctObjectId shapeId, int unifyEdges, int unifyFaces, int concatBsplines);
```

### `occt_model_fix_shape`

- **Returns:** `OcctModelAlgorithmResult`

```cpp
OCCTBRIDGE_API OcctModelAlgorithmResult occt_model_fix_shape(OcctModelHandle handle, OcctObjectId shapeId, double precision, double minTolerance, double maxTolerance);
```

### `occt_model_project_point_on_edge`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_project_point_on_edge(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d point, OcctModelProjectionResult* result);
```

### `occt_model_project_point_on_face`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_project_point_on_face(OcctModelHandle handle, OcctObjectId faceId, OcctPoint3d point, OcctModelProjectionResult* result);
```

### `occt_model_ray_intersections`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ray_intersections(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d origin, OcctVector3d direction, double minimumParameter, double maximumParameter, double tolerance);
```

### `occt_model_ray_hits_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_ray_hits_copy(OcctModelHandle handle, OcctModelRayHit* results, int capacity);
```

### `occt_model_classify_point`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_classify_point(OcctModelHandle handle, OcctObjectId solidId, OcctPoint3d point, double tolerance);
```

### `occt_model_mesh`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_mesh(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelMeshParameters* parameters);
```

### `occt_model_clear_mesh`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_clear_mesh(OcctModelHandle handle, OcctObjectId shapeId);
```

### `occt_model_face_mesh_nodes_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_mesh_nodes_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshNode* results, int capacity);
```

### `occt_model_face_mesh_triangles_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_mesh_triangles_copy(OcctModelHandle handle, OcctObjectId faceId, OcctModelMeshTriangle* results, int capacity);
```

### `occt_model_import_file`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_file(OcctModelHandle handle, const char* utf8Path);
```

### `occt_model_import_step`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_step(OcctModelHandle handle, const char* utf8Path);
```

### `occt_model_import_iges`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_iges(OcctModelHandle handle, const char* utf8Path);
```

### `occt_model_import_brep`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_brep(OcctModelHandle handle, const char* utf8Path);
```

### `occt_model_import_stl`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_import_stl(OcctModelHandle handle, const char* utf8Path);
```

### `occt_model_export_step`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_step(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

### `occt_model_export_iges`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_iges(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

### `occt_model_export_brep`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_brep(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path);
```

### `occt_model_export_stl`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_export_stl(OcctModelHandle handle, OcctObjectId shapeId, const char* utf8Path, double linearDeflection, double angularDeflection, int asciiMode);
```

### `occt_model_history_generated_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_generated_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
```

### `occt_model_history_modified_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_modified_copy(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, OcctObjectId* results, int capacity);
```

### `occt_model_history_is_removed`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_history_is_removed(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId);
```

### `occt_model_display_in_engine`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_display_in_engine(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId shapeId, int fit);
```

### `occt_update_object_shape_from_model`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_update_object_shape_from_model(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId viewerObjectId, OcctObjectId modelShapeId, unsigned int options);
```

## `OcctModelingExtensions.h`

### `occt_model_shape_is_same`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_same( OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId);
```

### `occt_model_shape_is_partner`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_is_partner( OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId);
```

### `occt_model_shape_oriented_bounds`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_oriented_bounds( OcctModelHandle handle, OcctObjectId shapeId, int optimal, OcctOrientedBounds* result);
```

### `occt_model_make_face_with_holes`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_make_face_with_holes( OcctModelHandle handle, OcctObjectId outerWireId, const OcctObjectId* innerWireIds, int innerWireCount);
```

### `occt_model_trim_edge`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_trim_edge( OcctModelHandle handle, OcctObjectId edgeId, double firstParameter, double lastParameter);
```

### `occt_model_offset_wire`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_offset_wire( OcctModelHandle handle, OcctObjectId wireId, double offset, double altitude, int joinType, int openResult);
```

## `OcctModelingBSpline.h`

### `occt_model_edge_bspline_info`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_info( OcctModelHandle handle, OcctObjectId edgeId, OcctModelBSplineCurveInfo* result);
```

### `occt_model_edge_bspline_pole_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_pole_at( OcctModelHandle handle, OcctObjectId edgeId, int index, OcctPoint3d* pole, double* weight);
```

### `occt_model_edge_bspline_knot_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_bspline_knot_at( OcctModelHandle handle, OcctObjectId edgeId, int index, double* knot, int* multiplicity);
```

### `occt_model_face_bspline_info`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_info( OcctModelHandle handle, OcctObjectId faceId, OcctModelBSplineSurfaceInfo* result);
```

### `occt_model_face_bspline_pole_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_pole_at( OcctModelHandle handle, OcctObjectId faceId, int uIndex, int vIndex, OcctPoint3d* pole, double* weight);
```

### `occt_model_face_bspline_u_knot_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_u_knot_at( OcctModelHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity);
```

### `occt_model_face_bspline_v_knot_at`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_face_bspline_v_knot_at( OcctModelHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity);
```

## `OcctModelingTopologyAnalysis.h`

### `occt_model_shape_free_bounds`

- **Returns:** `OcctObjectId`

```cpp
OCCTBRIDGE_API OcctObjectId occt_model_shape_free_bounds( OcctModelHandle handle, OcctObjectId shapeId, double tolerance, int boundaryKind, int splitClosed, int splitOpen);
```

### `occt_model_shape_edge_adjacency`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_edge_adjacency( OcctModelHandle handle, OcctObjectId shapeId, OcctModelEdgeAdjacency* items, int capacity, int* count);
```

## `OcctModelingFaceAnalysis.h`

### `occt_model_shape_face_analysis`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_face_analysis( OcctModelHandle handle, OcctObjectId shapeId, OcctModelFaceAnalysis* items, int capacity, int* count);
```

## `OcctModelingInertia.h`

### `occt_model_shape_linear_inertia`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_linear_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

### `occt_model_shape_surface_inertia`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_surface_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

### `occt_model_shape_volume_inertia`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_shape_volume_inertia(OcctModelHandle handle, OcctObjectId shapeId, OcctModelInertiaProperties* result);
```

## `OcctModelingIntersection.h`

### `occt_model_intersect_edges`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_intersect_edges( OcctModelHandle handle, OcctObjectId firstEdgeId, OcctObjectId secondEdgeId, double tolerance);
```

### `occt_model_edge_intersections_copy`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_edge_intersections_copy( OcctModelHandle handle, OcctModelEdgeIntersection* results, int capacity);
```

## `OcctModelingTopologyReference.h`

### `occt_model_create_topology_reference`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_create_topology_reference( OcctModelHandle handle, OcctObjectId rootShapeId, OcctObjectId subshapeId, OcctModelTopologyReference* result);
```

### `occt_model_resolve_topology_reference`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_resolve_topology_reference( OcctModelHandle handle, OcctObjectId rootShapeId, const OcctModelTopologyReference* reference, double matchingTolerance, OcctModelTopologyReferenceResult* result);
```

### `occt_model_resolve_topology_reference_with_history`

- **Returns:** `int`

```cpp
OCCTBRIDGE_API int occt_model_resolve_topology_reference_with_history( OcctModelHandle handle, OcctObjectId rootShapeId, OcctOperationId operationId, OcctObjectId sourceShapeId, const OcctModelTopologyReference* reference, double matchingTolerance, OcctModelTopologyReferenceResult* result);
```

