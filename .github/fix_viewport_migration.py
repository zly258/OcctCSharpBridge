from pathlib import Path

path = Path(__file__).resolve().parent / "apply_viewport_api_21.py"
text = path.read_text(encoding="utf-8-sig")
old = '''build = replace_once(
    build,
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck

""",
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck
    Assert-Path $ViewportApiCheck

    Write-Host "[viewport] Validating extended viewport contracts..." -ForegroundColor Cyan
    & $ViewportApiCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Viewport API validation failed."
    }

""",
    "viewport contract invocation")
'''
new = '''build = replace_once(
    build,
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck
    Assert-Path $SelectionContractCheck

    Write-Host "[selection] Validating point and rectangle selection behavior..." -ForegroundColor Cyan
""",
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck
    Assert-Path $SelectionContractCheck
    Assert-Path $ViewportApiCheck

    Write-Host "[viewport] Validating extended viewport contracts..." -ForegroundColor Cyan
    & $ViewportApiCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Viewport API validation failed."
    }

    Write-Host "[selection] Validating point and rectangle selection behavior..." -ForegroundColor Cyan
""",
    "viewport contract invocation")
'''
if old not in text:
    raise RuntimeError("The viewport migration hook was not found.")
path.write_text(text.replace(old, new, 1), encoding="utf-8-sig", newline="\n")
