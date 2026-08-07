$ErrorActionPreference = "Stop"

function Assert-Contains([string]$Path, [string]$Text) {
    $content = Get-Content $Path -Raw -Encoding UTF8
    if (-not $content.Contains($Text)) {
        throw "$Path does not contain required text: $Text"
    }
}

Assert-Contains "src/OcctNative/OcctNative.h" "occt_set_view_cube_language"
Assert-Contains "src/OcctNative/OcctObjectInteraction.cpp" "V3d_TypeOfOrientation_Zup_Front"
Assert-Contains "src/OcctNative/OcctObjectInteraction.cpp" "Microsoft YaHei UI"
Assert-Contains "src/OcctNet/OcctEngine.ObjectInteraction.cs" "SetViewCubeLanguage"
Assert-Contains "src/OcctNet/OcctObjectIntegrationTypes.cs" "OcctViewCubeLanguage"
Assert-Contains "src/CadWinForms/MainForm.cs" "ApplyViewCubeLanguage"
Assert-Contains "src/CadWpf/MainWindow.xaml.cs" "ApplyViewCubeLanguage"
Assert-Contains "src/CadCommon/CadLocalization.cs" "https://github.com/zly258/OcctCSharpBridge"
Assert-Contains "src/CadCommon/CadLocalization.cs" "zhangly1403@qq.com"
Assert-Contains "src/CadCommon/CadSession.Preconditions.cs" "GetCommandAvailability"
Assert-Contains "src/OcctNet.WinForms/OcctViewportControl.cs" "Preserve the gesture"
Assert-Contains "docs/API_COVERAGE.md" 'Native exports: `339`'
Assert-Contains "docs/API_COVERAGE.md" 'Managed P/Invoke declarations: `339`'
Assert-Contains "docs/API_COVERAGE.md" 'Public .NET types: `80`'
Assert-Contains "docs/API_COVERAGE.md" "occt_set_view_cube_language"
Assert-Contains "docs/API_COVERAGE.md" "OcctViewCubeLanguage"

$utf8BomFiles = @(
    "src/OcctNative/OcctNative.h",
    "src/OcctNative/CMakeLists.txt",
    "src/OcctNative/OcctObjectInteraction.cpp",
    "src/OcctNet/ObjectIntegrationNativeMethods.cs",
    "src/OcctNet/OcctObjectIntegrationTypes.cs",
    "src/OcctNet/OcctEngine.ObjectInteraction.cs",
    "src/CadCommon/CadLocalization.cs",
    "src/CadWinForms/MainForm.cs",
    "src/CadWpf/MainWindow.xaml.cs",
    "tests/check-demo-usability-localization.ps1"
)
foreach ($path in $utf8BomFiles) {
    $bytes = [System.IO.File]::ReadAllBytes($path)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
        throw "Modified file is not UTF-8 with BOM: $path"
    }
}
Write-Host "Demo usability and localization contract passed."
