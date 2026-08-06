from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    target = ROOT / path
    target.write_bytes(BOM + text.replace("\r\n", "\n").replace("\n", "\r\n").encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


engine_path = "src/OcctNative/OcctEngine.cpp"
engine = read(engine_path)
old_point = """        return execute(e, [&] { e->context->MoveTo(x,y,e->view,Standard_False); e->context->SelectDetected(append ? AIS_SelectionScheme_XOR : AIS_SelectionScheme_Replace); e->view->Redraw(); });
"""
new_point = """        return execute(e, [&]
        {
            e->context->MoveTo(x, y, e->view, Standard_False);
            if (e->context->HasDetected())
            {
                e->context->SelectDetected(
                    append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            }
            else if (!append)
            {
                e->context->ClearSelected(Standard_False);
            }
            e->context->UpdateCurrentViewer();
        });
"""
engine = replace_once(engine, old_point, new_point, "point selection")
old_rectangle = """            // OCCT uses full inclusion for rectangle selection by default. Configure the
            // selector explicitly for every gesture so callers can request crossing selection.
            const Handle(StdSelect_ViewerSelector3d)& selector = e->context->MainSelector();
            selector->AllowOverlapDetection(allowOverlap != 0);
            Graphic3d_Vec2i minPoint(std::min(x1,x2), std::min(y1,y2));
            Graphic3d_Vec2i maxPoint(std::max(x1,x2), std::max(y1,y2));
            e->context->SelectRectangle(
                minPoint,
                maxPoint,
                e->view,
                append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            e->view->Redraw();
"""
new_rectangle = """            // Keep the standard OCCT rectangle-selection path used by the reference
            // Viewport examples. Full inclusion is the default; overlap is opt-in only.
            const Handle(StdSelect_ViewerSelector3d)& selector = e->context->MainSelector();
            selector->AllowOverlapDetection(allowOverlap != 0);
            const Graphic3d_Vec2i minPoint(std::min(x1, x2), std::min(y1, y2));
            const Graphic3d_Vec2i maxPoint(std::max(x1, x2), std::max(y1, y2));
            e->context->SelectRectangle(
                minPoint,
                maxPoint,
                e->view,
                append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            selector->AllowOverlapDetection(Standard_False);
            e->context->UpdateCurrentViewer();
"""
engine = replace_once(engine, old_rectangle, new_rectangle, "rectangle selection")
write(engine_path, engine)

overlay_path = "src/OcctNative/OcctSelectionOverlay.cpp"
overlay = read(overlay_path)
overlay = replace_once(
    overlay,
    "new Graphic3d_TransformPers(Graphic3d_TMF_2d, Aspect_TOTP_LEFT_UPPER));",
    "new Graphic3d_TransformPers(Graphic3d_TMF_2d, Aspect_TOTP_LEFT_LOWER));",
    "rubber-band anchor")
old_coordinates = """            const int minX = std::min(x1, x2);
            const int maxX = std::max(x1, x2);
            const int minY = std::min(y1, y2);
            const int maxY = std::max(y1, y2);
            engine->selectionRubberBand->SetRectangle(minX, -maxY, maxX, -minY);
"""
new_coordinates = """            const int minX = std::min(x1, x2);
            const int maxX = std::max(x1, x2);
            const int minClientY = std::min(y1, y2);
            const int maxClientY = std::max(y1, y2);
            Standard_Integer windowWidth = 0;
            Standard_Integer windowHeight = 0;
            engine->window->Size(windowWidth, windowHeight);
            if (windowHeight <= 0) throw std::runtime_error("The OCCT window height is invalid.");

            // AIS_RubberBand with LEFT_LOWER persistence uses a bottom-left Y origin,
            // while WinForms/WPF mouse coordinates use a top-left Y origin.
            const int minY = windowHeight - maxClientY;
            const int maxY = windowHeight - minClientY;
            engine->selectionRubberBand->SetRectangle(minX, minY, maxX, maxY);
"""
overlay = replace_once(overlay, old_coordinates, new_coordinates, "rubber-band coordinate conversion")
old_redraw = """            // The rubber band is an immediate top-layer presentation. Updating only that
            // layer avoids a full scene redraw on every mouse move and prevents flicker.
            engine->view->InvalidateImmediate();
            engine->view->RedrawImmediate();
"""
new_redraw = """            engine->view->Redraw();
"""
overlay = replace_once(overlay, old_redraw, new_redraw, "rubber-band redraw")
overlay = replace_once(
    overlay,
    """            engine->selectionRubberBand->ClearPoints();
            engine->view->InvalidateImmediate();
            engine->view->RedrawImmediate();
""",
    """            engine->selectionRubberBand->ClearPoints();
            engine->view->Redraw();
""",
    "rubber-band hide redraw")
write(overlay_path, overlay)

control_path = "src/OcctNet.WinForms/OcctViewportControl.cs"
control = read(control_path)
control = replace_once(
    control,
    "public int RectangleSelectionThreshold { get; set; } = 5;",
    "public int RectangleSelectionThreshold { get; set; } = 3;",
    "selection threshold")
control = replace_once(
    control,
    "public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; } = OcctRectangleSelectionBehavior.Overlap;",
    "public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; } = OcctRectangleSelectionBehavior.Inclusive;",
    "selection default")
control = replace_once(
    control,
    "else if (e.Button == MouseButtons.Left)\n        {",
    "else if (e.Button == MouseButtons.Left && !ModifierKeys.HasFlag(Keys.Shift))\n        {",
    "left-button gesture")
old_distance = """        var dx = Math.Abs(end.X - _selectionStart.X);
        var dy = Math.Abs(end.Y - _selectionStart.Y);
        var useRectangle = EnableRectangleSelection
                           && (_rectangleDragStarted
                               || dx >= RectangleSelectionThreshold
                               || dy >= RectangleSelectionThreshold);
"""
new_distance = """        var dragDistance = Math.Abs(end.X - _selectionStart.X)
                           + Math.Abs(end.Y - _selectionStart.Y);
        var useRectangle = EnableRectangleSelection
                           && (_rectangleDragStarted
                               || dragDistance > RectangleSelectionThreshold);
"""
control = replace_once(control, old_distance, new_distance, "Manhattan drag threshold")
control = replace_once(
    control,
    "if (!Capture && !_releasingMouseCapture)\n        {",
    "if (!Capture && !_releasingMouseCapture && !_selectingRectangle)\n        {",
    "capture-change handling")
write(control_path, control)

build_path = "build.ps1"
build = read(build_path)
build = replace_once(
    build,
    '$NativeBuildCheck = Join-Path $RepoRoot "tests\\check-native-build-structure.ps1"\n',
    '$NativeBuildCheck = Join-Path $RepoRoot "tests\\check-native-build-structure.ps1"\n$SelectionContractCheck = Join-Path $RepoRoot "tests\\check-selection-contract.ps1"\n',
    "selection check variable")
build = replace_once(
    build,
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck

    Write-Host "[native-build] Validating CMake sources and toolkit boundaries..." -ForegroundColor Cyan
""",
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck
    Assert-Path $SelectionContractCheck

    Write-Host "[selection] Validating point and rectangle selection behavior..." -ForegroundColor Cyan
    & $SelectionContractCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Selection contract validation failed."
    }

    Write-Host "[native-build] Validating CMake sources and toolkit boundaries..." -ForegroundColor Cyan
""",
    "selection validation invocation")
write(build_path, build)

selection_test = r'''param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Text {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path $Path -PathType Leaf)) {
        throw "Required selection file was not found: $Path"
    }
    return [System.IO.File]::ReadAllText($Path)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Token,
        [Parameter(Mandatory = $true)][string]$Description
    )
    if (-not $Text.Contains($Token)) {
        throw "Selection contract is missing $Description."
    }
}

$engine = Read-Text (Join-Path $RepositoryRoot "src\OcctNative\OcctEngine.cpp")
$overlay = Read-Text (Join-Path $RepositoryRoot "src\OcctNative\OcctSelectionOverlay.cpp")
$control = Read-Text (Join-Path $RepositoryRoot "src\OcctNet.WinForms\OcctViewportControl.cs")

Assert-Contains $engine 'AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace' 'add/replace selection schemes'
Assert-Contains $engine 'SelectRectangle(' 'the standard OCCT SelectRectangle call'
Assert-Contains $engine 'UpdateCurrentViewer();' 'viewer updates after selection'
if ($engine.Contains('AIS_SelectionScheme_XOR')) {
    throw 'Point selection must not use XOR; Ctrl selection follows the reference Add behavior.'
}

Assert-Contains $overlay 'Aspect_TOTP_LEFT_LOWER' 'the reference lower-left rubber-band anchor'
Assert-Contains $overlay 'windowHeight - maxClientY' 'top-left to bottom-left Y conversion'
Assert-Contains $overlay 'windowHeight - minClientY' 'top-left to bottom-left Y conversion'
if ($overlay.Contains('Aspect_TOTP_LEFT_UPPER') -or $overlay.Contains('SetRectangle(minX, -maxY')) {
    throw 'Legacy inverted rubber-band coordinates remain.'
}

Assert-Contains $control 'RectangleSelectionThreshold { get; set; } = 3;' 'the three-pixel reference threshold'
Assert-Contains $control 'OcctRectangleSelectionBehavior.Inclusive' 'inclusive rectangle selection as the default'
Assert-Contains $control 'dragDistance > RectangleSelectionThreshold' 'Manhattan-distance gesture classification'
Assert-Contains $control '!ModifierKeys.HasFlag(Keys.Shift)' 'Shift exclusion for left-button box selection'

Write-Host '[selection] Point selection, box-selection coordinates, and default behavior validated.' -ForegroundColor Green
'''
write("tests/check-selection-contract.ps1", selection_test)

(ROOT / ".github/apply_box_selection_fix.py").unlink()
(ROOT / ".github/workflows/apply-box-selection-fix.yml").unlink()
