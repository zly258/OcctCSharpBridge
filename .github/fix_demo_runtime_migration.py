from pathlib import Path

path = Path(__file__).resolve().parent / "apply_demo_runtime_hardening.py"
text = path.read_text(encoding="utf-8-sig")

old_resource = '''publish = replace_once(
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
'''
new_resource = '''publish = replace_once(
    publish,
    """    foreach ($name in $resourceNames) {
        $source = Join-Path $sourceRoot $name
        if (Test-Path $source -PathType Container) {
            Copy-Item $source (Join-Path $destinationRoot $name) -Recurse -Force
        }
    }
""",
    """    foreach ($name in $resourceNames) {
        $source = Join-Path $sourceRoot $name
        if (-not (Test-Path $source -PathType Container)) {
            throw "Required OCCT resource directory was not found: $name"
        }
        Copy-Item $source (Join-Path $destinationRoot $name) -Recurse -Force
    }
""",
    "required OCCT resources")
'''
if old_resource not in text:
    raise RuntimeError("Resource migration block was not found.")
text = text.replace(old_resource, new_resource, 1)

text = text.replace('''    $runtimeNames = @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll"
    )
''', '''    $names = @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll"
    )
''', 1)
text = text.replace('''    $runtimeNames = @(
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
''', '''    $names = @(
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
''', 1)

text = text.replace(
    'throw "Required OCCT resource directory was not found: $resourceName"',
    'throw "Required OCCT resource directory was not found: $name"')

path.write_text(text, encoding="utf-8-sig", newline="\n")
