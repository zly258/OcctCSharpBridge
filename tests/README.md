# Tests and Validation

`tests` 保留三类验证：**少量仓库级静态契约、Managed 回归测试、真实 Native Smoke**。Bridge 3 仅支持 ABI 5，不保留 ABI 4 Consumer、兼容测试或兼容入口。验证由本地 `build.ps1` / `build.sh`、精确的 .NET SDK 10.0.302 和真实 OCCT 7.9.0 环境完成。

## 1. 静态契约

当前保留 6 个 Windows/仓库级检查脚本，Linux 另有对应平台契约检查：

| Script | Responsibility |
| --- | --- |
| `check-version-contract.ps1` | 校验 `bridge-contract.json` 与 Native/Managed/.NET/CMake 的版本、ABI、平台和 SDK 契约 |
| `check-architecture-boundaries.ps1` | 校验 Core/UI 依赖方向、Managed/Native 领域目录、Interop 归属，以及禁止应用层和兼容层回流 |
| `check-abi5-contract.ps1` | 保证 ABI 5 是唯一受支持 Native ABI，拒绝 pre-ABI5 文件、元数据、Handle 与 Binary SDK Manifest 残留 |
| `check-bulk-abi.ps1` | 高数量 Modeling 集合与 Selection Hit 必须保持 Snapshot/Buffer ABI，禁止恢复 N+1 indexed ABI |
| `check-native-build-structure.ps1` | 校验 CMake Native 源清单、领域边界、平台隔离和 OCCT 7.9 数据交换 Toolkit |
| `check-api-surface.ps1` | 校验 Native declaration/definition 与 Core `LibraryImport + Cdecl` 一一对应；UI Adapter 可调用 Win32/X11 等平台 API，但不得自行声明 `occt_*` Bridge ABI 入口 |
| `check-linux-contract.sh` | Linux x64 的 ABI5、TFM、构建、发布与 Manifest 平台契约 |

Windows 静态验证：

```powershell
.\build.ps1 validate Release
```

Linux：

```bash
./build.sh validate Release
```

静态脚本只维护长期仓库不变量。具体方法内部实现、文件长度、README 固定文案等不作为契约；这些由编译、Managed Test、Native Smoke 和代码评审覆盖。`OcctNet` Core 的 Bridge C ABI 绑定必须全部使用 source-generated `LibraryImport`；WPF/Avalonia 等宿主自身所需的操作系统 P/Invoke 不属于 Bridge ABI，但 UI Adapter 不能绕过 Core 直接绑定 `occt_*`。

## 2. Managed 回归

`OcctNet.ManagedTests` 使用 `MSTest.Sdk`，根目录 `global.json` 固定 .NET SDK 10.0.302 并选择 Microsoft Testing Platform。

测试不加载 OCCT，主要覆盖：

- Value Type 与 Guard；
- Owner-aware Handle 语义；
- Geometry/Transform 纯 Managed 行为；
- Runtime Diagnostic 无副作用；
- Viewport Interaction Policy；
- Inertia、Intersection、Topology Reference 等 DTO Mapping。

执行：

```powershell
.\build.ps1 test Release
```

或：

```powershell
dotnet test .\tests\OcctNet.ManagedTests\OcctNet.ManagedTests.csproj -c Release -p:Platform=x64
```

## 3. Native Smoke

`OcctNet.Smoke` 使用当前构建的 ABI5 `OcctNative.dll` / `libOcctNative.so` 和 OCCT 7.9.0，验证只有真实 Native 执行才能确认的行为：

- Native Bridge 加载、ABI 5 与精确 BridgeVersion 配对；
- Primitive / Boolean / Feature；
- Geometry / Topology；
- Selection / Viewer 关键路径；
- Mesh；
- STEP / IGES / BREP / STL；
- Inertia、Structured Intersection、Topology Reference 等关键能力。

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

Avalonia Viewer 的 Linux Smoke 需要 X11/XWayland Display：

```bash
./build.sh avalonia-smoke Release
```

## 4. 构建缓存与清理

默认构建保留 `bin/obj`，交给 MSBuild/CMake 做增量判断。只有确实需要全量重建时执行：

```powershell
.\build.ps1 clean Release
```

不要在日常构建前手工删除每个项目的 `bin/obj`。

## 5. 推荐验证顺序

日常 Managed 修改：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
```

涉及 Native / ABI / OCCT：

```powershell
.\build.ps1 all Release
```

Windows 与 Linux 的 `all` 都执行：静态契约检查、Native 构建、Managed 构建、Managed Tests 和 Native Smoke。`docs` 与 `dist` 保持独立，不会在默认 `all` 中修改生成文档或 Binary SDK。

## 6. 新增检查原则

新增静态 Contract Check 前，先确认：

1. 它是仓库级长期不变量，而不是当前实现细节；
2. 编译器、Managed Test 或 Native Smoke 无法更可靠地覆盖；
3. 正常重命名、移动内部实现或文档整理不会触发误报；
4. 与已有脚本没有重复职责。

不满足时，优先补充现有检查、测试或 Smoke，不新增重复脚本。
