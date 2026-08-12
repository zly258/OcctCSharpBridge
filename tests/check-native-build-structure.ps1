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

$cmakeText = [System.IO.File]::ReadAllText($cmakePath)
$match = [regex]::Match(
    $cmakeText,
    'add_library\s*\(\s*OcctNative\s+SHARED(?<sources>.*?)\)',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $match.Success) {
    throw "The OcctNative add_library source list is missing or malformed."
}

$sourceTokens = @($match.Groups['sources'].Value -split '\s+' |
    ForEach-Object { $_.Trim() } |
    Where-Object { $_ -and -not $_.StartsWith('#') })

$duplicates = @($sourceTokens | Group-Object | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) {
    throw "Duplicate native source entries were found: $(($duplicates.Name) -join ', ')"
}

foreach ($source in $sourceTokens) {
    $sourcePath = Join-Path $nativeRoot $source
    if (-not (Test-Path $sourcePath -PathType Leaf)) {
        throw "Native CMake source entry does not exist: $source"
    }
}

$cppFiles = @(Get-ChildItem $nativeRoot -Filter '*.cpp' -File | Select-Object -ExpandProperty Name)
$unlistedCpp = @($cppFiles | Where-Object { $_ -notin $sourceTokens })
if ($unlistedCpp.Count -gt 0) {
    throw "Native C++ files are not listed in add_library: $($unlistedCpp -join ', ')"
}

foreach ($legacyToolkit in @("TKSTEPBase", "TKSTEPAttr", "TKSTEP209", "TKSTEP", "TKIGES")) {
    if ($cmakeText.Contains($legacyToolkit)) {
        throw "Legacy pre-7.9 data-exchange toolkit remains: $legacyToolkit"
    }
}

foreach ($requiredToolkit in @("TKDESTEP", "TKDEIGES", "TKDESTL", "TKLCAF", "TKCAF", "TKXCAF")) {
    if (-not $cmakeText.Contains($requiredToolkit)) {
        throw "Required OCCT 7.9 data-exchange/XDE toolkit is missing: $requiredToolkit"
    }
}

$nativeFiles = @(Get-ChildItem $nativeRoot -File -Recurse | Where-Object {
    $_.Extension -in @('.h', '.hpp', '.hxx', '.cpp')
})
$nativeText = ($nativeFiles | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"

if ($nativeText -match '\bSTEPCAFControl_(?:Reader|Writer)\b') {
    foreach ($requiredXdeType in @("XCAFDoc_ShapeTool", "XCAFDoc_ColorTool")) {
        if ($nativeText -notmatch "\b$requiredXdeType\b") {
            throw "Structured STEP exchange uses STEPCAF but is missing required XDE support: $requiredXdeType"
        }
    }
}

Write-Host "[native-build] CMake source inventory and OCCT 7.9 exchange/XDE toolkits validated." -ForegroundColor Green
