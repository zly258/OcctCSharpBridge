# 打包与运行时部署

OcctCSharpBridge 明确区分 **Managed SDK** 与 **Native OCCT Runtime**。`main` 只打包可复用 Bridge/Host；完整 CAD 应用发布由 `demo` 负责。

## Managed 包

```powershell
.\build.ps1 pack Release
```

输出四套 SDK，目标框架统一来自 `bridge-contract.json`（当前为 **`net10.0-windows`**）：

```text
artifacts/packages/
├─ OcctNet.<version>.nupkg
├─ OcctNet.<version>.snupkg
├─ OcctNet.WinForms.<version>.nupkg
├─ OcctNet.WinForms.<version>.snupkg
├─ OcctNet.Wpf.<version>.nupkg
├─ OcctNet.Wpf.<version>.snupkg
├─ OcctNet.Avalonia.<version>.nupkg
└─ OcctNet.Avalonia.<version>.snupkg
```

Package Version 与 Managed Target Contract 都统一来自 `bridge-contract.json`；包校验脚本不再单独硬编码另一份 Target Framework。

## 包职责

- `OcctNet`：核心 Bridge API，不引用 WinForms/WPF/Avalonia；
- `OcctNet.WinForms`：WinForms HWND Host；
- `OcctNet.Wpf`：WPF Host；
- `OcctNet.Avalonia`：Avalonia + Windows HWND Host。

Avalonia 包当前仍是 Windows-only Host，不表示 Native Viewer 已跨平台。

## Managed 包包含

- Managed 程序集；
- IntelliSense XML；
- NuGet 依赖关系；
- README / LICENSE；
- portable PDB / symbol package。

明确不包含：

- `OcctNative.dll`；
- OCCT `TK*.dll`；
- OCCT 第三方 Runtime DLL；
- OCCT Resource 目录；
- `OcctDemo.*` 编排/应用代码或任何完整 CAD 应用代码。

`tests/check-sdk-package.ps1` 和 `build.ps1 pack` 会校验这些边界。

## .NET Runtime 要求

当前 Managed 包目标为 `net10.0-windows`。Framework-dependent 桌面程序因此需要 .NET Desktop Runtime 10.x。构建 SDK 固定为 10.0.302 以保证构建一致性；运行应用时遵循正常的 .NET 10 补丁版本前滚规则，不要求固定到某一个 `10.0.x` Runtime 补丁。

如果需要 Self-contained 发布，由 `demo` 应用层根据实际交付方式决定；可复用 Managed NuGet 本身不捆绑 .NET Runtime。

## 为什么 Native Runtime 单独部署

OCCT Runtime 与具体 OCCT Build、MSVC Runtime、第三方依赖和许可证要求有关。Managed NuGet 因此不伪装成“自带整套 OCCT 的跨机器包”。应用必须显式部署与 Bridge 2.6 / ABI 3 匹配的 `OcctNative.dll` 和 OCCT 依赖闭包。

## Runtime 查找与诊断

发布时优先 app-local。也可以显式配置：

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\runtime\occt-7.9.0",
    nativeBridgeDirectory: @"D:\runtime\bridge");
```

排查：

```csharp
var info = OcctRuntime.GetDiagnosticInfo();
var report = OcctRuntime.GetDiagnosticReport();
```

## 应用发布

完整桌面应用发布只属于 `demo`，因为只有应用层知道具体 EXE、`OcctDemo.Common`、应用资源和 app-local Native 依赖闭包。

正式发布前在安装 OCCT 7.9.0 的 Windows 机器执行：

```powershell
.\build.ps1 smoke Release
```

若 OCCT 不在默认 `D:\tools\occt-vc144-64`：

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

Demo 发布：

```powershell
.\publish.ps1 all Release -Zip
```

## 本地 NuGet 源

执行 `build.ps1 pack` 后，将 `artifacts/packages` 添加为本地 NuGet Source 即可。业务应用仍需部署匹配的 Native Runtime。

在 Native Runtime 分发、许可证审查和正式 Release 流程明确之前，不建议直接发布到公共 NuGet Feed。
