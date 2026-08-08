# Demo Preview Images

This directory contains the canonical full-resolution PNG screenshots used by the `demo` branch READMEs.

Canonical file names:

```text
assets/previews/winform-demo-en.png
assets/previews/winform-demo-zh.png
assets/previews/wpf-demo-en.png
assets/previews/wpf-demo-zh.png
assets/previews/avalonia-demo-en.png
assets/previews/avalonia-demo-zh.png
```

Rules:

- Keep exactly one English and one Simplified Chinese PNG for each demo application.
- Commit the original lossless PNG screenshots directly; do not convert them to WebP/JPEG and do not downsample them.
- Keep English and Simplified Chinese screenshots at the same resolution when possible.
- Capture the full application window with a representative OCCT model visible.
- Root `README.md` must reference the three `-en.png` files; `README.zh-CN.md` must reference the three `-zh.png` files.
- Root README image URLs are pinned to the `demo` branch so rendering does not depend on the viewer's current branch or copied Markdown context.
- `tests/check-demo-package.ps1` validates the six canonical assets, language mapping, PNG format, and README URL contract.

Do not add legacy preview aliases. If a screenshot is refreshed, replace the canonical PNG in place so documentation and website references remain stable.
