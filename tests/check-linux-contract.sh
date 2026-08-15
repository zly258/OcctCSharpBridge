#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
CONTRACT="${ROOT_DIR}/bridge-contract.json"

fail() { printf '[linux-contract] ERROR: %s\n' "$*" >&2; exit 1; }
require_file() { [[ -f "$1" ]] || fail "Required file is missing: ${1#${ROOT_DIR}/}"; }
require_text() {
    local file="$1" text="$2" message="$3"
    grep -Fq -- "${text}" "${file}" || fail "${message}"
}
forbid_text() {
    local file="$1" text="$2" message="$3"
    if grep -Fq -- "${text}" "${file}"; then fail "${message}"; fi
}
tracked_path_exists() {
    local relative="$1"
    [[ -n "$(git -C "${ROOT_DIR}" ls-files -- "${relative}" "${relative}/**")" ]]
}
tracked_source_contains() {
    local root="$1" token="$2"
    local file
    while IFS= read -r file; do
        case "${file}" in
            *.h|*.hpp|*.hxx|*.cpp|*.cxx)
                if grep -Fq -- "${token}" "${ROOT_DIR}/${file}"; then return 0; fi
                ;;
        esac
    done < <(git -C "${ROOT_DIR}" ls-files -- "${root}/**")
    return 1
}

require_file "${CONTRACT}"
require_text "${CONTRACT}" '"schemaVersion": 3' "ABI5-only contract must use schemaVersion 3."
require_text "${CONTRACT}" '"platform": "cross-platform-x64"' "Source contract must remain cross-platform-x64."
require_text "${CONTRACT}" '"supportedPlatforms": ["windows-x64", "linux-x64"]' "Source contract must declare Windows/Linux x64 support."
require_text "${CONTRACT}" '"current": 5' "Current native ABI must remain ABI 5."
require_text "${CONTRACT}" '"minimumSupported": 5' "Minimum supported native ABI must be ABI 5."
require_text "${CONTRACT}" '"policy": "abi5-only"' "API policy must remain ABI5-only."
forbid_text "${CONTRACT}" '"legacy"' "Legacy ABI metadata must not be reintroduced."
forbid_text "${CONTRACT}" '"compatibility"' "Compatibility metadata must not be reintroduced."
forbid_text "${CONTRACT}" 'legacyAbi4Exports' "ABI4 accounting must not be reintroduced."
forbid_text "${CONTRACT}" 'compatibilityExtensions' "Compatibility export accounting must not be reintroduced."

CORE_PROJECT="${ROOT_DIR}/src/OcctNet/OcctNet.csproj"
AVALONIA_PROJECT="${ROOT_DIR}/src/OcctNet.Avalonia/OcctNet.Avalonia.csproj"
MANAGED_TESTS="${ROOT_DIR}/tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj"
SMOKE_PROJECT="${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj"
AVALONIA_SMOKE_PROJECT="${ROOT_DIR}/tests/OcctNet.AvaloniaSmoke/OcctNet.AvaloniaSmoke.csproj"
BUILD_SH="${ROOT_DIR}/build.sh"

for file in "${CORE_PROJECT}" "${AVALONIA_PROJECT}" "${MANAGED_TESTS}" "${SMOKE_PROJECT}" "${AVALONIA_SMOKE_PROJECT}" "${BUILD_SH}"; do
    require_file "${file}"
done

for project in "${CORE_PROJECT}" "${AVALONIA_PROJECT}" "${MANAGED_TESTS}" "${SMOKE_PROJECT}" "${AVALONIA_SMOKE_PROJECT}"; do
    require_text "${project}" '<TargetFramework>net10.0</TargetFramework>' "Cross-platform project must target net10.0: ${project#${ROOT_DIR}/}"
    forbid_text "${project}" 'net10.0-windows' "Windows-only TFM escaped into a Linux/core project: ${project#${ROOT_DIR}/}"
done

require_text "${MANAGED_TESTS}" '..\..\src\OcctNet\OcctNet.csproj' "Managed tests must reference OcctNet core."
forbid_text "${MANAGED_TESTS}" 'OcctNet.WinForms' "Managed core tests must not reference WinForms."
forbid_text "${MANAGED_TESTS}" 'OcctNet.Wpf' "Managed core tests must not reference WPF."
require_text "${AVALONIA_SMOKE_PROJECT}" '..\..\src\OcctNet.Avalonia\OcctNet.Avalonia.csproj' "Avalonia smoke must reference the formal Avalonia adapter."

for retired in \
    tests/contracts/abi4-exports.txt \
    tests/check-abi-compatibility.ps1 \
    tests/compatibility; do
    if tracked_path_exists "${retired}"; then fail "Retired ABI4 artifact must not be tracked: ${retired}"; fi
done

for token in 'OcctModelHandle' 'modelOf('; do
    if tracked_source_contains src/OcctNative "${token}"; then
        fail "Retired pre-ABI5 modeling implementation token remains in tracked Native source: ${token}"
    fi
done

require_text "${BUILD_SH}" 'validate_common()' "Linux build must keep common validation independent from native validation."
require_text "${BUILD_SH}" 'validate_native()' "Linux build must keep an explicit native validation layer."
require_text "${BUILD_SH}" 's/"platform": "cross-platform-x64"/"platform": "linux-x64"/' "Linux distribution must specialize the source contract to linux-x64."
require_text "${BUILD_SH}" '"platform": "linux-x64"' "Linux distribution manifest must identify linux-x64."
require_text "${BUILD_SH}" 'avalonia-smoke)' "Linux build must expose an explicit Avalonia viewer smoke target."
require_text "${BUILD_SH}" 'DISPLAY' "Avalonia viewer smoke must require an X11/XWayland display."

printf '[linux-contract] ABI5-only cross-platform tracked source/test/distribution boundaries validated.\n'
