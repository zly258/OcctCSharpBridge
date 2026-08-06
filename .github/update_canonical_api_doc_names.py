from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def update(path: str, replacements: list[tuple[str, str]]) -> None:
    target = ROOT / path
    text = target.read_bytes().decode("utf-8-sig").replace("\r\n", "\n")
    for old, new in replacements:
        if old not in text:
            raise RuntimeError(f"Expected documentation text was not found in {path}: {old}")
        text = text.replace(old, new)
    target.write_bytes(BOM + text.replace("\n", "\r\n").encode("utf-8"))


update(
    "docs/API_COVERAGE.md",
    [
        (
            "Use `GetCurveType()` or `GetSurfaceType()` first, then read exact analytic parameters instead of estimating centers, axes, and radii from sampled points.",
            "Use `GetEdgeCurveType()` or `GetFaceSurfaceType()` first, then read exact analytic parameters instead of estimating centers, axes, and radii from sampled points. The older `GetCurveType()` and `GetSurfaceType()` names remain compatibility aliases."
        ),
        ("var edgeType = model.GetCurveType(edge);", "var edgeType = model.GetEdgeCurveType(edge);"),
        ("var faceType = model.GetSurfaceType(face);", "var faceType = model.GetFaceSurfaceType(face);")
    ])

update(
    "docs/API_COVERAGE.zh-CN.md",
    [
        (
            "`GetCurveType()` 和 `GetSurfaceType()` 用于判断几何类型；确认类型后，可读取精确解析参数，而不是通过离散采样反推半径、轴线和中心。",
            "优先使用 `GetEdgeCurveType()` 和 `GetFaceSurfaceType()` 判断几何类型；确认类型后，可读取精确解析参数，而不是通过离散采样反推半径、轴线和中心。旧的 `GetCurveType()` 和 `GetSurfaceType()` 仅作为兼容别名保留。"
        ),
        ("var edgeType = model.GetCurveType(edge);", "var edgeType = model.GetEdgeCurveType(edge);"),
        ("var faceType = model.GetSurfaceType(face);", "var faceType = model.GetFaceSurfaceType(face);")
    ])

Path(__file__).unlink()
