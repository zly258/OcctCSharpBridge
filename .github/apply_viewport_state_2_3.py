from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    normalized = text.replace("\r\n", "\n").replace("\n", "\r\n")
    (ROOT / path).write_bytes(BOM + normalized.encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


header = read("src/OcctNative/OcctNative.h")
state_struct = """    struct OcctProjectionRay { OcctPoint3d origin; OcctVector3d direction; };
    struct OcctViewportState
    {
        int width;
        int height;
        int projectionType;
        int computedMode;
        int antialiasingEnabled;
        int msaaSamples;
        int renderingMethod;
        int shadowsEnabled;
        int frustumCullingEnabled;
        int faceBoundariesVisible;
        int selectionTolerance;
        int automaticHighlight;
        double perspectiveFov;
        double renderResolutionScale;
        double renderResolutionDpi;
    };
"""
header = replace_once(
    header,
    "    struct OcctProjectionRay { OcctPoint3d origin; OcctVector3d direction; };\n",
    state_struct,
    "viewport state structure")
function_anchor = "    OCCTBRIDGE_API int occt_set_face_boundaries_visible(OcctHandle handle, int visible, int applyExisting);\n"
state_functions = function_anchor + """    OCCTBRIDGE_API int occt_get_viewport_state(OcctHandle handle, OcctViewportState* result);
    OCCTBRIDGE_API int occt_reset_view(OcctHandle handle);
    OCCTBRIDGE_API int occt_reset_view_orientation(OcctHandle handle);
    OCCTBRIDGE_API int occt_reset_view_mapping(OcctHandle handle);
    OCCTBRIDGE_API int occt_fit_selected(OcctHandle handle, double margin);
    OCCTBRIDGE_API int occt_get_scene_gravity_point(OcctHandle handle, OcctPoint3d* result);
"""
header = replace_once(header, function_anchor, state_functions, "viewport state functions")
write("src/OcctNative/OcctNative.h", header)

engine = read("src/OcctNative/OcctEngine.cpp")
engine = engine.replace("2.2.0", "2.3.0")
write("src/OcctNative/OcctEngine.cpp", engine)

info = read("src/OcctNet/OcctBridgeInfo.cs")
info = info.replace('ManagedVersion = "2.2.0"', 'ManagedVersion = "2.3.0"')
write("src/OcctNet/OcctBridgeInfo.cs", info)

viewport_check = read("tests/check-viewport-api.ps1")
viewport_check = replace_once(
    viewport_check,
    '    "src/OcctNet/OcctEngine.Viewport.cs" = @(\n',
    '    "src/OcctNative/OcctViewportState.cpp" = @(\n'
    '        "occt_get_viewport_state", "occt_reset_view", "occt_reset_view_orientation",\n'
    '        "occt_reset_view_mapping", "occt_fit_selected", "occt_get_scene_gravity_point"\n'
    '    )\n'
    '    "src/OcctNet/OcctEngine.Viewport.cs" = @(\n',
    "native viewport state validation")
viewport_check = replace_once(
    viewport_check,
    '        "SetRenderingMethod", "SetFaceBoundariesVisible"\n',
    '        "SetRenderingMethod", "SetFaceBoundariesVisible", "GetViewportState",\n'
    '        "ResetView", "FitSelected", "GetSceneGravityPoint", "ScreenToPlane"\n',
    "managed viewport state validation")
write("tests/check-viewport-api.ps1", viewport_check)

zh_guide = r'''## 选择程序集

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

'''

en_guide = r'''## Choose an assembly

| Scenario | Reference | Purpose |
|---|---|---|
| Headless geometry and exchange | `OcctNet` | Type-safe viewer API plus `OcctModelingSession` |
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

'''


def update_inventory(path: str, chinese: bool) -> None:
    text = read(path)
    text = re.sub(r"Native exports:\s*`?\d+`?", "Native exports: `313`", text, count=1)
    text = re.sub(r"Managed P/Invoke declarations:\s*`?\d+`?", "Managed P/Invoke declarations: `313`", text, count=1)
    text = re.sub(r"Public \.NET types:\s*`?\d+`?", "Public .NET types: `61`", text, count=1)
    text = text.replace("Viewer and interaction (72)", "Viewer and interaction (78)", 1)
    list_anchor = "- `occt_set_face_boundaries_visible`\n"
    additions = list_anchor + """- `occt_get_viewport_state`
- `occt_reset_view`
- `occt_reset_view_orientation`
- `occt_reset_view_mapping`
- `occt_fit_selected`
- `occt_get_scene_gravity_point`
"""
    text = replace_once(text, list_anchor, additions, f"viewport API inventory in {path}")
    text = replace_once(text, "- `OcctProjectionRay`\n", "- `OcctProjectionRay`\n- `OcctViewportState`\n", f"native viewport state type in {path}")
    text = replace_once(text, "- `OcctViewportControl`\n", "- `OcctViewportControl`\n- `OcctViewportState`\n", f"public viewport state type in {path}")
    text = text.replace("2.2.0", "2.3.0")

    guide = zh_guide if chinese else en_guide
    raw_heading = "## 原生 C ABI\n" if chinese else "## Native C ABI\n"
    if "## 常用功能与使用方法" not in text and "## Feature guide and examples" not in text:
        text = replace_once(text, raw_heading, guide + raw_heading, f"usage guide in {path}")
    write(path, text)


update_inventory("docs/API_COVERAGE.md", False)
update_inventory("docs/API_COVERAGE.zh-CN.md", True)

readme = read("README.md")
readme = readme.replace("2.2.0", "2.3.0")
if "GetViewportState" not in readme:
    readme = readme.replace(
        "Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations reduce repeated P/Invoke calls for large scenes.\n",
        "Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations reduce repeated P/Invoke calls for large scenes. Viewport-state snapshots, selected-object fitting, reset operations, scene gravity points, and screen-to-plane projection support reusable CAD interaction tools.\n")
write("README.md", readme)

readme_zh = read("README.zh-CN.md")
readme_zh = readme_zh.replace("2.2.0", "2.3.0")
if "视口状态快照" not in readme_zh:
    readme_zh = readme_zh.replace(
        "新增批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择接口，减少大型场景中的重复 P/Invoke 调用。\n",
        "新增批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择接口，减少大型场景中的重复 P/Invoke 调用。视口状态快照、适配选择集、视图重置、场景重心和屏幕投影到平面接口可直接支撑 CAD 交互工具。\n")
write("README.zh-CN.md", readme_zh)

(ROOT / ".github/apply_viewport_state_2_3.py").unlink()
