from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    normalized = text.replace("\r\n", "\n").replace("\n", "\r\n")
    target.write_bytes(BOM + normalized.encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


# Native C ABI declarations.
header_path = "src/OcctNative/OcctNative.h"
header = read(header_path)
header = replace_once(
    header,
    "    struct OcctCameraState { OcctPoint3d eye; OcctPoint3d center; OcctVector3d up; OcctVector3d direction; double scale; };\n",
    "    struct OcctCameraState { OcctPoint3d eye; OcctPoint3d center; OcctVector3d up; OcctVector3d direction; double scale; };\n"
    "    struct OcctProjectionRay { OcctPoint3d origin; OcctVector3d direction; };\n",
    "projection ray data type")
header = replace_once(
    header,
    "    enum OcctDisplayMode { OcctDisplay_Wireframe = 0, OcctDisplay_Shaded = 1 };\n",
    "    enum OcctDisplayMode { OcctDisplay_Wireframe = 0, OcctDisplay_Shaded = 1 };\n"
    "    enum OcctRenderingMethod { OcctRendering_Rasterization = 0, OcctRendering_RayTracing = 1 };\n"
    "    enum OcctZUpViewOrientation\n"
    "    {\n"
    "        OcctZUp_Front = 0, OcctZUp_Back = 1, OcctZUp_Left = 2, OcctZUp_Right = 3,\n"
    "        OcctZUp_Top = 4, OcctZUp_Bottom = 5,\n"
    "        OcctZUp_XNegativeYNegative = 6, OcctZUp_XPositiveYNegative = 7,\n"
    "        OcctZUp_XNegativeYPositive = 8, OcctZUp_XPositiveYPositive = 9\n"
    "    };\n",
    "viewport enums")
viewport_declarations = """    OCCTBRIDGE_API int occt_fit_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, double margin);
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

"""
header = replace_once(
    header,
    "    // Registry, AIS attributes and lifecycle.\n",
    viewport_declarations + "    // Registry, AIS attributes and lifecycle.\n",
    "viewport declarations")
write(header_path, header)

# Native implementation isolated from the existing engine/view source files.
write("src/OcctNative/OcctViewportExtensions.cpp", r'''#include "OcctInternal.hxx"

#include <BRepBndLib.hxx>
#include <Bnd_Box.hxx>
#include <Graphic3d_RenderingParams.hxx>
#include <Prs3d_Drawer.hxx>
#include <V3d_TypeOfOrientation.hxx>

using namespace OcctBridge;

namespace
{
    V3d_TypeOfOrientation zUpOrientation(int value)
    {
        switch (value)
        {
            case OcctZUp_Front: return V3d_TypeOfOrientation_Zup_Front;
            case OcctZUp_Back: return V3d_TypeOfOrientation_Zup_Back;
            case OcctZUp_Left: return V3d_TypeOfOrientation_Zup_Left;
            case OcctZUp_Right: return V3d_TypeOfOrientation_Zup_Right;
            case OcctZUp_Top: return V3d_TypeOfOrientation_Zup_Top;
            case OcctZUp_Bottom: return V3d_TypeOfOrientation_Zup_Bottom;
            case OcctZUp_XNegativeYNegative: return V3d_XnegYnegZpos;
            case OcctZUp_XPositiveYNegative: return V3d_XposYnegZpos;
            case OcctZUp_XNegativeYPositive: return V3d_XnegYposZpos;
            case OcctZUp_XPositiveYPositive: return V3d_XposYposZpos;
            default: throw std::invalid_argument("Z-up view orientation is out of range.");
        }
    }

    void validateMargin(double margin)
    {
        if (margin < 0.0 || margin >= 1.0)
            throw std::invalid_argument("Fit margin must be in the range [0, 1).");
    }
}

extern "C"
{
    int occt_fit_objects(OcctHandle h, const OcctObjectId* objectIds, int count, double margin)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");
            requireCount(count, 1, "Object ID array");
            validateMargin(margin);

            Bnd_Box bounds;
            for (int index = 0; index < count; ++index)
            {
                const ObjectEntry* entry = e->findShape(objectIds[index]);
                if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
                BRepBndLib::Add(entry->shape, bounds);
            }
            if (bounds.IsVoid()) throw std::runtime_error("Selected shapes have no finite bounds.");
            e->view->FitAll(bounds, margin, Standard_False);
            e->view->ZFitAll();
            e->view->Redraw();
        });
    }

    int occt_set_zup_view(OcctHandle h, int orientation, int fitAll)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->view->SetProj(zUpOrientation(orientation));
            if (fitAll != 0)
            {
                e->view->FitAll(0.01, Standard_False);
                e->view->ZFitAll();
            }
            e->view->Redraw();
        });
    }

    int occt_screen_to_ray(OcctHandle h, int x, int y, OcctProjectionRay* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            Standard_Real px = 0.0, py = 0.0, pz = 0.0;
            Standard_Real vx = 0.0, vy = 0.0, vz = 0.0;
            e->view->ConvertWithProj(x, y, px, py, pz, vx, vy, vz);
            const gp_Dir rayDirection(gp_Vec(vx, vy, vz));
            result->origin = {px, py, pz};
            result->direction = {rayDirection.X(), rayDirection.Y(), rayDirection.Z()};
        });
    }

    int occt_zoom_at_point(OcctHandle h, int x, int y, double delta)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(delta) || std::abs(delta) <= Precision::Confusion())
                throw std::invalid_argument("Zoom delta must be finite and non-zero.");
            e->view->StartZoomAtPoint(x, y);
            e->view->ZoomAtPoint(0, 0, delta, 0);
        });
    }

    int occt_select_all_visible(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->context->ClearSelected(Standard_False);
            for (const auto& pair : e->objects)
            {
                if (!pair.second.presentation.IsNull() && e->context->IsDisplayed(pair.second.presentation))
                    e->context->AddSelect(pair.second.presentation);
            }
            e->context->HilightSelected(Standard_False);
            e->context->UpdateCurrentViewer();
        });
    }

    int occt_invert_selection(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            for (const auto& pair : e->objects)
            {
                if (!pair.second.presentation.IsNull() && e->context->IsDisplayed(pair.second.presentation))
                    e->context->AddOrRemoveSelected(pair.second.presentation, Standard_False);
            }
            e->context->HilightSelected(Standard_False);
            e->context->UpdateCurrentViewer();
        });
    }

    int occt_hide_selected(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            std::vector<Handle(AIS_InteractiveObject)> selected;
            for (e->context->InitSelected(); e->context->MoreSelected(); e->context->NextSelected())
            {
                const Handle(AIS_InteractiveObject) value = e->context->SelectedInteractive();
                if (!value.IsNull()) selected.push_back(value);
            }
            for (const auto& value : selected) e->context->Erase(value, Standard_False);
            e->context->ClearSelected(Standard_False);
            e->context->UpdateCurrentViewer();
        });
    }

    int occt_set_automatic_highlight(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->context->SetAutomaticHilight(enabled != 0); });
    }

    int occt_set_msaa_samples(OcctHandle h, int samples)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (samples < 0 || samples > 16) throw std::invalid_argument("MSAA sample count must be between 0 and 16.");
            e->view->ChangeRenderingParams().NbMsaaSamples = samples;
            e->view->Redraw();
        });
    }

    int occt_set_render_resolution_scale(OcctHandle h, double scale)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(scale) || scale < 0.25 || scale > 4.0)
                throw std::invalid_argument("Render resolution scale must be between 0.25 and 4.0.");
            e->view->ChangeRenderingParams().RenderResolutionScale = static_cast<Standard_ShortReal>(scale);
            e->view->Redraw();
        });
    }

    int occt_set_render_resolution(OcctHandle h, double dpi)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (!std::isfinite(dpi) || dpi < 36.0 || dpi > 600.0)
                throw std::invalid_argument("Render resolution must be between 36 and 600 DPI.");
            e->view->ChangeRenderingParams().Resolution = static_cast<unsigned int>(std::lround(dpi));
            e->view->Redraw();
        });
    }

    int occt_set_rendering_method(OcctHandle h, int method)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (method != OcctRendering_Rasterization && method != OcctRendering_RayTracing)
                throw std::invalid_argument("Rendering method is out of range.");
            e->view->ChangeRenderingParams().Method = method == OcctRendering_RayTracing
                ? Graphic3d_RM_RAYTRACING
                : Graphic3d_RM_RASTERIZATION;
            e->view->Redraw();
        });
    }

    int occt_set_shadows_enabled(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->view->ChangeRenderingParams().IsShadowEnabled = enabled != 0;
            e->view->Redraw();
        });
    }

    int occt_set_immediate_update(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->view->SetImmediateUpdate(enabled != 0); });
    }

    int occt_set_frustum_culling(OcctHandle h, int enabled)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->view->SetFrustumCulling(enabled != 0); e->view->Redraw(); });
    }

    int occt_set_face_boundaries_visible(OcctHandle h, int visible, int applyExisting)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->context->DefaultDrawer()->SetFaceBoundaryDraw(visible != 0);
            if (applyExisting != 0)
            {
                for (auto& pair : e->objects)
                {
                    if (pair.second.kind != OcctObject_Shape || pair.second.presentation.IsNull()) continue;
                    pair.second.presentation->Attributes()->SetFaceBoundaryDraw(visible != 0);
                    e->context->Redisplay(pair.second.presentation, Standard_False, Standard_True);
                }
            }
            e->view->Redraw();
        });
    }
}
''')

cmake_path = "src/OcctNative/CMakeLists.txt"
cmake = read(cmake_path)
cmake = replace_once(cmake, "    OcctView.cpp\n", "    OcctView.cpp\n    OcctViewportExtensions.cpp\n", "native viewport source")
write(cmake_path, cmake)

# Managed declarations and public wrappers.
native_methods_path = "src/OcctNet/NativeMethods.cs"
native_methods = read(native_methods_path)
native_methods = replace_once(native_methods, "internal static class NativeMethods\n", "internal static partial class NativeMethods\n", "partial native methods")
write(native_methods_path, native_methods)

write("src/OcctNet/ViewportNativeMethods.cs", r'''using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_fit_objects(IntPtr handle, [In] long[] objectIds, int count, double margin);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_zup_view(IntPtr handle, int orientation, int fitAll);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_screen_to_ray(IntPtr handle, int x, int y, out OcctProjectionRay result);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_zoom_at_point(IntPtr handle, int x, int y, double delta);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_select_all_visible(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_invert_selection(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_hide_selected(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_automatic_highlight(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_msaa_samples(IntPtr handle, int samples);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_render_resolution_scale(IntPtr handle, double scale);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_render_resolution(IntPtr handle, double dpi);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_rendering_method(IntPtr handle, int method);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_shadows_enabled(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_immediate_update(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_frustum_culling(IntPtr handle, int enabled);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_set_face_boundaries_visible(IntPtr handle, int visible, int applyExisting);
}
''')

write("src/OcctNet/OcctViewportTypes.cs", r'''using System.Runtime.InteropServices;

namespace OcctNet;

public enum OcctRenderingMethod
{
    Rasterization = 0,
    RayTracing = 1
}

public enum OcctZUpViewOrientation
{
    Front = 0,
    Back = 1,
    Left = 2,
    Right = 3,
    Top = 4,
    Bottom = 5,
    IsometricXNegativeYNegative = 6,
    IsometricXPositiveYNegative = 7,
    IsometricXNegativeYPositive = 8,
    IsometricXPositiveYPositive = 9
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctProjectionRay
{
    public OcctPoint3d Origin;
    public OcctVector3d Direction;

    public OcctProjectionRay(OcctPoint3d origin, OcctVector3d direction)
    {
        Origin = origin;
        Direction = direction;
    }
}
''')

write("src/OcctNet/OcctEngine.Viewport.cs", r'''namespace OcctNet;

public sealed partial class OcctEngine
{
    public void Fit(IEnumerable<OcctShape> shapes, double margin = 0.05)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        var ids = shapes.Select(shape => shape.Id).Where(id => id > 0).Distinct().ToArray();
        if (ids.Length == 0) throw new ArgumentException("At least one valid shape is required.", nameof(shapes));
        CheckInitialized(() => NativeMethods.occt_fit_objects(_handle, ids, ids.Length, margin));
    }

    public void SetZUpView(OcctZUpViewOrientation orientation, bool fitAll = true) =>
        CheckInitialized(() => NativeMethods.occt_set_zup_view(_handle, (int)orientation, fitAll ? 1 : 0));

    public OcctProjectionRay ScreenToRay(int x, int y)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_screen_to_ray(_handle, x, y, out var result));
        return result;
    }

    public void ZoomAtPoint(int x, int y, double delta) =>
        CheckInitialized(() => NativeMethods.occt_zoom_at_point(_handle, x, y, delta));

    public void SelectAllVisible() => CheckInitialized(() => NativeMethods.occt_select_all_visible(_handle));
    public void InvertSelection() => CheckInitialized(() => NativeMethods.occt_invert_selection(_handle));
    public void HideSelected() => CheckInitialized(() => NativeMethods.occt_hide_selected(_handle));
    public void SetAutomaticHighlight(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_automatic_highlight(_handle, enabled ? 1 : 0));

    public void SetMsaaSamples(int samples) => CheckInitialized(() => NativeMethods.occt_set_msaa_samples(_handle, samples));
    public void SetRenderResolutionScale(double scale) => CheckInitialized(() => NativeMethods.occt_set_render_resolution_scale(_handle, scale));
    public void SetRenderResolution(double dpi) => CheckInitialized(() => NativeMethods.occt_set_render_resolution(_handle, dpi));
    public void SetRenderingMethod(OcctRenderingMethod method) => CheckInitialized(() => NativeMethods.occt_set_rendering_method(_handle, (int)method));
    public void SetShadowsEnabled(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_shadows_enabled(_handle, enabled ? 1 : 0));
    public void SetImmediateUpdate(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_immediate_update(_handle, enabled ? 1 : 0));
    public void SetFrustumCulling(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_frustum_culling(_handle, enabled ? 1 : 0));
    public void SetFaceBoundariesVisible(bool visible, bool applyExisting = true) =>
        CheckInitialized(() => NativeMethods.occt_set_face_boundaries_visible(_handle, visible ? 1 : 0, applyExisting ? 1 : 0));
}
''')

# Prefer the packaged runtime over developer-machine environment variables and remove OCAF/XDE resources.
runtime_path = "src/OcctNet/OcctRuntime.cs"
runtime = read(runtime_path)
runtime = replace_once(
    runtime,
    """                     explicitDirectory,
                     Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR"),
                     AppContext.BaseDirectory,
                     Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native"),
                     portableRuntimeDirectory
""",
    """                     explicitDirectory,
                     portableRuntimeDirectory,
                     Environment.GetEnvironmentVariable("OCCT_BRIDGE_NATIVE_DIR"),
                     AppContext.BaseDirectory,
                     Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native")
""",
    "portable native precedence")
runtime = replace_once(
    runtime,
    """                     explicitRoot,
                     Environment.GetEnvironmentVariable("OCCT_ROOT"),
                     Environment.GetEnvironmentVariable("CASROOT"),
                     portableOcctRoot,
                     DefaultOcctRoot
""",
    """                     explicitRoot,
                     portableOcctRoot,
                     Environment.GetEnvironmentVariable("OCCT_ROOT"),
                     Environment.GetEnvironmentVariable("CASROOT"),
                     DefaultOcctRoot
""",
    "portable OCCT precedence")
for obsolete in (
    '        SetDirectoryIfExists("CSF_TObjMessage", resourceDirectory, "TObj");\n',
    '        SetDirectoryIfExists("CSF_XCAFDefaults", resourceDirectory, "XCAFResources");\n',
    '        SetDirectoryIfExists("CSF_XmlOcafResource", resourceDirectory, "XmlOcafResource");\n',
):
    runtime = runtime.replace(obsolete, "")
write(runtime_path, runtime)

# Backward-compatible feature release: same ABI, new exports.
bridge_info_path = "src/OcctNet/OcctBridgeInfo.cs"
bridge_info = read(bridge_info_path).replace('public const string ManagedVersion = "2.0.0";', 'public const string ManagedVersion = "2.1.0";')
write(bridge_info_path, bridge_info)
engine_path = "src/OcctNative/OcctEngine.cpp"
engine = read(engine_path).replace('const char* occt_bridge_version() { return "2.0.0"; }', 'const char* occt_bridge_version() { return "2.1.0"; }')
engine = engine.replace('std::string("OcctCSharpBridge/2.0.0; ABI=2; OCCT=")', 'std::string("OcctCSharpBridge/2.1.0; ABI=2; OCCT=")')
write(engine_path, engine)

# Permanent API contract guard.
write("tests/check-viewport-api.ps1", r'''param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$required = [ordered]@{
    "src/OcctNative/OcctViewportExtensions.cpp" = @(
        "occt_fit_objects", "occt_set_zup_view", "occt_screen_to_ray", "occt_zoom_at_point",
        "occt_select_all_visible", "occt_invert_selection", "occt_hide_selected",
        "occt_set_msaa_samples", "occt_set_rendering_method", "occt_set_face_boundaries_visible"
    )
    "src/OcctNet/OcctEngine.Viewport.cs" = @(
        "Fit(IEnumerable<OcctShape>", "SetZUpView", "ScreenToRay", "ZoomAtPoint",
        "SelectAllVisible", "InvertSelection", "HideSelected", "SetMsaaSamples",
        "SetRenderingMethod", "SetFaceBoundariesVisible"
    )
    "src/OcctNet/OcctRuntime.cs" = @(
        "portableRuntimeDirectory", "portableOcctRoot"
    )
}

foreach ($entry in $required.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $entry.Key
    if (-not (Test-Path $path -PathType Leaf)) { throw "Viewport API file is missing: $($entry.Key)" }
    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in $entry.Value) {
        if (-not $text.Contains($token)) { throw "Viewport API token is missing: $token ($($entry.Key))" }
    }
}

$runtime = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctRuntime.cs"))
foreach ($forbidden in @("CSF_TObjMessage", "CSF_XCAFDefaults", "CSF_XmlOcafResource")) {
    if ($runtime.Contains($forbidden)) { throw "OCAF/XDE runtime configuration remains: $forbidden" }
}

Write-Host "[viewport] Extended view, selection, rendering, and portable-runtime contracts validated." -ForegroundColor Green
''')

build_path = "build.ps1"
build = read(build_path)
build = replace_once(
    build,
    '$SelectionContractCheck = Join-Path $RepoRoot "tests\\check-selection-contract.ps1"\n',
    '$SelectionContractCheck = Join-Path $RepoRoot "tests\\check-selection-contract.ps1"\n$ViewportApiCheck = Join-Path $RepoRoot "tests\\check-viewport-api.ps1"\n',
    "viewport contract path")
build = replace_once(
    build,
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck

""",
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck
    Assert-Path $ViewportApiCheck

    Write-Host "[viewport] Validating extended viewport contracts..." -ForegroundColor Cyan
    & $ViewportApiCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Viewport API validation failed."
    }

""",
    "viewport contract invocation")
write(build_path, build)

# Keep the two source-derived inventories accurate.
new_exports = [
    "occt_fit_objects", "occt_set_zup_view", "occt_screen_to_ray", "occt_zoom_at_point",
    "occt_select_all_visible", "occt_invert_selection", "occt_hide_selected", "occt_set_automatic_highlight",
    "occt_set_msaa_samples", "occt_set_render_resolution_scale", "occt_set_render_resolution",
    "occt_set_rendering_method", "occt_set_shadows_enabled", "occt_set_immediate_update",
    "occt_set_frustum_culling", "occt_set_face_boundaries_visible"
]
for doc_path in ("docs/API_COVERAGE.md", "docs/API_COVERAGE.zh-CN.md"):
    doc = read(doc_path)
    doc = re.sub(r"Native exports:\s*`281`", "Native exports: `297`", doc)
    doc = re.sub(r"Managed P/Invoke declarations:\s*`281`", "Managed P/Invoke declarations: `297`", doc)
    doc = re.sub(r"Public \.NET types:\s*`56`", "Public .NET types: `59`", doc)
    doc = doc.replace("Viewer and interaction (56)", "Viewer and interaction (72)")
    doc = doc.replace("查看器与交互（56）", "查看器与交互（72）")
    anchor = "- `occt_world_to_screen`\n"
    addition = "".join(f"- `{name}`\n" for name in new_exports)
    if anchor in doc:
        doc = doc.replace(anchor, anchor + addition, 1)
    native_type_anchor = "- `OcctProjectionType`\n"
    if native_type_anchor in doc:
        doc = doc.replace(native_type_anchor, native_type_anchor + "- `OcctProjectionRay`\n- `OcctRenderingMethod`\n", 1)
    view_type_anchor = "- `OcctViewOrientation`\n"
    if view_type_anchor in doc:
        doc = doc.replace(view_type_anchor, view_type_anchor + "- `OcctZUpViewOrientation`\n", 1)
    public_anchor = "- `OcctProjectionType`\n"
    public_index = doc.find("## Public .NET types")
    if public_index < 0:
        public_index = doc.find("## 公共 .NET 类型")
    if public_index >= 0:
        before = doc[:public_index]
        after = doc[public_index:]
        after = after.replace(public_anchor, public_anchor + "- `OcctProjectionRay`\n- `OcctRenderingMethod`\n", 1)
        after = after.replace(view_type_anchor, view_type_anchor + "- `OcctZUpViewOrientation`\n", 1)
        doc = before + after
    doc = doc.replace("Native bridge version: `2.0.0`", "Native bridge version: `2.1.0`")
    doc = doc.replace("原生桥接版本：`2.0.0`", "原生桥接版本：`2.1.0`")
    write(doc_path, doc)

print("Viewport API 2.1 migration applied.")
