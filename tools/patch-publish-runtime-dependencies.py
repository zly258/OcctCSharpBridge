from pathlib import Path
import re


def read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8-sig")


def write(path: str, text: str) -> None:
    Path(path).write_text(text, encoding="utf-8", newline="\n")


publish_path = "publish.ps1"
text = read(publish_path)

new_runtime_functions = r'''function Resolve-Dumpbin {
    $command = Get-Command "dumpbin.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Path)) {
        return $command.Path
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere -PathType Leaf) {
        $matches = @(& $vswhere `
            -latest `
            -products * `
            -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
            -find "VC\Tools\MSVC\**\bin\Hostx64\x64\dumpbin.exe" 2>$null)
        foreach ($match in $matches) {
            if (-not [string]::IsNullOrWhiteSpace($match) -and (Test-Path $match -PathType Leaf)) {
                return [System.IO.Path]::GetFullPath($match)
            }
        }
    }

    throw "dumpbin.exe was not found. Install the Visual Studio C++ x64 build tools used to build OcctNative.dll."
}

function Get-PeDependencies {
    param(
        [Parameter(Mandatory = $true)][string]$DumpbinPath,
        [Parameter(Mandatory = $true)][string]$BinaryPath
    )

    $output = @(& $DumpbinPath /nologo /dependents $BinaryPath 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect native dependencies: $BinaryPath"
    }

    $collecting = $false
    $dependencies = [System.Collections.Generic.List[string]]::new()
    foreach ($line in $output) {
        $value = ([string]$line).Trim()
        if ($value -match "^Image has the following dependencies:") {
            $collecting = $true
            continue
        }
        if (-not $collecting) {
            continue
        }
        if ($value -eq "Summary") {
            break
        }
        if ($value -match "^[A-Za-z0-9_.+\-]+\.dll$") {
            $dependencies.Add($value)
        }
    }

    return @($dependencies | Sort-Object -Unique)
}

function Test-SystemDependency {
    param([Parameter(Mandatory = $true)][string]$Name)

    if ($Name -match "^(?i:api-ms-win-|ext-ms-win-)") {
        return $true
    }

    return Test-Path (Join-Path ([Environment]::SystemDirectory) $Name) -PathType Leaf
}

function Test-RuntimeCandidate {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $path = $File.FullName.ToLowerInvariant()
    if ($path -match "[\\/](?:lib-static(?:-ucrt)?|static(?:-ucrt)?)[\\/]") {
        return $false
    }
    if ($path -match "[\\/](?:x86|win32)[\\/]") {
        return $false
    }
    if ($Configuration -ne "Debug") {
        if ($path -match "[\\/]debug[\\/]" -or $File.Name -match "(?i)(?:_debug|debug)\.dll$") {
            return $false
        }
    }
    return $true
}

function Get-RuntimeCandidateScore {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $path = $File.FullName.ToLowerInvariant()
    $score = 0
    if ([string]::Equals($File.DirectoryName, $OcctBinDir, [StringComparison]::OrdinalIgnoreCase)) {
        $score += 100000
    }
    if ($path -match "[\\/](?:bin|bin64)[\\/]") { $score += 5000 }
    if ($path -match "[\\/](?:vc2022|vc143|vc14\.4)[\\/]") { $score += 4000 }
    elseif ($path -match "[\\/](?:vc2019|vc142)[\\/]") { $score += 3000 }
    elseif ($path -match "[\\/](?:vc2017|vc141)[\\/]") { $score += 2000 }
    elseif ($path -match "[\\/](?:vc2015|vc140)[\\/]") { $score += 1000 }
    elseif ($path -match "[\\/]vc2013[\\/]") { $score += 500 }
    if ($path -match "(?:x64|amd64|win64)") { $score += 300 }
    if ($path -match "ucrt") { $score += 100 }
    if ($Configuration -eq "Debug") {
        if ($path -match "[\\/]debug[\\/]" -or $File.Name -match "(?i)(?:_debug|debug|d)\.dll$") {
            $score += 500
        }
    }
    else {
        if ($path -notmatch "[\\/]debug[\\/]") { $score += 200 }
    }
    return $score
}

function New-RuntimeCandidateIndex {
    $index = @{}
    $files = [System.Collections.Generic.List[System.IO.FileInfo]]::new()

    Get-ChildItem $OcctBinDir -File -Filter "*.dll" | ForEach-Object { $files.Add($_) }
    if (Test-Path $OcctThirdPartyDir -PathType Container) {
        Get-ChildItem $OcctThirdPartyDir -Recurse -File -Filter "*.dll" | Where-Object {
            Test-RuntimeCandidate $_
        } | ForEach-Object { $files.Add($_) }
    }

    foreach ($file in $files) {
        $key = $file.Name.ToLowerInvariant()
        if (-not $index.ContainsKey($key)) {
            $index[$key] = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
        }
        $index[$key].Add($file)
    }
    return $index
}

function Resolve-RuntimeDependency {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][hashtable]$CandidateIndex
    )

    $key = $Name.ToLowerInvariant()
    if (-not $CandidateIndex.ContainsKey($key)) {
        return $null
    }

    $ranked = @($CandidateIndex[$key] | ForEach-Object {
        [pscustomobject]@{
            File = $_
            Score = Get-RuntimeCandidateScore $_
        }
    } | Sort-Object -Property @{ Expression = "Score"; Descending = $true }, @{ Expression = { $_.File.FullName }; Descending = $false })

    if ($ranked.Count -eq 0) {
        return $null
    }

    $topScore = $ranked[0].Score
    $top = @($ranked | Where-Object { $_.Score -eq $topScore })
    if ($top.Count -gt 1) {
        $hashGroups = @($top | Group-Object { Get-FileHashValue $_.File.FullName })
        if ($hashGroups.Count -gt 1) {
            $paths = ($top | ForEach-Object { "  $($_.File.FullName)" }) -join "`n"
            throw "Ambiguous required runtime DLL '$Name'. Multiple equally ranked binaries were found:`n$paths"
        }
    }

    return $ranked[0].File
}

function Copy-OcctRuntime {
    Assert-Path $OcctBinDir "OCCT runtime directory"
    $rootBinary = Join-Path $RuntimeRoot "OcctNative.dll"
    Assert-Path $rootBinary "Packaged OcctNative.dll"

    $dumpbin = Resolve-Dumpbin
    $candidateIndex = New-RuntimeCandidateIndex
    $queue = [System.Collections.Generic.Queue[string]]::new()
    $processed = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $records = [System.Collections.Generic.List[string]]::new()
    $queue.Enqueue($rootBinary)

    Write-Host "[runtime] Resolving native dependency closure from OcctNative.dll..." -ForegroundColor Cyan
    while ($queue.Count -gt 0) {
        $binary = [System.IO.Path]::GetFullPath($queue.Dequeue())
        if (-not $processed.Add($binary)) {
            continue
        }

        foreach ($dependency in Get-PeDependencies $dumpbin $binary) {
            if (Test-SystemDependency $dependency) {
                continue
            }

            $destination = Join-Path $RuntimeRoot $dependency
            if (-not (Test-Path $destination -PathType Leaf)) {
                $source = Resolve-RuntimeDependency $dependency $candidateIndex
                if ($null -eq $source) {
                    throw "Required native dependency '$dependency' imported by '$binary' was not found in:`n  $OcctBinDir`n  $OcctThirdPartyDir"
                }
                Copy-RuntimeDll $source "required native dependency"
                $records.Add("$([System.IO.Path]::GetFileName($binary)) -> $dependency <- $($source.FullName)")
            }
            else {
                $records.Add("$([System.IO.Path]::GetFileName($binary)) -> $dependency")
            }
            $queue.Enqueue($destination)
        }
    }

    $reportPath = Join-Path $PackageRoot "native-dependencies.txt"
    [System.IO.File]::WriteAllLines($reportPath, @($records | Sort-Object -Unique), $utf8)
    Write-Host "[runtime] Native dependency closure contains $($processed.Count) binaries." -ForegroundColor Green
}
'''

pattern = re.compile(r"function Copy-OcctRuntime \{.*?\n\}\n\nfunction Copy-VisualCppRuntime", re.S)
replacement = new_runtime_functions + "\nfunction Copy-VisualCppRuntime"
text, count = pattern.subn(replacement, text, count=1)
if count != 1:
    raise SystemExit(f"Expected one Copy-OcctRuntime block, found {count}.")

old_readme_line = '        "Before redistribution, review all license files in the licenses directory."\n'
new_readme_line = (
    '        "Native DLLs are selected from the actual dependency closure of OcctNative.dll;",\n'
    '        "unused SDK, sample, static-library and alternate-toolset DLLs are intentionally excluded.",\n'
    '        "",\n'
    '        "Before redistribution, review all license files in the licenses directory."\n'
)
if old_readme_line not in text:
    raise SystemExit("Package README insertion anchor was not found.")
text = text.replace(old_readme_line, new_readme_line, 1)
write(publish_path, text)

sections = {
    "docs/PUBLISHING_DEMO.md": r'''

## Native dependency closure

`publish.ps1` does not recursively copy every DLL under `3rdparty-vc14-64`. That directory may contain several compiler generations, static-library variants, debug binaries, SDK tools, and sample-only libraries such as GLFW. Copying all of them creates duplicate file names and can package an ABI-incompatible binary.

The script now locates the Visual C++ `dumpbin.exe`, starts from the built `OcctNative.dll`, reads each PE import table, and recursively copies only the OCCT and third-party DLLs that are actually required. Windows system DLLs are excluded, while the supported Visual C++ redistributable DLLs are copied separately. The selected dependency graph is written to `native-dependencies.txt` in the package root.

When multiple third-party files have the same name, candidates are ranked by runtime location, x64 architecture, and the VC 2022/vc14.4 toolset used by this project. Static and static-UCRT directories are never considered. An ambiguity is reported only when an actually imported DLL still has multiple equally ranked, different binaries.
''',
    "docs/PUBLISHING_DEMO.zh-CN.md": r'''

## 原生依赖闭包

`publish.ps1` 不再递归复制 `3rdparty-vc14-64` 下的全部 DLL。该目录通常同时包含多个编译器版本、静态库变体、调试版本、SDK 工具和 GLFW 等仅供示例程序使用的库。全部复制不仅会出现同名冲突，还可能把 ABI 不兼容的 DLL 放进发布包。

脚本现在会定位 Visual C++ 的 `dumpbin.exe`，以已编译的 `OcctNative.dll` 为入口读取 PE 导入表，并递归复制实际依赖的 OCCT 与第三方 DLL。Windows 系统 DLL 会被排除，受支持的 Visual C++ 运行库仍单独复制。最终依赖关系会写入发布包根目录的 `native-dependencies.txt`。

当第三方目录中存在多个同名文件时，脚本会根据运行目录、x64 架构以及本项目使用的 VC 2022/vc14.4 工具集进行排序；静态库和 static-UCRT 目录不会参与选择。只有某个实际依赖的 DLL 仍存在多个同优先级且内容不同的候选时，脚本才会报告明确的歧义。
'''
}

for path, section in sections.items():
    doc = read(path)
    heading = section.strip().splitlines()[0]
    if heading not in doc:
        doc = doc.rstrip() + section + "\n"
        write(path, doc)

workflow_path = ".github/workflows/publish-script-check.yml"
workflow = read(workflow_path)
check_step = r'''      - name: Verify dependency-closure packaging
        shell: pwsh
        run: |
          $text = Get-Content .\publish.ps1 -Raw
          $requiredTokens = @(
              'Resolve-Dumpbin',
              'Get-PeDependencies',
              'New-RuntimeCandidateIndex',
              'Resolve-RuntimeDependency',
              'native-dependencies.txt',
              'lib-static'
          )
          foreach ($token in $requiredTokens) {
              if (-not $text.Contains($token)) {
                  throw "Dependency-closure packaging token was not found: $token"
              }
          }
          if ($text -match 'Copying detected third-party DLLs') {
              throw 'publish.ps1 still contains the old blanket third-party DLL copy.'
          }
'''
if "Verify dependency-closure packaging" not in workflow:
    workflow = workflow.rstrip() + "\n" + check_step
    write(workflow_path, workflow)

print("Updated publish.ps1 to copy only the native dependency closure and refreshed publishing docs/checks.")
