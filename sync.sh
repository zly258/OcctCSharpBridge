#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REMOTE="origin"
SOURCE_BRANCH="main"
SDK_ROOT=""
PORTABLE_ROOT=""
FORCE_REBUILD=false

usage() {
    cat <<'EOF'
Usage: ./sync.sh [options]

Options:
  --remote <name>              Git remote (default: origin)
  --source <branch>            Bridge SDK source branch (default: main)
  --sdk-root <directory>       Use an already generated linux-x64 Binary SDK
  --portable-root <directory>  Matching Portable SDK (required with --sdk-root)
  --force-rebuild              Rebuild Bridge even when the synchronized SDK cache matches
  -h, --help                   Show this help

Without --sdk-root, sync.sh resolves the selected Bridge source branch, builds the
Release linux-x64 Binary SDK, packages its matching Portable SDK, validates both,
and installs them under external/OcctCSharpBridge. Bridge tests and smoke tests are
not run by synchronization.
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --remote) [[ $# -ge 2 ]] || { echo "Missing value for --remote" >&2; exit 2; }; REMOTE="$2"; shift 2 ;;
        --source) [[ $# -ge 2 ]] || { echo "Missing value for --source" >&2; exit 2; }; SOURCE_BRANCH="$2"; shift 2 ;;
        --sdk-root) [[ $# -ge 2 ]] || { echo "Missing value for --sdk-root" >&2; exit 2; }; SDK_ROOT="$2"; shift 2 ;;
        --portable-root) [[ $# -ge 2 ]] || { echo "Missing value for --portable-root" >&2; exit 2; }; PORTABLE_ROOT="$2"; shift 2 ;;
        --force-rebuild) FORCE_REBUILD=true; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Unknown argument: $1" >&2; usage; exit 2 ;;
    esac
done

fail() { printf '[sync-linux] ERROR: %s\n' "$*" >&2; exit 1; }
log() { printf '[sync-linux] %s\n' "$*"; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }
json_number() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*([0-9]+).*/\\1/p" "$1" | head -n 1; }

EXTERNAL_ROOT="${ROOT_DIR}/external"
SOURCE_ROOT="${EXTERNAL_ROOT}/.cache/OcctCSharpBridge-source"
BRIDGE_ROOT="${EXTERNAL_ROOT}/OcctCSharpBridge"
DESTINATION="${BRIDGE_ROOT}/linux-x64"
PORTABLE_DESTINATION="${BRIDGE_ROOT}/portable/linux-x64"

validate_sdk() {
    local root="$1"
    local contract="${root}/bridge-contract.json"
    local manifest="${root}/bridge-manifest.json"

    for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json bridge-manifest.json; do
        [[ -f "${root}/${name}" ]] || return 1
    done

    [[ "$(json_number "${contract}" schemaVersion)" == "3" ]] || return 1
    [[ "$(json_number "${contract}" current)" == "5" ]] || return 1
    [[ "$(json_number "${contract}" minimumSupported)" == "5" ]] || return 1
    [[ "$(json_string "${contract}" policy)" == "abi5-only" ]] || return 1
    [[ "$(json_string "${contract}" platform)" == "linux-x64" ]] || return 1
    [[ "$(json_string "${contract}" languageVersion)" == "14.0" ]] || return 1

    local bridge_tfm bridge_sdk
    bridge_tfm="$(json_string "${contract}" targetFramework)"
    case "${bridge_tfm}" in net8.0|net9.0|net10.0) ;; *) return 1 ;; esac
    bridge_sdk="$(json_string "${contract}" sdkVersion)"
    [[ "${bridge_sdk}" =~ ^10\.0\.[0-9]+$ ]] || return 1

    [[ "$(json_number "${manifest}" schemaVersion)" == "2" ]] || return 1
    [[ "$(json_number "${manifest}" current)" == "5" ]] || return 1
    [[ "$(json_number "${manifest}" minimumSupported)" == "5" ]] || return 1
    [[ "$(json_string "${manifest}" platform)" == "linux-x64" ]] || return 1
    [[ "$(json_string "${manifest}" targetFramework)" == "${bridge_tfm}" ]] || return 1
    [[ "$(json_string "${manifest}" sdkVersion)" == "${bridge_sdk}" ]] || return 1
    [[ "$(json_string "${manifest}" languageVersion)" == "14.0" ]] || return 1
    [[ "$(json_string "${manifest}" configuration)" == "Release" ]] || return 1
    [[ -n "$(json_string "${manifest}" sourceCommit)" ]] || return 1

    local count=0 name expected actual
    while read -r name expected; do
        [[ -n "${name}" && -n "${expected}" ]] || continue
        [[ -f "${root}/${name}" ]] || return 1
        actual="$(sha256sum "${root}/${name}" | awk '{print $1}')"
        [[ "${actual}" == "${expected}" ]] || return 1
        count=$((count + 1))
    done < <(sed -nE 's/.*"name"[[:space:]]*:[[:space:]]*"([^"]+)"[[:space:]]*,[[:space:]]*"sha256"[[:space:]]*:[[:space:]]*"([0-9a-fA-F]+)".*/\1 \2/p' "${manifest}")
    [[ ${count} -eq 4 ]]
}

validate_portable() {
    local root="$1" expected_commit="$2" expected_version="$3"
    [[ -f "${root}/package-manifest.json" ]] || return 1
    [[ -f "${root}/bridge-contract.json" && -f "${root}/bridge-manifest.json" ]] || return 1
    [[ -f "${root}/runtime/libOcctNative.so" ]] || return 1
    [[ -d "${root}/occt/resources" ]] || return 1

    python3 - "${root}" "${expected_commit}" "${expected_version}" <<'PY'
import hashlib, json, os, sys
root, expected_commit, expected_version = sys.argv[1:]
try:
    with open(os.path.join(root, 'package-manifest.json'), encoding='utf-8') as f:
        m = json.load(f)
    if m.get('product') != 'OcctCSharpBridge Portable SDK': raise SystemExit(1)
    if m.get('platform') != 'linux-x64' or not m.get('portableRuntime'): raise SystemExit(1)
    if m.get('bridgeSourceCommit') != expected_commit: raise SystemExit(1)
    if m.get('bridgeVersion') != expected_version: raise SystemExit(1)
    for entry in m.get('files', []):
        path = os.path.join(root, *entry['name'].split('/'))
        if not os.path.isfile(path): raise SystemExit(1)
        h = hashlib.sha256()
        with open(path, 'rb') as f:
            for chunk in iter(lambda: f.read(1024 * 1024), b''): h.update(chunk)
        if h.hexdigest().lower() != str(entry['sha256']).lower(): raise SystemExit(1)
except Exception:
    raise SystemExit(1)
PY
}

copy_sdk() {
    local source="$1"
    validate_sdk "${source}" || fail "The supplied Binary SDK is not a valid Bridge 3 ABI5-only linux-x64 consumer SDK."
    rm -rf "${DESTINATION}"
    mkdir -p "${DESTINATION}"
    cp -a "${source}/." "${DESTINATION}/"
    validate_sdk "${DESTINATION}" || fail "Copied Binary SDK failed validation."
    log "Binary SDK synchronized: ${DESTINATION}"
}

copy_portable() {
    local source="$1" expected_commit="$2" expected_version="$3"
    validate_portable "${source}" "${expected_commit}" "${expected_version}" || fail "The supplied Portable SDK is invalid or does not match the Binary SDK."
    rm -rf "${PORTABLE_DESTINATION}"
    mkdir -p "${PORTABLE_DESTINATION}"
    cp -a "${source}/." "${PORTABLE_DESTINATION}/"
    validate_portable "${PORTABLE_DESTINATION}" "${expected_commit}" "${expected_version}" || fail "Copied Portable SDK failed validation."
    log "Portable Bridge runtime synchronized: ${PORTABLE_DESTINATION}"
}

resolve_source_url() {
    if [[ "${REMOTE}" == "." || "${REMOTE}" == "local" ]]; then
        printf '%s\n' "${ROOT_DIR}"
        return
    fi
    git -C "${ROOT_DIR}" remote get-url "${REMOTE}" 2>/dev/null ||
        fail "Unable to resolve Git remote '${REMOTE}'."
}

prepare_source_cache() {
    local source_url="$1"

    if [[ ! -d "${SOURCE_ROOT}/.git" ]]; then
        rm -rf "${SOURCE_ROOT}"
        mkdir -p "$(dirname "${SOURCE_ROOT}")"
        log "Bridge source cache is missing; cloning ${source_url}..."
        git clone --quiet --filter=blob:none "${source_url}" "${SOURCE_ROOT}" ||
            fail "Unable to clone Bridge source."
    else
        git -C "${SOURCE_ROOT}" remote set-url origin "${source_url}" ||
            fail "Unable to update cached Bridge origin."
    fi

    log "Fetching Bridge source ${SOURCE_BRANCH}..."
    git -C "${SOURCE_ROOT}" fetch --quiet --prune origin "${SOURCE_BRANCH}" ||
        fail "Unable to fetch Bridge source branch '${SOURCE_BRANCH}'."
    SOURCE_COMMIT="$(git -C "${SOURCE_ROOT}" rev-parse FETCH_HEAD)"
    [[ -n "${SOURCE_COMMIT}" ]] || fail "Unable to resolve Bridge source commit."

    git -C "${SOURCE_ROOT}" checkout --quiet --detach --force "${SOURCE_COMMIT}" ||
        fail "Unable to checkout Bridge source commit."
    git -C "${SOURCE_ROOT}" reset --hard --quiet "${SOURCE_COMMIT}" ||
        fail "Unable to reset Bridge source cache."
    git -C "${SOURCE_ROOT}" clean -ffdx --quiet ||
        fail "Unable to clean Bridge source cache."
}

build_source_sdk() {
    local build_script="${SOURCE_ROOT}/build.sh"
    local portable_script="${SOURCE_ROOT}/tools/package-portable-sdk.sh"
    local source_sdk="${SOURCE_ROOT}/dist/linux-x64"
    local portable_output="${SOURCE_ROOT}/artifacts/demo-sync-portable"
    local occt_root="${OCCT_ROOT:-/usr/local}"
    local occt_lib_dir="${OCCT_LIB_DIR:-${occt_root}/lib}"

    [[ -f "${build_script}" ]] || fail "Bridge Linux build script was not found."
    [[ -f "${portable_script}" ]] || fail "Bridge Portable SDK packager was not found."

    log "Building Bridge linux-x64 Binary SDK from ${SOURCE_BRANCH} @ ${SOURCE_COMMIT:0:7}..."
    (
        cd "${SOURCE_ROOT}"
        OCCT_ROOT="${occt_root}" OCCT_LIB_DIR="${occt_lib_dir}" ./build.sh dist Release
    ) || fail "Bridge linux-x64 Binary SDK build failed."

    validate_sdk "${source_sdk}" || fail "Built Bridge Binary SDK failed validation."
    local built_commit bridge_version package_name portable_source
    built_commit="$(json_string "${source_sdk}/bridge-manifest.json" sourceCommit)"
    bridge_version="$(json_string "${source_sdk}/bridge-contract.json" bridgeVersion)"
    [[ "${built_commit}" == "${SOURCE_COMMIT}" ]] ||
        fail "Built Bridge sourceCommit does not match the selected source commit."

    rm -rf "${portable_output}"
    mkdir -p "${portable_output}"
    log "Packaging matching Bridge Portable SDK..."
    bash "${portable_script}" "${source_sdk}" "${occt_root}" "${occt_lib_dir}" "${portable_output}" false ||
        fail "Bridge Portable SDK packaging failed."

    package_name="OcctCSharpBridge-${bridge_version}-linux-x64-portable"
    portable_source="${portable_output}/${package_name}"
    validate_portable "${portable_source}" "${SOURCE_COMMIT}" "${bridge_version}" ||
        fail "Built Bridge Portable SDK failed validation."

    copy_sdk "${source_sdk}"
    copy_portable "${portable_source}" "${SOURCE_COMMIT}" "${bridge_version}"
}

require_command git
require_command sha256sum
require_command python3

if [[ -n "${SDK_ROOT}" ]]; then
    [[ "${FORCE_REBUILD}" == false ]] || fail "--force-rebuild cannot be combined with --sdk-root."
    [[ -n "${PORTABLE_ROOT}" ]] || fail "--portable-root is required with --sdk-root so Binary and Portable SDKs remain one coherent Bridge build."
    SDK_ROOT="$(cd "${SDK_ROOT}" && pwd)"
    PORTABLE_ROOT="$(cd "${PORTABLE_ROOT}" && pwd)"
    validate_sdk "${SDK_ROOT}" || fail "The supplied Binary SDK is invalid."
    SOURCE_COMMIT="$(json_string "${SDK_ROOT}/bridge-manifest.json" sourceCommit)"
    BRIDGE_VERSION="$(json_string "${SDK_ROOT}/bridge-contract.json" bridgeVersion)"
    copy_sdk "${SDK_ROOT}"
    copy_portable "${PORTABLE_ROOT}" "${SOURCE_COMMIT}" "${BRIDGE_VERSION}"
    exit 0
fi
[[ -z "${PORTABLE_ROOT}" ]] || fail "--portable-root is only valid together with --sdk-root."

SOURCE_URL="$(resolve_source_url)"
prepare_source_cache "${SOURCE_URL}"

if [[ "${FORCE_REBUILD}" == false &&
      -d "${DESTINATION}" &&
      -d "${PORTABLE_DESTINATION}" ]] &&
   validate_sdk "${DESTINATION}"; then
    EXISTING_COMMIT="$(json_string "${DESTINATION}/bridge-manifest.json" sourceCommit)"
    BRIDGE_VERSION="$(json_string "${DESTINATION}/bridge-contract.json" bridgeVersion)"
    if [[ "${EXISTING_COMMIT}" == "${SOURCE_COMMIT}" ]] &&
       validate_portable "${PORTABLE_DESTINATION}" "${SOURCE_COMMIT}" "${BRIDGE_VERSION}"; then
        log "Binary SDK and Portable SDK already match ${SOURCE_BRANCH} @ ${SOURCE_COMMIT:0:7}."
        exit 0
    fi
fi

build_source_sdk
log "Synchronization completed for ${SOURCE_BRANCH} @ ${SOURCE_COMMIT:0:7}."
