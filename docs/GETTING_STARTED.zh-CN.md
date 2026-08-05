# 快速开始

本指南面向可复用的 `main` 分支。需要完整桌面应用时，请切换到 `demo` 分支。

## 1. 环境要求

- Windows 10/11 x64
- Visual Studio 2022，安装“使用 C++ 的桌面开发”
- .NET 8 SDK
- CMake 3.21+
- MSVC x64 版本的 Open CASCADE Technology 7.9.0

典型 OCCT 目录：

```text
D:\tools\occt-vc144-64
├─ inc
├─ win64\vc14\lib
├─ win64\vc14\bin
├─ 3rdparty-vc14-64
└─ src
```

项目会检查 `Standard_Version.hxx`，其他 OCCT 版本会被拒绝。

## 2. 配置运行时

可以设置环境变量：

```powershell
$env:OCCT_ROOT = "D:\tools\occt-vc144-64"
```

也可以在创建任何会话之前配置：

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: AppContext.BaseDirectory);
```

`OcctNative.dll` 会从应用目录和 `OCCT_BRIDGE_NATIVE_DIR` 查找。其依赖的 OCCT 与第三方 DLL 必须位于应用目录或 `PATH` 中。

## 3. 构建

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

`validate` 校验 C 声明、C++ 定义和 C# P/Invoke 名称；`smoke` 验证无窗口建模、OCAF 事务、持久化、XDE 元数据和 Shape 转移。

## 4. 第一个 Viewer 程序

`OcctEngine` 需要有效的 Windows HWND。控件创建句柄后再初始化，宿主尺寸变化时调用 `Resize()`。

```csharp
using OcctNet;
using System.Drawing;

using var engine = new OcctEngine();
engine.Initialize(hwnd);
engine.SetGradientBackground(Color.White, Color.LightSteelBlue);
engine.SetTriedronVisible(true);
engine.SetViewCubeVisible(true);

var box = engine.MakeBox(100, 80, 60);
engine.SetColor(box, Color.SteelBlue);
engine.SetMaterial(box, OcctMaterial.Plastified);

// 创建 Shape 不会改变相机。
engine.SetView(OcctViewOrientation.Isometric);
engine.FitAll();
```

不要为每个命令重复创建和销毁 `OcctEngine`。通常一个视口对应一个长期存在的 Engine。

## 5. 一次创建多个对象

一个逻辑命令创建或修改多个显示对象时，应使用批量作用域：

```csharp
using (engine.BeginDisplayBatch())
{
    var basePlate = engine.MakeBox(200, 120, 12);
    var column1 = engine.MakeCylinder(15, 100, 30, 30, 12);
    var column2 = engine.MakeCylinder(15, 100, 170, 90, 12);

    engine.SetColor(basePlate, Color.SlateGray);
    engine.SetColor(column1, Color.Goldenrod);
    engine.SetColor(column2, Color.Goldenrod);

    engine.FitAll(); // 可选，并且必须显式调用
}
```

不调用 `FitAll()` 时，批次结束只进行一次 Redraw，并保持当前相机。

## 6. 第一个 Headless 程序

`OcctModelingSession` 不初始化 OpenGL，也不需要窗口。

```csharp
using var model = new OcctModelingSession();

var body = model.MakeBox(100, 80, 60);
var cutter = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    12,
    80);

var operation = model.Cut(body, cutter);
if (!operation.Succeeded)
    throw new InvalidOperationException(operation.Report);

var result = operation.Shape;
if (!model.IsValid(result))
    throw new InvalidOperationException("布尔结果无效。");

var bounds = model.GetBounds(result);
var mass = model.GetVolumeProperties(result);
model.ExportStep(result, @"D:\output\result.step");
```

不再需要注册 Shape 时，应释放 Headless Session。

## 7. 将 Headless Shape 显示到 Viewer

```csharp
using var model = new OcctModelingSession();
var source = model.MakeBox(100, 80, 60);

var displayed = engine.AddShape(model, source);
engine.SetColor(displayed, Color.CornflowerBlue);
engine.FitAll();
```

Shape 会被复制，Viewer 不会持有 Headless 注册表中的原生指针。

## 8. 第一个 OCAF/XDE 文档

```csharp
using var model = new OcctModelingSession();
var body = model.MakeBox(100, 80, 60);

using var document = new OcafDocument(OcafDocumentFormats.BinaryXde)
{
    UndoLimit = 20
};

using (var command = document.BeginCommand())
{
    var label = document.AddShape(model, body);
    document.SetName(label, "Housing");
    document.SetComment(label, "Main equipment body");
    document.SetColor(label, OcafColorType.Surface,
        new OcafColor(0.2, 0.45, 0.8));
    command.Commit();
}

document.SaveAs(@"D:\output\housing.xbf");
```

未提交的命令应按 API 约定 Abort 或释放。撤销重做使用文档级 Undo/Redo，不要保存原生 Label 指针。

## 9. 异常处理

原生错误会转换为 `OcctException` 或算法结果对象。昂贵算法执行前应校验输入，算法提供 Report 时应读取并记录。

```csharp
try
{
    var result = engine.MakeBox(100, 80, 60);
}
catch (OcctException ex)
{
    logger.LogError(ex, "OCCT 操作失败");
}
```

## 10. 线程约束

Viewer 操作应在拥有 HWND 的 UI 线程执行，不要从多个线程并发调用同一个 `OcctEngine`。后台几何计算可使用独立 `OcctModelingSession`，完成后再切回 UI 线程复制到 Viewer。

## 下一步

- [Viewer 与显示刷新](VIEWER_AND_DISPLAY.zh-CN.md)
- [部署与运行时目录](DEPLOYMENT.zh-CN.md)
- [API 覆盖说明](API_COVERAGE.md)
- [OCAF/XDE 覆盖说明](OCAF_COVERAGE.md)
