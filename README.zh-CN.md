# OcctCSharpBridge Website

项目双语静态官网。

官网按最终分支模型展示：

- `main` / `main-dev`：唯一 Bridge SDK 源与开发线。
- `demo` / `demo-dev`：统一 Binary SDK Consumer；Windows x64 提供 WinForms、WPF、Avalonia，Linux x64 仅提供 Avalonia。
- `website`：本静态官网。
- `backup/*`：历史备份分支，本次迁移保持不变。

官网展示的当前 Contract：

- Bridge `3.0.0-preview.1`
- Native ABI `5` only
- OCCT `7.9.0`
- .NET SDK `10.0.303` exact
- C# `14`

Demo 区统一引用正式 `demo` 分支中的 WinForms/Windows、WPF/Windows、Avalonia/Windows、Avalonia/Linux 截图。目标架构中不再存在独立 Avalonia 分支。

Linux Avalonia 当前 Viewer Backend 明确为 X11/XWayland，官网不宣称 Native Wayland Viewer 已完成。

## 文件

- `index.html`：当前仓库架构、能力、Demo 平台矩阵、文档链接和构建示例。
- `app.js`：中英文切换、主题、复制和预览 Lightbox。
- `styles.css`：统一视觉样式。
- `.nojekyll`：静态托管标记。

后续不要重新引入 ABI4、Bridge 2.x、过期 API 数量统计或独立 Avalonia 分支描述。
