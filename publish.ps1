param(
    [Parameter(Position = 0)]
    [ValidateSet("all", "winform", "wpf", "avalonia")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$OutputDirectory = "",
    [switch]$SelfContained,
    [switch]$FrameworkDependent,
    [switch]$Zip,
    [switch]$KeepExisting
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($SelfContained.IsPresent -and $FrameworkDependent.IsPresent) {
    throw "Use either -SelfContained or -FrameworkDependent, not both."
}
$UseSelfContained = -not $FrameworkDependent.IsPresent
if ($SelfContained.IsPresent) { $UseSelfContained = $true }

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$ManifestPath = Join-Path $DistRoot "bridge-manifest.json"
$NativeDll = Join-Path $DistRoot "OcctNative.dll"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"

if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepoRoot "artifacts\publish"
}
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

$Projects = [ordered]@{
    winform = @{
        Name = "WinForms"
        Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"
        Executable = "CAD-Winform.exe"
    }
    wpf = @{
        Name = "WPF"
        Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"
        Executable = "CAD-WPF.exe"
    }
    avalonia = @{
        Name = "Avalonia"
        Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
        Executable = "CAD-Avalonia.exe"
    }
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if ($null -eq (Get-Command -Name $Name -ErrorAction SilentlyContinue)) {
        throw "$Name was not found in PATH."
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw $ErrorMessage }
}

function Get-SelectedKeys {
    if ($Target -eq "all") {
        # Establish one canonical self-contained runtime first. Later hosts add
        # only runtime-pack files that are not already present. This keeps one
        # portable directory without assuming that any UI framework depends on
        # another UI framework.
        return @("wpf", "winform", "avalonia")
    }
    return @($Target)
}

function Get-RuntimePackAssetNames {
    param(
        [Parameter(Mandatory = $true)][string]$SourceRoot,
        [Parameter(Mandatory = $true)][string]$Executable
    )

    $result = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    if (-not $UseSelfContained) { return ,$result }

    $applicationName = [System.IO.Path]::GetFileNameWithoutExtension($Executable)
    $depsPath = Join-Path $SourceRoot "$applicationName.deps.json"
    if (-not (Test-Path -LiteralPath $depsPath -PathType Leaf)) {
        throw "Self-contained publish did not produce the expected dependency manifest: $depsPath"
    }

    $deps = Get-Content -LiteralPath $depsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $runtimePackLibraries = @($deps.libraries.PSObject.Properties |
        Where-Object { [string]$_.Value.type -eq "runtimepack" } |
        ForEach-Object { $_.Name })

    if ($runtimePackLibraries.Count -eq 0) {
        throw "Self-contained dependency manifest contains no runtime-pack libraries: $depsPath"
    }

    # Self-contained .deps.json files can contain more than one target graph.
    # In .NET 10 the RID-specific runtime-pack entries are not guaranteed to be
    # in the first graph, so inspect every graph instead of selecting the first.
    $targetProperties = @($deps.targets.PSObject.Properties)
    if ($targetProperties.Count -eq 0) {
        throw "Dependency manifest contains no target graph: $depsPath"
    }

    foreach ($targetProperty in $targetProperties) {
        $targetGraph = $targetProperty.Value

        foreach ($libraryName in $runtimePackLibraries) {
            $libraryProperty = $targetGraph.PSObject.Properties[$libraryName]
            if ($null -eq $libraryProperty) { continue }
            $library = $libraryProperty.Value

            foreach ($sectionName in @("runtime", "runtimeTargets", "native", "resources")) {
                $sectionProperty = $library.PSObject.Properties[$sectionName]
                if ($null -eq $sectionProperty -or $null -eq $sectionProperty.Value) { continue }

                foreach ($asset in $sectionProperty.Value.PSObject.Properties) {
                    $fileName = [System.IO.Path]::GetFileName([string]$asset.Name)
                    if (-not [string]::IsNullOrWhiteSpace($fileName)) {
                        [void]$result.Add($fileName)
                    }
                }
            }
        }
    }

    if ($result.Count -eq 0) {
        throw "Self-contained dependency manifest contains runtime-pack libraries but no runtime-pack assets: $depsPath"
    }

    # Prevent PowerShell from enumerating the HashSet into the output pipeline.
    # This guarantees a stable collection object even when it is empty.
    return ,$result
}

function Copy-MergedFile {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination,
        [Parameter(Mandatory = $true)][bool]$IsRuntimePackFile,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.HashSet[string]]$RuntimeBaseline
    )

    $destinationDirectory = Split-Path -Parent $Destination
    if (-not (Test-Path -LiteralPath $destinationDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
    }

    $fileName = [System.IO.Path]::GetFileName($Source)
    if (Test-Path -LiteralPath $Destination -PathType Leaf) {
        $sourceHash = (Get-FileHash -LiteralPath $Source -Algorithm SHA256).Hash
        $destinationHash = (Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash
        if ($sourceHash -eq $destinationHash) {
            if ($IsRuntimePackFile) { [void]$RuntimeBaseline.Add($fileName) }
            return
        }

        if ($UseSelfContained -and $IsRuntimePackFile -and $RuntimeBaseline.Contains($fileName)) {
            Write-Host "[merge] Runtime pack: $fileName (reusing canonical copy)" -ForegroundColor DarkGray
            return
        }

        throw "Publish output collision contains different application/package files: $Destination"
    }

    Copy-Item -LiteralPath $Source -Destination $Destination -Force
    if ($IsRuntimePackFile) { [void]$RuntimeBaseline.Add($fileName) }
}

function Merge-PublishTree {
    param(
        [Parameter(Mandatory = $true)][string]$SourceRoot,
        [Parameter(Mandatory = $true)][string]$DestinationRoot,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.HashSet[string]]$SourceRuntimePackFiles,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.HashSet[string]]$RuntimeBaseline
    )

    $normalizedSourceRoot = [System.IO.Path]::GetFullPath($SourceRoot).TrimEnd('\', '/')
    Get-ChildItem -LiteralPath $normalizedSourceRoot -File -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring($normalizedSourceRoot.Length).TrimStart('\', '/')
        $isRuntimePackFile = $SourceRuntimePackFiles.Contains($_.Name)
        $copyArguments = @{
            Source = $_.FullName
            Destination = Join-Path $DestinationRoot $relativePath
            IsRuntimePackFile = $isRuntimePackFile
            RuntimeBaseline = $RuntimeBaseline
        }
        Copy-MergedFile @copyArguments
    }
}

function Get-PeImportedDllNames {
    param([Parameter(Mandatory = $true)][string]$Path)

    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    $reader = [System.IO.BinaryReader]::new($stream)
    try {
        $readUInt16At = {
            param([long]$Offset)
            $stream.Position = $Offset
            return $reader.ReadUInt16()
        }
        $readUInt32At = {
            param([long]$Offset)
            $stream.Position = $Offset
            return $reader.ReadUInt32()
        }
        $readAsciiZAt = {
            param([long]$Offset)
            $stream.Position = $Offset
            $bytes = [System.Collections.Generic.List[byte]]::new()
            while ($stream.Position -lt $stream.Length) {
                $value = $reader.ReadByte()
                if ($value -eq 0) { break }
                $bytes.Add($value)
            }
            return [System.Text.Encoding]::ASCII.GetString($bytes.ToArray())
        }

        if ((& $readUInt16At 0) -ne 0x5A4D) { return @() }
        $peOffset = [long](& $readUInt32At 0x3C)
        if ((& $readUInt32At $peOffset) -ne 0x00004550) { return @() }

        $coffOffset = $peOffset + 4
        $numberOfSections = [int](& $readUInt16At ($coffOffset + 2))
        $sizeOfOptionalHeader = [int](& $readUInt16At ($coffOffset + 16))
        $optionalOffset = $coffOffset + 20
        $magic = & $readUInt16At $optionalOffset
        if ($magic -eq 0x20B) {
            $dataDirectoryOffset = $optionalOffset + 112
        }
        elseif ($magic -eq 0x10B) {
            $dataDirectoryOffset = $optionalOffset + 96
        }
        else {
            return @()
        }

        $sections = @()
        $sectionOffset = $optionalOffset + $sizeOfOptionalHeader
        for ($index = 0; $index -lt $numberOfSections; ++$index) {
            $offset = $sectionOffset + ($index * 40)
            $sections += [pscustomobject]@{
                VirtualSize = [uint32](& $readUInt32At ($offset + 8))
                VirtualAddress = [uint32](& $readUInt32At ($offset + 12))
                RawSize = [uint32](& $readUInt32At ($offset + 16))
                RawPointer = [uint32](& $readUInt32At ($offset + 20))
            }
        }

        $rvaToOffset = {
            param([uint32]$Rva)
            foreach ($section in $sections) {
                $span = [Math]::Max([uint64]$section.VirtualSize, [uint64]$section.RawSize)
                $start = [uint64]$section.VirtualAddress
                $end = $start + $span
                if ([uint64]$Rva -ge $start -and [uint64]$Rva -lt $end) {
                    return [long]([uint64]$section.RawPointer + ([uint64]$Rva - $start))
                }
            }
            return [long]-1
        }

        $names = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
        $importRva = [uint32](& $readUInt32At ($dataDirectoryOffset + 8))
        if ($importRva -ne 0) {
            $descriptorOffset = & $rvaToOffset $importRva
            if ($descriptorOffset -ge 0) {
                for ($index = 0; $index -lt 4096; ++$index) {
                    $entryOffset = $descriptorOffset + ($index * 20)
                    if ($entryOffset + 20 -gt $stream.Length) { break }
                    $originalFirstThunk = [uint32](& $readUInt32At $entryOffset)
                    $timeDateStamp = [uint32](& $readUInt32At ($entryOffset + 4))
                    $forwarderChain = [uint32](& $readUInt32At ($entryOffset + 8))
                    $nameRva = [uint32](& $readUInt32At ($entryOffset + 12))
                    $firstThunk = [uint32](& $readUInt32At ($entryOffset + 16))
                    if (($originalFirstThunk -bor $timeDateStamp -bor $forwarderChain -bor $nameRva -bor $firstThunk) -eq 0) { break }
                    if ($nameRva -eq 0) { continue }
                    $nameOffset = & $rvaToOffset $nameRva
                    if ($nameOffset -lt 0) { continue }
                    $name = & $readAsciiZAt $nameOffset
                    if (-not [string]::IsNullOrWhiteSpace($name)) { [void]$names.Add($name) }
                }
            }
        }

        return @($names | Sort-Object)
    }
    finally {
        $reader.Dispose()
        $stream.Dispose()
    }
}

function Test-SystemRuntimeDependency {
    param([Parameter(Mandatory = $true)][string]$Name)

    if ($Name -match '(?i)^(api-ms-win-|ext-ms-win-)') { return $true }

    # The VC++ runtime is redistributable, not an operating-system contract.
    # It is commonly installed into System32 on a developer machine; treating
    # it as a Windows DLL would produce packages that fail with Win32 126 on a
    # clean target computer.
    if ($Name -match '(?i)^(msvcp|vcruntime|concrt|vccorlib)\d+.*\.dll$') { return $false }

    $system32 = Join-Path $env:SystemRoot "System32\$Name"
    if (Test-Path -LiteralPath $system32 -PathType Leaf) { return $true }
    return $false
}

function Get-VcRuntimeDirectories {
    $result = [System.Collections.Generic.List[string]]::new()

    if (-not [string]::IsNullOrWhiteSpace($env:VCToolsRedistDir)) {
        foreach ($directory in @(Get-Item -Path (Join-Path $env:VCToolsRedistDir "x64\Microsoft.VC14*.CRT") -ErrorAction SilentlyContinue)) {
            if ($directory.PSIsContainer) { $result.Add($directory.FullName) }
        }
    }

    $roots = @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($root in $roots) {
        $pattern = Join-Path $root "Microsoft Visual Studio\2022\*\VC\Redist\MSVC\*\x64\Microsoft.VC14*.CRT"
        foreach ($directory in @(Get-Item -Path $pattern -ErrorAction SilentlyContinue | Sort-Object FullName -Descending)) {
            if ($directory.PSIsContainer -and -not $result.Contains($directory.FullName)) { $result.Add($directory.FullName) }
        }
    }

    return @($result)
}

$VcRuntimeDirectories = @(Get-VcRuntimeDirectories)

function Resolve-NativeDependencySource {
    param([Parameter(Mandatory = $true)][string]$Name)

    $occtCandidate = Join-Path $OcctBinDir $Name
    if (Test-Path -LiteralPath $occtCandidate -PathType Leaf) { return $occtCandidate }

    foreach ($directory in $VcRuntimeDirectories) {
        $candidate = Join-Path $directory $Name
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    }

    if (Test-Path -LiteralPath $OcctThirdPartyDir -PathType Container) {
        # The PE import name is authoritative. Do not reject an exact imported
        # DLL merely because its file or directory contains "debug". Some OCCT
        # distributions intentionally link a Release toolkit to a debug-named
        # third-party runtime (for example tbb12_debug.dll).
        $matches = @(Get-ChildItem -LiteralPath $OcctThirdPartyDir -Filter $Name -File -Recurse -ErrorAction SilentlyContinue |
            Sort-Object \
                @{ Expression = { if ($_.FullName -match '(?i)[\\/](debug|dbg)[\\/]') { 1 } else { 0 } } }, \
                @{ Expression = { if ($_.DirectoryName -match '(?i)[\\/]bin([\\/]|$)') { 0 } else { 1 } } }, \
                FullName)
        if ($matches.Count -gt 0) { return $matches[0].FullName }
    }

    return $null
}

function Copy-PortableNativeRuntime {
    param([Parameter(Mandatory = $true)][string]$Destination)

    Copy-Item -LiteralPath $NativeDll -Destination (Join-Path $Destination "OcctNative.dll") -Force
    Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $Destination "bridge-contract.json") -Force
    Copy-Item -LiteralPath $ManifestPath -Destination (Join-Path $Destination "bridge-manifest.json") -Force

    $queue = [System.Collections.Generic.Queue[string]]::new()
    $queued = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $processed = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $copied = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $missing = [ordered]@{}

    $rootNative = Join-Path $Destination "OcctNative.dll"
    $queue.Enqueue($rootNative)
    [void]$queued.Add("OcctNative.dll")
    [void]$copied.Add("OcctNative.dll")

    while ($queue.Count -gt 0) {
        $currentPath = $queue.Dequeue()
        $currentName = [System.IO.Path]::GetFileName($currentPath)
        if (-not $processed.Add($currentName)) { continue }

        foreach ($dependency in @(Get-PeImportedDllNames -Path $currentPath)) {
            if (Test-SystemRuntimeDependency -Name $dependency) { continue }

            $destinationPath = Join-Path $Destination $dependency
            if (-not (Test-Path -LiteralPath $destinationPath -PathType Leaf)) {
                $source = Resolve-NativeDependencySource -Name $dependency
                if ([string]::IsNullOrWhiteSpace($source)) {
                    if (-not $missing.Contains($dependency)) {
                        $missing[$dependency] = $currentName
                    }
                    continue
                }
                Copy-Item -LiteralPath $source -Destination $destinationPath -Force
                Write-Host "[runtime] $currentName -> $dependency" -ForegroundColor DarkGray
            }

            [void]$copied.Add($dependency)
            if (-not $processed.Contains($dependency) -and $queued.Add($dependency)) {
                $queue.Enqueue($destinationPath)
            }
        }
    }

    if ($missing.Count -gt 0) {
        $details = @($missing.GetEnumerator() |
            Sort-Object { [string]$_.Key } |
            ForEach-Object { "$($_.Value) -> $($_.Key)" })
        throw "Portable native runtime is incomplete. Missing imported DLLs:`n - $($details -join "`n - ")"
    }

    $debugNamedThirdParty = @($copied | Where-Object { $_ -match '(?i)^tbb.*_debug\.dll$' })
    if ($debugNamedThirdParty.Count -gt 0) {
        Write-Warning "Current OCCT binaries explicitly import debug-named TBB runtime(s): $($debugNamedThirdParty -join ', '). Packaging the exact imported DLLs."
    }

    # Reject unrelated GUI/test dependencies and Microsoft debug CRTs. A
    # debug-named third-party DLL is allowed only when it is an actual PE import;
    # if it in turn needs a non-redistributable Microsoft debug CRT, this check
    # still prevents producing a misleading portable package.
    $forbidden = @($copied | Where-Object {
        $_ -match '^(?i:Qt\d|vtk|tcl\d|tk86\.dll$)' -or
        $_ -match '(?i)^(MSVCP|VCRUNTIME|CONCRT|VCCORLIB)\d+D.*\.dll$' -or
        $_ -match '(?i)^ucrtbased\.dll$'
    })
    if ($forbidden.Count -gt 0) {
        throw "Unexpected GUI/test or Microsoft debug-runtime dependencies entered the portable runtime closure: $($forbidden -join ', ')"
    }

    $runtimeManifest = @(
        "OcctCSharpBridge Demo native runtime closure",
        "Bridge: $($contract.bridgeVersion)",
        "OCCT: $($contract.occtVersion)",
        "Architecture: win-x64",
        "",
        "Packaged native DLLs:"
    ) + @($copied | Sort-Object | ForEach-Object { "- $_" })
    [System.IO.File]::WriteAllLines((Join-Path $Destination "native-runtime-manifest.txt"), $runtimeManifest, [System.Text.UTF8Encoding]::new($true))
}

Assert-Command "dotnet"
Assert-Path $BuildScript

# Validate the Binary SDK once before touching publish output.
& $BuildScript validate $Configuration

Assert-Path $OcctBinDir
$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$packageRoot = Join-Path $OutputDirectory ("OcctCSharpBridge-Demo-{0}-win-x64" -f $Target)
$stagingRoot = Join-Path $OutputDirectory (".OcctCSharpBridge-Demo-{0}-staging-{1}" -f $Target, $PID)

if ((Test-Path -LiteralPath $packageRoot) -and -not $KeepExisting.IsPresent) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

$runtimeBaseline = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

try {
    # Resolve the native closure first. Native packaging problems should fail
    # fast instead of recompiling all managed demo hosts before surfacing them.
    Write-Host "[publish] Resolving portable OCCT native runtime..." -ForegroundColor Cyan
    Copy-PortableNativeRuntime -Destination $packageRoot

    foreach ($key in Get-SelectedKeys) {
        $definition = $Projects[$key]
        $projectPath = Join-Path $RepoRoot $definition.Project
        $stagingDestination = Join-Path $stagingRoot $key
        Assert-Path $projectPath
        New-Item -ItemType Directory -Path $stagingDestination -Force | Out-Null

        Write-Host "[publish] $($definition.Name) from Bridge $($contract.bridgeVersion), ABI $($contract.nativeAbiVersion)..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @(
            "publish", $projectPath,
            "-c", $Configuration,
            "-r", "win-x64",
            "-p:Platform=x64",
            "-p:Version=$($contract.bridgeVersion)",
            "-p:DebugType=None",
            "-p:DebugSymbols=false",
            "--self-contained", $UseSelfContained.ToString().ToLowerInvariant(),
            "--nologo",
            "-o", $stagingDestination
        ) "$($definition.Name) publish failed."

        Assert-Path (Join-Path $stagingDestination $definition.Executable)
        $runtimePackFiles = Get-RuntimePackAssetNames -SourceRoot $stagingDestination -Executable $definition.Executable
        if ($UseSelfContained) {
            Write-Host "[merge] $($definition.Name) runtime-pack assets: $($runtimePackFiles.Count)" -ForegroundColor DarkGray
        }
        $mergeArguments = @{
            SourceRoot = $stagingDestination
            DestinationRoot = $packageRoot
            SourceRuntimePackFiles = $runtimePackFiles
            RuntimeBaseline = $runtimeBaseline
        }
        Merge-PublishTree @mergeArguments
    }

    foreach ($key in Get-SelectedKeys) {
        Assert-Path (Join-Path $packageRoot $Projects[$key].Executable)
    }
}
finally {
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
}

if ($Zip.IsPresent) {
    $zipPath = "$packageRoot.zip"
    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "Package: $zipPath" -ForegroundColor Green
}
else {
    Write-Host "Package: $packageRoot" -ForegroundColor Green
}
