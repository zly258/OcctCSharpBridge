param(
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
$selectionState = Read-Text (Join-Path $RepositoryRoot "src\OcctNative\OcctSelectionState.cpp")
$selectionHeader = Read-Text (Join-Path $RepositoryRoot "src\OcctNative\OcctSelectionState.h")
$overlay = Read-Text (Join-Path $RepositoryRoot "src\OcctNative\OcctSelectionOverlay.cpp")
$control = Read-Text (Join-Path $RepositoryRoot "src\OcctNet.WinForms\OcctViewportControl.cs")
$managedHits = Read-Text (Join-Path $RepositoryRoot "src\OcctNet\OcctEngine.SelectionHits.cs")
$managedTypes = Read-Text (Join-Path $RepositoryRoot "src\OcctNet\OcctSelectionHitTypes.cs")
$managedNative = Read-Text (Join-Path $RepositoryRoot "src\OcctNet\NativeMethods.SelectionHits.cs")

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

Assert-Contains $selectionHeader 'struct OcctSelectionHit' 'the structured native selection-hit DTO'
Assert-Contains $selectionHeader 'occt_selected_hits' 'the batched selected-hit declaration'
Assert-Contains $selectionHeader 'occt_detected_hit' 'the detected-hit declaration'
Assert-Contains $selectionHeader 'ordering as occt_get_subshape' 'the subshape-index ordering contract'
if ($selectionHeader.Contains('hasPoint') -or $selectionHeader.Contains('OcctPoint3d point')) {
    throw 'Selection hit ABI must not expose an unimplemented hit-point field.'
}

Assert-Contains $selectionState 'StdSelect_BRepOwner' 'BRep entity-owner extraction'
Assert-Contains $selectionState 'TopExp_Explorer' 'subshape indexing compatible with GetSubshapeAt'
Assert-Contains $selectionState 'IsSame(selected)' 'topological identity matching'
Assert-Contains $selectionState 'collectSelectedHits' 'single-pass selected-hit collection'
Assert-Contains $selectionState 'occt_selected_hits' 'batched selected-hit implementation'
Assert-Contains $selectionState 'occt_detected_hit' 'detected-hit implementation'
Assert-Contains $selectionState 'subshapeIndex = -1' 'object-level selection sentinel'
Assert-Contains $selectionState 'SelectedOwner()' 'selected entity-owner lookup'
Assert-Contains $selectionState 'DetectedOwner()' 'detected entity-owner lookup'
if ($selectionState.Contains('occt_selected_hit_count') -or $selectionState.Contains('occt_selected_hit_at')) {
    throw 'Legacy N+1 selected-hit ABI must not be reintroduced.'
}

Assert-Contains $managedTypes 'public readonly record struct OcctSelectionHit' 'the public managed selection-hit type'
Assert-Contains $managedTypes 'public bool IsSubshape => SubshapeIndex >= 0;' 'managed subshape sentinel semantics'
if ($managedTypes.Contains('Point)') -or $managedTypes.Contains('OcctPoint3d? Point')) {
    throw 'Managed selection hit must not expose an unimplemented hit-point property.'
}
Assert-Contains $managedNative 'occt_selected_hits' 'batched selected-hit P/Invoke'
Assert-Contains $managedNative 'occt_detected_hit' 'detected-hit P/Invoke'
Assert-Contains $managedHits 'public IReadOnlyList<OcctSelectionHit> GetSelectedHits()' 'managed selected-hit API'
Assert-Contains $managedHits 'public bool TryGetDetectedHit(out OcctSelectionHit hit)' 'managed detected-hit API'
Assert-Contains $managedHits 'Check(NativeMethods.occt_selected_hits(_handle, null, 0, out var count));' 'two-call selected-hit count query'
Assert-Contains $managedHits 'Check(NativeMethods.occt_detected_hit(_handle, out var native, out var hasHit));' 'normal native error propagation'
Assert-Contains $managedHits 'GetObject(native.OwnerObjectId)' 'managed owner-object resolution'

Assert-Contains $control 'RectangleSelectionThreshold { get; set; } = 3;' 'the three-pixel reference threshold'
Assert-Contains $control 'OcctRectangleSelectionBehavior.Inclusive' 'inclusive rectangle selection as the default'
Assert-Contains $control 'dragDistance > RectangleSelectionThreshold' 'Manhattan-distance gesture classification'
Assert-Contains $control '!ModifierKeys.HasFlag(Keys.Shift)' 'Shift exclusion for left-button box selection'
Assert-Contains $control 'private bool _leftSelectionGesture;' 'an explicit left-selection gesture state'
Assert-Contains $control 'private bool IsActiveRectangleGesture =>' 'a capture-independent active rectangle state'
Assert-Contains $control 'EnsureRectangleCapture();' 'capture recovery during rectangle dragging'
Assert-Contains $control 'ScheduleRectangleCaptureRecovery();' 'asynchronous recovery after host capture loss'
Assert-Contains $control 'ScheduleSelectionFrameRestore();' 'rubber-band restoration after host resize'
Assert-Contains $control 'WindowsFormsHost and first-focus DPI/layout negotiation' 'the first-gesture resize guard'

$resizeStart = $control.IndexOf('protected override void OnResize(EventArgs e)', [StringComparison]::Ordinal)
$visibleStart = $control.IndexOf('protected override void OnVisibleChanged(EventArgs e)', [StringComparison]::Ordinal)
if ($resizeStart -lt 0 -or $visibleStart -le $resizeStart) {
    throw 'Unable to inspect OcctViewportControl.OnResize().'
}
$resizeBlock = $control.Substring($resizeStart, $visibleStart - $resizeStart)
if ($resizeBlock.Contains('CancelRectangleSelection();')) {
    throw 'OnResize must preserve an active rectangle gesture instead of cancelling the first drag.'
}

Write-Host '[selection] Point/box selection, batched selected/detected hits, subshape-index contract, and first-gesture capture recovery validated.' -ForegroundColor Green
