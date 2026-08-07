# OcctCSharpBridge

[English](README.md) · [桌面 Demo](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 是面向 Windows x64 的 **Open CASCADE Technology 7.9.0 → .NET 8** 桥接项目。`main` 分支保持纯净可复用，只包含原生 C++ 桥接、稳定 C ABI、类型安全的 C# 封装、可选 UI 视口宿主、接口校核与建模 Smoke Test；完整 WinForms/WPF/Avalonia 应用位于 `demo` 分支。

桥接层不使用 OCAF/XDE 作为应用文档机制。Document、Entity、Command、Undo/Redo、JSON 持久化、Tool、捕捉和动态预览等职责由上层应用实现。

## 环境要求

- Windows x64
- Visual Studio 2022 / MSVC v143 兼容工具链
- .NET 8 SDK
- CMake 3.21 或更高
- Open CASCADE Technology **7.9.0**，VC14 x64 目录结构
- PowerShell 5.1+ 或 PowerShell 7+

典型 OCCT 目录：

```text
D:\tools\occt-vc144-64\
├─ inc\
├─ win64\vc14\bin\
├─ win64\vc14\lib\
└─ 3rdparty-vc14-64\
```

## 从克隆开始

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch main
```

建议在当前 PowerShell 会话设置 OCCT 路径：

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

也可以不设置环境变量，在需要原生 OCCT 的命令后显式传入 `-OcctRoot`。

## 目录结构

```text
src/OcctNative         C++17 原生桥接与稳定 C ABI
src/OcctNet            不依赖 UI 的类型安全 .NET 封装
src/OcctNet.WinForms   可选 WinForms HWND 视口宿主
src/OcctNet.Wpf        可选 WPF 视口宿主
tests                   接口契约校核与真实原生建模 Smoke Test
docs                    中英文接口清单
build.ps1               校核、构建与 Smoke Test 统一入口
```

核心会话：

- `OcctEngine`：交互式 Viewer/AIS、相机、投影、显示属性、对象身份、选择、变换、注释和屏幕/世界坐标转换。
- `OcctModelingSession`：无窗口建模、拓扑、构造算法、网格、分析、修复、历史和工程文件交换。

`OcctViewportControl` 位于 `OcctNet.WinForms`；`OcctWpfViewport` 位于 `OcctNet.Wpf`。它们是可复用宿主控件，不是完整 CAD 应用。

## `build.ps1` 怎么用

统一格式：

```powershell
.\build.ps1 <target> <configuration> [-OcctRoot <path>]
```

`main` 支持：

| Target | 作用 | 是否需要 OCCT SDK |
| --- | --- | --- |
| `validate` | 校核版本、API 组织、Native/PInvoke、UI Host 等契约，不编译 OCCT | 否 |
| `managed` | 构建 `OcctNet`、WinForms Host、WPF Host | 否 |
| `native` | 使用 CMake/MSVC 构建 `OcctNative.dll` | 是 |
| `smoke` | 构建并执行真实 OCCT 建模 Smoke 场景 | 是 |
| `all` | 构建原生桥接与全部可复用托管项目 | 是 |

配置可用 `Debug`、`Release`、`RelWithDebInfo`。

### 只做接口校核

修改公开 API、C ABI、P/Invoke 或目录组织后优先执行：

```powershell
.\build.ps1 validate Release
```

### 只构建托管封装

```powershell
.\build.ps1 managed Release
```

### 只构建原生桥接

```powershell
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
```

### 完整构建

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

如果已经设置 `$env:OCCT_ROOT`：

```powershell
.\build.ps1 all Release
```

### 运行真实建模 Smoke Test

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

这一步会真正加载 Native Bridge 并执行 OCCT 建模，比纯编译更能发现运行时问题。

## 运行桌面 Demo

`main` 不放完整 Demo。需要界面示例时切换到 `demo`：

```powershell
git switch demo
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
.\build.ps1 all Release
.\run.ps1 winform
.\run.ps1 wpf
.\run.ps1 avalonia
```

`demo` 分支 README 会详细说明 `build.ps1`、`run.ps1` 和 `publish.ps1`。

## 在其他项目中引用

开发期可直接使用项目引用：

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- 可选 WinForms Host -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <!-- 可选 WPF Host -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
</ItemGroup>
```

部署时应保证 `OcctNet.dll`、所选 Host 程序集、`OcctNative.dll`、OCCT Runtime DLL 和第三方运行库来自同一兼容构建，不要混用不同提交生成的 Native/Managed 文件。

## 兼容性契约

- OCCT：严格 `7.9.0`
- .NET：`8.0`
- 平台：Windows x64
- Bridge：`2.5.0`
- Native ABI：`2`
- `OcctBridgeInfo` 用于运行时 ABI 校验
- 原生会话持有可变状态，同一实例应由单一应用线程使用

## 接口清单

- [中文接口清单](docs/API_COVERAGE.zh-CN.md)
- [English API inventory](docs/API_COVERAGE.md)

`build.ps1 validate` 会主动阻止声明、P/Invoke、调用约定、源码组织或接口清单未同步的提交。

## 常见问题

**提示 `OCCT_ROOT is not configured`**  
设置 `$env:OCCT_ROOT`，或在命令中传 `-OcctRoot`。

**找不到 `TKernel.lib` / `TKernel.dll`**  
检查 OCCT 是否为 7.9.0，并确认存在 `win64\vc14\lib` 与 `win64\vc14\bin`。

**托管项目能编译，但运行时报 Native DLL 加载失败**  
`managed` 不负责完整部署 OCCT Runtime。请构建 native/all，并确保对应 OCCT 与第三方 DLL 可被应用找到。

**需要可直接运行的界面示例**  
请使用 `demo` 分支，不要把应用层代码重新放回 `main`。

## 许可证

项目使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。Open CASCADE Technology 与第三方组件仍遵循各自许可证。
