# Viewer, Selection, and Display Updates

## Viewer lifecycle

`OcctEngine` owns one OCCT Viewer/View/InteractiveContext stack. Initialize it once with a valid HWND and dispose it when the host control is destroyed.

Recommended lifecycle:

1. Construct `OcctEngine`.
2. Wait until the Win32 host handle exists.
3. Call `Initialize(hwnd)` once.
4. Call `Resize()` from the host resize event.
5. Perform all Viewer operations on the owning UI thread.
6. Dispose the engine after the host window is no longer used.

## Camera policy

Creating, copying, transforming, importing, or displaying a shape does not automatically call `FitAll`. The current eye, center, up direction, projection, scale, and user navigation are preserved.

Use explicit commands:

| API | Purpose |
|---|---|
| `Fit(shape)` | Fit one displayed shape |
| `FitAll()` | Fit all displayed presentations |
| `WindowFit(x1, y1, x2, y2)` | Fit a screen rectangle |
| `SetView(...)` | Change standard orientation |
| `SetCamera(...)` | Restore or set an exact camera |

This separation prevents every modeling command from unexpectedly resetting the user's view.

## Redraw versus Fit

A scene change and a camera change are different operations:

- Shape creation calls `Display(..., false)` and requests a redraw.
- Color, material, transparency, visibility, and display-mode changes request a redraw.
- `Fit` and `FitAll` modify camera parameters and then redraw.
- Selection overlays use OCCT immediate layers and do not require a full scene fit.

## Display batches

Use `BeginDisplayBatch()` around one logical operation that creates or changes several objects.

```csharp
using (engine.BeginDisplayBatch())
{
    var a = engine.MakeBox(100, 80, 60);
    var b = engine.MakeCylinder(20, 80, 130, 0, 0);
    engine.SetColor(a, Color.SteelBlue);
    engine.SetColor(b, Color.OrangeRed);
}
```

The outermost scope produces one final redraw. The camera remains unchanged.

To fit once at the end, either call `engine.FitAll()` inside the batch or request it from the scope:

```csharp
using (engine.BeginDisplayBatch(fitAllOnDispose: true))
{
    // create and style objects
}
```

Prefer an explicit `FitAll()` inside the command when the intent should be visible in the workflow code.

### Nested batches

Batches can be nested. Inner scopes only decrement the update depth. Pending redraw or fit work executes when the outermost scope ends.

Always use `using`/`Dispose`; leaving a batch open prevents the final update.

## Selection

The Viewer supports object and subshape selection modes:

- Object
- Vertex
- Edge
- Wire
- Face
- Shell
- Solid

Point selection uses screen coordinates. Rectangle selection normalizes the two corners and delegates to the OCCT interactive context.

Appending selection should be controlled by the host UI, typically with `Ctrl`.

## Rubber-band overlay

The rectangle indicator uses `AIS_RubberBand` in an OCCT top-level immediate layer. It does not use Win32 XOR drawing. Updating the rectangle redisplays only the overlay and redraws the immediate layer, which avoids flicker and stale screen artifacts.

The viewport control exposes line color, fill color, transparency, line width, and drag threshold properties.

## Selection event flow

A host should preserve the rectangle-selection state until after mouse-up processing. Releasing mouse capture can synchronously trigger capture-changed events, so the drag result must be stored before capture is released.

The shared `OcctViewportControl` already implements this sequence for both WinForms and WPF hosts.

## Visibility and display properties

Display-related methods keep object IDs stable:

- `SetVisible`
- `SetColor`
- `SetTransparency`
- `SetMaterial`
- `SetDisplayMode`
- `SetLineWidth`
- `Redisplay`
- `Highlight` / `Unhighlight`

Use separate objects rather than merging everything into one Compound when independent selection, properties, deletion, or model-tree entries are required. A Compound is appropriate only when the application intentionally treats the result as one topological object.

## Performance guidance

- Batch multi-object creation and styling.
- Avoid `FitAll()` in loops.
- Avoid repeated full `Redraw()` calls during mouse move; use immediate overlays for temporary feedback.
- Run expensive Boolean, healing, or mesh work in a headless session when practical, then copy the final shape to the Viewer.
- Do not call the same engine concurrently from several threads.
- Keep one engine per viewport instead of rebuilding the Viewer for each document command.

## WPF hosting

The WPF demo uses `WindowsFormsHost` to reuse `OcctViewportControl`. Therefore selection, camera, batching, and rubber-band behavior are shared with WinForms; fixes belong in `OcctNet`, not in two separate UI implementations.

## Depth precision and coplanar objects

The Viewer uses two separate mechanisms:

- `SetAutoZFitMode()` and `AutoZFit()` adjust the camera near/far Z range. This improves depth-buffer precision and avoids clipping, but cannot distinguish two surfaces at exactly the same depth.
- Polygon offsets apply a render-time depth bias to a specific AIS object. Use this for previews, overlays, reference faces, or other objects intentionally displayed coplanar with another object.

```csharp
engine.SetAutoZFitMode(true, 1.0);
engine.AutoZFit();

var reference = engine.MakePlaneFace(100, 80);
var overlay = engine.MakePlaneFace(100, 80);

// Negative values move the overlay toward the viewport.
engine.SetPolygonOffsets(
    overlay,
    OcctPolygonOffsetMode.Fill,
    factor: -1.0,
    units: -1.0);

// Restore the current Viewer default, normally Fill / 1 / 1.
engine.ResetPolygonOffsets(overlay);
```

Do not assign the same custom offset to both coplanar objects; their depth relationship would remain ambiguous. Duplicate production geometry should still be removed or hidden. Polygon offsets are intended for deliberate visual layering, not for repairing invalid topology.

