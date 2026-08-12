#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET="${1:-all}"
CONFIGURATION="${2:-Release}"

OCCT_ROOT="${OCCT_ROOT:-/usr/local}"
OCCT_INCLUDE_DIR="${OCCT_INCLUDE_DIR:-${OCCT_ROOT}/include/opencascade}"
OCCT_LIB_DIR="${OCCT_LIB_DIR:-${OCCT_ROOT}/lib}"
BUILD_DIR="${ROOT_DIR}/build/native"
NATIVE_BIN_DIR="${BUILD_DIR}/bin"
DOTNET_SDK_VERSION="10.0.302"

log() {
    printf '[linux] %s\n' "$*"
}

fail() {
    printf '[linux] ERROR: %s\n' "$*" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"
}

find_occt_library() {
    local name="$1"
    compgen -G "${OCCT_LIB_DIR}/lib${name}.so*" | head -n 1 || true
}

validate() {
    [[ "$(uname -s)" == "Linux" ]] || fail "build.sh supports Linux only."

    case "$(uname -m)" in
        x86_64|amd64) ;;
        *) fail "The linux branch currently supports Linux x64 only; detected $(uname -m)." ;;
    esac

    require_command cmake
    require_command dotnet
    require_command c++

    local sdk_version
    sdk_version="$(dotnet --version)"
    [[ "${sdk_version}" == "${DOTNET_SDK_VERSION}" ]] || \
        fail "OcctCSharpBridge requires .NET SDK ${DOTNET_SDK_VERSION}; detected ${sdk_version}."

    [[ -f "${OCCT_INCLUDE_DIR}/Standard.hxx" ]] || \
        fail "OCCT include directory is invalid: ${OCCT_INCLUDE_DIR}"
    [[ -f "${OCCT_INCLUDE_DIR}/Standard_Version.hxx" ]] || \
        fail "OCCT version header is missing: ${OCCT_INCLUDE_DIR}/Standard_Version.hxx"
    [[ -n "$(find_occt_library TKernel)" ]] || \
        fail "OCCT TKernel library was not found under ${OCCT_LIB_DIR}."

    log "Target:        ${TARGET}"
    log "Configuration: ${CONFIGURATION}"
    log "OCCT root:     ${OCCT_ROOT}"
    log "OCCT include:  ${OCCT_INCLUDE_DIR}"
    log "OCCT lib:      ${OCCT_LIB_DIR}"
    log "Dotnet SDK:    ${sdk_version}"
}

native() {
    validate
    log "Configuring native Linux bridge..."
    cmake \
        -S "${ROOT_DIR}/src/OcctNative" \
        -B "${BUILD_DIR}" \
        -DCMAKE_BUILD_TYPE="${CONFIGURATION}" \
        -DOCCT_ROOT="${OCCT_ROOT}" \
        -DOCCT_INCLUDE_DIR="${OCCT_INCLUDE_DIR}" \
        -DOCCT_LIB_DIR="${OCCT_LIB_DIR}"

    log "Building libOcctNative.so..."
    cmake --build "${BUILD_DIR}" --config "${CONFIGURATION}" --parallel

    [[ -f "${NATIVE_BIN_DIR}/libOcctNative.so" ]] || \
        fail "Native bridge was not produced at ${NATIVE_BIN_DIR}/libOcctNative.so"
}

managed() {
    validate
    log "Building OcctNet core..."
    dotnet build "${ROOT_DIR}/src/OcctNet/OcctNet.csproj" \
        -c "${CONFIGURATION}" -p:Platform=x64 --nologo

    log "Building Avalonia adapter..."
    dotnet build "${ROOT_DIR}/src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" \
        -c "${CONFIGURATION}" -p:Platform=x64 --nologo
}

test_managed() {
    validate
    log "Compiling Linux headless smoke consumer..."
    dotnet build "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" \
        -c "${CONFIGURATION}" -p:Platform=x64 --nologo
}

build_x11_smoke() {
    validate
    log "Compiling Linux X11 viewer smoke consumer..."
    dotnet build "${ROOT_DIR}/tests/OcctNet.X11Smoke/OcctNet.X11Smoke.csproj" \
        -c "${CONFIGURATION}" -p:Platform=x64 --nologo
}

configure_runtime_environment() {
    export OCCT_ROOT
    export CASROOT="${CASROOT:-${OCCT_ROOT}}"
    export OCCT_BRIDGE_NATIVE_DIR="${NATIVE_BIN_DIR}"
    export LD_LIBRARY_PATH="${NATIVE_BIN_DIR}:${OCCT_LIB_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}"
}

smoke() {
    native
    test_managed
    configure_runtime_environment

    log "Running headless native smoke tests..."
    dotnet run \
        --project "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" \
        -c "${CONFIGURATION}" \
        -p:Platform=x64 \
        --no-build
}

x11_smoke() {
    native
    build_x11_smoke
    configure_runtime_environment
    [[ -n "${DISPLAY:-}" ]] || fail "DISPLAY is not set. X11 viewer smoke requires an X11/XWayland desktop session."

    log "Running X11 viewer smoke tests on DISPLAY=${DISPLAY}..."
    dotnet run \
        --project "${ROOT_DIR}/tests/OcctNet.X11Smoke/OcctNet.X11Smoke.csproj" \
        -c "${CONFIGURATION}" \
        -p:Platform=x64 \
        --no-build
}

clean() {
    log "Cleaning Linux build outputs..."
    rm -rf "${ROOT_DIR}/build/native"
    rm -rf "${ROOT_DIR}/src/OcctNet/bin" "${ROOT_DIR}/src/OcctNet/obj"
    rm -rf "${ROOT_DIR}/src/OcctNet.Avalonia/bin" "${ROOT_DIR}/src/OcctNet.Avalonia/obj"
    rm -rf "${ROOT_DIR}/tests/OcctNet.Smoke/bin" "${ROOT_DIR}/tests/OcctNet.Smoke/obj"
    rm -rf "${ROOT_DIR}/tests/OcctNet.X11Smoke/bin" "${ROOT_DIR}/tests/OcctNet.X11Smoke/obj"
}

all() {
    native
    managed
    test_managed
    build_x11_smoke
    configure_runtime_environment
    log "Running headless native smoke tests..."
    dotnet run \
        --project "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" \
        -c "${CONFIGURATION}" \
        -p:Platform=x64 \
        --no-build
    log "Linux build completed. X11 viewer smoke is separate: ./build.sh x11-smoke ${CONFIGURATION}"
}

case "${TARGET}" in
    validate) validate ;;
    native) native ;;
    managed) managed ;;
    test) test_managed ;;
    smoke) smoke ;;
    x11-smoke) x11_smoke ;;
    clean) clean ;;
    all) all ;;
    *) fail "Unknown target '${TARGET}'. Supported targets: validate, native, managed, test, smoke, x11-smoke, clean, all." ;;
esac
