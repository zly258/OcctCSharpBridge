# Demo Platform Matrix

| Platform | Common | WinForms | WPF | Avalonia |
|---|---:|---:|---:|---:|
| Windows x64 | yes | yes | yes | yes |
| Linux x64 | yes | no | no | yes |

The Demo is a Binary/Portable SDK consumer. `main` / `main-dev` own Bridge implementation and SDK production; Demo never vendors Bridge Native/Core source.

| Capability | Windows x64 | Linux x64 |
|---|---|---|
| Minimal Binary SDK | `dist/win-x64` | `dist/linux-x64` |
| Portable SDK cache | `dist/portable/win-x64` | `dist/portable/linux-x64` |
| Demo hosts | WinForms / WPF / Avalonia | Avalonia |
| Native host | HWND | X11/XWayland XID |
| Platform-neutral pointer/key input | yes | yes |
| Host lifecycle / first-frame contract | yes | yes |
| Viewer projection sample | yes | yes |
| Source sync on cache miss | Bridge `dist Release` only | Bridge `dist Release` only |
| Bridge tests during sync | no | no |
| Default publish package count | 1 unified package | 1 |
| Default .NET publish mode | one shared private .NET 10 Desktop Runtime | self-contained Avalonia |
| Bridge/OCCT native closure | one shared `runtime/` | one `runtime/` |
| Package integrity | `package-manifest.json` SHA-256 | `package-manifest.json` SHA-256 |

## Synchronization

Windows:

```powershell
.\sync.ps1
```

Linux:

```bash
./sync.sh
```

When the local SDK cache matches the requested Bridge `sourceCommit` and all hashes validate, synchronization skips Bridge compilation entirely.

On a cache miss, both platforms generate the consumer Binary SDK with Bridge `dist Release`, then call the Bridge-owned Portable SDK packager. Synchronization does not run the Bridge `sdk`/`all` QA gate, ManagedTests, Core Smoke, or graphical viewport smokes.

Matching prebuilt Binary + Portable SDKs may be supplied directly to avoid compiling Bridge at all.

## Windows publication

Default:

```powershell
.\publish.ps1 all Release -Zip
```

Layout:

```text
CAD-Demo-win-x64/
├─ apps/winform/
├─ apps/wpf/
├─ apps/avalonia/
├─ dotnet/                  # one private .NET 10 Desktop Runtime shared by all apps
├─ runtime/                 # one Bridge + OCCT closure
├─ occt/resources/
├─ run-winform.cmd
├─ run-wpf.cmd
├─ run-avalonia.cmd
└─ package-manifest.json
```

The default unified package does not require a system .NET 10 installation.

Explicit alternatives:

```powershell
# Three separate self-contained application runtime closures
.\publish.ps1 all Release -SelfContained -Zip

# No private runtime; machine-installed .NET is required
.\publish.ps1 all Release -FrameworkDependent -Zip
```

Single Windows targets (`winform`, `wpf`, `avalonia`) remain independently publishable.

## Linux publication

```bash
./publish.sh Release
```

Linux publishes Avalonia and merges the matching Bridge Portable Runtime/resources. Native portability is constrained by the glibc/libstdc++ ABI baseline used to build OCCT and `libOcctNative.so`; packaging alone does not make a newer native build compatible with older distributions.

## Branch use

- `demo-dev` defaults to `main-dev` while validating unreleased Bridge changes.
- formal `demo` should consume formal `main` artifacts.
- WinForms/WPF remain Windows-only.
- Avalonia is the Linux UI host.
- there are no standalone Avalonia source branches.
