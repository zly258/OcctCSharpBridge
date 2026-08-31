# Viewer, Selection and Interaction

`OcctEngine` owns AIS/viewer state. The formal SDK exposes the same viewport contract through three UI adapters:

- `OcctNet.WinForms` — Windows x64;
- `OcctNet.Wpf` — Windows x64;
- `OcctNet.Avalonia` — Windows x64 / Linux x64.

Applications should build interaction on the managed viewport contracts instead of handling HWND/X11 input directly.

## Viewport host lifecycle

All three adapters implement `IOcctViewportHost`:

```csharp
IOcctViewportHost host = viewport;

host.HostStateChanged += (_, e) => { /* Detached / Initializing / Ready / Faulted / Disposed */ };
host.Faulted += (_, e) => { /* initialization/runtime host failure */ };
host.EngineRecreated += (_, e) => { /* bind services to e.Engine / e.Generation */ };
host.EngineDisposing += (_, e) => { /* detach from the current engine */ };
host.FirstFrameRendered += (_, e) => { /* first configured frame is visible */ };
host.NativeHandleChanged += (_, e) => { /* advanced host integration only */ };
```

`EngineGeneration` increases whenever the native host creates a new engine. External services that retain engine-bound state should use `EngineRecreated` / `EngineDisposing` instead of assuming a viewport owns one engine forever.

`RenderReady` means that the first configured OCCT frame has been submitted. `HostState == Ready` is reached only after that first frame.

`NativeHandle` represents the actual OCCT render host (`HWND` on Windows, XID on the current Linux backend). It is intentionally an advanced integration/diagnostic escape hatch. Normal CAD interaction should use the managed input and viewport APIs and must not depend on HWND/X11 details.

## First-frame configuration

Set `InitialOptions` before the native host is created:

```csharp
viewport.InitialOptions = new OcctViewportInitializationOptions
{
    BackgroundColor = Color.FromArgb(245, 247, 250),
    ViewOrientation = OcctViewOrientation.Isometric,
    Projection = OcctProjectionType.Orthographic,
    TriedronVisible = true,
    ViewCubeVisible = true
};
```

The adapters initialize the native surface with `redrawAfterInitialize: false`, apply initial options and the initial resize inside one `BeginDisplayBatch()` scope, then coalesce the first real `ResizeSurface() + Redraw()` into the UI dispatcher's Render phase. A native handle being created, the engine being initialized, and the first OCCT frame being submitted are three different states. Applications must not treat `NativeHandle != 0` or `IsEngineInitialized` as first-frame completion; use `RenderReady` / `FirstFrameRendered`.

## Blank first frame or “appears after moving the mouse”

This is one of the most common lifecycle failures when hosting a native OCCT viewport in WPF or Avalonia. Pointer input is not the renderer.

Both WPF `HwndHost` and Avalonia `NativeControlHost` place OCCT in a separate native child window/surface. When the native handle is created, the outer UI framework may still be finishing measure/arrange, DPI synchronization, visibility changes, or the final native bounds update. OCCT, however, needs the viewport size synchronized after host-size changes and a real redraw after the view becomes visible. OCCT's `V3d_View::MustBeResized()` handles window-size changes and `Redraw()` performs an explicit redraw; creating a handle or invalidating content alone does not submit the first frame.

A typical failing sequence is:

1. create the native handle;
2. initialize the OCCT surface;
3. resize/redraw before the final arranged size is known, or omit the redraw after final layout;
4. WPF/Avalonia finishes layout but the native OCCT surface receives no effective refresh;
5. the viewport remains blank until a later input, resize, DPI, or visibility event happens to request another redraw.

“Moving the mouse makes it appear” is a strong diagnostic signature. The default hover path calls `OcctEngine.MoveTo(...)`, and the Bridge native selection path then calls `requestRedraw()`. Pointer movement therefore happens to submit the missing frame; it is not a valid initialization strategy.

Follow these rules:

- do not treat native-handle creation or engine initialization inside a Window/UserControl constructor as a visible viewport;
- use `InitialOptions` for static first-frame configuration and `EngineRecreated` for engine-bound services;
- use `RenderReady` / `FirstFrameRendered` as the first-visible-frame contract;
- custom hosts must apply the final layout size through `ResizeSurface()` and then perform a real `Redraw()`;
- size, DPI, visibility, tab/docking, minimize/restore, and native-host reattachment changes should use the coalesced `RefreshNativeView()` path rather than pointer events, timers, or redraw loops;
- `Invalidate` only marks content dirty and does not replace the first real `Redraw`;
- 125%/150% DPI scaling is usually not the root cause, but it increases layout, DPI, and native-bounds transitions and therefore exposes timing defects more often.

For diagnosis, if the viewport is blank on startup but becomes correct after mouse movement, temporarily call `RefreshNativeView()` after final layout. If that immediately fixes the frame, the root cause is almost certainly native-host layout / first-frame Resize+Redraw timing. The permanent fix belongs in the host lifecycle, not in the temporary call.

After the viewport is ready, use normal `OcctEngine` APIs to change background, view, projection, triedron, or view cube state.

## Platform-neutral pointer and keyboard input

All three adapters implement `IOcctViewportInputSource` and expose the same input events:

```csharp
viewport.PreviewPointerInput += (_, e) => { /* optionally e.Handled = true */ };
viewport.PointerInput += (_, e) => { };
viewport.PreviewKeyInput += (_, e) => { /* optionally e.Handled = true */ };
viewport.KeyInput += (_, e) => { };
```

The public event arguments use Bridge types (`OcctPointerButton`, `OcctPointerButtons`, `OcctKey`, `OcctInputModifiers`) rather than WinForms, WPF, Avalonia, Win32, or X11 key/button types.

`PreviewPointerInput` / `PreviewKeyInput` run before default viewport interaction. Setting `Handled` suppresses the corresponding Bridge default behavior, which is the intended entry point for commands such as drawing, grips, snapping, orthogonal mode, dynamic input, and custom navigation.

## Interaction features

`OcctViewportInteractionFeatures` replaces the previous all-or-nothing default-interaction switch. Features can be combined independently:

```csharp
viewport.InteractionFeatures =
    OcctViewportInteractionFeatures.HoverDetection |
    OcctViewportInteractionFeatures.PointSelection |
    OcctViewportInteractionFeatures.RectangleSelection |
    OcctViewportInteractionFeatures.Pan |
    OcctViewportInteractionFeatures.Zoom;
```

The grouped values `Selection`, `Navigation`, and `Default` provide convenient presets.

## Hover detection

`HoverHitChanged` reports a change only when the detected owner/subshape identity changes. Moving inside the same edge or face does not produce an event storm merely because the detected 3D point/depth changes.

```csharp
viewport.HoverHitChanged += (_, e) =>
{
    OcctSelectionHitDetail? hit = e.Hit;
};
```

The event reuses the existing OCCT detection pipeline (`MoveTo` + detected hit detail); it does not create a second native picking system.

## Batched viewer updates

Use the existing `BeginDisplayBatch()` API when many scene changes should produce one final redraw:

```csharp
using (engine.BeginDisplayBatch())
{
    var box = engine.MakeBox(100, 80, 60);
    engine.SetObjectColor(box, Color.SteelBlue);
    engine.SetObjectTransparency(box, 0.15);
}
```

Do not add a parallel `BeginUpdate`, `EndUpdate`, or `DeferRefresh` abstraction for the same purpose.

## Edge and face point projection

Viewer geometry queries project a point onto real trimmed BRep topology and return the local differential direction needed by snapping/work-plane consumers in the same call:

```csharp
var edgeProjection = engine.ProjectPointToEdge(edge, sourcePoint);
OcctPoint3d nearestOnEdge = edgeProjection.Point;
OcctVector3d tangent = edgeProjection.Tangent;
var edgePoint = engine.EvaluateEdge(edge, edgeProjection.NormalizedParameter);

var faceProjection = engine.ProjectPointToFace(face, sourcePoint);
OcctPoint3d nearestOnFace = faceProjection.Point;
OcctVector3d normal = faceProjection.Normal;
var facePoint = engine.EvaluateFace(face, faceProjection.U, faceProjection.V);
```

`OcctEdgeProjectionResult` returns the nearest point, normalized edge parameter (`0..1`), normalized tangent and distance. `OcctFaceProjectionResult` returns the nearest point, face `U/V`, orientation-corrected normalized normal and distance. The returned parameter/UV can be evaluated back to the same point and direction through `EvaluateEdge` / `EvaluateFace`.

These queries form the geometry foundation for nearest/perpendicular/tangent snapping, face-normal interaction and face-based work planes without requiring a second geometry query solely to recover tangent/normal data.

## Adapter notes

WPF owns a native child HWND and separates surface resize from redraw so repeated layout notifications can be coalesced.

Avalonia is part of the formal SDK. Windows uses an HWND host; Linux currently uses an X11/XWayland XID host. Both backends normalize pointer and keyboard input into the same managed Bridge contracts. Native Wayland hosting can be added later without changing application-level input APIs.

## Engine thread affinity

`OcctEngine` binds itself to the current thread after the native surface is initialized successfully and captures the current `SynchronizationContext`. Subsequent synchronous Viewer, AIS, Scene, Selection, and Exchange calls must run on that thread. A cross-thread synchronous call throws a descriptive `InvalidOperationException` instead of entering the non-thread-safe OCCT viewer.

Asynchronous import and export methods on `OcctEngine` are posted to the surface thread rather than invoking the viewer from `Task.Run`. This preserves thread safety, but file parsing can still occupy the UI thread. Use a separate `OcctModelingSession` for true background exchange or modeling, then update the viewer shape on the UI thread.

Call `OcctEngine.Dispose()` on the surface thread. The WinForms, WPF, and Avalonia adapters already dispose their engines as part of the native host lifecycle.
