# OcctCSharpBridge Demo

[English](README.md) · [中文接口清单](docs/API_COVERAGE.zh-CN.md) · [主 SDK 分支](https://github.com/zly258/OcctCSharpBridge/tree/main)

`demo` 分支在与 `main` 同步维护的可复用桥接层之上，提供完整的 WinForms、WPF、Avalonia CAD 示例应用。Native/.NET 公共封装与契约元数据持续和 `main` 做一致性检查；应用界面、Demo 场景、运行脚本、发布工具和发布包校核仅保留在本分支。

桥接层不使用 OCAF/XDE 作为应用文档机制。Document、Entity、Command、Undo/Redo、JSON 持久化和 Tool 等应用层职责由上层程序实现。

## 工具链与契约

- Windows x64
- Open CASCADE Technology **7.9.0**，VC14 x64 目录结构
- 本分支使用 .NET SDK **10.0.302**，由 `global.json` 固定
- 目标框架仍保持 **`net8.0-windows`**
- C# 12.0
- CMake 3.21+
- Avalonia `12.1.0`
- Bridge `2.5.0`，Native ABI `2`

`bridge-contract.json` 与 `main` 共享，是 Bridge/ABI/OCCT/.NET/API 元数据的权威来源，其中同时记录 `main` 使用的 Core SDK 和 Avalonia Demo 所需的较新 SDK。`global.json` 因此有意按分支设置：`main` 保持 .NET SDK 8.0.423，`demo` 使用 10.0.302；目标框架和语言级别仍统一为 .NET 8 与 C# 12。`Wrapper Branch Sync` 继续严格比较共享源码与公共契约，但不再要求两个分支的 `global.json` 完全相同。

## 分层结构

- `OcctNet`：不依赖 UI 的 Viewer、建模、拓扑、几何、分析、网格、修复和文件交换接口。
- `OcctNet.WinForms`：基于原生 HWND 的 `OcctViewportControl`。
- `OcctNet.Wpf`：封装 WPF 事件、DPI 与原生尺寸同步的 `OcctWpfViewport`。
- `OcctNet.Avalonia`：Windows x64 下基于 `NativeControlHost` 和独立子 HWND 的原生宿主。
- `CadCommon`：三个桌面 Demo 共用的 Session/Command 应用层。
- `CadWinForms`、`CadWpf`、`CadAvalonia`：可直接运行的参考应用。

当前 Avalonia Viewer 仍基于 Windows HWND，本仓库暂不声明 Linux/macOS OCCT Viewer 支持。

## 界面预览

<table>
  <tr>
    <th>WinForms</th>
    <th>WPF</th>
  </tr>
  <tr>
    <td><img src="assets/previews/winform-demo-zh.webp" alt="OCCT CAD WinForms 中文界面" width="100%"></td>
    <td><img src="assets/previews/wpf-demo-zh.webp" alt="OCCT CAD WPF 中文界面" width="100%"></td>
  </tr>
</table>

Avalonia 与 WPF 共用 `CadSession` 和 `CadCommandCatalog`，覆盖模型创建、选择、模型树、属性、撤销重做、文件交换、标注、分析、视图与显示控制、示例、快捷键和中英文界面。

## 首次配置

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

OCCT 目录应包含 `inc`、`win64\vc14\lib`、`win64\vc14\bin`，以及可选的 `3rdparty-vc14-64`。

## 构建与校验

`build.ps1` 是统一构建入口：

| Target | 作用 | 是否需要 OCCT SDK |
| --- | --- | --- |
| `validate` | 只执行契约、源码和发布包规则校验 | 否 |
| `managed` | 构建 Core、WinForms/WPF/Avalonia Host 与 `CadCommon` | 否 |
| `ci` | 执行与 GitHub Actions 相同的托管构建，包括三个 Demo 和 Smoke 项目编译 | 否 |
| `native` | 构建 `OcctNative.dll` | 是 |
| `winform` / `wpf` / `avalonia` | 构建指定可运行 Demo | 是 |
| `smoke` | 构建 Native 并执行真实 OCCT 建模 Smoke | 是 |
| `all` | 构建 Native、三个 Demo 和 Smoke 项目 | 是 |

```powershell
# 最快的源码/API 契约校验
.\build.ps1 validate Release

# 在本地复现普通 GitHub Actions
.\build.ps1 ci Release

# 完整 Native + Demo 构建
.\build.ps1 all Release

# 最强的 Native 运行时校验
.\build.ps1 smoke Release
```

GitHub Actions 直接调用 `build.ps1 ci Release`，因此本地提交前检查与托管 CI 使用同一条托管构建路径，不再分别维护多套 `dotnet build` 命令。

## 运行

`run.ps1` 只启动已经构建好的程序，不会隐式重新编译：

```powershell
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

如果程序不是最新版本，应先构建对应 target。

## 发布

`publish.ps1` 当前正式输出 WinForms 和 WPF 的完整部署包。Avalonia 已纳入 build/run/CI，但暂未进入正式 publish target。

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

发布包包含应用程序、匹配版本的托管 Wrapper/Host、`OcctNative.dll`、递归解析的 OCCT/第三方运行库、必要 OCCT 资源、`package-contract.json` 和 `native-dependencies.txt`。只有目标电脑已经安装匹配的 .NET 8 Desktop Runtime 时才建议使用 `-FrameworkDependent`。

## Demo 覆盖的主要能力

- 点选、框选、方向框选、多选和子形选择
- 相机/视图状态、Z-up、Fit、屏幕/世界坐标转换、ViewCube 与 Triedron
- Shaded/Wireframe/Shaded with Edges，以及精度、材质和光照控制
- 稳定 ApplicationTag、变换、可见性、颜色、透明度和批量操作
- 基础体、特征、布尔、扫掠、放样、拓扑与几何查询
- 解析几何、微分几何、质量属性、距离/射线/投影和网格读取
- BRep 文字以及长度、角度、半径、直径标注
- STEP、IGES、BREP、STL 文件交换
- 中英文桌面界面

## 常见问题

- `OCCT_ROOT is not configured`：设置 `$env:OCCT_ROOT`，或给需要 Native 的 target 传 `-OcctRoot`。
- Native DLL 加载失败：使用正确 OCCT SDK 重新构建，并确保匹配的运行库存在。
- Avalonia Analyzer/编译器版本不匹配：使用本分支 `global.json` 固定的 SDK，不要通过降级目标框架或关闭 Analyzer 来绕过。
- Avalonia 启动异常：查看 `src\CadAvalonia\bin\x64\<Configuration>\net8.0-windows\CAD-Avalonia.log`。
- 修改 API/Host/Menu 后先执行 `build.ps1 validate`；提交前优先执行 `build.ps1 ci`；有真实 OCCT SDK 时再执行 `build.ps1 smoke`。

## 许可证

项目使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 与第三方组件仍遵循各自许可证。
