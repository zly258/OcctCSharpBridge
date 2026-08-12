# OcctCSharpBridge Demo

[English](README.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [Demo 维护说明](docs/README.md) · [中文 API Reference](https://github.com/zly258/OcctCSharpBridge/tree/main/docs/zh-CN/api)

`demo` 分支是 OcctCSharpBridge 的标准 **Binary SDK 消费示例**。它只保留应用/Demo 代码，不再复制 Bridge 的 Native 或 Managed 源码。

## 项目信息

| 项目 | 当前值 |
| --- | --- |
| Author | **zly258** |
| Demo / Bridge 版本 | **2.6.0** |
| Native ABI | **4** |
| Open CASCADE Technology | **7.9.0** |
| .NET SDK | **10.0.302** |
| Target Framework | **`net10.0-windows`** |
| C# | **14.0** |
| Native Bridge | **C++17** |
| Avalonia | **12.1.0** |
| Platform | **Windows x64** |
| SDK 接口规模 | **344 Native exports / 344 P/Invoke / 105 public .NET types** |

Demo 实际消费的 Bridge 版本以 `dist/win-x64/bridge-contract.json` 与 `bridge-manifest.json` 为机器可读事实源。

## 应用项目

```text
src/OcctDemo.Common
src/OcctDemo.WinForms
src/OcctDemo.Wpf
src/OcctDemo.Avalonia
```

WinForms、WPF、Avalonia 三套 Demo 的 About 信息统一读取 `OcctDemo.Common/DemoProductInfo.cs`，作者、版本和技术栈不再各自硬编码。

## 三端统一交互与测试覆盖

WinForms、WPF、Avalonia 使用同一套 Demo 业务行为，并覆盖以下交互与建模验证：

- 对象、顶点、边、线框、面、壳、实体选择过滤；WinForms 工具栏选择下拉框与 Viewer 选择模式直接同步；
- 框选或 Ctrl 多选多个对象时，属性面板不再显示第一个对象的属性，只显示“已选择 N 个对象”；
- 三套 Viewport 均直接使用 Binary SDK 的 `ZoomSensitivity`，默认 `1.0`，可在“视图”菜单调整鼠标滚轮缩放灵敏度；
- Undo/Redo 由共享历史模型驱动，Avalonia 菜单和工具栏状态随 `HistoryChanged` / `ModelChanged` 同步刷新；
- Avalonia 主窗口、属性区、日志、状态栏和参数对话框统一使用 `Microsoft YaHei UI`；
- “示例”菜单提供 **B 样条曲面测试**：真实创建非直纹放样曲面，并校验次数、控制点、权重、节点和重复度；
- “示例”菜单提供 **网格生成测试**：真实调用 `GetShapeMeshData`，校验面数、节点、三角形以及逐面 provenance 连续区间。

这些测试不是静态展示图，而是直接执行 Bridge 建模/分析 API；测试结果会写入 Demo 命令日志。

## Binary SDK

`main/publish.ps1` 负责把已验证 SDK 发布到本分支：

```text
dist/win-x64/
├─ OcctNative.dll
├─ OcctNet.dll
├─ OcctNet.WinForms.dll
├─ OcctNet.Wpf.dll
├─ OcctNet.Avalonia.dll
├─ bridge-contract.json
└─ bridge-manifest.json
```

Demo 不再维护反向同步脚本。SDK 发布统一从 `main` 发起，默认 OCCT 路径已由主分支脚本处理：

```powershell
# main branch
.\publish.ps1
```

main 发布流程会生成中英文完整 API Reference，执行 Release Native/Managed Build，生成 Binary SDK，再通过临时 worktree 同步 `dist/win-x64` 到 demo。Managed regression 与 Native Smoke 都保留为显式测试目标，不再阻塞 Binary SDK 发布。

## 环境要求

构建 Demo 只需要：

- Windows 10/11 x64
- .NET SDK `10.0.302`
- 完整有效的 `dist/win-x64` Binary SDK

**Demo 构建不需要 CMake/MSVC。** CMake/MSVC 只属于 `main` 生成 Bridge Binary SDK 的生产流程。

运行时还需要 OCCT 7.9.0 Runtime，可通过 `OCCT_ROOT`、`CASROOT` 或显式 `OcctRuntime` 配置提供。

## 构建

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release
```

单独构建：

```powershell
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

`validate` 校验 Binary SDK Contract、Manifest 与 SHA-256。

## 运行

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"

.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

应用输出目录自动包含 `OcctNative.dll`，`run.ps1` 负责配置 OCCT 与第三方 Runtime 搜索路径。

## 发布 Demo 应用

```powershell
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

三个 Demo 统一发布到同一个目录，共享一套 .NET、Bridge 和 OCCT Runtime，不再生成 `winform/`、`wpf/`、`avalonia/` 三套重复运行环境：

```text
artifacts/publish/OcctCSharpBridge-Demo-all-win-x64/
├─ CAD-Winform.exe
├─ CAD-WPF.exe
├─ CAD-Avalonia.exe
├─ OcctDemo.Common.dll
├─ OcctNet*.dll
├─ OcctNative.dll
├─ 必需的 OCCT 7.9 Runtime
└─ 三个应用共享的 .NET/Avalonia Runtime
```

发布器只复制 Bridge 实际使用的 OCCT Toolkit 及其必要传递依赖；不会再递归复制整个 OCCT/3rdparty SDK，因此 Qt、VTK、Tcl/Tk、Draw/Test 和 Debug DLL 不进入正式 Demo 包。STEP/IGES 所需的少量 CAF/XCAF DLL 属于 OCCT Toolkit 自身传递运行时依赖，并不表示 Demo 或 Bridge 使用 OCAF/XDE 文档架构。

默认仍为 self-contained 单目录包；如果目标机器已安装 .NET 10 Desktop Runtime，可进一步减小体积：

```powershell
.\publish.ps1 all Release -FrameworkDependent -Zip
```

Demo 的 `publish.ps1` 只发布应用，不生成或重新编译 Bridge。

## 项目结构

```text
dist/win-x64/               main 发布的已验证 Binary SDK
src/OcctDemo.Common/        共享 Demo 行为与产品元数据
src/OcctDemo.WinForms/      WinForms Demo
src/OcctDemo.Wpf/           WPF Demo
src/OcctDemo.Avalonia/      Avalonia 12.1.0 Demo
assets/previews/            中英文标准截图
docs/README.md              Demo 维护规则
OcctDemo.sln                Demo-only Solution
build.ps1                   Demo 构建入口
run.ps1                     本地运行入口
publish.ps1                 Demo 应用发布入口
```

## 依赖规则

- Demo 只引用 `dist/win-x64/OcctNet*.dll`，不引用 main `.csproj`；
- Demo 不包含 `src/OcctNative`、`src/OcctNet*`、Bridge tests、CMake producer 或 ABI producer 脚本；
- Bridge 技术文档与完整中英文 Managed + Native API Reference 统一维护在 `main/docs/zh-CN`、`main/docs/en-US`；
- Demo 调用不匹配时修改 Demo，不恢复已删除 Legacy Alias 或旧 Wrapper；
- 不使用 GitHub Actions 完成构建、验证或分支同步。

## Native 启动排查

本地 `run.ps1` 运行出现 `DllNotFoundException` 或 Win32 126 时检查：

```text
应用目录/OcctNative.dll
%OCCT_ROOT%/win64/vc14/bin/TKernel.dll
%OCCT_ROOT%/3rdparty-vc14-64/**/bin/所需运行库
```

正式 `publish.ps1` 输出已经把所需 Native Runtime 放到三个 EXE 的共享目录，不要求目标机器保留完整 OCCT SDK。

Avalonia 仍通过 Windows 子 HWND 承载 Native Viewer，因此三套 Demo 都是 Windows x64 应用。

## Author

**zly258**  
zhangly1403@gmail.com

## 许可证

OcctCSharpBridge 使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。
