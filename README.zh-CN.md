# OcctCSharpBridge

[English](README.md) · [WinForms/WPF 演示分支](https://github.com/zly258/OcctCSharpBridge/tree/demo)

面向 Windows x64 的 Open CASCADE Technology 7.9.0 与 .NET 8 桥接项目。可复用的 `main` 分支只保留原生 C ABI、托管封装、接口检查和接口清单；WinForms、WPF 完整示例位于 `demo` 分支。

## 目录结构

```text
src/OcctNative         C++17 原生桥接与稳定 C ABI
src/OcctNet            不依赖 UI 的类型安全 .NET 封装
src/OcctNet.WinForms   可选的 WinForms OCCT 视口控件
src/OcctNet.Wpf        可选的 WPF OCCT 视口控件
tests            接口一致性检查与原生 Smoke Test
docs             中英文接口清单
```

封装提供两类原生会话：

- `OcctEngine`：HWND Viewer、AIS 对象、选择、相机、显示属性、文字和尺寸。
- `OcctModelingSession`：无窗口几何、拓扑、算法、网格、分析、修复和文件交换。

新增批量颜色、透明度、可见性、显示模式、线宽、材质、重显示和选择接口，减少大型场景中的重复 P/Invoke 调用。视口状态快照、适配选择集、视图重置、场景重心和屏幕投影到平面接口可直接支撑 CAD 交互工具。

桥接层不再包含 OCAF/XDE。应用文档、撤销重做和 JSON 持久化由上层应用自行实现，避免把文档机制耦合进几何桥接。

## 兼容性约束

- OCCT 必须为 `7.9.0`。
- 托管目标为 `.NET 8`、Windows x64。
- 使用 `OcctBridgeInfo` 在运行时校验原生桥接 ABI。
- `OcctNet.dll` 与 `OcctNative.dll` 必须来自同一次构建。
- OCCT 及第三方 DLL 必须位于应用目录，或通过运行库搜索路径配置。

## 构建

```powershell
# 检查声明、实现、P/Invoke 调用约定和接口清单。
.\build.ps1 validate Release

# 只构建可复用托管封装。
.\build.ps1 managed Release

# 构建原生与托管封装。
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"

# 构建并执行原生建模测试。
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

## 项目引用

```xml
<ItemGroup>
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet\OcctNet.csproj" />
  <!-- WinForms 宿主。 -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.WinForms\OcctNet.WinForms.csproj" />
  <!-- WPF 宿主，内部复用 WinForms HWND 宿主。 -->
  <ProjectReference Include="..\OcctCSharpBridge\src\OcctNet.Wpf\OcctNet.Wpf.csproj" />
</ItemGroup>
```

## 接口清单

- [中文接口清单](docs/API_COVERAGE.zh-CN.md)
- [English interface inventory](docs/API_COVERAGE.md)

`build.ps1 validate` 会检查声明、P/Invoke 映射、调用约定和清单数量；未同步时直接失败。定时工作流还会检查 `main` 与 `demo` 的可复用封装目录是否完全一致。

## 许可证

项目使用 [PolyForm Noncommercial License 1.0.0](LICENSE)。OCCT 及第三方组件仍遵循各自许可证。
