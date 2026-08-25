#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET="${1:-all}"
CONFIGURATION="${2:-Release}"
DIST_ROOT="${ROOT_DIR}/dist/linux-x64"
CONTRACT="${DIST_ROOT}/bridge-contract.json"
MANIFEST="${DIST_ROOT}/bridge-manifest.json"
GLOBAL_JSON="${ROOT_DIR}/global.json"
COMMON_PROJECT="${ROOT_DIR}/src/OcctDemo.Common/OcctDemo.Common.csproj"
AVALONIA_PROJECT="${ROOT_DIR}/src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
TFM="net10.0"

log() { printf '[demo-linux] %s\n' "$*"; }
fail() { printf '[demo-linux] ERROR: %s\n' "$*" >&2; exit 1; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }
json_number() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*([0-9]+).*/\\1/p" "$1" | head -n 1; }

sdk_is_compatible() {
    local baseline="$1" detected="$2" policy="$3"
    [[ "${policy}" == "latestFeature" ]] || return 1
    [[ "${baseline}" != *-* && "${detected}" != *-* ]] || return 1
    local min_major min_minor min_patch detected_major detected_minor detected_patch
    IFS=. read -r min_major min_minor min_patch <<<"${baseline}"
    IFS=. read -r detected_major detected_minor detected_patch <<<"${detected}"
    [[ "${min_major}" =~ ^[0-9]+$ && "${min_minor}" =~ ^[0-9]+$ && "${min_patch}" =~ ^[0-9]+$ ]] || return 1
    [[ "${detected_major}" =~ ^[0-9]+$ && "${detected_minor}" =~ ^[0-9]+$ && "${detected_patch}" =~ ^[0-9]+$ ]] || return 1
    [[ "${detected_major}" == "${min_major}" && "${detected_minor}" == "${min_minor}" ]] || return 1
    ((10#${detected_patch} >= 10#${min_patch}))
}

validate_sdk() {
    [[ "$(uname -s)" == "Linux" ]] || fail "build.sh supports Linux only; use build.ps1 on Windows."
    case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
    case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) fail "Unknown configuration: ${CONFIGURATION}" ;; esac
    bash "${ROOT_DIR}/tests/check-no-reflection-dispatch.sh" "${ROOT_DIR}"
    require_command dotnet
    require_command git
    require_command sha256sum

    [[ -f "${GLOBAL_JSON}" ]] || fail "global.json was not found."
    local required_sdk roll_forward detected_sdk
    required_sdk="$(json_string "${GLOBAL_JSON}" version)"
    roll_forward="$(json_string "${GLOBAL_JSON}" rollForward)"
    [[ -n "${required_sdk}" ]] || fail "Unable to read the SDK baseline from global.json."
    detected_sdk="$(cd "${ROOT_DIR}" && dotnet --version)"
    sdk_is_compatible "${required_sdk}" "${detected_sdk}" "${roll_forward}" || fail "A stable .NET 10 SDK compatible with baseline ${required_sdk} / ${roll_forward} is required; detected ${detected_sdk}."

    for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json bridge-manifest.json; do
        [[ -f "${DIST_ROOT}/${name}" ]] || fail "Linux Binary SDK is incomplete: ${name} is missing. Run ./sync.sh first."
    done

    [[ "$(json_number "${CONTRACT}" schemaVersion)" == "3" ]] || fail "Bridge contract schema 3 is required."
    [[ "$(json_number "${CONTRACT}" current)" == "5" && "$(json_number "${CONTRACT}" minimumSupported)" == "5" ]] || fail "Bridge contract must be ABI5-only."
    [[ "$(json_string "${CONTRACT}" policy)" == "abi5-only" ]] || fail "Bridge contract must use api.policy=abi5-only."
    [[ "$(json_string "${CONTRACT}" platform)" == "linux-x64" ]] || fail "Expected a linux-x64 Binary SDK."

    local bridge_tfm bridge_sdk
    bridge_tfm="$(json_string "${CONTRACT}" targetFramework)"
    case "${bridge_tfm}" in net8.0|net9.0|net10.0) ;; *) fail "Unsupported Bridge target framework: ${bridge_tfm}." ;; esac
    bridge_sdk="$(json_string "${CONTRACT}" sdkVersion)"
    [[ "${bridge_sdk}" =~ ^10\.0\.[0-9]+$ ]] || fail "Bridge SDK baseline must belong to stable .NET 10: ${bridge_sdk}."
    [[ "$(json_string "${CONTRACT}" languageVersion)" == "14.0" ]] || fail "Bridge 3 Demo requires C# 14.0 contract metadata."

    [[ "$(json_number "${MANIFEST}" schemaVersion)" == "2" ]] || fail "Binary SDK manifest schema 2 is required."
    [[ "$(json_number "${MANIFEST}" current)" == "5" && "$(json_number "${MANIFEST}" minimumSupported)" == "5" ]] || fail "Binary SDK manifest must be ABI5-only."
    [[ "$(json_string "${MANIFEST}" platform)" == "linux-x64" ]] || fail "Binary SDK manifest platform is not linux-x64."
    [[ "$(json_string "${MANIFEST}" targetFramework)" == "${bridge_tfm}" ]] || fail "Binary SDK manifest target framework does not match its contract."
    [[ "$(json_string "${MANIFEST}" sdkVersion)" == "${bridge_sdk}" ]] || fail "Binary SDK manifest SDK baseline does not match its contract."
    [[ "$(json_string "${MANIFEST}" languageVersion)" == "14.0" ]] || fail "Binary SDK manifest language version is not C# 14.0."
    [[ "$(json_string "${MANIFEST}" configuration)" == "Release" ]] || fail "Demo consumes a Release Binary SDK only."
    [[ -n "$(json_string "${MANIFEST}" sourceCommit)" ]] || fail "Binary SDK manifest sourceCommit is missing."

    local hash_lines=0 name expected actual
    while read -r name expected; do
        [[ -n "${name}" && -n "${expected}" ]] || continue
        [[ -f "${DIST_ROOT}/${name}" ]] || fail "Manifest file is missing from SDK: ${name}"
        actual="$(sha256sum "${DIST_ROOT}/${name}" | awk '{print $1}')"
        [[ "${actual}" == "${expected}" ]] || fail "Binary SDK hash mismatch: ${name}"
        hash_lines=$((hash_lines + 1))
    done < <(sed -nE 's/.*"name"[[:space:]]*:[[:space:]]*"([^"]+)"[[:space:]]*,[[:space:]]*"sha256"[[:space:]]*:[[:space:]]*"([0-9a-fA-F]+)".*/\1 \2/p' "${MANIFEST}")
    [[ ${hash_lines} -eq 4 ]] || fail "Linux Binary SDK manifest must hash exactly four files."

    bash "${ROOT_DIR}/tests/check-sdk-consumer.sh" "${ROOT_DIR}"
    log "Bridge $(json_string "${CONTRACT}" bridgeVersion), ABI 5 only, OCCT $(json_string "${CONTRACT}" occtVersion), target ${bridge_tfm}; Demo SDK ${detected_sdk}"
}

build_common() {
    validate_sdk
    dotnet build "${COMMON_PROJECT}" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="$(json_string "${CONTRACT}" bridgeVersion)" --nologo
}

build_avalonia() {
    validate_sdk
    dotnet build "${COMMON_PROJECT}" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="$(json_string "${CONTRACT}" bridgeVersion)" --nologo
    dotnet build "${AVALONIA_PROJECT}" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="$(json_string "${CONTRACT}" bridgeVersion)" --nologo
    local output="${ROOT_DIR}/src/OcctDemo.Avalonia/bin/x64/${CONFIGURATION}/${TFM}"
    [[ -f "${output}/CAD-Avalonia.dll" ]] || fail "CAD-Avalonia.dll was not produced."
    [[ -f "${output}/libOcctNative.so" ]] || fail "libOcctNative.so was not copied beside CAD-Avalonia."
    log "Avalonia: ${output}"
}

clean() {
    rm -rf "${ROOT_DIR}/artifacts"
    for path in src/OcctDemo.Common src/OcctDemo.Avalonia; do
        rm -rf "${ROOT_DIR}/${path}/bin" "${ROOT_DIR}/${path}/obj"
    done
    log "Generated Linux Demo outputs removed."
}

case "${TARGET}" in
    validate) validate_sdk ;;
    common) build_common ;;
    avalonia) build_avalonia ;;
    all) build_avalonia ;;
    clean) clean ;;
    *) fail "Unknown target '${TARGET}'. Use validate, common, avalonia, all, or clean." ;;
esac
