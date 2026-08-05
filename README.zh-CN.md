# OcctCSharpBridge 演示程序

OcctCSharpBridge 通过原生 C++ DLL、稳定 C ABI 和类型安全的 .NET 8 API 封装 Open CASCADE Technology 7.9.0。

`demo` 分支包含 WinForms 和 WPF 参考程序；可复用封装位于 `src/OcctNative` 和 `src/OcctNet`。

## 演示功能

- WinForms 与 WPF OCCT 三维视口
- 对象和子拓扑选择
- `Ctrl` 点选切换多选：再次 `Ctrl` 点选已选对象可取消选择
- 矩形框选
- 可修改选中高亮颜色和悬浮高亮颜色
- 纯色及渐变场景背景
- 环境光、相机直射光、太阳光和补光
- 中性、摄影棚、日光和平光预设
- 标准视图、投影、适配、平移、缩放和旋转
- 几何创建、拓扑分析、布尔和特征操作
- STEP、IGES、BREP 和 STL 数据交换
- OCAF、TNaming 和 XDE 封装示例
- 简体中文与英文界面

## 构建

```powershell
.\build.ps1 Release
```

## 运行

```powershell
.\run.ps1 winform
.\run.ps1 wpf
```

## 发布

默认生成精简的框架依赖 WinForms 发布包：

```powershell
.\publish.ps1
```

可通过发布脚本参数选择 WPF、自包含运行时、完整 OCCT 资源、诊断文件或 ZIP 输出。

## 接口清单

- [中文接口清单](docs/API_COVERAGE.zh-CN.md)
- [English API inventory](docs/API_COVERAGE.md)

## 环境要求

- Windows x64
- 源码构建需要 .NET 8 SDK
- 原生构建需要 OCCT 7.9.0
- 原生编译需要 Visual Studio C++ 工具和 CMake

重新分发前，请检查仓库许可证及发布包中的第三方许可证说明。
