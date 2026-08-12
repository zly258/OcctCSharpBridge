# Third-party notices

OcctCSharpBridge depends on third-party software. Those components are not relicensed by the OcctCSharpBridge license.

## Open CASCADE Technology (OCCT)

OcctCSharpBridge 2.7 targets Open CASCADE Technology 7.9.0. OCCT is distributed under GNU LGPL version 2.1 with the Open CASCADE exception. The OCCT exception text is included at `third-party/OCCT/OCCT_LGPL_EXCEPTION.txt`; the GNU LGPL 2.1 text is included at `LICENSE_LGPL_21.txt`.

The `OcctNet.Runtime.win-x64` package may redistribute the OCCT runtime modules required by `OcctNative.dll`. Applications redistributing that package remain responsible for complying with the OCCT license and notices.

## Runtime closure dependencies

The Windows runtime package is built from the configured OCCT 7.9.0 SDK and follows the actual PE import dependency closure of `OcctNative.dll`. Depending on the OCCT distribution, that closure can include redistributable third-party runtime DLLs such as threading, font, compression, image, or Microsoft Visual C++ runtime components.

During NuGet runtime staging, `tools/prepare-nuget-runtime.ps1` copies discoverable `LICENSE*`, `COPYING*`, `NOTICE*`, and `COPYRIGHT*` files from the configured OCCT SDK into the runtime package under `licenses/occt-sdk/`. These files remain under their respective upstream terms.

No third-party trademark rights or additional patent rights are granted by OcctCSharpBridge.
