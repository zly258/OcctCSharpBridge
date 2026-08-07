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

Write-Host '[selection] Point selection, first-gesture capture recovery, box coordinates, and default behavior validated.' -ForegroundColor Green
