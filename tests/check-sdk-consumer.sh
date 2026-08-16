#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
cd "${ROOT_DIR}"

fail() { printf '[consumer] ERROR: %s\n' "$*" >&2; exit 1; }

mapfile -t tracked < <(git ls-files)
for path in "${tracked[@]}"; do
    case "${path}" in
        src/OcctNative/*|src/OcctNet/*|src/OcctNet.WinForms/*|src/OcctNet.Wpf/*|src/OcctNet.Avalonia/*)
            fail "demo must consume the Binary SDK and must not track SDK implementation source: ${path}"
            ;;
    esac
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

printf '[consumer] Demo is a Bridge 3/ABI5 consumer only: no SDK sources, direct native ABI calls, pre-ABI5 handles/metadata, or retired managed APIs.\n'
