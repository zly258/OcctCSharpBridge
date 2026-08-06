from pathlib import Path

root = Path(__file__).resolve().parents[1]
path = root / ".github/apply_api_organization_2_5.py"
text = path.read_bytes().decode("utf-8-sig")
text = text.replace(
    '    "精确的直线、圆、椭圆、平面、圆柱、圆锥、球面和圆环面参数可用于特征识别与工程自动化。",\n',
    '    "新增直线、圆、椭圆、平面、圆柱、圆锥、球面和圆环面的精确解析参数读取，可用于特征识别与工程规则判断。",\n')
text = text.replace(
    '    "- 托管目标：`.NET 8`，Windows x64。\\n",\n'
    '    "- 托管目标：`.NET 8`，Windows x64。\\n- Bridge 版本：`2.5.0`；ABI：`2`。\\n",\n',
    '    "- 托管目标为 `.NET 8`、Windows x64。\\n",\n'
    '    "- 托管目标为 `.NET 8`、Windows x64。\\n- Bridge 版本：`2.5.0`；ABI：`2`。\\n",\n')
path.write_bytes(b"\xef\xbb\xbf" + text.encode("utf-8"))
Path(__file__).unlink()
