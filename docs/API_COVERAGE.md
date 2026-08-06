# OCCT Bridge API Inventory

This source-derived inventory lists the current native C ABI, C# P/Invoke mapping, and public .NET types.

OCAF/XDE is intentionally excluded; documents, undo/redo, and JSON persistence are application-layer responsibilities.

- OCCT: `7.9.0`
- Native exports: `327`
- Managed P/Invoke declarations: `327`
- Public .NET types: `75`

## API design and naming rules

The public wrapper follows one vocabulary across native C ABI, P/Invoke, and managed APIs:

| Layer | Rule | Example |
|---|---|---|
| Native C ABI | `occt_model_<subject>_<operation>` | `occt_model_face_curvature` |
| P/Invoke | Preserve the native symbol exactly; use Cdecl and exact spelling | `occt_model_edge_differential` |
| Managed query | `Get<Subject><Result>` | `GetFaceCurvature()` |
| Managed evaluation | `Evaluate<Subject><ParameterMeaning>` | `EvaluateEdgeAtParameter()` |
| Collection indexing | Use the `At` suffix for zero-based indexed access | `GetSubshapeAt()` |
| Compatibility alias | Existing ambiguous names remain forwarding aliases | `GetBounds()` forwards to `GetShapeBounds()` |

Managed modeling APIs are organized by responsibility: session/core, shape queries, topology, geometry queries, analytic geometry, differential geometry, construction, algorithms, analysis, mesh, exchange, and operation history. A method must be placed in the corresponding partial-class file instead of the session core file.

Parameter semantics are explicit. `EvaluateEdgeNormalized()` accepts `[0, 1]`; `EvaluateEdgeAtParameter()` accepts the exact OCCT curve parameter. Face evaluation methods use exact surface `U` and `V` parameters.

## Choose an assembly

| Scenario | Reference | Purpose |
|---|---|---|
| Headless geometry and exchange | `OcctNet` | UI-independent viewer, modeling, analysis, mesh, healing, and exchange APIs |
| WinForms viewport | `OcctNet.WinForms` | `OcctViewportControl` bound directly to an HWND |
| WPF viewport | `OcctNet.Wpf` | `OcctWpfViewport` with `WindowsFormsHost`, dependency properties, and forwarded events |

A WPF application should reference `OcctNet.Wpf` directly instead of constructing a `WindowsFormsHost` in every application window.

```xml
<Window
    xmlns:occt="clr-namespace:OcctNet;assembly=OcctNet.Wpf">
    <occt:OcctWpfViewport x:Name="Viewport"
                          EnableRectangleSelection="True"
                          RectangleSelectionBehavior="Directional"
                          RectangleSelectionThreshold="3" />
</Window>
```

```csharp
Viewport.EngineInitialized += (_, _) =>
{
    Viewport.Engine.SetGradientBackground(Color.White, Color.LightSteelBlue);
    Viewport.Engine.SetZUpView(OcctZUpViewOrientation.IsometricXPositiveYNegative);
};
```

## Feature guide and examples

### Viewer, camera, and screen coordinates

| Managed API | Purpose | Notes |
|---|---|---|
| `FitAll()` | Fit all displayed objects | Changes the camera only |
| `Fit(shape)` / `Fit(shapes)` | Fit one or several objects | The native layer combines object bounds |
| `FitSelected()` | Fit the current selection | The selection must contain topological shapes |
| `SetView()` / `SetZUpView()` | Apply standard orientations | Z-up orientations are suitable for plant and building applications |
| `GetCamera()` / `SetCamera()` | Save and restore a camera | Camera data can be stored in an application JSON document |
| `GetViewportState()` | Read projection, antialiasing, MSAA, shadows, DPI, and selection settings | Useful for settings panels and diagnostics |
| `ResetView()` | Reset mapping and orientation | Restores the viewer defaults |
| `ResetViewOrientation()` | Reset orientation only | Keeps mapping and scale |
| `ResetViewMapping()` | Reset center and scale mapping only | Keeps the viewing direction |
| `ScreenToRay()` | Convert a screen point to a world ray | Used by snapping, work planes, and ray tests |
| `ScreenToPlane()` | Project a screen point onto a plane | Throws when the ray is parallel to the plane |
| `TryScreenToPlane()` | Safe plane projection | Returns `false` for a parallel ray |
| `GetSceneGravityPoint()` | Read the scene rotation center | Useful for custom orbit tools |

```csharp
var point = engine.ScreenToPlane(
    mouseX,
    mouseY,
    new OcctPoint3d(0, 0, 0),
    new OcctVector3d(0, 0, 1));
```

### Selection and rectangle selection

- `Select(x, y)` performs point selection; `appendSelection=true` adds to the current set.
- `SelectRectangle(...)` uses full inclusion by default; `allowOverlap=true` selects intersecting objects.
- `OcctRectangleSelectionBehavior.Directional` uses left-to-right inclusion and right-to-left crossing selection.
- `SetSelectionMode()` activates object, vertex, edge, wire, face, shell, or solid selection.
- `SelectAllVisible()`, `InvertSelection()`, and `ClearSelection()` manage selection sets.
- `CopySelectedSubshape()` promotes a selected subshape to an independent `OcctShape`.

### Batch object operations

Batch APIs validate and update several AIS objects in one P/Invoke call and are intended for scenes containing hundreds or thousands of objects.

```csharp
using (engine.BeginDisplayBatch())
{
    engine.SetColor(objects, Color.SteelBlue);
    engine.SetTransparency(objects, 0.25);
    engine.SetMaterial(objects, OcctMaterial.Steel);
    engine.SetDisplayMode(objects, OcctDisplayMode.Shaded);
    engine.SetVisible(objects, true);
}
```

Batch operations cover color, transparency, visibility, display mode, line width, material, redisplay, and selection. `IsVisible()` and `IsSelected()` read individual object state.

### Analytic geometry parameters

Use `GetEdgeCurveType()` or `GetFaceSurfaceType()` first, then read exact analytic parameters instead of estimating centers, axes, and radii from sampled points. The earlier `GetCurveType()` and `GetSurfaceType()` names remain compatibility aliases.

| Managed API | Geometry | Returned parameters |
|---|---|---|
| `GetLineGeometry()` | Line edge | Origin, direction, first and last parameters |
| `GetCircleGeometry()` | Circle or circular arc | Center, normal, X direction, radius, parameter range |
| `GetEllipseGeometry()` | Ellipse or elliptic arc | Center, normal, X direction, radii, parameter range |
| `GetPlaneGeometry()` | Plane | Origin, normal, X direction |
| `GetCylinderGeometry()` | Cylinder | Axis origin, axis direction, X direction, radius |
| `GetConeGeometry()` | Cone | Apex, axis, X direction, reference radius, semi-angle |
| `GetSphereGeometry()` | Sphere | Center, axis, X direction, radius |
| `GetTorusGeometry()` | Torus | Center, axis, X direction, major and minor radii |

```csharp
var edgeType = model.GetEdgeCurveType(edge);
if (edgeType == OcctCurveType.Circle)
{
    OcctCircleGeometry circle = model.GetCircleGeometry(edge);
    Console.WriteLine($"R = {circle.Radius:F3}");
}

var faceType = model.GetFaceSurfaceType(face);
if (faceType == OcctSurfaceType.Cylinder)
{
    OcctCylinderGeometry cylinder = model.GetCylinderGeometry(face);
    Console.WriteLine($"Axis = {cylinder.Axis}, R = {cylinder.Radius:F3}");
}
```

A type mismatch, non-edge/non-face input, or a shape from another session produces an argument or `OcctException`. These exact parameters support feature recognition, hole-axis extraction, dimensions, engineering rules, and parametric reconstruction.

### Differential geometry

Differential queries expose exact curve and surface derivatives, periodicity, normals, and curvature without converting the model to a mesh.

| Managed API | Input semantics | Returned data |
|---|---|---|
| `GetEdgeParameterRange()` | Edge | Exact first/last parameters, closed/periodic flags and period |
| `EvaluateEdgeAtParameter()` | Exact curve parameter | Point, first derivative and second derivative |
| `GetEdgeCurvature()` | Exact curve parameter | Tangent, normal, center and scalar curvature with definition flags |
| `GetFacePeriodicity()` | Face | U/V closed and periodic flags plus periods |
| `EvaluateFaceDifferential()` | Exact U/V | Point, oriented normal, first and second partial derivatives |
| `GetFaceCurvature()` | Exact U/V | Principal, mean and Gaussian curvature, principal directions and umbilic state |

```csharp
var range = model.GetEdgeParameterRange(edge);
var parameter = (range.FirstParameter + range.LastParameter) * 0.5;
var differential = model.EvaluateEdgeAtParameter(edge, parameter);
var curvature = model.GetEdgeCurvature(edge, parameter);

var uv = model.GetFaceUvBounds(face);
var u = (uv.UMin + uv.UMax) * 0.5;
var v = (uv.VMin + uv.VMax) * 0.5;
var surface = model.EvaluateFaceDifferential(face, u, v);
var surfaceCurvature = model.GetFaceCurvature(face, u, v);
```

Normals follow the topological face orientation. For reversed faces, principal curvatures and mean curvature are sign-adjusted and principal maximum/minimum values are reordered; Gaussian curvature is unchanged. Undefined tangents, normals, and curvature are represented by explicit `Has...` flags.

### Geometry and feature modeling

```csharp
using var model = new OcctModelingSession();
var baseSolid = model.MakeBox(100, 80, 20);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), new OcctVector3d(0, 0, 1), 10, 30);
var result = model.Boolean(baseSolid, hole, OcctModelBooleanOperation.Cut);
model.ExportStep(result.Shape, "part.step");
```

Coverage includes:

- Points, lines, arcs, circles, ellipses, Bezier, B-Spline, polylines, and regular polygons.
- Box, cylinder, cone, sphere, torus, and wedge solids.
- Wire, face, sewn shell, solid, and compound construction.
- Extrude, revolve, sweep, loft, fillet, chamfer, offset, and thick-solid features.
- Fuse, cut, common, section, and split operations.
- Topology traversal, bounds, distance, mass properties, curve/surface types, and point/normal evaluation.
- Projection, ray intersection, point classification, validity checking, and healing reports.
- Face triangulation nodes, normals, UV values, and triangle indices.
- STEP, IGES, BREP, and STL import/export.

### Lifetime, errors, and threading

- Dispose `OcctEngine`, `OcctModelingSession`, and `OcctDisplayBatch` through `using` or explicit `Dispose()`.
- Native failures are translated to `OcctException`; details originate from the session error state.
- A viewer or modeling session owns mutable native state and must not be called concurrently from several threads.
- Viewer creation, mouse interaction, and viewport destruction should remain on the UI thread.
- `OcctNet.dll`, the selected UI host assembly, and `OcctNative.dll` must come from the same build.

## Native C ABI

### OcctNative.h (7)

- `occt_create`
- `occt_destroy`
- `occt_last_error`
- `occt_version`
- `occt_bridge_abi_version`
- `occt_bridge_version`
- `occt_bridge_build_info`

### OcctNative.h — Viewer and interaction (78)

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
- `occt_set_objects_color`
- `occt_set_objects_transparency`
- `occt_set_objects_visibility`
- `occt_set_objects_display_mode`
- `occt_set_objects_line_width`
- `occt_set_objects_material`
- `occt_redisplay_objects`
- `occt_select_objects`
- `occt_get_viewport_state`
- `occt_reset_view`
- `occt_reset_view_orientation`
- `occt_reset_view_mapping`
- `occt_get_scene_gravity_point`
- `occt_fit_selected`
- `occt_screen_to_plane`

### OcctNative.h — Interactive objects (124)

- `occt_clear_objects`
- `occt_erase_object`
- `occt_object_exists`
- `occt_object_is_selected`
- `occt_object_is_visible`
- `occt_object_kind`
- `occt_object_redisplay`
- `occt_object_set_color`
- `occt_object_set_display_mode`
- `occt_object_set_line_width`
- `occt_object_set_material`
- `occt_object_set_selection_mode`
- `occt_object_set_transparency`
- `occt_object_set_visible`
- `occt_object_set_z_layer`
- `occt_object_transform`
- `occt_selected_object`
- `occt_shape_copy_selected_subshape`
- `occt_shape_explode`
- `occt_shape_exploded_at`
- `occt_shape_exploded_count`
- `occt_shape_get_location`
- `occt_shape_get_subshape_color`
- `occt_shape_remove_subshape_color`
- `occt_shape_set_location`
- `occt_shape_set_subshape_color`
- `occt_make_shape`
- `occt_make_vertex`
- `occt_make_segment`
- `occt_make_polyline`
- `occt_make_circle`
- `occt_make_arc_3p`
- `occt_make_ellipse`
- `occt_make_ellipse_arc`
- `occt_make_bezier`
- `occt_make_bspline`
- `occt_make_regular_polygon`
- `occt_make_plane`
- `occt_make_box`
- `occt_make_cylinder`
- `occt_make_cone`
- `occt_make_sphere`
- `occt_make_torus`
- `occt_make_wedge`
- `occt_make_prism`
- `occt_make_revol`
- `occt_make_pipe`
- `occt_make_loft`
- `occt_make_fillet`
- `occt_make_chamfer`
- `occt_make_offset`
- `occt_make_thick_solid`
- `occt_boolean`
- `occt_section`
- `occt_split`
- `occt_shape_transform`
- `occt_shape_translate`
- `occt_shape_rotate`
- `occt_shape_scale`
- `occt_shape_mirror_point`
- `occt_shape_mirror_axis`
- `occt_shape_mirror_plane`
- `occt_shape_bounds`
- `occt_shape_linear_properties`
- `occt_shape_surface_properties`
- `occt_shape_volume_properties`
- `occt_shape_distance`
- `occt_shape_check`
- `occt_shape_check_report`
- `occt_shape_fix`
- `occt_shape_unify_same_domain`
- `occt_shape_tolerance`
- `occt_shape_hash`
- `occt_shape_type`
- `occt_shape_orientation`
- `occt_shape_is_closed`
- `occt_shape_get_curve_type`
- `occt_shape_get_surface_type`
- `occt_shape_vertex_point`
- `occt_shape_edge_endpoints`
- `occt_shape_edge_point_at`
- `occt_shape_face_uv_bounds`
- `occt_shape_face_point_normal`
- `occt_shape_topology_count`
- `occt_shape_get_subshape`
- `occt_shape_outer_wire`
- `occt_shape_inner_wire_count`
- `occt_shape_inner_wire_at`
- `occt_shape_ancestor_count`
- `occt_shape_ancestor_at`
- `occt_shape_project_point_on_edge`
- `occt_shape_project_point_on_face`
- `occt_shape_ray_intersections`
- `occt_shape_ray_hit_count`
- `occt_shape_ray_hit_at`
- `occt_shape_classify_point`
- `occt_shape_mesh`
- `occt_shape_clear_mesh`
- `occt_shape_face_mesh_counts`
- `occt_shape_face_mesh_node`
- `occt_shape_face_mesh_triangle`
- `occt_shape_import_file`
- `occt_shape_import_step`
- `occt_shape_import_iges`
- `occt_shape_import_brep`
- `occt_shape_import_stl`
- `occt_shape_export_step`
- `occt_shape_export_iges`
- `occt_shape_export_brep`
- `occt_shape_export_stl`
- `occt_shape_display_in_engine`
- `occt_make_text_shape`
- `occt_make_length_annotation_shape`
- `occt_make_angle_annotation_shape`
- `occt_make_radius_annotation_shape`
- `occt_make_diameter_annotation_shape`
- `occt_text_update`
- `occt_text_get_properties`
- `occt_text_set_position`
- `occt_text_set_direction`
- `occt_text_set_color`
- `occt_text_set_height`
- `occt_dimension_update`
- `occt_dimension_get_properties`
- `occt_dimension_set_color`
- `occt_dimension_set_text_height`
- `occt_dimension_set_units`

### OcctModeling.h (118)

- `occt_model_capabilities`
- `occt_model_create`
- `occt_model_destroy`
- `occt_model_last_error`
- `occt_model_shape_count`
- `occt_model_shape_id_at`
- `occt_model_shape_exists`
- `occt_model_delete_shape`
- `occt_model_clear`
- `occt_model_copy_shape`
- `occt_model_make_vertex`
- `occt_model_make_edge_segment`
- `occt_model_make_edge_circle`
- `occt_model_make_edge_arc3p`
- `occt_model_make_edge_ellipse`
- `occt_model_make_edge_ellipse_arc`
- `occt_model_make_edge_bezier`
- `occt_model_make_edge_bspline`
- `occt_model_make_edge_polyline`
- `occt_model_make_edge_regular_polygon`
- `occt_model_make_wire`
- `occt_model_make_face_from_wire`
- `occt_model_make_face_from_wires`
- `occt_model_make_face_plane`
- `occt_model_make_box`
- `occt_model_make_cylinder`
- `occt_model_make_cone`
- `occt_model_make_sphere`
- `occt_model_make_torus`
- `occt_model_make_wedge`
- `occt_model_sew`
- `occt_model_make_solid`
- `occt_model_make_compound`
- `occt_model_transform`
- `occt_model_translate`
- `occt_model_rotate`
- `occt_model_scale`
- `occt_model_mirror_point`
- `occt_model_mirror_axis`
- `occt_model_mirror_plane`
- `occt_model_boolean`
- `occt_model_section`
- `occt_model_split`
- `occt_model_extrude`
- `occt_model_revolve`
- `occt_model_sweep`
- `occt_model_loft`
- `occt_model_fillet`
- `occt_model_chamfer`
- `occt_model_offset`
- `occt_model_thick_solid`
- `occt_model_shape_bounds`
- `occt_model_shape_linear_properties`
- `occt_model_shape_surface_properties`
- `occt_model_shape_volume_properties`
- `occt_model_shape_distance`
- `occt_model_shape_is_valid`
- `occt_model_check_report`
- `occt_model_fix_shape`
- `occt_model_unify_same_domain`
- `occt_model_shape_tolerance`
- `occt_model_shape_hash`
- `occt_model_shape_type`
- `occt_model_shape_orientation`
- `occt_model_shape_is_closed`
- `occt_model_get_location`
- `occt_model_set_location`
- `occt_model_edge_curve_type`
- `occt_model_face_surface_type`
- `occt_model_vertex_point`
- `occt_model_edge_endpoints`
- `occt_model_edge_point_at`
- `occt_model_face_uv_bounds`
- `occt_model_face_point_normal`
- `occt_model_topology_count`
- `occt_model_get_subshape`
- `occt_model_outer_wire`
- `occt_model_inner_wire_count`
- `occt_model_inner_wire_at`
- `occt_model_ancestor_count`
- `occt_model_ancestor_at`
- `occt_model_edge_line_geometry`
- `occt_model_edge_circle_geometry`
- `occt_model_edge_ellipse_geometry`
- `occt_model_face_plane_geometry`
- `occt_model_face_cylinder_geometry`
- `occt_model_face_cone_geometry`
- `occt_model_face_sphere_geometry`
- `occt_model_face_torus_geometry`
- `occt_model_edge_parameter_range`
- `occt_model_edge_differential`
- `occt_model_edge_curvature`
- `occt_model_face_periodicity`
- `occt_model_face_differential`
- `occt_model_face_curvature`
- `occt_model_project_point_on_edge`
- `occt_model_project_point_on_face`
- `occt_model_ray_intersections`
- `occt_model_ray_hit_count`
- `occt_model_ray_hit_at`
- `occt_model_classify_point`
- `occt_model_mesh`
- `occt_model_clear_mesh`
- `occt_model_face_mesh_counts`
- `occt_model_face_mesh_node`
- `occt_model_face_mesh_triangle`
- `occt_model_import_file`
- `occt_model_import_step`
- `occt_model_import_iges`
- `occt_model_import_brep`
- `occt_model_import_stl`
- `occt_model_export_step`
- `occt_model_export_iges`
- `occt_model_export_brep`
- `occt_model_export_stl`
- `occt_model_display_in_engine`
- `occt_model_operation_count`
- `occt_model_operation_id_at`
- `occt_model_operation_at`
- `occt_model_operation_result_count`
- `occt_model_operation_result_at`
- `occt_model_operation_modified_count`
- `occt_model_operation_modified_at`
- `occt_model_operation_generated_count`
- `occt_model_operation_generated_at`
- `occt_model_operation_is_deleted`

## Managed P/Invoke declarations

### NativeMethods.cs (209)

- `occt_auto_z_fit`
- `occt_begin_update`
- `occt_clear_objects`
- `occt_clear_selection`
- `occt_create`
- `occt_destroy`
- `occt_dimension_get_properties`
- `occt_dimension_set_color`
- `occt_dimension_set_text_height`
- `occt_dimension_set_units`
- `occt_dimension_update`
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
- `occt_invert_selection`
- `occt_is_updating`
- `occt_last_error`
- `occt_make_angle_annotation_shape`
- `occt_make_arc_3p`
- `occt_make_bezier`
- `occt_make_box`
- `occt_make_bspline`
- `occt_make_chamfer`
- `occt_make_circle`
- `occt_make_cone`
- `occt_make_cylinder`
- `occt_make_diameter_annotation_shape`
- `occt_make_ellipse`
- `occt_make_ellipse_arc`
- `occt_make_fillet`
- `occt_make_length_annotation_shape`
- `occt_make_loft`
- `occt_make_offset`
- `occt_make_pipe`
- `occt_make_plane`
- `occt_make_polyline`
- `occt_make_prism`
- `occt_make_radius_annotation_shape`
- `occt_make_regular_polygon`
- `occt_make_revol`
- `occt_make_segment`
- `occt_make_shape`
- `occt_make_sphere`
- `occt_make_text_shape`
- `occt_make_thick_solid`
- `occt_make_torus`
- `occt_make_vertex`
- `occt_make_wedge`
- `occt_move_to`
- `occt_object_erase`
- `occt_object_exists`
- `occt_object_get_subshape_color`
- `occt_object_is_selected`
- `occt_object_is_visible`
- `occt_object_kind`
- `occt_object_redisplay`
- `occt_object_remove_subshape_color`
- `occt_object_set_color`
- `occt_object_set_display_mode`
- `occt_object_set_line_width`
- `occt_object_set_location`
- `occt_object_set_material`
- `occt_object_set_selection_mode`
- `occt_object_set_subshape_color`
- `occt_object_set_transparency`
- `occt_object_set_visible`
- `occt_object_set_z_layer`
- `occt_object_transform`
- `occt_pan`
- `occt_redraw`
- `occt_redisplay_objects`
- `occt_reset_object_polygon_offsets`
- `occt_reset_scene_lighting`
- `occt_reset_view`
- `occt_reset_view_mapping`
- `occt_reset_view_orientation`
- `occt_resize`
- `occt_rotation`
- `occt_screen_to_plane`
- `occt_screen_to_ray`
- `occt_screen_to_world`
- `occt_select`
- `occt_select_all_visible`
- `occt_select_object`
- `occt_select_objects`
- `occt_select_rectangle`
- `occt_select_rectangle_ex`
- `occt_selected_at`
- `occt_selected_count`
- `occt_selected_object`
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
- `occt_set_objects_color`
- `occt_set_objects_display_mode`
- `occt_set_objects_line_width`
- `occt_set_objects_material`
- `occt_set_objects_transparency`
- `occt_set_objects_visibility`
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
- `occt_set_zup_view`
- `occt_shape_ancestor_at`
- `occt_shape_ancestor_count`
- `occt_shape_check`
- `occt_shape_check_report`
- `occt_shape_classify_point`
- `occt_shape_clear_mesh`
- `occt_shape_copy_selected_subshape`
- `occt_shape_edge_endpoints`
- `occt_shape_edge_point_at`
- `occt_shape_erase`
- `occt_shape_explode`
- `occt_shape_exploded_at`
- `occt_shape_exploded_count`
- `occt_shape_export_brep`
- `occt_shape_export_iges`
- `occt_shape_export_step`
- `occt_shape_export_stl`
- `occt_shape_face_mesh_counts`
- `occt_shape_face_mesh_node`
- `occt_shape_face_mesh_triangle`
- `occt_shape_face_point_normal`
- `occt_shape_face_uv_bounds`
- `occt_shape_fix`
- `occt_shape_get_curve_type`
- `occt_shape_get_location`
- `occt_shape_get_subshape`
- `occt_shape_get_surface_type`
- `occt_shape_hash`
- `occt_shape_import_brep`
- `occt_shape_import_file`
- `occt_shape_import_iges`
- `occt_shape_import_step`
- `occt_shape_import_stl`
- `occt_shape_inner_wire_at`
- `occt_shape_inner_wire_count`
- `occt_shape_is_closed`
- `occt_shape_is_valid`
- `occt_shape_linear_properties`
- `occt_shape_mesh`
- `occt_shape_outer_wire`
- `occt_shape_project_point_on_edge`
- `occt_shape_project_point_on_face`
- `occt_shape_ray_hit_at`
- `occt_shape_ray_hit_count`
- `occt_shape_ray_intersections`
- `occt_shape_set_location`
- `occt_shape_split`
- `occt_shape_surface_properties`
- `occt_shape_tolerance`
- `occt_shape_topology_count`
- `occt_shape_transform`
- `occt_shape_translate`
- `occt_shape_rotate`
- `occt_shape_scale`
- `occt_shape_mirror_point`
- `occt_shape_mirror_axis`
- `occt_shape_mirror_plane`
- `occt_shape_type`
- `occt_shape_unify_same_domain`
- `occt_shape_vertex_point`
- `occt_shape_volume_properties`
- `occt_split`
- `occt_start_rotation`
- `occt_text_get_properties`
- `occt_text_set_color`
- `occt_text_set_direction`
- `occt_text_set_height`
- `occt_text_set_position`
- `occt_text_update`
- `occt_version`
- `occt_window_fit`
- `occt_world_to_screen`
- `occt_zoom_at_point`
- `occt_fit_objects`
- `occt_get_viewport_state`
- `occt_get_scene_gravity_point`
- `occt_fit_selected`
- `occt_bridge_abi_version`
- `occt_bridge_version`
- `occt_bridge_build_info`

### ModelNativeMethods.cs (118)

- `occt_model_ancestor_at`
- `occt_model_ancestor_count`
- `occt_model_boolean`
- `occt_model_capabilities`
- `occt_model_chamfer`
- `occt_model_check_report`
- `occt_model_classify_point`
- `occt_model_clear`
- `occt_model_clear_mesh`
- `occt_model_copy_shape`
- `occt_model_create`
- `occt_model_delete_shape`
- `occt_model_destroy`
- `occt_model_display_in_engine`
- `occt_model_edge_endpoints`
- `occt_model_edge_point_at`
- `occt_model_edge_curve_type`
- `occt_model_edge_line_geometry`
- `occt_model_edge_circle_geometry`
- `occt_model_edge_ellipse_geometry`
- `occt_model_face_surface_type`
- `occt_model_face_plane_geometry`
- `occt_model_face_cylinder_geometry`
- `occt_model_face_cone_geometry`
- `occt_model_face_sphere_geometry`
- `occt_model_face_torus_geometry`
- `occt_model_edge_parameter_range`
- `occt_model_edge_differential`
- `occt_model_edge_curvature`
- `occt_model_face_periodicity`
- `occt_model_face_differential`
- `occt_model_face_curvature`
- `occt_model_export_brep`
- `occt_model_export_iges`
- `occt_model_export_step`
- `occt_model_export_stl`
- `occt_model_extrude`
- `occt_model_face_mesh_counts`
- `occt_model_face_mesh_node`
- `occt_model_face_mesh_triangle`
- `occt_model_face_point_normal`
- `occt_model_face_uv_bounds`
- `occt_model_fillet`
- `occt_model_fix_shape`
- `occt_model_get_location`
- `occt_model_get_subshape`
- `occt_model_import_brep`
- `occt_model_import_file`
- `occt_model_import_iges`
- `occt_model_import_step`
- `occt_model_import_stl`
- `occt_model_inner_wire_at`
- `occt_model_inner_wire_count`
- `occt_model_last_error`
- `occt_model_loft`
- `occt_model_make_box`
- `occt_model_make_compound`
- `occt_model_make_cone`
- `occt_model_make_cylinder`
- `occt_model_make_edge_arc3p`
- `occt_model_make_edge_bezier`
- `occt_model_make_edge_bspline`
- `occt_model_make_edge_circle`
- `occt_model_make_edge_ellipse`
- `occt_model_make_edge_ellipse_arc`
- `occt_model_make_edge_polyline`
- `occt_model_make_edge_regular_polygon`
- `occt_model_make_edge_segment`
- `occt_model_make_face_from_wire`
- `occt_model_make_face_from_wires`
- `occt_model_make_face_plane`
- `occt_model_make_solid`
- `occt_model_make_sphere`
- `occt_model_make_torus`
- `occt_model_make_vertex`
- `occt_model_make_wedge`
- `occt_model_make_wire`
- `occt_model_mesh`
- `occt_model_mirror_axis`
- `occt_model_mirror_plane`
- `occt_model_mirror_point`
- `occt_model_offset`
- `occt_model_operation_at`
- `occt_model_operation_count`
- `occt_model_operation_generated_at`
- `occt_model_operation_generated_count`
- `occt_model_operation_id_at`
- `occt_model_operation_is_deleted`
- `occt_model_operation_modified_at`
- `occt_model_operation_modified_count`
- `occt_model_operation_result_at`
- `occt_model_operation_result_count`
- `occt_model_outer_wire`
- `occt_model_project_point_on_edge`
- `occt_model_project_point_on_face`
- `occt_model_ray_hit_at`
- `occt_model_ray_hit_count`
- `occt_model_ray_intersections`
- `occt_model_revolve`
- `occt_model_rotate`
- `occt_model_scale`
- `occt_model_section`
- `occt_model_set_location`
- `occt_model_sew`
- `occt_model_shape_bounds`
- `occt_model_shape_count`
- `occt_model_shape_distance`
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
- `occt_model_shape_exists`
- `occt_model_split`
- `occt_model_sweep`
- `occt_model_thick_solid`
- `occt_model_topology_count`
- `occt_model_transform`
- `occt_model_translate`
- `occt_model_unify_same_domain`
- `occt_model_vertex_point`

## P/Invoke-compatible structs and enums

- `NativeModelAlgorithmResult`
- `OcctAutoZFitSettings`
- `OcctBooleanOperation`
- `OcctBounds`
- `OcctCameraState`
- `OcctCurveType`
- `OcctDimensionProperties`
- `OcctDirectionalLightSettings`
- `OcctDisplayMode`
- `OcctDistanceResult`
- `OcctEdgeEvaluation`
- `OcctFaceEvaluation`
- `OcctGradientFillMethod`
- `OcctLightingPreset`
- `OcctLineGeometry`
- `OcctCircleGeometry`
- `OcctEllipseGeometry`
- `OcctPlaneGeometry`
- `OcctCylinderGeometry`
- `OcctConeGeometry`
- `OcctSphereGeometry`
- `OcctTorusGeometry`
- `OcctModelParameterRange`
- `OcctModelCurveDifferential`
- `OcctModelCurveCurvature`
- `OcctModelSurfacePeriodicity`
- `OcctModelSurfaceDifferential`
- `OcctModelSurfaceCurvature`
- `OcctMassProperties`
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
- `OcctViewportState`
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
- `OcctLineGeometry`
- `OcctCircleGeometry`
- `OcctEllipseGeometry`
- `OcctPlaneGeometry`
- `OcctCylinderGeometry`
- `OcctConeGeometry`
- `OcctSphereGeometry`
- `OcctTorusGeometry`
- `OcctModelParameterRange`
- `OcctModelCurveDifferential`
- `OcctModelCurveCurvature`
- `OcctModelSurfacePeriodicity`
- `OcctModelSurfaceDifferential`
- `OcctModelSurfaceCurvature`
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
- `OcctViewportState`
- `OcctWpfViewport`
- `OcctViewportErrorEventArgs`
- `OcctViewportSelectionEventArgs`
- `OcctViewportWorldPointEventArgs`

`OcctViewportControl` and its event types are provided by `OcctNet.WinForms`; `OcctWpfViewport` is provided by `OcctNet.Wpf`; all remaining managed types stay in the UI-independent `OcctNet` assembly.

## Bridge ABI contract

- Managed expected ABI: `2`
- Native bridge version: `2.5.0`
- `OcctBridgeInfo` validates the loaded `OcctNative.dll` before creating viewer or modeling sessions.
- Managed and native binaries should always be deployed from the same build.

## Consistency rule

`tests/check-api-surface.ps1` verifies that every native declaration has both a C++ definition and a C# P/Invoke declaration.