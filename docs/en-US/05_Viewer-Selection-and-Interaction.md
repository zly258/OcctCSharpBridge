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

The adapters create the native surface without an immediate redraw, apply the initial options inside one `BeginDisplayBatch()` scope, resize the surface, and submit the first frame once. Native window mapping is deferred until that first real redraw, so the first visible HWND/XID frame already contains the configured background, view, projection and decorations instead of exposing an empty/default native window first.

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
