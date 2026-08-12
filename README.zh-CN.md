# OcctCSharpBridge Website

OcctCSharpBridge 静态官网 / GitHub Pages 源码分支。

## 当前项目状态

Website 明确区分当前源码与当前已发布 Binary SDK：

- 源码：Bridge **2.7.0**、ABI **4**、Native/PInvoke **349/349**、Public .NET types **117**、Viewer **215**、Modeling **134**；
- 已发布 `main/dist/win-x64`：Bridge **2.6.0**、ABI **4**、**347/347**、Public types **110**、Viewer **213**、Modeling **134**；
- OCCT **7.9.0**、.NET SDK **10.0.302**、C# **14**、Windows x64。

页面会显示 `已发布 SDK 2.6.0` 状态徽标，避免把源码进度误认为已经发布的 Binary SDK。等 2.7 在 Windows 完成真实构建验证和发布后，再随正式版本一起更新 Website 状态。

## 网站能力

- English / 简体中文语言切换并本地记忆；
- 浅色 / 深色主题切换并本地记忆，首次进入默认跟随系统主题；
- 左上角不使用图形 Logo，只显示 `OcctCSharpBridge` 项目文字；
- Demo 预览图直接读取 `demo/assets/previews` 中被版本管理的三端截图，并随语言切换 EN/ZH 图；
- 架构说明已更新为 Bridge 2.7 的 STEP/XDE 装配模型：XDE 只用于 STEP 交换内部，不作为应用层 Document；
- 授权区明确说明：**非商业使用免费，商业使用需要单独授权**；
- 不使用前端框架和构建链，只有 `index.html`、`styles.css`、`app.js` 与 `.nojekyll`。

## 分支职责

- `main`：Bridge 源码、文档与被跟踪的正式 Binary SDK；
- `demo`：WinForms/WPF/Avalonia 消费示例，本地 `dist/` 被忽略；
- `website`：当前静态官网。

## 本地预览

```powershell
python -m http.server 8080
```

然后访问 `http://localhost:8080`。

## 许可说明

网站上的授权说明用于便于理解，但不替代正式许可证。软件授权以 `main/LICENSE` 为准；商业授权说明见 `main/COMMERCIAL.md`。
