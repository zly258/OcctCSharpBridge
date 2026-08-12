# Demo Preview Images

This directory contains the canonical full-resolution screenshots for the two Windows demo hosts on the `demo` branch.

Canonical files:

```text
assets/previews/winform-demo-en.png
assets/previews/winform-demo-zh.png
assets/previews/wpf-demo-en.png
assets/previews/wpf-demo-zh.png
```

Rules:

- keep exactly one English and one Simplified Chinese PNG for WinForms and WPF;
- commit lossless full-resolution PNGs;
- keep the two language variants at the same resolution when practical;
- show a representative OCCT model and the complete application window;
- replace canonical files in place when refreshing screenshots;
- do not add Avalonia preview files to `demo`; Avalonia belongs to the separate `avalonia` branch.

The website should reference these four demo screenshots only.