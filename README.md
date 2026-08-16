# OcctCSharpBridge Website

Static bilingual website for the repository.

The site reflects the final branch model:

- `main` / `main-dev` — the sole Bridge SDK source and development line.
- `demo` / `demo-dev` — the unified Binary SDK consumer: Windows x64 has WinForms, WPF and Avalonia; Linux x64 has Avalonia only.
- `website` — this static site.
- `backup/*` — historical backup branches, intentionally unchanged.

Current contract displayed by the site:

- Bridge `3.0.0-preview.1`
- Native ABI `5` only
- OCCT `7.9.0`
- .NET SDK `10.0.303` exactly
- C# `14`

The Demo section uses canonical screenshots from the formal `demo` branch for WinForms/Windows, WPF/Windows, Avalonia/Windows and Avalonia/Linux. There is no standalone Avalonia branch in the supported architecture.

The current Linux Avalonia Viewer backend is described as X11/XWayland; the site does not claim native Wayland Viewer support.

## Files

- `index.html` — current repository architecture, capabilities, Demo matrix, documentation links and build examples.
- `app.js` — EN/ZH switching, theme, copy and preview lightbox.
- `styles.css` — shared visual system.
- `.nojekyll` — static hosting marker.

Do not reintroduce stale ABI4, Bridge 2.x, generated API-reference counts, or standalone Avalonia branch descriptions.
