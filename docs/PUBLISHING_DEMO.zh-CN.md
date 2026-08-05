# WinForms/WPF Demo 免配置发布

[English](PUBLISHING_DEMO.md)

`publish.ps1` 用于生成可复制到其他 Windows x64 电脑直接运行的发布包。目标电脑无需安装 OCCT SDK、CMake、Visual Studio，也无需手工设置 OCCT 环境变量。脚本默认使用 .NET 自包含发布。

## 发布电脑要求

- Windows x64
- .NET 8 SDK
- Visual Studio 2022 C++ 构建工具
- CMake 3.21+
- 本仓库要求的 OCCT 7.9.0 SDK
- 对所有被复制运行库具有合法再分发权限

目标电脑不需要上述开发工具。

## 基本命令

同时发布 WinForms、WPF 并生成 ZIP：

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -OutputDirectory ".\artifacts\publish" `
  -Zip
```

只发布 WinForms：

```powershell
.\publish.ps1 winform Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

只发布 WPF：

```powershell
.\publish.ps1 wpf Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -Zip
```

## 参数说明

| 参数 | 取值/默认值 | 说明 |
|---|---|---|
| `Target` | `all`、`winform`、`wpf`，默认 `all` | 需要发布的应用 |
| `Configuration` | `Debug`、`Release`、`RelWithDebInfo`，默认 `Release` | 原生和托管构建配置 |
| `-OcctRoot` | `OCCT_ROOT` 或 `D:\tools\occt-vc144-64` | OCCT 7.9.0 SDK 根目录 |
| `-OutputDirectory` | `artifacts\publish` | 发布包父目录 |
| `-FrameworkDependent` | 默认关闭 | 不打包 .NET 运行时 |
| `-Zip` | 默认关闭 | 完成后生成 ZIP |
| `-KeepExisting` | 默认关闭 | 不清理旧发布目录 |

需要交给无需安装任何运行环境的用户时，不要使用 `-FrameworkDependent`。

## 脚本执行内容

1. 检查 `dotnet`、仓库输入文件和 OCCT 根目录。
2. 通过 `build.ps1 native` 构建 `OcctNative.dll`。
3. 将选中的桌面项目发布为 `win-x64`。
4. 将 `OcctNative.dll` 复制到共享运行库目录。
5. 复制 `win64\vc14\bin` 中的 OCCT 运行库 DLL。
6. 递归检测 `3rdparty-vc14-64` 下的第三方 DLL。
7. 复制当前系统可用的 x64 Visual C++ 运行库 DLL。
8. 复制实际存在的 OCCT 资源目录。
9. 复制项目、OCCT 和检测到的第三方许可证文件。
10. 生成使用相对路径的启动脚本。
11. 生成包含文件大小、版本和 SHA-256 的 `runtime-manifest.txt`。
12. 指定 `-Zip` 时创建压缩包。

遇到同名 DLL 时，脚本会比较 SHA-256：内容相同则忽略重复项；内容不同则停止发布，避免静默选择错误版本。

## 发布目录结构

```text
artifacts\publish\OcctCSharpBridge-Demo-win-x64
├─ apps
│  ├─ winform
│  │  └─ CAD-Winform.exe
│  └─ wpf
│     └─ CAD-WPF.exe
├─ runtime
│  ├─ OcctNative.dll
│  ├─ TKernel.dll
│  ├─ TK*.dll
│  ├─ 第三方 DLL
│  └─ Visual C++ 运行库 DLL
├─ occt
│  └─ src
│     ├─ Shaders
│     ├─ StdResource
│     ├─ UnitsAPI
│     ├─ XSMessage
│     ├─ XSTEPResource
│     └─ ...
├─ licenses
├─ Start-WinForms.cmd
├─ Start-WPF.cmd
├─ runtime-manifest.txt
└─ README.txt
```

只复制所选 OCCT 安装中实际存在的资源目录。

## 启动脚本如何工作

生成的 `.cmd` 根据发布包自身位置设置：

```text
PATH=<发布包>\runtime;%PATH%
OCCT_BRIDGE_NATIVE_DIR=<发布包>\runtime
OCCT_ROOT=<发布包>\occt
CASROOT=<发布包>\occt
```

随后从对应应用目录启动程序，因此发布包可以解压到任意路径，不需要修改绝对路径。

## 推荐分发流程

1. 使用 `Release` 和 `-Zip` 生成发布包。
2. 在干净的 Windows x64 电脑或虚拟机解压。
3. 确认目标机 `PATH` 中不存在开发电脑的 OCCT 路径。
4. 分别运行 `Start-WinForms.cmd` 和 `Start-WPF.cmd`。
5. 验证 Viewer 初始化、基本体创建、点选和框选。
6. 验证 STEP 导入导出和 BinXCAF 保存重开。
7. 对照 `runtime-manifest.txt` 检查文件完整性。
8. 检查 `licenses`，移除不允许再分发的组件。
9. 分发完整 ZIP，不要单独发送 EXE。

## 更新发布包

默认情况下，脚本会先删除已有发布目录，防止依赖更新后残留旧 DLL。

`-KeepExisting` 只建议用于诊断，不适合最终发布，因为旧文件可能掩盖依赖收集遗漏。

## 框架依赖发布

```powershell
.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -FrameworkDependent `
  -Zip
```

这种方式包体更小，但目标电脑必须安装兼容的 .NET 8 Desktop Runtime。OCCT 和原生依赖仍会被打包。

## 常见问题

### 无法加载 `OcctNative.dll`

- 必须从生成的 `.cmd` 启动，不要在其他工作目录直接运行。
- 检查 `runtime\OcctNative.dll` 是否存在。
- 检查其依赖 DLL，并对照 `runtime-manifest.txt`。
- 确认发布包和目标系统都是 x64。

### 缺少 `TK*.dll` 或第三方 DLL

- 确认发布时指定的是正确 OCCT 7.9.0 根目录。
- 检查 `3rdparty-vc14-64` 是否存在特殊运行时子目录。
- 检查是否存在同名但哈希不同的 DLL，脚本会将其报告为冲突。

### STEP 或 OCAF 资源错误

- 从启动脚本运行，确保 `CASROOT` 与 `OCCT_ROOT` 已设置。
- 检查 `occt\src` 下是否存在相应资源目录。
- 将发布包资源与构建原生 DLL 时使用的 OCCT SDK 对照。

### Visual C++ 运行库错误

脚本会复制发布电脑中可用的常见 x64 运行库 DLL。正式分发时，也可以采用 Microsoft 官方 Visual C++ Redistributable，并遵守其最新再分发条款。

## 安全与许可证

“自动复制 DLL”不代表自动获得再分发权限。应逐个检查第三方库的许可证和安全状态，保留许可证说明，并在升级 OCCT 或第三方依赖后重新生成发布包。
