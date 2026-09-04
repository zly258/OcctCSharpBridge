#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIGURATION="${1:-Release}"
DEFAULT_SDK_ROOT="${HOME:-}/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64"
export OCCTCSHARPBRIDGE_SDK="${OCCTCSHARPBRIDGE_SDK:-${DEFAULT_SDK_ROOT}}"

case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) printf '[run-linux] ERROR: Unknown configuration: %s\n' "${CONFIGURATION}" >&2; exit 2 ;; esac

fail() { printf '[run-linux] ERROR: %s\n' "$*" >&2; exit 1; }
log() { printf '[run-linux] %s\n' "$*"; }

[[ "$(uname -s)" == "Linux" ]] || fail "run.sh supports Linux only; use run.ps1 on Windows."
case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
[[ -n "${OCCTCSHARPBRIDGE_SDK:-}" ]] || fail "HOME is not set. Set OCCTCSHARPBRIDGE_SDK explicitly."
command -v dotnet >/dev/null 2>&1 || fail "dotnet was not found in PATH."
[[ -n "${DISPLAY:-}" ]] || fail "DISPLAY is not set. CAD-Avalonia currently requires X11/XWayland."

"${ROOT_DIR}/build.sh" avalonia "${CONFIGURATION}"

APP_DIR="${ROOT_DIR}/src/OcctDemo.Avalonia/bin/x64/${CONFIGURATION}/net10.0"
APP_DLL="${APP_DIR}/CAD-Avalonia.dll"
NATIVE_BRIDGE="${APP_DIR}/libOcctNative.so"
OCCT_ROOT="${OCCT_ROOT:-/usr/local}"
OCCT_LIB_DIR="${OCCT_LIB_DIR:-${OCCT_ROOT}/lib}"

[[ -f "${APP_DLL}" ]] || fail "CAD-Avalonia was not found: ${APP_DLL}"
[[ -f "${NATIVE_BRIDGE}" ]] || fail "libOcctNative.so was not found beside CAD-Avalonia."
[[ -d "${OCCT_LIB_DIR}" ]] || fail "OCCT library directory was not found: ${OCCT_LIB_DIR}"

export OCCT_ROOT
export CASROOT="${CASROOT:-${OCCT_ROOT}}"
export OCCT_BRIDGE_NATIVE_DIR="${APP_DIR}"
export LD_LIBRARY_PATH="${APP_DIR}:${OCCT_LIB_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}"

log "Application: ${APP_DLL}"
log "Bridge SDK:  ${OCCTCSHARPBRIDGE_SDK}"
log "OCCT root:   ${OCCT_ROOT}"
log "DISPLAY:     ${DISPLAY}"
cd "${APP_DIR}"
exec dotnet "${APP_DLL}"
