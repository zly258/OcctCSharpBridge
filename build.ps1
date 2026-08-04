param(
    [Parameter(Position = 0)]
    [ValidateSet("native", "winform", "wpf", "all")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
$env:DOTNET_CLI_UI_LANGUAGE = "en-US"
$env:VSLANG = "1033"
if (Test-Path "$env:SystemRoot\System32\chcp.com") {
    & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null
}

$Target = $Target.ToLowerInvariant()
$OcctRoot = "D:\tools\occt-vc144-64"
$OcctIncludeDir = Join-Path $OcctRoot "inc"
$OcctLibDir = Join-Path $OcctRoot "win64\vc14\lib"
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$NativeSource = Join-Path $RepoRoot "src\OcctNative"
$NativeBuild = Join-Path $RepoRoot "build\native"
$NativeDll = Join-Path $NativeBuild "bin\$Configuration\OcctNative.dll"

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path $Path)) {
        throw "Required path was not found: $Path"
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

function Clean-ProjectOutput {
    param([Parameter(Mandatory = $true)][string]$ProjectDirectory)

    Remove-Item (Join-Path $ProjectDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $ProjectDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue
}

function Build-Native {
    Write-Host "[native] Configuring..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "-S", $NativeSource,
        "-B", $NativeBuild,
        "-G", "Visual Studio 17 2022",
        "-A", "x64",
        "-DOCCT_ROOT=$OcctRoot",
        "-DOCCT_INCLUDE_DIR=$OcctIncludeDir",
        "-DOCCT_LIB_DIR=$OcctLibDir",
        "-DOCCT_BIN_DIR=$OcctBinDir"
    ) "CMake configure failed."

    Write-Host "[native] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "--build", $NativeBuild,
        "--config", $Configuration,
        "--parallel"
    ) "Native build failed."

    Assert-Path $NativeDll
    Write-Host "Native: $NativeDll" -ForegroundColor Green
}

function Build-ManagedProject {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$ProjectFile,
        [Parameter(Mandatory = $true)][string]$TargetFramework,
        [Parameter(Mandatory = $true)][string]$ExecutableName
    )

    $projectDirectory = Split-Path -Parent $ProjectFile
    Clean-ProjectOutput $projectDirectory

    Write-Host "[$Name] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $ProjectFile,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "$ExecutableName build failed."

    $outputDirectory = Join-Path $projectDirectory "bin\x64\$Configuration\$TargetFramework"
    $executablePath = Join-Path $outputDirectory $ExecutableName
    Assert-Path $executablePath
    Assert-Path (Join-Path $outputDirectory "OcctNative.dll")
    Write-Host ("{0}: {1}" -f $Name, $executablePath) -ForegroundColor Green
}

foreach ($tool in @("cmake", "dotnet")) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        throw "$tool was not found in PATH."
    }
}

foreach ($path in @(
    $OcctIncludeDir,
    $OcctLibDir,
    $OcctBinDir,
    $OcctThirdPartyDir,
    (Join-Path $OcctIncludeDir "Standard.hxx"),
    (Join-Path $OcctLibDir "TKernel.lib"),
    (Join-Path $OcctBinDir "TKernel.dll")
)) {
    Assert-Path $path
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray

Build-Native
if ($Target -eq "native") {
    return
}

Clean-ProjectOutput (Join-Path $RepoRoot "src\OcctNet")
Clean-ProjectOutput (Join-Path $RepoRoot "src\CadCommon")

$projects = @{
    winform = @{
        Name = "CAD-Winform"
        Project = "src\CadWinForms\CadWinForms.csproj"
        Framework = "net8.0-windows"
        Executable = "CAD-Winform.exe"
    }
    wpf = @{
        Name = "CAD-WPF"
        Project = "src\CadWpf\CadWpf.csproj"
        Framework = "net8.0-windows"
        Executable = "CAD-WPF.exe"
    }
}

$selectedTargets = if ($Target -eq "all") {
    @("winform", "wpf")
}
else {
    @($Target)
}

foreach ($selectedTarget in $selectedTargets) {
    $project = $projects[$selectedTarget]
    Build-ManagedProject `
        -Name $project.Name `
        -ProjectFile (Join-Path $RepoRoot $project.Project) `
        -TargetFramework $project.Framework `
        -ExecutableName $project.Executable
}

Write-Host "Build completed." -ForegroundColor Green
