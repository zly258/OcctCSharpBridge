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

`bridge-contract.json` 与 `main` 共享，是 Bridge/ABI/OCCT/.NET/API 元数据的权威来源，其中同时记录 `main` 使用的 Core SDK 和 Avalonia Demo 所需的较新 SDK。`global.json` 有意按分支设置：`main` 保持 .NET SDK 8.0.423，`demo` 使用 10.0.302；目标框架和语言级别仍统一为 .NET 8 与 C# 12。

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
  <tr><th>WinForms</th><th>WPF</th></tr>
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
| `ci` | 执行普通 CI 托管构建，包括三个 Demo 和 Smoke 项目编译 | 否 |
| `native` | 构建 `OcctNative.dll` | 是 |
| `winform` / `wpf` / `avalonia` | 构建指定可运行 Demo | 是 |
| `smoke` | 构建 Native 并执行真实 OCCT 建模场景 | 是 |
| `all` | 构建 Native、三个 Demo 和 Smoke 项目 | 是 |

```powershell
.\build.ps1 validate Release
.\build.ps1 ci Release
.\build.ps1 all Release
.\build.ps1 smoke Release
```

普通 GitHub Actions 直接调用 `build.ps1 ci Release`。`smoke` 更强，它需要真实 OCCT SDK，因为它会实际加载 Native Bridge 并执行 OCCT 建模操作。

## 运行

`run.ps1` 只启动已经构建好的程序，不会隐式重新编译：

```powershell
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

## 发布

`publish.ps1` 已正式支持 **WinForms、WPF、Avalonia**，`all` 会同时发布三个应用：

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 winform Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 wpf Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
.\publish.ps1 avalonia Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

发布过程会递归解析 `OcctNative.dll`、OCCT TK 模块、第三方库和 Visual C++ 运行库的完整 PE 依赖闭包；当实际依赖时也会纳入 `vcomp140.dll`。解析完成后的 Native DLL 会**复制到每个 EXE 同目录**，不再只依赖包根目录的兄弟 `runtime` 文件夹；运行时解析器也改为优先使用应用目录。

发布包必须连续通过两层校验：先执行 `dumpbin` 静态依赖闭包检查，再启动独立 PowerShell 子进程，以受限 DLL 搜索路径实际执行 `LoadLibraryExW`。如果包在干净电脑上会出现 `Win32 126`，发布阶段就会直接失败。顶层 `runtime` 目录继续作为依赖清单和诊断用的基准副本，而 `apps\winform`、`apps\wpf`、`apps\avalonia` 均可直接运行。

发布包还包含必要 OCCT 资源、`package-contract.json`、`native-dependencies.txt` 和可获取的许可证信息。只有目标电脑已安装匹配的 .NET 8 Desktop Runtime 时才建议使用 `-FrameworkDependent`。

## 测试说明

`tests` 下的 PowerShell 文件主要是接口契约和静态回归检查。已经删除两份脱离当前构建入口、且与现有检查重复的旧脚本。`tests/OcctNet.Smoke` 则明确保留，因为它是功能级 Native 集成测试，是目前唯一会真实加载 OCCT 并执行建模操作的测试层。

建议使用频率：

- 修改 API/源码后执行 `build.ps1 validate`。
- 提交前执行 `build.ps1 ci`。
- 修改 Native、建模或运行时加载逻辑后执行 `build.ps1 smoke`。
- 对外发包前执行 `publish.ps1`；其内部会额外完成便携包 Native 加载探针。

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
- `Unable to load OcctNative.dll ... Win32 126`：不要继续分发旧发布包。使用当前 `publish.ps1` 重新发布；应用 EXE 同目录必须带有 `OcctNative.dll` 及其依赖闭包，发布时的受限 LoadLibrary 探针会提前拦截缺失依赖。
- Avalonia Analyzer/编译器版本不匹配：使用本分支 `global.json` 固定的 SDK，不要通过降级目标框架或关闭 Analyzer 来绕过。
- Avalonia 启动异常：查看 `src\CadAvalonia\bin\x64\<Configuration>\net8.0-windows\CAD-Avalonia.log`。

## 许可证

项目使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 与第三方组件仍遵循各自许可证。

## 联系方式

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
