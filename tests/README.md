# Tests and Validation

`tests` 只保留三类验证：**少量仓库级静态契约、Managed 回归测试、真实 Native Smoke**。仓库不依赖 GitHub Actions；验证由本地 `build.ps1`、`.NET 10.0.302`、MSVC 和真实 OCCT 7.9.0 环境完成。

## 1. 静态契约

当前只保留 6 个脚本：

| Script | Responsibility |
|---|---|
| `check-version-contract.ps1` | `bridge-contract.json` 与 Native/Managed/.NET/CMake 的版本、平台和数量契约，同时验证 .NET 10 MTP Runner |
| `check-architecture-boundaries.ps1` | Core/UI 依赖方向、`main`/应用层边界、禁止兼容层和 CAD Framework 下沉 |
| `check-bulk-abi.ps1` | 高数量 Modeling 集合与 Selection Hit 必须保持 Bulk ABI，禁止恢复 N+1 indexed ABI |
| `check-native-build-structure.ps1` | CMake Native 源清单、OCCT 7.9 数据交换 Toolkit、禁止 OCAF/XDE |
| `check-api-surface.ps1` | Native declaration/definition/PInvoke 对等、CallingConvention/ExactSpelling、API 数量 |
| `check-sdk-package.ps1` | 四个 Managed SDK 项目的 NuGet 元数据与目标框架一致性 |

执行：

```powershell
.\build.ps1 validate Release
```

静态脚本不检查具体方法必须位于哪个 partial 文件、某段实现源码必须逐字存在、README 固定文案或人为源文件大小限制。这些内容由编译、测试和代码评审覆盖。

## 2. Managed 回归

`OcctNet.ManagedTests` 使用 `MSTest.Sdk`。由于仓库固定 .NET 10，根目录 `global.json` 明确设置：

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

因此 `dotnet test` 走 .NET 10 的 Microsoft Testing Platform，而不是旧 VSTest Target。测试不加载 OCCT，覆盖：

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

每个 TestMethod 独立报告失败，不使用顶层 EXE Runner 或 `[ModuleInitializer]` 隐式执行。

## 3. Native Smoke

`OcctNet.Smoke` 使用真实 `OcctNative.dll` 和 OCCT 7.9.0，验证只有 Native 执行才能确认的行为：

- DLL 加载与 ABI Compatibility；
- Primitive/Boolean/Feature；
- Geometry/Topology；
- Selection/Viewer 关键路径；
- Mesh；
- STEP/IGES/BREP/STL；
- Inertia、Structured Intersection、Topology Reference 等关键能力。

运行前，`build.ps1` 只把当前构建的 `OcctNative.dll` 放到 Smoke 输出目录，并给 Smoke 子进程显式设置解析后的 `OCCT_ROOT`。`OcctRuntime` 会在首次加载 Bridge 前把 OCCT `win64/vc14/bin` 与 `3rdparty-vc14-64` 下的运行时目录注册到 Windows DLL 搜索路径，因此测试不依赖机器 PATH 中偶然存在的 OCCT DLL，也不需要把整套 Runtime 平铺复制到测试目录。

执行：

```powershell
.\build.ps1 smoke Release
```

其它 OCCT 路径：

```powershell
.\build.ps1 smoke Release -OcctRoot "E:\SDK\occt-7.9.0"
```

如果仍出现 Win32 126，应检查 `OCCT_ROOT`、`TKernel.dll` 和第三方 Runtime 目录结构，定位实际缺失依赖。

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

涉及 Native/ABI/OCCT：

```powershell
.\build.ps1 all Release
.\build.ps1 smoke Release
```

## 6. 新增检查原则

新增 PowerShell Contract Check 前，先确认：

1. 它是仓库级长期不变量，不是当前实现写法；
2. 编译器、Managed Test 或 Native Smoke 无法更可靠地覆盖；
3. 正常重命名、移动内部方法或文档整理不会触发误报；
4. 与已有脚本没有重复职责。

不满足时，不新增静态脚本。
