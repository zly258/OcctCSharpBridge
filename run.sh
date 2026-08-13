#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARG1="${1:-}"
ARG2="${2:-}"
case "${ARG1}" in
    "") CONFIGURATION="Release" ;;
    Debug|Release|RelWithDebInfo) CONFIGURATION="${ARG1}"; [[ -z "${ARG2}" ]] || { printf '[avalonia-linux] ERROR: Unexpected second argument: %s\n' "${ARG2}" >&2; exit 1; } ;;
    avalonia) CONFIGURATION="${ARG2:-Release}" ;;
    *) printf '[avalonia-linux] ERROR: Expected Debug, Release, or RelWithDebInfo; received: %s\n' "${ARG1}" >&2; exit 1 ;;
esac
case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) printf '[avalonia-linux] ERROR: Unknown configuration: %s\n' "${CONFIGURATION}" >&2; exit 1 ;; esac

OCCT_ROOT="${OCCT_ROOT:-/usr/local}"
OCCT_LIB_DIR="${OCCT_LIB_DIR:-${OCCT_ROOT}/lib}"
DEMO_TFM="net10.0"
DEMO_OUTPUT="${ROOT_DIR}/src/OcctDemo.Avalonia/bin/x64/${CONFIGURATION}/${DEMO_TFM}"
APP_DLL="${DEMO_OUTPUT}/CAD-Avalonia.dll"
NATIVE_BRIDGE="${DEMO_OUTPUT}/libOcctNative.so"

fail() { printf '[avalonia-linux] ERROR: %s\n' "$*" >&2; exit 1; }

[[ "$(uname -s)" == "Linux" ]] || fail "run.sh supports Linux only; use run.ps1 on Windows."
case "$(uname -m)" in x86_64|amd64) ;; *) fail "CAD-Avalonia currently supports Linux x64 only; detected $(uname -m)." ;; esac
command -v dotnet >/dev/null 2>&1 || fail "dotnet was not found in PATH."
[[ -n "${DISPLAY:-}" ]] || fail "DISPLAY is not set. CAD-Avalonia currently requires an X11/XWayland desktop session."
[[ -f "${APP_DLL}" ]] || fail "CAD-Avalonia was not found: ${APP_DLL}. Run ./build.sh ${CONFIGURATION} first."
[[ -f "${NATIVE_BRIDGE}" ]] || fail "libOcctNative.so was not found beside CAD-Avalonia. Run ./build.sh ${CONFIGURATION} first."
[[ -d "${OCCT_LIB_DIR}" ]] || fail "OCCT library directory was not found: ${OCCT_LIB_DIR}"

export OCCT_ROOT
export CASROOT="${CASROOT:-${OCCT_ROOT}}"
export OCCT_BRIDGE_NATIVE_DIR="${DEMO_OUTPUT}"
export LD_LIBRARY_PATH="${DEMO_OUTPUT}:${OCCT_LIB_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}"

printf 'Application: %s\n' "${APP_DLL}"
printf 'OCCT root:   %s\n' "${OCCT_ROOT}"
printf 'DISPLAY:     %s\n' "${DISPLAY}"

cd "${DEMO_OUTPUT}"
exec dotnet "${APP_DLL}"
