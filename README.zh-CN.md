# OCCT 7.9.0 C# 工程级封装

[English](README.md)

本仓库通过 **C++17 原生 DLL + 稳定 C ABI + .NET 8 P/Invoke** 封装 Open CASCADE Technology **7.9.0**。`main` 分支只保留可复用封装；完整 WinForms/WPF 示例保存在 [`demo`](https://github.com/zly258/OcctCSharpBridge/tree/demo) 分支。

## 架构

```text
业务程序
├─ OcctModelingSession      无窗口建模、查询、修复、网格和纯几何交换
├─ OcafDocument             OCAF/TDF/TNaming/XDE、装配、属性和持久化
└─ OcctEngine               AIS/V3d Viewer、选择、显示和标注
          ↓
OcctNet (.NET 8, x64)
          ↓ P/Invoke / C ABI
OcctNative (C++17)
          ↓
OCCT 7.9.0
```

三个会话分别管理生命周期。Shape 在 Headless、OCAF/XDE 与 Viewer 之间按值复制，不向 C# 暴露 `Handle(...)`、`TDF_Label` 或其他 C++ 指针。

## 版本约束

OCAF/TNaming/XDE 对版本签名较敏感，因此本项目严格要求 **OCCT 7.9.0**：

- CMake 读取 `Standard_Version.hxx`，非 7.9.0 直接停止配置；
- C++ 使用编译期 `static_assert`；
- `OcafDocument` 创建时再次校验已加载 DLL 的版本。

不允许用 7.8、7.9.1/7.9.3 或 8.0 静默替代。

## 已封装范围

| 模块 | 主要能力 |
|---|---|
| Headless 核心 | Shape 注册、复制、删除、Location、Orientation、Hash、包围盒、长度/面积/体积和重心 |
| 几何与实体 | 点、线、曲线、平面轮廓、Box、Cylinder、Cone、Sphere、Torus、Compound、Wire、Sewing 等 |
| 拓扑与算法 | 子拓扑、几何求值、布尔、Splitter、拉伸、旋转、扫掠、放样、圆角、倒角、偏移、厚实体 |
| 修复与分析 | BRepCheck、ShapeFix、距离、投影、射线求交、实体内外分类、显式网格 |
| OCAF 文档 | 新建/打开/保存、BinXCAF/XmlXCAF/BinOcaf/XmlOcaf、事务、Undo/Redo |
| TDF/TData | Label 层级、通用属性枚举、Name/Comment/数值/引用/数组/Position/Shape 属性 |
| TNaming | Generated、Modify、Delete、Select、NamedShape 历史和 Selector 求解 |
| XDE 装配 | Shape、自由 Shape、Assembly、Component、Reference、Location 和装配更新 |
| XDE 元数据 | RGBA 颜色、可见性、图层、物理材料、面积、体积、质心和长度单位 |
| 数据交换 | 纯 Shape STEP/IGES/BREP/STL；保留装配和元数据的 STEPCAF/IGESCAF |
| Viewer | HWND Viewer、AIS 显示、子拓扑选择、相机、材质、光照、文字和基础尺寸 |

详细边界见 [API 覆盖说明](docs/API_COVERAGE.md) 和 [OCAF/XDE 覆盖说明](docs/OCAF_COVERAGE.md)。

## OCAF/XDE 示例

```csharp
using OcctNet;

OcctRuntime.Configure(
    occtRoot: @"D:\tools\occt-vc144-64",
    nativeBridgeDirectory: @"D:\libs\OcctBridge");

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
    document.SetColor(product, OcafColorType.Surface, new OcafColor(0.2, 0.45, 0.8));

    var layer = document.AddLayer("Equipment");
    document.SetLayer(product, layer);
    document.SetMaterial(product, "Steel", "Structural steel", 7.85);
    command.Commit();
}

document.SaveAs(@"D:\output\assembly.xbf");
document.ExportStep(@"D:\output\assembly.step");
```

## Headless 示例

```csharp
using var model = new OcctModelingSession();
var body = model.MakeBox(100, 80, 60);
var hole = model.MakeCylinder(new OcctPoint3d(50, 40, -10), OcctVector3d.UnitZ, 12, 80);
var cut = model.Cut(body, hole);
model.ExportStep(cut.Shape, @"D:\output\result.step");
```

## 构建与测试

环境：Windows x64、Visual Studio 2022（C++ 桌面开发）、.NET 8 SDK、CMake 3.21+、**OCCT 7.9.0 VC++ x64**。

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 native Release
.\build.ps1 managed Release
.\build.ps1 all Release
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

`smoke` 同时验证 Headless 建模，以及 OCAF 事务、Undo/Redo、TDF 属性、XDE Shape/名称/颜色/图层/材料、BinXCAF 保存重开和 Shape 回传。

## 引用

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

部署时需要将 `OcctNative.dll` 放在应用目录或通过 `OCCT_BRIDGE_NATIVE_DIR` 指定，并保证 OCCT 与第三方运行库可从 `PATH` 找到。

## 边界

本项目提供工程级工作流 API，不逐个暴露 OCCT 的全部 C++ 实现类。原生 Handle、Label/Attribute 指针、持久化驱动内部类、TDF Delta 实现类和自定义 TFunction Driver 回调不跨 C ABI。高级 GD&T、View、Note、Clipping Plane、PBR Visual Material 当前可通过区段 Label 和通用属性 JSON 检查，专用强类型 CRUD 可按模块继续扩展。

## 许可证

本项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT 及第三方组件仍适用各自许可证。
