from pathlib import Path

path = Path("publish.ps1")
text = path.read_text(encoding="utf-8-sig")
marker = "function Resolve-Dumpbin {"
positions = []
start = 0
while True:
    index = text.find(marker, start)
    if index < 0:
        break
    positions.append(index)
    start = index + len(marker)

if len(positions) == 2:
    text = text[:positions[0]] + text[positions[1]:]
elif len(positions) != 1:
    raise SystemExit(f"Expected one or two Resolve-Dumpbin definitions, found {len(positions)}.")

required = [
    "Resolve-Dumpbin",
    "Get-PeDependencies",
    "Test-SystemDependency",
    "Test-RuntimeCandidate",
    "Get-RuntimeCandidateScore",
    "New-RuntimeCandidateIndex",
    "Resolve-RuntimeDependency",
    "Copy-OcctRuntime",
]
for name in required:
    count = text.count(f"function {name} {{")
    if count != 1:
        raise SystemExit(f"Expected one {name} definition, found {count}.")

path.write_text(text, encoding="utf-8", newline="\n")
print("Removed duplicate native dependency resolver functions.")
