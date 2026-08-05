# OCCT 7.9.0 C# CAD Demo

[English](README.md)

`demo` 分支是在 `main` 可复用封装之上的完整演示环境。项目通过 **C++17 原生 DLL + 稳定 C ABI + .NET 8 P/Invoke** 封装 Open CASCADE Technology **7.9.0**，包含 WinForms、WPF、共享 CAD 命令层、无窗口建模、OCAF/TNaming/XDE 和接口覆盖工具。

## 项目结构

```text
CadWinForms ─┐
             ├─ CadCommon ── OcctNet ── OcctNative ── OCCT 7.9.0
CadWpf ──────┘                  │
                                ├─ OcctEngine            Viewer/AIS
                                ├─ OcctModelingSession   Headless 建模
                                └─ OcafDocument          OCAF/XDE
```

| 项目 | 说明 |
|---|---|
| `src/OcctNative` | Viewer、建模、OCAF/XDE 的 C++17 与稳定 C ABI |
| `src/OcctNet` | 类型安全的 .NET 8 API、P/Invoke 和视口控件 |
| `src/CadCommon` | WinForms/WPF 共用命令、会话、撤销重做、国际化和 API 场景 |
| `src/CadWinForms` | 传统 WinForms CAD 应用 |
| `src/CadWpf` | WPF CAD 应用，通过 `WindowsFormsHost` 复用 OCCT 视口 |
| `tests/OcctNet.Smoke` | Headless、OCAF/XDE 和持久化 Smoke Test |

## API 中心

WinForms 和 WPF 顶部菜单均增加 **API 中心**：

- 通过反射自动读取 `OcctNet` 程序集中全部公开类型、构造函数、属性、字段、事件和方法；
- 支持按模块、类型、方法、签名和前置条件搜索；
- 每个接口标识为自动场景、交互操作、文件依赖、环境依赖或目录型接口；
- `main` 后续增加公开 API 后，目录自动更新，不再手工维护按钮清单；
- 两个 UI 使用同一份 `CadCommon.ApiDemoCatalog`，不重复编写演示逻辑。

内置综合场景包括：

1. 公共 API 目录校验；
2. Viewer、相机、投影、显示精度、材质和光照；
3. 现有 CAD 基础实体、布尔、放样和注释场景；
4. Headless 建模、拓扑、网格、空间分析、修复和算法报告；
5. Headless Shape 复制到 Viewer；
6. BREP 临时文件往返；
7. OCAF Label、属性、变量、表达式、关系式、事务与 BinXCAF；
8. TNaming 历史与持久选择；
9. XDE 装配、组件、颜色、图层和材料复用。

“覆盖全部接口”表示所有公开成员都会进入可检索目录。需要鼠标、选中对象、用户文件或特定文档状态的接口会显示前置条件，不会用无意义的默认参数强制自动执行。

## 开发环境

- Windows x64
- Visual Studio 2022，安装“使用 C++ 的桌面开发”和“.NET 桌面开发”
- .NET 8 SDK
- CMake 3.21+
- **OCCT 7.9.0 VC++ x64**

默认 OCCT 根目录为：

```text
D:\tools\occt-vc144-64
```

也可设置 `OCCT_ROOT`，或通过 `-OcctRoot` 显式传入。

## 构建

```powershell
Set-ExecutionPolicy -Scope Process Bypass

# 不需要安装 OCCT：校验 C/C++/PInvoke 接口集合
.\build.ps1 validate Release

# 不需要安装 OCCT：编译托管封装和共享 Demo 层
.\build.ps1 managed Release

# 需要 OCCT 7.9.0
.\build.ps1 native Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 smoke Release
.\build.ps1 all Release

# 自定义 OCCT 目录
.\build.ps1 all Release -OcctRoot "D:\SDK\occt-vc144-64"
```

| 目标 | 内容 |
|---|---|
| `validate` | C 头文件、C++ 实现、C# P/Invoke 名称一致性 |
| `managed` | `OcctNet` 与 `CadCommon` |
| `native` | `OcctNative.dll` |
| `winform` | Native + WinForms |
| `wpf` | Native + WPF |
| `smoke` | Native + Smoke Test 编译与运行 |
| `all` | Native、WinForms、WPF 和 Smoke Test 编译 |

运行：

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
```

## CAD 界面与交互

两个应用采用统一布局：左侧 Model Explorer，中间 OCCT Viewport，右侧 Properties/Command Line，底部显示命令、选择和世界坐标。

| 操作 | 行为 |
|---|---|
| 左键 | 选择对象或子拓扑 |
| 左键拖动 | 矩形框选 |
| `Ctrl` + 选择 | 追加选择 |
| 右键拖动 | 旋转视图 |
| 中键拖动 | 平移 |
| 滚轮 | 缩放 |
| `Esc` | 清除选择 |
| `Ctrl+Z` / `Ctrl+Y` | 命令重放式撤销/重做 |

选择过滤器支持 Object、Vertex、Edge、Wire、Face、Shell 和 Solid。界面默认英文，可在 `Language` 菜单切换简体中文。

## 封装能力

| 模块 | 主要能力 |
|---|---|
| Viewer/AIS | HWND Viewer、显示、隐藏、选择、子拓扑、相机、投影、材质、光照、文字和尺寸 |
| Headless | 几何、实体、布尔、Splitter、特征、修复、拓扑、距离、投影、射线、网格和纯 Shape 交换 |
| OCAF/TDF | 文档、持久化、事务、Undo/Redo、Label、标量、数组、引用、变量、表达式和关系式 |
| TNaming | Generated/Modify/Delete/Select、NamedShape 历史和 Selector |
| XDE | Shape、装配、组件、实例位置、颜色、图层、材料、验证属性和长度单位 |
| 交换 | STEP/IGES/BREP/STL；STEPCAF/IGESCAF 保留 XDE 元数据 |

详细边界见：

- `docs/API_COVERAGE.md`
- `docs/OCAF_COVERAGE.md`
- `docs/OCAF_EXTENDED_API.md`

## 测试与边界

GitHub Actions 会自动校验 491 个 C ABI 入口的声明、实现与 P/Invoke 对应关系，并编译 `OcctNet`、`CadCommon`、WinForms、WPF 和 Smoke Test。

原生 DLL 链接、Viewer 渲染、BinXCAF/STEPCAF/IGESCAF 的真实运行仍必须在安装 OCCT 7.9.0 的 Windows 目标环境执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

应用异常日志写入：

```text
%LOCALAPPDATA%\OcctCSharpBridge\Logs
```

## 许可证

本项目采用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT 及第三方组件仍适用各自许可证。
