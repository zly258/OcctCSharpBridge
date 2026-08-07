# OcctCSharpBridge Website

[English](README.md)

本分支保存 OcctCSharpBridge 的静态 GitHub Pages 网站。网站与 C++/.NET 构建完全解耦，不依赖 Node.js、npm、打包器、前端框架、CDN 或外部字体，直接使用 HTML/CSS/JavaScript。

## 文件结构

```text
index.html      页面结构与项目内容
styles.css      响应式布局、浅色/深色样式
app.js          中英文切换、代码复制、图片大图预览
.nojekyll       GitHub Pages 按纯静态文件发布
README.md       英文维护说明
README.zh-CN.md 中文维护说明
```

## 本地预览

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch website
```

可以直接打开 `index.html`，但建议使用一个简单静态服务器，更接近 GitHub Pages 的实际运行方式：

```powershell
python -m http.server 8000
```

随后在浏览器打开 `http://localhost:8000/`。网站不需要编译。

## GitHub Pages 配置

```text
Settings
  → Pages
  → Deploy from a branch
  → Branch: website
  → Folder: / (root)
```

网站源码应继续保留在 `website` 分支根目录；`.nojekyll` 用于关闭 Jekyll 处理。

## 语言与主题

- 默认显示英文。
- 页眉按钮可手动切换 English / 简体中文。
- 手动选择的语言写入 `localStorage`。
- 浅色/深色主题继续跟随浏览器或系统 `prefers-color-scheme`。
- 作者始终显示为 `Liaoyuan Zhang`。

翻译文本统一维护在 `app.js` 的 `translations` 中；`index.html` 使用 `data-i18n` 关联翻译键。新增或修改翻译键时必须同时更新英文和中文。

## Demo 图片

网站中的桌面 Demo 截图直接读取 `demo` 分支：

```text
assets/previews/winform-demo-en.png
assets/previews/winform-demo-zh.png
assets/previews/wpf-demo-en.png
assets/previews/wpf-demo-zh.png
assets/previews/avalonia-demo-en.png
assets/previews/avalonia-demo-zh.png
```

WinForms 和 WPF 同时保留 WebP 回退资源。切换网站语言时，`app.js` 会同步切换对应语言截图。

截图支持点击查看大图：

- 鼠标点击图片打开全屏 Lightbox；
- 键盘焦点位于图片时可按 Enter/Space 打开；
- 点击遮罩、关闭按钮或按 Esc 关闭；
- 关闭后焦点返回原图片。

新增图片时，优先放在 `.preview-card` 中，这样会自动获得相同的大图预览行为。

## Getting Started 内容原则

首页不再只列几个零散构建命令，而是从第一次使用开始展示完整顺序：

```powershell
# 克隆仓库
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge

# 切换到可运行桌面 Demo
git switch demo

# 配置 OCCT 7.9.0
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"

# 接口校核
.\build.ps1 validate Release

# 完整构建
.\build.ps1 all Release

# 运行 Demo
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia

# 真实 Native Smoke Test
.\build.ps1 smoke Release

# 发布 WinForms/WPF 包
.\publish.ps1 all Release -Zip
```

每个脚本的详细参数和 target 解释放在 `main`、`demo` 各自 README 中；网站首页只保留清晰、可复制的首次使用流程。

## 页面维护要求

- 保持纯静态、无构建步骤。
- 默认英文，中文为手动切换项。
- 保持桌面、平板、手机响应式布局。
- 标题字号克制，不使用过度夸张的营销式大标题。
- 不为了装饰引入图标库、前端框架或远程字体。
- 图片必须保留有效 `alt` 文本和键盘操作能力。
- 分支职责、PowerShell 用法变化时，网站和对应分支 README 要同步修改。

## 各分支职责

- [`main`](https://github.com/zly258/OcctCSharpBridge/tree/main)：纯净、可复用的 OCCT C++/C# Bridge
- [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo)：WinForms、WPF、Avalonia 参考应用
- `website`：本静态网站

## 作者

Liaoyuan Zhang

## 联系方式

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
