# 部署与运行时目录

## 发布包需要包含什么

Viewer 或 OCAF 应用不仅需要托管程序集。一个可直接分发的 Windows x64 包通常包括：

1. `dotnet publish` 输出的应用文件。
2. `OcctNet.dll` 和业务程序集。
3. `OcctNative.dll`。
4. 原生桥接链接到的 OCCT Toolkit DLL。
5. 当前 OCCT 构建依赖的第三方 DLL，例如 FreeType、TBB、FreeImage 等。
6. Microsoft Visual C++ 可再发行运行库，或要求目标机已安装兼容版本。
7. 数据交换、持久化、单位、Shader、消息和纹理使用的 OCCT 资源目录。
8. 项目、OCCT、Microsoft 和第三方许可证说明。

依赖集合由编译 `OcctNative.dll` 时使用的 OCCT 构建决定，不能混用其他 OCCT 版本或编译器工具链的 DLL。

## 原生桥接查找

`OcctNet` 会从以下位置查找 `OcctNative.dll`：

- 应用基础目录；
- `OCCT_BRIDGE_NATIVE_DIR`；
- `OcctRuntime.Configure(...)` 配置的候选目录。

找到 `OcctNative.dll` 后，Windows 还必须解析其依赖 DLL。可以把依赖放在桥接 DLL 同目录，或在第一次 P/Invoke 前将运行时目录加入 `PATH`。

## OCCT 资源查找

数据交换和持久化除 DLL 外还需要资源文件。便携包应设置：

```cmd
set "OCCT_ROOT=%~dp0occt"
set "CASROOT=%~dp0occt"
```

并保留安装中实际存在的资源目录，例如：

```text
occt\src\StdResource
occt\src\UnitsAPI
occt\src\SHMessage
occt\src\XSMessage
occt\src\XSTEPResource
occt\src\Shaders
occt\src\Textures
occt\src\XmlOcafResource
```

## 推荐目录结构

```text
OcctCSharpBridge-Demo-win-x64
├─ apps
│  ├─ winform
│  └─ wpf
├─ runtime
│  ├─ OcctNative.dll
│  ├─ TK*.dll
│  ├─ TKernel.dll
│  ├─ 第三方 DLL
│  └─ VC++ 运行库 DLL
├─ occt
│  └─ src\...
├─ licenses
├─ Start-WinForms.cmd
├─ Start-WPF.cmd
├─ runtime-manifest.txt
└─ README.txt
```

启动脚本应根据自身位置设置 `PATH`、`OCCT_BRIDGE_NATIVE_DIR`、`OCCT_ROOT` 和 `CASROOT`，不能写开发电脑的绝对路径。

## 框架依赖与自包含

框架依赖发布体积更小，但目标电脑必须安装兼容的 .NET Desktop Runtime。

自包含发布会带上 .NET 运行时，更适合“解压即用”。但它不会自动包含 OCCT、第三方原生库、OCCT 资源和 VC++ 运行库，这些仍需额外打包。

## `demo` 分支发布

`demo` 分支提供 `publish.ps1`：

```powershell
.\publish.ps1 all Release `
  -OcctRoot "D:\tools\occt-vc144-64" `
  -OutputDirectory ".\artifacts\publish" `
  -Zip
```

脚本会：

- 构建 `OcctNative.dll`；
- 发布 WinForms 和/或 WPF 的 `win-x64` 程序；
- 默认使用 .NET 自包含发布；
- 复制 OCCT 运行库和检测到的第三方 DLL；
- 复制存在的 OCCT 资源目录；
- 尽可能复制可再发行的 VC++ 运行库文件；
- 生成相对路径启动脚本；
- 生成包含大小、版本和 SHA-256 的运行时清单；
- 按需生成 ZIP。

## 分发前验证

应在没有开发环境、没有 OCCT SDK 路径的干净 Windows x64 电脑或虚拟机上测试：

- WinForms 和 WPF 能从生成的启动脚本启动；
- Viewer 初始化成功；
- 基本体创建和选择正常；
- 矩形框选和指示框稳定；
- STEP 导入导出正常；
- BinXCAF 保存重开正常；
- 文字和字体渲染正常；
- 包内没有引用开发电脑的绝对 OCCT 路径。

目标机提示 DLL 缺失时，可使用 Process Monitor 或依赖检查工具定位。

## 再分发与许可证

脚本负责收集文件，但不能替代许可证判断。分发前需要检查：

- 本仓库 PolyForm Noncommercial License；
- OCCT LGPL 2.1 与 OCCT Exception；
- Microsoft Visual C++ 可再发行条款；
- 从 OCCT 安装目录复制的每个第三方组件许可证。

发布包应保留许可证文本，并移除不需要或不允许再分发的组件。
