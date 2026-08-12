# Tests and Validation

`avalonia` 分支只保留必要验证：**静态契约、Managed 回归、Headless Native Smoke、Avalonia Viewer Smoke**。不使用 GitHub Actions，不维护 Binary SDK 发布测试。

## 静态契约

当前保留 5 个仓库级检查：

| Script | Responsibility |
|---|---|
| `check-version-contract.ps1` | `bridge-contract.json` 与 Native/Managed/.NET/CMake 的版本、平台和数量契约 |
| `check-architecture-boundaries.ps1` | Core/Avalonia 依赖方向，以及禁止 WinForms/WPF、`dist`、发布脚本、兼容层回流 |
| `check-bulk-abi.ps1` | 高数量集合与 Selection Hit 保持 Bulk ABI |
| `check-native-build-structure.ps1` | Native 源清单与 OCCT Toolkit 依赖完整性 |
| `check-api-surface.ps1` | Native declaration/definition/PInvoke 对等与 API 数量 |

Windows 执行：

```powershell
.\build.ps1 validate Release
```

## Managed 回归

`OcctNet.ManagedTests` 使用 `.NET 10.0.302` 和 Microsoft Testing Platform，不加载 OCCT，覆盖纯 Managed DTO、Guard、Handle、Geometry/Transform、Runtime Diagnostic 和 Viewport Interaction Policy 等行为。

```powershell
.\build.ps1 test Release
```

Linux：

```bash
./build.sh test Release
```

## Headless Native Smoke

`OcctNet.Smoke` 使用真实 Native Bridge 和 OCCT 7.9.0，验证 ABI、建模、几何/拓扑、Mesh、STEP/IGES/BREP/STL 等关键路径，不要求图形桌面。

Windows：

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux：

```bash
./build.sh smoke Release
```

## Avalonia Viewer Smoke

`OcctNet.AvaloniaSmoke` 直接创建公开 `OcctAvaloniaViewport`，初始化真实 OCCT Viewer、绘制 Box 后自动退出。Windows 使用 HWND/WNT_Window；Linux 当前使用 X11/XWayland XID/Xw_Window。

Windows：

```powershell
.\build.ps1 avalonia-smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux 需在 X11/XWayland 桌面会话中执行：

```bash
./build.sh avalonia-smoke Release
```

不再保留独立 `X11Smoke`；完整 Avalonia Smoke 已覆盖实际 Host → Native Surface → OCCT Viewer 链路。

## 默认完整验证

Windows：

```powershell
.\build.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64"
```

Linux：

```bash
./build.sh all Release
```

两边的 `all` 都执行 Native + Managed + ManagedTests + Headless Smoke。Viewer Smoke 单独执行，因为 Linux 需要图形会话。

## 清理

```powershell
.\build.ps1 clean
```

```bash
./build.sh clean
```

日常构建保留 MSBuild/CMake 增量缓存；只有需要全量重建时再执行 `clean`。
