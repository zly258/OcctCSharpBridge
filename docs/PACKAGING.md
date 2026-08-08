# Packaging and Runtime Deployment

OcctCSharpBridge intentionally separates **managed SDK packages** from the **native OCCT runtime**. This prevents a local development package from pretending to be a fully self-contained OCCT distribution.

## Managed packages

Create the managed packages from the `main` branch with:

```powershell
.\build.ps1 pack Release
```

Output:

```text
artifacts/packages/
├─ OcctNet.<version>.nupkg
├─ OcctNet.<version>.snupkg
├─ OcctNet.WinForms.<version>.nupkg
├─ OcctNet.WinForms.<version>.snupkg
├─ OcctNet.Wpf.<version>.nupkg
└─ OcctNet.Wpf.<version>.snupkg
```

The version is injected from `bridge-contract.json`.

The packages contain managed assemblies, XML documentation, package dependency relationships, README/license metadata, and symbol packages. They do **not** contain `OcctNative.dll`, OCCT `TK*.dll`, OCCT third-party runtime DLLs, or OCCT resource directories.

Native deployment remains an explicit application responsibility because it depends on the exact OCCT build, compiler runtime, optional third-party dependencies, and license obligations.

For release testing on a machine with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "D:\tools\occt-vc144-64"
```

The `demo` branch owns complete desktop publishing:

```powershell
.\publish.ps1 all Release -Zip -OcctRoot "D:\tools\occt-vc144-64"
```

Do not publish the managed packages to a public package feed until the native runtime distribution and release process are intentionally defined.
