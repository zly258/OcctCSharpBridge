# Viewer 选择与交互

Avalonia 分支唯一公开 Host 是 `OcctAvaloniaViewport`。

```text
Windows: Avalonia NativeControlHost → HWND → WNT_Window → OCCT
Linux:   Avalonia NativeControlHost → XID  → Xw_Window  → OCCT
```

Windows/Linux 对外统一 Engine、Selection、Rectangle Selection、Rotate、Pan、Zoom、Hover 与 World Point 语义。

Windows 保留 Native Child Window/WndProc；Linux 当前内部使用 Avalonia Pointer Input + X11/XWayland Child Surface。

当前 Linux Viewer 目标为 X11/XWayland。Native Wayland 暂不宣称完成，但后续新增 Backend 不应改变上层 Viewport API。