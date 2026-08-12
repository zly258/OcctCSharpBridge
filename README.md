# OcctCSharpBridge Website

Static website / GitHub Pages source for OcctCSharpBridge.

## Project status

The website distinguishes the current source contract from the actually published Binary SDK:

- Source contract: Bridge **2.7.0**, ABI **4**, **349/349** Native/PInvoke, **117** public .NET types, Viewer **215**, Modeling **134**.
- Published Binary SDK version is read at runtime from `main/dist/win-x64/bridge-contract.json`; it is not hard-coded in the website.
- OCCT **7.9.0**, .NET SDK **10.0.302**, C# **14**, Windows x64.

If the published contract request is unavailable, the page falls back to a neutral `main/dist` status instead of inventing a version.

## Website behavior

- bilingual English / Simplified Chinese switch with local persistence;
- Chinese copy keeps established engineering/software terms such as `Viewer`, `AIS`, `Headless Modeling`, `Selection & Input`, `Geometry & Topology`, `Meshing`, `STEP Assembly Exchange`, `XDE`, `UI Host`, `Native`, `P/Invoke` and `C ABI` instead of forcing literal Chinese translations;
- light / dark theme switch with local persistence and system-theme fallback;
- no graphical logo in the upper-left corner — only the project name;
- published SDK status sourced from the tracked `main/dist/win-x64` contract;
- three live Demo previews sourced from versioned `demo/assets/previews` screenshots and switched with language;
- every Demo preview supports click-to-enlarge Lightbox viewing, keyboard `Enter` / `Space`, `Esc` to close, close button and backdrop click;
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
