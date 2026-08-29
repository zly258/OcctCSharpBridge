# Build and Publish

## Windows

```powershell
.\build.ps1 build Release -OcctRoot "D:\tools\occt-vc144-64"
.\build.ps1 dist Release -OcctRoot "D:\tools\occt-vc144-64"
```

`build` compiles Native and Managed projects. `dist` creates the Release Binary SDK.

Formal Windows packaging uses:

```powershell
.\publish.ps1 -OcctRoot "D:\tools\occt-vc144-64" -Zip
```

## Linux

```bash
./build.sh build Release
./build.sh dist Release
```
