# OcctCSharpBridge · OcctScript

[English](README.md) · [main 分支](https://github.com/zly258/OcctCSharpBridge/tree/main)

`script` 分支提供 **OcctScript 可用初版**：在 `OcctCSharpBridge` 可复用封装基础上实现轻量、可扩展的参数化 CAD 脚本编辑器，并持续同步 `main` 中最新 OCCT 封装。

本分支不包含 Demo 应用项目。`OcctNet.WinForms` 与 `OcctNet.Wpf` 是从 `main` 同步的可复用视口宿主库，`OcctScript.Editor` 才是本分支提供的桌面应用。

OcctScript **不使用 OCAF/XDE**。文档机制、历史和持久化属于应用层，文件使用可读 JSON 保存。

## 初版已经具备

- Windows x64、.NET 8、Open CASCADE Technology 7.9.0。
- WPF 参数化编辑器和 OCCT 三维视口。
- 默认英文界面，运行时可切换简体中文。
- 带版本号的 `.json` / `.ocsproj` 文档。
- 命名参数和直接表达式，不需要 `${...}`。
- 支持 `+ - * / ^`、`PI`、`E` 和常用数学函数。
- 元数据驱动的 Command 注册与属性编辑。
- Command 引用、依赖排序、循环依赖检测和拓扑类型校验。
- 从建模历史完整重建模型，支持文档级撤销/重做。
- `samples/Scripts` 提供可直接打开的 JSON 示例。
- `帮助 → 关于 OcctScript`。
- `OcctScript.Smoke` 使用真实 `OcctModelingSession` 验证建模链。

## 初版 Command 范围

**曲线与线框：** `Vertex`、`Line`、`Polyline`、`Circle`、`Arc`、`Ellipse`、`RegularPolygon`、`Bezier`、`BSpline`、`Rectangle`、`Wire`

**面：** `Face`、`PlaneFace`

**基本体与拓扑体：** `Box`、`Cylinder`、`Cone`、`Sphere`、`Torus`、`Wedge`、`Compound`、`Sew`、`SolidFromShell`

**特征：** `Extrude`、`Revolve`、`Sweep`、`Loft`、`Fillet`、`Chamfer`、`Offset`、`Shell`

**布尔：** `Fuse`、`Cut`、`Common`、`Section`

**显式变换：** `Move`、`RotateShape`、`ScaleShape`、`Mirror`

此外，每个 Command 自带统一 Transform，可在构建完成后执行 X/Y/Z 平移、绕 X/Y/Z 旋转和整体缩放。

## 典型建模链

```text
直线 / 圆弧 / Bezier / B-Spline
                ↓
              Wire
                ↓
              Face
                ↓
             Extrude
                ↓
              Solid
                ↓
      Fillet / Chamfer / Shell
                ↓
         Boolean / Transform
```

同时支持边 → 拉伸 → 面、面 → 拉伸 → 体、轮廓 → 旋转、轮廓 + 路径 → 扫掠、多个截面 → 放样。

## 目录结构

```text
src/OcctNative              C++17 OCCT 桥接与稳定 C ABI
src/OcctNet                 与 UI 无关的托管封装
src/OcctNet.WinForms        WPF 视口内部复用的 HWND 宿主库
src/OcctNet.Wpf             可复用 WPF 视口宿主

src/OcctScript.Domain       JSON 文档和 Command 元数据
src/OcctScript.Expressions  表达式解析与计算
src/OcctScript.Serialization JSON 持久化
src/OcctScript.Application  校验、参数、撤销重做
src/OcctScript.Geometry     依赖图和真实 OCCT Builder
src/OcctScript.Editor       WPF 参数化编辑器应用

samples/Scripts             可直接打开的 JSON 示例
tests/OcctScript.Smoke      脚本/建模 Smoke Test
docs/script                 精简的 OcctScript 文档
```

## 从克隆开始

```powershell
git clone https://github.com/zly258/OcctCSharpBridge.git
cd OcctCSharpBridge
git switch script
$env:OCCT_ROOT = "D:\\tools\\occt-vc144-64"
```

`script` 分支不是 Demo 分支，它提供 `OcctScript.Editor` 参数化编辑器。第一次使用建议先执行完整 `script` target，再启动 Editor。

### 脚本使用速查

| 命令 | 用途 |
| --- | --- |
| `.\build.ps1 managed Release` | 只构建可复用托管 Bridge/Host |
| `.\build.ps1 script Release -OcctRoot <path>` | 校核 Bridge，构建 Native、OcctScript 各层、Editor，并运行 Script Smoke |
| `.\run.ps1` | 启动已经构建好的 Editor |
| `.\run.ps1 Release -OcctRoot <path>` | 指定 OCCT 路径启动 Editor |
| `.\run.ps1 Release -OcctRoot <path> -Build` | 先执行完整 script 构建，再启动 Editor |

`run.ps1` 默认不会重新构建；需要保证输出目录与当前源码一致，或者使用 `-Build`。

## 环境要求

Windows x64；Visual Studio 2022 / MSVC；.NET 8 SDK；CMake 3.16+；Open CASCADE Technology 7.9.0 VC14 x64；配置 `OCCT_ROOT` 或通过 `-OcctRoot` 指定。

## 一键构建 OcctScript 初版

```powershell
# 检查 bridge 契约，构建 Native/托管层/编辑器，并运行 OcctScript.Smoke
.\build.ps1 script Release -OcctRoot "D:\tools\occt-vc144-64"
```

编辑器输出目录：

```text
src\OcctScript.Editor\bin\x64\Release\net8.0-windows\
```

`build.ps1 script` 会把同一次构建得到的 `OcctNative.dll` 复制到编辑器和脚本测试输出目录。运行时建议继续保留 `OCCT_ROOT`，以便解析 OCCT 运行库 DLL。

## 启动编辑器

构建完成后，在仓库根目录直接运行：

```powershell
.\run.ps1
```

需要明确指定 OCCT 安装目录时：

```powershell
.\run.ps1 Release -OcctRoot "D:\tools\occt-vc144-64"
```

需要先完整构建 `script` 再启动时：

```powershell
.\run.ps1 Release -OcctRoot "D:\tools\occt-vc144-64" -Build
```

程序默认英文，通过 **Language → 中文** 切换中文。

## 示例 JSON

[`samples/Scripts`](samples/Scripts/README.md) 中提供：`01-Curves.json`、`02-Extrude.json`、`03-Revolve.json`、`04-Sweep.json`、`05-Loft.json`、`06-Booleans.json`、`07-Primitives-Transforms.json`、`08-Edge-Features.json`。

最小文档结构：

```json
{
  "format": "OcctScript.Document",
  "version": 1,
  "name": "Example",
  "lengthUnit": "mm",
  "angleUnit": "deg",
  "parameters": [],
  "commands": [],
  "outputCommandIds": []
}
```

详细说明见 [JSON 格式](docs/script/JSON_FORMAT.md) 和 [Command 清单](docs/script/COMMANDS.md)。

## 初版边界

本版本目标是把普通参数化模型从“线—面—体—特征”完整跑通。本阶段明确不加入：型钢和专业截面生成器、复杂过渡件、线性/环形/不规则阵列、几何约束草图求解器、OCAF/XDE 文档、装配产品结构编辑。

这些能力后续可以继续扩展，不需要破坏当前 JSON Command 模型。

## Bridge 兼容性

Bridge 版本：2.5.0；ABI：2。

当前同步的封装目标：OCCT `7.9.0`、.NET `8`、Windows x64、`OcctBridgeInfo.ManagedVersion` `2.5.0`、Native ABI `2`。

`OcctEngine` 负责交互式 AIS/Viewer，`OcctModelingSession` 负责无界面几何、拓扑和算法对象。OcctScript 在 `OcctModelingSession` 中构建，再把结果复制到 WPF 三维视口。

## 文档

- [OcctScript 总览](docs/script/README.md)
- [Command 清单](docs/script/COMMANDS.md)
- [JSON 格式](docs/script/JSON_FORMAT.md)
- [OcctCSharpBridge 中文接口清单](docs/API_COVERAGE.zh-CN.md)
- [English API inventory](docs/API_COVERAGE.md)

## 许可证

项目使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT 及第三方组件遵循各自许可证。