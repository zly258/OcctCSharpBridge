param(
    [string]$SourceRef = "origin/main"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
Set-Location $RepoRoot

if ($null -eq (Get-Command git -ErrorAction SilentlyContinue)) {
    throw "git was not found in PATH."
}

if ($SourceRef -eq "origin/main") {
    & git fetch origin main
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to fetch origin/main."
    }
}

$changes = @(& git status --porcelain -- dist/win-x64)
if ($LASTEXITCODE -ne 0) {
    throw "Failed to inspect dist/win-x64."
}
if ($changes.Count -gt 0) {
    throw "dist/win-x64 contains local changes. Commit or discard them before syncing."
}

& git restore --source $SourceRef --staged --worktree -- dist/win-x64
if ($LASTEXITCODE -ne 0) {
    throw "Failed to restore dist/win-x64 from $SourceRef. Run main/dist.ps1 and commit the generated Binary SDK first."
}

Write-Host "Bridge Binary SDK synced from $SourceRef." -ForegroundColor Green
Write-Host "Review and commit dist/win-x64 on the demo branch." -ForegroundColor Green
