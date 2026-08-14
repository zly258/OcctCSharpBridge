param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
$baselinePath = Join-Path $RepositoryRoot "tests\contracts\abi4-exports.txt"
if (-not (Test-Path $baselinePath -PathType Leaf)) {
    throw "ABI 4 export baseline was not found: tests/contracts/abi4-exports.txt"
}

$contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$baseline = @(Get-Content $baselinePath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
$duplicates = @($baseline | Group-Object | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) { throw "ABI 4 export baseline contains duplicate symbols." }
if ($baseline.Count -ne [int]$contract.api.legacyAbi4Exports) {
    throw "ABI 4 export baseline count differs from bridge-contract.json."
}

$headerText = @(
    $contract.api.nativeHeaders | ForEach-Object {
        $path = Join-Path $RepositoryRoot ("src\OcctNative\" + [string]$_)
        if (-not (Test-Path $path -PathType Leaf)) { throw "Native API header was not found: $_" }
        [System.IO.File]::ReadAllText($path)
    }
) -join [Environment]::NewLine

$current = @(
    [regex]::Matches(
        $headerText,
        '\b(occt_[a-z0-9_]+)\s*\([^{};]*\)\s*;',
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    ) | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
)

$missing = @($baseline | Where-Object { $_ -notin $current })
if ($missing.Count -gt 0) {
    throw "Frozen ABI 4 exports are missing: $($missing -join ', ')"
}

$currentOnly = @($current | Where-Object { $_ -notin $baseline })
$expectedCurrentOnly = [int]$contract.api.abi5Exports + [int]$contract.api.compatibilityExtensions
if ($currentOnly.Count -ne $expectedCurrentOnly) {
    throw "Current-only export count differs from ABI contract: actual=$($currentOnly.Count), expected=$expectedCurrentOnly."
}

$invalidNewExports = @($currentOnly | Where-Object { $_ -match '(_v[0-9]+|_ex[0-9]*)$' })
if ($invalidNewExports.Count -gt 0) {
    throw "New exports must use semantic names instead of version or Ex suffixes: $($invalidNewExports -join ', ')"
}

$publicTypeMatches = [regex]::Matches(
    $headerText,
    '\b(?:struct|enum|using|typedef)\s+([A-Za-z_][A-Za-z0-9_]*(?:V[0-9]+|Ex[0-9]*))\b'
)
if ($publicTypeMatches.Count -gt 0) {
    $names = @($publicTypeMatches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
    throw "Public native types must not use version or Ex suffixes: $($names -join ', ')"
}

$trackedNames = @(git -C $RepositoryRoot ls-files)
$invalidNames = @($trackedNames | Where-Object { [System.IO.Path]::GetFileName($_) -match '(^|[._-])V[0-9]+([._-]|$)' })
if ($invalidNames.Count -gt 0) {
    throw "Tracked filenames must not contain V-number suffixes: $($invalidNames -join ', ')"
}

Write-Host ("[abi] ABI 4 frozen at {0} exports; {1} current-only exports validated with semantic naming." -f
    $baseline.Count,
    $currentOnly.Count) -ForegroundColor Green
