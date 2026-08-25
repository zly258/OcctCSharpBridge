#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"

command -v rg >/dev/null 2>&1 || {
    printf '[no-reflection] ERROR: rg is required.\n' >&2
    exit 1
}

patterns=(
    'System\.Reflection'
    '\bMethodInfo\b'
    '\bDynamicInvoke\s*\('
    '\bActivator\.CreateInstance\s*\('
    '\bGetMethod[s]?\s*\('
    '\bdynamic\b'
)

arguments=()
for pattern in "${patterns[@]}"; do
    arguments+=(--regexp "${pattern}")
done

if rg --line-number --glob '*.cs' "${arguments[@]}" "${ROOT_DIR}/src" "${ROOT_DIR}/tests"; then
    printf '[no-reflection] ERROR: Reflection or dynamic interface dispatch is forbidden. Use direct, strongly typed API calls.\n' >&2
    exit 1
fi

printf '[no-reflection] Direct, strongly typed dispatch policy passed.\n'
