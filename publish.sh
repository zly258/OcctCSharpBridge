#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REMOTE="${1:-origin}"
BUILD_SCRIPT="${ROOT_DIR}/build.sh"
DIST_ROOT="${ROOT_DIR}/dist/linux-x64"
CONTRACT="${DIST_ROOT}/bridge-contract.json"
MANIFEST="${DIST_ROOT}/bridge-manifest.json"

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

assert_remote_main_ancestor() {
    git -C "${ROOT_DIR}" fetch --quiet "${REMOTE}" main || fail "Failed to fetch ${REMOTE}/main."
    if git -C "${ROOT_DIR}" merge-base --is-ancestor "${REMOTE}/main" HEAD; then
        return
    fi

    local counts remote_only local_only
    counts="$(git -C "${ROOT_DIR}" rev-list --left-right --count "${REMOTE}/main...HEAD")" || fail "Failed to compare HEAD with ${REMOTE}/main."
    read -r remote_only local_only <<<"${counts}"
    fail "Local main is stale or diverged from ${REMOTE}/main (remote-only: ${remote_only:-?}, local-only: ${local_only:-?}). Synchronize main before publishing."
}

assert_binary_sdk() {
    local expected_source_commit="$1"
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
        [[ -f "${DIST_ROOT}/${required}" ]] || fail "Binary SDK file is missing: ${required}"
    done

    [[ "$(json_number "${CONTRACT}" schemaVersion)" == "3" ]] || fail "Binary SDK contract must use schemaVersion 3."
    [[ "$(json_number "${CONTRACT}" current)" == "5" ]] || fail "Binary SDK contract current ABI must be 5."
    [[ "$(json_number "${CONTRACT}" minimumSupported)" == "5" ]] || fail "Binary SDK contract minimum supported ABI must be 5."
    [[ "$(json_string "${CONTRACT}" policy)" == "abi5-only" ]] || fail "Binary SDK contract API policy must remain abi5-only."
    [[ "$(json_string "${CONTRACT}" platform)" == "linux-x64" ]] || fail "Binary SDK contract platform must be linux-x64."

    ! grep -Fq '"nativeAbiVersion"' "${MANIFEST}" || fail "Binary SDK manifest must not contain retired flat nativeAbiVersion metadata."
    [[ "$(json_number "${MANIFEST}" schemaVersion)" == "2" ]] || fail "Binary SDK manifest must use schemaVersion 2."
    [[ "$(json_string "${MANIFEST}" author)" == "$(json_string "${CONTRACT}" author)" ]] || fail "Binary SDK author differs between contract and manifest."
    [[ "$(json_string "${MANIFEST}" bridgeVersion)" == "$(json_string "${CONTRACT}" bridgeVersion)" ]] || fail "Bridge version differs between contract and manifest."
    [[ "$(json_number "${MANIFEST}" current)" == "$(json_number "${CONTRACT}" current)" ]] || fail "Current ABI differs between contract and manifest."
    [[ "$(json_number "${MANIFEST}" minimumSupported)" == "$(json_number "${CONTRACT}" minimumSupported)" ]] || fail "Minimum supported ABI differs between contract and manifest."
    [[ "$(json_string "${MANIFEST}" occtVersion)" == "$(json_string "${CONTRACT}" occtVersion)" ]] || fail "OCCT version differs between contract and manifest."
    [[ "$(json_string "${MANIFEST}" platform)" == "linux-x64" ]] || fail "Binary SDK manifest platform must be linux-x64."
    [[ "$(json_string "${MANIFEST}" targetFramework)" == "$(json_string "${CONTRACT}" targetFramework)" ]] || fail "Target framework differs between contract and manifest."
    [[ "$(json_string "${MANIFEST}" sdkVersion)" == "$(json_string "${CONTRACT}" sdkVersion)" ]] || fail ".NET SDK baseline differs between contract and manifest."
    [[ "$(json_string "${MANIFEST}" sdkRollForward)" == "$(json_string "${CONTRACT}" sdkRollForward)" ]] || fail ".NET SDK roll-forward policy differs between contract and manifest."
    [[ "$(json_string "${MANIFEST}" languageVersion)" == "$(json_string "${CONTRACT}" languageVersion)" ]] || fail "Language version differs between contract and manifest."
    [[ "$(json_string "${MANIFEST}" configuration)" == "Release" ]] || fail "Linux Binary SDK manifest is not Release."
    [[ "$(json_string "${MANIFEST}" sourceCommit)" == "${expected_source_commit}" ]] || fail "Linux Binary SDK sourceCommit does not match the publishing source commit."

    local manifest_entry_count
    manifest_entry_count="$(grep -c '^[[:space:]]*{ "name": ' "${MANIFEST}" || true)"
    [[ "${manifest_entry_count}" == "${#hashed_files[@]}" ]] || fail "Binary SDK manifest contains an unexpected number of hashed files."

    for name in "${hashed_files[@]}"; do
        actual_hash="$(sha256sum "${DIST_ROOT}/${name}" | awk '{print $1}')"
        expected_hash="$(sed -nE "s/.*\"name\": \"${name}\", \"sha256\": \"([^\"]+)\".*/\\1/p" "${MANIFEST}" | head -n 1)"
        [[ -n "${expected_hash}" ]] || fail "Binary SDK manifest does not hash required file: ${name}"
        [[ "${actual_hash}" == "${expected_hash}" ]] || fail "Binary SDK hash mismatch: ${name}"
    done
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
[[ "$(uname -s)" == "Linux" ]] || fail "publish.sh supports Linux only; use publish.ps1 on Windows."
case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
[[ -x "${BUILD_SCRIPT}" || -f "${BUILD_SCRIPT}" ]] || fail "build.sh was not found."

branch="$(current_branch)"
[[ "${branch}" == "main" ]] || fail "publish.sh validates formal publishing from main only. Current branch: ${branch}"
assert_clean_worktree "before publishing"
assert_remote_main_ancestor
log "Formal main ancestry validated."

source_commit="$(git -C "${ROOT_DIR}" rev-parse HEAD)" || fail "Failed to resolve the source commit used for Binary SDK publishing."
[[ -n "${source_commit}" ]] || fail "Failed to resolve the source commit used for Binary SDK publishing."

log "Building and validating the Release ABI5 Binary SDK..."
"${BUILD_SCRIPT}" dist Release
assert_binary_sdk "${source_commit}"
assert_only_dist_changes

log "Bridge Binary SDK validated successfully."
log "Source: ${source_commit}"
log "Output: ${DIST_ROOT}"
log "No Git commit or push was performed. Review the generated dist payload and publish it through the normal reviewed workflow."
