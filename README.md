# OcctCSharpBridge Website

Static bilingual project website for the current **OcctCSharpBridge Bridge 3** architecture.

The website is intentionally a presentation and navigation layer. It does not define SDK behavior, duplicate API documentation, or maintain independent version/API counts.

## Current architecture shown by the site

- `main / main-dev` — the sole Bridge SDK source line.
  - Native Core
  - `OcctNet`
  - `OcctNet.WinForms`
  - `OcctNet.Wpf`
  - `OcctNet.Avalonia`
  - ABI checks, tests, SDK documentation and platform Binary SDK production
- `demo / demo-dev` — the single SDK consumer application line.
  - Windows x64: WinForms, WPF, Avalonia
  - Linux x64: Avalonia only
  - SDK synchronization, consumer validation, run and publish workflows
- `website` — this static site.

The retired standalone Avalonia-branch model must not be reintroduced into site navigation or copy.

## Contract displayed

The site reflects the formal source contract from `main`:

- Bridge `3.0.0-preview.1`
- Native ABI `5` only
- API policy `abi5-only`
- OCCT `7.9.0`
- .NET SDK `10.0.303`
- C# `14`
- Windows x64 / Linux x64

`main/bridge-contract.json` remains the machine-readable source of truth. Do not add hard-coded Native/PInvoke counts or generated per-type API statistics to the website.

## Page structure

1. **Hero** — current contract and repository flow.
2. **Repository model** — responsibilities of SDK, Demo and website branches.
3. **Platform matrix** — explicit Windows/Linux host, publish and Viewer backend coverage.
4. **Bridge capabilities** — concise reusable SDK capability overview.
5. **Demo previews** — canonical WinForms/WPF/Avalonia (Windows x64) screenshots from the formal `demo` branch.
6. **Consumer workflow** — SDK generation → synchronization → build/run → publish.
7. **Documentation** — authoritative links to SDK docs, Demo docs, source contract and license.
8. **Licensing** — concise separation between Bridge license and the project linking exception.

## Canonical preview sources

All screenshots must come from the formal `demo` branch. The current set keeps one English PNG per supported Windows host; the Simplified-Chinese and Linux variants are no longer maintained.

```text
demo/assets/previews/winform-demo-en.png
demo/assets/previews/wpf-demo-en.png
demo/assets/previews/avalonia-win-demo-en.png
```

Do not duplicate those images into `website`.

## Design rules

- clean technical presentation rather than marketing decoration;
- light/dark theme with the same information hierarchy;
- English / Simplified Chinese switching;
- responsive desktop/tablet/mobile layouts;
- visible keyboard focus, skip link and keyboard-operable screenshot lightbox;
- restrained motion and `prefers-reduced-motion` support;
- no decorative icon grids, unnecessary gradients or heavy card shadows;
- use typography, spacing, borders and hierarchy to carry the design.

## Content rules

- describe current behavior, not migration plans;
- use `main` for SDK implementation facts and `demo` for consumer workflow facts;
- keep Windows/Linux support explicit;
- never describe WinForms/WPF as Linux-supported;
- describe the Linux interactive Viewer as X11/XWayland / `Xw_Window`;
- do not imply that Binary SDK payloads are a second source tree;
- do not reintroduce generated API-reference claims.

## Files

- `index.html` — self-contained minimalist static website with embedded styles, bilingual scripts, theme detection, and canonical links.
- `.nojekyll` — static hosting marker.

When SDK or Demo architecture changes, update the source branches first and update the website only after the formal branch state is known.
