param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path -LiteralPath $contractPath -PathType Leaf)) {
    throw "bridge-contract.json was not found."
}
$contract = Get-Content -LiteralPath $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$baseline = [string]$contract.release.apiBaselineCommit
if ([string]::IsNullOrWhiteSpace($baseline)) {
    throw "Stable release contract is missing release.apiBaselineCommit."
}

& git -C $RepositoryRoot cat-file -e "$baseline`^{commit}" 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Stable API baseline commit is not available locally: $baseline"
}

function Get-TextAtRef {
    param(
        [Parameter(Mandatory = $true)][string]$Ref,
        [Parameter(Mandatory = $true)][string]$Path
    )
    $value = @(& git -C $RepositoryRoot show "$Ref`:$Path" 2>$null)
    if ($LASTEXITCODE -ne 0) { throw "Unable to read $Path at $Ref." }
    return $value -join "`n"
}

function Get-TrackedPathsAtRef {
    param(
        [Parameter(Mandatory = $true)][string]$Ref,
        [Parameter(Mandatory = $true)][string[]]$Roots,
        [Parameter(Mandatory = $true)][string[]]$Extensions
    )

    $paths = @(& git -C $RepositoryRoot ls-tree -r --name-only $Ref -- @Roots)
    if ($LASTEXITCODE -ne 0) { throw "Unable to enumerate API source at $Ref." }
    return @($paths | Where-Object { [System.IO.Path]::GetExtension($_) -in $Extensions } | Sort-Object -Unique)
}

function Normalize-Signature {
    param([Parameter(Mandatory = $true)][string]$Value)
    $normalized = [regex]::Replace($Value, '\s+', ' ').Trim()

    # Field initializer values are implementation/version metadata rather than member signatures.
    # Parameter default values remain part of method signatures because the expression contains '('.
    if ($normalized -notmatch '\(' -and $normalized -match '=') {
        $normalized = ($normalized -replace '\s*=.*$', '').Trim()
    }
    return $normalized
}

function Get-ManagedPublicSurface {
    param([Parameter(Mandatory = $true)][string]$Ref)

    $roots = @(
        "src/OcctNet",
        "src/OcctNet.WinForms",
        "src/OcctNet.Wpf",
        "src/OcctNet.Avalonia"
    )
    $result = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($path in @(Get-TrackedPathsAtRef $Ref $roots @('.cs'))) {
        $text = Get-TextAtRef $Ref $path

        $typePattern = '(?ms)^[ \t]*public[ \t]+(?:(?:abstract|sealed|static|partial|readonly|ref|unsafe|new)[ \t]+)*(?:class|struct|interface|enum|record(?:[ \t]+(?:class|struct))?)[ \t]+[^\r\n{]+'
        foreach ($match in [regex]::Matches($text, $typePattern)) {
            [void]$result.Add("TYPE " + (Normalize-Signature $match.Value))
        }

        $memberPattern = '(?ms)^[ \t]*public[ \t]+(?!abstract[ \t]+class\b|sealed[ \t]+class\b|static[ \t]+class\b|partial[ \t]+class\b|readonly[ \t]+struct\b|ref[ \t]+struct\b|class\b|struct\b|interface\b|enum\b|record\b)(.*?)(?=\{|=>|;)'
        foreach ($match in [regex]::Matches($text, $memberPattern)) {
            $signature = Normalize-Signature $match.Value
            if (-not [string]::IsNullOrWhiteSpace($signature)) {
                [void]$result.Add("MEMBER " + $signature)
            }
        }
    }
    return @($result | Sort-Object)
}

function Get-NativeExportSurface {
    param([Parameter(Mandatory = $true)][string]$Ref)

    $refContract = Get-TextAtRef $Ref "bridge-contract.json" | ConvertFrom-Json
    $headers = @($refContract.api.nativeHeaders | ForEach-Object { [string]$_ })
    if ($headers.Count -eq 0) { throw "Contract at $Ref has no api.nativeHeaders." }

    $result = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $pattern = '\b(?:[A-Za-z_][A-Za-z0-9_]*[ \t\r\n\*]+)+(occt_[a-z0-9_]+)\s*\([^{};]*\)\s*;'
    foreach ($header in $headers) {
        $text = Get-TextAtRef $Ref ("src/OcctNative/" + $header)
        foreach ($match in [regex]::Matches($text, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            [void]$result.Add("EXPORT " + (Normalize-Signature $match.Value))
        }
    }
    return @($result | Sort-Object)
}

function Assert-BaselineSubset {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string[]]$Baseline,
        [Parameter(Mandatory = $true)][string[]]$Current
    )

    $missing = @($Baseline | Where-Object { $_ -notin $Current })
    if ($missing.Count -eq 0) {
        Write-Host "[stable-api] $Name baseline preserved ($($Baseline.Count) baseline entries)." -ForegroundColor Green
        return
    }

    Write-Host "[stable-api] Removed or changed $Name entries:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    throw "Stable API compatibility check failed for $Name."
}

$baselineManaged = @(Get-ManagedPublicSurface $baseline)
$currentManaged = @(Get-ManagedPublicSurface "HEAD")
Assert-BaselineSubset "managed public API" $baselineManaged $currentManaged

$baselineNative = @(Get-NativeExportSurface $baseline)
$currentNative = @(Get-NativeExportSurface "HEAD")
Assert-BaselineSubset "native ABI5 signatures" $baselineNative $currentNative

Write-Host "Stable 3.x API/ABI baseline compatibility passed against $baseline." -ForegroundColor Green
