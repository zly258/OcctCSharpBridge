# OCCT 7.9.0 C# 封装

[English](README.md)

这是面向 Windows x64 的精简 OCCT 7.9.0 C# 封装：

```text
C# 业务项目
    ↓ ProjectReference
OcctNet（.NET 8）
    ↓ P/Invoke / 稳定 C ABI
OcctNative（C++17 DLL）
    ↓
OCCT 7.9.0
```

`main` 分支仅保留可复用封装。完整 WinForms、WPF CAD 示例已原样保存在 [`demo`](../../tree/demo) 分支。

## 仓库结构

| 路径 | 说明 |
|---|---|
| `src/OcctNative` | C++ 原生桥接层、稳定 C ABI、几何拓扑、AIS、视图、注释与文件交换 |
| `src/OcctNet` | C# 类型安全 API、原生对象生命周期、运行库定位和 WinForms 视口宿主 |
| `build.ps1` | 构建原生封装、托管封装或全部内容 |

## 环境要求

- Windows x64
- Visual Studio 2022，并安装“使用 C++ 的桌面开发”
- .NET 8 SDK
- CMake 3.21 或更高版本
- 使用 Visual C++ x64 编译的 OCCT 7.9.0

默认开发路径为 `D:\tools\occt-vc144-64`。其他安装位置可通过 `-OcctRoot` 参数或 `OCCT_ROOT` 环境变量指定。

## 构建

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\build.ps1 all Release
.\build.ps1 native Debug
.\build.ps1 managed Release
.\build.ps1 all Release -OcctRoot "D:\SDK\occt-vc144-64"
```

输出目录：

```text
build\native\bin\<Configuration>\OcctNative.dll
src\OcctNet\bin\x64\<Configuration>\net8.0-windows\OcctNet.dll
```

使用 `all` 目标时，脚本还会将 `OcctNative.dll` 复制到 `OcctNet.dll` 同目录。

## 在其他项目中引用

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
</ItemGroup>
```

使用非默认运行库路径时，应在创建第一个引擎实例前完成配置：

```csharp
using OcctNet;

OcctRuntime.Configure(
    occtRoot: @"D:\SDK\occt-vc144-64",
    nativeBridgeDirectory: @"D:\Libraries\OcctBridge");

using var engine = new OcctEngine();
```

应用发布时需要携带 `OcctNative.dll`。OCCT 运行库及第三方运行库目录必须可通过 `PATH` 访问；找到有效 OCCT 根目录后，`OcctRuntime` 会自动完成配置。

## 封装范围

- 几何与拓扑创建
- 拉伸、旋转、扫掠、放样、圆角、倒角、偏移、抽壳和钻孔
- 布尔运算与截交线
- AIS 显示、选择、高亮、相机、标准视图和 ViewCube
- 线性、角度、半径和直径尺寸
- STEP、IGES、BREP、STL 导入导出
- 包围盒、质量属性、重心、距离、拓扑统计和有效性检查

## 分支职责

- `main`：仅包含可复用的原生封装和 C# 封装
- `demo`：包含完整 CAD 示例程序及示例公共层

## 许可证

本项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT 与第三方组件仍适用各自许可证。
