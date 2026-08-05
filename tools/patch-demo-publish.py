from pathlib import Path


def replace_exact(path: str, old: str, new: str, description: str) -> None:
    file = Path(path)
    text = file.read_text(encoding="utf-8-sig")
    if old in text:
        file.write_text(text.replace(old, new), encoding="utf-8", newline="\n")
        print(f"Updated {description}.")
    elif new in text:
        print(f"{description} already updated.")
    else:
        raise SystemExit(f"Expected text for {description} was not found in {path}.")


replace_exact(
    "publish.ps1",
    '$safeName = $relative -replace "[\\\\/:*?\\\"<>|]", "_"',
    "$safeName = $relative -replace '[\\\\/:*?\"<>|]', '_'",
    "PowerShell-safe license filename expression",
)

replace_exact(
    "src/CadCommon/CadSession.cs",
    "        Engine.Fit(shape);\n",
    "",
    "Demo shape creation camera preservation",
)
