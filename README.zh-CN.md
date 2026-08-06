# OcctCSharpBridge Demo

[English main branch](https://github.com/zly258/OcctCSharpBridge/tree/main) · [中文接口清单](docs/API_COVERAGE.zh-CN.md) · [API inventory](docs/API_COVERAGE.md)

`demo` 在可复用 OCCT C# 封装基础上提供 WinForms、WPF 示例应用。底层 `src/OcctNative`、`src/OcctNet`、`docs` 和 `tests` 与 `main` 保持同步，界面、场景测试和发布脚本仅保留在本分支。

## 主要能力

- WinForms、WPF OCCT Viewer
- 点选、框选、Ctrl 切换多选和子形选择
- 选中及悬浮高亮颜色设置
- 纯色、渐变背景和多灯光预设
- 二维曲线、基本实体、布尔、特征、变换及分析
- 复杂齿轮、多通道阀体、扭转风管等测试场景
- BRep 矢量文字及线性、角度、半径、直径注释
- STEP、IGES、BREP、STL 导入导出
- 中英文界面

复杂场景执行时使用显示批处理，并在结束后删除截面、刀具体、路径和辅助几何，只保留最终结果。

## 兼容性

- OCCT：必须为 `7.9.0`
- .NET：`8.0`，Windows x64
- Bridge ABI：`1`
- 接口数量：Native `517`，P/Invoke `517`
- `OcctNet.dll` 与 `OcctNative.dll` 必须来自同一次构建

## 构建与运行

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 winform
.\run.ps1 wpf
```

## 发布

默认生成体积较小的框架依赖 WinForms 包：

```powershell
.\publish.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
```

发布 WinForms 和 WPF：

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

生成无需预装 .NET 8 Desktop Runtime 的自包含包：

```powershell
.\publish.ps1 all Release -SelfContained -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1` 只复制原生依赖闭包和需要的 OCCT 资源；`-FullResources`、`-Diagnostics` 仅在需要时开启。
