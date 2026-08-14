#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET="${1:-all}"
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
LANGUAGE_VERSION="$(contract_string languageVersion)"
AUTHOR="$(contract_string author)"
CURRENT_ABI="$(contract_number current)"
SDK_MAJOR="${SDK_VERSION%%.*}"
TFM="net10.0"

validate() {
    [[ "$(uname -s)" == "Linux" ]] || fail "build.sh supports Linux only; use build.ps1 on Windows."
    case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
    case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) fail "Unknown configuration: ${CONFIGURATION}" ;; esac
    require_command cmake
    require_command c++
    require_command dotnet
    [[ -f "${OCCT_INCLUDE_DIR}/Standard.hxx" ]] || fail "OCCT headers were not found under ${OCCT_INCLUDE_DIR}."
    compgen -G "${OCCT_LIB_DIR}/libTKernel.so*" >/dev/null || fail "OCCT TKernel was not found under ${OCCT_LIB_DIR}."
    local detected_sdk="$(dotnet --version)"
    [[ "${detected_sdk%%.*}" == "${SDK_MAJOR}" ]] || fail ".NET ${SDK_MAJOR}.x is required; detected ${detected_sdk}."
    [[ -n "${BRIDGE_VERSION}" && -n "${CURRENT_ABI}" ]] || fail "Bridge contract metadata is incomplete."
    log "Bridge ${BRIDGE_VERSION}, ABI ${CURRENT_ABI}, OCCT ${OCCT_VERSION}, Linux x64"
}

native() {
    validate
    cmake -S "${ROOT_DIR}/src/OcctNative" -B "${BUILD_DIR}" \
        -DCMAKE_BUILD_TYPE="${CONFIGURATION}" \
        -DOCCT_ROOT="${OCCT_ROOT}" \
        -DOCCT_INCLUDE_DIR="${OCCT_INCLUDE_DIR}" \
        -DOCCT_LIB_DIR="${OCCT_LIB_DIR}"
    cmake --build "${BUILD_DIR}" --config "${CONFIGURATION}" --parallel
    [[ -f "${NATIVE_DIR}/libOcctNative.so" ]] || fail "libOcctNative.so was not produced."
}

managed() {
    validate
    dotnet build "${ROOT_DIR}/src/OcctNet/OcctNet.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
    dotnet build "${ROOT_DIR}/src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

test_managed() {
    validate
    dotnet test "${ROOT_DIR}/tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

smoke() {
    native
    dotnet build "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
    export OCCT_BRIDGE_NATIVE_DIR="${NATIVE_DIR}"
    export CASROOT="${CASROOT:-${OCCT_ROOT}}"
    export LD_LIBRARY_PATH="${NATIVE_DIR}:${OCCT_LIB_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}"
    dotnet run --project "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --no-build
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
    sed 's/"platform": "windows-x64"/"platform": "linux-x64"/' "${CONTRACT}" > "${DIST_STAGING}/bridge-contract.json"

    local names=(libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json)
    {
        printf '{\n  "schemaVersion": 1,\n'
        printf '  "author": "%s",\n' "${AUTHOR}"
        printf '  "bridgeVersion": "%s",\n' "${BRIDGE_VERSION}"
        printf '  "nativeAbiVersion": %s,\n' "${CURRENT_ABI}"
        printf '  "occtVersion": "%s",\n' "${OCCT_VERSION}"
        printf '  "platform": "linux-x64",\n'
        printf '  "targetFramework": "%s",\n' "${TFM}"
        printf '  "sdkVersion": "%s",\n' "${SDK_VERSION}"
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
    for path in src/OcctNet src/OcctNet.Avalonia tests/OcctNet.ManagedTests tests/OcctNet.Smoke tests/OcctNet.AvaloniaSmoke tools/OcctApiDocsGenerator; do
        rm -rf "${ROOT_DIR}/${path}/bin" "${ROOT_DIR}/${path}/obj"
    done
}

case "${TARGET}" in
    validate) validate ;;
    native) native ;;
    managed) managed ;;
    test) test_managed ;;
    smoke) smoke ;;
    dist) dist ;;
    clean) clean ;;
    all) native; managed; test_managed; smoke ;;
    *) fail "Unknown target '${TARGET}'. Use validate, native, managed, test, smoke, dist, clean, or all." ;;
esac
