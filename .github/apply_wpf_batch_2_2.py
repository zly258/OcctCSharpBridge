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


# Native ABI declarations and build inputs.
header = read("src/OcctNative/OcctNative.h")
anchor = "    OCCTBRIDGE_API int occt_set_object_material(OcctHandle handle, OcctObjectId objectId, int material);\n"
batch_declarations = anchor + """    OCCTBRIDGE_API int occt_set_objects_color(OcctHandle handle, const OcctObjectId* objectIds, int count, double r, double g, double b);
    OCCTBRIDGE_API int occt_set_objects_transparency(OcctHandle handle, const OcctObjectId* objectIds, int count, double transparency);
    OCCTBRIDGE_API int occt_set_objects_visible(OcctHandle handle, const OcctObjectId* objectIds, int count, int visible);
    OCCTBRIDGE_API int occt_set_objects_display_mode(OcctHandle handle, const OcctObjectId* objectIds, int count, int displayMode);
    OCCTBRIDGE_API int occt_set_objects_line_width(OcctHandle handle, const OcctObjectId* objectIds, int count, double width);
    OCCTBRIDGE_API int occt_set_objects_material(OcctHandle handle, const OcctObjectId* objectIds, int count, int material);
    OCCTBRIDGE_API int occt_redisplay_objects(OcctHandle handle, const OcctObjectId* objectIds, int count);
    OCCTBRIDGE_API int occt_select_objects(OcctHandle handle, const OcctObjectId* objectIds, int count, int appendSelection);
    OCCTBRIDGE_API int occt_object_is_visible(OcctHandle handle, OcctObjectId objectId);
    OCCTBRIDGE_API int occt_object_is_selected(OcctHandle handle, OcctObjectId objectId);
"""
header = replace_once(header, anchor, batch_declarations, "batch native declarations")
write("src/OcctNative/OcctNative.h", header)

internal = read("src/OcctNative/OcctInternal.hxx")
internal = replace_once(
    internal,
    "    void fillMassProperties(const GProp_GProps& properties, OcctMassProperties* result);\n",
    "    void fillMassProperties(const GProp_GProps& properties, OcctMassProperties* result);\n"
    "    Graphic3d_NameOfMaterial materialName(int value);\n",
    "material helper declaration")
write("src/OcctNative/OcctInternal.hxx", internal)

cmake = read("src/OcctNative/CMakeLists.txt")
cmake = replace_once(
    cmake,
    "    OcctViewportExtensions.cpp\n",
    "    OcctViewportExtensions.cpp\n    OcctObjectBatch.cpp\n",
    "batch CMake source")
write("src/OcctNative/CMakeLists.txt", cmake)

engine = read("src/OcctNative/OcctEngine.cpp")
if engine.count("2.1.0") < 2:
    raise RuntimeError("Native bridge version markers were not found.")
engine = engine.replace("2.1.0", "2.2.0")
write("src/OcctNative/OcctEngine.cpp", engine)

bridge_info = read("src/OcctNet/OcctBridgeInfo.cs")
bridge_info = replace_once(
    bridge_info,
    'public const string ManagedVersion = "2.1.0";',
    'public const string ManagedVersion = "2.2.0";',
    "managed bridge version")
write("src/OcctNet/OcctBridgeInfo.cs", bridge_info)

# Solution and build integration for the reusable WPF host.
solution = read("OcctBridge.sln")
wpf_guid = "{B65176A4-9A31-4C15-BE31-2F4FB10EFB56}"
solution = replace_once(
    solution,
    'Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "OcctNet.WinForms", "src\\OcctNet.WinForms\\OcctNet.WinForms.csproj", "{7EA6CC4D-34D3-4A91-9C6A-CFC4E339CE58}"\nEndProject\n',
    'Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "OcctNet.WinForms", "src\\OcctNet.WinForms\\OcctNet.WinForms.csproj", "{7EA6CC4D-34D3-4A91-9C6A-CFC4E339CE58}"\nEndProject\n'
    f'Project("{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}") = "OcctNet.Wpf", "src\\OcctNet.Wpf\\OcctNet.Wpf.csproj", "{wpf_guid}"\nEndProject\n',
    "WPF solution project")
config_anchor = """        {7EA6CC4D-34D3-4A91-9C6A-CFC4E339CE58}.Release|x64.ActiveCfg = Release|x64
        {7EA6CC4D-34D3-4A91-9C6A-CFC4E339CE58}.Release|x64.Build.0 = Release|x64
"""
config_insert = config_anchor + f"""        {wpf_guid}.Debug|x64.ActiveCfg = Debug|x64
        {wpf_guid}.Debug|x64.Build.0 = Debug|x64
        {wpf_guid}.Release|x64.ActiveCfg = Release|x64
        {wpf_guid}.Release|x64.Build.0 = Release|x64
"""
solution = replace_once(solution, config_anchor, config_insert, "WPF solution configurations")
write("OcctBridge.sln", solution)

build = read("build.ps1")
build = replace_once(
    build,
    '$WinFormsProject = Join-Path $RepoRoot "src\\OcctNet.WinForms\\OcctNet.WinForms.csproj"\n',
    '$WinFormsProject = Join-Path $RepoRoot "src\\OcctNet.WinForms\\OcctNet.WinForms.csproj"\n'
    '$WpfProject = Join-Path $RepoRoot "src\\OcctNet.Wpf\\OcctNet.Wpf.csproj"\n',
    "WPF build project")
build = replace_once(
    build,
    '$WinFormsOutput = Join-Path $RepoRoot "src\\OcctNet.WinForms\\bin\\x64\\$Configuration\\net8.0-windows"\n',
    '$WinFormsOutput = Join-Path $RepoRoot "src\\OcctNet.WinForms\\bin\\x64\\$Configuration\\net8.0-windows"\n'
    '$WpfOutput = Join-Path $RepoRoot "src\\OcctNet.Wpf\\bin\\x64\\$Configuration\\net8.0-windows"\n',
    "WPF build output")
build = replace_once(
    build,
    '$ViewportApiCheck = Join-Path $RepoRoot "tests\\check-viewport-api.ps1"\n',
    '$ViewportApiCheck = Join-Path $RepoRoot "tests\\check-viewport-api.ps1"\n'
    '$UiHostsCheck = Join-Path $RepoRoot "tests\\check-ui-hosts.ps1"\n',
    "UI host validation path")
build = replace_once(
    build,
    '    Assert-Path $ViewportApiCheck\n\n    Write-Host "[viewport] Validating extended viewport contracts..." -ForegroundColor Cyan\n',
    '    Assert-Path $ViewportApiCheck\n'
    '    Assert-Path $UiHostsCheck\n\n'
    '    Write-Host "[ui-hosts] Validating WinForms and WPF viewport hosts..." -ForegroundColor Cyan\n'
    '    & $UiHostsCheck -RepositoryRoot $RepoRoot\n'
    '    if (-not $?) {\n'
    '        throw "UI host validation failed."\n'
    '    }\n\n'
    '    Write-Host "[viewport] Validating extended viewport contracts..." -ForegroundColor Cyan\n',
    "UI host validation invocation")
build = replace_once(
    build,
    '    foreach ($project in @($ManagedProject, $WinFormsProject)) {\n',
    '    foreach ($project in @($ManagedProject, $WinFormsProject, $WpfProject)) {\n',
    "managed cleanup projects")
build = replace_once(
    build,
    '    Assert-Path (Join-Path $ManagedOutput "OcctNet.dll")\n    Assert-Path (Join-Path $WinFormsOutput "OcctNet.WinForms.dll")\n',
    '    Write-Host "[managed] Building optional WPF host ($Configuration)..." -ForegroundColor Cyan\n'
    '    Invoke-Checked "dotnet" @(\n'
    '        "build", $WpfProject,\n'
    '        "-c", $Configuration,\n'
    '        "-p:Platform=x64",\n'
    '        "--nologo"\n'
    '    ) "OcctNet.Wpf build failed."\n\n'
    '    Assert-Path (Join-Path $ManagedOutput "OcctNet.dll")\n'
    '    Assert-Path (Join-Path $WinFormsOutput "OcctNet.WinForms.dll")\n'
    '    Assert-Path (Join-Path $WpfOutput "OcctNet.Wpf.dll")\n',
    "WPF build invocation")
build = replace_once(
    build,
    '    Write-Host "Managed WinForms: $WinFormsOutput" -ForegroundColor Green\n',
    '    Write-Host "Managed WinForms: $WinFormsOutput" -ForegroundColor Green\n'
    '    Write-Host "Managed WPF:      $WpfOutput" -ForegroundColor Green\n',
    "WPF build result")
write("build.ps1", build)

api_workflow = read(".github/workflows/api-surface.yml")
api_workflow = replace_once(
    api_workflow,
    "      - name: Compile smoke test\n",
    "      - name: Build optional WPF host\n"
    "        shell: pwsh\n"
    "        run: dotnet build .\\src\\OcctNet.Wpf\\OcctNet.Wpf.csproj -c Release -p:Platform=x64 --nologo\n\n"
    "      - name: Compile smoke test\n",
    "WPF CI build")
write(".github/workflows/api-surface.yml", api_workflow)

sync_workflow = read(".github/workflows/wrapper-sync.yml")
sync_workflow = replace_once(
    sync_workflow,
    '            "src/OcctNet.WinForms",\n',
    '            "src/OcctNet.WinForms",\n            "src/OcctNet.Wpf",\n',
    "WPF branch synchronization")
write(".github/workflows/wrapper-sync.yml", sync_workflow)

# Documentation and API inventory.
batch_functions = [
    "occt_object_is_selected",
    "occt_object_is_visible",
    "occt_redisplay_objects",
    "occt_select_objects",
    "occt_set_objects_color",
    "occt_set_objects_display_mode",
    "occt_set_objects_line_width",
    "occt_set_objects_material",
    "occt_set_objects_transparency",
    "occt_set_objects_visible",
]


def update_inventory(path: str, chinese: bool) -> None:
    text = read(path)
    text = re.sub(r"Native exports:\s*`?\d+`?", "Native exports: `307`", text, count=1)
    text = re.sub(r"Managed P/Invoke declarations:\s*`?\d+`?", "Managed P/Invoke declarations: `307`", text, count=1)
    text = re.sub(r"Public \.NET types:\s*`?\d+`?", "Public .NET types: `60`", text, count=1)
    text = text.replace("Registry, AIS attributes and lifecycle (23)", "Registry, AIS attributes and lifecycle (33)", 1)
    anchor_line = "- `occt_object_kind`\n"
    addition = anchor_line + "".join(f"- `{name}`\n" for name in batch_functions)
    text = replace_once(text, anchor_line, addition, f"batch API list in {path}")
    text = replace_once(text, "- `OcctViewportControl`\n", "- `OcctViewportControl`\n- `OcctWpfViewport`\n", f"WPF public type in {path}")
    text = text.replace("2.1.0", "2.2.0")
    if chinese:
        old_note = "上述 `OcctViewport*` 类型由可选的 `OcctNet.WinForms` 程序集提供；其余托管类型仍位于不依赖 UI 的 `OcctNet` 程序集中。"
        new_note = "`OcctViewportControl` 及其事件参数由可选的 `OcctNet.WinForms` 程序集提供；`OcctWpfViewport` 由 `OcctNet.Wpf` 提供；其余托管类型位于不依赖 UI 的 `OcctNet` 程序集中。"
    else:
        old_note = "The `OcctViewport*` types above are provided by the optional `OcctNet.WinForms` assembly; all remaining managed types stay in the UI-independent `OcctNet` assembly."
        new_note = "`OcctViewportControl` and its event types are provided by `OcctNet.WinForms`; `OcctWpfViewport` is provided by `OcctNet.Wpf`; all remaining managed types stay in the UI-independent `OcctNet` assembly."
    text = replace_once(text, old_note, new_note, f"UI assembly note in {path}")
    write(path, text)


update_inventory("docs/API_COVERAGE.md", chinese=False)
update_inventory("docs/API_COVERAGE.zh-CN.md", chinese=True)

readme = read("README.md")
readme = replace_once(
    readme,
    "src/OcctNet.WinForms   Optional WinForms OCCT viewport control\n",
    "src/OcctNet.WinForms   Optional WinForms OCCT viewport control\n"
    "src/OcctNet.Wpf        Optional WPF OCCT viewport control\n",
    "English WPF structure")
readme = replace_once(
    readme,
    "- `OcctViewportControl` is provided separately by `OcctNet.WinForms`; the core wrapper no longer depends on WinForms.\n",
    "- `OcctViewportControl` is provided by `OcctNet.WinForms`; `OcctWpfViewport` is provided by `OcctNet.Wpf`.\n",
    "English UI host description")
readme = replace_once(
    readme,
    "  <!-- Add only when a WinForms/WPF host needs OcctViewportControl. -->\n  <ProjectReference Include=\"..\\OcctCSharpBridge\\src\\OcctNet.WinForms\\OcctNet.WinForms.csproj\" />\n",
    "  <!-- WinForms host. -->\n"
    "  <ProjectReference Include=\"..\\OcctCSharpBridge\\src\\OcctNet.WinForms\\OcctNet.WinForms.csproj\" />\n"
    "  <!-- WPF host; references the WinForms HWND host internally. -->\n"
    "  <ProjectReference Include=\"..\\OcctCSharpBridge\\src\\OcctNet.Wpf\\OcctNet.Wpf.csproj\" />\n",
    "English project references")
readme = replace_once(
    readme,
    "The bridge intentionally excludes OCAF/XDE.",
    "Batch color, transparency, visibility, display-mode, line-width, material, redisplay, and selection operations reduce repeated P/Invoke calls for large scenes.\n\nThe bridge intentionally excludes OCAF/XDE.",
    "English batch summary")
write("README.md", readme)

readme_zh = read("README.zh-CN.md")
readme_zh = replace_once(
    readme_zh,
    "src/OcctNet.WinForms   可选的 WinForms OCCT 视口控件\n",
    "src/OcctNet.WinForms   可选的 WinForms OCCT 视口控件\n"
    "src/OcctNet.Wpf        可选的 WPF OCCT 视口控件\n",
    "Chinese WPF structure")
readme_zh = replace_once(
    readme_zh,
    "桥接层不再包含 OCAF/XDE。",
    "新增批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择接口，减少大型场景中的重复 P/Invoke 调用。\n\n桥接层不再包含 OCAF/XDE。",
    "Chinese batch summary")
readme_zh = replace_once(
    readme_zh,
    "  <!-- 仅 WinForms/WPF 宿主需要 OcctViewportControl 时引用。 -->\n  <ProjectReference Include=\"..\\OcctCSharpBridge\\src\\OcctNet.WinForms\\OcctNet.WinForms.csproj\" />\n",
    "  <!-- WinForms 宿主。 -->\n"
    "  <ProjectReference Include=\"..\\OcctCSharpBridge\\src\\OcctNet.WinForms\\OcctNet.WinForms.csproj\" />\n"
    "  <!-- WPF 宿主，内部复用 WinForms HWND 宿主。 -->\n"
    "  <ProjectReference Include=\"..\\OcctCSharpBridge\\src\\OcctNet.Wpf\\OcctNet.Wpf.csproj\" />\n",
    "Chinese project references")
write("README.zh-CN.md", readme_zh)

# Remove one-shot migration files after the validated commit is produced.
(ROOT / ".github/apply_wpf_batch_2_2.py").unlink()
(ROOT / ".github/workflows/apply-wpf-batch-2-2.yml").unlink()
