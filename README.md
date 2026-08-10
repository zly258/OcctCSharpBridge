# OcctCSharpBridge Demo

[简体中文](README.zh-CN.md) · [Main SDK](https://github.com/zly258/OcctCSharpBridge) · [Demo Maintenance](docs/README.md) · [Main English Docs](https://github.com/zly258/OcctCSharpBridge/tree/main/docs/en-US)

The `demo` branch is a **Binary SDK-only consumer** of OcctCSharpBridge. It contains no native/managed Bridge source and does not own ABI checks, CMake validation, managed regression, or native smoke.

Application projects:

```text
src/OcctDemo.Common
src/OcctDemo.WinForms
src/OcctDemo.Wpf
src/OcctDemo.Avalonia
```

The Bridge is published from `main/publish.ps1` into `dist/win-x64`:

```text
OcctNative.dll
OcctNet.dll
OcctNet.WinForms.dll
OcctNet.Wpf.dll
OcctNet.Avalonia.dll
bridge-contract.json
bridge-manifest.json
```

## Requirements

- Windows x64
- .NET SDK `10.0.302`
- OCCT `7.9.0` runtime

CMake/MSVC are not required to build demo applications; the native toolchain belongs only to the Bridge production workflow on `main`.

## Binary SDK updates

The demo branch no longer owns a reverse synchronization script. Publishing starts from `main`:

```powershell
# main branch
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Main validates Release build, managed tests, native smoke, manifest and SHA-256, then uses a temporary worktree to publish `dist/win-x64` to demo.

## Build

```powershell
.\build.ps1 validate Release
.\build.ps1 all Release
```

Individual targets:

```powershell
.\build.ps1 common Release
.\build.ps1 winform Release
.\build.ps1 wpf Release
.\build.ps1 avalonia Release
```

Validation checks the Binary SDK contract, manifest and hashes and rejects reintroduced Bridge source.

## Run

```powershell
.\run.ps1 winform Release
.\run.ps1 wpf Release
.\run.ps1 avalonia Release
```

`OcctNative.dll` is copied beside each executable. `run.ps1` configures OCCT and third-party runtime paths.

## Publish demo applications

```powershell
.\publish.ps1 all Release -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

The demo `publish.ps1` publishes applications only. It consumes `dist/win-x64` and never builds Bridge source.

## Structure

```text
dist/win-x64/               Validated Binary SDK published by main
src/OcctDemo.Common/        Shared demo behavior
src/OcctDemo.WinForms/      WinForms demo
src/OcctDemo.Wpf/           WPF demo
src/OcctDemo.Avalonia/      Avalonia demo
assets/previews/            Demo screenshots
docs/README.md              Demo maintenance notes
OcctDemo.sln                Demo-only solution
build.ps1                   Demo build entry point
run.ps1                     Local runner
publish.ps1                 Demo application publisher
```

## Dependency rules

- Demo projects reference `dist/win-x64/OcctNet*.dll`, never Bridge `.csproj` files.
- Demo contains no `src/OcctNative`, `src/OcctNet*`, or Bridge tests.
- Update callers when the SDK changes; do not restore legacy aliases or compatibility wrappers.
- Bridge conceptual docs and complete bilingual API Reference are maintained only under `main/docs/zh-CN` and `main/docs/en-US`.
- GitHub Actions are not used for build or branch synchronization.

## Runtime troubleshooting

For `DllNotFoundException` or Win32 error 126, verify:

```text
application/OcctNative.dll
%OCCT_ROOT%/win64/vc14/bin/TKernel.dll
%OCCT_ROOT%/3rdparty-vc14-64/**/bin/*.dll
```

The Avalonia host still uses a Windows child HWND, so all demo applications are Windows x64.

## License

OcctCSharpBridge is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE).
