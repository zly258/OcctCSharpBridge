# 快速开始

OcctCSharpBridge 2.6 面向 **Windows x64**、**.NET 8** 和 **Open CASCADE Technology 7.9.0**。核心 Managed API 有两个入口：

- `OcctModelingSession`：Headless 建模、拓扑、分析、网格、修复和文件交换；
- `OcctEngine`：AIS/Viewer、相机、选择、显示、注释和交互式 OCCT 对象。

WinForms、WPF、Avalonia 只是可选 Viewport Host。Document、Command、Tool 等 CAD 应用层职责不属于 Bridge，见 [架构边界](ARCHITECTURE_BOUNDARIES.zh-CN.md)。

## 1. 构建 Managed SDK

```powershell
.\build.ps1 managed Release
```

不需要 OCCT SDK。会构建：

```text
OcctNet
OcctNet.WinForms
OcctNet.Wpf
OcctNet.Avalonia
```

Avalonia Host 当前仍是 Windows HWND Adapter。

## 2. OCCT SDK 与 Runtime

Native 构建约定默认 OCCT 根目录：

```text
D:\tools\occt-vc144-64
```

默认目录存在时可以直接执行：

```powershell
.\build.ps1 all Release
```

如果 SDK 在其它位置：

```powershell
.\build.ps1 all Release -OcctRoot "E:\SDK\occt-7.9.0"
```

运行应用时，如果 Native Runtime 没有 app-local 部署，可在创建第一个 Engine/ModelingSession 前显式配置：

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: @"D:\workspace\OcctCSharpBridge\build\native\bin\Release");
```

`OCCT_ROOT`、`CASROOT` 和 `OCCT_BRIDGE_NATIVE_DIR` 仍受支持。部署排查使用：

```csharp
Console.WriteLine(OcctRuntime.GetDiagnosticReport());
```

## 3. Headless 建模

```csharp
using var model = new OcctModelingSession();

var box = model.MakeBox(100, 80, 60);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    radius: 12,
    height: 80);

var result = model.Cut(box, hole);
var bounds = model.GetShapeBounds(result.Shape);
model.Triangulate(result.Shape);
var mesh = model.GetShapeMeshData(result.Shape);
model.ExportStep(result.Shape, @"D:\temp\part.step");
```

`OcctModelShape` 与创建它的 Session 强绑定；不要通过裸 ID 伪造 Shape。

## 4. 交互式 Viewer

直接使用 `OcctEngine` 时需要原生窗口句柄：

```csharp
using var engine = new OcctEngine();
engine.Initialize(hwnd);
engine.SetView(OcctViewOrientation.Isometric);
engine.SetProjection(OcctProjectionType.Orthographic);
engine.FitAll();
```

应用通常直接使用对应 Host：

```text
WinForms  → OcctViewportControl
WPF       → OcctWpfViewport
Avalonia  → OcctAvaloniaViewport   (Windows HWND)
```

不要在每个业务项目重复实现 OCCT HWND 生命周期、框选和基本 Viewer 输入连接。

## 5. Headless Shape 显示到 Viewer

```csharp
using var model = new OcctModelingSession();
var shape = model.MakeBox(100, 80, 60);

using var engine = new OcctEngine();
engine.Initialize(hwnd);
var displayed = engine.Display(model, shape, fit: true);
```

显示后的 `OcctShape` 属于目标 `OcctEngine`；原始 `OcctModelShape` 仍属于 ModelingSession。

## 6. 校验

不需要 OCCT SDK 的完整 Managed 门禁：

```powershell
.\build.ps1 ci Release
```

它覆盖静态契约、四个 Managed SDK、Managed 回归、公共 API 签名快照、Smoke 编译和 NuGet 包校验。

真实 Native 发布门禁：

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

只有后者能证明本机 OCCT 7.9.0 的 C++ 编译/链接、DLL 加载和真实几何/拓扑执行。

Managed 包与 Native Runtime 的关系见 [打包与运行时部署](PACKAGING.zh-CN.md)。
