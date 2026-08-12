#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIGURATION="Release"
SKIP_X11=0

for argument in "$@"; do
    case "${argument}" in
        --skip-x11) SKIP_X11=1 ;;
        Debug|Release|RelWithDebInfo|MinSizeRel) CONFIGURATION="${argument}" ;;
        *)
            printf '[publish-linux] ERROR: Unknown argument: %s\n' "${argument}" >&2
            exit 1
            ;;
    esac
done

fail() {
    printf '[publish-linux] ERROR: %s\n' "$*" >&2
    exit 1
}

log() {
    printf '[publish-linux] %s\n' "$*"
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"
}

json_value() {
    local key="$1"
    sed -nE "s/^[[:space:]]*\"${key}\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "${ROOT_DIR}/bridge-contract.json" | head -n 1
}

json_number() {
    local key="$1"
    sed -nE "s/^[[:space:]]*\"${key}\"[[:space:]]*:[[:space:]]*([0-9]+).*/\\1/p" "${ROOT_DIR}/bridge-contract.json" | head -n 1
}

[[ "$(uname -s)" == "Linux" ]] || fail "publish.sh supports Linux only."
require_command git
require_command sha256sum
require_command sed

cd "${ROOT_DIR}"
CURRENT_BRANCH="$(git branch --show-current)"
[[ "${CURRENT_BRANCH}" == "linux" ]] || fail "Linux Binary SDK publishing must run from the linux branch; current branch: ${CURRENT_BRANCH:-<detached>}"
[[ -z "$(git status --porcelain)" ]] || fail "Working tree must be clean before publishing."

BRIDGE_VERSION="$(json_value bridgeVersion)"
ABI_VERSION="$(json_number nativeAbiVersion)"
OCCT_VERSION="$(json_value occtVersion)"
PLATFORM="$(json_value platform)"
TARGET_FRAMEWORK="$(json_value targetFramework)"
SDK_VERSION="$(json_value sdkVersion)"
LANGUAGE_VERSION="$(json_value languageVersion)"

[[ "${PLATFORM}" == "linux-x64" ]] || fail "bridge-contract.json platform must be linux-x64; actual: ${PLATFORM}"
[[ -n "${BRIDGE_VERSION}" && -n "${ABI_VERSION}" && -n "${OCCT_VERSION}" ]] || fail "Unable to read Bridge contract metadata."

log "Building Bridge ${BRIDGE_VERSION}, ABI ${ABI_VERSION}, OCCT ${OCCT_VERSION}..."
"${ROOT_DIR}/build.sh" all "${CONFIGURATION}"

if [[ "${SKIP_X11}" -eq 0 ]]; then
    [[ -n "${DISPLAY:-}" ]] || fail "DISPLAY is not set. Run in X11/XWayland or pass --skip-x11 explicitly."
    "${ROOT_DIR}/build.sh" x11-smoke "${CONFIGURATION}"
else
    log "X11 viewer smoke explicitly skipped."
fi

NATIVE_SOURCE="${ROOT_DIR}/build/native/bin/libOcctNative.so"
CORE_OUTPUT="${ROOT_DIR}/src/OcctNet/bin/x64/${CONFIGURATION}/net10.0"
AVALONIA_OUTPUT="${ROOT_DIR}/src/OcctNet.Avalonia/bin/x64/${CONFIGURATION}/net10.0"

[[ -f "${NATIVE_SOURCE}" ]] || fail "Native bridge output is missing: ${NATIVE_SOURCE}"
[[ -f "${CORE_OUTPUT}/OcctNet.dll" ]] || fail "Managed core output is missing: ${CORE_OUTPUT}/OcctNet.dll"
[[ -f "${AVALONIA_OUTPUT}/OcctNet.Avalonia.dll" ]] || fail "Avalonia output is missing: ${AVALONIA_OUTPUT}/OcctNet.Avalonia.dll"

DIST_ROOT="${ROOT_DIR}/dist/linux-x64"
STAGING="${ROOT_DIR}/dist/.linux-x64-staging-$$"
rm -rf "${STAGING}"
mkdir -p "${STAGING}"
trap 'rm -rf "${STAGING}"' EXIT

cp "${NATIVE_SOURCE}" "${STAGING}/libOcctNative.so"
cp "${CORE_OUTPUT}/OcctNet.dll" "${STAGING}/OcctNet.dll"
cp "${AVALONIA_OUTPUT}/OcctNet.Avalonia.dll" "${STAGING}/OcctNet.Avalonia.dll"
[[ -f "${CORE_OUTPUT}/OcctNet.xml" ]] && cp "${CORE_OUTPUT}/OcctNet.xml" "${STAGING}/OcctNet.xml"
[[ -f "${AVALONIA_OUTPUT}/OcctNet.Avalonia.xml" ]] && cp "${AVALONIA_OUTPUT}/OcctNet.Avalonia.xml" "${STAGING}/OcctNet.Avalonia.xml"
cp "${ROOT_DIR}/bridge-contract.json" "${STAGING}/bridge-contract.json"
cp "${ROOT_DIR}/LICENSE" "${STAGING}/LICENSE"
cp "${ROOT_DIR}/LICENSE_LGPL_21.txt" "${STAGING}/LICENSE_LGPL_21.txt"
cp "${ROOT_DIR}/OcctCSharpBridge_LGPL_EXCEPTION.txt" "${STAGING}/OcctCSharpBridge_LGPL_EXCEPTION.txt"
cp "${ROOT_DIR}/COMMERCIAL.md" "${STAGING}/COMMERCIAL.md"

cat > "${STAGING}/README-LINUX.txt" <<EOF
OcctCSharpBridge ${BRIDGE_VERSION} Linux x64 Binary SDK

Runtime requirements:
- Linux x64
- .NET ${TARGET_FRAMEWORK} / SDK contract ${SDK_VERSION}
- Open CASCADE Technology ${OCCT_VERSION}
- X11/XWayland for OcctNet.Avalonia viewer hosting

Default OCCT development layout used by this branch:
  include: /usr/local/include/opencascade
  library: /usr/local/lib

This Binary SDK contains OcctCSharpBridge binaries only. It does not redistribute OCCT or its third-party shared libraries.
Set OCCT_ROOT/CASROOT and ensure the OCCT shared libraries are available to the dynamic loader when using a non-default installation.
EOF

SOURCE_COMMIT="$(git rev-parse HEAD)"
FILES=(libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json)
if [[ -f "${STAGING}/OcctNet.xml" ]]; then FILES+=(OcctNet.xml); fi
if [[ -f "${STAGING}/OcctNet.Avalonia.xml" ]]; then FILES+=(OcctNet.Avalonia.xml); fi

{
    printf '{\n'
    printf '  "schemaVersion": 1,\n'
    printf '  "author": "zly258",\n'
    printf '  "bridgeVersion": "%s",\n' "${BRIDGE_VERSION}"
    printf '  "nativeAbiVersion": %s,\n' "${ABI_VERSION}"
    printf '  "occtVersion": "%s",\n' "${OCCT_VERSION}"
    printf '  "platform": "%s",\n' "${PLATFORM}"
    printf '  "targetFramework": "%s",\n' "${TARGET_FRAMEWORK}"
    printf '  "sdkVersion": "%s",\n' "${SDK_VERSION}"
    printf '  "languageVersion": "%s",\n' "${LANGUAGE_VERSION}"
    printf '  "configuration": "%s",\n' "${CONFIGURATION}"
    printf '  "sourceCommit": "%s",\n' "${SOURCE_COMMIT}"
    printf '  "occtRuntimeBundled": false,\n'
    printf '  "files": [\n'
    for ((index=0; index<${#FILES[@]}; index++)); do
        name="${FILES[$index]}"
        hash="$(sha256sum "${STAGING}/${name}" | awk '{print $1}')"
        comma=','
        if (( index == ${#FILES[@]} - 1 )); then comma=''; fi
        printf '    { "name": "%s", "sha256": "%s" }%s\n' "${name}" "${hash}" "${comma}"
    done
    printf '  ]\n'
    printf '}\n'
} > "${STAGING}/bridge-manifest.json"

rm -rf "${DIST_ROOT}"
mv "${STAGING}" "${DIST_ROOT}"
trap - EXIT

log "Linux Binary SDK staged at ${DIST_ROOT}"
log "Source commit: ${SOURCE_COMMIT}"
log "OCCT runtime is intentionally not bundled."
