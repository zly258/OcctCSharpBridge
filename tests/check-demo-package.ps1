param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) { throw "bridge-contract.json was not found." }
$contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$targetFramework = [string]$contract.dotnet.targetFramework
if ([string]::IsNullOrWhiteSpace($targetFramework)) { throw "Bridge contract target framework is missing." }

$runPath = Join-Path $RepositoryRoot "run.ps1"
if (-not (Test-Path $runPath -PathType Leaf)) { throw "run.ps1 was not found." }
$runText = [System.IO.File]::ReadAllText($runPath)
foreach ($token in @(
    '$Contract = Get-Content $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json',
    '$TargetFramework = [string]$Contract.dotnet.targetFramework',
    'src\OcctDemo.WinForms\bin\x64\$Configuration\$TargetFramework\CAD-Winform.exe',
    'src\OcctDemo.Wpf\bin\x64\$Configuration\$TargetFramework\CAD-WPF.exe',
    'src\OcctDemo.Avalonia\bin\x64\$Configuration\$TargetFramework\CAD-Avalonia.exe'
)) {
    if (-not $runText.Contains($token)) { throw "Demo run target-framework contract is missing: $token" }
}
if ($runText.Contains('net8.0-windows')) { throw "Legacy net8.0-windows output path remains in run.ps1." }
$publishPath = Join-Path $RepositoryRoot "publish.ps1"
if (-not (Test-Path $publishPath -PathType Leaf)) { throw "publish.ps1 was not found." }
$text = [System.IO.File]::ReadAllText($publishPath)

$requiredTokens = @(
    '[ValidateSet("all", "winform", "wpf", "avalonia")]',
    'Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"',
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
    "src\OcctNet.Wpf\OcctNet.Wpf.csproj",
    "src\OcctNet.Avalonia\OcctNet.Avalonia.csproj",
    "src\OcctDemo.Common\OcctDemo.Common.csproj",
    "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj",
    "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj",
    "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
)) {
    $projectPath = Join-Path $RepositoryRoot $relativePath
    if (-not (Test-Path $projectPath -PathType Leaf)) { throw "Demo managed project was not found: $relativePath" }
    $projectText = [System.IO.File]::ReadAllText($projectPath)
    if (-not $projectText.Contains("<TargetFramework>$targetFramework</TargetFramework>")) {
        throw "Demo managed project target framework does not match bridge-contract.json: $relativePath"
    }
    if (-not $projectText.Contains('<IsPackable>false</IsPackable>')) {
        throw "Demo managed project must remain non-packable; NuGet packaging is main-only: $relativePath"
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

# Keep the user-facing Win32 126 troubleshooting instructions aligned with the shared
# startup diagnostics so a future UI refactor cannot leave stale README guidance.
$troubleshootingContracts = [ordered]@{
    "README.md" = @(
        'OcctNative.dll [missing]',
        'TKernel.dll [missing]',
        'native-dependencies.txt',
        '%LOCALAPPDATA%\OcctCSharpBridge\Logs'
    )
    "README.zh-CN.md" = @(
        'OcctNative.dll [缺失]',
        'TKernel.dll [缺失]',
        'native-dependencies.txt',
        '%LOCALAPPDATA%\OcctCSharpBridge\Logs'
    )
}

foreach ($readmeContract in $troubleshootingContracts.GetEnumerator()) {
    $readmeText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $readmeContract.Key))
    foreach ($token in $readmeContract.Value) {
        if (-not $readmeText.Contains($token)) {
            throw "Native troubleshooting documentation is missing from $($readmeContract.Key): $token"
        }
    }
}

# All three demo hosts must surface actionable app-local native diagnostics instead of
# showing only the raw DllNotFoundException/Win32 126 message.
$crashReporterPath = Join-Path $RepositoryRoot "src\OcctDemo.Common\CrashReporter.cs"
if (-not (Test-Path $crashReporterPath -PathType Leaf)) {
    throw "Shared CrashReporter was not found."
}
$crashReporter = [System.IO.File]::ReadAllText($crashReporterPath)
foreach ($token in @(
    'BuildUserMessage',
    'OcctRuntime.GetDiagnosticInfo()',
    'OcctRuntime.GetDiagnosticReport()',
    'ApplicationNativeBridgePath',
    'ApplicationOcctKernelPath',
    'DllNotFoundException',
    'BadImageFormatException',
    'EntryPointNotFoundException',
    'Win32 126',
    'demo/publish.ps1'
)) {
    if (-not $crashReporter.Contains($token)) {
        throw "Native startup diagnostic contract is missing from CrashReporter: $token"
    }
}

$hostDiagnostics = [ordered]@{
    "src\OcctDemo.WinForms\Program.cs" = @(
        'CrashReporter.Write(ApplicationName, exception, source)',
        'CrashReporter.BuildUserMessage(exception, logPath)'
    )
    "src\OcctDemo.Wpf\App.xaml.cs" = @(
        'CrashReporter.Write(ApplicationName, e.Exception, "DispatcherUnhandledException")',
        'CrashReporter.BuildUserMessage(e.Exception, logPath)'
    )
    "src\OcctDemo.Avalonia\Program.cs" = @(
        'CrashReporter.Write(ApplicationName, exception, message)',
        'CrashReporter.BuildUserMessage(exception, logPath)',
        'args.SetObserved();'
    )
}

foreach ($hostContract in $hostDiagnostics.GetEnumerator()) {
    $hostPath = Join-Path $RepositoryRoot $hostContract.Key
    if (-not (Test-Path $hostPath -PathType Leaf)) {
        throw "Demo host diagnostic file was not found: $($hostContract.Key)"
    }
    $hostText = [System.IO.File]::ReadAllText($hostPath)
    foreach ($token in $hostContract.Value) {
        if (-not $hostText.Contains($token)) {
            throw "Demo host does not use the shared native startup diagnostics: $($hostContract.Key) -> $token"
        }
    }
}

Write-Host "[package] Demo publishing, contract-driven TFM/run paths, eight non-packable managed projects, app-local native closure, VC runtime resolution, restricted LoadLibrary probe, shared Win32 126 diagnostics, protected troubleshooting docs, and README preview paths validated." -ForegroundColor Green
