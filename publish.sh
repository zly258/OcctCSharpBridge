#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIGURATION="Release"
OUTPUT_ROOT="${ROOT_DIR}/artifacts/publish"
SELF_CONTAINED=true
CREATE_ARCHIVE=true
DEFAULT_SDK_ROOT="${HOME:-}/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64"
DIST_ROOT="${OCCTCSHARPBRIDGE_SDK:-${DEFAULT_SDK_ROOT}}"
PORTABLE_ROOT="${DIST_ROOT}/portable"

export OCCTCSHARPBRIDGE_SDK="${DIST_ROOT}"

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
[[ -n "${OCCTCSHARPBRIDGE_SDK:-}" ]] || fail "HOME is not set. Set OCCTCSHARPBRIDGE_SDK explicitly."
require_command dotnet
require_command realpath
require_command tar
require_command python3

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
[[ -f "${CONTRACT}" && -f "${MANIFEST}" ]] || fail "Installed linux-x64 SDK is missing at '${DIST_ROOT}'. Run Bridge main ./publish.sh as the current user, or set OCCTCSHARPBRIDGE_SDK."
[[ -f "${PORTABLE_MANIFEST}" && -f "${PORTABLE_ROOT}/runtime/libOcctNative.so" && -d "${PORTABLE_ROOT}/occt/resources" ]] || fail "Installed Linux SDK is incomplete: portable runtime is missing under '${PORTABLE_ROOT}'. Re-run Bridge main ./publish.sh."

BRIDGE_COMMIT="$(json_string "${MANIFEST}" sourceCommit)"
BRIDGE_VERSION="$(json_string "${CONTRACT}" bridgeVersion)"
[[ -n "${BRIDGE_COMMIT}" && -n "${BRIDGE_VERSION}" ]] || fail "Bridge SDK metadata is incomplete."

python3 - "${PORTABLE_ROOT}" "${BRIDGE_COMMIT}" "${BRIDGE_VERSION}" <<'PY' || exit $?
import hashlib, json, os, sys
root, expected_commit, expected_version = sys.argv[1:]
with open(os.path.join(root, 'package-manifest.json'), encoding='utf-8') as f:
    m = json.load(f)
if m.get('product') != 'OcctCSharpBridge Portable SDK': raise SystemExit('unexpected Bridge portable product')
if m.get('platform') != 'linux-x64' or not m.get('portableRuntime'): raise SystemExit('unexpected Bridge portable platform/runtime mode')
if m.get('bridgeSourceCommit') != expected_commit or m.get('bridgeVersion') != expected_version:
    raise SystemExit('Installed Bridge portable runtime does not match the Binary SDK')
for entry in m.get('files', []):
    path = os.path.join(root, *entry['name'].split('/'))
    if not os.path.isfile(path): raise SystemExit(f"portable file missing: {entry['name']}")
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b''): h.update(chunk)
    if h.hexdigest().lower() != str(entry['sha256']).lower():
        raise SystemExit(f"portable hash mismatch: {entry['name']}")
PY

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
rm -f "${PACKAGE_DIR}/libOcctNative.so"

# Merge the complete validated Bridge Portable SDK payload installed by Bridge main.
python3 - "${PORTABLE_ROOT}" "${PACKAGE_DIR}" <<'PY'
import hashlib, os, shutil, sys
src, dst = sys.argv[1:]

def digest(path):
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b''): h.update(chunk)
    return h.hexdigest()

for base, _, names in os.walk(src):
    for name in names:
        source = os.path.join(base, name)
        rel = os.path.relpath(source, src)
        target_rel = 'bridge-portable-manifest.json' if rel == 'package-manifest.json' else rel
        target = os.path.join(dst, target_rel)
        os.makedirs(os.path.dirname(target) or dst, exist_ok=True)
        if os.path.isfile(target):
            if digest(source) != digest(target):
                raise SystemExit(f"Demo publish conflicts with installed Bridge portable payload: {target_rel}")
            continue
        shutil.copy2(source, target)
PY

[[ -f "${PACKAGE_DIR}/runtime/libOcctNative.so" ]] || fail "Merged Demo package is missing runtime/libOcctNative.so."
[[ -f "${PACKAGE_DIR}/bridge-portable-manifest.json" ]] || fail "Merged Demo package is missing bridge-portable-manifest.json."

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

rm -rf "${STAGING_DIR}"
if [[ "${CREATE_ARCHIVE}" == true ]]; then
    rm -f "${ARCHIVE_PATH}"
    tar -C "${OUTPUT_ROOT}" -czf "${ARCHIVE_PATH}" "${PACKAGE_NAME}"
    log "Archive: ${ARCHIVE_PATH}"
fi

log "Bridge SDK:     ${DIST_ROOT}"
log "Runtime SDK:    ${PORTABLE_ROOT}"
log "Bridge source:  ${BRIDGE_COMMIT}"
log "Package:        ${PACKAGE_DIR}"
log "Run:            ${PACKAGE_DIR}/run.sh"
