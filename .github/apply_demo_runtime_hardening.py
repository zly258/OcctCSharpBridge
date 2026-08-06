from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    normalized = text.replace("\r\n", "\n").replace("\n", "\r\n")
    target.write_bytes(BOM + normalized.encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


publish_path = "publish.ps1"
publish = read(publish_path)

# Do not treat VC runtime files as operating-system DLLs merely because they exist in System32.
publish = replace_once(
    publish,
    """function Test-SystemDependency {
    param([Parameter(Mandatory = $true)][string]$Name)

    if ($Name -match "^(?i:api-ms-win-|ext-ms-win-)") {
        return $true
    }

    return Test-Path (Join-Path ([Environment]::SystemDirectory) $Name) -PathType Leaf
}
""",
    """function Test-VisualCppRuntimeDependency {
    param([Parameter(Mandatory = $true)][string]$Name)

    return $Name -match "^(?i:concrt140|msvcp140(?:_[0-9]+|_atomic_wait|_codecvt_ids)?|vcruntime140(?:_[0-9]+|_threads)?)\\.dll$"
}

function Test-SystemDependency {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (Test-VisualCppRuntimeDependency $Name) {
        return $false
    }
    if ($Name -match "^(?i:api-ms-win-|ext-ms-win-)") {
        return $true
    }

    return Test-Path (Join-Path ([Environment]::SystemDirectory) $Name) -PathType Leaf
}
""",
    "VC runtime dependency classification")

# Add VS redistributable and System32 VC runtime files to the candidate index.
publish = replace_once(
    publish,
    """function New-RuntimeCandidateIndex {
    $index = @{}
    $files = [System.Collections.Generic.List[System.IO.FileInfo]]::new()

    Get-ChildItem $OcctBinDir -File -Filter "*.dll" | ForEach-Object { $files.Add($_) }
""",
    """function Get-VisualCppRuntimeFiles {
    $result = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\\Installer\\vswhere.exe"
    if (Test-Path $vswhere -PathType Leaf) {
        $matches = @(& $vswhere `
            -latest `
            -products * `
            -requires Microsoft.VisualStudio.Component.VC.Redist.14.Latest `
            -find "VC\\Redist\\MSVC\\**\\x64\\Microsoft.VC*.CRT\\*.dll" 2>$null)
        foreach ($match in $matches) {
            if ([string]::IsNullOrWhiteSpace($match) -or -not (Test-Path $match -PathType Leaf)) { continue }
            $file = Get-Item -LiteralPath $match
            if ((Test-VisualCppRuntimeDependency $file.Name) -and $seen.Add($file.FullName)) {
                $result.Add($file)
            }
        }
    }

    foreach ($name in @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "msvcp140_atomic_wait.dll",
        "msvcp140_codecvt_ids.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll",
        "vcruntime140_threads.dll"
    )) {
        $path = Join-Path ([Environment]::SystemDirectory) $name
        if (Test-Path $path -PathType Leaf) {
            $file = Get-Item -LiteralPath $path
            if ($seen.Add($file.FullName)) { $result.Add($file) }
        }
    }

    return @($result)
}

function New-RuntimeCandidateIndex {
    $index = @{}
    $files = [System.Collections.Generic.List[System.IO.FileInfo]]::new()

    Get-ChildItem $OcctBinDir -File -Filter "*.dll" | ForEach-Object { $files.Add($_) }
    Get-VisualCppRuntimeFiles | ForEach-Object { $files.Add($_) }
""",
    "VC runtime candidate discovery")

# Required resource directories must be present; silently incomplete packages are not acceptable.
publish = replace_once(
    publish,
    """    foreach ($resourceName in $resourceNames) {
        $source = Find-OcctResource $resourceName
        if ([string]::IsNullOrWhiteSpace($source)) {
            continue
        }
        $destination = Join-Path $resourceDestination $resourceName
        Copy-Item $source $destination -Recurse -Force
    }
""",
    """    foreach ($resourceName in $resourceNames) {
        $source = Find-OcctResource $resourceName
        if ([string]::IsNullOrWhiteSpace($source)) {
            throw "Required OCCT resource directory was not found: $resourceName"
        }
        $destination = Join-Path $resourceDestination $resourceName
        Copy-Item $source $destination -Recurse -Force
    }
""",
    "required OCCT resources")

# Add deterministic closure verification and machine-readable package contract.
marker = "function Write-LicenseFiles {\n"
insert = r'''function Test-PackagedNativeClosure {
    $dumpbin = Resolve-Dumpbin
    $runtimeFiles = @(Get-ChildItem $RuntimeRoot -File -Filter "*.dll" | Sort-Object Name)
    if ($runtimeFiles.Count -eq 0) {
        throw "The packaged runtime directory contains no DLL files."
    }

    $packagedNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($file in $runtimeFiles) { [void]$packagedNames.Add($file.Name) }

    $rows = [System.Collections.Generic.List[string]]::new()
    $unresolved = [System.Collections.Generic.List[string]]::new()
    foreach ($file in $runtimeFiles) {
        $dependencies = @(Get-PeDependencies $dumpbin $file.FullName)
        $rows.Add("$($file.Name)`t$($dependencies -join ',')")
        foreach ($dependency in $dependencies) {
            if ($packagedNames.Contains($dependency)) { continue }
            if (Test-SystemDependency $dependency) { continue }
            $unresolved.Add("$($file.Name) -> $dependency")
        }
    }

    [System.IO.File]::WriteAllLines(
        (Join-Path $PackageRoot "native-dependencies.txt"),
        @("Binary`tDependencies") + $rows,
        $utf8Bom)

    if ($unresolved.Count -gt 0) {
        throw "The package has unresolved non-system native dependencies:`n$($unresolved -join "`n")"
    }
}

function Write-PackageContract {
    $applications = [System.Collections.Generic.List[object]]::new()
    foreach ($key in @("winform", "wpf")) {
        if ($Target -ne "all" -and $Target -ne $key) { continue }
        $application = $Projects[$key]
        $relativeExecutable = Join-Path (Join-Path "apps" $application.Folder) $application.Executable
        $fullExecutable = Join-Path $PackageRoot $relativeExecutable
        Assert-Path $fullExecutable "$($application.Name) packaged executable"
        $applications.Add([ordered]@{
            name = $application.Name
            executable = $relativeExecutable.Replace('\\', '/')
            selfContained = [bool]$UseSelfContained
        })
    }

    $resources = @(Get-ChildItem (Join-Path $OcctPackageRoot "src") -Directory | Sort-Object Name | Select-Object -ExpandProperty Name)
    $contract = [ordered]@{
        schemaVersion = 1
        packageName = $PackageName
        platform = "windows-x64"
        configuration = $Configuration
        selfContained = [bool]$UseSelfContained
        managedRuntime = if ($UseSelfContained) { "embedded-in-application" } else { "requires-.NET-8-Desktop-Runtime" }
        applications = $applications
        nativeRuntimeDirectory = "runtime"
        occtRootDirectory = "occt"
        occtResourceDirectories = $resources
        dependencyManifest = "native-dependencies.txt"
        licenseDirectory = "licenses"
    }

    $json = $contract | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText((Join-Path $PackageRoot "package-contract.json"), $json, $utf8Bom)
}

'''
if marker not in publish:
    raise RuntimeError("Write-LicenseFiles marker was not found.")
publish = publish.replace(marker, insert + marker, 1)

# Expand the VC fallback list and make missing directly imported CRT files an error through closure validation.
publish = replace_once(
    publish,
    """    $runtimeNames = @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll"
    )
""",
    """    $runtimeNames = @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "msvcp140_atomic_wait.dll",
        "msvcp140_codecvt_ids.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll",
        "vcruntime140_threads.dll"
    )
""",
    "expanded VC runtime fallback")

# Call validation and write the contract before packaging/ZIP.
publish = replace_once(
    publish,
    """    Copy-OcctRuntime
    Copy-VisualCppRuntime
    Copy-OcctResources
    Write-LicenseFiles
    Write-PackageReadme
    Write-Manifest

""",
    """    Copy-OcctRuntime
    Copy-VisualCppRuntime
    Copy-OcctResources
    Test-PackagedNativeClosure
    Write-LicenseFiles
    Write-PackageReadme
    Write-PackageContract
    Write-Manifest

""",
    "package validation sequence")

# Package README must explicitly describe all mandatory runtime components.
publish = replace_once(
    publish,
    '        "The default package contains both WinForms and WPF executables, the native dependency closure,",\n        "the required OCCT resources and consolidated license notices.",\n',
    '        "The default package contains both WinForms and WPF executables with the .NET runtime embedded,",\n        "OcctNative.dll, the complete OCCT/third-party/Visual C++ native dependency closure,",\n        "required OCCT resources, package-contract.json, native-dependencies.txt and license notices.",\n',
    "package README dependency description")

write(publish_path, publish)

# Static package rules kept outside workflow YAML so they can also run locally.
write("tests/check-demo-package.ps1", r'''param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$publishPath = Join-Path $RepositoryRoot "publish.ps1"
if (-not (Test-Path $publishPath -PathType Leaf)) { throw "publish.ps1 was not found." }
$text = [System.IO.File]::ReadAllText($publishPath)

foreach ($token in @(
    '[string]$Target = "all"',
    '$UseSelfContained = -not $FrameworkDependent.IsPresent',
    '--self-contained", $UseSelfContained.ToString().ToLowerInvariant()',
    'function Test-PackagedNativeClosure',
    'function Write-PackageContract',
    'package-contract.json',
    'native-dependencies.txt',
    'Get-VisualCppRuntimeFiles',
    'msvcp140_atomic_wait.dll',
    'vcruntime140_threads.dll',
    'throw "Required OCCT resource directory was not found: $resourceName"'
)) {
    if (-not $text.Contains($token)) { throw "Required package token is missing: $token" }
}

if ($text -match 'return Test-Path \(Join-Path \(\[Environment\]::SystemDirectory\) \$Name\)' -and
    -not $text.Contains('Test-VisualCppRuntimeDependency')) {
    throw "VC runtime dependencies may still be incorrectly classified as system DLLs."
}

Write-Host "[package] Self-contained .NET, OCCT, third-party, VC runtime, resources and closure validation rules passed." -ForegroundColor Green
''')

# Update branch-specific documentation without changing the reusable main README.
readme_path = "README.md"
readme = read(readme_path)
needle = "The default command publishes both WinForms and WPF as self-contained Windows x64 applications. Target computers do not need a separate .NET installation.\n"
replacement = needle + "\nThe package is intentionally deployment-complete: each executable embeds its .NET runtime, while `runtime` contains `OcctNative.dll`, the recursively resolved OCCT/third-party/Visual C++ DLL closure, and `occt/src` contains the required OCCT resources. Publishing fails when a required native dependency or OCCT resource is missing. `package-contract.json` and `native-dependencies.txt` describe the generated package.\n"
readme = replace_once(readme, needle, replacement, "English complete package guidance")
write(readme_path, readme)

readme_zh_path = "README.zh-CN.md"
readme_zh = read(readme_zh_path)
needle_zh = "默认命令同时发布 WinForms 和 WPF，并生成 Windows x64 自包含程序；目标电脑不需要另外安装 .NET。\n"
replacement_zh = needle_zh + "\n发布包按可直接部署设计：两个可执行程序分别内嵌 .NET 运行时，`runtime` 包含 `OcctNative.dll` 以及递归解析得到的 OCCT、第三方库和 Visual C++ DLL 依赖闭包，`occt/src` 包含必须的 OCCT 资源。缺少任何必需原生依赖或 OCCT 资源时发布会直接失败；`package-contract.json` 与 `native-dependencies.txt` 用于说明和校核包内容。\n"
readme_zh = replace_once(readme_zh, needle_zh, replacement_zh, "Chinese complete package guidance")
write(readme_zh_path, readme_zh)

# The migration helper is one-shot; the workflow is removed separately after validation.
(ROOT / ".github/apply_demo_runtime_hardening.py").unlink()
print("Demo runtime hardening applied.")
