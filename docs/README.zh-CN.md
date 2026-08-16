# Demo 分支说明

`demo` / `demo-dev` 是统一的 Binary SDK Consumer 分支。

平台：

- Windows x64：WinForms、WPF、Avalonia 三个 Demo。
- Linux x64：仅 Avalonia Demo。

Bridge SDK 只来自 `main` / `main-dev`；Demo 分支不包含 `OcctNative` 或 `OcctNet*` 实现源码，也不直接调用 `occt_*` ABI。

Windows 使用 `sync.ps1 / build.ps1 / run.ps1 / publish.ps1`。
Linux 使用 `sync.sh / build.sh / run.sh / publish.sh`。

`dist/` 为本地 SDK 缓存，不提交 Git；同步脚本按 `manifest.sourceCommit` 和 SHA256 校验 SDK。

独立 `avalonia` / `avalonia-dev` 分支迁移完成后废弃；`backup/*` 分支保持不变。
