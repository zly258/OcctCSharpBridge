# Demo Preview Images

This directory contains the canonical full-resolution screenshots for the unified `demo` branch.

Canonical files:

```text
assets/previews/winform-demo-en.png
assets/previews/winform-demo-zh.png
assets/previews/wpf-demo-en.png
assets/previews/wpf-demo-zh.png
assets/previews/avalonia-win-demo-en.png
assets/previews/avalonia-win-demo-zh.png
assets/previews/avalonia-linux-demo-en.png
assets/previews/avalonia-linux-demo-zh.png
```

Rules:

- Windows has WinForms, WPF and Avalonia previews;
- Linux has Avalonia previews only;
- keep one English and one Simplified Chinese PNG for each supported host/platform pair;
- commit lossless full-resolution PNGs and keep language variants at the same resolution when practical;
- replace canonical files in place when refreshing screenshots;
- do not create a separate Avalonia branch-specific preview set.

The website should reference these canonical Demo screenshots from the `demo` branch.
