# Build and Publish

## Windows build

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
```

`build` compiles Native and Managed only. It never updates the machine-wide SDK.

## Windows publish/install

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64"
```

Publishing builds and validates the Release Binary SDK, then atomically installs it to:

```text
C:\Program Files\OcctCSharpBridge\SDK\3.0\win-x64
```

Use `OCCTCSHARPBRIDGE_SDK` or `-InstallRoot` to override the install root. Writing to `Program Files` normally requires an elevated PowerShell session.

Windows consumers reference the installed SDK directly. Repository-local sync copies are not the supported consumption model.

## Native viewport first frame

WPF and Avalonia hosts own initial native presentation. Applications add the viewport to the normal visual tree and may wait for `HostState == Ready` when work depends on a presented surface.

`Ready` means a usable arranged size has completed `ResizeSurface + Redraw`. Do not use mouse-motion simulation, arbitrary dispatcher delays, duplicate `FitAll`, or extra startup redraw calls to reveal the first frame.

## Linux

```bash
./build.sh build Release
./build.sh dist Release
```

Linux remains a source-build workflow.
