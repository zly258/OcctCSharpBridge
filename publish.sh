#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REMOTE="${1:-origin}"
BRANCH="$(git -C "${ROOT_DIR}" rev-parse --abbrev-ref HEAD)"
DIST="${ROOT_DIR}/dist/linux-x64"

fail() { printf '[publish-linux] ERROR: %s\n' "$*" >&2; exit 1; }
[[ "${BRANCH}" == "main" ]] || fail "publish.sh must run from main; current branch is ${BRANCH}."
[[ -z "$(git -C "${ROOT_DIR}" status --porcelain --untracked-files=all)" ]] || fail "The main working tree must be clean."

git -C "${ROOT_DIR}" fetch --quiet "${REMOTE}" main
git -C "${ROOT_DIR}" merge-base --is-ancestor "${REMOTE}/main" HEAD || fail "Local main is not based on the latest ${REMOTE}/main."

"${ROOT_DIR}/build.sh" dist Release
for name in libOcctNative.so OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json bridge-manifest.json; do
    [[ -f "${DIST}/${name}" ]] || fail "Binary SDK file is missing: ${name}"
done

git -C "${ROOT_DIR}" add -f -- dist/linux-x64
if ! git -C "${ROOT_DIR}" diff --cached --quiet -- dist/linux-x64; then
    git -C "${ROOT_DIR}" commit -m "Publish Linux Binary SDK"
fi
git -C "${ROOT_DIR}" push "${REMOTE}" main
printf '[publish-linux] Published dist/linux-x64 from %s.\n' "$(git -C "${ROOT_DIR}" rev-parse HEAD)"
