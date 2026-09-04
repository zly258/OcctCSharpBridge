# Unified Demo Branch Notes

`demo` is a reference consumer of the installed OcctCSharpBridge SDK. It does not contain Bridge implementation source and does not synchronize or rebuild Bridge.

## Projects

```text
OcctDemo.Common
├─ OcctDemo.WinForms  → Windows x64
├─ OcctDemo.Wpf       → Windows x64
└─ OcctDemo.Avalonia  → Windows x64 / Linux x64
```

## SDK workflow

Bridge `main` owns SDK production and installation. Demo consumes that installed SDK directly.

Windows default:

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

Linux default:

```text
$HOME/.local/share/OcctCSharpBridge/SDK/3.0/linux-x64
```

The installed SDK contains the Binary SDK plus its matching Portable Runtime under `portable/`. Demo does not keep a second Bridge SDK under the repository and has no sync step.

## Consumer boundary

- no tracked Bridge Native/Core implementation source;
- no direct `occt_*` ABI imports;
- no pre-ABI5 compatibility APIs;
- no duplicate OCCT dependency collector;
- no Bridge clone/sync/rebuild workflow inside the Demo.

## Publication

Windows `publish.ps1 all Release` produces the unified WinForms/WPF/Avalonia package. Linux `publish.sh Release` publishes Avalonia. Both consume the installed Bridge SDK and its matching Portable Runtime.

Linux native compatibility still depends on the glibc/libstdc++ ABI baseline used to build OCCT and `libOcctNative.so`.

For third-party project architecture and deployment guidance, use the formal Bridge documentation under `main`, especially `docs/*/09_Third-Party-SDK-Consumption`.
