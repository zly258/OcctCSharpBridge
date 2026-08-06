from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    target = ROOT / path
    normalized = text.replace("\r\n", "\n").replace("\n", "\r\n")
    target.write_bytes(BOM + normalized.encode("utf-8"))


def replace_once(text: str, old: str, new: str, name: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block was not found: {name}")
    return text.replace(old, new, 1)


native_ocaf_files = [
    "src/OcctNative/OcctOcaf.h",
    "src/OcctNative/OcctOcafExtended.h",
    "src/OcctNative/OcctOcafInternal.hxx",
    "src/OcctNative/OcctOcafCore.cpp",
    "src/OcctNative/OcctOcafAttributes.cpp",
    "src/OcctNative/OcctOcafNaming.cpp",
    "src/OcctNative/OcctOcafXde.cpp",
    "src/OcctNative/OcctOcafXdeAppearance.cpp",
    "src/OcctNative/OcctOcafXdeProperties.cpp",
    "src/OcctNative/OcctOcafExchange.cpp",
    "src/OcctNative/OcctOcafExtendedDocument.cpp",
    "src/OcctNative/OcctOcafExtendedXde.cpp",
]

managed_ocaf_files = [
    "src/OcctNet/OcafDocument.cs",
    "src/OcctNet/OcafDocument.Attributes.cs",
    "src/OcctNet/OcafDocument.Extended.cs",
    "src/OcctNet/OcafDocument.Labels.cs",
    "src/OcctNet/OcafDocument.Naming.cs",
    "src/OcctNet/OcafDocument.Xde.cs",
    "src/OcctNet/OcafExtendedTypes.cs",
    "src/OcctNet/OcafNativeMethods.Core.cs",
    "src/OcctNet/OcafNativeMethods.Attributes.cs",
    "src/OcctNet/OcafNativeMethods.Extended.cs",
    "src/OcctNet/OcafNativeMethods.Naming.cs",
    "src/OcctNet/OcafNativeMethods.Xde.cs",
    "src/OcctNet/OcafTypes.cs",
]

for relative in native_ocaf_files + managed_ocaf_files:
    path = ROOT / relative
    if not path.exists():
        raise RuntimeError(f"Expected OCAF/XDE file was not found: {relative}")
    path.unlink()

cmake = read("src/OcctNative/CMakeLists.txt")
cmake = re.sub(r"(?m)^\s+OcctOcaf[^\n]*\n", "", cmake)
cmake, count = re.subn(
    r"\n# These toolkits are directly referenced by the OCAF/XDE bridge and are mandatory\.\n"
    r"set\(ocaf_required_libraries.*?list\(REMOVE_DUPLICATES occt_libraries\)\n",
    "\nlist(REMOVE_DUPLICATES occt_libraries)\n",
    cmake,
    flags=re.DOTALL,
)
if count != 1:
    raise RuntimeError("Unable to remove the mandatory OCAF toolkit block from CMakeLists.txt.")
if "OcctOcaf" in cmake or "ocaf_required_libraries" in cmake or "TKXCAF" in cmake:
    raise RuntimeError("OCAF/XDE build inputs remain in CMakeLists.txt.")
write("src/OcctNative/CMakeLists.txt", cmake)

engine = read("src/OcctNative/OcctEngine.cpp")
engine = replace_once(engine, "int occt_bridge_abi_version() { return 1; }", "int occt_bridge_abi_version() { return 2; }", "native ABI version")
engine = engine.replace('return "1.1.0";', 'return "2.0.0";', 1)
engine = engine.replace('std::string("OcctCSharpBridge/1.1.0; ABI=1; OCCT=")', 'std::string("OcctCSharpBridge/2.0.0; ABI=2; OCCT=")', 1)
write("src/OcctNative/OcctEngine.cpp", engine)

bridge_info = read("src/OcctNet/OcctBridgeInfo.cs")
bridge_info = replace_once(bridge_info, "public const int ExpectedAbiVersion = 1;", "public const int ExpectedAbiVersion = 2;", "managed ABI version")
bridge_info = replace_once(bridge_info, 'public const string ManagedVersion = "1.1.0";', 'public const string ManagedVersion = "2.0.0";', "managed bridge version")
write("src/OcctNet/OcctBridgeInfo.cs", bridge_info)

api_check = read("tests/check-api-surface.ps1")
api_check = api_check.replace('    Join-Path $nativeRoot "OcctOcaf.h"\n', "")
api_check = api_check.replace('    Join-Path $nativeRoot "OcctOcafExtended.h"\n', "")
old_groups = """$groups = [ordered]@{
    Viewer = @($declarations | Where-Object { $_ -notlike 'occt_model_*' -and $_ -notlike 'occt_ocaf_*' })
    Modeling = @($declarations | Where-Object { $_ -like 'occt_model_*' })
    Ocaf = @($declarations | Where-Object { $_ -like 'occt_ocaf_*' })
}
"""
new_groups = """$ocafExports = @($declarations | Where-Object { $_ -like 'occt_ocaf_*' })
if ($ocafExports.Count -ne 0) {
    throw "OCAF/XDE exports are not allowed in the reusable bridge."
}

$groups = [ordered]@{
    Viewer = @($declarations | Where-Object { $_ -notlike 'occt_model_*' })
    Modeling = @($declarations | Where-Object { $_ -like 'occt_model_*' })
}
"""
api_check = replace_once(api_check, old_groups, new_groups, "API groups")
write("tests/check-api-surface.ps1", api_check)

smoke = read("tests/OcctNet.Smoke/Program.cs")
start = smoke.find("var xbfPath =")
end = smoke.find('Console.WriteLine($"OCCT ')
if start < 0 or end < 0 or end <= start:
    raise RuntimeError("Unable to locate the OCAF/XDE smoke-test block.")
smoke = smoke[:start] + smoke[end:]
smoke = smoke.replace('Console.WriteLine($"OCAF capabilities: {OcafDocument.Capabilities}");\n', "")
smoke = smoke.replace('Console.WriteLine("Modeling and extended OCAF/XDE smoke tests passed.");', 'Console.WriteLine("Modeling smoke tests passed.");')
write("tests/OcctNet.Smoke/Program.cs", smoke)

readme = read("README.md")
readme = replace_once(readme, "The wrapper provides three session types:", "The wrapper provides two native session types:", "English session count")
readme = readme.replace("- `OcafDocument`: OCAF/TNaming/XDE documents, assemblies, metadata, persistence, and undo/redo.\n", "")
readme = readme.replace("# Build and run native modeling/OCAF smoke scenarios.", "# Build and run native modeling smoke scenarios.")
readme = readme.replace(
    "- `OcctModelingSession`: headless geometry, topology, algorithms, mesh, analysis, healing, and exchange.\n",
    "- `OcctModelingSession`: headless geometry, topology, algorithms, mesh, analysis, healing, and exchange.\n\n"
    "The bridge intentionally excludes OCAF/XDE. Application documents, undo/redo, and JSON persistence belong to the consuming application rather than the geometry bridge.\n",
    1,
)
write("README.md", readme)

readme_zh = read("README.zh-CN.md")
readme_zh = replace_once(readme_zh, "封装提供三类会话：", "封装提供两类原生会话：", "Chinese session count")
readme_zh = readme_zh.replace("- `OcafDocument`：OCAF、TNaming、XDE 文档、装配、元数据、持久化和撤销重做。\n", "")
readme_zh = readme_zh.replace("# 构建并执行建模及 OCAF 原生测试。", "# 构建并执行原生建模测试。")
readme_zh = readme_zh.replace(
    "- `OcctModelingSession`：无窗口几何、拓扑、算法、网格、分析、修复和文件交换。\n",
    "- `OcctModelingSession`：无窗口几何、拓扑、算法、网格、分析、修复和文件交换。\n\n"
    "桥接层不再包含 OCAF/XDE。应用文档、撤销重做和 JSON 持久化由上层应用自行实现，避免把文档机制耦合进几何桥接。\n",
    1,
)
write("README.zh-CN.md", readme_zh)


def update_inventory(path: str, chinese: bool) -> None:
    text = read(path)
    text, section_count = re.subn(r"\n### OcctOcaf.*?(?=\n## )", "\n", text, flags=re.DOTALL)
    if section_count != 1:
        raise RuntimeError(f"Unable to remove OCAF API sections from {path}.")

    text = re.sub(r"(?m)^- `OcctOcaf[^`]*`\n", "", text)
    text = re.sub(r"(?m)^- `Ocaf[^`]*`\n", "", text)
    text = re.sub(r"Native exports:\s*`?\d+`?", "Native exports: `281`", text, count=1)
    text = re.sub(r"Managed P/Invoke declarations:\s*`?\d+`?", "Managed P/Invoke declarations: `281`", text, count=1)

    public_heading = "## 公开 .NET 类型" if chinese else "## Public .NET types"
    public_start = text.find(public_heading)
    if public_start < 0:
        raise RuntimeError(f"Public type section was not found in {path}.")
    public_end = text.find("\n## ", public_start + len(public_heading))
    public_section = text[public_start:] if public_end < 0 else text[public_start:public_end]
    public_count = len(re.findall(r"(?m)^- `[^`]+`$", public_section))
    text = re.sub(r"Public \.NET types:\s*`?\d+`?", f"Public .NET types: `{public_count}`", text, count=1)

    if chinese:
        text = text.replace("- 托管层要求的 ABI：`1`", "- 托管层要求的 ABI：`2`")
        text = text.replace("- 原生桥接版本：`1.1.0`", "- 原生桥接版本：`2.0.0`")
        text = text.replace("Viewer、建模或 OCAF 会话", "Viewer 或建模会话")
        note = "\n桥接层不包含 OCAF/XDE；文档、撤销重做和 JSON 持久化由上层应用实现。\n"
        anchor = "本文件由源码接口声明整理，列出当前原生 C ABI、C# P/Invoke 映射及公开 .NET 类型。\n"
    else:
        text = text.replace("- Managed expected ABI: `1`", "- Managed expected ABI: `2`")
        text = text.replace("- Native bridge version: `1.1.0`", "- Native bridge version: `2.0.0`")
        text = text.replace("viewer, modeling, or OCAF sessions", "viewer or modeling sessions")
        note = "\nOCAF/XDE is intentionally excluded; documents, undo/redo, and JSON persistence are application-layer responsibilities.\n"
        anchor = "This source-derived inventory lists the current native C ABI, C# P/Invoke mapping, and public .NET types.\n"
    if note.strip() not in text:
        text = text.replace(anchor, anchor + note, 1)

    write(path, text)


update_inventory("docs/API_COVERAGE.md", chinese=False)
update_inventory("docs/API_COVERAGE.zh-CN.md", chinese=True)

for root_name in ("src", "tests"):
    for path in (ROOT / root_name).rglob("*"):
        if not path.is_file() or path.suffix.lower() not in {".cs", ".cpp", ".h", ".hxx", ".txt", ".ps1", ".csproj"}:
            continue
        text = path.read_bytes().decode("utf-8-sig", errors="ignore")
        if re.search(r"OcctOcaf|occt_ocaf_|OcafDocument|OcafNativeMethods", text):
            raise RuntimeError(f"OCAF/XDE source reference remains: {path.relative_to(ROOT)}")

for path in native_ocaf_files + managed_ocaf_files:
    if (ROOT / path).exists():
        raise RuntimeError(f"OCAF/XDE file still exists: {path}")

(ROOT / ".github/apply_remove_ocaf_xde.py").unlink()
(ROOT / ".github/workflows/apply-remove-ocaf-xde.yml").unlink()
