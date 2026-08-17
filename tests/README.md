# Tests and Validation

`tests` 保留五类验证：**仓库级静态契约、Consumer 兼容编译矩阵、Managed 回归测试、Core Native Smoke、Viewport Host Smoke**。Bridge 3 仅支持 ABI 5，不保留 ABI 4 Consumer、兼容测试或兼容入口。验证由本地 `build.ps1` / `build.sh`、稳定版 .NET 10 SDK（`10.0.100 + latestFeature`）和真实 OCCT 7.9.0 环境完成；不再要求精确某一个 SDK 补丁号。

## 1. 静态契约

当前保留 7 个 Windows/仓库级检查脚本，Linux 另有对应平台契约检查：

| Script | Responsibility |
| --- | --- |
| `check-version-contract.ps1` | 校验 `bridge-contract.json` 与 Native/Managed/.NET/CMake 的版本、ABI、平台和 SDK 契约 |
| `check-architecture-boundaries.ps1` | 校验 Core/UI 依赖方向、Managed/Native 领域目录、Interop 归属，以及禁止应用层、Demo consumer 和兼容层回流到 SDK source line |
| `check-abi5-contract.ps1` | 保证 ABI 5 是唯一受支持 Native ABI，拒绝 pre-ABI5 文件、元数据、Handle 与 Binary SDK Manifest 残留 |
| `check-bulk-abi.ps1` | 高数量 Modeling 集合与 Selection Hit 必须保持 Snapshot/Buffer ABI，禁止恢复 N+1 indexed ABI |
| `check-native-build-structure.ps1` | 校验 CMake Native 源清单、领域边界、平台隔离和 OCCT 7.9 数据交换 Toolkit |
| `check-api-surface.ps1` | 校验 Native declaration/definition 与 Core `LibraryImport + Cdecl` 一一对应；UI Adapter 不得自行声明 `occt_*` Bridge ABI 入口 |
| `check-consumer-matrix.ps1` | 保证 Core/Avalonia 与 WinForms/WPF Consumer Matrix 的 `TargetFrameworks` 与 `bridge-contract.json` 中支持列表完全一致 |
| `check-linux-contract.sh` | Linux x64 的 ABI5、TFM、构建、发布与 Manifest 平台契约，并防止 Linux `publish.sh` 恢复旧 metadata 或自动 Git commit/push |

Windows 静态验证：

```powershell
.\build.ps1 validate Release
```

Linux：

```bash
./build.sh validate Release
```

静态脚本只维护长期仓库不变量。具体方法内部实现、文件长度、README 固定文案等不作为契约；这些由编译、Managed Test、Native Smoke 和代码评审覆盖。`OcctNet` Core 的 Bridge C ABI 绑定必须全部使用 source-generated `LibraryImport`；WPF/Avalonia 等宿主自身所需的操作系统 P/Invoke 不属于 Bridge ABI，但 UI Adapter 不能绕过 Core 直接绑定 `occt_*`。

## 2. Consumer 兼容编译矩阵

兼容矩阵只做**编译验证**，不会加载 OCCT，也不会创建 Native Viewer：

```text
OcctNet.ConsumerMatrix
  net8.0
  net9.0
  net10.0
  -> OcctNet + OcctNet.Avalonia

OcctNet.DesktopConsumerMatrix
  net8.0-windows
  net9.0-windows
  net10.0-windows
  -> OcctNet.WinForms + OcctNet.Wpf
```

执行：

```powershell
.\build.ps1 consumer Release
```

这里验证的是**同一套以 .NET 8 为最低 TFM 的 Bridge Managed DLL 是否能被 .NET 8/9/10 Consumer 编译引用**。不会为三个 TFM 各自产生一套 Binary SDK。

`tests/check-consumer-matrix.ps1` 从 `bridge-contract.json` 读取 `supportedConsumerFrameworks` / `supportedDesktopConsumerFrameworks`，因此修改支持矩阵时必须同时更新正式 Contract，而不是在测试项目里单独硬编码另一套事实源。

## 3. Managed 回归

`OcctNet.ManagedTests` 使用 `MSTest.Sdk`。根目录 `global.json` 以 `10.0.100` 为稳定版 .NET 10 SDK 基线并使用 `latestFeature` 滚动，同时选择 Microsoft Testing Platform。

测试不加载 OCCT，主要覆盖：

- Value Type 与 Guard；
- Owner-aware Handle 语义；
- Geometry/Transform 纯 Managed 行为；
- Runtime Diagnostic 无副作用；
- 平台无关 Pointer/Key/InteractionFeatures 输入契约与未知 flag 拒绝；
- Hover identity tracker：同一 Owner/Subshape 内 Point/Depth 变化不重复触发；
- Viewport Host lifecycle / generation / first-frame options / `NativeHandleChanged` DTO；
- `OcctEdgeProjectionResult` / `OcctFaceProjectionResult` ABI layout；
- Inertia、Intersection、Topology Reference 等 DTO Mapping。

MSTest Analyzer 保持启用；测试代码必须遵循 `Assert.AreEqual(expected, actual)` 等 analyzer 规则，不通过禁用规则绕过错误。

执行：

```powershell
.\build.ps1 test Release
```

或直接调用项目：

```powershell
dotnet test .\tests\OcctNet.ManagedTests\OcctNet.ManagedTests.csproj -c Release -p:Platform=x64
```

日常仓库验证优先使用 `build.ps1`，因为它会先按 Contract 解析稳定版 .NET 10 SDK 并运行静态 Contract Checks。

## 4. Core Native Smoke

`OcctNet.Smoke` 使用当前构建的 ABI5 `OcctNative.dll` / `libOcctNative.so` 和 OCCT 7.9.0，验证只有真实 Native 执行才能确认的行为：

- Native Bridge 加载、ABI 5 与精确 BridgeVersion 配对；
- Primitive / Boolean / Feature；
- Geometry / Topology；
- Selection / Viewer 关键路径；
- Mesh；
- STEP / IGES / BREP / STL；
- Inertia、Structured Intersection、Topology Reference 等关键能力；
- Shape / Mesh / Algorithm owned resource 生命周期，包括源 Registry entry 或 Modeling Session 释放后的独立可用性。

Windows：

```powershell
.\build.ps1 smoke Release
```

指定其它 OCCT 路径：

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

Linux：

```bash
./build.sh smoke Release
```

## 5. Viewport Host Smoke

Windows 对三个正式 UI Adapter 分别保留最小 Native Host Smoke：

| Project | Validation |
| --- | --- |
| `OcctNet.WinFormsSmoke` | WinForms HWND host、HostState、EngineGeneration、RenderReady、`NativeHandle`/`NativeHandleChanged`、first frame、Box/Fit/Redraw、Dispose |
| `OcctNet.WpfSmoke` | WPF HwndHost、HostState、EngineGeneration、RenderReady、`NativeHandle`/`NativeHandleChanged`、first frame、Box/Fit/Redraw、Dispose |
| `OcctNet.AvaloniaSmoke` | Avalonia Windows HWND 或 Linux X11/XWayland XID host、HostState、EngineGeneration、RenderReady、`NativeHandle`/`NativeHandleChanged`、first frame、Box/Fit/Redraw、Edge/Face point projection、Dispose |

Avalonia Smoke 还验证：

- `ProjectPointToEdge` 内部最近点、裁剪端点和 `[0,1]` normalized parameter；
- 投影 parameter 通过 `EvaluateEdge` 回代到同一点；
- `ProjectPointToFace` 最近点、距离、UV；
- UV 通过 `EvaluateFace` 回代到同一点。

Windows 一次运行三个 Host Smoke：

```powershell
.\build.ps1 viewport-smoke Release
```

`all` 同时覆盖 Consumer Matrix、Core Smoke 与三个 Windows Viewport Host Smoke：

```powershell
.\build.ps1 all Release
```

如果还需要在完整 Gate 通过后生成正式 Windows Binary SDK：

```powershell
.\build.ps1 sdk Release
```

Linux 只存在 Avalonia UI Adapter，且 Viewer Smoke 需要可用的 X11/XWayland `DISPLAY`：

```bash
./build.sh avalonia-smoke Release
```

Linux Headless 环境仍可以运行 `validate`、`managed`、`test` 和 Core `smoke`；交互式 Avalonia Host Smoke 不伪装成 headless test。

## 6. 构建缓存与清理

默认构建保留 `bin/obj`，交给 MSBuild/CMake 做增量判断。只有确实需要全量重建时执行：

```powershell
.\build.ps1 clean
```

不要在日常构建前手工删除每个项目的 `bin/obj`。

## 7. 推荐验证顺序

日常 Managed 修改：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 consumer Release
.\build.ps1 test Release
```

涉及 Native / ABI / OCCT：

```powershell
.\build.ps1 smoke Release
```

涉及 WinForms/WPF/Avalonia Adapter、输入、生命周期、Native Handle 或首帧行为：

```powershell
.\build.ps1 viewport-smoke Release
```

Windows 完整 Gate：

```powershell
.\build.ps1 all Release
```

Windows 完整 Gate + Binary SDK：

```powershell
.\build.ps1 sdk Release
```

Windows `all` 覆盖静态契约检查、Native/Managed 构建、Consumer Matrix、Managed Tests、Core Native Smoke 和三个 Viewport Host Smoke，但不修改 `dist`。`sdk` 在同一套完整 Gate 后直接复用已验证 Native/Managed 输出生成 `dist/win-x64`。底层 `dist` 只负责 Release 打包，不运行 Consumer/Regression/Smoke，因此不作为正式 Release Gate。

Linux `all` 覆盖静态契约、Native、Managed、Managed Tests 与 Core Smoke；Avalonia Linux Viewer Smoke 保持显式目标，因为它要求图形 Display。

完整 target、环境要求、SDK 解析和发布说明见：

- `docs/zh-CN/08_构建测试与发布.md`
- `docs/en-US/08_Build-Test-and-Publish.md`

## 8. 新增检查原则

新增静态 Contract Check 前，先确认：

1. 它是仓库级长期不变量，而不是当前实现细节；
2. 编译器、Managed Test 或 Native Smoke 无法更可靠地覆盖；
3. 正常重命名、移动内部实现或文档整理不会触发误报；
4. 与已有脚本没有重复职责。

不满足时，优先补充现有检查、测试或 Smoke，不新增重复脚本。
