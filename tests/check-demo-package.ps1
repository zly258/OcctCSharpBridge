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

Write-Host "[package] WinForms/WPF/Avalonia publishing, app-local native closure, VC runtime resolution and restricted LoadLibrary probe validated." -ForegroundColor Green
