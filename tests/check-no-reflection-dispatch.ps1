param(
    [Parameter(Position = 0)]
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$patterns = @(
    'System\.Reflection',
    '\bMethodInfo\b',
    '\bDynamicInvoke\s*\(',
    '\bActivator\.CreateInstance\s*\(',
    '\bGetMethod[s]?\s*\(',
    '\bdynamic\b'
)

$sourceFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot "src") -Filter *.cs -File -Recurse
    Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot "tests") -Filter *.cs -File -Recurse
) | Where-Object {
    $relativePath = [System.IO.Path]::GetRelativePath($RepositoryRoot, $_.FullName)
    $segments = $relativePath -split '[\\/]'
    $segments -notcontains "bin" -and $segments -notcontains "obj"
}
$matches = @($sourceFiles | Select-String -Pattern $patterns)
if ($matches.Count -gt 0) {
    $matches | ForEach-Object { Write-Error "$($_.Path):$($_.LineNumber):$($_.Line.Trim())" }
    throw "Reflection or dynamic interface dispatch is forbidden. Use direct, strongly typed API calls."
}

Write-Host "[no-reflection] Direct, strongly typed dispatch policy passed." -ForegroundColor Green
