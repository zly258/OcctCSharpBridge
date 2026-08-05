# OCCT 7.9.0 C# 工程级封装

[English](README.md) · [文档索引](docs/README.zh-CN.md) · [WinForms/WPF Demo 分支](https://github.com/zly258/OcctCSharpBridge/tree/demo)

OcctCSharpBridge 通过 **C++17 原生 DLL + 稳定 C ABI + .NET 8 P/Invoke** 封装 Open CASCADE Technology **7.9.0**。`main` 分支只保留可复用 SDK；`demo` 分支增加完整 WinForms、WPF、共享命令层、API 中心、综合场景和一键发布脚本。

## 设计目标

- 不向 C# 暴露 OCCT 的 `Handle(...)`、Label 指针和内部实现类。
- 提供稳定的工程工作流 API，而不是逐个机械映射所有 C++ 类。
- Viewer、无窗口建模、OCAF/XDE 文档分别管理原生生命周期。
- **创建或修改 Shape 时不自动调用 `FitAll`，保持用户当前相机不变。**
- 多对象创建和属性修改可通过嵌套批量作用域统一刷新。
- 严格绑定 OCCT 7.9.0，避免 OCAF、TNaming、XDE 和持久化版本漂移。

## 架构

```text
业务程序
├─ OcctEngine              Viewer/AIS、选择、相机、显示和标注
├─ OcctModelingSession     无窗口几何、拓扑、算法、修复和网格
└─ OcafDocument            OCAF/TDF/TNaming/XDE、装配、属性和持久化
          ↓
OcctNet（.NET 8，Windows x64）
          ↓ P/Invoke / 稳定 C ABI
OcctNative（C++17 DLL）
          ↓
Open CASCADE Technology 7.9.0
```

三个高级会话拥有独立原生注册表。Shape 在不同注册表之间按值复制，原生 `Handle(...)`、`TopoDS_Shape*`、`TDF_Label*` 和持久化驱动对象不会跨越 ABI。

## 应该使用哪个入口

| 需求 | 使用接口 |
|---|---|
| 交互式三维视口、拾取、相机、颜色和尺寸 | `OcctEngine` |
| 后台、服务端或无窗口建模 | `OcctModelingSession` |
| 参数化文档、产品结构、属性、持久化和撤销重做 | `OcafDocument` |
| 后台生成 Shape 后显示到视口 | `OcctModelingSession` + `OcctEngine.AddShape(...)` |
| STEP/IGES 中保留装配、名称、颜色、图层和材料 | `OcafDocument` 的 STEPCAF/IGESCAF API |

## Viewer 快速开始

```csharp
using OcctNet;

OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: AppContext.BaseDirectory);

using var engine = new OcctEngine();
engine.Initialize(viewportHandle);

var box = engine.MakeBox(100, 80, 60);
engine.SetColor(box, Color.SteelBlue);

// 创建 Shape 后不会自动调整相机。
// 业务确实需要时再显式调用。
engine.FitAll();
```

### 多对象高效显示

```csharp
using (engine.BeginDisplayBatch())
{
    var box = engine.MakeBox(100, 80, 60);
    var cylinder = engine.MakeCylinder(20, 80, 150, 0, 0);
    engine.SetMaterial(box, OcctMaterial.Plastified);
    engine.SetColor(cylinder, Color.OrangeRed);

    // 可选。不调用时保持原相机。
    engine.FitAll();
}
```

批量期间，Display、Redisplay、颜色、材质、透明度、显隐、删除等操作只登记待刷新状态。最外层作用域结束时统一 Redraw。批次支持嵌套。

## Headless 快速开始

```csharp
using var model = new OcctModelingSession();
var body = model.MakeBox(100, 80, 60);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    12,
    80);
var result = model.Cut(body, hole);

if (!result.Succeeded || !model.IsValid(result.Shape))
    throw new InvalidOperationException(result.Report);

model.ExportStep(result.Shape, @"D:\output\result.step");
```

## OCAF/XDE 快速开始

```csharp
using var model = new OcctModelingSession();
var body = model.MakeBox(100, 80, 60);

using var document = new OcafDocument(OcafDocumentFormats.BinaryXde)
{
    UndoLimit = 20
};

using (var command = document.BeginCommand())
{
    var product = document.AddShape(model, body);
    document.SetName(product, "Housing");
    document.SetColor(product, OcafColorType.Surface,
        new OcafColor(0.2, 0.45, 0.8));
    var layer = document.AddLayer("Equipment");
    document.SetLayer(product, layer);
    document.SetMaterial(product, "Steel", "Structural steel", 7.85);
    command.Commit();
}

document.SaveAs(@"D:\output\assembly.xbf");
document.ExportStep(@"D:\output\assembly.step");
```

## 能力概览

| 模块 | 主要能力 |
|---|---|
| Viewer/AIS | HWND 初始化、相机、正交/透视投影、视图立方体、坐标轴、光照、抗锯齿、材质、颜色、透明度、文字和尺寸 |
| 选择 | 对象与子拓扑过滤、点选、矩形框选、稳定选择状态、OCCT Overlay 框选指示框 |
| 几何 | 点、线、圆弧、圆、椭圆、Bezier、B-spline、Wire、Face 和常用实体 |
| 特征 | 布尔、Splitter、拉伸、旋转、扫掠、放样、圆角、倒角、偏移、厚实体、钻孔和变换 |
| 分析 | 包围盒、质量属性、拓扑统计、距离、投影、射线、点内外分类、有效性和算法报告 |
| 网格与修复 | 显式网格、面三角网读取、BRepCheck 和 ShapeFix |
| OCAF/TDF | 文档、Label、事务、Undo/Redo、标量/引用/数组/位置/Shape 属性、变量、表达式和关系式 |
| TNaming | Generated/Modify/Delete/Select、NamedShape 历史和 Selector 求解 |
| XDE | 自由 Shape、装配、组件、实例位置、名称、颜色、可见性、图层、材料和验证属性 |
| 数据交换 | BREP/STL/STEP/IGES 纯 Shape，以及保留元数据的 STEPCAF/IGESCAF |

## 版本约束

本仓库严格要求 **OCCT 7.9.0**：

1. CMake 解析 `Standard_Version.hxx`，版本不符时停止配置。
2. 原生代码包含编译期版本断言。
3. 托管 OCAF 初始化时再次检查已加载桥接 DLL。

不得静默替换为 7.8、7.9.1/7.9.3 或 8.x。

## 构建

环境要求：

- Windows 10/11 x64
- Visual Studio 2022，安装“使用 C++ 的桌面开发”
- .NET 8 SDK
- CMake 3.21+
- OCCT 7.9.0 VC++ x64 SDK

```powershell
Set-ExecutionPolicy -Scope Process Bypass

# 校验 C 头文件、C++ 实现和 P/Invoke 一致性。
.\build.ps1 validate Release

# 构建可复用托管程序集。
.\build.ps1 managed Release

# 构建原生桥接并运行 Smoke Test。
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## 项目引用

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

部署时，将 `OcctNative.dll` 放在应用目录，或通过 `OCCT_BRIDGE_NATIVE_DIR` 指定。OCCT 与第三方 DLL 必须位于应用目录或 `PATH` 中；涉及数据交换和持久化时，还需通过 `CASROOT`/`OCCT_ROOT` 提供 OCCT 资源目录。

`demo` 分支中的 `publish.ps1` 会生成 Windows x64 自包含包，包含 .NET 运行时、原生桥接、OCCT DLL、第三方 DLL、资源目录、启动脚本、清单和许可证，目标电脑无需安装开发环境。

## 文档

- [文档索引](docs/README.zh-CN.md)
- [快速开始](docs/GETTING_STARTED.zh-CN.md)
- [Viewer、选择与显示刷新](docs/VIEWER_AND_DISPLAY.zh-CN.md)
- [部署与运行时目录](docs/DEPLOYMENT.zh-CN.md)
- [完整 API 覆盖说明](docs/API_COVERAGE.md)
- [OCAF/XDE 覆盖说明](docs/OCAF_COVERAGE.md)
- [OCAF 扩展 API](docs/OCAF_EXTENDED_API.md)

## 边界与扩展原则

本项目不会跨 C ABI 暴露原生 Handle、Label/Attribute 指针、具体 TDF Delta 子类、持久化驱动内部类或任意 C++ 回调。新增能力应继续以稳定结构体、ID、字符串和数组形式扩展，不破坏既有 ABI。

高级 GD&T、View、Note、Clipping Plane 和 PBR Visual Material 当前可通过区段 Label 与通用属性 JSON 检查，后续可按模块增加强类型 CRUD。

## 许可证

本项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT、Microsoft 运行库及第三方组件仍适用各自许可证和再分发条款。
