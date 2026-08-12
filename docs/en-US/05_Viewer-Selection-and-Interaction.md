# 05 Viewer, Selection and Interaction

`OcctEngine` owns the AIS interactive context, view/camera state and displayed objects.

## Selection

The Bridge supports point selection, rectangle selection, selection operations, selection modes, selected/detected structured identities, selectability and application-driven selection sets. Applications can disable default interaction and consume raw input for their own CAD tools, snapping and dynamic previews.

## Presentation

Viewer APIs cover display mode, materials, color, transparency, line width, local transforms, dimensions, text, view cube, lighting, background, camera/projection and display batching.

## First-class points

`OcctPoint` is backed by a real OCCT `AIS_Point` / `Geom_CartesianPoint`. `OcctPointMarker` maps to standard OCCT marker types. Position/style updates redisplay without forcing an immediate full redraw, so display batches can coalesce multiple edits.

## WPF resizing

The WPF host uses a dedicated no-redraw Native surface-resize API. Resize events are coalesced and presented at `DispatcherPriority.Render`; `WM_PAINT` no longer triggers an OCCT redraw. This avoids redundant redraw storms and reduces resize flicker.

WinForms, WPF and Avalonia hosts remain independent adapters over the same `OcctEngine` semantics.
