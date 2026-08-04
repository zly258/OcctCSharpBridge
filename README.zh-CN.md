# OCCT 7.9.0 C# 工程级封装

[English](README.md)

本仓库通过 **C++17 原生 DLL + 稳定 C ABI + .NET 8 P/Invoke** 封装 Open CASCADE Technology 7.9.0。`main` 分支只保留可复用封装；完整 WinForms/WPF 示例保存在 [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo) 分支。

## 架构

```text
业务程序
├─ OcctModelingSession      无窗口建模、查询、修复、网格和数据交换
└─ OcctEngine               AIS / V3d Viewer、选择、显示和标注
          ↓
OcctNet (.NET 8, x64)
          ↓ P/Invoke / C ABI
OcctNative (C++17)
          ↓
OCCT 7.9.0
```

`OcctModelingSession` 不需要 HWND，不创建 `AIS_InteractiveContext`，可用于后台建模、批处理、服务程序和单元测试。需要显示时，可将 Headless Shape 复制到现有 Viewer：

```csharp
using var model = new OcctModelingSession();
var box = model.MakeBox(100, 80, 60);

using var viewer = new OcctEngine();
viewer.Initialize(hwnd);
var displayed = viewer.Display(model, box, fit: true);
```

两个会话分别管理 Shape 生命周期，显示操作复制 `TopoDS_Shape`，不会让 Viewer 依赖建模会话内部指针。

## 已封装范围

| 模块 | 主要能力 |
|---|---|
| Headless 核心 | Shape 注册、复制、删除、Location、Orientation、Hash、包围盒、长度/面积/体积和重心 |
| 几何构造 | 点、线、多段线、圆、两类圆弧、椭圆、Bezier、插值 BSpline、规则多边形、矩形和平面面 |
| 基本实体 | Box、Cylinder、Cone、Sphere、Torus、Wedge、Compound、Wire、Sewing、Shell 转 Solid |
| 拓扑查询 | 子拓扑、Outer/Inner Wire、祖先关系、顶点坐标、边端点/切向、Face UV/法向、曲线/曲面类型 |
| 造型算法 | Fuse、Cut、Common、Section、Splitter、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、ThickSolid、UnifySameDomain |
| 高级布尔参数 | Fuzzy、并行、Non-destructive、Glue、反向实体检查、边/面简化 |
| 算法历史 | Generated、Modified、Removed 和算法报告；不依赖 OCAF/TNaming |
| 检查与修复 | `BRepCheck_Analyzer` 详细报告、`ShapeFix_Shape`、容差范围控制 |
| 空间分析 | Shape 距离、点投影到 Edge/Face、射线交点、点在 Solid 内外分类 |
| 网格 | 显式网格化、清理 Triangulation、Face 节点/三角形/UV/法向读取 |
| 数据交换 | Headless STEP、IGES、BREP、STL 导入导出 |
| Viewer | HWND Viewer、AIS 显示、子拓扑选择、相机、标准视图、材质、光照、文字和基础尺寸 |

完整清单见 [API 覆盖说明](docs/API_COVERAGE.md)。

## Headless 示例

```csharp
using OcctNet;

OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: @"D:\libs\OcctBridge");

using var model = new OcctModelingSession();

var body = model.MakeBox(100, 80, 60);
var hole = model.MakeCylinder(
    new OcctPoint3d(50, 40, -10),
    OcctVector3d.UnitZ,
    radius: 12,
    height: 80);

var cut = model.Cut(body, hole);
var faces = model.GetSubshapes(cut.Shape, OcctShapeType.Face);

model.Mesh(cut.Shape);
var mesh = model.GetFaceMesh(faces[0]);

var generated = model.GetGeneratedShapes(cut.OperationId, body);
model.ExportStep(cut.Shape, @"D:\output\result.step");
```

## 构建与测试

环境：

- Windows x64；
- Visual Studio 2022，安装“使用 C++ 的桌面开发”；
- .NET 8 SDK；
- CMake 3.21 或更高版本；
- OCCT 7.9.0 Visual C++ x64 版本。

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 native Release
.\build.ps1 managed Release
.\build.ps1 all Release
.\build.ps1 smoke Release

.\build.ps1 smoke Debug -OcctRoot "D:\SDK\occt-vc144-64"
```

`smoke` 会执行无窗口布尔、拓扑、网格、射线、Loft、ShapeFix、UnifySameDomain 和 BREP 往返测试。

## 引用

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

部署时需要将 `OcctNative.dll` 放在应用目录或通过 `OCCT_BRIDGE_NATIVE_DIR` 指定，并保证 OCCT 与第三方运行库可从 `PATH` 找到。

## 明确不包含

本项目不封装：

- OCAF Document、Label、Attribute；
- OCAF Undo/Redo；
- TNaming；
- XDE ShapeTool、ColorTool、LayerTool 及装配文档持久化。

因此 STEP/IGES 接口面向纯 `TopoDS_Shape` 几何交换，不承诺保留 XDE 装配层级、实例、名称、颜色和图层。

本项目也不以逐类映射 OCCT 全部 C++ 类型为目标，而是提供稳定的工程工作流 API。更专门的曲面填充、变量圆角、PipeShell、曲线/曲面全组合求交及 glTF/OBJ Provider 可在现有 C ABI 模块化结构上继续扩展。

## 许可证

本项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT 及第三方组件仍适用各自许可证。
