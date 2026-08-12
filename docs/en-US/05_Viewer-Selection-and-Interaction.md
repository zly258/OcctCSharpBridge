# Viewer, Selection and Interaction

On `main`, `OcctEngine` owns AIS/viewer state and is hosted through two Windows adapters:

- `OcctNet.WinForms`
- `OcctNet.Wpf`

The Viewer API covers camera/projection, display mode, color/material/transparency, transforms, text/dimensions/points, lighting, redraw batching, selection, detection, rectangle selection, screen/world conversion and raw interaction helpers.

WPF uses a native HWND host and separates surface resize from redraw so repeated resize notifications can be coalesced.

Avalonia is not a `main` UI adapter. Use the `avalonia` branch for the single cross-platform `OcctAvaloniaViewport` API on Windows/Linux.