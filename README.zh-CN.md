# OcctCSharpBridge Website

当前 **OcctCSharpBridge Bridge 3** 架构的双语静态官网。

官网只负责项目说明与导航，不定义 SDK 行为、不复制 API 文档，也不单独维护接口数量或版本事实源。

## 官网展示的当前架构

- `main / main-dev`：唯一 Bridge SDK 源。
  - Native Core
  - `OcctNet`
  - `OcctNet.WinForms`
  - `OcctNet.Wpf`
  - `OcctNet.Avalonia`
  - ABI 契约检查、测试、SDK 文档与平台 Binary SDK 生产
- `demo / demo-dev`：唯一 SDK Consumer 应用树。
  - Windows x64：WinForms、WPF、Avalonia
  - Linux x64：仅 Avalonia
  - SDK 同步、Consumer 校验、运行与发布流程
- `website`：当前静态官网。

已经淘汰的独立 Avalonia 分支模型不得重新出现在官网导航或文案中。

## 官网展示的 Contract

官网以正式 `main` 源码契约为准：

- Bridge `3.0.0-preview.1`
- Native ABI：仅 `5`
- API Policy：`abi5-only`
- OCCT `7.9.0`
- .NET SDK `10.0.303`
- C# `14`
- Windows x64 / Linux x64

`main/bridge-contract.json` 是机器可读事实源。官网不要重新增加 Native/PInvoke 数量、Public Type 数量或逐类型 Generated API Reference 统计。

## 页面结构

1. **首页 Hero**：当前 Contract 与仓库数据流。
2. **仓库模型**：SDK、Demo、website 三类职责。
3. **平台矩阵**：明确列出 Windows/Linux Host、发布和 Viewer Backend 支持范围。
4. **Bridge 能力**：简洁说明可复用 SDK 能力。
5. **Demo 预览**：正式 `demo` 分支中的 WinForms/WPF/Avalonia 截图。
6. **Consumer 流程**：SDK 生成 → 同步校验 → 构建运行 → 发布。
7. **文档入口**：SDK 文档、Demo 文档、源码 Contract 与许可证。
8. **许可证**：区分 Bridge License 与项目 Linking Exception。

## 正式截图来源

所有截图必须直接来自正式 `demo` 分支：

```text
demo/assets/previews/winform-demo-en.png
demo/assets/previews/winform-demo-zh.png
demo/assets/previews/wpf-demo-en.png
demo/assets/previews/wpf-demo-zh.png
demo/assets/previews/avalonia-win-demo-en.png
demo/assets/previews/avalonia-win-demo-zh.png
demo/assets/previews/avalonia-linux-demo-en.png
demo/assets/previews/avalonia-linux-demo-zh.png
```

不要把这些图片再复制一份到 `website`。

## 设计规范

- 技术型、正式、克制，不做营销式装饰；
- 浅色/深色模式保持同一信息层级；
- 支持英文/简体中文切换；
- 桌面、平板、移动端响应式；
- 明确 Keyboard Focus，提供 Skip Link，截图 Lightbox 支持键盘操作；
- 支持 `prefers-reduced-motion`；
- 不堆装饰性图标，不使用无必要的大面积渐变和重阴影；
- 主要通过字体层级、间距、边框和结构体现设计质量。

## 内容规范

- 描述当前正式状态，不写迁移计划；
- SDK 实现事实以 `main` 为准，Consumer 流程以 `demo` 为准；
- Windows/Linux 支持范围必须明确；
- 不得把 WinForms/WPF 描述成 Linux 支持项；
- Linux 交互 Viewer 明确为 X11/XWayland / `Xw_Window`；
- Binary SDK 是构建/发布产物，不是第二套源码树；
- 不重新引入 Generated API Reference 或硬编码 API 数量。

## 文件

- `index.html`：语义化页面结构与正式仓库/文档链接。
- `app.js`：中英文文案、主题状态、截图 Lightbox 与复制按钮。
- `styles.css`：响应式视觉系统。
- `.nojekyll`：静态托管标记。

当 SDK 或 Demo 架构发生变化时，应先更新并确认正式源码分支，再更新官网。
