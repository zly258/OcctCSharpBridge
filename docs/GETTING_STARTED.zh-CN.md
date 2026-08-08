# 快速开始

OcctCSharpBridge 2.6 面向 **Windows x64**、**.NET 8** 和 **Open CASCADE Technology 7.9.0**。Managed API 主要分为两个入口：

- `OcctModelingSession`：无界面建模、拓扑、几何分析、三角网格、修复和文件交换。
- `OcctEngine`：AIS/Viewer、相机、选择、显示、注释和交互式 CAD 操作。

## 1. 构建 Managed SDK

```powershell
.\build.ps1 managed Release
```

该步骤不需要安装 OCCT。Managed 程序集版本统一读取 `bridge-contract.json`。

## 2. 配置 OCCT 运行时

如果运行时没有与应用程序一起部署，请在创建第一个 Engine 或 ModelingSession 前配置：

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: @"D:\workspace\OcctCSharpBridge\build\native\bin\Release");
```

正式发布包优先使用应用目录中的 `OcctNative.dll`。同时支持 `OCCT_ROOT`、`CASROOT` 和 `OCCT_BRIDGE_NATIVE_DIR`。

如果出现 Win32 126 等部署问题，建议把运行时诊断信息写入日志：

```csharp
Console.WriteLine(OcctRuntime.GetDiagnosticReport());
```

## 3. 无界面建模

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
var mesh = model.GetShapeMesh(result.Shape);

model.ExportStep(result.Shape, @"D:\temp\part.step");
```

ModelingSession 返回的 Shape 与该 Session 强绑定。仅在确实需要持久化时保存 Native ID；恢复时使用 `GetShape` / `TryGetShape`，不要自行构造裸句柄。

## 4. 交互式 Viewer

`OcctEngine` 在执行 Viewer 操作前需要有效的原生窗口句柄：

```csharp
using var engine = new OcctEngine();
engine.Initialize(hwnd);
engine.SetView(OcctViewOrientation.Isometric);
engine.SetProjection(OcctProjectionType.Orthographic);
engine.FitAll();
```

WinForms/WPF 项目优先使用 `OcctNet.WinForms` 和 `OcctNet.Wpf` 中的可复用 Viewport Host，不要在业务项目重复实现 HWND 托管逻辑。

## 5. Headless 模型显示到 Viewer

```csharp
using var model = new OcctModelingSession();
var shape = model.MakeBox(100, 80, 60);

using var engine = new OcctEngine();
engine.Initialize(hwnd);
var displayed = engine.Display(model, shape, fit: true);
```

显示后的 `OcctShape` 属于目标 `OcctEngine`；原始 `OcctModelShape` 仍属于原 ModelingSession。

## 6. 发布前验证

Managed 校验：

```powershell
.\build.ps1 ci Release
```

在安装了 OCCT 7.9.0 的机器执行 Native 校验：

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Native Smoke 覆盖 Bridge/ABI 一致性、建模、三角网格、拓扑、BREP 往返和 STEP 往返。

Managed 包和 Native Runtime 的部署关系见 [PACKAGING.zh-CN.md](PACKAGING.zh-CN.md)。
