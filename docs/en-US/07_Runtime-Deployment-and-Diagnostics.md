# Runtime Deployment and Diagnostics

The `avalonia` branch resolves the Native bridge by operating system:

```text
Windows: OcctNative.dll
Linux:   libOcctNative.so
```

Windows developer default:

```text
D:\tools\occt-vc144-64
```

Linux developer defaults:

```text
/usr/local/include/opencascade
/usr/local/lib
```

`OCCT_ROOT`, `OCCT_BRIDGE_NATIVE_DIR` and platform loader configuration can be used for non-default development or application deployment layouts.

The `avalonia` branch is source-only and does not track Binary SDK output. Applications that deploy these assemblies are responsible for deploying the matching platform Native bridge and OCCT runtime dependencies.

Linux Viewer execution currently requires X11/XWayland for the Viewer backend. Headless modeling does not require a Viewer surface.
