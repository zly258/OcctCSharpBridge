# OcctCSharpBridge

[English](README.md) · [文档索引](docs/INDEX.zh-CN.md) · [架构边界](docs/ARCHITECTURE_BOUNDARIES.zh-CN.md) · [桌面 Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 是面向 Windows x64 的 **Open CASCADE Technology 7.9.0 → .NET 10** 桥接项目。`main` 只保留可复用 OCCT Native/C# 封装、WinForms/WPF/Avalonia 视口宿主、契约测试、Native Smoke 场景和 Managed SDK 打包；完整 CAD 应用与上层 CAD 框架位于 `demo`。

Bridge **2.6.0 / Native ABI 3** 继续坚持一个边界：**Bridge 提供 OCCT 能力和 UI Viewport Adapter，不提供应用级 CAD Framework。** OCAF/XDE、Document、Feature/Entity、Command、Tool、Undo/Redo、Snap/Grip、JSON 项目持久化和产品 UI 都不进入 `main`。

## 环境

- Windows x64
- Visual Studio 2022 / MSVC v143 兼容工具链
- .NET SDK **10.0.302**（`global.json`）
- Framework-dependent 桌面应用需要 .NET Desktop Runtime **10.x**
- 目标框架 **`net10.0-windows`**
- C# **14.0**
- CMake 3.21+
- Open CASCADE Technology **7.9.0**，VC14 x64 目录结构
- PowerShell 5.1+ 或 PowerShell 7+

.NET 10 运行时遵循正常的补丁版本前滚规则，不要求使用方固定到某个 `10.0.x` 补丁版本；例如 `10.0.10` Runtime/Desktop Runtime 可以满足本项目运行要求。

约定的默认 OCCT 根目录是：

```text
D:\tools\occt-vc144-64\
├─ inc\
├─ win64\vc14\bin\
├─ win64\vc14\lib\
└─ 3rdparty-vc14-64\
```

如果 OCCT 位于该目录，`native` / `smoke` / `all` 可以不设置 `OCCT_ROOT`。其它位置使用 `$env:OCCT_ROOT` 或 `-OcctRoot` 覆盖。`validate` / `managed` / `pack` / `ci` 不需要 OCCT SDK。

## main 仓库结构

```text
bridge-contract.json     Bridge / ABI / OCCT / .NET / API 契约
src/OcctNative           C++17 OCCT Bridge 与稳定 C ABI
src/OcctNet              核心 C# 封装
src/OcctNet.WinForms     WinForms HWND 视口宿主
src/OcctNet.Wpf          WPF 视口宿主
src/OcctNet.Avalonia     Avalonia + Windows HWND 视口宿主
tests                    契约检查、Managed 回归、Native Smoke
docs                     API、架构、部署与诊断文档
build.ps1                validate/build/pack/smoke 统一入口
```

`main` 不应该出现 `OcctDemo.*`、完整 CAD 应用、DocumentManager、CommandBus、ToolManager 等上层实现。具体原则见 [架构边界](docs/ARCHITECTURE_BOUNDARIES.zh-CN.md)。

## Managed 入口

### `OcctEngine`

交互式 AIS/Viewer/Object façade，负责 View、Camera、Selection、显示对象、外观、变换、标注和 Viewer 场景中的几何操作。

### `OcctModelingSession`

Headless 建模 façade，负责几何构造、拓扑、Boolean/Feature 算法、分析、网格、Healing、Operation History 与 STEP/IGES/BREP/STL 交换。

两者存在部分同名几何操作是有意设计：一个管理 Viewer/AIS 生命周期，一个管理 Headless Shape 生命周期，不为了形式上的 DRY 合并成巨型类。

## UI Host

`main` 正式提供三种可复用宿主：

- `OcctNet.WinForms`
- `OcctNet.Wpf`
- `OcctNet.Avalonia`

Avalonia Host 当前通过 `NativeControlHost` 创建 Windows 子 HWND，因此仍是 **Windows x64 Host**，不表示 Native Bridge 已经支持 Linux/macOS。

WinForms 与 Avalonia 只共享框选方向/阈值、Hover/WorldPoint 节流和默认缩放倍率等无框架逻辑；窗口生命周期、DPI、Capture 和 Win32 子类化仍各自处理。

## API 与兼容契约

权威值来自 `bridge-contract.json`：

- Bridge：`2.6.0`
- Native ABI：`3`
- OCCT：严格 `7.9.0`
- .NET SDK：`10.0.302`
- Target Framework：`net10.0-windows`
- C#：`14.0`
- Native exports：`348`
- Managed P/Invoke declarations：`348`
- Public .NET types：`99`
- Compatibility .NET types：`1`
- Viewer API：`214`
- Modeling API：`134`

`OcctObject` 是唯一单独统计的 Bridge 2.5 兼容公共类型。2.x 不直接删除它，但也不继续增加新的 legacy API；新代码优先使用 owner-aware 的 `OcctShape` / `OcctText` / `OcctDimension` / `IOcctObject`。

Viewer 结构化 Selection Hit 与 `GetSubshapeAt()` 的运行时 Subshape Index 都不是 Persistent Naming，不能直接作为长期 Feature 拓扑引用。

完整接口分类见 [API 覆盖说明](docs/API_COVERAGE.zh-CN.md)。

## Headless 示例

```csharp
using var model = new OcctModelingSession();

var plate = model.MakeBox(100, 80, 10);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -5),
    OcctVector3d.UnitZ,
    8,
    20);

var cut = model.Cut(plate, hole);
var inspection = model.InspectShape(cut.Shape);
var mesh = model.GetShapeMeshData(cut.Shape);
model.ExportStep(cut.Shape, "plate.step");
```

Bridge 还提供 B-Spline 数据、解析/微分几何、投影/射线/点分类、批量 Edge/Face 分析、Free Bounds、OBB、Trim/Offset、Healing、Triangulation 和 Mesh Face 来源追溯等能力。

## 构建

```powershell
.\build.ps1 <target> <configuration> [-OcctRoot <path>]
```

| Target | 作用 | 需要 OCCT SDK |
|---|---|---|
| `validate` | 版本/API/目录/PInvoke/UI Host/包/分支边界契约 | 否 |
| `managed` | 构建 Core + WinForms + WPF + Avalonia | 否 |
| `pack` | 构建并校验 Managed NuGet / symbol package | 否 |
| `ci` | 契约 + Managed 构建/测试 + API 签名快照 + Smoke 编译 + 包校验 | 否 |
| `native` | CMake/MSVC 构建 `OcctNative.dll` | 是 |
| `smoke` | 构建并真实执行 OCCT Native 场景 | 是 |
| `all` | Native + 全部可复用 Managed Host | 是 |

没有本机 OCCT SDK 时：

```powershell
.\build.ps1 ci Release
```

默认 OCCT 目录存在时可直接：

```powershell
.\build.ps1 all Release
```

其它 OCCT 位置：

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

GitHub 云端没有本项目真实 OCCT SDK，因此 CI 负责静态契约、Managed 编译/测试、公共 API 签名快照、Smoke 源码编译和 NuGet 包校验。真正的 C++ 编译/链接、DLL Load 与几何/拓扑执行仍以本地 `smoke` 为准。

## NuGet

`main` 生成四套 Managed SDK 包：

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

```powershell
.\build.ps1 pack Release
```

包输出到 `artifacts/packages`。Managed NuGet 不捆绑 `OcctNative.dll`、OCCT `TK*.dll` 或 `runtimes/` Native Payload；应用必须部署与 Managed Bridge 匹配的 Native/OCCT Runtime。详见 [打包说明](docs/PACKAGING.zh-CN.md)。

## 引用

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- 三选一或按需引用 -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Avalonia\OcctNet.Avalonia.csproj" />
</ItemGroup>
```

## demo

完整 WinForms/WPF/Avalonia CAD 示例位于 `demo`：

```powershell
git switch demo
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

`demo` 使用明确命名的 `OcctDemo.Common` 编排层与三个 `OcctDemo.*` 应用。这些只属于参考应用，不属于 `OcctNet` 公共 API，也不应继续扩展成第二套可复用 CAD Framework。

## Runtime 诊断

`OcctRuntime.GetDiagnosticInfo()` 与 `GetDiagnosticReport()` 用于检查 app-local、配置路径、实际加载的 `OcctNative.dll` / `TKernel.dll`、进程架构和依赖状态，调用本身不会强制加载 Native。详见 [结构化 Runtime 诊断](docs/RUNTIME_DIAGNOSTICS.zh-CN.md)。

## License

项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 与第三方组件遵循各自许可证。

## 联系方式

Liaoyuan Zhang · [zhangly1403@gmail.com](mailto:zhangly1403@gmail.com)
