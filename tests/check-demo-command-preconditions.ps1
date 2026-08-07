param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-RequiredText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Required demo precondition file was not found: $RelativePath"
    }
    return [System.IO.File]::ReadAllText($path)
}

$preconditions = Read-RequiredText "src\CadCommon\CadSession.Preconditions.cs"
$session = Read-RequiredText "src\CadCommon\CadSession.cs"
$winForms = Read-RequiredText "src\CadWinForms\MainForm.cs"
$wpf = Read-RequiredText "src\CadWpf\MainWindow.xaml.cs"

foreach ($command in @(
    "Extrude", "Revolve", "Sweep", "Loft",
    "Fuse", "Cut", "Common", "Section",
    "Fillet", "Chamfer", "Offset", "Shell", "Drill",
    "Translate", "Rotate", "Scale", "Mirror", "Copy", "Delete",
    "LengthDimension", "AngleDimension", "RadiusDimension", "DiameterDimension",
    "AnalyzeBounds", "AnalyzeMass", "AnalyzeTopology", "AnalyzeDistance", "ValidateShape"
)) {
    if (-not $preconditions.Contains("CadCommandId.$command")) {
        throw "Selection-dependent command has no precondition rule: $command"
    }
}

foreach ($token in @(
    "GetCommandAvailability",
    "EnsureCommandAvailable",
    "RequireShapeCount",
    "RequireSubshapeCount",
    "IsProfileType",
    "exactly: true",
    "OcctShapeType.Solid or OcctShapeType.CompSolid"
)) {
    if (-not $preconditions.Contains($token)) {
        throw "Demo command precondition contract is missing: $token"
    }
}

if (-not $session.Contains("public sealed partial class CadSession")) {
    throw "CadSession must be partial so command preconditions stay in a dedicated category file."
}
if (-not $session.Contains("EnsureCommandAvailable(commandId);")) {
    throw "CadSession.Execute must enforce preconditions for non-UI callers."
}
if ($session.Contains("if (selectedObjectIds.Count == 0 && ActiveObject")) {
    throw "Command history must not silently substitute ActiveObject for an explicit selection."
}

foreach ($entry in @(
    @{ Name = "WinForms"; Text = $winForms },
    @{ Name = "WPF"; Text = $wpf }
)) {
    $availabilityIndex = $entry.Text.IndexOf("GetCommandAvailability(id)", [StringComparison]::Ordinal)
    $dialogIndex = $entry.Text.IndexOf("ParameterDialog.TryGetValues", [StringComparison]::Ordinal)
    if ($availabilityIndex -lt 0) {
        throw "$($entry.Name) does not evaluate command availability."
    }
    if ($dialogIndex -lt 0 -or $availabilityIndex -gt $dialogIndex) {
        throw "$($entry.Name) must reject invalid selection before opening the parameter dialog."
    }
    if (-not $entry.Text.Contains("ReportCommandPrecondition")) {
        throw "$($entry.Name) does not report a non-exception command hint."
    }
}

Write-Host "[demo-preconditions] Selection count, topology suitability, early UI checks, and execution safeguards validated." -ForegroundColor Green
