#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REMOTE="${1:-origin}"
BRANCH="$(git -C "${ROOT_DIR}" rev-parse --abbrev-ref HEAD)"
DIST="${ROOT_DIR}/dist/linux-x64"
CONTRACT="${DIST}/bridge-contract.json"
MANIFEST="${DIST}/bridge-manifest.json"

fail() { printf '[publish-linux] ERROR: %s\n' "$*" >&2; exit 1; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }
json_number() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*([0-9]+).*/\\1/p" "$1" | head -n 1; }

[[ "${BRANCH}" == "main" ]] || fail "publish.sh must run from main; current branch is ${BRANCH}."
[[ -z "$(git -C "${ROOT_DIR}" status --porcelain --untracked-files=all)" ]] || fail "The main working tree must be clean."
command -v sha256sum >/dev/null 2>&1 || fail "sha256sum was not found in PATH."

git -C "${ROOT_DIR}" fetch --quiet "${REMOTE}" main
git -C "${ROOT_DIR}" merge-base --is-ancestor "${REMOTE}/main" HEAD || fail "Local main is not based on the latest ${REMOTE}/main."
SOURCE_COMMIT="$(git -C "${ROOT_DIR}" rev-parse HEAD)"

"${ROOT_DIR}/build.sh" dist Release
for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json bridge-manifest.json; do
    [[ -f "${DIST}/${name}" ]] || fail "Binary SDK file is missing: ${name}"
done

[[ "$(json_string "${CONTRACT}" platform)" == "linux-x64" ]] || fail "Distribution contract platform must be linux-x64."
[[ "$(json_string "${MANIFEST}" platform)" == "linux-x64" ]] || fail "Distribution manifest platform must be linux-x64."
[[ "$(json_string "${MANIFEST}" bridgeVersion)" == "$(json_string "${CONTRACT}" bridgeVersion)" ]] || fail "Bridge version differs between contract and manifest."
[[ "$(json_number "${MANIFEST}" nativeAbiVersion)" == "$(json_number "${CONTRACT}" current)" ]] || fail "Native ABI differs between contract and manifest."
[[ "$(json_string "${MANIFEST}" occtVersion)" == "$(json_string "${CONTRACT}" occtVersion)" ]] || fail "OCCT version differs between contract and manifest."
[[ "$(json_string "${MANIFEST}" targetFramework)" == "$(json_string "${CONTRACT}" targetFramework)" ]] || fail "Target framework differs between contract and manifest."
[[ "$(json_string "${MANIFEST}" sdkVersion)" == "$(json_string "${CONTRACT}" sdkVersion)" ]] || fail ".NET SDK version differs between contract and manifest."
[[ "$(json_string "${MANIFEST}" languageVersion)" == "$(json_string "${CONTRACT}" languageVersion)" ]] || fail "Language version differs between contract and manifest."
[[ "$(json_string "${MANIFEST}" configuration)" == "Release" ]] || fail "Linux Binary SDK manifest is not Release."
[[ "$(json_string "${MANIFEST}" sourceCommit)" == "${SOURCE_COMMIT}" ]] || fail "Linux Binary SDK sourceCommit does not match the publishing source commit."

for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json; do
    hash="$(sha256sum "${DIST}/${name}" | awk '{print $1}')"
    grep -Fq "{ \"name\": \"${name}\", \"sha256\": \"${hash}\" }" "${MANIFEST}" || fail "Binary SDK hash mismatch or manifest entry missing: ${name}"
done

git -C "${ROOT_DIR}" add -f -- dist/linux-x64
if ! git -C "${ROOT_DIR}" diff --cached --quiet -- dist/linux-x64; then
    git -C "${ROOT_DIR}" commit -m "Publish Linux Binary SDK"
fi
git -C "${ROOT_DIR}" push "${REMOTE}" main
printf '[publish-linux] Published dist/linux-x64 from source %s.\n' "${SOURCE_COMMIT}"
