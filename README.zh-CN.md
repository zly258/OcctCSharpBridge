# OcctCSharpBridge Demo

[English](README.md) · [中文接口清单](docs/API_COVERAGE.zh-CN.md) · [主 SDK 分支](https://github.com/zly258/OcctCSharpBridge/tree/main)

`demo` 在可复用 OCCT C# 封装基础上提供 WinForms、WPF 示例应用。底层 `src/OcctNative`、`src/OcctNet`、`src/OcctNet.WinForms`、`docs` 和 `tests` 与 `main` 保持同步，界面、场景测试和发布脚本仅保留在本分支。

托管封装分为：

- `OcctNet`：不依赖界面的 Viewer、建模、分析、修复、网格和文件交换接口。
- `OcctNet.WinForms`：可选的 `OcctViewportControl` 宿主，供 WinForms 及 WPF `WindowsFormsHost` 使用。

桥接层不包含 OCAF/XDE。文档、JSON 持久化、撤销重做和命令历史由上层应用自行实现。

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
- Bridge ABI：`2`
- 接口数量：Native `281`，P/Invoke `281`
- 使用 Demo 视口时，`OcctNet.dll`、`OcctNet.WinForms.dll` 与 `OcctNative.dll` 必须来自同一次构建
- 原生会话释放已改为幂等且终结器安全，但同一会话仍应由单一应用线程调用

## 构建与运行

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 winform
.\run.ps1 wpf
```

`build.ps1 managed` 不需要 OCCT SDK，可先验证原生源文件清单与接口一致性，再构建核心封装、可选 WinForms 宿主和公共 Demo 层。

## 发布

默认命令同时发布 WinForms 和 WPF，并生成 Windows x64 自包含程序；目标电脑不需要另外安装 .NET。

发布包按可直接部署设计：两个可执行程序分别内嵌 .NET 运行时，`runtime` 包含 `OcctNative.dll` 以及递归解析得到的 OCCT、第三方库和 Visual C++ DLL 依赖闭包，`occt/src` 包含必须的 OCCT 资源。缺少任何必需原生依赖或 OCCT 资源时发布会直接失败；`package-contract.json` 与 `native-dependencies.txt` 用于说明和校核包内容。

```powershell
.\publish.ps1 -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

只发布其中一个程序：

```powershell
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

只有目标电脑已经安装 .NET 8 Desktop Runtime 时，才使用体积较小的框架依赖模式：

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

`publish.ps1` 只复制原生依赖闭包和纯几何桥接需要的资源；被引用的 `OcctNet` 与 `OcctNet.WinForms` 程序集由 `dotnet publish` 自动包含。`-FullResources`、`-Diagnostics` 仅在需要时开启。
