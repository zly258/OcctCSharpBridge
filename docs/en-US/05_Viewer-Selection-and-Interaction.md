# Viewer, Selection and Interaction

The only public Avalonia host is `OcctAvaloniaViewport`.

```text
Windows: Avalonia NativeControlHost → HWND → WNT_Window → OCCT
Linux:   Avalonia NativeControlHost → XID  → Xw_Window  → OCCT
```

The control exposes the same Engine, selection, rectangle selection, rotation, pan, zoom, hover and world-point semantics on both platforms.

Windows keeps its native child-window/WndProc path. Linux uses Avalonia pointer input with an X11/XWayland child surface internally.

The current Linux Viewer target is X11/XWayland. Native Wayland is intentionally not advertised as complete; callers do not need to change API when another backend is added.