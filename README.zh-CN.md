# OCCT 7.9.0 C# WinForms 与 WPF Demo

[English](README.md) · [文档索引](docs/README.zh-CN.md) · [可复用 SDK：`main` 分支](https://github.com/zly258/OcctCSharpBridge/tree/main)

`demo` 分支是在 `main` 可复用封装基础上的完整桌面演示环境，包含 WinForms、WPF、共享 CAD 命令层、Viewer 与 Headless 示例、OCAF/TNaming/XDE 场景、可检索的公共 API 目录、持续集成和 Windows x64 免配置发布脚本。

## 项目结构

```text
CadWinForms ─┐
             ├─ CadCommon ── OcctNet ── OcctNative ── OCCT 7.9.0
CadWpf ──────┘                  │
                                ├─ OcctEngine            Viewer/AIS
                                ├─ OcctModelingSession   Headless 建模
                                └─ OcafDocument          OCAF/TNaming/XDE
```

| 项目 | 作用 |
|---|---|
| `src/OcctNative` | C++17 桥接、稳定 C ABI、Viewer、建模和 OCAF/XDE 原生实现 |
| `src/OcctNet` | 类型安全的 .NET 8 API、运行时查找、P/Invoke 和共享视口控件 |
| `src/CadCommon` | 两个 UI 共用的命令、会话、撤销重做、国际化、示例和 API 场景 |
| `src/CadWinForms` | 经典 WinForms CAD 应用 |
| `src/CadWpf` | WPF CAD 应用，通过 `WindowsFormsHost` 复用 OCCT 视口 |
| `tests/OcctNet.ApiCatalog` | 确保所有公开成员都进入 API 目录 |
| `tests/OcctNet.Smoke` | Headless、OCAF/XDE、持久化和 Shape 转移 Smoke Test |

## 桌面应用主要功能

WinForms 与 WPF 都具有：

- Model Explorer 和对象选择状态；
- 中央 OCCT Viewer、标准视图和相机控制；
- 命令面板与属性编辑；
- 对象和子拓扑选择过滤；
- 点选与矩形框选；
- 基于 OCCT `AIS_RubberBand` 的框选指示框，不再使用 Win32 XOR，避免闪烁；
- 显示模式、显隐、颜色、透明度、材质和线宽操作；
- 几何、实体、变换、布尔、特征、分析和注释演示；
- 共享 Demo 层中的命令重放式撤销重做；
- 英文与简体中文界面；
- `%LOCALAPPDATA%\OcctCSharpBridge\Logs` 异常日志。

## 相机与显示行为

创建 Shape 时只显示并刷新场景，**不会自动调用 `Fit` 或 `FitAll`**。普通命令执行后保持用户当前相机，不会每创建一个对象就跳动视图。

确实需要调整相机时显式调用：

```csharp
engine.Fit(shape);
engine.FitAll();
engine.WindowFit(x1, y1, x2, y2);
```

多对象示例采用可嵌套批量显示：

```csharp
using (engine.BeginDisplayBatch())
{
    var box = engine.MakeBox(100, 80, 60);
    var cylinder = engine.MakeCylinder(20, 80, 140, 0, 0);
    engine.SetColor(box, Color.SteelBlue);
    engine.SetColor(cylinder, Color.OrangeRed);

    // 仅示例陈列等确有需要时，显式适配一次。
    engine.FitAll();
}
```

所有对象仍保留独立 ID，可独立选择、修改属性、删除并显示在模型树中。批量作用域只消除中间多次 OpenGL 刷新，不会把多个对象强行合并为 Compound。

## API 中心

WinForms 与 WPF 都提供 **API 中心 → 接口目录与综合场景**。

API 目录通过反射读取 `OcctNet` 中所有公开内容：

- 类型和枚举；
- 构造函数；
- 属性和字段；
- 事件；
- 方法和完整签名。

接口按执行条件分类：

- 自动场景；
- 交互依赖；
- 文件依赖；
- 环境依赖；
- 仅目录展示。

这样无需为几百个接口堆积低价值按钮，同时保证 `main` 新增公开 API 后可以自动进入目录。

### 可执行综合场景

共享 `CadCommon.ApiDemoCatalog` 当前包括：

1. 公共 API 目录完整性校验；
2. Viewer、相机、投影、显示精度、材质和光照；
3. CAD 基本体、布尔、放样和注释；
4. Headless 建模、拓扑、网格、射线分析、修复和算法报告；
5. Headless Shape 复制到 Viewer；
6. BREP 临时文件往返；
7. OCAF Label、属性、变量、表达式、关系式、事务和 BinXCAF；
8. TNaming 演化历史与持久选择；
9. XDE 装配、组件、位置、颜色、图层和材料。

API 中心一次运行多个 CAD 示例时使用一个外层批次，内部示例批次嵌套后仍只进行一次最终刷新。

## 交互方式

| 输入 | 行为 |
|---|---|
| 左键 | 选择对象或当前子拓扑类型 |
| 左键拖动 | 矩形框选 |
| `Ctrl` + 选择 | 追加选择 |
| 右键拖动 | 旋转视图 |
| 中键拖动 | 平移 |
| 滚轮 | 缩放 |
| `Esc` | 清除选择 |
| `Ctrl+Z` / `Ctrl+Y` | Demo 命令历史撤销/重做 |

选择模式包括 Object、Vertex、Edge、Wire、Face、Shell 和 Solid。

## 开发环境

- Windows 10/11 x64
- Visual Studio 2022，安装“使用 C++ 的桌面开发”和“.NET 桌面开发”
- .NET 8 SDK
- CMake 3.21+
- **严格使用 OCCT 7.9.0 VC++ x64**

默认 OCCT 根目录：

```text
D:\tools\occt-vc144-64
```

也可通过 `OCCT_ROOT` 或 `-OcctRoot` 指定。

## 构建与运行

```powershell
Set-ExecutionPolicy -Scope Process Bypass

# C ABI、C++ 定义和 P/Invoke 静态一致性。
.\build.ps1 validate Release

# 托管封装和共享 Demo 层。
.\build.ps1 managed Release

# 以下目标需要 OCCT SDK。
.\build.ps1 native Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

从开发目录运行：

```powershell
.\run.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64"
.\run.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64"
```

## 免配置发布

`publish.ps1` 用于生成可直接分发的 Windows x64 包。使用该发布包的其他人无需安装或配置 OCCT SDK、CMake、Visual Studio，也无需单独安装 .NET 运行时。

同时发布两个应用：

```powershell
.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -OutputDirectory ".\artifacts\publish" `
  -Zip
```

只发布一个应用：

```powershell
.\publish.ps1 winform Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
.\publish.ps1 wpf Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

默认采用自包含发布，打包内容包括：

- .NET 8 运行时；
- WinForms 和/或 WPF 应用文件；
- `OcctNative.dll`；
- OCCT 运行库 DLL；
- 检测到的 OCCT 第三方 DLL；
- 当前系统中可用的 Visual C++ 运行库 DLL；
- 数据交换、持久化、Shader、消息、单位和纹理所需的 OCCT 资源目录；
- 相对路径启动脚本；
- 文件版本与 SHA-256 清单；
- 项目、OCCT 和检测到的第三方许可证文件。

生成的启动脚本会根据解压目录自动设置 `PATH`、`OCCT_BRIDGE_NATIVE_DIR`、`OCCT_ROOT` 和 `CASROOT`。接收者只需运行 `Start-WinForms.cmd` 或 `Start-WPF.cmd`，不需要手工设置环境变量。

详细说明见 [Demo 免配置发布](docs/PUBLISHING_DEMO.zh-CN.md) 和 [部署与运行时目录](docs/DEPLOYMENT.zh-CN.md)。

## 发布包结构

```text
OcctCSharpBridge-Demo-win-x64
├─ apps
│  ├─ winform
│  └─ wpf
├─ runtime
│  ├─ OcctNative.dll
│  ├─ TK*.dll
│  ├─ 第三方 DLL
│  └─ Visual C++ 运行库 DLL
├─ occt\src\...
├─ licenses
├─ Start-WinForms.cmd
├─ Start-WPF.cmd
├─ runtime-manifest.txt
└─ README.txt
```

必须保持整个目录结构，不要只把 EXE 单独发给别人。

## 封装能力概览

| 模块 | 主要覆盖 |
|---|---|
| Viewer/AIS | 相机、投影、视图立方体、坐标轴、显示、选择、材质、光照、文字和尺寸 |
| 几何 | 曲线、Wire、Face、常用实体和 Compound |
| 特征 | 布尔、Splitter、拉伸、旋转、扫掠、放样、圆角、倒角、偏移、厚实体和变换 |
| 分析 | 包围盒、质量属性、拓扑、距离、投影、射线、有效性和算法报告 |
| Headless | 后台建模、修复、网格、面三角网和纯 Shape 交换 |
| OCAF/TDF | 文档、Label、属性、事务、Undo/Redo、变量、表达式和关系式 |
| TNaming | 演化历史和持久 Selector 工作流 |
| XDE | Shape、装配、组件、位置、名称、颜色、图层、材料和验证属性 |
| 数据交换 | BREP/STL/STEP/IGES，以及保留元数据的 STEPCAF/IGESCAF |

## 持续集成

当前分支自动检查：

- C ABI 声明、C++ 定义和 C# P/Invoke 一致性；
- `OcctNet` 与 `CadCommon` 编译；
- 全部公开 API 进入 API Catalog；
- WinForms 与 WPF 编译；
- Smoke Test 编译；
- `publish.ps1` PowerShell 语法和必要项目输入。

GitHub Runner 不包含仓库所需的 OCCT 7.9.0 SDK，因此原生链接、真实 Viewer 渲染和完整发布包生成，仍需在安装 OCCT 7.9.0 的 Windows 开发电脑上验证。

## 文档

- [Demo 文档索引](docs/README.zh-CN.md)
- [快速开始](docs/GETTING_STARTED.zh-CN.md)
- [Viewer、选择与显示刷新](docs/VIEWER_AND_DISPLAY.zh-CN.md)
- [Demo 免配置发布](docs/PUBLISHING_DEMO.zh-CN.md)
- [部署与运行时目录](docs/DEPLOYMENT.zh-CN.md)
- [API 覆盖说明](docs/API_COVERAGE.md)
- [OCAF/XDE 覆盖说明](docs/OCAF_COVERAGE.md)
- [OCAF 扩展 API](docs/OCAF_EXTENDED_API.md)

## 许可证

本仓库采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT、Microsoft 运行库和第三方组件仍适用各自许可证与再分发条款。分发前必须检查生成包中的 `licenses` 目录。
