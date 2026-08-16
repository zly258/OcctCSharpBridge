# Demo SDK Consumer Policy

The Demo branch consumes Binary SDKs generated from `main`; it is never an SDK implementation branch.

Windows consumes `dist/win-x64` and supports WinForms, WPF and Avalonia.
Linux consumes `dist/linux-x64` and supports Avalonia only.

Rules:

- no `OcctNative` or `OcctNet*` implementation source;
- no direct `occt_*` ABI calls;
- no ABI4 compatibility metadata or handles;
- no committed Binary SDK under `dist/`;
- synchronize by source commit and validate manifest hashes;
- use only current Bridge 3 public managed APIs.
