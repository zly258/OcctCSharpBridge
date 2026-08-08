# 结构化 Runtime 诊断

`OcctRuntime.GetDiagnosticReport()` 继续保留完整的人类可读报告；`OcctRuntime.GetDiagnosticInfo()` 提供强类型、无副作用的运行时快照，适合启动诊断、UI、自动检查和支持信息收集。

两个诊断接口都**不会配置 Runtime、不会修改 DLL 搜索路径、不会修改 OCCT 环境变量，也不会强制加载 `OcctNative.dll`**。

## 获取诊断快照

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

Console.WriteLine(info.ProcessArchitecture);
Console.WriteLine(info.ApplicationNativeBridgePath);
Console.WriteLine(info.ApplicationNativeBridgeExists);
Console.WriteLine(info.ApplicationOcctKernelPath);
Console.WriteLine(info.ApplicationOcctKernelExists);
Console.WriteLine(info.LoadedNativeBridgePath);
Console.WriteLine(info.LoadedOcctKernelPath);
```

## 三层诊断语义

诊断快照刻意区分三个不同问题。

### 1. App-local 发布目录状态

下面字段直接检查 EXE 所在目录，不依赖环境变量：

- `ApplicationNativeBridgePath`；
- `ApplicationNativeBridgeExists`；
- `ApplicationOcctKernelPath`；
- `ApplicationOcctKernelExists`。

正常 Portable Demo 发布会把 `OcctNative.dll`、`TKernel.dll`、所需 OCCT 模块、VC++ Runtime 和第三方 Native DLL 放在 EXE 同目录。因此在第一次 Native 调用之前，就可以先判断发布目录是否明显缺文件。

### 2. 显式/环境配置状态

同时报告：

- `ConfiguredNativeDirectory`：来自 `OCCT_BRIDGE_NATIVE_DIR`；
- `ConfiguredOcctRoot`：来自 `OCCT_ROOT`；
- `ConfiguredCasRoot`：来自 `CASROOT`；
- `ConfiguredNativeBridgePath` 与可空 `ConfiguredNativeBridgeExists`；
- `ConfiguredOcctKernelPath` 与可空 `ConfiguredOcctKernelExists`。

可空 Exists 语义为：

- `null`：对应环境路径没有配置；
- `false`：配置了路径，但预期文件不存在；
- `true`：预期文件确实存在。

### 3. 当前进程实际加载状态

当模块已经进入进程后，还会报告：

- `LoadedNativeBridgePath`；
- `LoadedOcctKernelPath`；
- `NativeBridgeLoaded`；
- `OcctKernelLoaded`。

第一次 Native 调用之前 `NativeBridgeLoaded == false` 并不代表错误，因为 `GetDiagnosticInfo()` 本身不会触发加载。

## 排查 Win32 126

推荐先做结构化判断：

```csharp
var info = OcctRuntime.GetDiagnosticInfo();

if (!info.ApplicationNativeBridgeExists)
{
    // EXE 同目录没有 OcctNative.dll。
}

if (info.ConfiguredNativeBridgeExists == false)
{
    // OCCT_BRIDGE_NATIVE_DIR 已配置，但目录中没有 OcctNative.dll。
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

Native 操作成功以后，实际 Loaded 路径特别适合判断进程是否意外加载了其他目录中的旧 DLL。

## 文本报告

`GetDiagnosticReport()` 仍适合直接写日志或复制完整诊断块，内容包括：

- Runtime 是否已配置；
- BaseDirectory；
- app-local Bridge/Kernel 是否存在；
- 已配置的 Native/OCCT 路径；
- Repository Probing 状态；
- Native Bridge 候选路径；
- 关键 OCCT Resource 环境变量。

报告是纯观察行为。读取报告不得修改 `PATH`、`OCCT_BRIDGE_NATIVE_DIR`、`OCCT_ROOT` 或 `CASROOT`。

## Runtime 代码组织

Runtime 职责已经拆开，不再继续堆在一个大文件：

```text
OcctRuntime.cs                 配置状态与重配置冲突校验
OcctRuntime.Probing.cs         Bridge/OCCT/仓库/资源路径探测
OcctRuntime.Environment.cs     DLL Search Policy、PATH、OCCT Resource 环境变量
OcctRuntime.Diagnostics.cs     结构化诊断与文本报告
```

这是内部职责整理；对外仍然是同一个 `OcctRuntime` 静态 partial 类型，不增加平行 RuntimeManager。

## 桌面 UI 集成

WinForms、WPF、Avalonia 的启动诊断区域建议优先显示：

1. 进程架构；
2. app-local Bridge/Kernel 是否存在；
3. 配置 Bridge/Kernel 是否存在；
4. 实际 Loaded Bridge/Kernel 路径；
5. 完整 `DiagnosticReport` 放在详细信息中。

比解析中英文日志字符串更稳定。

## 路径隐私

诊断信息包含本机文件路径。将内容发到 Issue、邮件或外部支持渠道之前，应按需要检查并脱敏用户名、工程目录、网络共享路径等环境信息。

## 与发布校验的关系

Runtime 诊断不会替代发布校验。`demo` 的 `publish.ps1` 仍负责解析 Native 依赖闭包，并在发布前执行受限 `LoadLibraryExW` 探针。

- **Publish Validation**：判断发布包是否具有完整可加载的依赖闭包；
- **Runtime Diagnostics**：判断当前进程此刻看到了什么路径、实际加载了什么模块。

正式发布优先使用 app-local Native Runtime；只有开发/部署布局明确需要时才使用 `OCCT_BRIDGE_NATIVE_DIR`、`OCCT_ROOT` 或 `CASROOT`。
