from pathlib import Path

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


publish = read("publish.ps1")

old_candidate = r'''function Test-RuntimeCandidate {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $path = $File.FullName.ToLowerInvariant()
    if ($path -match "[\\/](?:lib-static(?:-ucrt)?|static(?:-ucrt)?)[\\/]") {
        return $false
    }
    if ($path -match "[\\/](?:x86|win32)[\\/]") {
        return $false
    }
    return $true
}

function Get-RuntimeCandidateScore {
'''
new_candidate = r'''function Test-RuntimeCandidate {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $path = $File.FullName.ToLowerInvariant()
    if ($path -match "[\\/](?:lib-static(?:-ucrt)?|static(?:-ucrt)?)[\\/]") {
        return $false
    }
    if ($path -match "[\\/](?:x86|win32|arm|arm64)[\\/]") {
        return $false
    }
    if ($path -match "[\\/](?:onecore|uwp|store|debug_nonredist)[\\/]") {
        return $false
    }
    return $true
}

function Get-RuntimeCandidateVersion {
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $pathMatch = [regex]::Match(
        $File.FullName,
        "(?i)[\\/]VC[\\/]Redist[\\/]MSVC[\\/](?<version>[0-9]+(?:\\.[0-9]+){1,3})[\\/]")
    if ($pathMatch.Success) {
        try {
            return [version]$pathMatch.Groups["version"].Value
        }
        catch {
        }
    }

    $fileVersion = $File.VersionInfo.FileVersion
    if (-not [string]::IsNullOrWhiteSpace($fileVersion)) {
        $versionMatch = [regex]::Match($fileVersion, "[0-9]+(?:\\.[0-9]+){1,3}")
        if ($versionMatch.Success) {
            try {
                return [version]$versionMatch.Value
            }
            catch {
            }
        }
    }

    return [version]"0.0"
}

function Get-RuntimeCandidateScore {
'''
publish = replace_once(publish, old_candidate, new_candidate, "runtime candidate filters and version")

old_score = r'''    if ([string]::Equals($File.DirectoryName, $OcctBinDir, [StringComparison]::OrdinalIgnoreCase)) {
        $score += 100000
    }
    if ($path -match "[\\/](?:bin|bin64)[\\/]") { $score += 5000 }
'''
new_score = r'''    if ([string]::Equals($File.DirectoryName, $OcctBinDir, [StringComparison]::OrdinalIgnoreCase)) {
        $score += 100000
    }
    if ($path -match "[\\/]vc[\\/]redist[\\/]msvc[\\/][^\\/]+[\\/]x64[\\/]microsoft\\.vc[0-9]+\\.crt[\\/]") {
        $score += 20000
    }
    if ($path -match "[\\/](?:bin|bin64)[\\/]") { $score += 5000 }
'''
publish = replace_once(publish, old_score, new_score, "desktop VC runtime score")

old_vc_add = r'''            $file = Get-Item -LiteralPath $match
            if ((Test-VisualCppRuntimeDependency $file.Name) -and $seen.Add($file.FullName)) {
                $result.Add($file)
            }
'''
new_vc_add = r'''            $file = Get-Item -LiteralPath $match
            if ((Test-RuntimeCandidate $file) -and
                (Test-VisualCppRuntimeDependency $file.Name) -and
                $seen.Add($file.FullName)) {
                $result.Add($file)
            }
'''
publish = replace_once(publish, old_vc_add, new_vc_add, "VC runtime candidate filtering")

old_ranking = r'''    $ranked = @($CandidateIndex[$key] | ForEach-Object {
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
'''
new_ranking = r'''    $ranked = @($CandidateIndex[$key] | Where-Object {
        Test-RuntimeCandidate $_
    } | ForEach-Object {
        [pscustomobject]@{
            File = $_
            Score = Get-RuntimeCandidateScore $_
            Version = Get-RuntimeCandidateVersion $_
        }
    } | Sort-Object -Property `
        @{ Expression = "Score"; Descending = $true }, `
        @{ Expression = "Version"; Descending = $true }, `
        @{ Expression = { $_.File.FullName }; Descending = $false })

    if ($ranked.Count -eq 0) {
        return $null
    }

    $topScore = $ranked[0].Score
    $topVersion = $ranked[0].Version
    $top = @($ranked | Where-Object {
        $_.Score -eq $topScore -and $_.Version -eq $topVersion
    })
'''
publish = replace_once(publish, old_ranking, new_ranking, "runtime ranking")

old_copy_vc = r'''function Copy-VisualCppRuntime {
    $names = @(
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

    foreach ($name in $names) {
        $candidate = Join-Path ([Environment]::SystemDirectory) $name
        if (Test-Path $candidate -PathType Leaf) {
            Copy-RuntimeDll (Get-Item $candidate) "Visual C++ runtime"
        }
    }
}
'''
new_copy_vc = r'''function Copy-VisualCppRuntime {
    $names = @(
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

    $candidateIndex = New-RuntimeCandidateIndex
    foreach ($name in $names) {
        $source = Resolve-RuntimeDependency $name $candidateIndex
        if ($null -ne $source) {
            Copy-RuntimeDll $source "Visual C++ runtime"
        }
    }
}
'''
publish = replace_once(publish, old_copy_vc, new_copy_vc, "consistent VC runtime copying")

write("publish.ps1", publish)

check = read("tests/check-demo-package.ps1")
old_tokens = r'''    'Get-VisualCppRuntimeFiles',
    'msvcp140_atomic_wait.dll',
'''
new_tokens = r'''    'Get-VisualCppRuntimeFiles',
    'Get-RuntimeCandidateVersion',
    'Test-RuntimeCandidate $file',
    '(?:onecore|uwp|store|debug_nonredist)',
    '$source = Resolve-RuntimeDependency $name $candidateIndex',
    'msvcp140_atomic_wait.dll',
'''
check = replace_once(check, old_tokens, new_tokens, "package validation tokens")

old_footer = r'''if ($text -match 'return Test-Path \(Join-Path \(\[Environment\]::SystemDirectory\) \$Name\)' -and
    -not $text.Contains('Test-VisualCppRuntimeDependency')) {
    throw "VC runtime dependencies may still be incorrectly classified as system DLLs."
}

Write-Host "[package] Self-contained .NET, OCCT, third-party, VC runtime, resources and closure validation rules passed." -ForegroundColor Green
'''
new_footer = r'''if ($text -match 'return Test-Path \(Join-Path \(\[Environment\]::SystemDirectory\) \$Name\)' -and
    -not $text.Contains('Test-VisualCppRuntimeDependency')) {
    throw "VC runtime dependencies may still be incorrectly classified as system DLLs."
}

if ($text -match 'Copy-RuntimeDll \(Get-Item \$candidate\) "Visual C\+\+ runtime"') {
    throw "Visual C++ runtime copying must use the same deterministic dependency resolver as OCCT dependencies."
}

Write-Host "[package] Self-contained .NET, OCCT, third-party, desktop VC runtime, resources and closure validation rules passed." -ForegroundColor Green
'''
check = replace_once(check, old_footer, new_footer, "package validation footer")
write("tests/check-demo-package.ps1", check)

(ROOT / ".github/fix_vc_runtime_resolution.py").unlink()
