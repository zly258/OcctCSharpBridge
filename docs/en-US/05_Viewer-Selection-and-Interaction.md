# Viewer, Selection and Interaction

On `main`, `OcctEngine` owns AIS/viewer state. The formal SDK provides three UI adapters:

- `OcctNet.WinForms` — Windows x64;
- `OcctNet.Wpf` — Windows x64;
- `OcctNet.Avalonia` — Windows x64 / Linux x64.

The Viewer API covers camera/projection, display mode, color/material/transparency, transforms, text/dimensions/points, lighting, redraw batching, selection, detection, rectangle selection, screen/world conversion and raw interaction helpers.

WPF uses a native HWND host and separates surface resize from redraw so repeated resize notifications can be coalesced.

Avalonia is part of the formal `main` SDK. Windows uses the Windows native Viewer host; Linux uses the X11/XWayland backend. The unified `demo` branch provides an Avalonia host on both platforms while WinForms and WPF remain Windows-only.
