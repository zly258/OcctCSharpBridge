from pathlib import Path

root = Path(__file__).resolve().parents[1]
bom = b"\xef\xbb\xbf"
path = root / "publish.ps1"
text = path.read_bytes().decode("utf-8-sig").replace("\r\n", "\n")
old = '[string]$OcctRoot = $(if ($env:OCCT_ROOT) { $env:OCCT_ROOT } else { "D:\\tools\\occt-vc144-64" }),'
new = '[string]$OcctRoot = $env:OCCT_ROOT,'
if old not in text:
    raise RuntimeError("publish.ps1 OCCT root default was not found")
text = text.replace(old, new, 1)
anchor = '$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path\n'
replacement = '''$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw "OCCT_ROOT is not configured. Pass -OcctRoot <path> or set the OCCT_ROOT environment variable."
}
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
'''
if anchor not in text:
    raise RuntimeError("publish.ps1 repository root anchor was not found")
text = text.replace(anchor, replacement, 1)
path.write_bytes(bom + text.replace("\n", "\r\n").encode("utf-8"))
Path(__file__).unlink()
