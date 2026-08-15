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
if ([int]$contract.nativeAbi.current -ne 5 -or [int]$contract.nativeAbi.minimumSupported -ne 5) {
    throw "API surface validation requires an ABI5-only bridge contract."
}
if ([string]$contract.api.policy -ne "abi5-only") {
    throw "bridge-contract.json api.policy must be 'abi5-only'."
}

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
$publicManagedRoots = @(
    $managedRoot,
    (Join-Path $RepositoryRoot "src\OcctNet.WinForms"),
    (Join-Path $RepositoryRoot "src\OcctNet.Wpf"),
    (Join-Path $RepositoryRoot "src\OcctNet.Avalonia")
)

$nativeHeaderNames = @($contract.api.nativeHeaders | ForEach-Object { [string]$_ })
if ($nativeHeaderNames.Count -eq 0) {
    throw "bridge-contract.json does not declare api.nativeHeaders."
}
if (@($nativeHeaderNames | Group-Object | Where-Object Count -gt 1).Count -gt 0) {
    throw "bridge-contract.json contains duplicate api.nativeHeaders entries."
}
$headerFiles = @($nativeHeaderNames | ForEach-Object { Join-Path $nativeRoot $_ })
$cppFiles = @(Get-ChildItem $nativeRoot -Filter "*.cpp" -File -Recurse | Select-Object -ExpandProperty FullName)
$managedSourceFiles = @($publicManagedRoots | ForEach-Object {
    if (-not (Test-Path $_ -PathType Container)) {
        throw "Public managed API root is missing: $_"
    }
    Get-ChildItem $_ -Filter "*.cs" -File -Recurse | Select-Object -ExpandProperty FullName
})
$interopFiles = @(Get-ChildItem $managedRoot -Filter "*.cs" -File -Recurse | Where-Object {
    $text = [System.IO.File]::ReadAllText($_.FullName)
    $text.Contains("[LibraryImport(") -or $text.Contains("[DllImport(")
} | Select-Object -ExpandProperty FullName)

foreach ($path in @($headerFiles + $cppFiles + $managedSourceFiles + $interopFiles)) {
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "API validation input was not found: $path"
    }
}

function Read-AllText {
    param([string[]]$Paths)
    return ($Paths | ForEach-Object { [System.IO.File]::ReadAllText($_) }) -join "`n"
}

function Get-RawMatches {
    param([string]$Text, [string]$Pattern)
    return @([regex]::Matches(
        $Text,
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Multiline -bor
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    ) | ForEach-Object { $_.Groups[1].Value })
}

function Assert-NoDuplicates {
    param([string]$Name, [string[]]$Values)
    $duplicates = @($Values | Group-Object | Where-Object Count -gt 1 | Sort-Object Name)
    if ($duplicates.Count -eq 0) { return }
    Write-Host "[api] Duplicate ${Name} entries:" -ForegroundColor Red
    $duplicates | ForEach-Object { Write-Host ("  {0} ({1})" -f $_.Name, $_.Count) -ForegroundColor Red }
    throw "Duplicate API entries were found for $Name."
}

function Assert-SetEqual {
    param([string]$Name, [string[]]$Expected, [string[]]$Actual)
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

$headerText = Read-AllText $headerFiles
$cppText = Read-AllText $cppFiles
$managedText = Read-AllText $managedSourceFiles
$interopText = Read-AllText $interopFiles

$declarationRaw = Get-RawMatches $headerText '\b(occt_[a-z0-9_]+)\s*\([^{};]*\)\s*;'
$definitionRaw = Get-RawMatches $cppText '\b(occt_[a-z0-9_]+)\s*\([^;{}]*\)\s*\{'
$interopRaw = Get-RawMatches $interopText '\b(?:extern|partial)\s+[A-Za-z0-9_<>,\[\]?*]+\s+(occt_[a-z0-9_]+)\s*\('
$libraryImportRaw = Get-RawMatches $interopText '(?s)\[LibraryImport\([^\]]+\)\]\s*\[UnmanagedCallConv\(CallConvs\s*=\s*\[typeof\(CallConvCdecl\)\]\)\]\s*internal\s+static\s+partial\s+[A-Za-z0-9_<>,\[\]?*]+\s+(occt_[a-z0-9_]+)\s*\('

Assert-NoDuplicates "native declarations" $declarationRaw
Assert-NoDuplicates "native definitions" $definitionRaw
Assert-NoDuplicates "managed interop declarations" $interopRaw
Assert-NoDuplicates "LibraryImport declarations" $libraryImportRaw

$declarations = @($declarationRaw | Sort-Object -Unique)
$definitions = @($definitionRaw | Sort-Object -Unique)
$interopDeclarations = @($interopRaw | Sort-Object -Unique)
$libraryImports = @($libraryImportRaw | Sort-Object -Unique)

Assert-SetEqual "native definitions" $declarations $definitions
Assert-SetEqual "managed ABI5 interop" $declarations $interopDeclarations
Assert-SetEqual "LibraryImport + Cdecl bindings" $declarations $libraryImports

$dllImportFiles = @($managedSourceFiles | Where-Object {
    [System.IO.File]::ReadAllText($_).Contains("[DllImport(")
})
if ($dllImportFiles.Count -gt 0) {
    Write-Host "[api] DllImport is forbidden in OcctNet ABI5 production interop:" -ForegroundColor Red
    $dllImportFiles | ForEach-Object { Write-Host ("  " + $_.Substring($RepositoryRoot.Length).TrimStart('\')) -ForegroundColor Red }
    throw "Managed ABI5 interop must use LibraryImport only."
}

$allowedMetadataExports = @(
    "occt_version",
    "occt_bridge_version",
    "occt_bridge_build_info",
    "occt_bridge_current_abi_version"
)
$allowedPrefixes = @(
    "occt_engine_",
    "occt_model_",
    "occt_shape_",
    "occt_mesh_",
    "occt_algorithm_"
)
$invalidExports = @($declarations | Where-Object {
    $name = $_
    if ($name -in $allowedMetadataExports) { return $false }
    return -not @($allowedPrefixes | Where-Object { $name.StartsWith($_, [StringComparison]::Ordinal) }).Count
})
if ($invalidExports.Count -gt 0) {
    throw "Non-ABI5 native export names remain: $($invalidExports -join ', ')"
}

$invalidSemanticNames = @($declarations | Where-Object { $_ -match '(_v[0-9]+|_ex[0-9]*)$' })
if ($invalidSemanticNames.Count -gt 0) {
    throw "ABI5 exports must use semantic names instead of version/Ex suffixes: $($invalidSemanticNames -join ', ')"
}

$publicTypePatterns = @(
    '(?m)^[ \t]*public[ \t]+(?:(?:abstract|sealed|static|partial|readonly|ref|unsafe|new)[ \t]+)*(?:class|struct|interface|enum)[ \t]+([A-Za-z_][A-Za-z0-9_]*)',
    '(?m)^[ \t]*public[ \t]+(?:(?:abstract|sealed|static|partial|readonly|ref|unsafe|new)[ \t]+)*record(?:[ \t]+(?:class|struct))?[ \t]+([A-Za-z_][A-Za-z0-9_]*)',
    '(?m)^[ \t]*public[ \t]+(?:(?:unsafe|new)[ \t]+)*delegate[ \t]+[^;\r\n(]+?[ \t]+([A-Za-z_][A-Za-z0-9_]*)[ \t]*(?:<[^;\r\n>]+>)?[ \t]*\('
)
$publicTypeNames = @(
    foreach ($pattern in $publicTypePatterns) { Get-RawMatches $managedText $pattern }
) | Sort-Object -Unique

$ocafExports = @($declarations | Where-Object { $_ -like 'occt_ocaf_*' })
if ($ocafExports.Count -gt 0) {
    throw "OCAF/XDE exports are not allowed in the reusable bridge."
}

$modelingPrefixes = @("occt_model_", "occt_shape_", "occt_mesh_", "occt_algorithm_")
$modelingExports = @($declarations | Where-Object {
    $name = $_
    @($modelingPrefixes | Where-Object { $name.StartsWith($_, [StringComparison]::Ordinal) }).Count -gt 0
})
$viewerExports = @($declarations | Where-Object { $_ -notin $modelingExports })

Write-Host "[api] Native ABI5 exports: $($declarations.Count)" -ForegroundColor Cyan
Write-Host "[api] Managed LibraryImports: $($libraryImports.Count)" -ForegroundColor Cyan
Write-Host "[api] Viewer exports: $($viewerExports.Count)" -ForegroundColor Cyan
Write-Host "[api] Modeling exports: $($modelingExports.Count)" -ForegroundColor Cyan
Write-Host "[api] Public .NET types: $($publicTypeNames.Count)" -ForegroundColor Cyan
Write-Host "ABI5 API surface validation passed." -ForegroundColor Green
