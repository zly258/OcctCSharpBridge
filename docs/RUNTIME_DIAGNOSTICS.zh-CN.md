# 结构化 Runtime 诊断

`OcctRuntime.GetDiagnosticReport()` 继续保留完整的人类可读文本报告。Bridge 2.6 新增 `OcctRuntime.GetDiagnosticInfo()`，用于 UI、自动检查、启动诊断和支持信息收集，避免业务代码解析日志字符串。

## 获取诊断快照

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

Console.WriteLine(info.ProcessArchitecture);
Console.WriteLine(info.ConfiguredNativeDirectory);
Console.WriteLine(info.ConfiguredNativeBridgeExists);
Console.WriteLine(info.LoadedNativeBridgePath);
Console.WriteLine(info.LoadedOcctKernelPath);
```

`GetDiagnosticInfo()` **不会主动加载** `OcctNative.dll` 或 OCCT。Loaded 字段只反映当前进程中已经实际加载的模块。

## 主要字段

`OcctRuntimeDiagnosticInfo` 包含：

- 快照时间、.NET Framework 描述、操作系统描述；
- 进程架构与操作系统架构；
- `Is64BitProcess`；
- 应用 BaseDirectory 与 CurrentDirectory；
- `OCCT_BRIDGE_NATIVE_DIR` 配置；
- `OCCT_ROOT` 与 `CASROOT` 配置；
- 配置得到的 `OcctNative.dll` 路径，以及文件是否实际存在；
- 配置得到的 OCCT `TKernel.dll` 路径，以及文件是否实际存在；
- 当前进程实际加载的 `OcctNative.dll` 路径；
- 当前进程实际加载的 `TKernel.dll` 路径；
- `NativeBridgeLoaded`、`OcctKernelLoaded` 快捷标志；
- 原有完整 `DiagnosticReport` 文本。

`Configured...Exists` 使用可空布尔值：

- `null`：对应环境路径没有配置；
- `false`：已经配置路径，但预期文件不存在；
- `true`：预期文件确实存在于该配置路径。

## 排查 Win32 126

启动阶段可以先做结构化判断：

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

if (info.ConfiguredNativeBridgeExists == false)
{
    // OCCT_BRIDGE_NATIVE_DIR 已配置，但目录下没有 OcctNative.dll。
}

if (info.ConfiguredOcctKernelExists == false)
{
    // OCCT_ROOT/CASROOT 与预期 OCCT 7.9 VC14 x64 目录结构不匹配。
}

if (info.NativeBridgeLoaded && !info.OcctKernelLoaded)
{
    // Bridge 已进入进程，但 OCCT Runtime 依赖仍可能没有完整解析。
}
```

第一次 Native 调用之前，`NativeBridgeLoaded == false` **不代表错误**，因为诊断快照本身不会强制触发 Native Load。

成功执行 Native 操作之后，`LoadedNativeBridgePath` / `LoadedOcctKernelPath` 可以帮助判断是否意外加载了其他目录里的旧 DLL 或错误 Runtime 副本。

## 桌面 UI 集成

WinForms、WPF、Avalonia 应用可以直接把强类型字段展示在“启动诊断 / 故障排查”区域，把完整 `DiagnosticReport` 放在“详细信息”中。

建议摘要优先显示：

1. 进程架构；
2. Bridge 配置路径 + 是否存在；
3. OCCT Kernel 配置路径 + 是否存在；
4. 实际 Loaded Bridge 路径；
5. 实际 Loaded OCCT Kernel 路径。

这比解析中英文日志文本稳定得多，也更方便后续 OCStation 或其他宿主统一复用。

## 路径隐私

诊断信息包含本机文件路径。将诊断内容发到 Issue、邮件或外部支持渠道之前，应根据需要检查并脱敏用户名、工程目录、网络共享路径等环境信息。

## 与发布检查的关系

结构化 Runtime 快照不会替代发布打包校验。`demo` 的 `publish.ps1` 仍负责解析 Native 依赖闭包，并在发布前执行受限 `LoadLibraryExW` 探针。

两者解决的问题不同：

- **Publish 校验**：这个发布包放到干净机器上是否具备完整依赖；
- **Runtime 诊断**：当前这个进程此刻看到了什么配置、实际加载了什么 DLL。

正式发布优先使用 app-local Native Runtime；开发环境确需指定独立 Runtime 时再使用 `OCCT_BRIDGE_NATIVE_DIR`、`OCCT_ROOT` 或 `CASROOT`。
