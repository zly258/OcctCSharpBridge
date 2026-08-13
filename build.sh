#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
log() { printf '[avalonia-linux] %s\n' "$*"; }
fail() { printf '[avalonia-linux] ERROR: %s\n' "$*" >&2; exit 1; }

ARG1="${1:-}"
ARG2="${2:-}"
case "${ARG1}" in
    "") TARGET="avalonia"; CONFIGURATION="Release" ;;
    Debug|Release|RelWithDebInfo)
        TARGET="avalonia"
        CONFIGURATION="${ARG1}"
        [[ -z "${ARG2}" ]] || fail "Unexpected second argument: ${ARG2}"
        ;;
    validate|native|managed|test|smoke|avalonia-smoke|avalonia|demo|docs|clean|all)
        TARGET="${ARG1}"
        CONFIGURATION="${ARG2:-Release}"
        ;;
    *) fail "Unknown argument '${ARG1}'. Use Debug/Release/RelWithDebInfo, or an explicit maintenance target." ;;
esac
case "${CONFIGURATION}" in Debug|Release|RelWithDebInfo) ;; *) fail "Unknown configuration '${CONFIGURATION}'." ;; esac

OCCT_ROOT="${OCCT_ROOT:-/usr/local}"
OCCT_INCLUDE_DIR="${OCCT_INCLUDE_DIR:-${OCCT_ROOT}/include/opencascade}"
OCCT_LIB_DIR="${OCCT_LIB_DIR:-${OCCT_ROOT}/lib}"
BUILD_DIR="${ROOT_DIR}/build/native"
NATIVE_BIN_DIR="${BUILD_DIR}/bin/${CONFIGURATION}"
CONTRACT_FILE="${ROOT_DIR}/bridge-contract.json"
DEMO_TFM="net10.0"
DEMO_PROJECT="${ROOT_DIR}/src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
DEMO_OUTPUT="${ROOT_DIR}/src/OcctDemo.Avalonia/bin/x64/${CONFIGURATION}/${DEMO_TFM}"

require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
find_occt_library() { local name="$1"; compgen -G "${OCCT_LIB_DIR}/lib${name}.so*" | head -n 1 || true; }
contract_string() { local key="$1"; sed -nE "s/^[[:space:]]*\"${key}\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "${CONTRACT_FILE}" | head -n 1; }

[[ -f "${CONTRACT_FILE}" ]] || fail "Bridge contract was not found: ${CONTRACT_FILE}"
BRIDGE_VERSION="$(contract_string bridgeVersion)"
DOTNET_SDK_VERSION="$(contract_string sdkVersion)"
DOTNET_SDK_MAJOR="${DOTNET_SDK_VERSION%%.*}"
[[ -n "${BRIDGE_VERSION}" && -n "${DOTNET_SDK_VERSION}" && -n "${DOTNET_SDK_MAJOR}" ]] || fail "Unable to read Bridge version or .NET SDK version from bridge-contract.json."

validate_base() {
    [[ "$(uname -s)" == "Linux" ]] || fail "build.sh supports Linux only; use build.ps1 on Windows."
    case "$(uname -m)" in x86_64|amd64) ;; *) fail "The Avalonia branch currently supports Linux x64 only; detected $(uname -m)." ;; esac
    require_command dotnet
    local sdk_version sdk_major
    sdk_version="$(dotnet --version)"
    sdk_major="${sdk_version%%.*}"
    [[ "${sdk_major}" == "${DOTNET_SDK_MAJOR}" ]] || fail "OcctCSharpBridge requires a stable .NET SDK major ${DOTNET_SDK_MAJOR}; detected ${sdk_version}."
    grep -q '"platform": "cross-platform-x64"' "${CONTRACT_FILE}" || fail "bridge-contract.json is not the Avalonia cross-platform contract."
    grep -q '"windows-x64"' "${CONTRACT_FILE}" || fail "windows-x64 is missing from the contract."
    grep -q '"linux-x64"' "${CONTRACT_FILE}" || fail "linux-x64 is missing from the contract."
}

validate_native() {
    validate_base
    require_command cmake
    require_command c++
    [[ -f "${OCCT_INCLUDE_DIR}/Standard.hxx" ]] || fail "OCCT include directory is invalid: ${OCCT_INCLUDE_DIR}"
    [[ -f "${OCCT_INCLUDE_DIR}/Standard_Version.hxx" ]] || fail "OCCT version header is missing."
    [[ -n "$(find_occt_library TKernel)" ]] || fail "OCCT TKernel library was not found under ${OCCT_LIB_DIR}."
}

validate() {
    validate_native
    local sdk_version; sdk_version="$(dotnet --version)"
    log "Target:        ${TARGET}"
    log "Configuration: ${CONFIGURATION}"
    log "Bridge:        ${BRIDGE_VERSION}"
    log "OCCT root:     ${OCCT_ROOT}"
    log "OCCT include:  ${OCCT_INCLUDE_DIR}"
    log "OCCT lib:      ${OCCT_LIB_DIR}"
    log "Dotnet SDK:    ${sdk_version} (required major ${DOTNET_SDK_MAJOR})"
}

native() {
    validate_native
    cmake -S "${ROOT_DIR}/src/OcctNative" -B "${BUILD_DIR}" \
        -DCMAKE_BUILD_TYPE="${CONFIGURATION}" \
        -DOCCT_ROOT="${OCCT_ROOT}" \
        -DOCCT_INCLUDE_DIR="${OCCT_INCLUDE_DIR}" \
        -DOCCT_LIB_DIR="${OCCT_LIB_DIR}"
    cmake --build "${BUILD_DIR}" --config "${CONFIGURATION}" --parallel
    [[ -f "${NATIVE_BIN_DIR}/libOcctNative.so" ]] || fail "Native bridge was not produced at ${NATIVE_BIN_DIR}/libOcctNative.so"
}

managed() {
    validate_base
    dotnet build "${ROOT_DIR}/src/OcctNet/OcctNet.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
    dotnet build "${ROOT_DIR}/src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

build_demo() {
    validate_base
    [[ -f "${NATIVE_BIN_DIR}/libOcctNative.so" ]] || fail "Native bridge is missing. Run ./build.sh native ${CONFIGURATION} first."
    dotnet build "${ROOT_DIR}/src/OcctDemo.Common/OcctDemo.Common.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
    dotnet build "${DEMO_PROJECT}" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
    [[ -d "${DEMO_OUTPUT}" ]] || fail "Demo output directory was not produced: ${DEMO_OUTPUT}"
    cp -f "${NATIVE_BIN_DIR}/libOcctNative.so" "${DEMO_OUTPUT}/libOcctNative.so"
    [[ -f "${DEMO_OUTPUT}/CAD-Avalonia.dll" ]] || fail "CAD-Avalonia.dll was not produced in ${DEMO_OUTPUT}"
    [[ -f "${DEMO_OUTPUT}/libOcctNative.so" ]] || fail "libOcctNative.so was not deployed beside CAD-Avalonia."
    log "Demo: ${DEMO_OUTPUT}"
}

managed_tests() {
    validate_base
    dotnet test "${ROOT_DIR}/tests/OcctNet.ManagedTests/OcctNet.ManagedTests.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

build_headless_smoke() {
    validate_base
    dotnet build "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

build_avalonia_smoke() {
    validate_base
    dotnet build "${ROOT_DIR}/tests/OcctNet.AvaloniaSmoke/OcctNet.AvaloniaSmoke.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --nologo
}

configure_runtime_environment() {
    export OCCT_ROOT
    export CASROOT="${CASROOT:-${OCCT_ROOT}}"
    export OCCT_BRIDGE_NATIVE_DIR="${NATIVE_BIN_DIR}"
    export LD_LIBRARY_PATH="${NATIVE_BIN_DIR}:${OCCT_LIB_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}"
}

docs() {
    managed
    dotnet run --project "${ROOT_DIR}/tools/OcctApiDocsGenerator/OcctApiDocsGenerator.csproj" -c Release -- --repository-root "${ROOT_DIR}" --configuration "${CONFIGURATION}"
}

smoke() {
    native
    build_headless_smoke
    configure_runtime_environment
    dotnet run --project "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --no-build
}

avalonia_smoke() {
    native
    managed
    build_avalonia_smoke
    configure_runtime_environment
    [[ -n "${DISPLAY:-}" ]] || fail "DISPLAY is not set. The current Linux Avalonia Viewer backend requires X11/XWayland."
    dotnet run --project "${ROOT_DIR}/tests/OcctNet.AvaloniaSmoke/OcctNet.AvaloniaSmoke.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --no-build
}

avalonia_demo() {
    native
    managed
    build_demo
}

clean() {
    rm -rf "${ROOT_DIR}/build" "${ROOT_DIR}/artifacts"
    for path in "src/OcctNet" "src/OcctNet.Avalonia" "src/OcctDemo.Common" "src/OcctDemo.Avalonia" "tests/OcctNet.ManagedTests" "tests/OcctNet.Smoke" "tests/OcctNet.AvaloniaSmoke"; do
        rm -rf "${ROOT_DIR}/${path}/bin" "${ROOT_DIR}/${path}/obj"
    done
    rm -rf "${ROOT_DIR}/tools/OcctApiDocsGenerator/bin" "${ROOT_DIR}/tools/OcctApiDocsGenerator/obj"
}

all() {
    native
    managed
    managed_tests
    build_headless_smoke
    configure_runtime_environment
    dotnet run --project "${ROOT_DIR}/tests/OcctNet.Smoke/OcctNet.Smoke.csproj" -c "${CONFIGURATION}" -p:Platform=x64 -p:Version="${BRIDGE_VERSION}" --no-build
    build_demo
    log "Linux Native + Managed + ManagedTests + headless Smoke + CAD-Avalonia build passed. Use ./run.sh ${CONFIGURATION} in X11/XWayland to launch the demo."
}

case "${TARGET}" in
    validate) validate ;;
    native) native ;;
    managed) managed ;;
    test) managed_tests ;;
    smoke) smoke ;;
    avalonia-smoke) avalonia_smoke ;;
    avalonia|demo) avalonia_demo ;;
    docs) docs ;;
    clean) clean ;;
    all) all ;;
    *) fail "Unknown target '${TARGET}'. Supported maintenance targets: validate, native, managed, test, smoke, avalonia-smoke, avalonia, demo, docs, clean, all." ;;
esac
