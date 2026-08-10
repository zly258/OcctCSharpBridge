# Tests and Validation

`tests` 目录只保留三类验证：**少量仓库级静态契约、Managed 回归测试、真实 Native Smoke**。静态 PowerShell 不再承担“逐文件检查某段实现文本”的职责；能通过编译、单元/回归测试或真实 OCCT 执行验证的内容，优先交给对应层级验证。

## 1. 静态契约

当前只保留 6 个脚本：

| Script | Responsibility |
|---|---|
| `check-version-contract.ps1` | `bridge-contract.json` 与 Native/Managed/.NET/CMake 的版本、平台和数量契约 |
| `check-architecture-boundaries.ps1` | Core/UI 依赖方向、`main`/应用层边界、禁止兼容层和 CAD Framework 下沉 |
| `check-bulk-abi.ps1` | 高数量 Modeling 集合与 Selection Hit 必须保持 Bulk ABI，禁止恢复 N+1 indexed ABI |
| `check-native-build-structure.ps1` | CMake Native 源清单、OCCT 7.9 数据交换 Toolkit、禁止 OCAF/XDE |
| `check-api-surface.ps1` | Native declaration/definition/PInvoke 对等、CallingConvention/ExactSpelling、API 数量 |
| `check-sdk-package.ps1` | 四个 Managed SDK 项目的 NuGet 元数据与目标框架一致性 |

执行：

```powershell
.\build.ps1 validate Release
```

### 静态脚本不再检查

以下内容不再通过 `Contains()`/固定字符串扫描强制：

- 某个几何 API 必须放在指定 `.cs/.cpp` 文件；
- Runtime 某个方法必须位于某个 partial 文件；
- Viewer/Selection 的具体实现语句；
- Smoke 测试源文件必须包含某个调用文本；
- README/docs 必须存在某段固定文案；
- 某个源码文件不得超过人为规定的字节数。

这些规则会导致正常重构、文件移动或文档整理造成无意义失败。

## 2. Managed 回归

`OcctNet.ManagedTests` 不加载 OCCT，使用 `MSTest.Sdk` + 标准 `dotnet test`，验证：

- Value Type 与 Guard；
- Owner-aware Handle 语义；
- Geometry/Transform 的纯 Managed 行为；
- Runtime Diagnostic 的无副作用行为；
- Viewport Interaction Policy；
- Inertia、Intersection、Topology Reference 等 DTO Mapping。

执行：

```powershell
.\build.ps1 test Release
```

也可以直接执行：

```powershell
dotnet test .\tests\OcctNet.ManagedTests\OcctNet.ManagedTests.csproj -c Release -p:Platform=x64
```

测试现在由 Test Explorer/CLI 正常发现，每个 TestMethod 独立报告失败，不再依赖顶层 EXE Runner 或 `[ModuleInitializer]` 隐式执行。

## 3. Native Smoke

`OcctNet.Smoke` 使用真实 `OcctNative.dll` 和 OCCT 7.9.0，验证只有运行 Native 才能确认的行为：

- DLL 加载与 ABI Compatibility；
- Primitive/Boolean/Feature；
- Geometry/Topology；
- Selection/Viewer 关键路径；
- Mesh；
- STEP/IGES/BREP/STL；
- Inertia、Structured Intersection、Topology Reference 等关键能力。

执行：

```powershell
.\build.ps1 smoke Release
```

其它 OCCT 路径：

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

## 4. 构建缓存与清理

默认构建保留 `bin/obj`，交给 MSBuild/CMake 做增量判断。只有确实需要全量重建时执行：

```powershell
.\build.ps1 clean Release
```

不要在日常构建前手工删除每个项目的 `bin/obj`，否则会失去增量编译收益。

## 5. 推荐验证顺序

日常修改：

```powershell
.\build.ps1 validate Release
.\build.ps1 managed Release
.\build.ps1 test Release
```

准备提交：

```powershell
.\build.ps1 ci Release
```

涉及 Native/ABI/OCCT 算法或准备发布：

```powershell
.\build.ps1 smoke Release
```

## 6. 新增检查的原则

新增 PowerShell Contract Check 前，先判断它是否满足：

1. 这是仓库级长期不变量，而不是当前实现写法；
2. 编译器、Managed Test 或 Native Smoke 无法更可靠地覆盖；
3. 检查不会因为重命名内部文件、移动方法或修改文档措辞而失败；
4. 与现有脚本没有重复职责。

不满足这些条件时，不新增静态脚本。