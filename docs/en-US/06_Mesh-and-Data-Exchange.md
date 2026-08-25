# Mesh and Data Exchange

The Core provides configurable triangulation plus STEP, IGES, BREP and STL-oriented exchange capabilities supported by the contracted OCCT 7.9 toolkits.

STEP assembly import uses XDE internally to preserve product hierarchy, occurrences, transforms, visibility, colors and subshape styles. Managed consumers receive `OcctAssemblyDocument` / `OcctAssemblyNode` snapshots rather than an exposed OCAF document model.

Valid multi-solid STEP parts remain one logical Part where the source product structure says they are one Part.

Non-geometric STEP metadata can round-trip through the retained imported XDE representation while geometry remains unchanged.

These Core exchange semantics are host-independent. WinForms, WPF and Avalonia all consume the same `OcctNet` Core contract; Linux Avalonia uses the same exchange model as Windows rather than a separate branch-specific contract.

## Background import into the viewer

`OcctEngine.ImportAsync`, `ImportStepAsync`, `ImportIgesAsync`, `ImportBrepAsync`, and `ImportStlAsync` parse files on the thread pool through an isolated headless `OcctModelingSession`. After parsing, only the TopoDS shape is copied to the surface thread to create the AIS viewer shape, so background threads never access the viewer.

Applications can also call `CreateShapeFromModel(session, shape)` to add an existing modeling shape to the viewer. Call it on the engine surface thread. The native bridge obtains a shape copy while holding the modeling-session lock before creating the viewer presentation.

Cancellation is honored before parsing starts and before the viewer commit. An OCCT parser already running is not forcibly interrupted, but a cancelled result is not added to the viewer.
