# OcctCSharpBridge Website

This branch contains the static GitHub Pages site for [OcctCSharpBridge](https://github.com/zly258/OcctCSharpBridge). It is intentionally independent from the C++/.NET build and has no Node.js, npm, bundler, framework, CDN, or external font dependency.

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

Then open `http://localhost:8000/` in a browser.

No compilation step is required.

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
- No application or author name is localized; the author display remains `Liaoyuan Zhang`.

Translation strings live in the `translations` object in `app.js`. Elements that participate in localization use `data-i18n` keys in `index.html`.

## Demo screenshots

The desktop screenshots shown by the site are loaded from the `demo` branch:

```text
assets/previews/winform-demo-en.webp
assets/previews/winform-demo-zh.webp
assets/previews/wpf-demo-en.webp
assets/previews/wpf-demo-zh.webp
```

`app.js` switches the screenshot source together with the selected site language.

Preview images are interactive:

- click an image to open a full-size lightbox;
- press Enter/Space when an image has keyboard focus;
- click the backdrop, click the close button, or press Esc to close;
- focus returns to the originating screenshot after closing.

When adding another screenshot, place it inside `.preview-card` so the same lightbox behavior is applied automatically.

## Getting Started section

The website is meant to show the complete first-run sequence rather than isolated build commands. Keep it in this order:

```powershell
# Clone
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge

# Select runnable desktop examples
git switch demo

# Configure OCCT 7.9.0
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"

# Validate
.\build.ps1 validate Release

# Build
.\build.ps1 all Release

# Run
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia

# Native smoke test
.\build.ps1 smoke Release

# Package WinForms/WPF
.\publish.ps1 all Release -Zip
```

Detailed script semantics belong in the `main`, `demo`, and `script` branch READMEs; the website should stay concise enough to scan.

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
- [`script`](https://github.com/zly258/OcctCSharpBridge/tree/script): OcctScript parametric editor
- `website`: this static site

## Author

Liaoyuan Zhang
