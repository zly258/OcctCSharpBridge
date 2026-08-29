#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET="${1:-all}"
CONFIGURATION="${2:-Release}"
DIST_ROOT="${ROOT_DIR}/external/OcctCSharpBridge/linux-x64"
CONTRACT="${DIST_ROOT}/bridge-contract.json"
COMMON_PROJECT="${ROOT_DIR}/src/OcctDemo.Common/OcctDemo.Common.csproj"
AVALONIA_PROJECT="${ROOT_DIR}/src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"

log() { printf '[demo-linux] %s\n' "$*"; }
fail() { printf '[demo-linux] ERROR: %s\n' "$*" >&2; exit 1; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }

prepare() {
    [[ "$(uname -s)" == "Linux" ]] || fail "build.sh supports Linux only; use build.ps1 on Windows."
    case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
    case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) fail "Unknown configuration: ${CONFIGURATION}" ;; esac
    require_command dotnet

    local missing=0
    for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json; do
        [[ -f "${DIST_ROOT}/${name}" ]] || missing=1
    done
    if [[ ${missing} -ne 0 ]]; then
        [[ -x "${ROOT_DIR}/sync.sh" ]] || fail "Bridge SDK is missing and sync.sh is unavailable."
        log "Bridge SDK is missing; synchronizing main..."
        "${ROOT_DIR}/sync.sh"
    fi

    for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json; do
        [[ -f "${DIST_ROOT}/${name}" ]] || fail "Bridge SDK is incomplete: ${name} is missing."
    done
}

build_project() {
    dotnet build "$1" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

clean() {
    rm -rf "${ROOT_DIR}/artifacts"
    for path in src/OcctDemo.Common src/OcctDemo.Avalonia; do
        rm -rf "${ROOT_DIR}/${path}/bin" "${ROOT_DIR}/${path}/obj"
    done
    log "Generated demo outputs removed."
}

if [[ "${TARGET}" == "clean" ]]; then
    clean
    exit 0
fi

prepare
BRIDGE_VERSION="$(json_string "${CONTRACT}" bridgeVersion)"
[[ -n "${BRIDGE_VERSION}" ]] || fail "Bridge version is missing from bridge-contract.json."
log "Bridge ${BRIDGE_VERSION}"

case "${TARGET}" in
    common) build_project "${COMMON_PROJECT}" ;;
    avalonia) build_project "${AVALONIA_PROJECT}" ;;
    all)
        build_project "${COMMON_PROJECT}"
        build_project "${AVALONIA_PROJECT}"
        ;;
    *) fail "Unknown target '${TARGET}'. Use common, avalonia, all, or clean." ;;
esac
