from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    target = ROOT / path
    target.write_bytes(BOM + text.replace("\r\n", "\n").replace("\n", "\r\n").encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


publish_path = "publish.ps1"
publish = read(publish_path)
publish = replace_once(
    publish,
    '[string]$Target = "winform",',
    '[string]$Target = "all",',
    "default publish target")
publish = replace_once(
    publish,
    """    [switch]$SelfContained,

    [switch]$FullResources,
""",
    """    [switch]$SelfContained,

    [switch]$FrameworkDependent,

    [switch]$FullResources,
""",
    "framework-dependent switch")
publish = replace_once(
    publish,
    "$Target = $Target.ToLowerInvariant()\n$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path\n",
    """$Target = $Target.ToLowerInvariant()
if ($SelfContained.IsPresent -and $FrameworkDependent.IsPresent) {
    throw "Use either -SelfContained or -FrameworkDependent, not both."
}
$UseSelfContained = -not $FrameworkDependent.IsPresent
if ($SelfContained.IsPresent) {
    $UseSelfContained = $true
}
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
""",
    "portable runtime mode")
publish = publish.replace(
    'EnableCompressionInSingleFile=$($SelfContained.IsPresent.ToString().ToLowerInvariant())',
    'EnableCompressionInSingleFile=$($UseSelfContained.ToString().ToLowerInvariant())')
publish = publish.replace(
    '"--self-contained", $SelfContained.IsPresent.ToString().ToLowerInvariant(),',
    '"--self-contained", $UseSelfContained.ToString().ToLowerInvariant(),')
publish = replace_once(
    publish,
    "$runtimeMode = if ($SelfContained) {",
    "$runtimeMode = if ($UseSelfContained) {",
    "package runtime mode")
publish = replace_once(
    publish,
    'Write-Host "Self-contained:     $($SelfContained.IsPresent)"',
    'Write-Host "Self-contained:     $UseSelfContained"',
    "runtime console output")
publish = replace_once(
    publish,
    '        "The package contains only the selected demo executable, the native dependency closure,",\n',
    '        "The default package contains both WinForms and WPF executables, the native dependency closure,",\n',
    "package README contents")
publish = replace_once(
    publish,
    '        "Use -SelfContained for machines without the .NET 8 Desktop Runtime.",\n',
    '        "The default package is self-contained and does not require a separate .NET installation.",\n        "Use -FrameworkDependent only when all target machines already have the .NET 8 Desktop Runtime.",\n',
    "package README runtime guidance")
publish = replace_once(
    publish,
    '        "Use -FullResources only when OCAF/XCAF or texture resources are needed.",\n',
    '        "Use -FullResources only when texture resources are needed.",\n',
    "package README resource guidance")
if '$SelfContained.IsPresent.ToString()' in publish:
    raise RuntimeError("Legacy switch-based self-contained publishing remains.")
write(publish_path, publish)

build_path = "build.ps1"
build = read(build_path)
build = replace_once(
    build,
    '$ApiSurfaceCheck = Join-Path $RepoRoot "tests\\check-api-surface.ps1"\n',
    '$ApiSurfaceCheck = Join-Path $RepoRoot "tests\\check-api-surface.ps1"\n$SelectionContractCheck = Join-Path $RepoRoot "tests\\check-selection-contract.ps1"\n',
    "selection contract variable")
build = replace_once(
    build,
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck

    Write-Host "[native-build] Validating CMake sources and toolkit boundaries..." -ForegroundColor Cyan
""",
    """    Assert-Path $ApiSurfaceCheck
    Assert-Path $NativeBuildCheck
    Assert-Path $SelectionContractCheck

    Write-Host "[selection] Validating point and rectangle selection behavior..." -ForegroundColor Cyan
    & $SelectionContractCheck -RepositoryRoot $RepoRoot
    if (-not $?) {
        throw "Selection contract validation failed."
    }

    Write-Host "[native-build] Validating CMake sources and toolkit boundaries..." -ForegroundColor Cyan
""",
    "selection contract invocation")
write(build_path, build)

workflow_path = ".github/workflows/demo-build.yml"
workflow = read(workflow_path)
workflow = replace_once(
    workflow,
    """      - name: Validate native build structure
        shell: pwsh
        run: .\\tests\\check-native-build-structure.ps1 -RepositoryRoot $PWD

""",
    """      - name: Validate selection contract
        shell: pwsh
        run: .\\tests\\check-selection-contract.ps1 -RepositoryRoot $PWD

      - name: Validate native build structure
        shell: pwsh
        run: .\\tests\\check-native-build-structure.ps1 -RepositoryRoot $PWD

""",
    "demo selection workflow step")
write(workflow_path, workflow)

check_path = ".github/workflows/publish-script-check.yml"
check = read(check_path)
check = replace_once(
    check,
    """              '[switch]$SelfContained',
              '[switch]$FullResources',
""",
    """              '[switch]$SelfContained',
              '[switch]$FrameworkDependent',
              '$UseSelfContained = -not $FrameworkDependent.IsPresent',
              '[switch]$FullResources',
""",
    "publish check runtime tokens")
check = check.replace(
    'EnableCompressionInSingleFile=$($SelfContained.IsPresent.ToString().ToLowerInvariant())',
    'EnableCompressionInSingleFile=$($UseSelfContained.ToString().ToLowerInvariant())')
old_default_check = """          if ($text -match '\\[string\\]\\$Target\\s*=\\s*\"all\"') {
              throw 'The default package target must not publish both UI demos.'
          }
"""
new_default_check = """          if ($text -notmatch '\\[string\\]\\$Target\\s*=\\s*\"all\"') {
              throw 'The default package target must publish both WinForms and WPF demos.'
          }
"""
check = replace_once(check, old_default_check, new_default_check, "default target check")
check = replace_once(
    check,
    """          if ($text -match 'EnableCompressionInSingleFile=true') {
              throw 'Framework-dependent publishing must not force single-file compression.'
          }
""",
    """          if ($text -match 'EnableCompressionInSingleFile=true') {
              throw 'Single-file compression must follow the selected runtime mode.'
          }
          if ($text -match '\\$UseSelfContained\\s*=\\s*\\$false') {
              throw 'Portable self-contained publishing must remain the default.'
          }
""",
    "self-contained default check")
write(check_path, check)

readme_path = "README.md"
readme = read(readme_path)
old_publish = """## Publish

Create the smaller framework-dependent WinForms package:

```powershell
.\\publish.ps1 winform Release -OcctRoot "D:\\tools\\occt-vc144-64"
```

Publish both applications:

```powershell
.\\publish.ps1 all Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

Create a self-contained package for machines without the .NET 8 Desktop Runtime:

```powershell
.\\publish.ps1 all Release -SelfContained -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

`publish.ps1` copies only the native dependency closure and resources required by the geometry-only bridge. The referenced `OcctNet` and `OcctNet.WinForms` assemblies are included by `dotnet publish`. Enable `-FullResources` or `-Diagnostics` only when needed.
"""
new_publish = """## Publish

The default command publishes both WinForms and WPF as self-contained Windows x64 applications. Target computers do not need a separate .NET installation.

```powershell
.\\publish.ps1 -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

Publish only one application when needed:

```powershell
.\\publish.ps1 winform Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
.\\publish.ps1 wpf Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

Create a smaller framework-dependent package only for computers that already have the .NET 8 Desktop Runtime:

```powershell
.\\publish.ps1 all Release -FrameworkDependent -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

`publish.ps1` copies only the native dependency closure and resources required by the geometry-only bridge. The referenced `OcctNet` and `OcctNet.WinForms` assemblies are included by `dotnet publish`. Enable `-FullResources` or `-Diagnostics` only when needed.
"""
readme = replace_once(readme, old_publish, new_publish, "English publish documentation")
write(readme_path, readme)

readme_zh_path = "README.zh-CN.md"
readme_zh = read(readme_zh_path)
old_publish_zh = """## 发布

默认生成体积较小的框架依赖 WinForms 包：

```powershell
.\\publish.ps1 winform Release -OcctRoot "D:\\tools\\occt-vc144-64"
```

发布 WinForms 和 WPF：

```powershell
.\\publish.ps1 all Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

生成无需预装 .NET 8 Desktop Runtime 的自包含包：

```powershell
.\\publish.ps1 all Release -SelfContained -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

`publish.ps1` 只复制原生依赖闭包和纯几何桥接需要的资源；被引用的 `OcctNet` 与 `OcctNet.WinForms` 程序集由 `dotnet publish` 自动包含。`-FullResources`、`-Diagnostics` 仅在需要时开启。
"""
new_publish_zh = """## 发布

默认命令同时发布 WinForms 和 WPF，并生成 Windows x64 自包含程序；目标电脑不需要另外安装 .NET。

```powershell
.\\publish.ps1 -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

只发布其中一个程序：

```powershell
.\\publish.ps1 winform Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
.\\publish.ps1 wpf Release -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

只有目标电脑已经安装 .NET 8 Desktop Runtime 时，才使用体积较小的框架依赖模式：

```powershell
.\\publish.ps1 all Release -FrameworkDependent -Zip -OcctRoot "D:\\tools\\occt-vc144-64"
```

`publish.ps1` 只复制原生依赖闭包和纯几何桥接需要的资源；被引用的 `OcctNet` 与 `OcctNet.WinForms` 程序集由 `dotnet publish` 自动包含。`-FullResources`、`-Diagnostics` 仅在需要时开启。
"""
readme_zh = replace_once(readme_zh, old_publish_zh, new_publish_zh, "Chinese publish documentation")
write(readme_zh_path, readme_zh)

(ROOT / ".github/apply_portable_demo_publish.py").unlink()
(ROOT / ".github/workflows/apply-portable-demo-publish.yml").unlink()
