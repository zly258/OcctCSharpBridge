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

function Read-PartialSet {
    param(
        [Parameter(Mandatory = $true)][string]$RelativeDirectory,
        [Parameter(Mandatory = $true)][string]$Pattern
    )

    $directory = Join-Path $RepositoryRoot $RelativeDirectory
    $files = @(Get-ChildItem $directory -Filter $Pattern -File | Sort-Object Name)
    if ($files.Count -eq 0) {
        throw "No partial source files matched $RelativeDirectory/$Pattern"
    }
    return ($files | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"
}

$preconditions = Read-RequiredText "src\OcctDemo.Common\DemoSession.Preconditions.cs"
$session = Read-RequiredText "src\OcctDemo.Common\DemoSession.cs"
$commands = Read-RequiredText "src\OcctDemo.Common\DemoSession.Commands.cs"
$winForms = Read-PartialSet "src\OcctDemo.WinForms" "MainForm*.cs"
$wpf = Read-PartialSet "src\OcctDemo.Wpf" "MainWindow*.cs"

foreach ($command in @(
    "Extrude", "Revolve", "Sweep", "Loft",
    "Fuse", "Cut", "Common", "Section",
    "Fillet", "Chamfer", "Offset", "Shell", "Drill",
    "Translate", "Rotate", "Scale", "Mirror", "Copy", "Delete",
    "LengthDimension", "AngleDimension", "RadiusDimension", "DiameterDimension",
    "AnalyzeBounds", "AnalyzeMass", "AnalyzeTopology", "AnalyzeDistance", "ValidateShape"
)) {
    if (-not $preconditions.Contains("DemoCommandId.$command")) {
        throw "Selection-dependent command has no precondition rule: $command"
    }
}

foreach ($token in @(
    "GetCommandAvailability",
    "EnsureCommandAvailable",
    "RequireShapeCount",
    "RequireSubshapeHits",
    "Engine.GetSelectedHits()",
    "hit.IsSubshape && hit.SubshapeType == requiredType",
    "var required = commandId == DemoCommandId.AngleDimension ? 2 : 1;",
    "RequireSubshapeHits(commandId, selectedHits, required, OcctShapeType.Edge)",
    "IsProfileType",
    "exactly: true",
    "OcctShapeType.Solid or OcctShapeType.CompSolid"
)) {
    if (-not $preconditions.Contains($token)) {
        throw "Demo command precondition contract is missing: $token"
    }
}

if ($preconditions.Contains("RequireSubshapeCount")) {
    throw "Subshape command validation must use structured selection hits rather than owner-object counts."
}

if (-not $preconditions.Contains("commandId is DemoCommandId.LengthDimension") -or
    -not $preconditions.Contains("or DemoCommandId.AngleDimension") -or
    -not $preconditions.Contains("or DemoCommandId.RadiusDimension") -or
    -not $preconditions.Contains("or DemoCommandId.DiameterDimension")) {
    throw "All dimension commands must share the structured edge-hit precondition path."
}

if (-not $session.Contains("public sealed partial class DemoSession") -or
    -not $commands.Contains("public sealed partial class DemoSession")) {
    throw "DemoSession core and command responsibilities must remain partials of the same session type."
}
if (-not $commands.Contains("public DemoCommandResult Execute(") -or
    -not $commands.Contains("EnsureCommandAvailable(commandId);")) {
    throw "DemoSession.Commands.Execute must enforce preconditions for non-UI callers."
}
if ($commands.Contains("if (selectedObjectIds.Count == 0 && ActiveObject")) {
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

Write-Host "[demo-preconditions] Selection count, topology suitability, structured subshape hits, split command dispatch, UI early checks, and execution safeguards validated." -ForegroundColor Green
