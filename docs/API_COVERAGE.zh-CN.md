# OCCT 封装接口详细清单

本文件由源码接口声明整理，列出当前原生 C ABI、C# P/Invoke 映射及公开 .NET 类型。

桥接层不包含 OCAF/XDE；文档、撤销重做和 JSON 持久化由上层应用实现。

- OCCT: `7.9.0`
- Native exports: `339`
- Managed P/Invoke declarations: `339`
- Public .NET types: `80`

## 接口设计与命名规范

原生 C ABI、P/Invoke 与托管接口使用统一词汇：

| 层级 | 规则 | 示例 |
|---|---|---|
| 原生 C ABI | `occt_model_<对象>_<操作>` | `occt_model_face_curvature` |
| P/Invoke | 完整保留原生符号，统一 Cdecl 和精确名称 | `occt_model_edge_differential` |
| 托管查询 | `Get<对象><结果>` | `GetFaceCurvature()` |
| 托管求值 | `Evaluate<对象><参数含义>` | `EvaluateEdgeAtParameter()` |
| 集合索引 | 零基索引访问统一使用 `At` 后缀 | `GetSubshapeAt()` |
| 兼容别名 | 已发布的含义不够明确的方法保留为转发别名 | `GetBounds()` 转发到 `GetShapeBounds()` |

建模接口按职责划分为：会话与生命周期、形状查询、拓扑、几何查询、解析几何、微分几何、构造、算法、分析、网格、文件交换和操作历史。新增方法必须进入对应的 partial class 文件，不再堆入会话核心文件。

参数含义必须体现在方法名中。`EvaluateEdgeNormalized()` 接收 `[0, 1]` 归一化参数；`EvaluateEdgeAtParameter()` 接收 OCCT 原始曲线参数；面求值接口使用原始 `U`、`V` 参数。

## 选择程序集

| 使用场景 | 引用项目 | 说明 |
|---|---|---|
| 无界面几何计算、导入导出 | `OcctNet` | 类型安全的 Viewer API 和无窗口 `OcctModelingSession` |
| WinForms 视口 | `OcctNet.WinForms` | 提供 `OcctViewportControl`，直接绑定 HWND |
| WPF 视口 | `OcctNet.Wpf` | 提供 `OcctWpfViewport`，封装 `WindowsFormsHost`、依赖属性和事件转发 |

WPF 项目应直接引用 `OcctNet.Wpf`，不需要在业务窗口中手工创建 `WindowsFormsHost`。

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

## 常用功能与使用方法

### Viewer、相机和屏幕坐标

| 托管接口 | 功能 | 使用要点 |
|---|---|---|
| `FitAll()` | 适配全部显示对象 | 不改变模型，仅调整相机 |
| `Fit(shape)` / `Fit(shapes)` | 适配一个或多个对象 | 多对象版本在原生侧合并包围盒 |
| `FitSelected()` | 适配当前选择集 | 选择集中必须包含拓扑形状 |
| `SetView()` / `SetZUpView()` | 切换标准视图 | 冶金、建筑类应用通常使用 Z-up 视图 |
| `GetCamera()` / `SetCamera()` | 保存和恢复相机 | 可写入应用 JSON 文档 |
| `GetViewportState()` | 读取投影、抗锯齿、MSAA、阴影、DPI 等状态 | 适合设置面板和诊断信息 |
| `ResetView()` | 重置映射和方向 | 恢复 Viewer 默认状态 |
| `ResetViewOrientation()` | 只重置方向 | 保留缩放和中心映射 |
| `ResetViewMapping()` | 只重置中心与缩放映射 | 保留观察方向 |
| `ScreenToRay()` | 获取屏幕点对应的世界射线 | 用于捕捉、工作平面和射线检测 |
| `ScreenToPlane()` | 屏幕点投影到指定平面 | 射线与平面平行时抛出异常 |
| `TryScreenToPlane()` | 安全投影到平面 | 平行时返回 `false` |
| `GetSceneGravityPoint()` | 获取场景旋转重心 | 可用于自定义旋转交互 |

```csharp
var point = engine.ScreenToPlane(
    mouseX,
    mouseY,
    new OcctPoint3d(0, 0, 0),
    new OcctVector3d(0, 0, 1));
```

### 选择与框选

- `Select(x, y)`：点选；`appendSelection=true` 时追加选择。
- `SelectRectangle(...)`：矩形选择；`allowOverlap=false` 为完全包含，`true` 为相交选择。
- `OcctRectangleSelectionBehavior.Directional`：从左向右完全包含，从右向左相交选择。
- `SetSelectionMode()`：对象、顶点、边、线框、面、壳、实体级选择。
- `SelectAllVisible()`、`InvertSelection()`、`ClearSelection()`：选择集管理。
- `CopySelectedSubshape()`：把选中的子形复制为独立 `OcctShape`。

### 大批量对象操作

批量接口在一次 P/Invoke 中完成参数校验和 AIS 更新，适合数百或数千对象的场景：

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

支持的批量操作包括颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择。`IsVisible()`、`IsSelected()` 用于读取单个对象状态。

### 解析几何参数读取

优先使用 `GetEdgeCurveType()` 和 `GetFaceSurfaceType()` 判断几何类型；确认类型后，可读取精确解析参数，而不是通过离散采样反推半径、轴线和中心。旧的 `GetCurveType()` 和 `GetSurfaceType()` 仅作为兼容别名保留。

| 托管接口 | 适用类型 | 返回内容 |
|---|---|---|
| `GetLineGeometry()` | 直线边 | 原点、方向、首尾参数 |
| `GetCircleGeometry()` | 圆或圆弧边 | 圆心、法向、X 方向、半径、首尾参数 |
| `GetEllipseGeometry()` | 椭圆或椭圆弧边 | 中心、法向、X 方向、长短半径、首尾参数 |
| `GetPlaneGeometry()` | 平面 | 原点、法向、X 方向 |
| `GetCylinderGeometry()` | 圆柱面 | 轴线原点、轴向、X 方向、半径 |
| `GetConeGeometry()` | 圆锥面 | 顶点、轴向、X 方向、参考半径、半角 |
| `GetSphereGeometry()` | 球面 | 球心、轴向、X 方向、半径 |
| `GetTorusGeometry()` | 圆环面 | 中心、轴向、X 方向、主半径、次半径 |

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

类型不匹配、对象不是边或面、对象不属于当前会话时会抛出 `OcctException` 或参数异常。解析参数可用于特征识别、孔轴提取、尺寸标注、工程规则判断和参数化重建。

### 微分几何

微分几何接口直接读取曲线、曲面的导数、周期性、法向和曲率，不需要先转换为三角网格。

| 托管接口 | 参数含义 | 返回内容 |
|---|---|---|
| `GetEdgeParameterRange()` | 边 | 原始首尾参数、闭合/周期标志和周期 |
| `EvaluateEdgeAtParameter()` | 原始曲线参数 | 点、一阶导数和二阶导数 |
| `GetEdgeCurvature()` | 原始曲线参数 | 切向、法向、曲率中心、曲率及定义标志 |
| `GetFacePeriodicity()` | 面 | U/V 闭合、周期标志及周期 |
| `EvaluateFaceDifferential()` | 原始 U/V | 点、按面方向修正的法向、一阶及二阶偏导 |
| `GetFaceCurvature()` | 原始 U/V | 主曲率、平均曲率、高斯曲率、主方向和脐点状态 |

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

法向遵循拓扑面的方向。面为反向时，主曲率和平均曲率会进行符号修正，并重新排列最大、最小主曲率；高斯曲率保持不变。切向、法向或曲率不可定义时，通过明确的 `Has...` 属性表达，不使用无意义数值冒充有效结果。

### 几何与特征建模

```csharp
using var model = new OcctModelingSession();
var baseSolid = model.MakeBox(100, 80, 20);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -5), new OcctVector3d(0, 0, 1), 10, 30);
var result = model.Boolean(baseSolid, hole, OcctModelBooleanOperation.Cut);
model.ExportStep(result.Shape, "part.step");
```

主要能力：

- 基础几何：点、直线、圆弧、圆、椭圆、Bezier、B-Spline、多段线和规则多边形。
- 实体：Box、Cylinder、Cone、Sphere、Torus、Wedge。
- 构造：Wire、Face、Shell 缝合、Solid、Compound。
- 特征：Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、ThickSolid。
- 布尔与分割：Fuse、Cut、Common、Section、Split。
- 查询：拓扑数量、子形、包围盒、距离、质量属性、曲线/曲面类型、点法向。
- 分析：点投影、射线求交、实体内外分类、有效性检查和修复报告。
- 网格：生成、清除和读取面三角网格节点、法向、UV 与三角形索引。
- 交换：STEP、IGES、BREP、STL 导入导出。

### 生命周期、异常和线程

- `OcctEngine`、`OcctModelingSession` 和 `OcctDisplayBatch` 均应使用 `using` 或显式 `Dispose()`。
- 原生调用失败会转换为 `OcctException`，详细信息来自对应会话的 `LastError`。
- 同一个 Viewer 或建模会话属于可变原生状态，不应由多个线程并发调用。
- 创建 Viewer、处理鼠标及销毁视口应在 UI 线程完成。
- `OcctNet.dll`、UI 宿主程序集和 `OcctNative.dll` 必须来自同一次构建。

## 原生 C ABI

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
- `occt_get_viewport_state`
- `occt_reset_view`
- `occt_reset_view_orientation`
- `occt_reset_view_mapping`
- `occt_fit_selected`
- `occt_get_scene_gravity_point`
- `occt_zoom`

### OcctNative.h — Registry, AIS attributes and lifecycle (44)

- `occt_clear`
- `occt_copy_selected_subshape`
- `occt_copy_selected_subshape_at`
- `occt_delete_object`
- `occt_delete_objects`
- `occt_get_object_name`
- `occt_set_object_application_tag`
- `occt_get_object_application_tag`
- `occt_find_object_by_application_tag`
- `occt_set_object_selectable`
- `occt_get_object_selectable`
- `occt_set_objects_selectable`
- `occt_set_selected_objects_ex`
- `occt_set_object_transform`
- `occt_get_object_transform`
- `occt_reset_object_transform`
- `occt_set_view_cube_language`
- `occt_hide_all`
- `occt_highlight_object`
- `occt_object_count`
- `occt_object_exists`
- `occt_object_id_at`
- `occt_object_kind`
- `occt_object_is_selected`
- `occt_object_is_visible`
- `occt_redisplay_objects`
- `occt_select_objects`
- `occt_set_objects_color`
- `occt_set_objects_display_mode`
- `occt_set_objects_line_width`
- `occt_set_objects_material`
- `occt_set_objects_transparency`
- `occt_set_objects_visible`
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

### OcctModeling.h (119)

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
- `occt_update_object_shape_from_model`
- `occt_model_edge_curve_type`
- `occt_model_edge_line_geometry`
- `occt_model_edge_circle_geometry`
- `occt_model_edge_ellipse_geometry`
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
- `OcctSelectionOperation`
- `OcctShapeUpdateOptions`
- `OcctViewCubeLanguage`
- `OcctTransform3d`
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

## 公开 .NET 类型

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
- `OcctSelectionOperation`
- `OcctShapeUpdateOptions`
- `OcctViewCubeLanguage`
- `OcctTransform3d`
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
- `OcctObjectTransformUpdate`
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
- `OcctViewportState`
- `OcctWpfViewport`
- `OcctViewportErrorEventArgs`
- `OcctViewportSelectionEventArgs`
- `OcctViewportWorldPointEventArgs`

`OcctViewportControl` 及其事件参数由可选的 `OcctNet.WinForms` 程序集提供；`OcctWpfViewport` 由 `OcctNet.Wpf` 提供；其余托管类型位于不依赖 UI 的 `OcctNet` 程序集中。

## 桥接 ABI 约束

- 托管层要求的 ABI：`2`
- 原生桥接版本：`2.5.0`
- `OcctBridgeInfo` 会在创建 Viewer 或建模会话前校验已加载的 `OcctNative.dll`。
- 托管与原生二进制文件必须来自同一次构建。

## 一致性规则

`tests/check-api-surface.ps1` 校验每个原生声明均存在 C++ 定义和 C# P/Invoke 声明。
