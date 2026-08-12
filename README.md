# OcctCSharpBridge Website

Static website / GitHub Pages source for OcctCSharpBridge.

## Current project status

The website distinguishes the current source from the currently published Binary SDK:

- Source: Bridge **2.7.0**, ABI **4**, **349/349** Native/PInvoke, **117** public .NET types, Viewer **215**, Modeling **134**.
- Published `main/dist/win-x64`: Bridge **2.6.0**, ABI **4**, **347/347**, **110** public types, Viewer **213**, Modeling **134**.
- OCCT **7.9.0**, .NET SDK **10.0.302**, C# **14**, Windows x64.

The public page shows a `Published SDK 2.6.0` status badge so source progress cannot be mistaken for a binary release. After the validated Windows 2.7 publish, update the website status together with the release.

## Website behavior

- bilingual English / Simplified Chinese switch with local persistence;
- light / dark theme switch with local persistence and system-theme fallback;
- no graphical logo in the upper-left corner — only the project name;
- live demo previews sourced from versioned `demo/assets/previews` screenshots and switched with language;
- project architecture aligned with the Bridge 2.7 STEP/XDE assembly model;
- licensing section clearly states: **non-commercial use is free; commercial use requires separate authorization**;
- no framework or build step: plain `index.html`, `styles.css`, `app.js` and `.nojekyll`.

## Branch model

- `main`: Bridge source, documentation and tracked Binary SDK.
- `demo`: WinForms/WPF/Avalonia consumers; local `dist/` is ignored.
- `website`: this static site.

## Local preview

```powershell
python -m http.server 8080
```

Then open `http://localhost:8080`.

## Licensing text

The site summarizes the repository policy but does not replace the license text. The authoritative software license remains `main/LICENSE`; commercial authorization details are in `main/COMMERCIAL.md`.
