# OcctCSharpBridge Website

[简体中文](README.zh-CN.md)

This branch contains the static GitHub Pages site for [OcctCSharpBridge](https://github.com/zly258/OcctCSharpBridge). It is intentionally independent from the C++/.NET build and has no Node.js, npm, bundler, framework, CDN, or external font dependency.

## Project contract

The website describes the current `main` Bridge contract: Bridge **2.6.0**, Native ABI **3**, OCCT **7.9.0**, .NET SDK **10.0.302**, target **`net10.0-windows`**, and C# **14.0**. `main/bridge-contract.json` is authoritative; when those values change, the website content and website contract checks must change in the same update.

## Files

```text
index.html      page structure and project content
styles.css      responsive light/dark presentation
app.js          localization, copy action and image lightbox
.nojekyll       serve the branch as plain static files on GitHub Pages
README.md       website maintenance notes
```

## Local preview

Clone the repository and switch to the website branch:

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch website
```

The site can be opened directly from `index.html`, but using a local static server gives behavior closer to GitHub Pages:

```powershell
python -m http.server 8000
```

Then open `http://localhost:8000/` in a browser. No compilation step is required.

## GitHub Pages

Configure repository Pages as:

```text
Settings
  → Pages
  → Deploy from a branch
  → Branch: website
  → Folder: / (root)
```

The published source must remain at the branch root. `.nojekyll` prevents Jekyll processing.

## Language and theme

- English is the default language.
- The header language button switches between English and Simplified Chinese.
- The manual language choice is saved in `localStorage`.
- Light/dark appearance follows the browser/system `prefers-color-scheme` setting.
- The author display remains `Liaoyuan Zhang`.

Translation strings live in the `translations` object in `app.js`. Elements that participate in localization use `data-i18n` keys in `index.html`.

## Demo screenshots

The desktop screenshots shown by the site are loaded from the `demo` branch:

```text
assets/previews/winform-demo-en.png
assets/previews/winform-demo-zh.png
assets/previews/wpf-demo-en.png
assets/previews/wpf-demo-zh.png
assets/previews/avalonia-demo-en.png
assets/previews/avalonia-demo-zh.png
```

WinForms and WPF additionally keep WebP fallbacks. `app.js` switches the screenshot source together with the selected site language. Preview images support a full-size lightbox: click or use Enter/Space to open, and click the backdrop/close button or press Esc to close. Keyboard focus returns to the originating image.

When adding another screenshot, place it inside `.preview-card` so the same lightbox behavior is applied automatically.

## Getting Started section

The website shows the complete first-run sequence rather than isolated build commands:

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"

.\build.ps1 validate Release
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
.\build.ps1 smoke Release
.\publish.ps1 all Release -Zip
```

Detailed script semantics belong in the `main` and `demo` branch READMEs; the website should stay concise enough to scan.

## Editing guidelines

- Keep the site framework-free and usable as plain GitHub Pages content.
- Default to English; Chinese is an optional manual switch.
- Preserve responsive behavior at desktop/tablet/mobile widths.
- Keep headings compact and avoid oversized marketing typography.
- Do not add decorative libraries or remote fonts just for appearance.
- Keep screenshot `alt` text meaningful and preserve keyboard access to the lightbox.
- When changing a translation key, update both English and Chinese entries.
- When changing branch responsibilities or PowerShell usage, update the website and the corresponding branch README together.

## Related branches

- [`main`](https://github.com/zly258/OcctCSharpBridge/tree/main): reusable OCCT C++/C# bridge
- [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo): WinForms, WPF and Avalonia reference applications
- `website`: this static site

## Author

Liaoyuan Zhang

## Contact

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
