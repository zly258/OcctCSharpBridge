#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET="${1:-all}"
CONFIGURATION="${2:-Release}"
DEFAULT_SDK_ROOT="${HOME:-}/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64"
DIST_ROOT="${OCCTCSHARPBRIDGE_SDK:-${DEFAULT_SDK_ROOT}}"
CONTRACT="${DIST_ROOT}/bridge-contract.json"
COMMON_PROJECT="${ROOT_DIR}/src/OcctDemo.Common/OcctDemo.Common.csproj"
AVALONIA_PROJECT="${ROOT_DIR}/src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"

export OCCTCSHARPBRIDGE_SDK="${DIST_ROOT}"

log() { printf '[demo-linux] %s\n' "$*"; }
fail() { printf '[demo-linux] ERROR: %s\n' "$*" >&2; exit 1; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }

prepare() {
    [[ "$(uname -s)" == "Linux" ]] || fail "build.sh supports Linux only; use build.ps1 on Windows."
    case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
    case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) fail "Unknown configuration: ${CONFIGURATION}" ;; esac
    require_command dotnet
    if [[ -z "${OCCTCSHARPBRIDGE_SDK:-}" && -z "${HOME:-}" ]]; then
        fail "HOME is not set. Set OCCTCSHARPBRIDGE_SDK explicitly."
    fi

    local missing=()
    local name
    for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json bridge-manifest.json; do
        [[ -f "${DIST_ROOT}/${name}" ]] || missing+=("${name}")
    done
    if [[ ${#missing[@]} -ne 0 ]]; then
        fail "Shared OcctCSharpBridge SDK is missing or incomplete at '${DIST_ROOT}': ${missing[*]}. Run Bridge main ./publish.sh as the current user, or set OCCTCSHARPBRIDGE_SDK."
    fi
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
log "Bridge SDK: ${DIST_ROOT}"
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
