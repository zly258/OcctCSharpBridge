# Changelog

All notable release-line changes are recorded here. The machine-readable compatibility contract remains `bridge-contract.json`.

## 3.0.0

### Stable contract

- Promoted Bridge 3 from preview to the `3.0.0` stable version line.
- Native ABI 5 remains the only supported ABI.
- OCCT remains fixed to 7.9.0.
- Managed Binary SDK remains based on `net8.0` / `net8.0-windows` and supports .NET 8, .NET 9, and .NET 10 consumers.
- Official prebuilt release assets are defined for Windows x64 only.
- Linux x64 remains a maintained source-build platform with Avalonia support; no official Linux prebuilt binary compatibility guarantee is made.

### Release validation

- Added a Windows Stable release gate that builds and publishes the existing full SDK candidate and then performs native-backed execution on actual .NET 8, 9, and 10 runtimes.
- Added an isolated Portable SDK smoke that extracts the generated Windows ZIP outside the repository, removes development OCCT/native path configuration, and validates app-local runtime/resource probing.
- Kept the fast `dist` consumer path separate from the full Bridge QA/release gate so Demo and third-party SDK refreshes do not rerun expensive regression and viewport tests.

### Native quality

- Removed aggregate-initialization warning sources for scene/modeling records while preserving their existing default field semantics.
- Windows Native Release continues to compile under `/W4 /WX`; managed projects continue to treat warnings as errors.

### Documentation

- Clarified official Windows prebuilt support versus Linux source-build support.
- Added stable compatibility rules covering threading, ownership/lifetime, units, coordinates, tolerance, Native ABI, Managed API, and SDK upgrade boundaries.
- Updated third-party and release guidance around the Windows Stable package and real .NET runtime validation.

### Migration

Bridge 3 is an ABI5-only line. Applications still using pre-ABI5 exports, compatibility shims, legacy handles, or Bridge 2.x Binary SDK payloads must migrate to the current `OcctNet`/ABI5 contract before adopting 3.0.0.

## 3.0.0-preview.1

- First public preview of the Bridge 3 ABI5-only architecture.
- Introduced the unified Native Core, .NET 8 baseline assemblies, WinForms/WPF/Avalonia adapters, Binary SDK manifests, Portable SDK packaging, runtime diagnostics, and cross-platform source build line.
