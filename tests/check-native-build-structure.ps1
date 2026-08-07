param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$cmakePath = Join-Path $nativeRoot "CMakeLists.txt"
if (-not (Test-Path $cmakePath -PathType Leaf)) {
    throw "Native CMakeLists.txt was not found: $cmakePath"
}

$text = [System.IO.File]::ReadAllText($cmakePath)
$match = [regex]::Match(
    $text,
    'add_library\s*\(\s*OcctNative\s+SHARED(?<sources>.*?)\)',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $match.Success) {
    throw "The OcctNative add_library source list is missing or not closed."
}

$sourceTokens = @(
    $match.Groups['sources'].Value -split '\s+' |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and -not $_.StartsWith('#') }
)
if ($sourceTokens.Count -eq 0) {
    throw "The OcctNative source list is empty."
}

$duplicates = @($sourceTokens | Group-Object | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) {
    $names = ($duplicates | Select-Object -ExpandProperty Name) -join ', '
    throw "Duplicate native source entries were found: $names"
}

foreach ($source in $sourceTokens) {
    $path = Join-Path $nativeRoot $source
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Native source entry does not exist: $source"
    }
}

foreach ($required in @(
    "OcctModelingExtensions.cpp",
    "OcctModelingExtensions.h"
)) {
    if ($required -notin $sourceTokens) {
        throw "Bridge 2.6 native extension file is not listed in add_library: $required"
    }
}

$extensionHeader = [System.IO.File]::ReadAllText((Join-Path $nativeRoot "OcctModelingExtensions.h"))
foreach ($symbol in @(
    "occt_model_shape_is_same",
    "occt_model_shape_is_partner",
    "occt_model_shape_oriented_bounds",
    "occt_model_make_face_with_holes",
    "occt_model_trim_edge",
    "occt_model_offset_wire"
)) {
    if (-not $extensionHeader.Contains($symbol)) {
        throw "Bridge 2.6 native extension declaration is missing: $symbol"
    }
}

$forbiddenPatterns = [ordered]@{
    'OCAF/XDE source' = 'OcctOcaf|occt_ocaf_'
    'OCAF/XDE toolkit' = '\b(?:TKCDF|TKLCAF|TKCAF|TKXCAF|TKBinL|TKXmlL|TKBinXCAF|TKXmlXCAF)\b'
}
foreach ($item in $forbiddenPatterns.GetEnumerator()) {
    if ($text -match $item.Value) {
        throw "$($item.Key) remains in the reusable native build."
    }
}

$unlistedCpp = @(
    Get-ChildItem $nativeRoot -Filter '*.cpp' -File |
        Where-Object { $_.Name -notin $sourceTokens } |
        Select-Object -ExpandProperty Name
)
if ($unlistedCpp.Count -gt 0) {
    throw "Native C++ files are not listed in add_library: $($unlistedCpp -join ', ')"
}

$migrationWorkflow = Join-Path $RepositoryRoot ".github\workflows\bridge-26-native-migration.yml"
if (Test-Path $migrationWorkflow) {
    throw "Completed one-time Bridge 2.6 migration workflow must not remain in the repository."
}

Write-Host "[native-build] $($sourceTokens.Count) native source entries and Bridge 2.6 extension layout validated; no OCAF/XDE inputs remain." -ForegroundColor Green
