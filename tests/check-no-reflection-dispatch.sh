#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"

command -v find >/dev/null 2>&1 || {
    printf '[no-reflection] ERROR: find is required.\n' >&2
    exit 1
}
command -v grep >/dev/null 2>&1 || {
    printf '[no-reflection] ERROR: grep is required.\n' >&2
    exit 1
}

patterns=(
    'System\.Reflection'
    'MethodInfo'
    'DynamicInvoke[[:space:]]*\('
    'Activator\.CreateInstance[[:space:]]*\('
    'GetMethods?[[:space:]]*\('
    '(^|[^[:alnum:]_])dynamic([^[:alnum:]_]|$)'
)

violations=()
while IFS= read -r -d '' path; do
    for pattern in "${patterns[@]}"; do
        while IFS= read -r match; do
            [[ -n "${match}" ]] && violations+=("${path}:${match}")
        done < <(grep -nE "${pattern}" "${path}" || true)
    done
done < <(
    find "${ROOT_DIR}/src" "${ROOT_DIR}/tests" \
        -type d \( -name bin -o -name obj \) -prune -o \
        -type f -name '*.cs' -print0
)

if (( ${#violations[@]} > 0 )); then
    printf '%s\n' "${violations[@]}"
    printf '[no-reflection] ERROR: Reflection or dynamic interface dispatch is forbidden. Use direct, strongly typed API calls.\n' >&2
    exit 1
fi

printf '[no-reflection] Direct, strongly typed dispatch policy passed.\n'
