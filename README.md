# OcctCSharpBridge Website

Static bilingual website for the repository.

The site now reflects the branch split explicitly:

- `main` — Windows x64 Bridge, `OcctNet + WinForms + WPF`, source contract 349/349 and 113 public .NET types.
- `demo` — Windows WinForms/WPF demos only.
- `avalonia` — standalone cross-platform `OcctNet + OcctNet.Avalonia`, `net10.0`, Windows x64 + Linux x64, source contract 350/350 and 109 public .NET types.
- `website` — this static site.

The demo section intentionally shows only WinForms and WPF screenshots. Avalonia is presented as a separate cross-platform source branch rather than a third Windows demo host.

The first Linux Avalonia Viewer backend is documented as X11/XWayland; the site does not claim native Wayland Viewer support is complete.

## Files

- `index.html` — page structure and branch/capability content.
- `app.js` — EN/ZH switching, theme, copy and preview lightbox.
- `styles.css` — shared visual system.
- `.nojekyll` — static hosting marker.

Do not reintroduce stale `OcctNet.Avalonia` content under main/demo descriptions.