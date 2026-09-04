# OcctCSharpBridge Demo Development

[简体中文](README.zh-CN.md) · [Bridge Development SDK](https://github.com/zly258/OcctCSharpBridge/tree/main) · [Third-party SDK Guide](https://github.com/zly258/OcctCSharpBridge/blob/main/docs/en-US/09_Third-Party-SDK-Consumption.md)

`demo` is a reference consumer of the installed OcctCSharpBridge SDK. It does not contain Bridge implementation source, clone Bridge, or maintain synchronized SDK copies.

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

The Demo targets .NET 10 to cover the latest supported consumer runtime.

## Installed Bridge SDK

Bridge `main` owns SDK production. Install/update the SDK there, then use Demo directly.

Windows default:

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

Linux default:

```text
$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64
```

The installed SDK contains the Binary SDK at its root and the exact matching runtime closure under `portable/`. `OCCTCSHARPBRIDGE_SDK` overrides the root on both platforms.

There is no Demo-side sync workflow.

## Build and run

Windows:

```powershell
.\build.ps1 all Release
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

Linux:

```bash
./build.sh all Release
./run.sh Release
```

## Samples

The shared `OcctDemo.Common` layer keeps only integrated examples that are not replaced by normal CAD commands:

1. **Section Analysis** — split by plane and display positive, negative and section results.
2. **Drawing Projection** — Front, Top, Right and Isometric HLR projections.
3. **Distance & Extrema** — curve/curve extrema and nearest-point visualization.
4. **Model Repair** — FixShape with before/after inspection.

## Windows publication

```powershell
.\publish.ps1 all Release -Zip
```

The unified package uses one Bridge/OCCT runtime closure. The default `all` layout also uses one shared private .NET 10 Desktop Runtime; `-SelfContained` and `-FrameworkDependent` remain explicit alternatives.

## Linux publication

```bash
./publish.sh Release
```

Linux publishes the Avalonia application and merges the Portable Runtime directly from the installed Bridge SDK. No `sync.sh`, clone, or `external/OcctCSharpBridge` cache is involved.

Linux native compatibility remains constrained by the glibc/libstdc++ ABI baseline used to build OCCT and `libOcctNative.so`.

## Consumer boundary

- no tracked `src/OcctNative` or `src/OcctNet*` implementation source;
- no direct `LibraryImport/DllImport("OcctNative")` declarations;
- no pre-ABI5 handles/metadata;
- no duplicate OCCT runtime-closure collector;
- no Bridge clone/sync/rebuild workflow in Demo.
