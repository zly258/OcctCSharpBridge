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
require_text "${CONTRACT}" '"targetFramework": "net8.0"' "Binary SDK minimum target framework must be net8.0."
require_text "${CONTRACT}" '"defaultRuntimeFramework": "net10.0"' "Default cross-platform execution target must be net10.0."
require_text "${CONTRACT}" '"sdkRollForward": "latestFeature"' "SDK contract must allow stable .NET 10 feature-band/patch roll-forward."
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
PUBLISH_SH="${ROOT_DIR}/publish.sh"

for file in "${CORE_PROJECT}" "${AVALONIA_PROJECT}" "${MANAGED_TESTS}" "${SMOKE_PROJECT}" "${AVALONIA_SMOKE_PROJECT}" "${BUILD_SH}" "${PUBLISH_SH}"; do
    require_file "${file}"
done

for project in "${CORE_PROJECT}" "${AVALONIA_PROJECT}"; do
    require_text "${project}" '<TargetFramework>net8.0</TargetFramework>' "Published cross-platform managed SDK must keep the net8.0 compatibility baseline: ${project#${ROOT_DIR}/}"
    forbid_text "${project}" 'net8.0-windows' "Windows-only TFM escaped into a Linux/core project: ${project#${ROOT_DIR}/}"
done

for project in "${MANAGED_TESTS}" "${SMOKE_PROJECT}" "${AVALONIA_SMOKE_PROJECT}"; do
    require_text "${project}" '<TargetFramework>net10.0</TargetFramework>' "Default Linux regression/smoke execution must target net10.0: ${project#${ROOT_DIR}/}"
    forbid_text "${project}" 'net10.0-windows' "Windows-only TFM escaped into a Linux/core test project: ${project#${ROOT_DIR}/}"
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

for token in 'OcctHandle' 'OcctModelHandle' 'modelOf('; do
    if tracked_source_contains src/OcctNative "${token}"; then
        fail "Retired pre-ABI5 native implementation token remains in tracked Native source: ${token}"
    fi
done

require_text "${BUILD_SH}" 'validate_common()' "Linux build must keep common validation independent from native validation."
require_text "${BUILD_SH}" 'validate_native()' "Linux build must keep an explicit native validation layer."
require_text "${BUILD_SH}" 'sdk_is_compatible' "Linux build must validate the resolved SDK against the rolling .NET 10 baseline."
require_text "${BUILD_SH}" 'SDK_ROLL_FORWARD' "Linux build must consume the SDK roll-forward policy from the contract."
forbid_text "${BUILD_SH}" '== "${SDK_VERSION}"' "Linux build must not require an exact SDK patch/feature-band match."
require_text "${BUILD_SH}" 's/"platform": "cross-platform-x64"/"platform": "linux-x64"/' "Linux distribution must specialize the source contract to linux-x64."
require_text "${BUILD_SH}" '"schemaVersion": 2' "Linux Binary SDK manifest must use schemaVersion 2."
require_text "${BUILD_SH}" '"nativeAbi"' "Linux Binary SDK manifest must use nested nativeAbi metadata."
require_text "${BUILD_SH}" '"current"' "Linux Binary SDK manifest must record the current ABI."
require_text "${BUILD_SH}" '"minimumSupported"' "Linux Binary SDK manifest must record the minimum ABI."
require_text "${BUILD_SH}" '"resolvedSdkVersion"' "Linux Binary SDK manifest must record the actual resolved build SDK."
forbid_text "${BUILD_SH}" 'nativeAbiVersion' "Linux Binary SDK generator must not emit retired flat nativeAbiVersion metadata."
require_text "${BUILD_SH}" '"platform": "linux-x64"' "Linux distribution manifest must identify linux-x64."
require_text "${BUILD_SH}" 'avalonia-smoke)' "Linux build must expose an explicit Avalonia viewer smoke target."
require_text "${BUILD_SH}" 'DISPLAY' "Avalonia viewer smoke must require an X11/XWayland display."

require_text "${PUBLISH_SH}" 'main) publish_mode="Formal" ;;' "Linux formal publishing must keep main as the Formal publication gate."
require_text "${PUBLISH_SH}" 'main-dev) publish_mode="Development" ;;' "Linux development publishing must keep main-dev as the Development publication gate."
require_text "${PUBLISH_SH}" 'assert_clean_worktree "before publishing"' "Linux publishing must require a clean worktree."
require_text "${PUBLISH_SH}" 'assert_remote_branch_ancestor "${branch}"' "Linux publishing must validate ancestry against the matching remote branch."
require_text "${PUBLISH_SH}" '"${BUILD_SCRIPT}" all Release' "Linux publishing must run the complete headless Release validation."
require_text "${PUBLISH_SH}" '"${BUILD_SCRIPT}" dist Release' "Linux publishing must produce a fresh Release Binary SDK."
require_text "${PUBLISH_SH}" '"$(json_number "${MANIFEST}" schemaVersion)" == "2"' "Linux publish validation must require manifest schemaVersion 2."
require_text "${PUBLISH_SH}" '"$(json_number "${MANIFEST}" current)" == "$(json_number "${CONTRACT}" current)"' "Linux publish validation must verify nested current ABI metadata."
require_text "${PUBLISH_SH}" '"$(json_number "${MANIFEST}" minimumSupported)" == "$(json_number "${CONTRACT}" minimumSupported)"' "Linux publish validation must verify nested minimum ABI metadata."
require_text "${PUBLISH_SH}" '! grep -Fq '\''"nativeAbiVersion"'\''' "Linux publish validation must reject retired flat ABI metadata."
require_text "${PUBLISH_SH}" 'sourceCommit' "Linux publish validation must bind the Binary SDK to the source commit."
require_text "${PUBLISH_SH}" 'sha256sum' "Linux publish validation must verify Binary SDK hashes."
require_text "${PUBLISH_SH}" 'No Git commit or push was performed.' "Linux publish script must clearly remain validation-only."
forbid_text "${PUBLISH_SH}" 'git -C "${ROOT_DIR}" add' "Linux publish script must never git-add Binary SDK output."
forbid_text "${PUBLISH_SH}" 'git -C "${ROOT_DIR}" commit' "Linux publish script must never create commits."
forbid_text "${PUBLISH_SH}" 'git -C "${ROOT_DIR}" push' "Linux publish script must never push branches."

# Keep generated payloads and accidental large files out of the source repository.
while IFS= read -r path; do
    case "${path}" in
        dist/*|artifacts/*|build/*|publish/*|.cache/*|*/bin/*|*/obj/*|*/TestResults/*|*/coverage/*)
            fail "Generated/cache path must not be tracked by the Bridge source branch: ${path}"
            ;;
        *.dll|*.exe|*.so|*.dylib|*.pdb|*.ilk|*.exp|*.idb|*.tlog|*.zip|*.tar|*.tar.gz|*.tgz|*.7z|*.rar|*.nupkg|*.snupkg)
            fail "Generated binary/archive must not be tracked by the Bridge source branch: ${path}"
            ;;
    esac

    if [[ -f "${ROOT_DIR}/${path}" ]]; then
        size="$(stat -c %s "${ROOT_DIR}/${path}")"
        if (( size > 2097152 )); then
            fail "Tracked file exceeds the 2 MiB repository hygiene limit: ${path} (${size} bytes). Use Release/Artifacts for large generated payloads or explicitly redesign the source policy before tracking them."
        fi
    fi
done < <(git -C "${ROOT_DIR}" ls-files)

printf '[linux-contract] ABI5-only cross-platform source/test/distribution boundaries validated: net8.0 Binary SDK compatibility baseline, net10.0 default execution, .NET 10 SDK.\n'
