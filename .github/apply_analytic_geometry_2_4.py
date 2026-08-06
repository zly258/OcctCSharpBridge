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


# Native ABI structures and functions.
header = read("src/OcctNative/OcctModeling.h")
struct_anchor = """    struct OcctModelMeshTriangle
    {
        int node1;
        int node2;
        int node3;
    };
"""
structs = struct_anchor + """
    struct OcctModelLineGeometry
    {
        OcctPoint3d origin;
        OcctVector3d direction;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelCircleGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double radius;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelEllipseGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double majorRadius;
        double minorRadius;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelPlaneGeometry
    {
        OcctPoint3d origin;
        OcctVector3d normal;
        OcctVector3d xDirection;
    };

    struct OcctModelCylinderGeometry
    {
        OcctPoint3d origin;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double radius;
    };

    struct OcctModelConeGeometry
    {
        OcctPoint3d apex;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double referenceRadius;
        double semiAngleRadians;
    };

    struct OcctModelSphereGeometry
    {
        OcctPoint3d center;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double radius;
    };

    struct OcctModelTorusGeometry
    {
        OcctPoint3d center;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double majorRadius;
        double minorRadius;
    };
"""
header = replace_once(header, struct_anchor, structs, "analytic geometry native structures")

function_anchor = "    OCCTBRIDGE_API int occt_model_face_point_normal(OcctModelHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* point, OcctVector3d* normal);\n"
functions = function_anchor + """    OCCTBRIDGE_API int occt_model_edge_line_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result);
    OCCTBRIDGE_API int occt_model_edge_circle_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result);
    OCCTBRIDGE_API int occt_model_edge_ellipse_geometry(OcctModelHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result);
    OCCTBRIDGE_API int occt_model_face_plane_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result);
    OCCTBRIDGE_API int occt_model_face_cylinder_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result);
    OCCTBRIDGE_API int occt_model_face_cone_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result);
    OCCTBRIDGE_API int occt_model_face_sphere_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result);
    OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);
"""
header = replace_once(header, function_anchor, functions, "analytic geometry native functions")
write("src/OcctNative/OcctModeling.h", header)

# Native source list and capability declaration.
cmake = read("src/OcctNative/CMakeLists.txt")
cmake = replace_once(
    cmake,
    "    OcctModelingAnalysis.cpp\n",
    "    OcctModelingAnalysis.cpp\n    OcctModelingAnalyticGeometry.cpp\n",
    "analytic geometry CMake source")
write("src/OcctNative/CMakeLists.txt", cmake)

core = read("src/OcctNative/OcctModelingCore.cpp")
core = replace_once(
    core,
    'return "headless;geometry-query;topology;history;healing;mesh;projection;ray-intersection;classification;advanced-boolean;splitter;sweep;loft;step;iges;brep;stl;viewer-interop";',
    'return "headless;geometry-query;analytic-geometry;topology;history;healing;mesh;projection;ray-intersection;classification;advanced-boolean;splitter;sweep;loft;step;iges;brep;stl;viewer-interop";',
    "model capability string")
write("src/OcctNative/OcctModelingCore.cpp", core)

engine = read("src/OcctNative/OcctEngine.cpp")
engine = engine.replace("2.3.0", "2.4.0")
write("src/OcctNative/OcctEngine.cpp", engine)

bridge_info = read("src/OcctNet/OcctBridgeInfo.cs")
bridge_info = replace_once(
    bridge_info,
    'public const string ManagedVersion = "2.3.0";',
    'public const string ManagedVersion = "2.4.0";',
    "managed bridge version")
write("src/OcctNet/OcctBridgeInfo.cs", bridge_info)

# Build validation.
build = read("build.ps1")
build = replace_once(
    build,
    '$UiHostsCheck = Join-Path $RepoRoot "tests\\check-ui-hosts.ps1"\n',
    '$UiHostsCheck = Join-Path $RepoRoot "tests\\check-ui-hosts.ps1"\n'
    '$AnalyticGeometryCheck = Join-Path $RepoRoot "tests\\check-analytic-geometry-api.ps1"\n',
    "analytic validation path")
build = replace_once(
    build,
    '    Assert-Path $UiHostsCheck\n\n',
    '    Assert-Path $UiHostsCheck\n'
    '    Assert-Path $AnalyticGeometryCheck\n\n'
    '    Write-Host "[analytic-geometry] Validating analytic curve and surface contracts..." -ForegroundColor Cyan\n'
    '    & $AnalyticGeometryCheck -RepositoryRoot $RepoRoot\n'
    '    if (-not $?) {\n'
    '        throw "Analytic geometry API validation failed."\n'
    '    }\n\n',
    "analytic validation invocation")
write("build.ps1", build)

analytic_functions = [
    "occt_model_edge_line_geometry",
    "occt_model_edge_circle_geometry",
    "occt_model_edge_ellipse_geometry",
    "occt_model_face_plane_geometry",
    "occt_model_face_cylinder_geometry",
    "occt_model_face_cone_geometry",
    "occt_model_face_sphere_geometry",
    "occt_model_face_torus_geometry",
]
analytic_types = [
    "OcctLineGeometry",
    "OcctCircleGeometry",
    "OcctEllipseGeometry",
    "OcctPlaneGeometry",
    "OcctCylinderGeometry",
    "OcctConeGeometry",
    "OcctSphereGeometry",
    "OcctTorusGeometry",
]


def update_inventory(path: str, chinese: bool) -> None:
    text = read(path)
    text = re.sub(r"Native exports:\s*`?\d+`?", "Native exports: `321`", text, count=1)
    text = re.sub(r"Managed P/Invoke declarations:\s*`?\d+`?", "Managed P/Invoke declarations: `321`", text, count=1)
    text = re.sub(r"Public \.NET types:\s*`?\d+`?", "Public .NET types: `69`", text, count=1)
    text = text.replace("### OcctModeling.h (104)", "### OcctModeling.h (112)", 1)
    text = text.replace("2.3.0", "2.4.0")

    function_anchor = "- `occt_model_edge_curve_type`\n"
    function_lines = function_anchor + "".join(f"- `{name}`\n" for name in analytic_functions[:3])
    text = replace_once(text, function_anchor, function_lines, f"analytic curve functions in {path}")

    surface_anchor = "- `occt_model_face_surface_type`\n"
    surface_lines = surface_anchor + "".join(f"- `{name}`\n" for name in analytic_functions[3:])
    text = replace_once(text, surface_anchor, surface_lines, f"analytic surface functions in {path}")

    native_type_anchor = "- `OcctMassProperties`\n"
    native_type_lines = native_type_anchor + "".join(f"- `{name}`\n" for name in analytic_types)
    text = replace_once(text, native_type_anchor, native_type_lines, f"analytic native types in {path}")

    public_type_anchor = "- `OcctMassProperties`\n"
    public_start = text.find("## Public .NET types") if not chinese else text.find("## 公开 .NET 类型")
    if public_start < 0:
        raise RuntimeError(f"Public type section not found: {path}")
    before = text[:public_start]
    after = text[public_start:]
    public_type_lines = public_type_anchor + "".join(f"- `{name}`\n" for name in analytic_types)
    after = replace_once(after, public_type_anchor, public_type_lines, f"analytic public types in {path}")
    text = before + after

    if chinese:
        heading = "### 解析几何参数读取\n"
        guide = """### 解析几何参数读取

`GetCurveType()` 和 `GetSurfaceType()` 用于判断几何类型；确认类型后，可读取精确解析参数，而不是通过离散采样反推半径、轴线和中心。

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
var edgeType = model.GetCurveType(edge);
if (edgeType == OcctCurveType.Circle)
{
    OcctCircleGeometry circle = model.GetCircleGeometry(edge);
    Console.WriteLine($"R = {circle.Radius:F3}");
}

var faceType = model.GetSurfaceType(face);
if (faceType == OcctSurfaceType.Cylinder)
{
    OcctCylinderGeometry cylinder = model.GetCylinderGeometry(face);
    Console.WriteLine($"Axis = {cylinder.Axis}, R = {cylinder.Radius:F3}");
}
```

类型不匹配、对象不是边或面、对象不属于当前会话时会抛出 `OcctException` 或参数异常。解析参数可用于特征识别、孔轴提取、尺寸标注、工程规则判断和参数化重建。

"""
        insert_before = "### 几何与特征建模\n"
    else:
        heading = "### Analytic geometry parameters\n"
        guide = """### Analytic geometry parameters

Use `GetCurveType()` or `GetSurfaceType()` first, then read exact analytic parameters instead of estimating centers, axes, and radii from sampled points.

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
var edgeType = model.GetCurveType(edge);
if (edgeType == OcctCurveType.Circle)
{
    OcctCircleGeometry circle = model.GetCircleGeometry(edge);
    Console.WriteLine($"R = {circle.Radius:F3}");
}

var faceType = model.GetSurfaceType(face);
if (faceType == OcctSurfaceType.Cylinder)
{
    OcctCylinderGeometry cylinder = model.GetCylinderGeometry(face);
    Console.WriteLine($"Axis = {cylinder.Axis}, R = {cylinder.Radius:F3}");
}
```

A type mismatch, non-edge/non-face input, or a shape from another session produces an argument or `OcctException`. These exact parameters support feature recognition, hole-axis extraction, dimensions, engineering rules, and parametric reconstruction.

"""
        insert_before = "### Geometry and feature modeling\n"

    if heading not in text:
        text = replace_once(text, insert_before, guide + insert_before, f"analytic usage guide in {path}")
    write(path, text)


update_inventory("docs/API_COVERAGE.md", False)
update_inventory("docs/API_COVERAGE.zh-CN.md", True)

readme = read("README.md")
readme = readme.replace("2.3.0", "2.4.0")
readme = replace_once(
    readme,
    "Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations reduce repeated P/Invoke calls for large scenes. Viewport-state snapshots, selected-object fitting, reset operations, scene gravity points, and screen-to-plane projection support reusable CAD interaction tools.\n",
    "Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations reduce repeated P/Invoke calls for large scenes. Viewport-state snapshots, selected-object fitting, reset operations, scene gravity points, and screen-to-plane projection support reusable CAD interaction tools. Exact line, circle, ellipse, plane, cylinder, cone, sphere, and torus parameters support feature recognition and engineering automation.\n",
    "English analytic summary")
write("README.md", readme)

readme_zh = read("README.zh-CN.md")
readme_zh = readme_zh.replace("2.3.0", "2.4.0")
readme_zh = replace_once(
    readme_zh,
    "新增批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择接口，减少大型场景中的重复 P/Invoke 调用。视口状态快照、适配选择集、视图重置、场景重心和屏幕投影到平面接口可直接支撑 CAD 交互工具。\n",
    "新增批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择接口，减少大型场景中的重复 P/Invoke 调用。视口状态快照、适配选择集、视图重置、场景重心和屏幕投影到平面接口可直接支撑 CAD 交互工具。新增直线、圆、椭圆、平面、圆柱、圆锥、球面和圆环面的精确解析参数读取，可用于特征识别与工程规则判断。\n",
    "Chinese analytic summary")
write("README.zh-CN.md", readme_zh)

(ROOT / ".github/apply_analytic_geometry_2_4.py").unlink()
