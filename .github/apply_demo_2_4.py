from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    normalized = text.replace("\r\n", "\n").replace("\n", "\r\n")
    (ROOT / path).write_bytes(BOM + normalized.encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


build = read("build.ps1")
build = replace_once(
    build,
    '$UiHostsCheck = Join-Path $RepoRoot "tests\\check-ui-hosts.ps1"\n',
    '$UiHostsCheck = Join-Path $RepoRoot "tests\\check-ui-hosts.ps1"\n'
    '$AnalyticGeometryCheck = Join-Path $RepoRoot "tests\\check-analytic-geometry-api.ps1"\n',
    "analytic check path")
build = replace_once(
    build,
    '        $UiHostsCheck,\n        $PackageCheck\n',
    '        $UiHostsCheck,\n        $AnalyticGeometryCheck,\n        $PackageCheck\n',
    "analytic check required file")
build = replace_once(
    build,
    '    Write-Host "[ui-hosts] Validating reusable WinForms and WPF hosts..." -ForegroundColor Cyan\n',
    '    Write-Host "[analytic-geometry] Validating analytic curve and surface contracts..." -ForegroundColor Cyan\n'
    '    & $AnalyticGeometryCheck -RepositoryRoot $RepoRoot\n'
    '    if (-not $?) { throw "Analytic geometry API validation failed." }\n\n'
    '    Write-Host "[ui-hosts] Validating reusable WinForms and WPF hosts..." -ForegroundColor Cyan\n',
    "analytic check invocation")
write("build.ps1", build)

english = read("README.md")
english = english.replace("2.3.0", "2.4.0").replace("Native `313`, P/Invoke `313`", "Native `321`, P/Invoke `321`")
english = english.replace("313-entry API surface", "321-entry API surface")
english = english.replace(
    "- Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations\n",
    "- Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations\n"
    "- Exact line, circle, ellipse, plane, cylinder, cone, sphere, and torus parameter queries\n")
english_preview = """## Preview

<table>
  <tr>
    <th>WinForms · English</th>
    <th>WPF · English</th>
  </tr>
  <tr>
    <td><img src="assets/previews/winform-demo-en.webp" alt="OCCT CAD WinForms English demo" width="100%"></td>
    <td><img src="assets/previews/wpf-demo-en.webp" alt="OCCT CAD WPF English demo" width="100%"></td>
  </tr>
</table>

"""
english, count = re.subn(r"## Preview\n\n<table>.*?</table>\n\n", english_preview, english, count=1, flags=re.DOTALL)
if count != 1:
    raise RuntimeError("English preview table was not found.")
write("README.md", english)

chinese = read("README.zh-CN.md")
chinese = chinese.replace("2.3.0", "2.4.0").replace("Native `313`，P/Invoke `313`", "Native `321`，P/Invoke `321`")
chinese = chinese.replace("313 项接口一致性", "321 项接口一致性")
chinese = chinese.replace(
    "- 批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择\n",
    "- 批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择\n"
    "- 直线、圆、椭圆、平面、圆柱、圆锥、球面和圆环面的精确参数读取\n")
chinese_preview = """## 界面预览

<table>
  <tr>
    <th>WinForms · 简体中文</th>
    <th>WPF · 简体中文</th>
  </tr>
  <tr>
    <td><img src="assets/previews/winform-demo-zh.webp" alt="OCCT CAD WinForms 中文界面" width="100%"></td>
    <td><img src="assets/previews/wpf-demo-zh.webp" alt="OCCT CAD WPF 中文界面" width="100%"></td>
  </tr>
</table>

"""
chinese, count = re.subn(r"## 界面预览\n\n<table>.*?</table>\n\n", chinese_preview, chinese, count=1, flags=re.DOTALL)
if count != 1:
    raise RuntimeError("Chinese preview table was not found.")
write("README.zh-CN.md", chinese)

workflow = read(".github/workflows/publish-script-check.yml")
workflow = replace_once(
    workflow,
    "              '.\\tests\\check-ui-hosts.ps1',\n",
    "              '.\\tests\\check-ui-hosts.ps1',\n              '.\\tests\\check-analytic-geometry-api.ps1',\n",
    "publish analytic check input")
write(".github/workflows/publish-script-check.yml", workflow)

(ROOT / ".github/apply_demo_2_4.py").unlink()
