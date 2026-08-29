#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET="${1:-build}"
CONFIGURATION="${2:-Release}"
OCCT_ROOT="${OCCT_ROOT:-/usr/local}"
OCCT_INCLUDE_DIR="${OCCT_INCLUDE_DIR:-${OCCT_ROOT}/include/opencascade}"
OCCT_LIB_DIR="${OCCT_LIB_DIR:-${OCCT_ROOT}/lib}"
BUILD_DIR="${ROOT_DIR}/build/native"
NATIVE_DIR="${BUILD_DIR}/bin/${CONFIGURATION}"
CONTRACT="${ROOT_DIR}/bridge-contract.json"
DIST_ROOT="${ROOT_DIR}/dist/linux-x64"
DIST_STAGING="${ROOT_DIR}/dist/.linux-x64-staging"

log() { printf '[bridge-linux] %s\n' "$*"; }
fail() { printf '[bridge-linux] ERROR: %s\n' "$*" >&2; exit 1; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
contract_string() { sed -nE "s/^[[:space:]]*\"$1\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "${CONTRACT}" | head -n 1; }
contract_number() { sed -nE "s/^[[:space:]]*\"$1\"[[:space:]]*:[[:space:]]*([0-9]+).*/\\1/p" "${CONTRACT}" | head -n 1; }

[[ -f "${CONTRACT}" ]] || fail "Bridge contract was not found: ${CONTRACT}"
BRIDGE_VERSION="$(contract_string bridgeVersion)"
OCCT_VERSION="$(contract_string occtVersion)"
SDK_VERSION="$(contract_string sdkVersion)"
SDK_ROLL_FORWARD="$(contract_string sdkRollForward)"
LANGUAGE_VERSION="$(contract_string languageVersion)"
AUTHOR="$(contract_string author)"
SOURCE_PLATFORM="$(contract_string platform)"
CURRENT_ABI="$(contract_number current)"
MINIMUM_ABI="$(contract_number minimumSupported)"
TFM="$(contract_string targetFramework)"
RESOLVED_SDK_VERSION=""

sdk_is_compatible() {
    local detected="$1"
    [[ "${SDK_ROLL_FORWARD}" == "latestFeature" ]] || return 1
    [[ "${detected}" != *-* ]] || return 1
    local min_major min_minor min_band detected_major detected_minor detected_band
    IFS=. read -r min_major min_minor min_band <<<"${SDK_VERSION}"
    IFS=. read -r detected_major detected_minor detected_band <<<"${detected}"
    [[ "${min_major}" =~ ^[0-9]+$ && "${min_minor}" =~ ^[0-9]+$ && "${min_band}" =~ ^[0-9]+$ ]] || return 1
    [[ "${detected_major}" =~ ^[0-9]+$ && "${detected_minor}" =~ ^[0-9]+$ && "${detected_band}" =~ ^[0-9]+$ ]] || return 1
    [[ "${detected_major}" == "${min_major}" && "${detected_minor}" == "${min_minor}" ]] || return 1
    ((10#${detected_band} >= 10#${min_band}))
}

validate_common() {
    [[ "$(uname -s)" == "Linux" ]] || fail "build.sh supports Linux only; use build.ps1 on Windows."
    case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
    case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) fail "Unknown configuration: ${CONFIGURATION}" ;; esac
    require_command dotnet
    [[ "${SOURCE_PLATFORM}" == "cross-platform-x64" ]] || fail "Source contract platform must be cross-platform-x64; found ${SOURCE_PLATFORM}."
    RESOLVED_SDK_VERSION="$(cd "${ROOT_DIR}" && dotnet --version)"
    sdk_is_compatible "${RESOLVED_SDK_VERSION}" || fail "A stable .NET 10 SDK compatible with baseline ${SDK_VERSION} / ${SDK_ROLL_FORWARD} is required; detected ${RESOLVED_SDK_VERSION}."
    [[ -n "${BRIDGE_VERSION}" && "${CURRENT_ABI}" == "5" && "${MINIMUM_ABI}" == "5" ]] || fail "Bridge contract must be complete and ABI5-only."
}

validate_native() {
    validate_common
    require_command cmake
    require_command c++
    [[ -f "${OCCT_INCLUDE_DIR}/Standard.hxx" ]] || fail "OCCT headers were not found under ${OCCT_INCLUDE_DIR}."
    compgen -G "${OCCT_LIB_DIR}/libTKernel.so*" >/dev/null || fail "OCCT TKernel was not found under ${OCCT_LIB_DIR}."
    log "Bridge ${BRIDGE_VERSION}, ABI ${CURRENT_ABI}, OCCT ${OCCT_VERSION}, Linux x64"
}

native() {
    validate_native
    cmake -S "${ROOT_DIR}/src/OcctNative" -B "${BUILD_DIR}" \
        -DCMAKE_BUILD_TYPE="${CONFIGURATION}" \
        -DOCCT_ROOT="${OCCT_ROOT}" \
        -DOCCT_INCLUDE_DIR="${OCCT_INCLUDE_DIR}" \
        -DOCCT_LIB_DIR="${OCCT_LIB_DIR}"
    cmake --build "${BUILD_DIR}" --config "${CONFIGURATION}" --parallel
    [[ -f "${NATIVE_DIR}/libOcctNative.so" ]] || fail "libOcctNative.so was not produced."
}

managed() {
    validate_common
    dotnet build "${ROOT_DIR}/src/OcctNet/OcctNet.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
    dotnet build "${ROOT_DIR}/src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

dist() {
    [[ "${CONFIGURATION}" == "Release" ]] || fail "Binary SDK distribution is Release-only."
    require_command git
    require_command sha256sum
    [[ -z "$(git -C "${ROOT_DIR}" status --porcelain --untracked-files=all)" ]] || fail "The working tree must be clean before producing dist/linux-x64."
    local source_commit="$(git -C "${ROOT_DIR}" rev-parse HEAD)"
    native
    managed

    local core="${ROOT_DIR}/src/OcctNet/bin/x64/Release/${TFM}/OcctNet.dll"
    local avalonia="${ROOT_DIR}/src/OcctNet.Avalonia/bin/x64/Release/${TFM}/OcctNet.Avalonia.dll"
    [[ -f "${core}" && -f "${avalonia}" ]] || fail "Managed SDK outputs are incomplete."
    rm -rf "${DIST_STAGING}"
    mkdir -p "${DIST_STAGING}"
    cp -f "${NATIVE_DIR}/libOcctNative.so" "${DIST_STAGING}/"
    cp -f "${core}" "${avalonia}" "${DIST_STAGING}/"
    sed 's/"platform": "cross-platform-x64"/"platform": "linux-x64"/' "${CONTRACT}" > "${DIST_STAGING}/bridge-contract.json"
    grep -q '"platform": "linux-x64"' "${DIST_STAGING}/bridge-contract.json" || fail "Linux distribution contract was not specialized to linux-x64."

    local names=(libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json)
    {
        printf '{\n  "schemaVersion": 2,\n'
        printf '  "author": "%s",\n' "${AUTHOR}"
        printf '  "bridgeVersion": "%s",\n' "${BRIDGE_VERSION}"
        printf '  "nativeAbi": {\n'
        printf '    "current": %s,\n' "${CURRENT_ABI}"
        printf '    "minimumSupported": %s\n' "${MINIMUM_ABI}"
        printf '  },\n'
        printf '  "occtVersion": "%s",\n' "${OCCT_VERSION}"
        printf '  "platform": "linux-x64",\n'
        printf '  "targetFramework": "%s",\n' "${TFM}"
        printf '  "sdkVersion": "%s",\n' "${SDK_VERSION}"
        printf '  "resolvedSdkVersion": "%s",\n' "${RESOLVED_SDK_VERSION}"
        printf '  "languageVersion": "%s",\n' "${LANGUAGE_VERSION}"
        printf '  "configuration": "Release",\n'
        printf '  "sourceCommit": "%s",\n  "files": [\n' "${source_commit}"
        local index=0 name hash comma
        for name in "${names[@]}"; do
            hash="$(sha256sum "${DIST_STAGING}/${name}" | awk '{print $1}')"
            index=$((index + 1)); comma=","; [[ ${index} -eq ${#names[@]} ]] && comma=""
            printf '    { "name": "%s", "sha256": "%s" }%s\n' "${name}" "${hash}" "${comma}"
        done
        printf '  ]\n}\n'
    } > "${DIST_STAGING}/bridge-manifest.json"

    rm -rf "${DIST_ROOT}"
    mv "${DIST_STAGING}" "${DIST_ROOT}"
    log "Binary SDK: ${DIST_ROOT}"
}

clean() {
    rm -rf "${ROOT_DIR}/build" "${ROOT_DIR}/artifacts"
    for path in src/OcctNet src/OcctNet.Avalonia; do
        rm -rf "${ROOT_DIR}/${path}/bin" "${ROOT_DIR}/${path}/obj"
    done
}

case "${TARGET}" in
    build) native; managed ;;
    dist) dist ;;
    clean) clean ;;
    *) fail "Unknown target '${TARGET}'. Use build, dist, or clean." ;;
esac
