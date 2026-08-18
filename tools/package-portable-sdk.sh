#!/usr/bin/env bash
set -euo pipefail

SDK_ROOT="${1:?SDK root is required}"
OCCT_ROOT="${2:?OCCT root is required}"
OCCT_LIB_DIR="${3:?OCCT library directory is required}"
OUTPUT_ROOT="${4:?Output root is required}"
CREATE_ARCHIVE="${5:-true}"

fail() { printf '[portable-linux] ERROR: %s\n' "$*" >&2; exit 1; }
log() { printf '[portable-linux] %s\n' "$*"; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }
json_number() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*([0-9]+).*/\\1/p" "$1" | head -n 1; }

require_command ldd
require_command realpath
require_command sha256sum
require_command patchelf
[[ "${CREATE_ARCHIVE}" != true ]] || require_command tar

SDK_ROOT="$(realpath -m "${SDK_ROOT}")"
OCCT_ROOT="$(realpath -m "${OCCT_ROOT}")"
OCCT_LIB_DIR="$(realpath -m "${OCCT_LIB_DIR}")"
OUTPUT_ROOT="$(realpath -m "${OUTPUT_ROOT}")"
CONTRACT="${SDK_ROOT}/bridge-contract.json"
BRIDGE_MANIFEST="${SDK_ROOT}/bridge-manifest.json"
NATIVE_SOURCE="${SDK_ROOT}/libOcctNative.so"

[[ -f "${CONTRACT}" && -f "${BRIDGE_MANIFEST}" && -f "${NATIVE_SOURCE}" ]] || fail "Validated linux-x64 Binary SDK is incomplete."
[[ "$(json_string "${CONTRACT}" platform)" == "linux-x64" ]] || fail "Portable Linux SDK requires a linux-x64 contract."
[[ "$(json_string "${BRIDGE_MANIFEST}" configuration)" == "Release" ]] || fail "Portable Linux SDK requires a Release Binary SDK."

BRIDGE_VERSION="$(json_string "${CONTRACT}" bridgeVersion)"
OCCT_VERSION="$(json_string "${CONTRACT}" occtVersion)"
SOURCE_COMMIT="$(json_string "${BRIDGE_MANIFEST}" sourceCommit)"
CURRENT_ABI="$(json_number "${CONTRACT}" current)"
[[ -n "${BRIDGE_VERSION}" && -n "${SOURCE_COMMIT}" ]] || fail "Bridge contract/manifest metadata is incomplete."

PACKAGE_NAME="OcctCSharpBridge-${BRIDGE_VERSION}-linux-x64-portable"
PACKAGE_ROOT="${OUTPUT_ROOT}/${PACKAGE_NAME}"
RUNTIME_DIR="${PACKAGE_ROOT}/runtime"
RESOURCE_ROOT="${PACKAGE_ROOT}/occt/resources"
ARCHIVE_PATH="${OUTPUT_ROOT}/${PACKAGE_NAME}.tar.gz"

rm -rf "${PACKAGE_ROOT}"
mkdir -p "${PACKAGE_ROOT}" "${RUNTIME_DIR}" "${RESOURCE_ROOT}"

for name in OcctNet.dll OcctNet.Avalonia.dll bridge-contract.json bridge-manifest.json; do
    [[ -f "${SDK_ROOT}/${name}" ]] || fail "Binary SDK file is missing: ${name}"
    cp -f "${SDK_ROOT}/${name}" "${PACKAGE_ROOT}/${name}"
done
cp -f "${NATIVE_SOURCE}" "${RUNTIME_DIR}/libOcctNative.so"

declare -A queued=()
declare -A copied=()
queue=("${RUNTIME_DIR}/libOcctNative.so")
queued["libOcctNative.so"]=1

should_bundle_dependency() {
    local path="$1" name="$2"
    [[ "${name}" == libTK*.so* || "${name}" == libTKernel.so* ]] && return 0
    [[ "${name}" == libtbb*.so* || "${name}" == libfreeimage*.so* || "${name}" == libFreeImage*.so* ]] && return 0
    [[ "${path}" == "${OCCT_ROOT}"/* || "${path}" == "${OCCT_LIB_DIR}"/* ]] && return 0
    return 1
}

while [[ ${#queue[@]} -gt 0 ]]; do
    current="${queue[0]}"
    queue=("${queue[@]:1}")
    while IFS= read -r line; do
        dep_path=""
        dep_name=""
        if [[ "${line}" =~ ^[[:space:]]*([^[:space:]]+)[[:space:]]+\=\>[[:space:]]+(/[^[:space:]]+) ]]; then
            dep_name="${BASH_REMATCH[1]}"
            dep_path="${BASH_REMATCH[2]}"
        elif [[ "${line}" =~ ^[[:space:]]*(/[^[:space:]]+) ]]; then
            dep_path="${BASH_REMATCH[1]}"
            dep_name="$(basename "${dep_path}")"
        fi
        [[ -n "${dep_path}" && -f "${dep_path}" && -n "${dep_name}" ]] || continue
        should_bundle_dependency "${dep_path}" "${dep_name}" || continue
        if [[ -z "${copied[${dep_name}]:-}" ]]; then
            cp -Lf "${dep_path}" "${RUNTIME_DIR}/${dep_name}"
            copied["${dep_name}"]=1
            log "native: ${dep_name}"
        fi
        if [[ -z "${queued[${dep_name}]:-}" ]]; then
            queued["${dep_name}"]=1
            queue+=("${RUNTIME_DIR}/${dep_name}")
        fi
    done < <(ldd "${current}" 2>/dev/null || true)
done

# Every bundled ELF shared library resolves peer runtime modules from the same directory.
while IFS= read -r -d '' library; do
    patchelf --set-rpath '$ORIGIN' "${library}"
done < <(find "${RUNTIME_DIR}" -maxdepth 1 -type f -name '*.so*' -print0)

if LD_LIBRARY_PATH="${RUNTIME_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}" ldd "${RUNTIME_DIR}/libOcctNative.so" | grep -q 'not found'; then
    LD_LIBRARY_PATH="${RUNTIME_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}" ldd "${RUNTIME_DIR}/libOcctNative.so" >&2 || true
    fail "The packaged native Bridge still has unresolved shared-library dependencies."
fi

copy_resource_dir() {
    local name="$1" source=""
    for candidate in \
        "${OCCT_ROOT}/share/opencascade/resources/${name}" \
        "${OCCT_ROOT}/share/opencascade/${name}" \
        "${OCCT_ROOT}/src/${name}"; do
        if [[ -d "${candidate}" ]]; then source="${candidate}"; break; fi
    done
    if [[ -n "${source}" ]]; then
        rm -rf "${RESOURCE_ROOT}/${name}"
        cp -a "${source}" "${RESOURCE_ROOT}/${name}"
        log "resource: ${name}"
    fi
}

for resource in SHMessage XSMessage XSTEPResource XCAFResources StdResource Textures Shaders UnitsAPI; do
    copy_resource_dir "${resource}"
done

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
for notice in LICENSE LICENSE_LGPL_21.txt OcctCSharpBridge_LGPL_EXCEPTION.txt THIRD_PARTY_NOTICES.md COMMERCIAL.md; do
    [[ -f "${REPO_ROOT}/${notice}" ]] && cp -f "${REPO_ROOT}/${notice}" "${PACKAGE_ROOT}/${notice}"
done

cat > "${PACKAGE_ROOT}/PORTABLE-SDK.txt" <<'EOF'
OcctCSharpBridge Portable SDK - Linux x64

This package contains the Bridge Binary SDK plus the OCCT runtime closure used to build it.
It does not bundle the .NET runtime or system libraries such as glibc, libGL, X11/Wayland components.

Recommended application layout:
1. Copy the managed DLLs, runtime/ and occt/ directories beside the application executable.
2. Reference the required OcctNet*.dll assemblies from the application.
3. Call OcctRuntime.Configure() before creating the first OcctEngine or OcctModelingSession.

The bundled native libraries use $ORIGIN so peer OCCT libraries are resolved from <app>/runtime.
OcctRuntime automatically probes <app>/runtime and <app>/occt.
EOF

MANIFEST_PATH="${PACKAGE_ROOT}/package-manifest.json"
python3_available=false
if command -v python3 >/dev/null 2>&1; then python3_available=true; fi
[[ "${python3_available}" == true ]] || fail "python3 is required to write the portable package manifest."

python3 - "${PACKAGE_ROOT}" "${MANIFEST_PATH}" "${BRIDGE_VERSION}" "${SOURCE_COMMIT}" "${CURRENT_ABI}" "${OCCT_VERSION}" <<'PY'
import hashlib
import json
import os
import sys

root, manifest_path, bridge_version, source_commit, abi, occt_version = sys.argv[1:]
files = []
for base, _, names in os.walk(root):
    for name in sorted(names):
        path = os.path.join(base, name)
        if os.path.abspath(path) == os.path.abspath(manifest_path):
            continue
        rel = os.path.relpath(path, root).replace(os.sep, '/')
        h = hashlib.sha256()
        with open(path, 'rb') as f:
            for chunk in iter(lambda: f.read(1024 * 1024), b''):
                h.update(chunk)
        files.append({'name': rel, 'size': os.path.getsize(path), 'sha256': h.hexdigest()})
manifest = {
    'schemaVersion': 1,
    'product': 'OcctCSharpBridge Portable SDK',
    'bridgeVersion': bridge_version,
    'bridgeSourceCommit': source_commit,
    'nativeAbi': int(abi),
    'occtVersion': occt_version,
    'platform': 'linux-x64',
    'configuration': 'Release',
    'portableRuntime': True,
    'nativeDirectory': 'runtime',
    'occtRoot': 'occt',
    'dotnetRuntimeBundled': False,
    'files': sorted(files, key=lambda x: x['name']),
}
with open(manifest_path, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(manifest, f, ensure_ascii=False, indent=2)
    f.write('\n')
PY

if [[ "${CREATE_ARCHIVE}" == true ]]; then
    rm -f "${ARCHIVE_PATH}"
    tar -C "${OUTPUT_ROOT}" -czf "${ARCHIVE_PATH}" "${PACKAGE_NAME}"
    log "Archive: ${ARCHIVE_PATH}"
fi

log "Portable SDK: ${PACKAGE_ROOT}"
