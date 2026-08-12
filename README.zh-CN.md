# OcctCSharpBridge Website

项目双语静态官网。

当前官网明确区分四个分支职责：

- `main`：Windows x64 Bridge，`OcctNet + WinForms + WPF`，源码契约 349/349、113 个 Public .NET Type。
- `demo`：仅 Windows WinForms/WPF Demo。
- `avalonia`：独立跨平台 `OcctNet + OcctNet.Avalonia`，`net10.0`，Windows x64 + Linux x64，源码契约 350/350、109 个 Public .NET Type。
- `website`：本静态官网。

Demo 区只展示 WinForms/WPF 四张中英文截图；Avalonia 不再作为第三个 Windows Demo，而是作为独立跨平台分支展示。

Linux Avalonia 第一阶段 Viewer Backend 明确写为 X11/XWayland，官网不会宣称 Native Wayland Viewer 已经完成。

## 文件

- `index.html`：页面结构、分支与能力说明。
- `app.js`：中英文切换、主题、复制和预览 Lightbox。
- `styles.css`：统一视觉样式。
- `.nojekyll`：静态托管标记。

后续不要再把 `OcctNet.Avalonia` 写回 main/demo 的职责描述。