param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) {
    throw "Bridge contract file was not found: bridge-contract.json"
}
$contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$expectedNativeCount = [int]$contract.api.nativeExports
$expectedManagedCount = [int]$contract.api.managedPInvokes
$expectedPublicTypeCount = [int]$contract.api.publicNetTypes
$expectedViewerCount = [int]$contract.api.viewer
$expectedModelingCount = [int]$contract.api.modeling

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
$publicManagedRoots = @(
    $managedRoot,
    (Join-Path $RepositoryRoot "src\OcctNet.WinForms"),
    (Join-Path $RepositoryRoot "src\OcctNet.Wpf")
)

# Public C ABI declarations are intentionally split by responsibility. Keep this list
# explicit so a new ABI module must be consciously added to surface validation.
$headerFiles = @(
    Join-Path $nativeRoot "OcctNative.h"
    Join-Path $nativeRoot "OcctSelectionOverlay.h"
    Join-Path $nativeRoot "OcctModeling.h"
    Join-Path $nativeRoot "OcctModelingExtensions.h"
    Join-Path $nativeRoot "OcctModelingBSpline.h"
    Join-Path $nativeRoot "OcctModelingTopologyAnalysis.h"
    Join-Path $nativeRoot "OcctModelingFaceAnalysis.h"
)
$cppFiles = Get-ChildItem $nativeRoot -Filter "*.cpp" -File | Select-Object -ExpandProperty FullName
$managedSourceFiles = @($publicManagedRoots | ForEach-Object {
    Get-ChildItem $_ -Filter "*.cs" -File | Select-Object -ExpandProperty FullName
})
$pinvokeFiles = Get-ChildItem $managedRoot -Filter "*NativeMethods*.cs" -File | Select-Object -ExpandProperty FullName

foreach ($path in @($headerFiles + $cppFiles + $managedSourceFiles + $pinvokeFiles)) {
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
$managedText = Read-AllText $managedSourceFiles
$pinvokeText = Read-AllText $pinvokeFiles

$declarationRaw = Get-RawMatches $headerText '\b(occt_[a-z0-9_]+)\s*\([^{};]*\)\s*;'
$definitionRaw = Get-RawMatches $cppText '\b(occt_[a-z0-9_]+)\s*\([^;{}]*\)\s*\{'
$pinvokeRaw = Get-RawMatches $pinvokeText '\bextern\s+[A-Za-z0-9_<>,\[\]?]+\s+(occt_[a-z0-9_]+)\s*\('
$cdeclPInvokes = Get-Matches $pinvokeText '(?s)\[DllImport\([^\]]*CallingConvention\s*=\s*CallingConvention\.Cdecl[^\]]*\)\]\s*internal\s+static\s+extern\s+[A-Za-z0-9_<>,\[\]?]+\s+(occt_[a-z0-9_]+)\s*\('
$exactPInvokes = Get-Matches $pinvokeText '(?s)\[DllImport\([^\]]*ExactSpelling\s*=\s*true[^\]]*\)\]\s*internal\s+static\s+extern\s+[A-Za-z0-9_<>,\[\]?]+\s+(occt_[a-z0-9_]+)\s*\('

$publicTypePatterns = @(
    '(?m)^[ \t]*public[ \t]+(?:(?:abstract|sealed|static|partial|readonly|ref|unsafe|new)[ \t]+)*(?:class|struct|interface|enum)[ \t]+([A-Za-z_][A-Za-z0-9_]*)',
    '(?m)^[ \t]*public[ \t]+(?:(?:abstract|sealed|static|partial|readonly|ref|unsafe|new)[ \t]+)*record(?:[ \t]+(?:class|struct))?[ \t]+([A-Za-z_][A-Za-z0-9_]*)',
    '(?m)^[ \t]*public[ \t]+(?:(?:unsafe|new)[ \t]+)*delegate[ \t]+[^;\r\n(]+?[ \t]+([A-Za-z_][A-Za-z0-9_]*)[ \t]*(?:<[^;\r\n>]+>)?[ \t]*\('
)
$publicTypeNames = @(
    foreach ($pattern in $publicTypePatterns) {
        Get-RawMatches $managedText $pattern
    }
) | Sort-Object -Unique

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
Assert-SetEqual "exact-name P/Invoke declarations" $pinvokes $exactPInvokes

if ($declarations.Count -ne $expectedNativeCount) {
    throw "Native export count differs from bridge-contract.json: actual=$($declarations.Count), expected=$expectedNativeCount."
}
if ($pinvokes.Count -ne $expectedManagedCount) {
    throw "Managed P/Invoke count differs from bridge-contract.json: actual=$($pinvokes.Count), expected=$expectedManagedCount."
}
if ($publicTypeNames.Count -ne $expectedPublicTypeCount) {
    Write-Host ("[api] Public .NET types detected: {0}" -f ($publicTypeNames -join ', ')) -ForegroundColor Yellow
    throw "Public .NET type count differs from bridge-contract.json: actual=$($publicTypeNames.Count), expected=$expectedPublicTypeCount."
}

$documentationFiles = @(
    Join-Path $RepositoryRoot "docs\API_COVERAGE.md"
    Join-Path $RepositoryRoot "docs\API_COVERAGE.zh-CN.md"
)
foreach ($documentationFile in $documentationFiles) {
    if (-not (Test-Path $documentationFile)) {
        throw "API inventory was not found: $documentationFile"
    }
    $documentation = [System.IO.File]::ReadAllText($documentationFile)
    $nativeCount = [regex]::Match($documentation, 'Native exports\s*[:：]\s*`?(\d+)`?').Groups[1].Value
    $managedCount = [regex]::Match($documentation, 'Managed P/Invoke declarations\s*[:：]\s*`?(\d+)`?').Groups[1].Value
    $publicTypeCount = [regex]::Match($documentation, 'Public \.NET types\s*[:：]\s*`?(\d+)`?').Groups[1].Value
    if ([string]::IsNullOrWhiteSpace($nativeCount) -or [string]::IsNullOrWhiteSpace($managedCount) -or [string]::IsNullOrWhiteSpace($publicTypeCount)) {
        throw "API inventory counts could not be parsed: $documentationFile"
    }
    if ([int]$nativeCount -ne $expectedNativeCount -or [int]$managedCount -ne $expectedManagedCount -or [int]$publicTypeCount -ne $expectedPublicTypeCount) {
        throw "API inventory differs from bridge-contract.json: $documentationFile (native=$nativeCount, managed=$managedCount, publicTypes=$publicTypeCount)."
    }
}

$ocafExports = @($declarations | Where-Object { $_ -like 'occt_ocaf_*' })
if ($ocafExports.Count -ne 0) {
    throw "OCAF/XDE exports are not allowed in the reusable bridge."
}

$groups = [ordered]@{
    Viewer = @($declarations | Where-Object { $_ -notlike 'occt_model_*' })
    Modeling = @($declarations | Where-Object { $_ -like 'occt_model_*' })
}

if ($groups.Viewer.Count -ne $expectedViewerCount) {
    throw "Viewer API count differs from bridge-contract.json: actual=$($groups.Viewer.Count), expected=$expectedViewerCount."
}
if ($groups.Modeling.Count -ne $expectedModelingCount) {
    throw "Modeling API count differs from bridge-contract.json: actual=$($groups.Modeling.Count), expected=$expectedModelingCount."
}

foreach ($group in $groups.GetEnumerator()) {
    Write-Host ("[api] {0}: {1}" -f $group.Key, $group.Value.Count) -ForegroundColor Cyan
}
Write-Host "[api] Public .NET types: $($publicTypeNames.Count)" -ForegroundColor Cyan

Write-Host ("API surface validation passed against bridge-contract.json ({0} native / {1} managed / {2} public types)." -f $expectedNativeCount, $expectedManagedCount, $expectedPublicTypeCount) -ForegroundColor Green
