# Demo 分支说明

`demo` / `demo` 是 OcctCSharpBridge Binary/Portable SDK 的参考 Consumer，不包含 Bridge 实现源码。

## 项目

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

## SDK 同步

`dist/` 是可删除的本地缓存。缓存命中时只校验 `sourceCommit` 和 Package Hash，不进行 Bridge 编译。

缓存失效时只使用 **Bridge `dist Release` Consumer 快路径**：

```text
Bridge dist Release
→ 最小 Binary SDK
→ Bridge Portable SDK Packager
→ Contract/sourceCommit/Hash 校验
→ Demo dist Cache
```

Demo 同步不得运行 Bridge `sdk`、`all`、ManagedTests、Core Smoke 或 Viewport/窗口 Smoke；这些属于 Bridge Release Validation 职责。

Windows：

```powershell
.\sync.ps1
.\sync.ps1 -ForceRebuild
```

Linux：

```bash
./sync.sh
./sync.sh --force-rebuild
```

如果已有匹配的 Binary + Portable SDK，可以直接传入，Bridge 编译次数为 0。

## Consumer 边界

- 不跟踪 Bridge Native/Core 实现源码；
- 不直接导入 `occt_*` ABI；
- 不使用 pre-ABI5 兼容 API；
- 不维护第二套 OCCT Dependency Collector；
- 不在 Consumer Sync 中隐藏 Bridge 完整 Release Gate。

Consumer Contract Check 会守住这些边界。

## 发布

Windows `publish.ps1 all Release` 默认生成统一包：`apps/` 下放三个应用，`dotnet/` 放一份共享私有 .NET 10 Desktop Runtime，`runtime/` 只放一份 Bridge/OCCT Closure，并共享 OCCT Resources。显式 `-SelfContained` 才生成三个应用各自的 Runtime Closure；显式 `-FrameworkDependent` 才要求目标机安装 .NET Runtime。

Linux 发布 Avalonia，并合并匹配的 Bridge Portable Runtime/Resources。Linux Native 兼容范围仍由 OCCT 与 `libOcctNative.so` 的 glibc/libstdc++ ABI 构建基线决定。

第三方项目的工程结构、引用、部署与版本锁定应以正式 Bridge `main` / `main` 文档中的 `09_第三方项目消费SDK.md` 为准。
