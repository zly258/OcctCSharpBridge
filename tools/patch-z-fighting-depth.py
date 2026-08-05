from pathlib import Path


def replace_exact(path: str, old: str, new: str, label: str) -> None:
    file = Path(path)
    text = file.read_text(encoding="utf-8-sig")
    if old in text:
        file.write_text(text.replace(old, new), encoding="utf-8", newline="\n")
        print(f"Updated {label}.")
    elif new in text:
        print(f"{label} already updated.")
    else:
        raise SystemExit(f"Expected anchor for {label} not found in {path}.")


replace_exact(
    "src/OcctNative/OcctNative.h",
    "    struct OcctCameraState { OcctPoint3d eye; OcctPoint3d center; OcctVector3d up; OcctVector3d direction; double scale; };\n",
    "    struct OcctCameraState { OcctPoint3d eye; OcctPoint3d center; OcctVector3d up; OcctVector3d direction; double scale; };\n"
    "    struct OcctAutoZFitSettings { int enabled; double scaleFactor; };\n"
    "    struct OcctPolygonOffsetSettings { int mode; double factor; double units; };\n",
    "native depth settings structures",
)

replace_exact(
    "src/OcctNative/OcctNative.h",
    "    OCCTBRIDGE_API int occt_set_selection_tolerance(OcctHandle handle, int pixelTolerance);\n",
    "    OCCTBRIDGE_API int occt_set_selection_tolerance(OcctHandle handle, int pixelTolerance);\n"
    "    OCCTBRIDGE_API int occt_set_auto_z_fit_mode(OcctHandle handle, int enabled, double scaleFactor);\n"
    "    OCCTBRIDGE_API int occt_get_auto_z_fit_mode(OcctHandle handle, OcctAutoZFitSettings* result);\n"
    "    OCCTBRIDGE_API int occt_auto_z_fit(OcctHandle handle);\n"
    "    OCCTBRIDGE_API int occt_set_default_polygon_offsets(OcctHandle handle, int mode, double factor, double units, int applyExisting);\n"
    "    OCCTBRIDGE_API int occt_get_default_polygon_offsets(OcctHandle handle, OcctPolygonOffsetSettings* result);\n"
    "    OCCTBRIDGE_API int occt_set_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, int mode, double factor, double units);\n"
    "    OCCTBRIDGE_API int occt_get_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId, OcctPolygonOffsetSettings* result);\n"
    "    OCCTBRIDGE_API int occt_reset_object_polygon_offsets(OcctHandle handle, OcctObjectId objectId);\n",
    "native depth control declarations",
)

replace_exact(
    "src/OcctNative/CMakeLists.txt",
    "    OcctView.cpp\n    OcctQueries.cpp\n",
    "    OcctView.cpp\n    OcctDepth.cpp\n    OcctQueries.cpp\n",
    "native depth source registration",
)

replace_exact(
    "src/OcctNative/OcctEngine.cpp",
    "#include <AIS_SelectionScheme.hxx>\n",
    "#include <AIS_SelectionScheme.hxx>\n#include <Aspect_PolygonOffsetMode.hxx>\n",
    "polygon offset include",
)

replace_exact(
    "src/OcctNative/OcctEngine.cpp",
    "#include <Graphic3d_Camera.hxx>\n",
    "#include <Graphic3d_Camera.hxx>\n#include <Graphic3d_AspectFillArea3d.hxx>\n",
    "fill aspect include",
)

replace_exact(
    "src/OcctNative/OcctEngine.cpp",
    "            engine->context = new AIS_InteractiveContext(engine->viewer);\n            engine->view = engine->viewer->CreateView();\n",
    "            engine->context = new AIS_InteractiveContext(engine->viewer);\n"
    "            engine->view = engine->viewer->CreateView();\n"
    "            engine->view->SetAutoZFitMode(Standard_True, 1.0);\n"
    "            const Handle(Prs3d_Drawer)& defaultDrawer = engine->context->DefaultDrawer();\n"
    "            defaultDrawer->SetupOwnShadingAspect();\n"
    "            defaultDrawer->ShadingAspect()->Aspect()->SetPolygonOffsets(\n"
    "                Aspect_POM_Fill, 1.0f, 1.0f);\n",
    "predictable default Z fitting and polygon offset",
)

english = Path("docs/VIEWER_AND_DISPLAY.md")
english_text = english.read_text(encoding="utf-8-sig")
english_section = r'''

## Depth precision and coplanar objects

The Viewer uses two separate mechanisms:

- `SetAutoZFitMode()` and `AutoZFit()` adjust the camera near/far Z range. This improves depth-buffer precision and avoids clipping, but cannot distinguish two surfaces at exactly the same depth.
- Polygon offsets apply a render-time depth bias to a specific AIS object. Use this for previews, overlays, reference faces, or other objects intentionally displayed coplanar with another object.

```csharp
engine.SetAutoZFitMode(true, 1.0);
engine.AutoZFit();

var reference = engine.MakePlaneFace(100, 80);
var overlay = engine.MakePlaneFace(100, 80);

// Negative values move the overlay toward the viewport.
engine.SetPolygonOffsets(
    overlay,
    OcctPolygonOffsetMode.Fill,
    factor: -1.0,
    units: -1.0);

// Restore the current Viewer default, normally Fill / 1 / 1.
engine.ResetPolygonOffsets(overlay);
```

Do not assign the same custom offset to both coplanar objects; their depth relationship would remain ambiguous. Duplicate production geometry should still be removed or hidden. Polygon offsets are intended for deliberate visual layering, not for repairing invalid topology.
'''
if "## Depth precision and coplanar objects" not in english_text:
    english.write_text(english_text.rstrip() + english_section + "\n", encoding="utf-8", newline="\n")

chinese = Path("docs/VIEWER_AND_DISPLAY.zh-CN.md")
chinese_text = chinese.read_text(encoding="utf-8-sig")
chinese_section = r'''

## 深度精度与共面对象

Viewer 中应区分两种机制：

- `SetAutoZFitMode()` 与 `AutoZFit()` 调整相机近、远 Z 范围，用于提高深度缓冲精度和避免裁剪，但无法区分两个深度完全相同的面。
- Polygon Offset 对指定 AIS 对象施加渲染深度偏移，适用于预览、覆盖面、参考面以及其他有意共面显示的对象。

```csharp
engine.SetAutoZFitMode(true, 1.0);
engine.AutoZFit();

var reference = engine.MakePlaneFace(100, 80);
var overlay = engine.MakePlaneFace(100, 80);

// 负值会让覆盖对象在深度上更靠近视口。
engine.SetPolygonOffsets(
    overlay,
    OcctPolygonOffsetMode.Fill,
    factor: -1.0,
    units: -1.0);

// 恢复当前 Viewer 默认值，通常为 Fill / 1 / 1。
engine.ResetPolygonOffsets(overlay);
```

不要给两个共面对象设置完全相同的自定义偏移，否则二者的深度关系仍然不明确。正式模型中的重复几何仍应删除或隐藏；Polygon Offset 用于有意的视觉分层，不用于修复无效拓扑。
'''
if "## 深度精度与共面对象" not in chinese_text:
    chinese.write_text(chinese_text.rstrip() + chinese_section + "\n", encoding="utf-8", newline="\n")
