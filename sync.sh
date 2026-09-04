#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REMOTE="origin"
SOURCE_BRANCH="main"
DEFAULT_SDK_ROOT="/usr/local/lib/OcctCSharpBridge/SDK/3.0/linux-x64"
SDK_ROOT="${OCCTCSHARPBRIDGE_SDK:-${DEFAULT_SDK_ROOT}}"
PORTABLE_ROOT=""
FORCE_REPACKAGE=false

usage() {
    cat <<'EOF'
Usage: ./sync.sh [options]

Options:
  --remote <name>              Git remote used to obtain the matching packager (default: origin)
  --source <branch>            Bridge source branch containing the installed SDK commit (default: main)
  --sdk-root <directory>       Installed linux-x64 Binary SDK (default: OCCTCSHARPBRIDGE_SDK or /usr/local/lib/OcctCSharpBridge/SDK/3.0/linux-x64)
  --portable-root <directory>  Copy an already generated matching Portable SDK instead of packaging one
  --force                      Regenerate the Portable SDK even when the local cache already matches
  --force-rebuild              Legacy alias for --force; the Binary SDK is not rebuilt
  -h, --help                   Show this help

The Demo consumes the installed Binary SDK directly. sync.sh only prepares the
matching Portable SDK payload required by Demo publication under
external/OcctCSharpBridge/portable/linux-x64.
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --remote) [[ $# -ge 2 ]] || { echo "Missing value for --remote" >&2; exit 2; }; REMOTE="$2"; shift 2 ;;
        --source) [[ $# -ge 2 ]] || { echo "Missing value for --source" >&2; exit 2; }; SOURCE_BRANCH="$2"; shift 2 ;;
        --sdk-root) [[ $# -ge 2 ]] || { echo "Missing value for --sdk-root" >&2; exit 2; }; SDK_ROOT="$2"; shift 2 ;;
        --portable-root) [[ $# -ge 2 ]] || { echo "Missing value for --portable-root" >&2; exit 2; }; PORTABLE_ROOT="$2"; shift 2 ;;
        --force|--force-rebuild) FORCE_REPACKAGE=true; shift ;;
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
PORTABLE_DESTINATION="${EXTERNAL_ROOT}/OcctCSharpBridge/portable/linux-x64"

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

copy_portable() {
    local source="$1" expected_commit="$2" expected_version="$3"
    validate_portable "${source}" "${expected_commit}" "${expected_version}" || fail "The supplied Portable SDK is invalid or does not match the installed Binary SDK."
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
    git -C "${ROOT_DIR}" remote get-url "${REMOTE}" 2>/dev/null || fail "Unable to resolve Git remote '${REMOTE}'."
}

prepare_source_cache() {
    local source_url="$1" source_commit="$2"

    if [[ ! -d "${SOURCE_ROOT}/.git" ]]; then
        rm -rf "${SOURCE_ROOT}"
        mkdir -p "$(dirname "${SOURCE_ROOT}")"
        log "Bridge source cache is missing; cloning ${source_url}..."
        git clone --quiet --filter=blob:none "${source_url}" "${SOURCE_ROOT}" || fail "Unable to clone Bridge source."
    else
        git -C "${SOURCE_ROOT}" remote set-url origin "${source_url}" || fail "Unable to update cached Bridge origin."
    fi

    log "Fetching Bridge source ${SOURCE_BRANCH}..."
    git -C "${SOURCE_ROOT}" fetch --quiet --prune origin "${SOURCE_BRANCH}" || fail "Unable to fetch Bridge source branch '${SOURCE_BRANCH}'."
    local branch_commit
    branch_commit="$(git -C "${SOURCE_ROOT}" rev-parse FETCH_HEAD)"
    git -C "${SOURCE_ROOT}" cat-file -e "${source_commit}^{commit}" 2>/dev/null ||
        git -C "${SOURCE_ROOT}" fetch --quiet origin "${source_commit}" || fail "Installed SDK source commit ${source_commit} is not available from ${REMOTE}."
    git -C "${SOURCE_ROOT}" merge-base --is-ancestor "${source_commit}" "${branch_commit}" || fail "Installed SDK commit ${source_commit} is not contained in ${SOURCE_BRANCH}."

    git -C "${SOURCE_ROOT}" checkout --quiet --detach --force "${source_commit}" || fail "Unable to checkout installed SDK source commit."
    git -C "${SOURCE_ROOT}" reset --hard --quiet "${source_commit}" || fail "Unable to reset Bridge source cache."
    git -C "${SOURCE_ROOT}" clean -ffdx --quiet || fail "Unable to clean Bridge source cache."
}

package_installed_sdk() {
    local source_commit="$1" bridge_version="$2"
    local portable_script="${SOURCE_ROOT}/tools/package-portable-sdk.sh"
    local portable_output="${SOURCE_ROOT}/artifacts/demo-sync-portable"
    local package_name="OcctCSharpBridge-${bridge_version}-linux-x64-portable"
    local portable_source="${portable_output}/${package_name}"
    local occt_root="${OCCT_ROOT:-/usr/local}"
    local occt_lib_dir="${OCCT_LIB_DIR:-${occt_root}/lib}"

    [[ -f "${portable_script}" ]] || fail "Matching Bridge Portable SDK packager was not found."
    rm -rf "${portable_output}"
    mkdir -p "${portable_output}"
    log "Packaging Portable SDK for installed Bridge ${bridge_version} @ ${source_commit:0:7}..."
    bash "${portable_script}" "${SDK_ROOT}" "${occt_root}" "${occt_lib_dir}" "${portable_output}" false || fail "Bridge Portable SDK packaging failed."
    copy_portable "${portable_source}" "${source_commit}" "${bridge_version}"
}

require_command git
require_command sha256sum
require_command python3

[[ "$(uname -s)" == "Linux" ]] || fail "sync.sh supports Linux only."
case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac

SDK_ROOT="$(cd "${SDK_ROOT}" 2>/dev/null && pwd)" || fail "Installed Binary SDK root was not found: ${SDK_ROOT}. Run Bridge main ./publish.sh or set OCCTCSHARPBRIDGE_SDK."
export OCCTCSHARPBRIDGE_SDK="${SDK_ROOT}"
validate_sdk "${SDK_ROOT}" || fail "Installed Binary SDK is invalid or incomplete: ${SDK_ROOT}"
SOURCE_COMMIT="$(json_string "${SDK_ROOT}/bridge-manifest.json" sourceCommit)"
BRIDGE_VERSION="$(json_string "${SDK_ROOT}/bridge-contract.json" bridgeVersion)"
[[ -n "${SOURCE_COMMIT}" && -n "${BRIDGE_VERSION}" ]] || fail "Installed Binary SDK metadata is incomplete."

if [[ -n "${PORTABLE_ROOT}" ]]; then
    PORTABLE_ROOT="$(cd "${PORTABLE_ROOT}" && pwd)" || fail "Portable SDK root was not found: ${PORTABLE_ROOT}"
    copy_portable "${PORTABLE_ROOT}" "${SOURCE_COMMIT}" "${BRIDGE_VERSION}"
    exit 0
fi

if [[ "${FORCE_REPACKAGE}" == false && -d "${PORTABLE_DESTINATION}" ]] &&
   validate_portable "${PORTABLE_DESTINATION}" "${SOURCE_COMMIT}" "${BRIDGE_VERSION}"; then
    log "Portable SDK already matches installed Bridge ${BRIDGE_VERSION} @ ${SOURCE_COMMIT:0:7}."
    exit 0
fi

SOURCE_URL="$(resolve_source_url)"
prepare_source_cache "${SOURCE_URL}" "${SOURCE_COMMIT}"
package_installed_sdk "${SOURCE_COMMIT}" "${BRIDGE_VERSION}"
log "Synchronization completed for installed Bridge ${BRIDGE_VERSION} @ ${SOURCE_COMMIT:0:7}."
