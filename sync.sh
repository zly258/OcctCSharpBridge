#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REMOTE="origin"
SOURCE_BRANCH="main"
SDK_ROOT=""
FORCE_REBUILD=false

usage() {
    cat <<'EOF'
Usage: ./sync.sh [options]

Options:
  --remote <name>         Git remote (default: origin)
  --source <branch>       SDK source branch (default: main)
  --sdk-root <directory>  Copy an already generated linux-x64 Binary SDK
  --force-rebuild         Rebuild even when sourceCommit already matches
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --remote) [[ $# -ge 2 ]] || { echo "Missing value for --remote" >&2; exit 2; }; REMOTE="$2"; shift 2 ;;
        --source) [[ $# -ge 2 ]] || { echo "Missing value for --source" >&2; exit 2; }; SOURCE_BRANCH="$2"; shift 2 ;;
        --sdk-root) [[ $# -ge 2 ]] || { echo "Missing value for --sdk-root" >&2; exit 2; }; SDK_ROOT="$2"; shift 2 ;;
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

DESTINATION="${ROOT_DIR}/dist/linux-x64"
WORKSPACE_ROOT="$(cd "${ROOT_DIR}/.." && pwd)"
WORKTREE_ROOT="${WORKSPACE_ROOT}/.OcctCSharpBridge-main-sdk-$$-${RANDOM}"
WORKTREE_ADDED=false

validate_sdk() {
    local root="$1"
    local contract="${root}/bridge-contract.json"
    local manifest="${root}/bridge-manifest.json"
    local sdk_version sdk_roll_forward
    for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json bridge-manifest.json; do
        [[ -f "${root}/${name}" ]] || return 1
    done
    [[ "$(json_number "${contract}" schemaVersion)" == "3" ]] || return 1
    [[ "$(json_number "${contract}" current)" == "5" && "$(json_number "${contract}" minimumSupported)" == "5" ]] || return 1
    [[ "$(json_string "${contract}" policy)" == "abi5-only" ]] || return 1
    [[ "$(json_string "${contract}" platform)" == "linux-x64" ]] || return 1
    [[ "$(json_string "${contract}" targetFramework)" == "net10.0" ]] || return 1
    sdk_version="$(json_string "${contract}" sdkVersion)"
    sdk_roll_forward="$(json_string "${contract}" sdkRollForward)"
    [[ "${sdk_version}" =~ ^10\.0\.[0-9]+$ ]] || return 1
    [[ "${sdk_roll_forward}" == "latestFeature" ]] || return 1
    [[ "$(json_string "${contract}" languageVersion)" == "14.0" ]] || return 1
    [[ "$(json_number "${manifest}" schemaVersion)" == "2" ]] || return 1
    [[ "$(json_number "${manifest}" current)" == "5" && "$(json_number "${manifest}" minimumSupported)" == "5" ]] || return 1
    [[ "$(json_string "${manifest}" platform)" == "linux-x64" ]] || return 1
    [[ "$(json_string "${manifest}" targetFramework)" == "net10.0" ]] || return 1
    [[ "$(json_string "${manifest}" sdkVersion)" == "${sdk_version}" ]] || return 1
    [[ "$(json_string "${manifest}" sdkRollForward)" == "${sdk_roll_forward}" ]] || return 1
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

copy_sdk() {
    local source="$1"
    validate_sdk "${source}" || fail "The supplied SDK is not a valid Bridge 3 ABI5-only linux-x64 Binary SDK."
    rm -rf "${DESTINATION}"
    mkdir -p "${DESTINATION}"
    cp -a "${source}/." "${DESTINATION}/"
    log "Binary SDK synchronized: ${DESTINATION}"
}

cleanup() {
    if [[ "${WORKTREE_ADDED}" == true ]]; then
        git -C "${ROOT_DIR}" worktree remove --force "${WORKTREE_ROOT}" >/dev/null 2>&1 || true
    else
        rm -rf "${WORKTREE_ROOT}" >/dev/null 2>&1 || true
    fi
}
trap cleanup EXIT

require_command git
require_command sha256sum

if [[ -n "${SDK_ROOT}" ]]; then
    [[ "${FORCE_REBUILD}" == false ]] || fail "--force-rebuild cannot be combined with --sdk-root."
    copy_sdk "$(cd "${SDK_ROOT}" && pwd)"
    exit 0
fi

log "Fetching ${REMOTE}/${SOURCE_BRANCH}..."
git -C "${ROOT_DIR}" fetch --quiet "${REMOTE}" "${SOURCE_BRANCH}" || fail "Unable to fetch ${REMOTE}/${SOURCE_BRANCH}."
SOURCE_COMMIT="$(git -C "${ROOT_DIR}" rev-parse "${REMOTE}/${SOURCE_BRANCH}")"
[[ -n "${SOURCE_COMMIT}" ]] || fail "Unable to resolve ${REMOTE}/${SOURCE_BRANCH}."

if [[ "${FORCE_REBUILD}" == false && -d "${DESTINATION}" ]] && validate_sdk "${DESTINATION}"; then
    EXISTING_COMMIT="$(json_string "${DESTINATION}/bridge-manifest.json" sourceCommit)"
    if [[ "${EXISTING_COMMIT}" == "${SOURCE_COMMIT}" ]]; then
        log "Binary SDK already matches ${REMOTE}/${SOURCE_BRANCH} @ ${SOURCE_COMMIT:0:7}; rebuild skipped."
        exit 0
    fi
fi

[[ "${FORCE_REBUILD}" == false ]] || log "Forced Binary SDK rebuild requested."
git -C "${ROOT_DIR}" worktree prune
git -C "${ROOT_DIR}" worktree add --detach "${WORKTREE_ROOT}" "${REMOTE}/${SOURCE_BRANCH}" >/dev/null || fail "Unable to create source worktree."
WORKTREE_ADDED=true

[[ -x "${WORKTREE_ROOT}/build.sh" || -f "${WORKTREE_ROOT}/build.sh" ]] || fail "${REMOTE}/${SOURCE_BRANCH} does not contain build.sh."
log "Building validated linux-x64 Binary SDK from ${REMOTE}/${SOURCE_BRANCH}..."
(
    cd "${WORKTREE_ROOT}"
    bash ./build.sh dist Release
) || fail "Binary SDK build failed on ${REMOTE}/${SOURCE_BRANCH}."

BUILT_SDK="${WORKTREE_ROOT}/dist/linux-x64"
validate_sdk "${BUILT_SDK}" || fail "The generated linux-x64 SDK failed validation."
BUILT_COMMIT="$(json_string "${BUILT_SDK}/bridge-manifest.json" sourceCommit)"
[[ "${BUILT_COMMIT}" == "${SOURCE_COMMIT}" ]] || fail "Generated SDK sourceCommit does not match ${REMOTE}/${SOURCE_BRANCH}."
copy_sdk "${BUILT_SDK}"
