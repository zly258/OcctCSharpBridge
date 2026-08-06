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

function Get-RawMatches {
    param(
        [string]$Text,
        [string]$Pattern
    )

    return @([regex]::Matches(
        $Text,
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Multiline -bor
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    ) | ForEach-Object { $_.Groups[1].Value })
}

function Get-Matches {
    param(
        [string]$Text,
        [string]$Pattern
    )

    return @(Get-RawMatches $Text $Pattern | Sort-Object -Unique)
}

function Assert-NoDuplicates {
    param(
        [string]$Name,
        [string[]]$Values
    )

    $duplicates = @($Values | Group-Object | Where-Object Count -gt 1 | Sort-Object Name)
    if ($duplicates.Count -eq 0) { return }

    Write-Host "[api] Duplicate ${Name} entries:" -ForegroundColor Red
    $duplicates | ForEach-Object { Write-Host ("  {0} ({1})" -f $_.Name, $_.Count) -ForegroundColor Red }
    throw "Duplicate API entries were found for $Name."
}

$headerText = Read-AllText $headerFiles
$cppText = Read-AllText $cppFiles
$pinvokeText = Read-AllText $pinvokeFiles

$declarationRaw = Get-RawMatches $headerText '\b(occt_[a-z0-9_]+)\s*\([^{};]*\)\s*;'
$definitionRaw = Get-RawMatches $cppText '\b(occt_[a-z0-9_]+)\s*\([^;{}]*\)\s*\{'
$pinvokeRaw = Get-RawMatches $pinvokeText '\bextern\s+[A-Za-z0-9_<>,\[\]?]+\s+(occt_[a-z0-9_]+)\s*\('
$cdeclPInvokes = Get-Matches $pinvokeText '(?s)\[DllImport\([^\]]*CallingConvention\s*=\s*CallingConvention\.Cdecl[^\]]*\)\]\s*internal\s+static\s+extern\s+[A-Za-z0-9_<>,\[\]?]+\s+(occt_[a-z0-9_]+)\s*\('

Assert-NoDuplicates "native declarations" $declarationRaw
Assert-NoDuplicates "native definitions" $definitionRaw
Assert-NoDuplicates "C# P/Invoke declarations" $pinvokeRaw

$declarations = @($declarationRaw | Sort-Object -Unique)
$definitions = @($definitionRaw | Sort-Object -Unique)
$pinvokes = @($pinvokeRaw | Sort-Object -Unique)

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
Assert-SetEqual "Cdecl P/Invoke declarations" $pinvokes $cdeclPInvokes

$documentationFiles = @(
    Join-Path $RepositoryRoot "docs\API_COVERAGE.md"
    Join-Path $RepositoryRoot "docs\API_COVERAGE.zh-CN.md"
)
foreach ($documentationFile in $documentationFiles) {
    if (-not (Test-Path $documentationFile)) {
        throw "API inventory was not found: $documentationFile"
    }
    $documentation = [System.IO.File]::ReadAllText($documentationFile)
    $nativeCount = [regex]::Match($documentation, 'Native exports:\s*`?(\d+)`?').Groups[1].Value
    $managedCount = [regex]::Match($documentation, 'Managed P/Invoke declarations:\s*`?(\d+)`?').Groups[1].Value
    if ([string]::IsNullOrWhiteSpace($nativeCount) -or [string]::IsNullOrWhiteSpace($managedCount)) {
        throw "API inventory counts could not be parsed: $documentationFile"
    }
    if ([int]$nativeCount -ne $declarations.Count -or [int]$managedCount -ne $pinvokes.Count) {
        throw "API inventory is stale: $documentationFile (native=$nativeCount, managed=$managedCount, expected=$($declarations.Count))."
    }
}

$groups = [ordered]@{
    Viewer = @($declarations | Where-Object { $_ -notlike 'occt_model_*' -and $_ -notlike 'occt_ocaf_*' })
    Modeling = @($declarations | Where-Object { $_ -like 'occt_model_*' })
    Ocaf = @($declarations | Where-Object { $_ -like 'occt_ocaf_*' })
}
foreach ($group in $groups.GetEnumerator()) {
    Write-Host ("[api] {0}: {1}" -f $group.Key, $group.Value.Count) -ForegroundColor Cyan
}

Write-Host "API surface validation passed." -ForegroundColor Green
