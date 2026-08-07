from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"
path = ROOT / "publish.ps1"
text = path.read_bytes().decode("utf-8-sig").replace("\r\n", "\n")
replacements = {
    r'(?<version>[0-9]+(?:\\.[0-9]+){1,3})': r'(?<version>[0-9]+(?:\.[0-9]+){1,3})',
    r'[0-9]+(?:\\.[0-9]+){1,3}': r'[0-9]+(?:\.[0-9]+){1,3}',
    r'microsoft\\.vc[0-9]+\\.crt': r'microsoft\.vc[0-9]+\.crt',
}
for old, new in replacements.items():
    if old not in text:
        raise RuntimeError(f"Expected regex fragment not found: {old}")
    text = text.replace(old, new)
path.write_bytes(BOM + text.replace("\n", "\r\n").encode("utf-8"))
(ROOT / ".github/fix_vc_runtime_regex.py").unlink()
