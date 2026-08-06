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

Write-Host "[native-build] $($sourceTokens.Count) source entries validated; no OCAF/XDE build inputs remain." -ForegroundColor Green
