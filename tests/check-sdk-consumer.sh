#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
cd "${ROOT_DIR}"

fail() { printf '[consumer] ERROR: %s\n' "$*" >&2; exit 1; }

mapfile -t tracked < <(git ls-files)
for path in "${tracked[@]}"; do
    case "${path}" in
        dist/*|external/*|artifacts/*|build/*|publish/*|.cache/*|*/bin/*|*/obj/*|*/TestResults/*|*/coverage/*)
            fail "Generated/cache path must not be tracked by the Demo branch: ${path}"
            ;;
        *.dll|*.exe|*.so|*.dylib|*.pdb|*.ilk|*.exp|*.idb|*.tlog|*.zip|*.tar|*.tar.gz|*.tgz|*.7z|*.rar|*.nupkg|*.snupkg)
            fail "Generated binary/archive must not be tracked by the Demo branch: ${path}"
            ;;
        src/OcctNative/*|src/OcctNet/*|src/OcctNet.WinForms/*|src/OcctNet.Wpf/*|src/OcctNet.Avalonia/*)
            fail "Demo must consume the Binary SDK and must not track SDK implementation source: ${path}"
            ;;
    esac

    if [[ -f "${path}" ]]; then
        size="$(stat -c %s "${path}")"
        if (( size > 2097152 )); then
            fail "Tracked file exceeds the 2 MiB repository hygiene limit: ${path} (${size} bytes). Move large generated assets to Release/Artifacts or explicitly redesign the repository policy before tracking them."
        fi
    fi
done

SYNC_SH="${ROOT_DIR}/sync.sh"
[[ -f "${SYNC_SH}" ]] || fail "Linux Demo sync script was not found."
for forbidden in 'build.sh all' 'build.sh test' 'build.sh smoke' 'publish.sh' 'worktree add'; do
    if grep -Fq -- "${forbidden}" "${SYNC_SH}"; then
        fail "sync.sh must not run the Bridge full validation/publish workflow: ${forbidden}"
    fi
done
for required in 'sourceCommit' 'sha256sum' 'package-manifest.json' '--sdk-root' '--portable-root' 'bash ./build.sh dist Release' 'package-portable-sdk.sh' 'EXTERNAL_ROOT="${ROOT_DIR}/external"' 'SOURCE_ROOT="${EXTERNAL_ROOT}/.cache/OcctCSharpBridge-source"' 'BRIDGE_ROOT="${EXTERNAL_ROOT}/OcctCSharpBridge"'; do
    grep -Fq -- "${required}" "${SYNC_SH}" || fail "sync.sh lost required source-build SDK integrity/layout behavior: ${required}"
done

patterns=(
    '\bocct_[A-Za-z0-9_]+\b'
    '(LibraryImport|DllImport)[[:space:]]*\([[:space:]]*"OcctNative"'
    '\b(OcctHandle|OcctModelHandle|nativeAbiVersion|legacyAbi4Exports|compatibilityExtensions|plannedRemoval)\b'
    '\bmodelOf[[:space:]]*\('
    '\bEngine\.(Objects|Shapes|Exists|GetShape|GetName|SetName)\b'
    '\bEngine\.(SetColor|SetTransparency|SetVisible|SetLineWidth|SetMaterial)\b'
    '\bEngine\.Display\b'
    '\bEngine\.(MakeTextShape|MakeLengthAnnotationShape|MakeAngleAnnotationShape|MakeRadiusAnnotationShape|MakeDiameterAnnotationShape)\b'
    '\b(EngineInitialized|EnableDefaultInteraction|EnableRectangleSelection)\b'
)

violations=()
while IFS= read -r path; do
    [[ -f "${path}" ]] || continue
    for pattern in "${patterns[@]}"; do
        while IFS= read -r match; do
            [[ -n "${match}" ]] && violations+=("${path}:${match}")
        done < <(grep -nE "${pattern}" "${path}" || true)
    done
done < <(git ls-files 'src/**/*.cs' 'src/*.cs')

if (( ${#violations[@]} > 0 )); then
    printf '[consumer] Demo implementation crosses the Bridge 3 consumer boundary or uses retired APIs:\n' >&2
    printf ' - %s\n' "${violations[@]}" >&2
    exit 1
fi

printf '[consumer] Demo remains a Bridge 3/ABI5 binary consumer; Linux SDK sync builds only dist Release plus the matching Portable SDK, external dependency paths are untracked, repository hygiene rejects generated/large tracked artifacts, and Bridge tests/smoke are not rerun.\n'
