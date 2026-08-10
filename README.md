# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [Demo Maintenance](docs/README.md) · [Main Technical Docs](https://github.com/zly258/OcctCSharpBridge/tree/main/docs)

## Purpose

The `demo` branch is a **binary-only consumer** of OcctCSharpBridge. It no longer contains the native or managed Bridge source, ABI checks, managed regression tests, or native smoke tests from `main`.

Only four application projects remain:

```text
src/OcctDemo.Common
src/OcctDemo.WinForms
src/OcctDemo.Wpf
src/OcctDemo.Avalonia
```

The validated Bridge Binary SDK is consumed from `dist/win-x64`:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

### Preview

<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/winform-demo-en.png" alt="WinForms demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/wpf-demo-en.png" alt="WPF demo" width="88%"></p>
<p align="center"><img src="https://raw.githubusercontent.com/zly258/OcctCSharpBridge/demo/assets/previews/avalonia-demo-en.png" alt="Avalonia demo" width="88%"></p>

## Requirements

- Windows x64
- .NET SDK `10.0.302`
- OCCT `7.9.0` runtime

CMake and MSVC are no longer required to build the demo branch. They are required only on `main` when producing `OcctNative.dll`.

Default OCCT root:

```text
D:\tools\occt-vc144-64
```

Use `OCCT_ROOT` or `CASROOT` for another runtime location.

## 1. Sync the Binary SDK

On `main`, produce the validated payload:

```powershell
.\dist.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

`dist.ps1` runs the Release build, managed tests, and native smoke validation before refreshing `dist/win-x64`.

After committing that payload on `main`, switch to `demo` and run:

```powershell
.\sync-dist.ps1
```

This restores the exact `dist/win-x64` payload from `origin/main`; no Bridge source checkout is required.

## 2. Build

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release
```

Build one target:

```powershell
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

Validation checks the Binary SDK contract, manifest, and SHA-256 hashes and rejects any reintroduced `src/OcctNative` or `src/OcctNet*` source directories.

## 3. Run

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

`OcctNative.dll` is copied beside each executable. `run.ps1` configures the OCCT and third-party runtime search paths.

## 4. Publish

```powershell
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

Publishing consumes the Binary SDK and does not compile Bridge source.

## Project Structure

```text
dist/README.md              Binary SDK notes
dist/win-x64/               Validated DLL/contract/manifest payload from main
src/OcctDemo.Common/        Shared demo behavior
src/OcctDemo.WinForms/      WinForms demo
src/OcctDemo.Wpf/           WPF demo
src/OcctDemo.Avalonia/      Avalonia demo
assets/previews/            Demo screenshots
docs/README.md              Demo maintenance rules
OcctDemo.sln                Demo-only solution
build.ps1                   Demo build entry point
sync-dist.ps1               Sync Binary SDK from main
run.ps1                     Local runner
publish.ps1                 Demo publisher
```

## Dependency Rules

- Demo projects reference `dist/win-x64/OcctNet*.dll`, never Bridge `.csproj` files.
- Demo contains no `src/OcctNative`, `src/OcctNet*`, or Bridge `tests` directory.
- If the current Bridge changes, update demo callers instead of restoring legacy aliases or compatibility wrappers.
- Bridge API, ABI, native, runtime, modeling, and SDK documentation remain authoritative on `main` only.
- GitHub Actions are not used for building or branch synchronization.

## Native Runtime Troubleshooting

For `DllNotFoundException` or Win32 error 126, verify:

```text
application/OcctNative.dll
%OCCT_ROOT%/win64/vc14/bin/TKernel.dll
%OCCT_ROOT%/3rdparty-vc14-64/**/bin/*.dll
```

The Avalonia host still uses a Windows child HWND and therefore remains a Windows x64 application.

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE). Open CASCADE Technology and other third-party dependencies remain subject to their own licenses.
