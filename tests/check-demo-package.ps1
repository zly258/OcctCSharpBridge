param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$publishPath = Join-Path $RepositoryRoot "publish.ps1"
if (-not (Test-Path $publishPath -PathType Leaf)) { throw "publish.ps1 was not found." }
$text = [System.IO.File]::ReadAllText($publishPath)

$requiredTokens = @(
    '[ValidateSet("all", "winform", "wpf", "avalonia")]',
    'Project = "src\CadAvalonia\CadAvalonia.csproj"',
    'Executable = "CAD-Avalonia.exe"',
    '$UseSelfContained = -not $FrameworkDependent.IsPresent',
    '--self-contained", $UseSelfContained.ToString().ToLowerInvariant()',
    'function Test-PackagedNativeClosure',
    'function Copy-NativeRuntimeToApplications',
    'function Test-PackagedNativeLoad',
    'LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR',
    'LOAD_LIBRARY_SEARCH_USER_DIRS',
    'LOAD_LIBRARY_SEARCH_SYSTEM32',
    'appLocalNativeRuntime = $true',
    'nativeRuntimeDeployment = "app-local-copy"',
    'package-contract.json',
    'native-dependencies.txt',
    'Get-VisualCppRuntimeFiles',
    'Get-RuntimeCandidateVersion',
    'vcomp140.dll',
    'VC\Redist\MSVC\**\x64\**\*.dll',
    'throw "Required OCCT resource directory was not found: $name"',
    'contact = "zhangly1403@gmail.com"'
)

foreach ($token in $requiredTokens) {
    if (-not $text.Contains($token)) {
        throw "Required package token is missing: $token"
    }
}

foreach ($key in @("winform", "wpf", "avalonia")) {
    if (-not $text.Contains("$key = @{")) {
        throw "Publish target is missing: $key"
    }
}

if ($text -match 'return Test-Path \(Join-Path \(\[Environment\]::SystemDirectory\) \$Name\)' -and
    -not $text.Contains('Test-VisualCppRuntimeDependency')) {
    throw "VC runtime dependencies may still be incorrectly classified as system DLLs."
}

$runtimeIndex = $text.IndexOf('Test-PackagedNativeClosure')
$appLocalIndex = $text.LastIndexOf('Copy-NativeRuntimeToApplications')
$probeIndex = $text.LastIndexOf('Test-PackagedNativeLoad')
if ($runtimeIndex -lt 0 -or $appLocalIndex -lt 0 -or $probeIndex -lt 0 -or $probeIndex -lt $appLocalIndex) {
    throw "Package validation must resolve the closure, deploy app-local native DLLs, then run the native load probe."
}

# NuGet SDK packaging belongs only to main. Demo projects are application/reference
# sources and must not accidentally inherit main's packable project metadata.
foreach ($relativePath in @(
    "src\OcctNet\OcctNet.csproj",
    "src\OcctNet.WinForms\OcctNet.WinForms.csproj",
    "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
)) {
    $projectText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $relativePath))
    if (-not $projectText.Contains('<IsPackable>false</IsPackable>')) {
        throw "Demo reusable project must remain non-packable; NuGet packaging is main-only: $relativePath"
    }
    foreach ($forbidden in @('<PackageReadmeFile>', '<PackageLicenseFile>', '<RepositoryUrl>')) {
        if ($projectText.Contains($forbidden)) {
            throw "Main-only NuGet metadata leaked into demo project ${relativePath}: $forbidden"
        }
    }
}

# README screenshots are branch-specific assets. Pin the rendered URLs to the demo
# branch and verify every referenced PNG has a matching repository file so a rename,
# language mix-up, or relative-path regression fails validation immediately.
$previewPrefix = "https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/"
$previewContracts = @(
    @{
        Readme = "README.md"
        Expected = @(
            "winform-demo-en.png",
            "wpf-demo-en.png",
            "avalonia-demo-en.png"
        )
        ForbiddenSuffix = "-zh.png"
    },
    @{
        Readme = "README.zh-CN.md"
        Expected = @(
            "winform-demo-zh.png",
            "wpf-demo-zh.png",
            "avalonia-demo-zh.png"
        )
        ForbiddenSuffix = "-en.png"
    }
)

foreach ($contract in $previewContracts) {
    $readmePath = Join-Path $RepositoryRoot $contract.Readme
    if (-not (Test-Path $readmePath -PathType Leaf)) {
        throw "Demo README was not found: $($contract.Readme)"
    }

    $readmeText = [System.IO.File]::ReadAllText($readmePath)
    if ($readmeText -match '(?i)\.webp(?:["''?#]|$)') {
        throw "Legacy WebP preview reference remains in $($contract.Readme)."
    }

    $previewUrls = @(
        [regex]::Matches($readmeText, '<img\s+[^>]*src="([^"]+)"', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase) |
            ForEach-Object { $_.Groups[1].Value } |
            Where-Object { $_ -like '*assets/previews/*' }
    )

    if ($previewUrls.Count -ne 3) {
        throw "$($contract.Readme) must reference exactly three demo preview images; found $($previewUrls.Count)."
    }

    foreach ($fileName in $contract.Expected) {
        $expectedUrl = $previewPrefix + $fileName
        if ($previewUrls -notcontains $expectedUrl) {
            throw "$($contract.Readme) is missing canonical preview URL: $expectedUrl"
        }

        $assetPath = Join-Path $RepositoryRoot ("assets\previews\" + $fileName)
        if (-not (Test-Path $assetPath -PathType Leaf)) {
            throw "README preview asset does not exist: assets/previews/$fileName"
        }
    }

    foreach ($url in $previewUrls) {
        if (-not $url.StartsWith($previewPrefix, [System.StringComparison]::Ordinal)) {
            throw "$($contract.Readme) preview URL is not pinned to the demo branch: $url"
        }
        if (-not $url.EndsWith('.png', [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "$($contract.Readme) preview must use PNG: $url"
        }
        if ($url.EndsWith($contract.ForbiddenSuffix, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "$($contract.Readme) references the wrong language preview: $url"
        }
    }
}

Write-Host "[package] Demo publishing, app-local native closure, VC runtime resolution, restricted LoadLibrary probe, main-only NuGet boundary, and README preview paths validated." -ForegroundColor Green
