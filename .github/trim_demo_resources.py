from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"

publish_path = ROOT / "publish.ps1"
text = publish_path.read_bytes().decode("utf-8-sig").replace("\r\n", "\n")
old = '        foreach ($name in @("Textures", "XmlOcafResource", "TObj", "XCAFResources")) {'
new = '        foreach ($name in @("Textures")) {'
if old not in text:
    raise RuntimeError("Expected full-resource list was not found in publish.ps1.")
text = text.replace(old, new, 1)
for forbidden in ("XmlOcafResource", "XCAFResources", "TObj"):
    if forbidden in text:
        raise RuntimeError(f"Forbidden OCAF/XDE package resource remains: {forbidden}")
publish_path.write_bytes(BOM + text.replace("\n", "\r\n").encode("utf-8"))

(ROOT / ".github/trim_demo_resources.py").unlink()
(ROOT / ".github/workflows/apply-trim-demo-resources.yml").unlink()
