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

[[ "$(uname -s)" == "Linux" ]] || fail "publish.sh supports Linux only; use publish.ps1 on Windows."
case "$(uname -m)" in x86_64|amd64) ;; *) fail "Linux x64 is required; detected $(uname -m)." ;; esac
require_command dotnet
require_command ldd
require_command realpath
require_command tar
require_command sha256sum

DIST_ROOT="${ROOT_DIR}/dist/linux-x64"
PROJECT="${ROOT_DIR}/src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
NATIVE_SOURCE="${DIST_ROOT}/libOcctNative.so"
CONTRACT="${DIST_ROOT}/bridge-contract.json"
MANIFEST="${DIST_ROOT}/bridge-manifest.json"
OCCT_ROOT="${OCCT_ROOT:-/usr/local}"
OCCT_LIB_DIR="${OCCT_LIB_DIR:-${OCCT_ROOT}/lib}"
PACKAGE_NAME="CAD-Avalonia-linux-x64"
mkdir -p "${OUTPUT_ROOT}"
PACKAGE_DIR="$(realpath -m "${OUTPUT_ROOT}/${PACKAGE_NAME}")"
STAGING_DIR="$(realpath -m "${OUTPUT_ROOT}/.${PACKAGE_NAME}-staging-$$")"
ARCHIVE_PATH="$(realpath -m "${OUTPUT_ROOT}/${PACKAGE_NAME}.tar.gz")"

[[ -f "${PROJECT}" ]] || fail "Avalonia Demo project was not found."
[[ -f "${NATIVE_SOURCE}" && -f "${CONTRACT}" && -f "${MANIFEST}" ]] || fail "linux-x64 Binary SDK is missing. Run ./sync.sh first."

"${ROOT_DIR}/build.sh" validate "${CONFIGURATION}"
"${ROOT_DIR}/build.sh" avalonia "${CONFIGURATION}"

rm -rf "${PACKAGE_DIR}" "${STAGING_DIR}"
mkdir -p "${PACKAGE_DIR}" "${STAGING_DIR}" "${PACKAGE_DIR}/native" "${PACKAGE_DIR}/occt/resources"

publish_args=(
    publish "${PROJECT}"
    -c "${CONFIGURATION}"
    -r linux-x64
    -p:Platform=x64
    --nologo
    -o "${STAGING_DIR}"
)
if [[ "${SELF_CONTAINED}" == true ]]; then
    publish_args+=(--self-contained true)
else
    publish_args+=(--self-contained false)
fi

dotnet "${publish_args[@]}"
cp -a "${STAGING_DIR}/." "${PACKAGE_DIR}/"
cp -f "${NATIVE_SOURCE}" "${PACKAGE_DIR}/native/libOcctNative.so"
cp -f "${CONTRACT}" "${PACKAGE_DIR}/bridge-contract.json"
cp -f "${MANIFEST}" "${PACKAGE_DIR}/bridge-manifest.json"
for notice in LICENSE LICENSE_LGPL_21.txt OcctCSharpBridge_LGPL_EXCEPTION.txt THIRD_PARTY_NOTICES.md COMMERCIAL.md; do
    [[ -f "${ROOT_DIR}/${notice}" ]] && cp -f "${ROOT_DIR}/${notice}" "${PACKAGE_DIR}/${notice}"
done

# Bundle OCCT and non-system runtime dependencies required by the Bridge.
declare -A queued=()
declare -A copied=()
queue=("${PACKAGE_DIR}/native/libOcctNative.so")
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
        if [[ "${line}" =~ =\>[[:space:]]+(/[^[:space:]]+) ]]; then
            dep_path="${BASH_REMATCH[1]}"
        elif [[ "${line}" =~ ^[[:space:]]*(/[^[:space:]]+) ]]; then
            dep_path="${BASH_REMATCH[1]}"
        fi
        [[ -n "${dep_path}" && -f "${dep_path}" ]] || continue
        dep_name="$(basename "${dep_path}")"
        should_bundle_dependency "${dep_path}" "${dep_name}" || continue
        if [[ -z "${copied[${dep_name}]:-}" ]]; then
            cp -Lf "${dep_path}" "${PACKAGE_DIR}/native/${dep_name}"
            copied["${dep_name}"]=1
            log "native: ${dep_name}"
        fi
        if [[ -z "${queued[${dep_name}]:-}" ]]; then
            queued["${dep_name}"]=1
            queue+=("${PACKAGE_DIR}/native/${dep_name}")
        fi
    done < <(ldd "${current}" 2>/dev/null || true)
done

if LD_LIBRARY_PATH="${PACKAGE_DIR}/native${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}" ldd "${PACKAGE_DIR}/native/libOcctNative.so" | grep -q 'not found'; then
    LD_LIBRARY_PATH="${PACKAGE_DIR}/native${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}" ldd "${PACKAGE_DIR}/native/libOcctNative.so" >&2 || true
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
        rm -rf "${PACKAGE_DIR}/occt/resources/${name}"
        cp -a "${source}" "${PACKAGE_DIR}/occt/resources/${name}"
        log "resource: ${name}"
    fi
}

for resource in SHMessage XSMessage XSTEPResource XCAFResources StdResource Textures; do
    copy_resource_dir "${resource}"
done

cat > "${PACKAGE_DIR}/run.sh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
APP_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NATIVE_DIR="${APP_DIR}/native"
RES_DIR="${APP_DIR}/occt/resources"
export OCCT_BRIDGE_NATIVE_DIR="${NATIVE_DIR}"
export CASROOT="${APP_DIR}/occt"
export LD_LIBRARY_PATH="${NATIVE_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}"
[[ -d "${RES_DIR}/SHMessage" ]] && export CSF_SHMessage="${RES_DIR}/SHMessage"
[[ -d "${RES_DIR}/XSMessage" ]] && export CSF_XSMessage="${RES_DIR}/XSMessage"
[[ -d "${RES_DIR}/XSTEPResource" ]] && export CSF_STEPDefaults="${RES_DIR}/XSTEPResource" && export CSF_IGESDefaults="${RES_DIR}/XSTEPResource"
[[ -d "${RES_DIR}/XCAFResources" ]] && export CSF_XCAFDefaults="${RES_DIR}/XCAFResources" && export CSF_PluginDefaults="${RES_DIR}/XCAFResources"
[[ -d "${RES_DIR}/Textures" ]] && export CSF_MDTVTexturesDirectory="${RES_DIR}/Textures"
if [[ -x "${APP_DIR}/CAD-Avalonia" ]]; then
    exec "${APP_DIR}/CAD-Avalonia" "$@"
fi
exec dotnet "${APP_DIR}/CAD-Avalonia.dll" "$@"
EOF
chmod +x "${PACKAGE_DIR}/run.sh"
[[ -f "${PACKAGE_DIR}/CAD-Avalonia" ]] && chmod +x "${PACKAGE_DIR}/CAD-Avalonia"

{
    echo "CAD-Avalonia Linux x64"
    echo "Configuration: ${CONFIGURATION}"
    echo "Self-contained: ${SELF_CONTAINED}"
    echo "Binary SDK: $(grep -m1 '"'"'bridgeVersion'"'"' "${CONTRACT}" | sed -E 's/.*:[[:space:]]*"([^"]+)".*/\1/')"
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

log "Package: ${PACKAGE_DIR}"
log "Run:     ${PACKAGE_DIR}/run.sh"
