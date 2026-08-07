# Demo Preview Images

Place full-resolution, lossless PNG screenshots for the demo applications in this directory.

Canonical file names:

```text
assets/previews/winform-demo-en.png
assets/previews/winform-demo-zh.png
assets/previews/wpf-demo-en.png
assets/previews/wpf-demo-zh.png
assets/previews/avalonia-demo-en.png
assets/previews/avalonia-demo-zh.png
```

Guidelines:

- Commit the original PNG screenshots directly; do not convert them to WebP/JPEG and do not downsample them.
- Keep English and Simplified Chinese screenshots at the same resolution when possible.
- Capture the full application window with a representative OCCT model visible.
- The website can use CSS to scale the thumbnail while its lightbox opens the same original PNG.
- Existing `.webp` files are legacy previews and can be removed after the PNG replacements have been committed and website references have been switched.
