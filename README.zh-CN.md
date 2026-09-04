# OcctCSharpBridge Demo 开发分支

[English](README.md) · [Bridge Development SDK](https://github.com/zly258/OcctCSharpBridge/tree/main) · [Bridge 第三方 SDK 接入说明](https://github.com/zly258/OcctCSharpBridge/blob/main/docs/zh-CN/09_%E7%AC%AC%E4%B8%89%E6%96%B9%E9%A1%B9%E7%9B%AE%E6%B6%88%E8%B4%B9SDK.md)

`demo` 是已安装 OcctCSharpBridge SDK 的参考 Consumer，不包含 Bridge 实现源码，不 clone Bridge，也不维护同步 SDK 副本。

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

Demo 自身使用 .NET 10，用于覆盖 Bridge 支持的最新 Consumer Runtime。

## 已安装 Bridge SDK

Bridge `main` 负责 SDK 的构建、校验、Portable Runtime 打包和安装。完成安装后，Demo 直接使用即可。

Windows 默认路径：

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

Linux 默认路径：

```text
$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64
```

已安装 SDK 根目录保存 Binary SDK，`portable/` 保存与之完全匹配的 Runtime Closure。两端均可用 `OCCTCSHARPBRIDGE_SDK` 覆盖 SDK Root。

**Demo 不再有 SDK sync 流程。**

## 构建与运行

Windows：

```powershell
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

Linux：

```bash
./build.sh all Release
./run.sh Release
```

## 综合能力案例

`OcctDemo.Common` 只保留普通 CAD 命令无法直接替代的综合案例：

1. **Section Analysis**：按平面拆分并显示正侧、负侧和截交结果。
2. **Drawing Projection**：生成 Front、Top、Right、Isometric 四个 HLR 工程投影。
3. **Distance & Extrema**：计算 Curve/Curve 极值并显示最近点。
4. **Model Repair**：执行 FixShape，并对比修复前后的检查结果。

## Windows 发布

```powershell
.\publish.ps1 all Release -Zip
```

统一包只保留一份 Bridge/OCCT Runtime Closure；默认 `all` 还使用一份共享私有 .NET 10 Desktop Runtime。`-SelfContained` 和 `-FrameworkDependent` 仍作为显式替代模式保留。

## Linux 发布

```bash
./publish.sh Release
```

Linux 直接从已安装 Bridge SDK 的 `portable/` 读取 Runtime Closure 并发布 Avalonia，不再需要 `sync.sh`，也不再使用 `external/OcctCSharpBridge` 缓存。

Linux Native 兼容范围仍由 OCCT 与 `libOcctNative.so` 的 glibc/libstdc++ ABI 构建基线决定。

## Consumer 边界

- 不跟踪 `src/OcctNative` 或 `src/OcctNet*` 实现源码；
- 不声明直接 `LibraryImport/DllImport("OcctNative")`；
- 不使用 pre-ABI5 Handle/Metadata；
- 不重新实现 OCCT Runtime Closure Collector；
- 不在 Demo 内 clone、sync 或重新构建 Bridge。
