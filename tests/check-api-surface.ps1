param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"

$headerFiles = @(
    Join-Path $nativeRoot "OcctNative.h"
    Join-Path $nativeRoot "OcctSelectionOverlay.h"
    Join-Path $nativeRoot "OcctModeling.h"
    Join-Path $nativeRoot "OcctOcaf.h"
    Join-Path $nativeRoot "OcctOcafExtended.h"
)
$cppFiles = Get-ChildItem $nativeRoot -Filter "*.cpp" -File | Select-Object -ExpandProperty FullName
$pinvokeFiles = Get-ChildItem $managedRoot -Filter "*NativeMethods*.cs" -File | Select-Object -ExpandProperty FullName

foreach ($path in @($headerFiles + $cppFiles + $pinvokeFiles)) {
    if (-not (Test-Path $path)) {
        throw "API validation input was not found: $path"
    }
}

function Read-AllText {
    param([string[]]$Paths)
    return ($Paths | ForEach-Object { [System.IO.File]::ReadAllText($_) }) -join "`n"
}

function Get-Matches {
    param(
        [string]$Text,
        [string]$Pattern
    )

    return [regex]::Matches(
        $Text,
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Multiline -bor
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    ) | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
}

$headerText = Read-AllText $headerFiles
$cppText = Read-AllText $cppFiles
$pinvokeText = Read-AllText $pinvokeFiles

$declarations = Get-Matches $headerText '\b(occt_[a-z0-9_]+)\s*\([^{};]*\)\s*;'
$definitions = Get-Matches $cppText '\b(occt_[a-z0-9_]+)\s*\([^;{}]*\)\s*\{'
$pinvokes = Get-Matches $pinvokeText '\bextern\s+[A-Za-z0-9_<>,\[\]?]+\s+(occt_[a-z0-9_]+)\s*\('

function Assert-SetEqual {
    param(
        [string]$Name,
        [string[]]$Expected,
        [string[]]$Actual
    )

    $missing = @($Expected | Where-Object { $_ -notin $Actual })
    $extra = @($Actual | Where-Object { $_ -notin $Expected })
    if ($missing.Count -eq 0 -and $extra.Count -eq 0) {
        Write-Host "[api] ${Name}: $($Expected.Count) entries matched." -ForegroundColor Green
        return
    }

    if ($missing.Count -gt 0) {
        Write-Host "[api] Missing ${Name} entries:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    }
    if ($extra.Count -gt 0) {
        Write-Host "[api] Unexpected ${Name} entries:" -ForegroundColor Yellow
        $extra | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    }
    throw "API surface validation failed for $Name."
}

Assert-SetEqual "native definitions" $declarations $definitions
Assert-SetEqual "C# P/Invoke declarations" $declarations $pinvokes

$groups = [ordered]@{
    Viewer = @($declarations | Where-Object { $_ -notlike 'occt_model_*' -and $_ -notlike 'occt_ocaf_*' })
    Modeling = @($declarations | Where-Object { $_ -like 'occt_model_*' })
    Ocaf = @($declarations | Where-Object { $_ -like 'occt_ocaf_*' })
}
foreach ($group in $groups.GetEnumerator()) {
    Write-Host ("[api] {0}: {1}" -f $group.Key, $group.Value.Count) -ForegroundColor Cyan
}

Write-Host "API surface validation passed." -ForegroundColor Green
