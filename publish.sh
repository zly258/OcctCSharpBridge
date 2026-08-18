#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIGURATION="Release"
OUTPUT_ROOT="${ROOT_DIR}/artifacts/publish"
SELF_CONTAINED=true
CREATE_ARCHIVE=true

usage() {
    cat <<'EOF'
Usage: ./publish.sh [Debug|Release|RelWithDebInfo] [options]

Options:
  --framework-dependent   Do not bundle the .NET runtime
  --self-contained        Bundle the .NET runtime (default)
  --output <directory>    Publish root (default: artifacts/publish)
  --no-archive            Do not create the .tar.gz package
  -h, --help              Show this help
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        Debug|Release|RelWithDebInfo) CONFIGURATION="$1"; shift ;;
        --framework-dependent) SELF_CONTAINED=false; shift ;;
        --self-contained) SELF_CONTAINED=true; shift ;;
        --output) [[ $# -ge 2 ]] || { echo "Missing value for --output" >&2; exit 2; }; OUTPUT_ROOT="$2"; shift 2 ;;
        --no-archive) CREATE_ARCHIVE=false; shift ;;
        -h|--help) usage; exit 0 ;;
        *) echo "Unknown argument: $1" >&2; usage; exit 2 ;;
    esac
done

fail() { printf '[publish-linux] ERROR: %s\n' "$*" >&2; exit 1; }
log() { printf '[publish-linux] %s\n' "$*"; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "Required command was not found: $1"; }
json_string() { sed -nE "s/^[[:space:]]*\"$2\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/p" "$1" | head -n 1; }

[[ "$(uname -s)" == "Linux" ]] || fail "publish.sh supports Linux only; use publish.ps1 on Windows."
case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
require_command dotnet
require_command realpath
require_command tar
require_command sha256sum
require_command python3

DIST_ROOT="${ROOT_DIR}/dist/linux-x64"
PORTABLE_ROOT="${ROOT_DIR}/dist/portable/linux-x64"
PROJECT="${ROOT_DIR}/src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
CONTRACT="${DIST_ROOT}/bridge-contract.json"
MANIFEST="${DIST_ROOT}/bridge-manifest.json"
PORTABLE_MANIFEST="${PORTABLE_ROOT}/package-manifest.json"
PACKAGE_NAME="CAD-Avalonia-linux-x64"
mkdir -p "${OUTPUT_ROOT}"
PACKAGE_DIR="$(realpath -m "${OUTPUT_ROOT}/${PACKAGE_NAME}")"
STAGING_DIR="$(realpath -m "${OUTPUT_ROOT}/.${PACKAGE_NAME}-staging-$$")"
ARCHIVE_PATH="$(realpath -m "${OUTPUT_ROOT}/${PACKAGE_NAME}.tar.gz")"

[[ -f "${PROJECT}" ]] || fail "Avalonia Demo project was not found."
[[ -f "${CONTRACT}" && -f "${MANIFEST}" ]] || fail "linux-x64 Binary SDK is missing. Run ./sync.sh first."
[[ -f "${PORTABLE_MANIFEST}" && -f "${PORTABLE_ROOT}/runtime/libOcctNative.so" && -d "${PORTABLE_ROOT}/occt/resources" ]] || fail "Matching Bridge portable runtime is missing. Run ./sync.sh first."

BRIDGE_COMMIT="$(json_string "${MANIFEST}" sourceCommit)"
BRIDGE_VERSION="$(json_string "${CONTRACT}" bridgeVersion)"
[[ -n "${BRIDGE_COMMIT}" && -n "${BRIDGE_VERSION}" ]] || fail "Bridge Binary SDK metadata is incomplete."

python3 - "${PORTABLE_ROOT}" "${BRIDGE_COMMIT}" "${BRIDGE_VERSION}" <<'PY'
import hashlib, json, os, sys
root, expected_commit, expected_version = sys.argv[1:]
try:
    with open(os.path.join(root, 'package-manifest.json'), encoding='utf-8') as f:
        m = json.load(f)
    if m.get('product') != 'OcctCSharpBridge Portable SDK': raise SystemExit(1)
    if m.get('platform') != 'linux-x64' or not m.get('portableRuntime'): raise SystemExit(1)
    if m.get('bridgeSourceCommit') != expected_commit or m.get('bridgeVersion') != expected_version: raise SystemExit(1)
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
[[ $? -eq 0 ]] || fail "Bridge portable runtime does not match the synchronized Binary SDK. Run ./sync.sh again."

"${ROOT_DIR}/build.sh" validate "${CONFIGURATION}"
"${ROOT_DIR}/build.sh" avalonia "${CONFIGURATION}"

rm -rf "${PACKAGE_DIR}" "${STAGING_DIR}"
mkdir -p "${PACKAGE_DIR}" "${STAGING_DIR}"

publish_args=(
    publish "${PROJECT}"
    -c "${CONFIGURATION}"
    -r linux-x64
    -p:Platform=x64
    --nologo
    -o "${STAGING_DIR}"
)
if [[ "${SELF_CONTAINED}" == true ]]; then publish_args+=(--self-contained true); else publish_args+=(--self-contained false); fi

dotnet "${publish_args[@]}"
cp -a "${STAGING_DIR}/." "${PACKAGE_DIR}/"

# The project build can copy the minimal native Bridge beside the executable. Remove it so
# OcctRuntime resolves the validated portable closure from <app>/runtime.
rm -f "${PACKAGE_DIR}/libOcctNative.so"

# Verify the managed Bridge assemblies emitted by dotnet publish are exactly those synchronized from the same Bridge revision.
for name in OcctNet.dll OcctNet.Avalonia.dll; do
    [[ -f "${PACKAGE_DIR}/${name}" ]] || continue
    [[ -f "${PORTABLE_ROOT}/${name}" ]] || fail "Bridge portable SDK is missing ${name}."
    [[ "$(sha256sum "${PACKAGE_DIR}/${name}" | awk '{print $1}')" == "$(sha256sum "${PORTABLE_ROOT}/${name}" | awk '{print $1}')" ]] || fail "Published Demo assembly differs from synchronized Bridge portable SDK: ${name}"
done

cp -a "${PORTABLE_ROOT}/runtime" "${PACKAGE_DIR}/runtime"
cp -a "${PORTABLE_ROOT}/occt" "${PACKAGE_DIR}/occt"
cp -f "${CONTRACT}" "${PACKAGE_DIR}/bridge-contract.json"
cp -f "${MANIFEST}" "${PACKAGE_DIR}/bridge-manifest.json"
cp -f "${PORTABLE_MANIFEST}" "${PACKAGE_DIR}/bridge-portable-manifest.json"
for notice in LICENSE LICENSE_LGPL_21.txt OcctCSharpBridge_LGPL_EXCEPTION.txt THIRD_PARTY_NOTICES.md COMMERCIAL.md; do
    [[ -f "${PORTABLE_ROOT}/${notice}" ]] && cp -f "${PORTABLE_ROOT}/${notice}" "${PACKAGE_DIR}/${notice}"
done
[[ -f "${PORTABLE_ROOT}/PORTABLE-SDK.txt" ]] && cp -f "${PORTABLE_ROOT}/PORTABLE-SDK.txt" "${PACKAGE_DIR}/BRIDGE-PORTABLE-SDK.txt"

cat > "${PACKAGE_DIR}/run.sh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
APP_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NATIVE_DIR="${APP_DIR}/runtime"
RES_DIR="${APP_DIR}/occt/resources"
export OCCT_BRIDGE_NATIVE_DIR="${NATIVE_DIR}"
export OCCT_ROOT="${APP_DIR}/occt"
export CASROOT="${APP_DIR}/occt"
export LD_LIBRARY_PATH="${NATIVE_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}"
[[ -d "${RES_DIR}/SHMessage" ]] && export CSF_SHMessage="${RES_DIR}/SHMessage"
[[ -d "${RES_DIR}/XSMessage" ]] && export CSF_XSMessage="${RES_DIR}/XSMessage"
[[ -d "${RES_DIR}/StdResource" ]] && export CSF_StandardDefaults="${RES_DIR}/StdResource"
[[ -d "${RES_DIR}/XSTEPResource" ]] && export CSF_STEPDefaults="${RES_DIR}/XSTEPResource" && export CSF_IGESDefaults="${RES_DIR}/XSTEPResource"
[[ -d "${RES_DIR}/XCAFResources" ]] && export CSF_XCAFDefaults="${RES_DIR}/XCAFResources" && export CSF_PluginDefaults="${RES_DIR}/XCAFResources"
[[ -d "${RES_DIR}/Shaders" ]] && export CSF_ShadersDirectory="${RES_DIR}/Shaders"
[[ -d "${RES_DIR}/Textures" ]] && export CSF_MDTVTexturesDirectory="${RES_DIR}/Textures"
if [[ -x "${APP_DIR}/CAD-Avalonia" ]]; then exec "${APP_DIR}/CAD-Avalonia" "$@"; fi
exec dotnet "${APP_DIR}/CAD-Avalonia.dll" "$@"
EOF
chmod +x "${PACKAGE_DIR}/run.sh"
[[ -f "${PACKAGE_DIR}/CAD-Avalonia" ]] && chmod +x "${PACKAGE_DIR}/CAD-Avalonia"

python3 - "${PACKAGE_DIR}" "${BRIDGE_VERSION}" "${BRIDGE_COMMIT}" "${CONFIGURATION}" "${SELF_CONTAINED}" <<'PY'
import hashlib, json, os, sys
root, version, commit, config, self_contained = sys.argv[1:]
manifest_path = os.path.join(root, 'package-manifest.json')
files = []
for base, _, names in os.walk(root):
    for name in sorted(names):
        path = os.path.join(base, name)
        if os.path.abspath(path) == os.path.abspath(manifest_path): continue
        rel = os.path.relpath(path, root).replace(os.sep, '/')
        h = hashlib.sha256()
        with open(path, 'rb') as f:
            for chunk in iter(lambda: f.read(1024 * 1024), b''): h.update(chunk)
        files.append({'name': rel, 'size': os.path.getsize(path), 'sha256': h.hexdigest()})
data = {
    'schemaVersion': 2,
    'product': 'OcctCSharpBridge Demo',
    'apps': ['Avalonia'],
    'bridgeVersion': version,
    'bridgeSourceCommit': commit,
    'bridgePortableRuntime': True,
    'bridgePortableManifest': 'bridge-portable-manifest.json',
    'nativeDirectory': 'runtime',
    'occtRoot': 'occt',
    'platform': 'linux-x64',
    'configuration': config,
    'selfContained': self_contained.lower() == 'true',
    'files': sorted(files, key=lambda x: x['name']),
}
with open(manifest_path, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
    f.write('\n')
PY

{
    echo "CAD-Avalonia Linux x64"
    echo "Configuration: ${CONFIGURATION}"
    echo "Self-contained: ${SELF_CONTAINED}"
    echo "Bridge: ${BRIDGE_VERSION}"
    echo "Bridge source: ${BRIDGE_COMMIT}"
    echo "Portable runtime: reused from dist/portable/linux-x64"
    echo
    echo "SHA256:"
    (cd "${PACKAGE_DIR}" && find . -type f -print0 | sort -z | xargs -0 sha256sum)
} > "${PACKAGE_DIR}/publish-manifest.txt"

rm -rf "${STAGING_DIR}"
if [[ "${CREATE_ARCHIVE}" == true ]]; then
    rm -f "${ARCHIVE_PATH}"
    tar -C "${OUTPUT_ROOT}" -czf "${ARCHIVE_PATH}" "${PACKAGE_NAME}"
    log "Archive: ${ARCHIVE_PATH}"
fi

log "Bridge portable runtime reused: ${BRIDGE_COMMIT}"
log "Package: ${PACKAGE_DIR}"
log "Run:     ${PACKAGE_DIR}/run.sh"
