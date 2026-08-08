# 打包与运行时部署

OcctCSharpBridge 明确区分 **Managed SDK 包** 与 **Native OCCT Runtime**。这样可以避免生成一个“能安装 NuGet，但换机器无法运行”的伪完整包。

## Managed 包

执行：

```powershell
.\build.ps1 pack Release
```

输出：

```text
artifacts/packages/
├─ OcctNet.<version>.nupkg
├─ OcctNet.<version>.snupkg
├─ OcctNet.WinForms.<version>.nupkg
├─ OcctNet.WinForms.<version>.snupkg
├─ OcctNet.Wpf.<version>.nupkg
└─ OcctNet.Wpf.<version>.snupkg
```

版本号统一从 `bridge-contract.json` 注入。

## Managed 包包含什么

包含：

- Managed 程序集；
- IntelliSense XML 文档；
- 包依赖关系；
- README 和许可证元数据；
- portable PDB / symbol package。

不包含：

- `OcctNative.dll`；
- OCCT `TK*.dll`；
- OCCT 第三方 Runtime DLL；
- OCCT Resource 目录。

## 为什么 Native Runtime 不直接塞进 NuGet

OCCT Runtime 部署与具体 OCCT 构建、编译器运行库、第三方依赖以及许可证要求有关。Bridge 因此把 Native 部署作为应用程序的显式责任，而不是隐藏到 Managed NuGet 包中。

## Runtime 查找

正式发布优先采用 app-local，即把完整 Native 依赖部署在应用程序附近。Runtime 也支持显式配置：

```csharp
OcctRuntime.Configure(
    occtRoot: @"D:\runtime\occt-7.9.0",
    nativeBridgeDirectory: @"D:\runtime\bridge");
```

排查运行问题：

```csharp
var report = OcctRuntime.GetDiagnosticReport();
```

## 应用发布

完整桌面应用发布由 `demo` 分支负责，因为只有它明确知道 WinForms/WPF/Avalonia 三个 EXE 和 app-local Native 依赖闭包。

安装 OCCT 7.9.0 的机器上，发布前先执行：

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

三套 Demo 发布：

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

## 本地 NuGet 源

运行 `build.ps1 pack` 后，可以把 `artifacts/packages` 添加为本地 NuGet 源，并像普通包一样引用。业务应用仍必须部署与 Bridge 2.6 / ABI 3 匹配的 Native Runtime。

在 Native Runtime 分发策略和正式 Release 流程明确之前，不建议把这些包直接发布到公共 NuGet Feed。
