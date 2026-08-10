# Demo Preview Images

This directory contains the canonical full-resolution PNG screenshots used by the `demo` branch documentation and the `website` branch.

Current project metadata shown by the demos and website:

```text
Author: Liaoyuan Zhang
Bridge: 2.6.0
Native ABI: 4
OCCT: 7.9.0
.NET SDK: 10.0.302
C#: 14.0
Avalonia: 12.1.0
Platform: Windows x64
```

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
- The website must reference these six canonical `demo` branch URLs directly.
- If About information is visible in a screenshot, the author must be `Liaoyuan Zhang` in both language modes and the displayed Bridge/ABI/technology baseline must match `DemoProductInfo`.
- Demo build validation is performed by `build.ps1 validate`; there is no `tests/check-demo-package.ps1` in the Binary SDK consumer branch.

Do not add legacy preview aliases. If a screenshot is refreshed, replace the canonical PNG in place so documentation and website references remain stable.
