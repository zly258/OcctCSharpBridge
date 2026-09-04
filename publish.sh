#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REMOTE="origin"
OUTPUT_ROOT="${ROOT_DIR}/artifacts/publish"
INSTALL_ROOT="${OCCTCSHARPBRIDGE_SDK:-}"
CREATE_ARCHIVE=true

usage() {
    cat <<'EOF'
Usage: ./publish.sh [remote] [options]

Options:
  --output <directory>        Portable SDK artifact root (default: artifacts/publish)
  --install-root <directory>  Complete SDK install root (default: $HOME/.local/share/OcctCSharpBridge/SDK/<major.minor>/linux-x64)
  --no-archive                Do not create the .tar.gz archive
  -h, --help                  Show this help
EOF
}

if [[ $# -gt 0 && "$1" != --* && "$1" != "-h" ]]; then
    REMOTE="$1"
    shift
fi
while [[ $# -gt 0 ]]; do
    case "$1" in
        --output) [[ $# -ge 2 ]] || { echo "Missing value for --output" >&2; exit 2; }; OUTPUT_ROOT="$2"; shift 2 ;;
        --install-root) [[ $# -ge 2 ]] || { echo "Missing value for --install-root" >&2; exit 2; }; INSTALL_ROOT="$2"; shift 2 ;;
        --no-archive) CREATE_ARCHIVE=false; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Unknown argument: $1" >&2; usage; exit 2 ;;
    esac
done

BUILD_SCRIPT="${ROOT_DIR}/build.sh"
PORTABLE_PACK_SCRIPT="${ROOT_DIR}/tools/package-portable-sdk.sh"
SOURCE_CONTRACT="${ROOT_DIR}/bridge-contract.json"
DIST_ROOT="${ROOT_DIR}/dist/linux-x64"
OCCT_ROOT="${OCCT_ROOT:-/usr/local}"
OCCT_LIB_DIR="${OCCT_LIB_DIR:-${OCCT_ROOT}/lib}"

fail() { printf '[publish-linux] ERROR: %s\n' "$*" >&2; exit 1; }
log() { printf '[publish-linux] %s\n' "$*"; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }
json_number() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*([0-9]+).*/\\1/p" "$1" | head -n 1; }

current_branch() {
    local branch
    branch="$(git -C "${ROOT_DIR}" rev-parse --abbrev-ref HEAD)" || fail "Failed to resolve the current Git branch."
    [[ -n "${branch}" && "${branch}" != "HEAD" ]] || fail "publish.sh must run from a named branch, not detached HEAD."
    printf '%s\n' "${branch}"
}

worktree_changes() {
    git -C "${ROOT_DIR}" status --porcelain --untracked-files=all
}

assert_clean_worktree() {
    local stage="$1"
    [[ -z "$(worktree_changes)" ]] || fail "The working tree must be clean ${stage}. Review or commit changes through the normal PR workflow first."
}

assert_remote_branch_ancestor() {
    local branch="$1"
    local remote_ref="${REMOTE}/${branch}"

    git -C "${ROOT_DIR}" fetch --quiet "${REMOTE}" "${branch}" || fail "Failed to fetch ${remote_ref}."
    if git -C "${ROOT_DIR}" merge-base --is-ancestor "${remote_ref}" HEAD; then
        return
    fi

    local counts remote_only local_only
    counts="$(git -C "${ROOT_DIR}" rev-list --left-right --count "${remote_ref}...HEAD")" || fail "Failed to compare HEAD with ${remote_ref}."
    read -r remote_only local_only <<<"${counts}"
    fail "Local ${branch} is stale or diverged from ${remote_ref} (remote-only: ${remote_only:-?}, local-only: ${local_only:-?}). Synchronize ${branch} before publishing."
}

assert_binary_sdk() {
    local root="$1"
    local expected_source_commit="$2"
    local contract="${root}/bridge-contract.json"
    local manifest="${root}/bridge-manifest.json"
    local required name actual_hash expected_hash
    local required_files=(
        libOcctNative.so
        OcctNet.dll
        OcctNet.Avalonia.dll
        bridge-contract.json
        bridge-manifest.json
    )
    local hashed_files=(
        libOcctNative.so
        OcctNet.dll
        OcctNet.Avalonia.dll
        bridge-contract.json
    )

    for required in "${required_files[@]}"; do
        [[ -f "${root}/${required}" ]] || fail "Binary SDK file is missing from ${root}: ${required}"
    done

    [[ "$(json_number "${contract}" schemaVersion)" == "3" ]] || fail "Binary SDK contract must use schemaVersion 3."
    [[ "$(json_number "${contract}" current)" == "5" ]] || fail "Binary SDK contract current ABI must be 5."
    [[ "$(json_number "${contract}" minimumSupported)" == "5" ]] || fail "Binary SDK contract minimum supported ABI must be 5."
    [[ "$(json_string "${contract}" policy)" == "abi5-only" ]] || fail "Binary SDK contract API policy must remain abi5-only."
    [[ "$(json_string "${contract}" platform)" == "linux-x64" ]] || fail "Binary SDK contract platform must be linux-x64."

    ! grep -Fq '"nativeAbiVersion"' "${manifest}" || fail "Binary SDK manifest must not contain retired flat nativeAbiVersion metadata."
    [[ "$(json_number "${manifest}" schemaVersion)" == "2" ]] || fail "Binary SDK manifest must use schemaVersion 2."
    [[ "$(json_string "${manifest}" author)" == "$(json_string "${contract}" author)" ]] || fail "Binary SDK author differs between contract and manifest."
    [[ "$(json_string "${manifest}" bridgeVersion)" == "$(json_string "${contract}" bridgeVersion)" ]] || fail "Bridge version differs between contract and manifest."
    [[ "$(json_number "${manifest}" current)" == "$(json_number "${contract}" current)" ]] || fail "Current ABI differs between contract and manifest."
    [[ "$(json_number "${manifest}" minimumSupported)" == "$(json_number "${contract}" minimumSupported)" ]] || fail "Minimum supported ABI differs between contract and manifest."
    [[ "$(json_string "${manifest}" occtVersion)" == "$(json_string "${contract}" occtVersion)" ]] || fail "OCCT version differs between contract and manifest."
    [[ "$(json_string "${manifest}" platform)" == "linux-x64" ]] || fail "Binary SDK manifest platform must be linux-x64."
    [[ "$(json_string "${manifest}" targetFramework)" == "$(json_string "${contract}" targetFramework)" ]] || fail "Target framework differs between contract and manifest."
    [[ "$(json_string "${manifest}" sdkVersion)" == "$(json_string "${contract}" sdkVersion)" ]] || fail ".NET SDK version differs between contract and manifest."
    [[ "$(json_string "${manifest}" languageVersion)" == "$(json_string "${contract}" languageVersion)" ]] || fail "Language version differs between contract and manifest."
    [[ "$(json_string "${manifest}" configuration)" == "Release" ]] || fail "Linux Binary SDK manifest is not Release."
    [[ "$(json_string "${manifest}" sourceCommit)" == "${expected_source_commit}" ]] || fail "Linux Binary SDK sourceCommit does not match the publishing source commit."

    local manifest_entry_count
    manifest_entry_count="$(grep -c '^[[:space:]]*{ \"name\": ' "${manifest}" || true)"
    [[ "${manifest_entry_count}" == "${#hashed_files[@]}" ]] || fail "Binary SDK manifest contains an unexpected number of hashed files."

    for name in "${hashed_files[@]}"; do
        actual_hash="$(sha256sum "${root}/${name}" | awk '{print $1}')"
        expected_hash="$(sed -nE "s/.*\"name\": \"${name}\", \"sha256\": \"([^\"]+)\".*/\\1/p" "${manifest}" | head -n 1)"
        [[ -n "${expected_hash}" ]] || fail "Binary SDK manifest does not hash required file: ${name}"
        [[ "${actual_hash}" == "${expected_hash}" ]] || fail "Binary SDK hash mismatch: ${name}"
    done
}

assert_portable_sdk() {
    local root="$1"
    local expected_source_commit="$2"
    local expected_version="$3"

    [[ -f "${root}/package-manifest.json" ]] || fail "Portable SDK manifest is missing: ${root}/package-manifest.json"
    [[ -f "${root}/runtime/libOcctNative.so" ]] || fail "Portable SDK native bridge is missing: ${root}/runtime/libOcctNative.so"
    [[ -d "${root}/occt/resources" ]] || fail "Portable SDK OCCT resources are missing: ${root}/occt/resources"

    python3 - "${root}" "${expected_source_commit}" "${expected_version}" <<'PY' || fail "Portable SDK validation failed: ${root}"
import hashlib
import json
import os
import sys

root, expected_commit, expected_version = sys.argv[1:]
manifest_path = os.path.join(root, 'package-manifest.json')
with open(manifest_path, encoding='utf-8') as f:
    manifest = json.load(f)

if manifest.get('product') != 'OcctCSharpBridge Portable SDK':
    raise SystemExit('unexpected portable product')
if manifest.get('platform') != 'linux-x64' or not manifest.get('portableRuntime'):
    raise SystemExit('unexpected portable platform/runtime mode')
if manifest.get('bridgeSourceCommit') != expected_commit:
    raise SystemExit('portable sourceCommit does not match Binary SDK')
if manifest.get('bridgeVersion') != expected_version:
    raise SystemExit('portable Bridge version does not match Binary SDK')

for entry in manifest.get('files', []):
    rel = entry.get('name')
    expected_hash = str(entry.get('sha256', '')).lower()
    if not rel or not expected_hash:
        raise SystemExit('invalid portable manifest entry')
    path = os.path.join(root, *rel.split('/'))
    if not os.path.isfile(path):
        raise SystemExit(f'portable file missing: {rel}')
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b''):
            h.update(chunk)
    if h.hexdigest().lower() != expected_hash:
        raise SystemExit(f'portable hash mismatch: {rel}')
PY
}

install_complete_sdk() {
    local binary_source="$1"
    local portable_source="$2"
    local destination="$3"
    local expected_source_commit="$4"
    local expected_version="$5"
    local parent name staging backup had_previous=false

    assert_binary_sdk "${binary_source}" "${expected_source_commit}"
    assert_portable_sdk "${portable_source}" "${expected_source_commit}" "${expected_version}"

    parent="$(dirname "${destination}")"
    name="$(basename "${destination}")"
    staging="${parent}/.${name}-staging-$$"
    backup="${parent}/.${name}-backup-$$"

    mkdir -p "${parent}" || fail "Unable to create SDK install parent '${parent}'. Check directory permissions or set OCCTCSHARPBRIDGE_SDK/--install-root."
    rm -rf "${staging}" "${backup}"
    mkdir -p "${staging}/portable" || fail "Unable to create SDK staging directory: ${staging}"
    cp -a "${binary_source}/." "${staging}/" || fail "Unable to stage Binary SDK for installation."
    cp -a "${portable_source}/." "${staging}/portable/" || fail "Unable to stage Portable SDK for installation."

    assert_binary_sdk "${staging}" "${expected_source_commit}"
    assert_portable_sdk "${staging}/portable" "${expected_source_commit}" "${expected_version}"

    if [[ -e "${destination}" ]]; then
        mv "${destination}" "${backup}" || fail "Unable to back up existing SDK installation: ${destination}"
        had_previous=true
    fi

    if ! mv "${staging}" "${destination}"; then
        if [[ "${had_previous}" == true && -e "${backup}" ]]; then
            mv "${backup}" "${destination}" || true
        fi
        fail "Unable to install complete SDK to ${destination}."
    fi

    rm -rf "${backup}"
    assert_binary_sdk "${destination}" "${expected_source_commit}"
    assert_portable_sdk "${destination}/portable" "${expected_source_commit}" "${expected_version}"
    log "Installed complete SDK updated: ${destination}"
}

assert_only_dist_changes() {
    local status path
    while IFS= read -r status; do
        [[ -n "${status}" ]] || continue
        path="${status:3}"
        case "${path}" in
            dist/linux-x64/*) ;;
            *) fail "Publishing produced an unexpected worktree change outside dist/linux-x64: ${status}" ;;
        esac
    done < <(worktree_changes)
}

require_command git
require_command sha256sum
require_command python3
[[ "$(uname -s)" == "Linux" ]] || fail "publish.sh supports Linux only; use publish.ps1 on Windows."
case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
[[ -x "${BUILD_SCRIPT}" || -f "${BUILD_SCRIPT}" ]] || fail "build.sh was not found."
[[ -f "${PORTABLE_PACK_SCRIPT}" ]] || fail "Portable SDK packager was not found: ${PORTABLE_PACK_SCRIPT}"
[[ -f "${SOURCE_CONTRACT}" ]] || fail "Bridge contract was not found: ${SOURCE_CONTRACT}"

branch="$(current_branch)"
case "${branch}" in
    main) publish_mode="Formal" ;;
    main-dev) publish_mode="Development" ;;
    *) fail "publish.sh validates publishing from main or main-dev only. Current branch: ${branch}" ;;
esac
assert_clean_worktree "before publishing"
assert_remote_branch_ancestor "${branch}"
log "${publish_mode} ${branch} ancestry validated."

source_commit="$(git -C "${ROOT_DIR}" rev-parse HEAD)" || fail "Failed to resolve the source commit used for SDK publishing."
[[ -n "${source_commit}" ]] || fail "Failed to resolve the source commit used for SDK publishing."

bridge_version="$(json_string "${SOURCE_CONTRACT}" bridgeVersion)"
IFS='.' read -r bridge_major bridge_minor _ <<<"${bridge_version}"
[[ "${bridge_major}" =~ ^[0-9]+$ && "${bridge_minor}" =~ ^[0-9]+$ ]] || fail "Invalid Bridge version in bridge-contract.json: ${bridge_version}"
bridge_line="${bridge_major}.${bridge_minor}"
if [[ -z "${INSTALL_ROOT}" ]]; then
    [[ -n "${HOME:-}" ]] || fail "HOME is not set. Set OCCTCSHARPBRIDGE_SDK or use --install-root."
    INSTALL_ROOT="${HOME}/.local/share/OcctCSharpBridge/SDK/${bridge_line}/linux-x64"
fi

log "Building and validating the Release ABI5 Binary SDK..."
"${BUILD_SCRIPT}" dist Release
assert_binary_sdk "${DIST_ROOT}" "${source_commit}"
assert_only_dist_changes

log "Building portable SDK with the OCCT runtime closure..."
bash "${PORTABLE_PACK_SCRIPT}" "${DIST_ROOT}" "${OCCT_ROOT}" "${OCCT_LIB_DIR}" "${OUTPUT_ROOT}" "${CREATE_ARCHIVE}"
portable_package="${OUTPUT_ROOT}/OcctCSharpBridge-${bridge_version}-linux-x64-portable"
assert_portable_sdk "${portable_package}" "${source_commit}" "${bridge_version}"

log "Installing the complete SDK to ${INSTALL_ROOT}..."
install_complete_sdk "${DIST_ROOT}" "${portable_package}" "${INSTALL_ROOT}" "${source_commit}" "${bridge_version}"

log "Bridge Binary SDK and portable runtime SDK validated successfully."
log "Mode:          ${publish_mode}"
log "Branch:        ${branch}"
log "Source:        ${source_commit}"
log "Binary SDK:    ${DIST_ROOT}"
log "Installed SDK: ${INSTALL_ROOT}"
log "Runtime SDK:   ${INSTALL_ROOT}/portable"
log "Artifacts:     ${OUTPUT_ROOT}"
log "No Git push was performed. Publish the portable package through the normal reviewed artifact workflow."
