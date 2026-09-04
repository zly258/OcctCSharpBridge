# Demo 分支说明

`demo` 是已安装 OcctCSharpBridge SDK 的参考 Consumer，不包含 Bridge 实现源码，也不负责同步或重新编译 Bridge。

## 项目

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

## SDK 使用方式

Bridge `main` 负责 SDK 的构建、校验和安装；Demo 只直接消费已安装 SDK。

Windows 默认路径：

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

Linux 默认路径：

```text
$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64
```

已安装 SDK 同时包含 Binary SDK 和与其完全匹配的 `portable/` Runtime Closure。Demo 仓库不再保存第二套 Bridge SDK，也没有 sync 步骤。

## Consumer 边界

- 不跟踪 Bridge Native/Core 实现源码；
- 不直接导入 `occt_*` ABI；
- 不使用 pre-ABI5 兼容 API；
- 不维护第二套 OCCT Dependency Collector；
- 不在 Demo 内 clone、sync 或重新构建 Bridge。

## 发布

Windows 使用 `publish.ps1 all Release` 生成 WinForms/WPF/Avalonia 统一包；Linux 使用 `publish.sh Release` 发布 Avalonia。两端都直接消费已安装的 Bridge SDK 和其中匹配的 Portable Runtime。

Linux Native 兼容范围仍由 OCCT 与 `libOcctNative.so` 的 glibc/libstdc++ ABI 构建基线决定。

第三方项目的工程结构、引用、部署与版本锁定应以 Bridge `main` 下的正式 SDK Consumer 文档为准。
